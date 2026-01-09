using iText.Layout.Borders;
using LiveCharts;
using LiveCharts.Maps;
using LiveCharts.Wpf;
using Microsoft.Win32;
using Microsoft.Win32;
using Newtonsoft.Json;
using Newtonsoft.Json;
using PDC_System.Helpers;
using PDC_System.HomeUi;
using PDC_System.Services;
using PDC_System.Models;
using System;
using System.Globalization;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Application = System.Windows.Application;
using Color = System.Windows.Media.Color;
using ColorConverter = System.Windows.Media.ColorConverter;
using System.Collections.Generic;

namespace PDC_System
{
    /// <summary>
    /// Interaction logic for HomeUIWindow.xaml
    /// </summary>
    public partial class HomeUIWindow : System.Windows.Controls.UserControl
    {
        private DispatcherTimer timer;
        public List<BirthdayInfo> UpcomingBirthdays { get; set; } = new List<BirthdayInfo>();
        private List<Employee> employees = new List<Employee>();
        public SeriesCollection SalesValues { get; set; }
        public SeriesCollection AttendanceSeries { get; set; }
        public string[] Labels { get; set; }

        // Set the directory path
        private string saversDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Savers");

        public HomeUIWindow()
        {
            InitializeComponent();
            LoadData();
            ThemeManager.ApplyTheme(this); // Apply initial theme
            LoadBarChartData();
            LoadAttendanceOverview();
        }

        // JsonData class
        public class JobCard
        {
            public DateTime JobCardDate { get; set; }
            public string Customer_Name { get; set; }
            public int Quantity { get; set; }
        }

        // Bar chart data class
        public class BarData
        {
            public string Label { get; set; }
            public int Value { get; set; }
        }

        private void LoadBarChartData()
        {
            string jsonFilePath = Path.Combine(saversDirectory, "jobcards.json");

            if (!File.Exists(jsonFilePath))
                return;

            string json = File.ReadAllText(jsonFilePath);
            var jsonData = JsonConvert.DeserializeObject<List<JobCard>>(json);

            if (jsonData == null || !jsonData.Any())
                return;

            // Last 5 months based on JSON data
            var monthsFromData = jsonData
                .Select(j => j.JobCardDate)
                .Select(d => d.Month + "/" + d.Year) // Month/Year string
                .Distinct()
                .OrderBy(m => m)
                .ToList();

            // Prepare bar chart data
            var filteredData = monthsFromData
                .Select(m =>
                {
                    var parts = m.Split('/');
                    int month = int.Parse(parts[0]);
                    int year = int.Parse(parts[1]);

                    int count = jsonData.Count(j => j.JobCardDate.Month == month && j.JobCardDate.Year == year);

                    return new BarData
                    {
                        Label = CultureInfo.CurrentCulture.DateTimeFormat.GetAbbreviatedMonthName(month),
                        Value = count * 20 // scale for rectangle height
                    };
                })
                .ToList();

            BarChart.ItemsSource = filteredData;

            // Example: show job count for the **most recent month** from JSON
            var latestMonth = monthsFromData.Last();
            var latestParts = latestMonth.Split('/');
            int latestMonthNum = int.Parse(latestParts[0]);
            int latestYear = int.Parse(latestParts[1]);
            int latestCount = jsonData.Count(j => j.JobCardDate.Month == latestMonthNum && j.JobCardDate.Year == latestYear);
            Countofjob.Text = latestCount.ToString();
        }

        private void LoadAttendanceOverview()
        {
            try
            {
                var attendanceManager = new AttendanceManager();
                DateTime endDate = DateTime.Today;
                DateTime startDate = endDate.AddDays(-6); // Last 7 days

                var attendanceRecords = attendanceManager.LoadAttendanceWithDateRange(startDate, endDate);

                if (attendanceRecords == null || !attendanceRecords.Any())
                {
                    // Set empty chart
                    AttendanceSeries = new SeriesCollection();
                    Labels = new string[0];
                    DataContext = this;
                    return;
                }

                // Calculate daily attendance statistics
                var dailyStats = new List<DailyAttendanceStats>();

                for (DateTime date = startDate; date <= endDate; date = date.AddDays(1))
                {
                    var dayRecords = attendanceRecords.Where(r => r.Date.Date == date.Date).ToList();

                    int presentCount = dayRecords.Count(r =>
                        !string.IsNullOrEmpty(r.Status) &&
                        r.Status != "Absent" &&
                        r.Status != "Missing Finger Print");

                    int absentCount = dayRecords.Count(r =>
                        r.Status == "Absent" ||
                        r.Status == "Missing Finger Print");

                    // Calculate attendance percentage
                    int totalEmployees = dayRecords.Count;
                    double attendancePercentage = totalEmployees > 0 ? (double)presentCount / totalEmployees * 100 : 0;

                    dailyStats.Add(new DailyAttendanceStats
                    {
                        Date = date,
                        PresentCount = presentCount,
                        AbsentCount = absentCount,
                        TotalEmployees = totalEmployees,
                        AttendancePercentage = attendancePercentage
                    });
                }

                // Create line chart series
                AttendanceSeries = new SeriesCollection
                {
                    new LineSeries
                    {
                        Title = "Present Employees",
                        Values = new ChartValues<int>(dailyStats.Select(d => d.PresentCount)),
                        Stroke = Brushes.DodgerBlue,
                        Fill = new SolidColorBrush(Color.FromArgb(40, 30, 144, 255)),
                        StrokeThickness = 3,
                        PointGeometry = DefaultGeometries.Circle,
                        PointGeometrySize = 8,
                        PointForeground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#4CAF50")),
                        DataLabels = true,
                        LabelPoint = point => $"{point.Y}"
                    },
                    new LineSeries
                    {
                        Title = "Absent Employees",
                        Values = new ChartValues<int>(dailyStats.Select(d => d.AbsentCount)),
                        Stroke = Brushes.Red,
                        Fill = new SolidColorBrush(Color.FromArgb(40, 255, 0, 0)),

                        StrokeThickness = 3,
                        PointGeometry = DefaultGeometries.Circle,
                        PointGeometrySize = 8,
                        PointForeground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F44336")),
                        DataLabels = true,
                        LabelPoint = point => $"{point.Y}"
                    }
                };

                // Create labels (day names with dates)
                Labels = dailyStats.Select(d => $"{d.Date:ddd}\n{d.Date:dd}").ToArray();

                // Update UI
                DataContext = this;
            }
            catch (Exception ex)
            {
                // Handle error gracefully
                AttendanceSeries = new SeriesCollection();
                Labels = new string[0];
                DataContext = this;

                // Optionally log the error
                System.Diagnostics.Debug.WriteLine($"Error loading attendance overview: {ex.Message}");
            }
        }

        private void LoadData()
        {
            string filePath = Path.Combine(saversDirectory, "employee.json");

            if (File.Exists(filePath))
            {
                employees = JsonConvert.DeserializeObject<List<Employee>>(File.ReadAllText(filePath));
                RefreshUpcomingBirthdays();
            }
        }

        private void RefreshUpcomingBirthdays()
        {
            UpcomingBirthdays = GetUpcomingBirthdays(30); // Get birthdays within next 30 days
            DataContext = this; // Update UI
        }

        private List<BirthdayInfo> GetUpcomingBirthdays(int daysAhead)
        {
            DateTime today = DateTime.Today;
            DateTime futureDate = today.AddDays(daysAhead);

            return employees
                .Where(e => e.Birthday.HasValue) // Ensure employee has a birthday
                .Select(e =>
                {
                    DateTime birthdayThisYear = new DateTime(today.Year, e.Birthday.Value.Month, e.Birthday.Value.Day);
                    if (birthdayThisYear < today)
                        birthdayThisYear = birthdayThisYear.AddYears(1);

                    int daysLeft = (birthdayThisYear - today).Days;

                    return new BirthdayInfo
                    {
                        Name = e.Name,
                        BirthDate = e.Birthday,
                        DaysLeft = daysLeft
                    };
                })
                .Where(b => b.DaysLeft <= daysAhead)
                .OrderBy(b => b.DaysLeft)
                .ToList();
        }

        // Helper class for daily attendance statistics
        public class DailyAttendanceStats
        {
            public DateTime Date { get; set; }
            public int PresentCount { get; set; }
            public int AbsentCount { get; set; }
            public int TotalEmployees { get; set; }
            public double AttendancePercentage { get; set; }
        }
    }
}
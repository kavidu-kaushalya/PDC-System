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
        public string[] Labels { get; set; }

        // Set the directory path
        private string saversDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Savers");



        public HomeUIWindow()
        {
            InitializeComponent();
            //StartClock();//
            //EnsureSaversDirectoryExists();//
            LoadData();
            ThemeManager.ApplyTheme(this); // Apply initial theme
            LoadBarChartData();
           



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




        // Hook system theme changes

        // SystemEvents.UserPreferenceChanged += SystemEvents_UserPreferenceChanged;//





        /* private void StartClock()
         {
             timer = new DispatcherTimer();
             timer.Interval = TimeSpan.FromSeconds(1);
             timer.Tick += Timer_Tick;
             timer.Start();
         }
         private void SystemEvents_UserPreferenceChanged(object sender, UserPreferenceChangedEventArgs e)
         {
             if (e.Category == UserPreferenceCategory.General)
             {
                 // Use Dispatcher to update UI thread
                 Dispatcher.Invoke(SetTitleBarColor);
             }
         }


         private void Timer_Tick(object sender, EventArgs e)
         {
             TimeText.Text = DateTime.Now.ToString("hh:mm tt"); // 12-hour format
         }

         private void EnsureSaversDirectoryExists()
         {
             if (!Directory.Exists(saversDirectory))
                 Directory.CreateDirectory(saversDirectory);
         } */

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



        


    }
}

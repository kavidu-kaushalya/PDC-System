using iText.Layout.Borders;
using Newtonsoft.Json;
using PDC_System.HomeUi;
using System;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

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
        



        public HomeUIWindow()
        {
            StartClock();
            InitializeComponent(); // This was missing
            LoadData();
            







        }

        private void StartClock()
        {
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += Timer_Tick;
            timer.Start();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            TimeText.Text = DateTime.Now.ToString("hh:mm tt"); // 12-hour format
        }


        private void LoadData()
        {
            if (File.Exists("employee.json"))
            {
                employees = JsonConvert.DeserializeObject<List<Employee>>(File.ReadAllText("employee.json"));
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

        private void WhatsAppWindow_Click(object sender, RoutedEventArgs e)
        {
            // Create a new instance of the window
            WhatsAppDetails newWindow = new WhatsAppDetails();

            // Show the window
            newWindow.Show();
        }


        private void EmailWindow_Click(object sender, RoutedEventArgs e)
        {
            // Create a new instance of the window
            EmailDetails newWindow = new EmailDetails();

            // Show the window
            newWindow.Show();
        }




    }
}

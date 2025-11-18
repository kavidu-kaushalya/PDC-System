using PDC_System.Helpers;
using PDC_System.Properties; // important
using PDC_System.Settings;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Threading; // add this for DispatcherTimer

namespace PDC_System
{
    public partial class App : Application
    {

        private DispatcherTimer appTimer; // Timer variable

        protected override async void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // 🔧 FIX: enable legacy encodings (windows-1252, cp1252, etc.)
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            ThemeManager.UpdateAllWindows();
            CleanupOldHistory();
            

            // 🕒 Get interval from Settings.settings
            int intervalSeconds = PDC_System.Properties.Settings.Default.TimerIntervalMinutes;

            // ✅ Initialize and start the timer
            appTimer = new DispatcherTimer();
            appTimer.Interval = TimeSpan.FromMinutes(intervalSeconds);
            appTimer.Tick += AppTimer_Tick;
            appTimer.Tick += AppTimer_Tick2;
            appTimer.Start();


            Console.WriteLine($"✅ Timer started with {intervalSeconds} second interval");
        }

        private void AppTimer_Tick(object? sender, EventArgs e)
        {
            try
            {
                var settingsWindow = Application.Current.Windows
                    .OfType<SettingsWindow>()
                    .FirstOrDefault();

                if (settingsWindow != null)
                {
                    settingsWindow.BtnLoad_Click();
                    NotificationHelper.ShowNotification("PDC System!", "IVMS Calclulate Complte!");
                    
                }
                else
                {
                    var backgroundSettings = new SettingsWindow();
                    backgroundSettings.Visibility = Visibility.Hidden;

                    backgroundSettings.BtnLoad_Click();
                    backgroundSettings.Close();
                    NotificationHelper.ShowNotification("PDC System!", "IVMS Calclulate Complte!");
                    
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("❌ Timer error: " + ex.Message,
                                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private void AppTimer_Tick2(object? sender, EventArgs e)
        {
            try
            {
                // 🔄 Create AttendanceManager
                var manager = new PDC_System.Services.AttendanceManager();

                // 🧾 Load all attendance (auto-refresh)
                var records = manager.LoadAttendance();

                // 💾 Save refreshed data
                manager.SaveAllAttendanceRecords(records);
                NotificationHelper.ShowNotification("PDC System!", "Attendance Calclulate Complte!");
                // 💬 Optional on-screen message
               
            }
            catch (Exception ex)
            {
                MessageBox.Show("❌ Auto-refresh failed:\n" + ex.Message,
                                "Error",
                                MessageBoxButton.OK,
                                MessageBoxImage.Error);
            }
        }


       





        private void CleanupOldHistory()
        {
            string historyFilePath = Path.Combine(Directory.GetCurrentDirectory(), "backup_history.txt");

            if (!File.Exists(historyFilePath))
                return;

            try
            {
                var lines = File.ReadAllLines(historyFilePath);
                var filteredLines = new List<string>();
                DateTime sevenDaysAgo = DateTime.Now.AddDays(-7);

                foreach (var line in lines)
                {
                    if (line.Length < 21)
                        continue;

                    string datePart = line.Substring(1, 19); // [yyyy-MM-dd HH:mm:ss]
                    if (DateTime.TryParse(datePart, out DateTime logDate))
                    {
                        if (logDate >= sevenDaysAgo)
                            filteredLines.Add(line);
                    }
                }

                File.WriteAllLines(historyFilePath, filteredLines);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error cleaning up history: " + ex.Message);
            }
        }
    }
}

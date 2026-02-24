using Microsoft.Win32;
using System;
using PDC_System.Services;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Diagnostics;
using System.Linq;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.ServiceProcess;

using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using XamlAnimatedGif;
using PDC_System.Helpers;

namespace PDC_System
{
    public partial class Loading : Window
    {
        private GoogleServiceManager googleManager;

        public Loading()
        {
            InitializeComponent();
            SeedAdmin();
            Loaded += LoadingWindow_Loaded; // window render වෙන විට
            ThemeManager.ApplyTheme(this); // Apply initial theme

        }



        private async void LoadingWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // GIF load background thread
            await Task.Run(() =>
            {
                string baseFolder = AppDomain.CurrentDomain.BaseDirectory;
                string gifPath = System.IO.Path.Combine(baseFolder, "Assets", "Wonder_Things.gif");
                var gifUri = new Uri(gifPath, UriKind.Absolute);

                Dispatcher.Invoke(() =>
                {
                    AnimationBehavior.SetSourceUri(MyGifImage, gifUri);
                    AnimationBehavior.SetRepeatBehavior(MyGifImage, System.Windows.Media.Animation.RepeatBehavior.Forever);
                });
            });




        }


        private void closeButton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }


        private void UpdateStatus(TextBlock icon, TextBlock text, bool ok)
        {
            if (ok)
            {
                icon.Text = "✅";
                icon.Foreground = Brushes.LimeGreen;
                text.Foreground = Brushes.LimeGreen;
            }
            else
            {
                icon.Text = "❌";
                icon.Foreground = Brushes.Red;
                text.Foreground = Brushes.Red;
            }
        }

        private bool HasActiveNetwork()
        {
            if (!NetworkInterface.GetIsNetworkAvailable())
                return false;

            return NetworkInterface.GetAllNetworkInterfaces()
                .Any(n =>
                    n.OperationalStatus == OperationalStatus.Up &&
                    n.NetworkInterfaceType != NetworkInterfaceType.Loopback &&
                    n.NetworkInterfaceType != NetworkInterfaceType.Tunnel &&
                    n.GetIPProperties().UnicastAddresses
                        .Any(a => a.Address.AddressFamily == AddressFamily.InterNetwork));
        }



        private async Task<bool> RunStartupChecksAsync()
        {
            RecheckButton.Visibility = Visibility.Collapsed;

            IvmsIcon.Text = "⏳";
            SqlIcon.Text = "⏳";
            NetIcon.Text = "⏳";

            await Task.Delay(500);
            bool ivmsOk = Process.GetProcessesByName("iVMS-4200.Framework.S").Any();
            UpdateStatus(IvmsIcon, IvmsText, ivmsOk);

            await Task.Delay(500);
            bool sqlOk = Process.GetProcessesByName("sqlservr").Any();
            UpdateStatus(SqlIcon, SqlText, sqlOk);

            await Task.Delay(500);
            bool netOk = HasActiveNetwork();
            UpdateStatus(NetIcon, NetText, netOk);

            bool allOk = ivmsOk && sqlOk && netOk;

            if (!allOk)
            {
                RecheckButton.Visibility = Visibility.Visible;
            }

            return allOk;
        }



        private async void RecheckButton_Click(object sender, RoutedEventArgs e)
        {
            bool passed = await RunStartupChecksAsync();

            if (passed)
            {
                var users = UserService.Load();
                var user = users.First(u => u.Username == UserName.Text);

                new Home(user).Show();
                Close();
            }
        }







        void SeedAdmin()
        {
            var users = UserService.Load();
            if (!users.Any())
            {
                users.Add(new Models.User
                {
                    Username = "admin",
                    PasswordHash = UserService.Hash("admin"),
                    Dashbord = true,
                    OderCheck = true,
                    Jobcard = true,
                    Customer = true,
                    Outsourcing = true,
                    Quotation = true,
                    Employee = true,
                    Attendance = true,
                    Payroll = true,
                    Paysheet = true,
                    UserManager = true,
                   
                });
                UserService.Save(users);
            }
        }

        private async void Login_Click(object sender, RoutedEventArgs e)
        {
            var users = UserService.Load();

            var user = users.FirstOrDefault(u =>
                u.Username == UserName.Text &&
                u.PasswordHash == UserService.Hash(Password.Password));

            if (user == null)
            {
                CustomMessageBox.Show("Invalid login");
                Logininfo.IsEnabled = true;
                return;
            }

      ((Button)sender).IsEnabled = false;
            Logininfo.IsEnabled = false;

            bool needStartupCheck =
                Properties.Settings.Default.SendDailyReport ||
                Properties.Settings.Default.SendAttendanceEmails;

            if (needStartupCheck)
            {
               
                bool passed = await RunStartupChecksAsync();
                IvmsVisibility.Visibility = Visibility.Visible;

                if (!passed)
                {
                    CustomMessageBox.Show(
                        "System check failed.\nFix the issues and click Recheck.",
                        "Startup Blocked",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);

                    ((Button)sender).IsEnabled = true;
                    return; // ⛔ DO NOT open Home
                }
            }

           

            // ✅ ONLY HERE Home opens
            new Home(user).Show();
            Close();
        }


    }
}

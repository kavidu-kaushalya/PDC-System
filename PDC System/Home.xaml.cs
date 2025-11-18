
using Google.Apis.PeopleService.v1;
using Hardcodet.Wpf.TaskbarNotification;
using PDC_System.Helpers;
using PDC_System.Outsourcing;
using PDC_System.Payroll_Details;
using PDC_System.Paysheets;
using PDC_System.Settings;
using System.Drawing;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;

namespace PDC_System
{
    /// <summary>
    /// Interaction logic for Home.xaml
    /// Main window with navigation, Google account info, tray icon, and theme handling.
    /// </summary>
    public partial class Home : Window
    {
        #region Fields

        private GoogleServiceManager googleManager;
        private PeopleServiceService peopleService;
        private TaskbarIcon _trayIcon;

        #endregion

        #region Constructor

        public Home()
        {
            InitializeComponent();
            ThemeManager.ApplyTheme(this); // Apply initial theme
            Loaded += Home_Loaded;

            SetupTrayIcon();

            LoadCalculateSettings();

            // Load Google account info
            RefreshGoogleAccountInfo();
        }

        #endregion

        #region Google Account

        /// <summary>
        /// Refresh all Google account related information
        /// </summary>
        public void RefreshGoogleAccountInfo()
        {
            GoogleAccountTokenCheck();
            GoogleAcoountinfo();
        }

        /// <summary>
        /// Check if a Google token exists in AppData
        /// </summary>
        public void GoogleAccountTokenCheck()
        {
            string credPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                                           "PDCBackupDemo", "token.json");
            string tokenFile = Path.Combine(credPath, "Google.Apis.Auth.OAuth2.Responses.TokenResponse-user");

            if (File.Exists(tokenFile))
            {
                GoogleAccountInfo.Visibility = Visibility.Visible;
            }
            else
            {
                GoogleAccountInfo.Visibility = Visibility.Hidden;

                // Clear previous user info when signed out
                Properties.Settings.Default.UserName = "";
                Properties.Settings.Default.UserEmail = "";
                Properties.Settings.Default.UserPicturePath = "";
                Properties.Settings.Default.Save();
            }
        }

        /// <summary>
        /// Load saved Google account info into UI
        /// </summary>
        public void GoogleAcoountinfo()
        {
            Dispatcher.Invoke(() =>
            {
                string name = Properties.Settings.Default.UserName;
                string email = Properties.Settings.Default.UserEmail;
                string savedPath = Properties.Settings.Default.UserPicturePath;

                lblUserName.Text = string.IsNullOrEmpty(name) ? "Unknown User" : name;
                lblEmail.Text = string.IsNullOrEmpty(email) ? "No Email" : email;

                try
                {
                    if (!string.IsNullOrEmpty(savedPath) && File.Exists(savedPath))
                    {
                        LoadProfileImage(savedPath);
                    }
                    else
                    {
                        // Clear profile image if no valid path or signed out
                        imgProfileEllipse.Fill = null;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Failed to load profile image: " + ex.Message);
                }
            });
        }

        private void LoadProfileImage(string path)
        {
            try
            {
                // Clear any existing image first
                imgProfileEllipse.Fill = null;
                
                if (string.IsNullOrEmpty(path) || !File.Exists(path))
                {
                    return;
                }

                // Load image into memory → file lock avoid karanna
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.CacheOption = BitmapCacheOption.OnLoad; // 🟢 important
                bitmap.CreateOptions = BitmapCreateOptions.IgnoreImageCache; // Force reload from file
                bitmap.UriSource = new Uri(path, UriKind.Absolute);
                bitmap.EndInit();
                bitmap.Freeze(); // 🟢 cross-thread safe + release file lock

                imgProfileEllipse.Fill = new ImageBrush(bitmap)
                {
                    Stretch = Stretch.UniformToFill
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine("Failed to load profile image: " + ex.Message);
                imgProfileEllipse.Fill = null;
            }
        }

        #endregion

        #region Tray Icon

        private void SetupTrayIcon()
        {
            _trayIcon = new TaskbarIcon();
            string iconPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "Main.ico");
            _trayIcon.Icon = new Icon(iconPath);
            _trayIcon.ToolTipText = "My WPF App";

            _trayIcon.TrayLeftMouseUp += (s, e) =>
            {
                this.Show();
                this.WindowState = WindowState.Normal;
                this.Activate();
            };

            var contextMenu = new ContextMenu();

            var menuOpen = new MenuItem { Header = "Open" };
            menuOpen.Click += (s, e) => ShowWindow();

            var menuSettings = new MenuItem { Header = "Backups" };
            menuSettings.Click += (s, e) => new BackupWindow().Show();

            var menuExit = new MenuItem { Header = "Exit" };
            menuExit.Click += (s, e) =>
            {
                _trayIcon.Dispose();
                Application.Current.Shutdown();
            };

            contextMenu.Items.Add(menuOpen);
            contextMenu.Items.Add(menuSettings);
            contextMenu.Items.Add(new Separator());
            contextMenu.Items.Add(menuExit);

            _trayIcon.ContextMenu = contextMenu;
        }

        private void ShowWindow()
        {
            this.Show();
            this.WindowState = WindowState.Normal;
            this.Activate();
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            _trayIcon.Dispose();
            base.OnClosing(e);
        }

        private void Window_StateChanged(object sender, EventArgs e)
        {
            if (this.WindowState == WindowState.Minimized)
                this.Hide();
        }

        #endregion

        #region Navigation

        private void Home_Loaded(object sender, RoutedEventArgs e)
        {
            MainContent.Content = new HomeUIWindow();
            LoadView(new HomeUIWindow());
        }

        private void OpenView1_Click(object sender, RoutedEventArgs e) => LoadView(new Customers());
        private void OpenView2_Click(object sender, RoutedEventArgs e) => LoadView(new Jobs());
        private void OpenView3_Click(object sender, RoutedEventArgs e) => LoadView(new QuotationWindow());
        private void OpenView4_Click(object sender, RoutedEventArgs e) => LoadView(new EmployeeWindow());
        private void OpenView5_Click(object sender, RoutedEventArgs e) => LoadView(new AttendanceWindow());
        //*private void OpenView6_Click(object sender, RoutedEventArgs e) => LoadView(new SalaryDetails());
        private void OpenView12_Click(object sender, RoutedEventArgs e) => LoadView(new PayrollDetailsWindow());
        private void OpenView13_Click(object sender, RoutedEventArgs e) => LoadView(new PaysheetWindow());
        private void OpenView8_Click(object sender, RoutedEventArgs e) => LoadView(new HomeUIWindow());
        private void OpenView9_Click(object sender, RoutedEventArgs e) => LoadView(new Oders());
        private void OpenView11_Click(object sender, RoutedEventArgs e) => LoadView(new OutsourcingWindow());

        private void LoadView(UserControl view)
        {
            TranslateTransform trans = new TranslateTransform();
            view.RenderTransform = trans;
            view.Opacity = 0;

            DoubleAnimation slideAnim = new DoubleAnimation
            {
                From = 50,
                To = 0,
                Duration = TimeSpan.FromMilliseconds(400),
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseInOut }
            };

            DoubleAnimation fadeAnim = new DoubleAnimation
            {
                From = 0,
                To = 1,
                Duration = TimeSpan.FromMilliseconds(400)
            };

            MainContent.Content = view;

            trans.BeginAnimation(TranslateTransform.YProperty, slideAnim);
            view.BeginAnimation(UserControl.OpacityProperty, fadeAnim);
        }

        #endregion

        #region Window Control

        public static readonly DependencyProperty IsLightProperty =
            DependencyProperty.Register("IsLight", typeof(bool), typeof(Home), new PropertyMetadata(true));

        public bool IsLight
        {
            get => (bool)GetValue(IsLightProperty);
            set => SetValue(IsLightProperty, value);
        }

        private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
                DragMove();
        }

        private void Minimize_Click(object sender, RoutedEventArgs e) => this.WindowState = WindowState.Minimized;

        private void Maximize_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = this.WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            foreach (Window win in Application.Current.Windows.OfType<BackupWindow>())
                win.Close();

            this.Close();
        }

        #endregion

        #region Settings Window

        private void Setting_Click(object sender, RoutedEventArgs e)
        {
            // Check if SettingsWindow is already open
            var existingWindow = Application.Current.Windows
                                    .OfType<SettingsWindow>()
                                    .FirstOrDefault();

            if (existingWindow != null)
            {
                // If already open, bring it to front
                existingWindow.Activate();
            }
            else
            {
                // If not open, create and show a new one
                var settingsWindow = new SettingsWindow();
                settingsWindow.GoogleAccountChanged += () =>
                {
                    // Refresh Home UI when Google account changes
                    RefreshGoogleAccountInfo();
                };
                settingsWindow.Show();
            }
        }


        #endregion

        private void AboutButton_Click(object sender, RoutedEventArgs e)
        {
            AboutWindow about = new AboutWindow();
            about.ShowDialog();
        }



        private void LoadCalculateSettings()
        {
           
            // 🔍 Try to find existing SettingsWindow
            var settingsWindow = Application.Current.Windows
                .OfType<SettingsWindow>()
                .FirstOrDefault();

            if (settingsWindow != null)
            {
                settingsWindow.BtnLoad_Click();
               
               
            }
            else
            {
                var backgroundSettings = new SettingsWindow();
                backgroundSettings.Visibility = Visibility.Hidden;

                if (backgroundSettings != null)
                {
                    backgroundSettings.BtnLoad_Click();
                    backgroundSettings.Close();
                    
                   
                }
                else
                {
                    MessageBox.Show("⚠️ Settings window could not be created.",
                                    "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }

            // 🧾 Attendance Manager section
            var manager = new PDC_System.Services.AttendanceManager();

            if (manager != null)
            {
                var records = manager.LoadAttendance();

                if (records != null && records.Any())
                {
                    manager.SaveAllAttendanceRecords(records);
                    NotificationHelper.ShowNotification("PDC System!", "Attendance Calculate Complete!");
                }
                else
                {
                    MessageBox.Show("⚠️ No attendance records found to process.",
                                    "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            else
            {
                MessageBox.Show("⚠️ Attendance Manager not initialized.",
                                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


    }
}

using Google.Apis.PeopleService.v1;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using PDC_System.Helpers;
using PDC_System.Properties;
using System;
using System.Data;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace PDC_System.Settings
{
    /// <summary>
    /// Settings Window handles Google Sign-In, Startup toggle, and Theme selection.
    /// </summary>
    public partial class SettingsWindow : Window
    {
        #region Fields


        private GoogleServiceManager googleManager;
        private PeopleServiceService peopleService;
        private string jsonFile = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Savers/ivms.json");
        public event Action GoogleAccountChanged;
        private bool isLoaded = false;

        #endregion

        #region Constructor

        public SettingsWindow()
        {
            InitializeComponent();
            this.Loaded += Window_Loaded;
            ThemeManager.ApplyTheme(this);
            // Startup checkbox
            chkStartup.IsChecked = StartupManager.IsInStartup();

            // Check existing Google auth
            GoogleAuthCheckSaveFile();

            // Initialize Google manager
            googleManager = new GoogleServiceManager();
            // Fix: Subscribe to the event with the method that has parameters
            googleManager.UserInfoLoaded += GoogleManager_UserInfoLoaded;

            // Load saved user info immediately
            GoogleManager_UserInfoLoaded();
            LoadSettings();
            isEnableStatus();
            LoadETF();
            LoadAttendance();
            isLoaded = true;
            // Apply theme when window loads
            ThemeManager.ApplyTheme(this);
        }

        #endregion

        #region Google Sign-In & Sign-Out

        private async void GoogleSingin_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (googleManager == null)
                {
                    MessageBox.Show("GoogleServiceManager not initialized!");
                    return;
                }

                await googleManager.InitializeAsync();

                // Update UI visibility GoogleSignButton & GoogleAccount


                // Refresh UI after successful sign-in
                RefreshUI();

                // Notify other windows that Google account status changed
                GoogleAccountChanged?.Invoke();


            }
            catch (Exception ex)
            {
                MessageBox.Show("Error during sign-in: " + ex.Message);
            }
        }

        private async void BtnSignOut_Click(object sender, RoutedEventArgs e)
        {
            if (googleManager != null)
            {
                // Clear UI first → release ImageBrush reference
                imgProfileEllipse.Fill = null;

                // Small delay for file lock release
                await Task.Delay(100); // 100 ms

                // Sign out from Google services
                await googleManager.SignOutAsync();

                // Clear UI elements
                lblUserName.Text = "User Name";
                lblEmail.Text = "No Email";

                // Update UI visibility
                RefreshUI();

                // Show sign-in button again  


                // Notify other windows that Google account status changed
                GoogleAccountChanged?.Invoke();
            }
        }

        #endregion

        #region UI Management

        private async void RefreshUI()
        {
            // Small delay to ensure Settings are saved
            await Task.Delay(100);

            // Check Google token status
            GoogleAuthCheckSaveFile();

            // Load user info
            GoogleManager_UserInfoLoaded();
        }

        #endregion

        #region Load Google User Info

        /// <summary>
        /// Load saved user info from Settings (offline fallback)
        /// </summary>
        public void GoogleManager_UserInfoLoaded()
        {
            Dispatcher.Invoke(() =>
            {
                string name = Properties.Settings.Default.UserName;
                string savedPath = Properties.Settings.Default.UserPicturePath;
                string email = Properties.Settings.Default.UserEmail;

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
                        // Clear profile image if no valid path
                        imgProfileEllipse.Fill = null;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Failed to load profile image: " + ex.Message);
                }
            });
        }

        /// <summary>
        /// Update user info after GoogleServiceManager loads data online
        /// </summary>
        private async void GoogleManager_UserInfoLoaded(string name, string pictureUrl, string email)
        {
            // Small delay to ensure file operations are complete
            await Task.Delay(200);

            Dispatcher.Invoke(() =>
            {
                lblUserName.Text = name;
                lblEmail.Text = email;

                // Clear existing image first
                imgProfileEllipse.Fill = null;

                // Force UI refresh
                this.UpdateLayout();

                // Small delay before loading new image
                Task.Delay(100).ContinueWith(_ =>
                {
                    Dispatcher.Invoke(() =>
                    {
                        // Try to load from saved path first (already downloaded)
                        string savedPath = Properties.Settings.Default.UserPicturePath;
                        if (!string.IsNullOrEmpty(savedPath) && File.Exists(savedPath))
                        {
                            LoadProfileImage(savedPath);
                        }
                        else if (!string.IsNullOrEmpty(pictureUrl))
                        {
                            // Fallback to URL if saved path doesn't exist
                            LoadProfileImageFromUrl(pictureUrl);
                        }

                        // Refresh UI visibility
                        GoogleAuthCheckSaveFile();
                    });
                });
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

                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.CacheOption = BitmapCacheOption.OnLoad; // avoid file lock
                bitmap.CreateOptions = BitmapCreateOptions.IgnoreImageCache; // Force reload from file
                bitmap.UriSource = new Uri(path, UriKind.Absolute);
                bitmap.EndInit();
                bitmap.Freeze(); // thread-safe

                imgProfileEllipse.Fill = new ImageBrush(bitmap) { Stretch = Stretch.UniformToFill };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to load profile image from path: {ex.Message}");
                imgProfileEllipse.Fill = null;
            }
        }

        private void LoadProfileImageFromUrl(string url)
        {
            try
            {
                // Clear any existing image first
                imgProfileEllipse.Fill = null;

                if (string.IsNullOrEmpty(url))
                {
                    return;
                }

                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.CacheOption = BitmapCacheOption.OnLoad; // avoid file lock
                bitmap.CreateOptions = BitmapCreateOptions.IgnoreImageCache; // Force download from URL
                bitmap.UriSource = new Uri(url, UriKind.Absolute);
                bitmap.EndInit();
                bitmap.Freeze(); // thread-safe

                imgProfileEllipse.Fill = new ImageBrush(bitmap) { Stretch = Stretch.UniformToFill };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to load profile image from URL: {ex.Message}");
                imgProfileEllipse.Fill = null;
            }
        }

        #endregion

        #region Google Token Check

        /// <summary>
        /// Check if Google token exists in AppData
        /// </summary>
        /// 
        private void GoogleAuthCheckSaveFile()
        {


            // Folder where FileDataStore saves
            string credPath = System.IO.Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "PDCBackupDemo",
                "token.json"
            );

            // Token file inside that folder
            string tokenFile = System.IO.Path.Combine(
                credPath,
                "Google.Apis.Auth.OAuth2.Responses.TokenResponse-user"
            );

            if (File.Exists(tokenFile))
            {

                GoogleSignButton.Visibility = Visibility.Collapsed;
                GoogleAccount.Visibility = Visibility.Visible;


            }

            else
            {
                // Token not found → open SignInWindow
                GoogleSignButton.Visibility = Visibility.Visible;
                GoogleAccount.Visibility = Visibility.Collapsed;




            }
        }


        #endregion

        #region Startup Checkbox

        private void chkStartup_Checked(object sender, RoutedEventArgs e)
        {
            StartupManager.AddToStartup();
            Properties.Settings.Default.RunAtStartup = true;
            Properties.Settings.Default.Save();
        }

        private void chkStartup_Unchecked(object sender, RoutedEventArgs e)
        {
            StartupManager.RemoveFromStartup();
            Properties.Settings.Default.RunAtStartup = false;
            Properties.Settings.Default.Save();
        }

        #endregion

        #region Theme Management

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            switch (ThemeManager.CurrentTheme)
            {
                case ThemeMode.Light:
                    LightRadio.IsChecked = true;
                    break;
                case ThemeMode.Dark:
                    DarkRadio.IsChecked = true;
                    break;
                case ThemeMode.SystemDefault:
                    SystemRadio.IsChecked = true;
                    break;
            }
        }

        private void LightRadio_Checked(object sender, RoutedEventArgs e)
        {
            ThemeManager.CurrentTheme = ThemeMode.Light;
            ThemeManager.UpdateAllWindows();
        }

        private void DarkRadio_Checked(object sender, RoutedEventArgs e)
        {
            ThemeManager.CurrentTheme = ThemeMode.Dark;
            ThemeManager.UpdateAllWindows();
        }

        private void SystemRadio_Checked(object sender, RoutedEventArgs e)
        {
            ThemeManager.CurrentTheme = ThemeMode.SystemDefault;
            ThemeManager.UpdateAllWindows();
        }

        #endregion

        #region Database Management



        private void LoadSettings()
        {
            TxtIP.Text = Properties.Settings.Default.DB_IP;
            TxtPort.Text = Properties.Settings.Default.DB_Port;
            TxtUser.Text = Properties.Settings.Default.DB_User;
            TxtPass.Password = Properties.Settings.Default.DB_Password;
            TxtDBName.Text = Properties.Settings.Default.DB_Name;
            TxtTable.Text = Properties.Settings.Default.DB_Table;

            // Load cached JSON data
            DataTable cachedData = LoadDataFromJson();
            if (cachedData != null)
            {




            }
        }

        private void SaveDataToJson(DataTable dt)
        {
            // Serialize with indentation for human readability
            string json = JsonConvert.SerializeObject(dt, Formatting.Indented);
            File.WriteAllText(jsonFile, json);
        }

        private DataTable LoadDataFromJson()
        {
            if (!File.Exists(jsonFile)) return null;

            string json = File.ReadAllText(jsonFile);
            return JsonConvert.DeserializeObject<DataTable>(json);
        }

        private void isEnableStatus()
        {
            if (Properties.Settings.Default.isEnable)
            {
                // When logged in / enabled
                SettingsDisable.IsEnabled = false;
                LoginBtn.Visibility = Visibility.Hidden;
                LogoutBtn.Visibility = Visibility.Visible;
            }
            else
            {
                // When logged out / disabled
                SettingsDisable.IsEnabled = true;
                LoginBtn.Visibility = Visibility.Visible;
                LogoutBtn.Visibility = Visibility.Hidden;
            }
        }


        private string GetConnectionString()
        {
            return $"Server={Properties.Settings.Default.DB_IP},{Properties.Settings.Default.DB_Port};" +
                   $"Database={Properties.Settings.Default.DB_Name};" +
                   $"User Id={Properties.Settings.Default.DB_User};" +
                   $"Password={Properties.Settings.Default.DB_Password};" +
                   "Encrypt=True;TrustServerCertificate=True;";
        }



        private string GetConnectionStringTemp()
        {
            return $"Server={TxtIP.Text},{TxtPort.Text};" +
                   $"Database={TxtDBName.Text};" +
                   $"User Id={TxtUser.Text};" +
                   $"Password={TxtPass.Password};" +
                   "Encrypt=True;TrustServerCertificate=True;";
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            // 1️⃣ Check if all required fields have value
            if (string.IsNullOrWhiteSpace(TxtIP.Text) ||
                string.IsNullOrWhiteSpace(TxtPort.Text) ||
                string.IsNullOrWhiteSpace(TxtUser.Text) ||
                string.IsNullOrWhiteSpace(TxtPass.Password) ||
                string.IsNullOrWhiteSpace(TxtDBName.Text) ||
                string.IsNullOrWhiteSpace(TxtTable.Text))
            {
                MessageBox.Show("⚠ Please fill all fields before saving!");
                return;
            }

            // 2️⃣ Test SQL Server connection using TEMP values (not Settings)
            try
            {
                using (SqlConnection conn = new SqlConnection(GetConnectionStringTemp()))
                {
                    conn.Open();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("❌ Connection failed: " + ex.Message);
                return; // Stop saving if connection fails
            }

            // 3️⃣ Save settings (only if connection successful)
            Properties.Settings.Default.DB_IP = TxtIP.Text;
            Properties.Settings.Default.DB_Port = TxtPort.Text;
            Properties.Settings.Default.DB_User = TxtUser.Text;
            Properties.Settings.Default.DB_Password = TxtPass.Password;
            Properties.Settings.Default.DB_Name = TxtDBName.Text;
            Properties.Settings.Default.DB_Table = TxtTable.Text;
            Properties.Settings.Default.isEnable = true;
            Properties.Settings.Default.Save();
            isEnableStatus();

            MessageBox.Show("✅ Settings saved and connection verified!");


            isEnableStatus();
        }



        public void BtnLoad_Click()
        {
            // ✅ Check if all required settings have value
            if (string.IsNullOrWhiteSpace(Properties.Settings.Default.DB_IP) ||
                string.IsNullOrWhiteSpace(Properties.Settings.Default.DB_Port) ||
                string.IsNullOrWhiteSpace(Properties.Settings.Default.DB_User) ||
                string.IsNullOrWhiteSpace(Properties.Settings.Default.DB_Password) ||
                string.IsNullOrWhiteSpace(Properties.Settings.Default.DB_Name) ||
                string.IsNullOrWhiteSpace(Properties.Settings.Default.DB_Table))
            {
                NotificationHelper.ShowNotification("PDC System!", "⚠ Database settings are empty! Please save your settings first.");
                return; // stop loading
            }

            try
            {
                using (SqlConnection conn = new SqlConnection(GetConnectionString()))
                {
                    conn.Open();
                    string query = $"SELECT * FROM {Properties.Settings.Default.DB_Table}";
                    SqlDataAdapter da = new SqlDataAdapter(query, conn);
                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    // Save to JSON for next time
                    SaveDataToJson(dt);

                    DaliyAttendance.CheckTodayAttendance();
                    EmployeeDailyAttendance.CheckTodayAttendanceAsync();
                    NotificationHelper.ShowNotification("PDC System!", "IVMS Calculate Complete!");

                    NotificationHelper.ShowNotification("PDC System!", "✅ Data loaded successfully!");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading data: " + ex.Message);
            }
        }


        private void BtnLogout_Click(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.Reset();

            TxtIP.Clear();
            TxtPort.Clear();
            TxtUser.Clear();
            TxtPass.Clear();
            TxtDBName.Clear();
            TxtTable.Clear();
            Properties.Settings.Default.isEnable = false;
            Properties.Settings.Default.Save();
            isEnableStatus();


            MessageBox.Show("🔒 Logged out, settings cleared.");
        }

        private void TxtNumber_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            // allow only 0-9
            e.Handled = !System.Text.RegularExpressions.Regex.IsMatch(e.Text, "^[0-9]+$");
        }

        private void TxtNumber_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            // allow Backspace, Delete, Tab, Left, Right
            if (e.Key == Key.Back || e.Key == Key.Delete ||
                e.Key == Key.Tab || e.Key == Key.Left || e.Key == Key.Right)
            {
                e.Handled = false;
            }
        }


        private void TxtIP_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            TextBox textBox = sender as TextBox;

            // ✅ Allow digits and dot
            if (!char.IsDigit(e.Text, 0) && e.Text != ".")
            {
                e.Handled = true; // Block
                return;
            }

            string currentText = textBox.Text.Insert(textBox.SelectionStart, e.Text);

            // ❌ Prevent multiple consecutive dots
            if (currentText.Contains(".."))
            {
                e.Handled = true;
                return;
            }

            // ❌ Prevent more than 3 digits in each block
            string[] blocks = currentText.Split('.');
            foreach (string block in blocks)
            {
                if (block.Length > 3)
                {
                    e.Handled = true;
                    return;
                }

                // ❌ Prevent values > 255
                if (int.TryParse(block, out int value) && value > 255)
                {
                    e.Handled = true;
                    return;
                }
            }

            // ❌ Prevent more than 4 blocks
            if (blocks.Length > 4)
            {
                e.Handled = true;
            }
        }







        #endregion

        #region Close Button

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }


        #endregion





        #region ETF


        private void LoadETF()
        {
            ETFEmployee.Text = Properties.Settings.Default.ETFEmployee.ToString("0.##", CultureInfo.InvariantCulture);
            ETFEmployer.Text = Properties.Settings.Default.ETFEmployer.ToString("0.##", CultureInfo.InvariantCulture);
            btnSave.Visibility = Visibility.Collapsed;
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!isLoaded) return;

            if (decimal.TryParse(ETFEmployee.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal empVal) &&
                decimal.TryParse(ETFEmployer.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal emrVal))
            {
                // show Save button only if values changed
                if (empVal != Properties.Settings.Default.ETFEmployee ||
                    emrVal != Properties.Settings.Default.ETFEmployer)
                {
                    btnSave.Visibility = Visibility.Visible;
                }
                else
                {
                    btnSave.Visibility = Visibility.Collapsed;
                }
            }
            else
            {
                // invalid number input
                btnSave.Visibility = Visibility.Collapsed;
            }
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (decimal.TryParse(ETFEmployee.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal empVal) &&
                decimal.TryParse(ETFEmployer.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal emrVal))
            {
                Properties.Settings.Default.ETFEmployee = empVal;
                Properties.Settings.Default.ETFEmployer = emrVal;
                Properties.Settings.Default.Save();

                btnSave.Visibility = Visibility.Collapsed;

                MessageBox.Show("Settings saved successfully ✅", "Saved", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show("Please enter valid decimal values ⚠️", "Invalid Input", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }


        #endregion

        #region Attendance
        private void LoadAttendance()
        {
            // Load saved decimal values into TextBoxes
            OT_RoundMinutes.Text = Properties.Settings.Default.OT_RoundMinutes.ToString("0.##", CultureInfo.InvariantCulture);
            TimerIntervalMinutes.Text = Properties.Settings.Default.TimerIntervalMinutes.ToString("0.##", CultureInfo.InvariantCulture);
            chkSendDailyReport.IsChecked = Properties.Settings.Default.SendDailyReport;
            SendAttendanceEmails.IsChecked = Properties.Settings.Default.SendAttendanceEmails;

            // Hide save button initially
            AttendancebtnSave.Visibility = Visibility.Collapsed;

            // Attach event handlers to detect text changes
            OT_RoundMinutes.TextChanged += AttendanceSettingChanged;
            TimerIntervalMinutes.TextChanged += AttendanceSettingChanged;
            chkSendDailyReport.Checked += AttendanceCheckBoxChanged;
            chkSendDailyReport.Unchecked += AttendanceCheckBoxChanged;
            SendAttendanceEmails.Checked += AttendanceCheckBoxChanged;
            SendAttendanceEmails.Unchecked += AttendanceCheckBoxChanged;
        }

        // Show Save button only when values actually change
        private void AttendanceSettingChanged(object sender, TextChangedEventArgs e)
        {
            bool otChanged = decimal.TryParse(OT_RoundMinutes.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal otValue) &&
                             otValue != Properties.Settings.Default.OT_RoundMinutes;

            bool timerChanged = decimal.TryParse(TimerIntervalMinutes.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal timerValue) &&
                                timerValue != Properties.Settings.Default.TimerIntervalMinutes;

            AttendancebtnSave.Visibility = (otChanged || timerChanged) ? Visibility.Visible : Visibility.Collapsed;
        }

        private void AttendanceCheckBoxChanged(object sender, RoutedEventArgs e)
        {
            if ((sender == chkSendDailyReport && chkSendDailyReport.IsChecked != Properties.Settings.Default.SendDailyReport) ||
                (sender == SendAttendanceEmails && SendAttendanceEmails.IsChecked != Properties.Settings.Default.SendAttendanceEmails))
            {
                AttendancebtnSave.Visibility = Visibility.Visible;
            }
            else
            {
                AttendancebtnSave.Visibility = Visibility.Collapsed;
            }
        }


        private void AttendancebtnSave_Click(object sender, RoutedEventArgs e)
        {
            if (decimal.TryParse(OT_RoundMinutes.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal otValue) &&
                decimal.TryParse(TimerIntervalMinutes.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal timerValue))
            {
                // Save the settings
                Properties.Settings.Default.OT_RoundMinutes = (int)Math.Round(otValue);
                Properties.Settings.Default.TimerIntervalMinutes = (int)Math.Round(timerValue);
                Properties.Settings.Default.SendDailyReport = chkSendDailyReport.IsChecked == true;
                Properties.Settings.Default.SendAttendanceEmails = SendAttendanceEmails.IsChecked == true;
                Properties.Settings.Default.Save();

                // Hide the save button
                AttendancebtnSave.Visibility = Visibility.Collapsed;

                MessageBox.Show("Attendance settings saved successfully ✅", "Saved", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show("Please enter valid decimal values ⚠️", "Invalid Input", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
        #endregion




    }
}

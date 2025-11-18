using Microsoft.Win32;
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using XamlAnimatedGif;

namespace PDC_System
{
    public partial class Loading : Window
    {
        private GoogleServiceManager googleManager;

        public Loading()
        {
            InitializeComponent();
            SetTitleBarColor();
            GoogleAccountTokenCheck();
            Loaded += LoadingWindow_Loaded; // window render වෙන විට
            googleManager = new GoogleServiceManager();
            googleManager.UserInfoLoaded += GoogleManager_UserInfoLoaded;
        }

        private void GoogleManager_UserInfoLoaded(string name, string pictureUrl, string email)
        {
            // Handle UI update
        }

        private async void LoadingWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // GIF load background thread
            await Task.Run(() =>
            {
                string baseFolder = AppDomain.CurrentDomain.BaseDirectory;
                string gifPath = System.IO.Path.Combine(baseFolder, "Assets", "crop.gif");
                var gifUri = new Uri(gifPath, UriKind.Absolute);

                Dispatcher.Invoke(() =>
                {
                    AnimationBehavior.SetSourceUri(MyGifImage, gifUri);
                    AnimationBehavior.SetRepeatBehavior(MyGifImage, System.Windows.Media.Animation.RepeatBehavior.Forever);
                });
            });

            // Add window hook for theme changes
            var hwndSource = HwndSource.FromHwnd(new WindowInteropHelper(this).Handle);
            hwndSource.AddHook(WndProc);

            // Animate UI elements
            AnimateElements();

            // Auto open Home window if token exists
            if (File.Exists(GetTokenFilePath()))
            {
                await Task.Delay(10000); // 10-second loading
                OpenHomeWindow();
            }
        }

        private string GetTokenFilePath()
        {
            string credPath = System.IO.Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "PDCBackupDemo",
                "token.json"
            );

            return System.IO.Path.Combine(credPath, "Google.Apis.Auth.OAuth2.Responses.TokenResponse-user");
        }

        private void AnimateElements()
        {
            int delay = 0;

            foreach (UIElement element in MainGrid.Children)
            {
                TranslateTransform trans = new TranslateTransform();
                element.RenderTransform = trans;
                element.Opacity = 0;

                DoubleAnimation slideAnim = new DoubleAnimation
                {
                    From = 50,
                    To = 0,
                    Duration = TimeSpan.FromMilliseconds(2000),
                    BeginTime = TimeSpan.FromMilliseconds(delay),
                    EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
                };

                DoubleAnimation fadeAnim = new DoubleAnimation
                {
                    From = 0,
                    To = 1,
                    Duration = TimeSpan.FromMilliseconds(2000),
                    BeginTime = TimeSpan.FromMilliseconds(delay),
                    EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
                };

                Storyboard sb = new Storyboard();
                sb.Children.Add(slideAnim);
                sb.Children.Add(fadeAnim);

                Storyboard.SetTarget(slideAnim, element);
                Storyboard.SetTargetProperty(slideAnim, new PropertyPath("(UIElement.RenderTransform).(TranslateTransform.Y)"));

                Storyboard.SetTarget(fadeAnim, element);
                Storyboard.SetTargetProperty(fadeAnim, new PropertyPath(UIElement.OpacityProperty));

                sb.Begin();
                delay += 50;
            }
        }

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            const int WM_SETTINGCHANGE = 0x001A;
            if (msg == WM_SETTINGCHANGE)
            {
                string changedSetting = Marshal.PtrToStringAuto(lParam);
                if (changedSetting == "ImmersiveColorSet" || changedSetting == "WindowsThemeElement")
                {
                    SetTitleBarColor();
                }
            }
            return IntPtr.Zero;
        }

        private void SetTitleBarColor()
        {
            var key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize");
            if (key != null)
            {
                var value = key.GetValue("SystemUsesLightTheme");
                bool isLight = value != null && (int)value == 1;

                if (isLight)
                {
                    LoadingB.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFFFFF"));
                    LoadingWindow.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1A1A1A"));
                    Application.Current.Resources["BtnSignLoadingColor"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF0078D7"));
                    Application.Current.Resources["BtnSignLoadingFColor"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("White"));
                }
                else
                {
                    LoadingB.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1A1A1A"));
                    LoadingWindow.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFFFFF"));
                    Application.Current.Resources["BtnSignLoadingColor"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("White"));
                    Application.Current.Resources["BtnSignLoadingFColor"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1A1A1A"));
                }
            }
        }

        private void GoogleAccountTokenCheck()
        {
            string tokenFile = GetTokenFilePath();

            if (File.Exists(tokenFile))
            {
                GoogleSignButton.Visibility = Visibility.Collapsed;
                LoadingBar.Visibility = Visibility.Visible;
            }
            else
            {
                LoadingBar.Visibility = Visibility.Collapsed;
                GoogleSignButton.Visibility = Visibility.Visible;
            }
        }

        private async void Continue_click(object sender, RoutedEventArgs e)
        {
            await LoadingWindow_LoadedAfterAsync();
        }

        private async void btnOpenMain_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (googleManager == null)
                {
                    MessageBox.Show("GoogleServiceManager not initialized!");
                    return;
                }

                await googleManager.InitializeAsync();
                await LoadingWindow_LoadedAfterAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error during sign-in: " + ex.Message);
            }
        }

        private async Task LoadingWindow_LoadedAfterAsync()
        {
            LoadingBar.Visibility = Visibility.Visible;
            GoogleSignButton.Visibility = Visibility.Collapsed;
            await Task.Delay(10000); // 10 seconds
            OpenHomeWindow();
        }

        private void OpenHomeWindow()
        {
            Home homeWindow = new Home();
            homeWindow.Show();
            this.Close();
        }
    }
}

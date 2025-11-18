using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using XamlAnimatedGif;

namespace PDC_System
{
    public partial class VerificationWindow : Window
    {
        private string currentCode;
        private readonly Action onVerified;
        private readonly Func<Task<string>> resendCallback; // callback to MainWindow
        private DateTime expiryTime;
        private DispatcherTimer timer;

        public VerificationWindow(string code, Action onVerifiedCallback, Func<Task<string>> resendFunc)
        {
            InitializeComponent();
            currentCode = code;
            onVerified = onVerifiedCallback;
            resendCallback = resendFunc;

            this.Loaded += VerificationWindow_Loaded; // Window render වෙලා පසුව run කරන handler

            StartCountdown();
        }

        private async void VerificationWindow_Loaded(object sender, RoutedEventArgs e)
        {
            await Task.Run(() =>
            {
                string baseFolder = AppDomain.CurrentDomain.BaseDirectory;
                string gifPath = System.IO.Path.Combine(baseFolder, "Assets", "Login.gif");
                var gifUri = new Uri(gifPath, UriKind.Absolute);

                // Dispatcher call to UI thread for GIF assignment
                Dispatcher.Invoke(() =>
                {
                    AnimationBehavior.SetSourceUri(MyGifImage, gifUri);
                    AnimationBehavior.SetRepeatBehavior(MyGifImage, System.Windows.Media.Animation.RepeatBehavior.Forever);
                });
            });
        }

        private void StartCountdown()
        {
            expiryTime = DateTime.UtcNow.AddMinutes(2);
            TimerTextBlock.Text = "Time remaining: 2:00";

            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += Timer_Tick;
            timer.Start();

            ResendButton.Visibility = Visibility.Collapsed;
            VerifyButton.IsEnabled = true;
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            var remaining = expiryTime - DateTime.UtcNow;
            if (remaining <= TimeSpan.Zero)
            {
                timer.Stop();
                TimerTextBlock.Text = "Code expired.";
                StatusTextBlock.Text = "Please request a new code.";
                VerifyButton.IsEnabled = false;
                ResendButton.Visibility = Visibility.Visible;
            }
            else
            {
                TimerTextBlock.Text = $"Time remaining: {remaining.Minutes:D2}:{remaining.Seconds:D2}";
            }
        }

        private void VerifyButton_Click(object sender, RoutedEventArgs e)
        {
            var entered = CodeTextBox.Text?.Trim();
            if (entered == currentCode)
            {
                timer.Stop();
                onVerified?.Invoke();
                Close();
            }
            else
            {
                StatusTextBlock.Text = "Incorrect code. Try again.";
            }
        }

        private async void ResendButton_Click(object sender, RoutedEventArgs e)
        {
            StatusTextBlock.Text = "Sending new code...";
            string newCode = await resendCallback(); // ask MainWindow to send again
            if (!string.IsNullOrEmpty(newCode))
            {
                currentCode = newCode;
                StatusTextBlock.Text = "New code sent! Check your email.";
                StartCountdown(); // restart timer
            }
            else
            {
                StatusTextBlock.Text = "Failed to resend code. Try again.";
            }
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}

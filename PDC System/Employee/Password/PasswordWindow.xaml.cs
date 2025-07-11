using System.Windows;
using System.Windows.Controls;

namespace PDC_System
{
    public partial class PasswordWindow : Window
    {
        public string Password { get; private set; }

        public PasswordWindow()
        {
            InitializeComponent();
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            Password = PasswordBox.Password;

            // Validate the password
            if (Password == "pdc123")  // Replace with your actual password or validation logic
            {
                DialogResult = true;
                Close();
            }
            else
            {
                MessageBox.Show("Incorrect password, please try again.");
                PasswordBox.Clear();
            }
        }

        private void ForgotPassword_Click(object sender, RoutedEventArgs e)
        {
            // This is where you can trigger your password recovery process.
            // For now, we’ll show a message box for simplicity.
            MessageBox.Show("If you've forgotten your password, please contact support.");

            // You can also navigate to a password reset page, open a form, etc.
        }
    }
}
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace PDC_System
{
    /// <summary>
    /// Interaction logic for WhatsAppDetails.xaml
    /// </summary>
    public partial class EmailDetails : Window
    {
        private const string predefinedSubject = "Test_Email";
        private const string predefinedBody = "This_is_a_test_email_to_check_email_functionality._Please_confirm_if_you_received_it_successfully.";

        public EmailDetails()
        {
            InitializeComponent();
        }

        private void SendEmail_Click(object sender, RoutedEventArgs e)
        {
            string email = WebUtility.UrlEncode(txtEmail.Text);
            string subject = WebUtility.UrlEncode(predefinedSubject);
            string body = WebUtility.UrlEncode(predefinedBody);

            if (string.IsNullOrWhiteSpace(email))
            {
                MessageBox.Show("Please enter an email address.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string mailto = $"mailto:{email}?subject={subject}&body={body}";
            try
            {
                Process.Start(new ProcessStartInfo(mailto) { UseShellExecute = true });
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error opening email client: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}

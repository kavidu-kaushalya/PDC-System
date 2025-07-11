using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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
    public partial class WhatsAppDetails : Window
    {
        public WhatsAppDetails()
        {
            InitializeComponent();
        }

        private void SendMessageButton_Click(object sender, RoutedEventArgs e)
        {
            // Get the contact number entered by the user
            string contactNo = ContactNumberTextBox.Text.Trim();

            // Validate the contact number (it should be 9 or 10 characters in length)
            if (string.IsNullOrEmpty(contactNo) || (contactNo.Length != 9 && contactNo.Length != 10))
            {
                MessageBox.Show("Please enter a valid contact number (9 or 10 digits).");
                return;
            }

            // If the contact number has 9 digits, add the country code +94
            if (contactNo.Length == 9)
            {
                contactNo = "+94" + contactNo;
            }
            // If the contact number starts with '0' and has 10 digits, replace the '0' with '+94'
            else if (contactNo.StartsWith("0") && contactNo.Length == 10)
            {
                contactNo = "+94" + contactNo.Substring(1);
            }

            // Pre-filled message
            string message = "*Hello!*\nThank you for reaching out to Priyantha Die Cutting! We appreciate your message.\n\n" +
                             "*Our services include:*\nDigital & Offset Printing\nVisiting Cards, Flyers, Brochures*\n Stickers, Banners, and More!\n\n" +
                             "*Business Hours:*\n\n*Weekday* - 8:30AM - 5:PM \n*Saturday* - 8:30 AM - 1:30PM\n*Sunday Not Working*\n *Location:* https://maps.app.goo.gl/Sijz1ANp5u4RRh9P7\n\n" +
                             "We’ll get back to you shortly. If urgent, call us at *0112 864267 | 072 2978667* .\n\nBest regards,\nPriyantha Die Cutting";

            // URL encode the message
            string encodedMessage = Uri.EscapeDataString(message);

            // Create the WhatsApp link
            string whatsappLink = $"https://wa.me/{contactNo}?text={encodedMessage}";

            // Open the WhatsApp link in the browser
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName = whatsappLink,
                UseShellExecute = true
            });
        }


        private void ContactNumberTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            // Check if the entered text is a number and if the current length is less than 10
            if (!e.Text.All(char.IsDigit) || ContactNumberTextBox.Text.Length >= 10)
            {
                e.Handled = true;
            }
        }
    }
}

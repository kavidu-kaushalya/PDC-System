using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using System.IO;

public static class LoanHistoryEmailService
{
    public static bool SendLoanHistoryEmail(
        string toEmail,
        string employeeName,
        string pdfPath)
    {
        try
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("PDC System", "pdc.system.app@gmail.com"));
            message.To.Add(new MailboxAddress(employeeName, toEmail));

            message.Subject = $"Loan History Report - {employeeName}";

            var builder = new BodyBuilder();
            builder.TextBody = $"Dear {employeeName},\n\nPlease find attached your loan history report.\n\nThank you.";

            // Attach PDF
            if (File.Exists(pdfPath))
            {
                builder.Attachments.Add(pdfPath);
            }

            message.Body = builder.ToMessageBody();

            using (var client = new SmtpClient())
            {
                client.Connect("smtp.gmail.com", 587, SecureSocketOptions.StartTls);

                // Login to email account
                client.Authenticate("pdc.system.app@gmail.com", "xfqdchnfsrrbyvyi");

                client.Send(message);
                client.Disconnect(true);
            }

            return true;
        }
        catch (Exception ex)
        {
            System.Windows.
                MessageBox.Show("Email Error: " + ex.Message);
            return false;
        }
    }
}

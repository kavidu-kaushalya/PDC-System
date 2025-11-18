using MimeKit;
using MailKit.Net.Smtp;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace PDC_System
{
    public class MailService
    {
        private const string senderEmail = "pdc.system.app@gmail.com";
        private const string senderAppPassword = "xfqdchnfsrrbyvyi";

        public async Task<bool> SendEmailAsync(string toEmail, List<string> ccEmails, string subject, string body)
        {
            try
            {
                var message = new MimeMessage();
                message.From.Add(new MailboxAddress("PDC System", senderEmail));
                message.To.Add(MailboxAddress.Parse(toEmail));

                // Add CC recipients if any
                if (ccEmails != null)
                {
                    foreach (var cc in ccEmails)
                    {
                        if (!string.IsNullOrWhiteSpace(cc))
                            message.Cc.Add(MailboxAddress.Parse(cc));
                    }
                }

                message.Subject = subject;

                // 🟢 Change "plain" → "html" for styled emails
                message.Body = new TextPart("html") { Text = body };

                using var client = new SmtpClient();
                await client.ConnectAsync("smtp.gmail.com", 587, MailKit.Security.SecureSocketOptions.StartTls);
                await client.AuthenticateAsync(senderEmail, senderAppPassword);
                await client.SendAsync(message);
                await client.DisconnectAsync(true);

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return false;
            }
        }
    }
}

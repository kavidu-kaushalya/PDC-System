using MimeKit;
using MailKit.Net.Smtp;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace PDC_System
{
    public class MailServiceFinance
    {
        private readonly string senderEmail = Properties.Settings.Default.UserEmail;
        private readonly string senderAppPassword = Properties.Settings.Default.SenderPassword;

        public async Task<bool> SendEmailWithAttachmentAsync(
     string toEmail,
     List<string> ccEmails,
     string subject,
     string body,
     string attachmentPath)
        {
            try
            {
                var message = new MimeMessage();
                message.From.Add(new MailboxAddress("PDC System", senderEmail));
                message.To.Add(MailboxAddress.Parse(toEmail));

                if (ccEmails != null)
                {
                    foreach (var cc in ccEmails)
                        if (!string.IsNullOrWhiteSpace(cc))
                            message.Cc.Add(MailboxAddress.Parse(cc));
                }

                message.Subject = subject;

                var builder = new BodyBuilder
                {
                    HtmlBody = body
                };

                if (File.Exists(attachmentPath))
                    builder.Attachments.Add(attachmentPath);

                message.Body = builder.ToMessageBody();

                using var client = new SmtpClient();
                await client.ConnectAsync("smtp.gmail.com", 587, MailKit.Security.SecureSocketOptions.StartTls);
                await client.AuthenticateAsync(senderEmail, senderAppPassword);
                await client.SendAsync(message);
                await client.DisconnectAsync(true);

                return true;
            }
            catch
            {
                return false;
            }
        }

    }
}

using MimeKit;
using MailKit.Net.Smtp;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace PDC_System
{
    public class MailServicePaysheet
    {
        private const string senderEmail = "pdc.system.app@gmail.com";
        private const string senderAppPassword = "xfqdchnfsrrbyvyi";

        public async Task<bool> SendEmailAsync(string toEmail, List<string> ccEmails, string subject, string body, string attachmentPath = null)
        {
            try
            {
                var message = new MimeMessage();
                message.From.Add(new MailboxAddress("PDC System", senderEmail));
                message.To.Add(MailboxAddress.Parse(toEmail));

                // CC
                if (ccEmails != null)
                {
                    foreach (var cc in ccEmails)
                    {
                        if (!string.IsNullOrWhiteSpace(cc))
                            message.Cc.Add(MailboxAddress.Parse(cc));
                    }
                }

                message.Subject = subject;

                // --- BODY & ATTACHMENT ---
                var builder = new BodyBuilder();
                builder.HtmlBody = body;

                if (!string.IsNullOrEmpty(attachmentPath) && File.Exists(attachmentPath))
                {
                    builder.Attachments.Add(attachmentPath);
                }

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

using schedulebackup.Settings;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Mail;
using System.Text;

namespace schedulebackup.Clients
{
    public class SmtpEmailClient
    {
        private readonly AppSettings AppSettings;
        public SmtpEmailClient(IOptions<AppSettings> appSettings)
        {
            AppSettings = appSettings.Value;
        }

        public void SendEmailNotify(string subject, string body)
        {
            try
            {
                using var client = new SmtpClient("smtp.gmail.com", AppSettings.Mail.Port);
                client.Credentials = new NetworkCredential(AppSettings.Mail.Account, AppSettings.Mail.Password);
                client.EnableSsl = true;

                using var message = new MailMessage()
                {
                    From = new MailAddress(AppSettings.Mail.Account),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = false,
                    SubjectEncoding = Encoding.GetEncoding("UTF-8"),
                    BodyEncoding = Encoding.GetEncoding("UTF-8"),
                };

                foreach(var email in AppSettings.Mail.Notify.Split(","))
                {
                    message.To.Add(email);
                }

                client.Send(message);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to send email. Error: {ex.Message}");
            }
        }
    }
}
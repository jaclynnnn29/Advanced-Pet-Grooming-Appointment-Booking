
using System.Net;
using System.Net.Mail;

namespace Advanced_Pet_Grooming.Services
{
    public class EmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendEmailAsync(
            string toEmail,
            string subject,
            string body)
        {
            string? senderEmail =
                _configuration["EmailSettings:SenderEmail"];

            string? appPassword =
                _configuration["EmailSettings:AppPassword"];

            if (string.IsNullOrEmpty(senderEmail) || string.IsNullOrEmpty(appPassword))
            {
                throw new Exception("Email settings are missing.");
            }

            using (SmtpClient client = new SmtpClient("smtp.gmail.com", 587))
            {
                client.EnableSsl = true;
                client.UseDefaultCredentials = false;

                client.Credentials = new NetworkCredential(
                    senderEmail,
                    appPassword
                );

                using (MailMessage message = new MailMessage(
                    senderEmail,
                    toEmail,
                    subject,
                    body))
                {
                    message.IsBodyHtml = true;

                    await client.SendMailAsync(message);
                }
            }
        }
    }
}

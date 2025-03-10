using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace Zoho_timesheet_API.Services
{
    public interface ISesSmtpEmailService
    {
        Task<bool> SendEmailAsync(string fromAddress, string toAddress, string subject, string body);
    }

    public class SesSmtpEmailService : ISesSmtpEmailService
    {
        private readonly IConfiguration _configuration;
        public SesSmtpEmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<bool> SendEmailAsync(string fromAddress, string toAddress, string subject, string body)
        {
            try
            {
                var message = new MimeMessage();
                message.From.Add(new MailboxAddress(null,fromAddress));
                message.To.Add(new MailboxAddress(null,toAddress));
                message.To.Add(new MailboxAddress(null, "rajeevd@quantratech.com"));
                message.Subject = subject;

                var bodyBuilder = new BodyBuilder();
                if (!string.IsNullOrEmpty(body))
                {
                    bodyBuilder.HtmlBody = body;
                }

                message.Body = bodyBuilder.ToMessageBody();

                using (var client = new SmtpClient())
                {
                    await client.ConnectAsync("email-smtp.us-east-1.amazonaws.com", 587, SecureSocketOptions.StartTls);
                    await client.AuthenticateAsync(_configuration["Aws:SmtpUserName"], _configuration["Aws:SmtpPassword"]);
                    await client.SendAsync(message);
                    await client.DisconnectAsync(true);
                }

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending email via SES: {ex.Message}");
                return false;
            }
        }
    }
}
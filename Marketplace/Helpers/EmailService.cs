using MailKit.Net.Smtp;
using MimeKit;

namespace Marketplace.Helpers
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _config;

        public EmailService(IConfiguration config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        public async Task SendEmailAsync(string receiver, string subject,  string content)
        {
            var message = new MimeMessage();
            var fromName = _config["EmailSettings:FromName"];
            var fromEmail = _config["EmailSettings:FromEmail"];

            message.From.Add(new MailboxAddress(fromName, fromEmail));
            message.To.Add(MailboxAddress.Parse(receiver));
            message.Subject = subject;

            var body = new BodyBuilder { HtmlBody = content };
            message.Body = body.ToMessageBody();

            using var client = new SmtpClient();
            await client.ConnectAsync("smtp.gmail.com", int.Parse(_config["EmailSettings:SmtpPort"] ?? "587"), MailKit.Security.SecureSocketOptions.StartTls);
            await client.AuthenticateAsync(_config["EmailSettings:SmtpUsername"], _config["EmailSettings:SmtpPassword"]);

            await client.SendAsync(message);
            await client.DisconnectAsync(true);
        }
    }
}

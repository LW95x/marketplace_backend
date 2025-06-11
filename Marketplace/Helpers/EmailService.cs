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
            message.From.Add(new MailboxAddress("U2U Marketplace", _config["EmailSettings:SmtpUsername"]));
            message.To.Add(MailboxAddress.Parse(receiver));
            message.Subject = subject;

            var body = new BodyBuilder { HtmlBody = content };
            message.Body = body.ToMessageBody();

            using var client = new SmtpClient();
            await client.ConnectAsync(_config["EmailSettings:SmtpServer"], int.Parse(_config["EmailSettings:SmtpPort"] ?? "587"), MailKit.Security.SecureSocketOptions.StartTls);
            await client.AuthenticateAsync(_config["EmailSettings:SmtpUsername"], _config["EmailSettings:SmtpPassword"]);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);
        }
    }
}

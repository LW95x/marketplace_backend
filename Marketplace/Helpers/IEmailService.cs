namespace Marketplace.Helpers
{
    public interface IEmailService
    {
        Task SendEmailAsync(string receiver, string subject, string content);
    }
}

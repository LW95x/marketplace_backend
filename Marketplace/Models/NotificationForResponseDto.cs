namespace Marketplace.Models
{
    public class NotificationForResponseDto
    {
        public Guid Id { get; set; }
        public string Message { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public bool IsRead { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;

    }
}

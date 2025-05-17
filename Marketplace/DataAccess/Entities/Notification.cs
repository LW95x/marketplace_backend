using System.ComponentModel.DataAnnotations.Schema;

namespace Marketplace.DataAccess.Entities
{
    public class Notification
    {
        public Guid Id { get; set; }
        public string Message { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsRead { get; set; }
        [ForeignKey("UserId")]
        public User User { get; set; } = null!;
        public string UserId { get; set; }
        public string Url { get; set; }

        public Notification (string userId, string message, string url)
        {
            UserId = userId;
            Message = message;
            Url = url;
            CreatedAt = DateTime.UtcNow;
            IsRead = false;
        }
    }
}

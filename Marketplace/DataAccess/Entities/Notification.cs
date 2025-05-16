using System.ComponentModel.DataAnnotations.Schema;

namespace Marketplace.DataAccess.Entities
{
    public class Notification
    {
        public int Id { get; set; }
        public string Message { get; set; } = null!;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsRead { get; set; }
        [ForeignKey("UserId")]
        public User User { get; set; } = null!;
        public string UserId { get; set; }

        public Notification (string userId, string message)
        {
            UserId = userId;
            Message = message;
            CreatedAt = DateTime.UtcNow;
            IsRead = false;
        }
    }
}

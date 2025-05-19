using System.ComponentModel.DataAnnotations.Schema;

namespace Marketplace.DataAccess.Entities
{
    public class Message
    {
        public Guid Id { get; set; }
        public string Content { get; set; }
        public DateTime SentTime { get; set; }
        [ForeignKey("SenderId")]
        public User Sender { get; set; } = null!;
        public string SenderId { get; set; }

        [ForeignKey("ReceiverId")]
        public User Receiver { get; set; } = null!;
        public string ReceiverId { get; set; }

        public Message(string senderId, string receiverId, string content)
        {
            SenderId = senderId;
            ReceiverId = receiverId;
            Content = content;
            SentTime = DateTime.UtcNow;
        }
    }
}

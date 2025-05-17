using System.ComponentModel.DataAnnotations;

namespace Marketplace.Models
{
    public class NotificationForCreationDto
    {
        [Required]
        [MaxLength(300)]
        public string Message { get; set; } = string.Empty;
        [Required]
        [MaxLength(300)]
        public string Url { get; set; } = string.Empty;
    }
}

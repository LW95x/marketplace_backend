using System.ComponentModel.DataAnnotations;

namespace Marketplace.Models
{
    public class PasswordResetDto
    {
        [Required]
        [MaxLength(300)]
        public string Email { get; set; } = string.Empty;
        [Required]
        [MaxLength(300)]
        public string ClientAppUrl { get; set; } = string.Empty;
    }
}

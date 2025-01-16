using System.ComponentModel.DataAnnotations;

namespace Marketplace.Models
{
    public class UserForCreationDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;
        [Required]
        [StringLength(20, MinimumLength = 6, ErrorMessage = "Username must contain at least 6 characters.")]
        public string UserName { get; set; } = string.Empty;
    }
}

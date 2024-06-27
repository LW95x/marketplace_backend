using System.ComponentModel.DataAnnotations;

namespace Marketplace.Models
{
    public class UserForUpdateDto
    {
        [MaxLength(255, ErrorMessage = "The profile picture URL must be under 255 characters in length.")]
        [Url]
        public string  ImageUrl { get; set; } = string.Empty;
    }
}

using System.ComponentModel.DataAnnotations;

namespace Marketplace.Models
{
    public class UserForLoginDto
    {
        [Required]
        public string UserName { get; set; } = string.Empty;
        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;
    }
}

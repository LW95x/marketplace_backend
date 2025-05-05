using System.ComponentModel.DataAnnotations;

namespace Marketplace.Models
{
    public class OrderForCreationDto
    {
        [Required]
        [MaxLength(255)]
        public string Address { get; set; } = string.Empty;

        [Required]
        [MaxLength(255)]
        public string StripePaymentId { get; set; } = string.Empty;
    }
}

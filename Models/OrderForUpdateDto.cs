using Marketplace.DataAccess.Entities;
using System.ComponentModel.DataAnnotations;

namespace Marketplace.Models
{
    public class OrderForUpdateDto
    {
        public OrderStatus? Status { get; set; }
        [MaxLength(255)]
        public string Address { get; set; } = string.Empty;
    }
}

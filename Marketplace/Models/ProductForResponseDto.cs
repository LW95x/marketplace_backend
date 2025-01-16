namespace Marketplace.Models
{
    public class ProductForResponseDto
    {
        public Guid ProductId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal? Price { get; set; }
        public int Quantity { get; set; }
        public string Category { get; set; }  = string.Empty;
        public string SellerName { get; set; } = string.Empty;
        public Guid SellerId { get; set; }
        public List<string> ImageUrls { get; set; } = new List<string>();
    }
}

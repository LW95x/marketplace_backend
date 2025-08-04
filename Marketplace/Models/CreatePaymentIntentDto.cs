namespace Marketplace.Models
{
    public class CreatePaymentIntentDto
    {
        public long Amount { get; set; }
        public string Currency { get; set; } = "gbp";
        public string PaymentMethodId { get; set; } = string.Empty;
    }
}

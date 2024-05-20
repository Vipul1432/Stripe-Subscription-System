namespace SubscriptionSystem.Dtos
{
    public class StripePaymentRequestDto
    {
        public string PriceId { get; set; }
        public string CustomerId { get; set; }
    }
}

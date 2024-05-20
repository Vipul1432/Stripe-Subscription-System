namespace SubscriptionSystem.Dtos
{
    public class PaymentDto
    {
        public string CustomerId { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
    }
}

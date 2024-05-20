namespace SubscriptionSystem.Dtos
{
    public class SubscriptionDto
    {
        public string SubscriptionId { get; set; }
        public string SubscriptionStatus { get; set; }
        public DateTime CurrentPeriodStart { get; set; }
        public DateTime CurrentPeriodEnd { get; set; }
    }
}

namespace RentAutomation.Models
{
    public class TenantEBPredictionViewModel
    {
        public int TenantId { get; set; }
        public string TenantName { get; set; }
        public decimal LastKnownEbBill { get; set; }
        public decimal PredictedEbBill { get; set; }
        public DateTime LastBillingDate { get; set; }
        public DateTime NextBillingPeriod { get; set; }
    }

}

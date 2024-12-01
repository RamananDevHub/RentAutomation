public class TenantEBProfitViewModel
{
    public int TenantId { get; set; }
    public string TenantName { get; set; }
    public int UnitsUsed { get; set; }
    public decimal EbBill { get; set; }
    public decimal CalculatedEbCharge { get; set; }
    public decimal Profit { get; set; }
    public DateTime BillingDate { get; set; }
}

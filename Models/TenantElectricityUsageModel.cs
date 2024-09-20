using System;
using System.ComponentModel.DataAnnotations.Schema;

public class TenantElectricityUsageViewModel
{
    public int TenantHouseNo { get; set; }
    public string TenantName { get; set; }
    public int UnitsUsed { get; set; }
    public DateTime BillingPeriod { get; set; } // Add this to represent billing period
}

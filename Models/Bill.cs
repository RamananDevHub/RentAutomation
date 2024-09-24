using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace RentAutomation.Models;

public class Bill
{
    public int Id { get; set; }
    public int TenantId { get; set; }

    public string TenantName { get; set; }


    public DateTime BillingDate { get; set; } // Billing period (Month and Year)
    public DateTime BillGenerationDate { get; set; }
    public int PreviousMonthUnit { get; set; }
    public int CurrentMonthUnit { get; set; }
    public int UnitsUsed { get; set; }

    public int CurrentMotorReading { get; set; }

    public int PreviousMotorReading { get; set; }
    public decimal EbPerUnit { get; set; }
    public decimal EbBill { get; set; }
    public decimal Rent { get; set; }
    public decimal Water { get; set; }
    public decimal TotalBill { get; set; }

    public Tenant Tenant { get; set; } // Relationship with Tenant
}

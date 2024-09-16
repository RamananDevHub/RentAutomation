using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace RentAutomation.Models;

public class ViewBillsViewModel
{
    public int Id { get; set; } // Tenant Id
    public DateTime BillingPeriod { get; set; } // Selected billing period (month and year)
    public List<Bill> Bills { get; set; } // List of bills for the selected period
}

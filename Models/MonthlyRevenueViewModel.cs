using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace RentAutomation.Models;
public class MonthlyRevenueViewModel
{
    public int Year { get; set; }
    public int Month { get; set; }
    public decimal TotalRevenue { get; set; }
}

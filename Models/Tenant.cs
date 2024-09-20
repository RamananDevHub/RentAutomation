using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace RentAutomation.Models;

public class Tenant
{
    public int Id { get; set; }
    public string TenantName { get; set; }
    public string TenantPhone { get; set; }
    public int TenantHouseNo { get; set; }
    public DateTime TenancyStartDate { get; set; }
    public decimal Rent { get; set; }
    public decimal Deposit { get; set; }
    public decimal EbPerUnit { get; set; }
    public decimal Water { get; set; }
    public int PreviousMonthUnit { get; set; }
    public int CurrentMonthUnit { get; set; }

    // Property to specify the billing period (e.g., month and year)
    public DateTime BillingPeriod { get; set; }

    public DateTime RentBillGenerationDate { get; set; }

    // Motor readings (specific to House No. 9)
  
    public bool IsBillGenerated { get; set; } // Track if bill is generated

    public DateTime? BillGenerationDate { get; set; } // Optional: Track the date of bill generation

    public DateTime LastEBCalculationDate { get; set; } // Track the last calculation date



    // Method to calculate the units used
    private int CalculateUnitsUsed()
    {
        // Step 1: Calculate the difference between the current and previous month units
        int calculatedUnitsUsed = CurrentMonthUnit - PreviousMonthUnit;

        // Debug log for checking the calculated value
        Console.WriteLine($"Calculated Units Used: {calculatedUnitsUsed}");

        // Step 2: Assign the calculated value to the unitsUsed variable
        return calculatedUnitsUsed;
    }

    // Property to calculate the actual units based on specific logic
    public int ActualUnits
    {
        get
        {
            int unitsUsed = CalculateUnitsUsed();
            // Debug log for checking the value
            Console.WriteLine($"Units Used: {unitsUsed}");
            return unitsUsed;
        }
    }

    // Calculated properties (do not map to database)
    [NotMapped]
    public int UnitsUsed
    {
        get
        {
            int unitsUsed = CalculateUnitsUsed();
            // Debug log for checking the value
            Console.WriteLine($"Units Used: {unitsUsed}");
            return unitsUsed;
        }
    }

    [NotMapped]
    public decimal EbBill
    {
        get
        {
            return UnitsUsed * EbPerUnit;
        }
    }

    [NotMapped]
    public decimal TotalBill
    {
        get
        {
            return Rent + Water + EbBill;
        }
    }
}

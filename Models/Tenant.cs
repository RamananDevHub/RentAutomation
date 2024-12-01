using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace RentAutomation.Models
{
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
        public int CurrentMotorReading { get; set; }
        public int PreviousMotorReading { get; set; }

        public DateTime BillingPeriod { get; set; }
        
        public DateTime? BillGenerationDate { get; set; }
        

        public bool IsBillGenerated { get; set; } = false;  // Default is false


        // Calculate the units used with negative check
        [NotMapped]
        public int UnitsUsed
        {
            get
            {
                int unitsUsed;

                if (TenantHouseNo == 9)
                {
                    // Special calculation for tenants with house number 9
                    unitsUsed = (CurrentMonthUnit - PreviousMonthUnit) - (CurrentMotorReading - PreviousMotorReading);
                }
                else
                {
                    // Regular calculation for other tenants
                    unitsUsed = CurrentMonthUnit - PreviousMonthUnit;
                }

                // Ensure no negative units used
                return Math.Max(unitsUsed, 0);
            }
        }

        // Calculated EB Bill based on units used
        [NotMapped]
        public decimal EbBill => UnitsUsed * EbPerUnit;

        // Total Bill calculation
        [NotMapped]
        public decimal TotalBill => Rent + Water + EbBill;
    }
}

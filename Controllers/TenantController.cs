using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using RentAutomation.Models;
using System;
using System.Globalization;
using System.Linq;

namespace RentAutomation.Controllers
{
    public class TenantController : Controller
    {
        private readonly ILogger<TenantController> _logger;
        private readonly ApplicationDbContext _context;


        public TenantController(ILogger<TenantController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;

        }

        // 1. Create Tenant Details
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Create(Tenant tenant)
        {
            if (ModelState.IsValid)
            {
                // Set BillingPeriod to the previous month (one month before the current month)
                var currentDate = DateTime.Now;
                tenant.BillingPeriod = new DateTime(currentDate.Year, currentDate.Month, 1).AddMonths(-1);

                _context.TenantTable.Add(tenant); // Entity Framework will handle Id generation
                _context.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(tenant);
        }

        // 2. Calculate EB for all tenants
        [HttpGet]
        public IActionResult CalculateEB(int id)
        {
            var tenant = _context.TenantTable.Find(id);
            if (tenant != null)
            {
                // Navigate based on current and previous month readings
                if (tenant.CurrentMonthUnit == 0 && tenant.PreviousMonthUnit == 0)
                {
                    return RedirectToAction("FirstTimeCalculateEB", new { id = tenant.Id });
                }
                else
                {
                    return RedirectToAction("SubsequentCalculateEB", new { id = tenant.Id });
                }
            }
            return NotFound();
        }

        private int CalculateUnitsUsed(Tenant tenant, int currentMonthUnit, int previousMonthUnit, int currentMotorReading, int previousMotorReading)
        {
            if (tenant.TenantHouseNo == 9)
            {
                return (currentMonthUnit - previousMonthUnit) - (currentMotorReading - previousMotorReading);
            }
            return currentMonthUnit - previousMonthUnit;
        }


        [HttpPost]
        public IActionResult CalculateEB(int id, int currentMonthUnit, int previousMonthUnit, int currentMotorReading, int previousMotorReading)
        {
            var tenant = _context.TenantTable.Find(id);



            if (tenant != null)
            {
                // Handle first-time calculations
                if (tenant.PreviousMonthUnit == 0 && tenant.CurrentMonthUnit == 0)
                {
                    // First-time calculation
                    tenant.PreviousMonthUnit = previousMonthUnit;
                    tenant.CurrentMonthUnit = currentMonthUnit;
                    tenant.PreviousMotorReading= previousMotorReading;
                    tenant.CurrentMotorReading= currentMotorReading;
                }
                else
                {
                    // Update the previous month unit to the current value
                    tenant.PreviousMonthUnit = tenant.CurrentMonthUnit;
                    tenant.CurrentMonthUnit = currentMonthUnit;
                }

                // Now the UnitsUsed property will automatically calculate the value
                int unitsUsed = tenant.UnitsUsed;

                // Update the billing period to the previous month
                tenant.BillingPeriod = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1).AddMonths(-1);

                // Save changes to the database
                _context.SaveChanges();

                // Redirect to GenerateBill view
                return RedirectToAction("GenerateBill", new { id = tenant.Id });
            }
            return NotFound();
        }




        //3.First EB calculation for all tenants

        [HttpGet]
        public IActionResult FirstTimeCalculateEB(int id)
        {
            var tenant = _context.TenantTable.Find(id);
            if (tenant != null)
            {
                // Only display existing data without modifying anything
                ViewBag.UnitsUsed = tenant.UnitsUsed;
                ViewBag.EbBill = tenant.EbBill;
                return View(tenant);
            }
            return NotFound();
        }
        [HttpPost]
        public IActionResult FirstTimeCalculateEB(int id, int CurrentMonthUnit, int PreviousMonthUnit, int CurrentMotorReading, int PreviousMotorReading)
        {
            var tenant = _context.TenantTable.Find(id);
            if (tenant != null)
            {
                if (PreviousMonthUnit > CurrentMonthUnit)
                {
                    ModelState.AddModelError(string.Empty, "Previous month reading must be less than or equal to the current month reading.");
                    return View("FirstTimeCalculateEB", tenant); // Re-render the view with an error message
                }

                // Handle initialization if PreviousMonthUnit is zero
                if (tenant.PreviousMonthUnit == 0)
                {
                    tenant.PreviousMonthUnit = tenant.CurrentMonthUnit;
                }

                // Calculate units used based on house number
                int calculatedUnitsUsed;
                if (tenant.TenantHouseNo == 9)
                {
                    // Special calculation for tenants with house number 9
                    calculatedUnitsUsed = (CurrentMonthUnit - PreviousMonthUnit) - (CurrentMotorReading - PreviousMotorReading);
                }
                else
                {
                    // Regular calculation for other tenants
                    calculatedUnitsUsed = CurrentMonthUnit - PreviousMonthUnit;
                }

                // Calculate EB bill
                var ebBill = calculatedUnitsUsed * tenant.EbPerUnit;

                // Save updates to the database
                _context.SaveChanges();

                // Set values to ViewBag for display
                ViewBag.UnitsUsed = calculatedUnitsUsed;
                ViewBag.EbBill = ebBill;

                // Redirect to GenerateBill view
                return RedirectToAction("GenerateBill", new { id = tenant.Id });
            }
            return NotFound();
        }



        //4.Subsequent EB calculation for all tenants
        [HttpGet]
        public IActionResult SubsequentCalculateEB(int id)
        {
            var tenant = _context.TenantTable.Find(id);
            if (tenant != null)
            {
                // Check if a bill has already been generated for this billing period
                var existingBill = _context.BillTable
                    .FirstOrDefault(b => b.TenantId == id &&
                                         b.BillingDate == tenant.BillingPeriod);

                // Pass the status to the view
                ViewBag.IsBillGenerated = existingBill != null;

                // Only display existing data without modifying anything
                ViewBag.PreviousMonthUnit = tenant.CurrentMonthUnit;
                return View(tenant);
            }
            return NotFound();
        }

        
        [HttpPost]
        public IActionResult SubsequentCalculateEB(int id, int currentMonthUnit, int currentMotorReading, int previousMotorReading)
        {
            var tenant = _context.TenantTable.Find(id);
            if (tenant != null)
            {
                // Update the previous month unit to the current value
                tenant.PreviousMonthUnit = tenant.CurrentMonthUnit;
                tenant.CurrentMonthUnit = currentMonthUnit;

                // Calculate units used based on house number
                int unitsUsed;
                if (tenant.TenantHouseNo == 9)
                {
                    // Special calculation for tenants with house number 9
                    unitsUsed = (currentMonthUnit - tenant.PreviousMonthUnit) - (currentMotorReading - previousMotorReading);
                }
                else
                {
                    // Regular calculation for other tenants
                    unitsUsed = currentMonthUnit - tenant.PreviousMonthUnit;
                }

                // Calculate EB bill
                var ebBill = unitsUsed * tenant.EbPerUnit;

                // Save other necessary changes to the database
                _context.SaveChanges();

                // Set values in ViewBag for display
                ViewBag.UnitsUsed = unitsUsed;
                ViewBag.EbBill = ebBill;

                // Redirect to GenerateBill view
                return RedirectToAction("GenerateBill", new { id = tenant.Id });
            }
            return NotFound();
        }





        [HttpGet]
        public IActionResult GenerateBill(int id)
        {
            var tenant = _context.TenantTable.Find(id);
            if (tenant != null)
            {
                // Check if a bill has already been generated for this billing period
                var existingBill = _context.BillTable
                    .FirstOrDefault(b => b.TenantId == id && b.BillingDate == tenant.BillingPeriod);

                if (existingBill != null)
                {
                    return View("Error"); // Or handle existing bill case more gracefully
                }

                // Calculate units used (ensure to set CurrentMonthUnit and PreviousMonthUnit if needed)
                var unitsUsed = tenant.UnitsUsed; // Ensure this property is calculated correctly based on the latest readings

                // Calculate EB Bill
                var ebBill = unitsUsed * tenant.EbPerUnit;

                // Calculate Total Bill
                var totalBill = tenant.Rent + tenant.Water + ebBill;

                // Create a new bill
                var bill = new Bill
                {
                    TenantId = tenant.Id,
                    TenantName = tenant.TenantName,
                    BillingDate = tenant.BillingPeriod,
                    BillGenerationDate = DateTime.Now,
                    PreviousMonthUnit = tenant.PreviousMonthUnit,
                    CurrentMonthUnit = tenant.CurrentMonthUnit,
                    PreviousMotorReading = tenant.PreviousMotorReading,
                    CurrentMotorReading = tenant.CurrentMotorReading,
                    UnitsUsed = unitsUsed,
                    EbPerUnit = tenant.EbPerUnit,
                    EbBill = ebBill,
                    Rent = tenant.Rent,
                    Water = tenant.Water,
                    TotalBill = totalBill
                };

                _context.BillTable.Add(bill);
                tenant.IsBillGenerated = true; // Mark bill as generated
                tenant.BillGenerationDate = DateTime.Now; // Update bill generation date
                _context.SaveChanges();

                // Store calculations in ViewBag for display in the view
                ViewBag.UnitsUsed = unitsUsed;
                ViewBag.EbBill = ebBill;
                ViewBag.TotalBill = totalBill;

                return View(tenant); // Pass tenant to the view for display
            }
            return NotFound();
        }



        [HttpGet]
        public IActionResult ViewBills(int id, int month = 0, int year = 0)
        {
            // Use current month and year if not provided
            var selectedMonth = month > 0 ? month : DateTime.Now.Month;
            var selectedYear = year > 0 ? year : DateTime.Now.Year;

            var bills = _context.BillTable
                .Where(b => b.TenantId == id &&
                            b.BillingDate.Month == selectedMonth &&
                            b.BillingDate.Year == selectedYear)
                .ToList();

            var viewModel = new ViewBillsViewModel
            {
                Id = id,

                BillingPeriod = new DateTime(selectedYear, selectedMonth, 1),
                Bills = bills
            };

            return View(viewModel);
        }


        //6.View Historical Data
        [HttpGet]
        public IActionResult ViewHistory(int id)
        {
            // Fetch all bills related to the tenant
            var tenantBills = _context.BillTable
                .Where(b => b.TenantId == id)
                .OrderByDescending(b => b.BillingDate)
                .ToList();

            if (tenantBills.Any())
            {
                return View(tenantBills); // Pass the bills to the view
            }
            return NotFound(); // If no bills are found, return NotFound
        }


        //7.Delete
        [HttpGet]
        public IActionResult Delete(int id)
        {
            var tenant = _context.TenantTable.Find(id);
            if (tenant == null)
            {
                return NotFound();
            }
            return View(tenant);
        }


        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            var tenant = _context.TenantTable.Find(id);
            if (tenant != null)
            {
                _context.TenantTable.Remove(tenant);
                _context.SaveChanges();
            }
            return RedirectToAction(nameof(Index));
        }

        //8.Reset EB Readings
        [HttpGet]
        public IActionResult ResetEB(int id)
        {
            var tenant = _context.TenantTable.Find(id);
            if (tenant == null)
            {
                return NotFound();
            }
            return View(tenant);
        }

        [HttpPost]
        public IActionResult ResetEBConfirmed(int id)
        {
            var tenant = _context.TenantTable.Find(id);
            if (tenant != null)
            {
                // Reset the EB readings and units used
                tenant.PreviousMonthUnit = 0;
                tenant.CurrentMonthUnit = 0;
                tenant.CurrentMotorReading = 0;
                tenant.PreviousMotorReading = 0;

                // Remove all bills related to the tenant
                var bills = _context.BillTable.Where(b => b.TenantId == id).ToList();
                _context.BillTable.RemoveRange(bills);

                // Save the changes to the database
                _context.SaveChanges();

                _logger.LogInformation("Reset EB readings for Tenant ID {TenantId}", tenant.Id);

                return RedirectToAction("index", new { id = tenant.Id });
            }
            return NotFound();
        }

        // 9. Edit Tenant Details
        [HttpGet]
        public IActionResult Edit(int id)
        {
            var tenant = _context.TenantTable.Find(id);
            if (tenant != null)
            {
                return View(tenant);
            }
            return NotFound();
        }

        [HttpPost]
        public IActionResult Edit(Tenant tenant)
        {
            if (ModelState.IsValid)
            {
                _context.TenantTable.Update(tenant);
                _context.SaveChanges();
                return RedirectToAction("Details", new { id = tenant.Id });
            }
            return View(tenant);
        }
        [HttpGet]
        public IActionResult Revenue()
        {
            // Dummy data for January to July 2024
            var random = new Random();
            var monthlyRevenue = new List<MonthlyRevenueViewModel>
    {
        new MonthlyRevenueViewModel { Year = 2024, Month = 1, TotalRevenue = random.Next(90000, 100001) },
        new MonthlyRevenueViewModel { Year = 2024, Month = 2, TotalRevenue = random.Next(90000, 100001) },
        new MonthlyRevenueViewModel { Year = 2024, Month = 3, TotalRevenue = random.Next(90000, 100001) },
        new MonthlyRevenueViewModel { Year = 2024, Month = 4, TotalRevenue = random.Next(90000, 100001) },
        new MonthlyRevenueViewModel { Year = 2024, Month = 5, TotalRevenue = random.Next(90000, 100001) },
        new MonthlyRevenueViewModel { Year = 2024, Month = 6, TotalRevenue = random.Next(90000, 100001) },
        new MonthlyRevenueViewModel { Year = 2024, Month = 7, TotalRevenue = random.Next(90000, 100001) }
    };

            return View(monthlyRevenue);
        }

        [HttpGet]
        public IActionResult ElectricityUsage(string billingPeriod)
        {
            if (!string.IsNullOrEmpty(billingPeriod))
            {
                // Parse the billingPeriod string (formatted as "yyyy-MM") into a DateTime object
                DateTime selectedBillingPeriod;
                if (DateTime.TryParseExact(billingPeriod, "yyyy-MM", CultureInfo.InvariantCulture, DateTimeStyles.None, out selectedBillingPeriod))
                {
                    // Filter tenants by the selected billing period
                    var tenants = _context.TenantTable
                        .Where(t => t.BillingPeriod.Year == selectedBillingPeriod.Year &&
                                    t.BillingPeriod.Month == selectedBillingPeriod.Month)
                        .Select(t => new TenantElectricityUsageViewModel
                        {
                            TenantHouseNo = t.TenantHouseNo,
                            TenantName = t.TenantName,
                            UnitsUsed = t.UnitsUsed,
                            BillingPeriod = t.BillingPeriod
                        })
                        .ToList();

                    // Show the "no data" message only if the billing period is selected and no tenants were found
                    if (!tenants.Any())
                    {
                        ViewBag.Message = "No data available for the selected billing period.";
                    }

                    return View(tenants);
                }
                else
                {
                    // If parsing fails, handle the error (optional)
                    ViewBag.Message = "Invalid billing period format. Please select a valid period.";
                    return View(new List<TenantElectricityUsageViewModel>());
                }
            }

            // If no billing period is selected, do not display any message or data
            return View(new List<TenantElectricityUsageViewModel>());
        }





        //10. View all tenants
        [HttpGet]
        public IActionResult Index()
        {
            var tenants = _context.TenantTable
                                  .OrderBy(t => t.TenantHouseNo)
                                  .ToList();
            return View(tenants);
        }

        //11. View tenant details
        [HttpGet]
        public IActionResult Details(int id)
        {
            var tenant = _context.TenantTable.Find(id);
            if (tenant != null)
            {
                return View(tenant);
            }
            return NotFound();
        }
    }
}



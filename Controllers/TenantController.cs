using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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

        [HttpGet]
        public IActionResult LandingPage()
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

        public IActionResult BillAlreadyExists(int id)
        {
            var tenant = _context.TenantTable.Find(id);
            if (tenant == null)
            {
                return NotFound();
            }

            // Pass the tenant model to the view
            return View(tenant);
        }

        // 2. Calculate EB for all tenants
        [HttpGet]
        public IActionResult CalculateEB(int id)
        {
            var tenant = _context.TenantTable.Find(id);
            if (tenant != null)
            {
                // Calculate the billing period (previous month)
                var billingPeriod = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1).AddMonths(-1);

                // Check if a bill already exists for the calculated billing period
                var existingBill = _context.BillTable.FirstOrDefault(b => b.TenantId == tenant.Id && b.BillingDate == billingPeriod);

                if (existingBill != null)
                {
                    // Redirect to a view that indicates a bill already exists
                    return RedirectToAction("BillAlreadyExists", new { id = tenant.Id });
                }

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
                
                    // Calculate the billing period (previous month)
                    var billingPeriod = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1).AddMonths(-1);

                    // Check if a bill already exists for the calculated billing period
                    var existingBill = _context.BillTable.FirstOrDefault(b => b.TenantId == tenant.Id && b.BillingDate == billingPeriod);

                    if (existingBill != null)
                    {
                        // Redirect to a view that indicates a bill already exists
                        return RedirectToAction("BillAlreadyExists", new { id = tenant.Id });
                    }


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
                // Get the current date
                var currentDate = DateTime.Now;

               

                // Only display existing data without modifying anything
                ViewBag.PreviousMonthUnit = tenant.CurrentMonthUnit;
                return View(tenant);
            }
            return NotFound();
        }

        [HttpPost]
        public IActionResult SubsequentCalculateEB(int id, int currentMonthUnit, int currentMotorReading)
        {
            var tenant = _context.TenantTable.Find(id);
            if (tenant != null)
            {
                
                    // If this is not the first calculation, assign the previous month's unit automatically
                    tenant.PreviousMonthUnit = tenant.CurrentMonthUnit; // Assign last month's current unit as this month's previous unit
                    tenant.CurrentMonthUnit = currentMonthUnit; // The user inputs the new current month unit

                    // For tenants with motor reading (TenantHouseNo == 9)
                    tenant.PreviousMotorReading = tenant.CurrentMotorReading; // Last month's motor reading becomes this month's previous motor reading
                    tenant.CurrentMotorReading = currentMotorReading; // User inputs the new motor reading
                

                // Calculate units used based on house number
                int unitsUsed;
                if (tenant.TenantHouseNo == 9)
                {
                    // Special calculation for tenants with house number 9
                    unitsUsed = (currentMonthUnit - tenant.PreviousMonthUnit) - (currentMotorReading - tenant.PreviousMotorReading);
                }
                else
                {
                    // Regular calculation for other tenants
                    unitsUsed = currentMonthUnit - tenant.PreviousMonthUnit;
                }

                // Calculate EB bill
                var ebBill = unitsUsed * tenant.EbPerUnit;

                // Save the updated values in the database
                _context.SaveChanges();

                // Set values in ViewBag for display in the next view
                ViewBag.UnitsUsed = unitsUsed;
                ViewBag.EbBill = ebBill;

               
                _context.SaveChanges();

                // Redirect to GenerateBill view with the updated tenant ID
                return RedirectToAction("GenerateBill", new { id = tenant.Id });
            }

            // If the tenant is not found, return NotFound result
            return NotFound();
        }



        [HttpGet]
        public IActionResult GenerateBill(int id)
        {
            var tenant = _context.TenantTable.Find(id);
            if (tenant != null)
            {
                // Get the current date
                var currentDate = DateTime.Now;

                // Calculate the billing period (previous month)
                var billingPeriod = new DateTime(currentDate.Year, currentDate.Month, 1).AddMonths(-1);

             

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
                    BillingDate = billingPeriod,
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
        // Controller Method
        [HttpGet]
        public async Task<IActionResult> ViewHistory(int id)
        {
            // Fetch all bills related to the tenant asynchronously
            var tenantBills = await _context.BillTable
                .Where(b => b.TenantId == id)
                .OrderByDescending(b => b.BillingDate)
                .ToListAsync();

            // Fetch tenant information
            var tenants = await _context.TenantTable.ToListAsync(); // Assuming you have a Tenants DbSet
            var tenantDictionary = tenants.ToDictionary(t => t.Id); // Creating a dictionary for quick lookup

            if (tenantBills.Any())
            {
                ViewBag.Tenants = tenantDictionary; // Pass tenants to the view
                return View(tenantBills); // Pass the bills to the view
            }

            // If no bills are found, return a user-friendly view
            ViewBag.Message = "No bills found for this tenant.";
            return View("NoBillsFound"); // Create a NoBillsFound view to inform the user
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
                var propertiesToUpdate = new string[] { "TenantName", "TenantPhone", "TenancyStartDate", "Rent", "Deposit", "EbPerUnit", "Water" };
                _context.TenantTable.Attach(tenant);
                foreach (var property in propertiesToUpdate)
                {
                    _context.Entry(tenant).Property(property).IsModified = true;
                }
                _context.SaveChanges();
                return RedirectToAction("Details", new { id = tenant.Id });
            }
            return View(tenant);
        }

        public IActionResult Revenue()
        {
            var allRevenueData = _context.BillTable
                .GroupBy(b => new { b.BillingDate.Year, b.BillingDate.Month })
                .Select(g => new MonthlyRevenueViewModel
                {
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    TotalRevenue = g.Sum(b => b.TotalBill)
                })
                .OrderBy(r => r.Year).ThenBy(r => r.Month)
                .ToList();

            return View(allRevenueData);
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

        // 13. Delete Last Generated Bill
        [HttpGet]
        public IActionResult DeleteLastBill(int id)
        {
            var lastBill = _context.BillTable
                .Where(b => b.TenantId == id)
                .OrderByDescending(b => b.BillingDate)
                .FirstOrDefault();

            if (lastBill == null)
            {
                return NotFound(); // Handle case where no bills exist
            }

            return View(lastBill); // Pass the last bill to the view for confirmation
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteLastBillConfirmed(int id)
        {
            var bill = _context.BillTable.Find(id);
            if (bill != null)
            {
                _context.BillTable.Remove(bill);
                _context.SaveChanges();

                _logger.LogInformation("Deleted the last generated bill with ID {BillId}", id);
                return RedirectToAction("Index"); // or redirect to another appropriate action
            }

            return NotFound(); // Handle case where bill is not found
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



using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using RentAutomation.Models;
using System;
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

        [HttpPost]
        public IActionResult CalculateEB(int id, int currentMonthUnit, int previousMonthUnit)
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
                }
                else
                {
                    // Update the previous month unit to the current value
                    tenant.PreviousMonthUnit = tenant.CurrentMonthUnit;
                    tenant.CurrentMonthUnit = currentMonthUnit;
                }

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
        public IActionResult FirstTimeCalculateEB(int id, int CurrentMonthUnit, int PreviousMonthUnit)
        {
            var tenant = _context.TenantTable.Find(id);
            if (tenant != null)
            {
                // Handle initialization if PreviousMonthUnit is zero
                if (tenant.PreviousMonthUnit == 0)
                {
                    tenant.PreviousMonthUnit = tenant.CurrentMonthUnit;
                }

                // Calculate units used and EB bill
                int calculatedUnitsUsed = tenant.CurrentMonthUnit - tenant.PreviousMonthUnit;
                var ebBill = calculatedUnitsUsed * tenant.EbPerUnit;




                // Optionally save updates to the database
                _context.SaveChanges();

                // Set values to ViewBag for display
                ViewBag.UnitsUsed = calculatedUnitsUsed;
                ViewBag.PreviousMonthUnit = tenant.PreviousMonthUnit;
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
        public IActionResult SubsequentCalculateEB(int id, int currentMonthUnit)
        {
            var tenant = _context.TenantTable.Find(id);
            if (tenant != null)
            {
                // Update the previous month unit to the current value
                tenant.PreviousMonthUnit = tenant.CurrentMonthUnit;
                tenant.CurrentMonthUnit = currentMonthUnit;

                // Calculate units used (without saving it as a property)
                int unitsUsed = tenant.CurrentMonthUnit - tenant.PreviousMonthUnit;

                // Calculate EB bill (without saving it as a property)
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




        // 5.Generate Total Bill
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
                    return View("Error");
                }

                // Calculate units used and bill
                var unitsUsed = tenant.UnitsUsed;
                var ebBill = unitsUsed * tenant.EbPerUnit;
                var totalBill = tenant.Rent + tenant.Water + ebBill;

                // Create a new bill
                var bill = new Bill
                {
                    TenantId = tenant.Id,
                    BillingDate = tenant.BillingPeriod,
                    BillGenerationDate = DateTime.Now, // Set the BillGenerationDate to the current date
                    PreviousMonthUnit = tenant.PreviousMonthUnit,
                    CurrentMonthUnit = tenant.CurrentMonthUnit,
                 

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

                ViewBag.UnitsUsed = unitsUsed;
                ViewBag.EbBill = ebBill;
                ViewBag.TotalBill = totalBill;

                return View(tenant);
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
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
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
                    //Redirect to a view that indicates a bill already exists
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
                    return RedirectToAction("BillAlreadyExists", new { id = tenant.Id });
                }


                // Handle first-time calculations
                if (tenant.PreviousMonthUnit == 0 && tenant.CurrentMonthUnit == 0)
                {
                    // First-time calculation
                    tenant.PreviousMonthUnit = previousMonthUnit;
                    tenant.CurrentMonthUnit = currentMonthUnit;
                    tenant.PreviousMotorReading = previousMotorReading;
                    tenant.CurrentMotorReading = currentMotorReading;
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
                var totalBill = tenant.Rent + tenant.Water + tenant.EbBill;

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
                ViewBag.BillingPeriod = billingPeriod; // Pass billing period to the view

                return View(tenant); // Pass tenant to the view for display

            }
            return NotFound();
        }


        [HttpGet]
        public IActionResult ViewBills(int id = 1, int month = 0, int year = 0, bool download = false)
        {
            // Get the current date
            var now = DateTime.Now;

            // Use previous month and year if not provided
            if (month <= 0 || year <= 0)
            {
                var previousMonthDate = now.AddMonths(-1);
                month = previousMonthDate.Month;
                year = previousMonthDate.Year;
            }

            var bills = _context.BillTable
                .Where(b => b.TenantId == id &&
                            b.BillingDate.Month == month &&
                            b.BillingDate.Year == year)
                .ToList();

            var viewModel = new ViewBillsViewModel
            {
                Id = id,
                BillingDate = new DateTime(year, month, 1),
                Bills = bills
            };
            if (download) // Check if the download query parameter is true
            {
                // Render the view to HTML
                var htmlContent = RenderRazorViewToString("ViewBills", viewModel);

                // Convert the HTML to PDF
                var converter = new SelectPdf.HtmlToPdf();
                var doc = converter.ConvertHtmlString(htmlContent);

                // Get the month name from the DateTime object
                string monthName = new DateTime(year, month, 1).ToString("MMMM");

                // Set the file name with the month name (e.g., "House No_1_January_2024.pdf")
                var fileName = $"House No_{id}_{monthName}_{year}.pdf";

                // Save the PDF and return it as a file
                var pdfBytes = doc.Save();
                return File(pdfBytes, "application/pdf", fileName);
            }

            // Return the view if not downloading
            return View(viewModel);
        }





        private string RenderRazorViewToString(string viewName, object model)
        {
            var controllerContext = new ControllerContext
            {
                HttpContext = HttpContext
            };

            var viewEngine = HttpContext.RequestServices.GetService(typeof(IRazorViewEngine)) as IRazorViewEngine;
            var tempDataProvider = HttpContext.RequestServices.GetService(typeof(ITempDataProvider)) as ITempDataProvider;

            var actionContext = new ActionContext(HttpContext, RouteData, ControllerContext.ActionDescriptor, ModelState);
            var viewResult = viewEngine.FindView(actionContext, viewName, false);

            using (var sw = new StringWriter())
            {
                var viewData = new ViewDataDictionary(new EmptyModelMetadataProvider(), controllerContext.ModelState)
                {
                    Model = model
                };

                var viewContext = new ViewContext(actionContext, viewResult.View, viewData, new TempDataDictionary(HttpContext, tempDataProvider), sw, new HtmlHelperOptions());
                viewResult.View.RenderAsync(viewContext).Wait();
                return sw.ToString();
            }
        }
        [HttpGet]
        public IActionResult SendBill(int id, int month, int year)
        {
            var tenant = _context.TenantTable.FirstOrDefault(t => t.Id == id);
            var bill = _context.BillTable.FirstOrDefault(b => b.TenantId == id && b.BillingDate.Month == month && b.BillingDate.Year == year);

            if (tenant != null && bill != null)
            {

                // Your UPI ID for payment
                var upiId = "8838326419@ptsbi";  
                // Use the TotalBill property for the amount to be paid
                decimal totalAmount = bill.TotalBill; // The total amount from the Bill model

                // Generate the UPI payment link
                var upiLink = $"upi://pay?pa={upiId}&pn=YourBusinessName&am={totalAmount}&cu=INR&tn=Rent%20Payment%20for%20{tenant.TenantName}";
                // Prepare the message to send via WhatsApp
                var message = $" *Rent for {bill.BillingDate:MMMM yyyy}* \n\n" +
                              $" {tenant.TenantName},\n\n" +
                              $" *House Number:* {tenant.TenantHouseNo}\n\n" +
                              $"🔹 *Summary* 🔹\n" +
                              $"  - *Old Reading:* {bill.PreviousMonthUnit} kWh\n" +
                              $"  - *New Reading:* {bill.CurrentMonthUnit} kWh\n" +
                              $"  - *Units Consumed:* {tenant.UnitsUsed} kWh\n" +
                              $"  - *Rate per Unit:* ₹{tenant.EbPerUnit}\n" +
                              $"  - *Rent:* ₹{tenant.Rent}\n" +
                              $"  - *Water Charges:* ₹{tenant.Water}\n" +
                              $"  - *EB:* ₹{bill.EbBill}\n\n" +
                              $" *Total Rent Amount:* ₹{bill.TotalBill}\n\n" +
                              $" *Cash Only*";



                // URL-encode the message
                var encodedMessage = Uri.EscapeDataString(message);

                // Ensure the tenant's phone number includes the country code (91 for India)
                var phoneNumber = "91" + tenant.TenantPhone;

          
                // Generate the WhatsApp URL with the bypass for the "Continue Chat" button
                var whatsappUrl = $"https://wa.me/{phoneNumber}?text={encodedMessage}";


                // Redirect to WhatsApp
                return Redirect(whatsappUrl);
            }

            return NotFound();
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
            if (string.IsNullOrEmpty(billingPeriod))
            {
                // Find the latest billing period from the TenantTable
                var latestBillingRecord = _context.TenantTable
                    .OrderByDescending(t => t.BillingPeriod)
                    .FirstOrDefault();

                if (latestBillingRecord != null)
                {
                    // Use the latest billing period as the default
                    billingPeriod = latestBillingRecord.BillingPeriod.ToString("yyyy-MM");
                }
                else
                {
                    ViewBag.Message = "No billing data available.";
                    return View(new List<TenantElectricityUsageViewModel>());
                }
            }

            // Parse the billingPeriod string (formatted as "yyyy-MM") into a DateTime object
            if (DateTime.TryParseExact(billingPeriod, "yyyy-MM", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime selectedBillingPeriod))
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

                if (!tenants.Any())
                {
                    ViewBag.Message = "No data available for the selected billing period.";
                }

                ViewBag.SelectedBillingPeriod = billingPeriod; // Pass the selected period to the view
                return View(tenants);
            }
            else
            {
                // If parsing fails, handle the error
                ViewBag.Message = "Invalid billing period format. Please select a valid period.";
                return View(new List<TenantElectricityUsageViewModel>());
            }
        }


        // 13. Delete Last Generated Bill
        [HttpGet]
        public IActionResult DeleteLastBill(int id)
        {
            var lastBill = _context.BillTable
                .Where(b => b.TenantId == id)
                .OrderByDescending(b => b.BillGenerationDate)
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
            // Find the last bill for the tenant
            var lastBill = _context.BillTable
                .Where(b => b.TenantId == id)
                .OrderByDescending(b => b.BillGenerationDate)
                .FirstOrDefault();

            if (lastBill != null)
            {
                // Before deleting the bill, revert the tenant's units
                var tenant = _context.TenantTable.Find(lastBill.TenantId);
                if (tenant != null)
                {
                    // Restore previous month and current month units from the last bill
                    //tenant.PreviousMonthUnit = lastBill.PreviousMonthUnit;
                    tenant.CurrentMonthUnit = lastBill.PreviousMonthUnit;
                    //tenant.PreviousMotorReading = lastBill.PreviousMotorReading;
                    tenant.CurrentMotorReading = lastBill.PreviousMotorReading;

                    // Save changes to the tenant table
                    _context.SaveChanges();
                }

                // Now delete the bill
                _context.BillTable.Remove(lastBill);
                _context.SaveChanges();

                _logger.LogInformation("Deleted the last generated bill with ID {BillId}", lastBill.Id);
                return RedirectToAction("Index"); // or redirect to another appropriate action
            }

            return NotFound(); // Handle case where bill is not found
        }

        [HttpGet]
        public IActionResult ReviewEBProfits(string billingPeriod)
        {
            // If no billing period is provided, use the latest month with data
            if (string.IsNullOrEmpty(billingPeriod))
            {
                var latestBill = _context.BillTable
                    .OrderByDescending(b => b.BillingDate)
                    .FirstOrDefault();

                if (latestBill != null)
                {
                    billingPeriod = latestBill.BillingDate.ToString("yyyy-MM");
                }
                else
                {
                    ViewBag.Message = "No billing data available.";
                    return View(new List<TenantEBProfitViewModel>());
                }
            }

            // Parse the billing period
            DateTime selectedBillingPeriod;
            if (!DateTime.TryParseExact(billingPeriod, "yyyy-MM", CultureInfo.InvariantCulture, DateTimeStyles.None, out selectedBillingPeriod))
            {
                ViewBag.Message = "Invalid billing period format.";
                return View(new List<TenantEBProfitViewModel>());
            }

            // Fetch tenants and their bills for the selected billing period
            var bills = _context.BillTable
                .Where(b => b.BillingDate.Year == selectedBillingPeriod.Year &&
                            b.BillingDate.Month == selectedBillingPeriod.Month)
                .OrderBy(t => t.TenantId)  // Ensure sorting by Tenant ID
                .ToList();

            if (!bills.Any())
            {
                ViewBag.Message = "No data available for the selected billing period.";
                return View(new List<TenantEBProfitViewModel>());
            }

            // List to store profits for each tenant
            var profits = new List<TenantEBProfitViewModel>();

            foreach (var bill in bills)
            {
                // Calculate EB charges using the tariff
                decimal ebCharge = CalculateEBCharge(bill.UnitsUsed);

                // Calculate the profit
                var profit = bill.EbBill - ebCharge;

                profits.Add(new TenantEBProfitViewModel
                {
                    TenantId = bill.TenantId,
                    TenantName = bill.TenantName,
                    UnitsUsed = bill.UnitsUsed,
                    EbBill = bill.EbBill,
                    CalculatedEbCharge = ebCharge,
                    Profit = profit,
                    BillingDate = bill.BillingDate
                });
            }

            ViewBag.SelectedBillingPeriod = billingPeriod; // Pass the selected period to the view
            return View(profits);
        }


        // Helper function to calculate EB charge based on the tariff
        private decimal CalculateEBCharge(int unitsUsed)
        {
            decimal charge = 0;

            if (unitsUsed > 1000)
            {
                charge += (unitsUsed - 1000) * 11.55m;
                unitsUsed = 1000;
            }
            if (unitsUsed > 800)
            {
                charge += (unitsUsed - 800) * 10.5m;
                unitsUsed = 800;
            }
            if (unitsUsed > 600)
            {
                charge += (unitsUsed - 600) * 9.45m;
                unitsUsed = 600;
            }
            if (unitsUsed > 500)
            {
                charge += (unitsUsed - 500) * 8.4m;
                unitsUsed = 500;
            }
            if (unitsUsed > 400)
            {
                charge += (unitsUsed - 400) * 6.3m;
                unitsUsed = 400;
            }
            if (unitsUsed > 200)
            {
                charge += (unitsUsed - 200) * 4.7m;
                unitsUsed = 200;
            }
            if (unitsUsed > 100)
            {
                charge += (unitsUsed - 100) * 2.35m;
            }

            return charge;
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

        // GET: Upload and List Files
        [HttpGet]
        public IActionResult UploadAndListFiles()
        {
            var files = _context.FilesTable.ToList(); // Fetch all uploaded files
            return View(files);
        }

        // POST: Upload File
        [HttpPost]
        public async Task<IActionResult> UploadFile(IFormFile file, string description)
        {
            if (file != null && file.Length > 0)
            {
                using (var memoryStream = new MemoryStream())
                {
                    await file.CopyToAsync(memoryStream);
                    var uploadedFile = new UploadedFile
                    {
                        FileName = file.FileName,
                        FileContent = memoryStream.ToArray(),
                        MimeType = file.ContentType, // Ensure MimeType is set
                        FileSize = file.Length,      // Set FileSize (optional, but good practice)
                        UploadDate = DateTime.Now,
                        Description= description
                    };

                    _context.FilesTable.Add(uploadedFile);
                    await _context.SaveChangesAsync();
                }
                return RedirectToAction("UploadAndListFiles"); // Redirect to the same view after upload
            }

            ViewBag.ErrorMessage = "Please select a file to upload.";
            return View();
        }

        // Helper function to get the MIME type from the file extension
        private string GetMimeType(string fileName)
        {
            var extension = Path.GetExtension(fileName).ToLowerInvariant();
            return extension switch
            {
                ".txt" => "text/plain",
                ".pdf" => "application/pdf",
                ".doc" => "application/msword",
                ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                ".xls" => "application/vnd.ms-excel",
                ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                ".png" => "image/png",
                ".jpg" => "image/jpeg",
                ".jpeg" => "image/jpeg",
                ".gif" => "image/gif",
                ".zip" => "application/zip",
                ".rar" => "application/x-rar-compressed",
                ".csv" => "text/csv",
                _ => "application/octet-stream", // Default MIME type for unknown extensions
            };
        }

        // GET: ViewFile
        [HttpGet]
        public IActionResult ViewFile(int id)
        {
            // Fetch the file from the database using the id
            var file = _context.FilesTable.Find(id);

            if (file == null)
            {
                // If the file is not found in the database, return a "not found" response
                return NotFound();
            }

            // Check if the file has a valid name
            if (string.IsNullOrEmpty(file.FileName))
            {
                // If the file name is invalid, return a bad request
                return BadRequest("File name is invalid.");
            }

            // Get the MIME type based on the file extension
            string mimeType = GetMimeType(file.FileName);

            // Set content disposition to inline to force the browser to display the file
            Response.Headers.Add("Content-Disposition", "inline; filename=" + file.FileName);

            // Serve the file content with the appropriate MIME type
            return File(file.FileContent, mimeType);
        }
    




// GET: Download File
[HttpGet]
        public IActionResult DownloadFile(int id)
        {
            var file = _context.FilesTable.Find(id);
            if (file != null)
            {
                return File(file.FileContent, "application/octet-stream", file.FileName); // Use appropriate content type
            }
            return NotFound();
        }

        // GET: Delete File
        [HttpGet]
        public IActionResult DeleteFile(int id)
        {
            var file = _context.FilesTable.Find(id);
            if (file != null)
            {
                _context.FilesTable.Remove(file);
                _context.SaveChanges();
                return RedirectToAction("UploadAndListFiles"); // Redirect to the list view after deletion
            }
            return NotFound();
        }


            // Helper method to get content type based on file extension
            private string GetContentType(string fileName)
        {
            var extension = Path.GetExtension(fileName).ToLowerInvariant();
            switch (extension)
            {
                case ".pdf":
                    return "application/pdf";
                case ".jpg":
                case ".jpeg":
                    return "image/jpeg";
                case ".png":
                    return "image/png";
                case ".gif":
                    return "image/gif";
                // Add more cases as needed
                default:
                    return "application/octet-stream"; // Default for unknown types
            }
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



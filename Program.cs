using Microsoft.EntityFrameworkCore;
using RentAutomation.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Register PdfService for Dependency Injection
builder.Services.AddTransient<IPdfService, PdfService>();

// Configure Entity Framework Core to use a SQL Server database
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("conn")));

// Build the application
var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Tenant/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

// Ensure the default route specifies "Tenant" as the default controller
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Tenant}/{action=LandingPage}/{id?}");

app.Run();

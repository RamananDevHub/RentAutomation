using RentAutomation.Models;
using Microsoft.EntityFrameworkCore;
public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Tenant> TenantTable { get; set; }
    public DbSet<Bill> BillTable { get; set; }

    public DbSet<UploadedFile> FilesTable { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure the table name for the Tenant entity
        modelBuilder.Entity<Tenant>().ToTable("TenantTable");
        modelBuilder.Entity<Bill>().ToTable("BillTable");


    }
}


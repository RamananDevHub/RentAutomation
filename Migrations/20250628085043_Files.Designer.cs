﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace RentAutomation.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20250628085043_Files")]
    partial class Files
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.8")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("RentAutomation.Models.Bill", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<DateTime>("BillGenerationDate")
                        .HasColumnType("datetime2");

                    b.Property<DateTime>("BillingDate")
                        .HasColumnType("datetime2");

                    b.Property<int>("CurrentMonthUnit")
                        .HasColumnType("int");

                    b.Property<int>("CurrentMotorReading")
                        .HasColumnType("int");

                    b.Property<decimal>("EbBill")
                        .HasColumnType("decimal(18,2)");

                    b.Property<decimal>("EbPerUnit")
                        .HasColumnType("decimal(18,2)");

                    b.Property<int>("PreviousMonthUnit")
                        .HasColumnType("int");

                    b.Property<int>("PreviousMotorReading")
                        .HasColumnType("int");

                    b.Property<decimal>("Rent")
                        .HasColumnType("decimal(18,2)");

                    b.Property<int>("TenantId")
                        .HasColumnType("int");

                    b.Property<string>("TenantName")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<decimal>("TotalBill")
                        .HasColumnType("decimal(18,2)");

                    b.Property<int>("UnitsUsed")
                        .HasColumnType("int");

                    b.Property<decimal>("Water")
                        .HasColumnType("decimal(18,2)");

                    b.HasKey("Id");

                    b.HasIndex("TenantId");

                    b.ToTable("BillTable", (string)null);
                });

            modelBuilder.Entity("RentAutomation.Models.Tenant", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<DateTime?>("BillGenerationDate")
                        .HasColumnType("datetime2");

                    b.Property<DateTime>("BillingPeriod")
                        .HasColumnType("datetime2");

                    b.Property<int>("CurrentMonthUnit")
                        .HasColumnType("int");

                    b.Property<int>("CurrentMotorReading")
                        .HasColumnType("int");

                    b.Property<decimal>("Deposit")
                        .HasColumnType("decimal(18,2)");

                    b.Property<decimal>("EbPerUnit")
                        .HasColumnType("decimal(18,2)");

                    b.Property<bool>("IsBillGenerated")
                        .HasColumnType("bit");

                    b.Property<int>("PreviousMonthUnit")
                        .HasColumnType("int");

                    b.Property<int>("PreviousMotorReading")
                        .HasColumnType("int");

                    b.Property<decimal>("Rent")
                        .HasColumnType("decimal(18,2)");

                    b.Property<DateTime>("TenancyStartDate")
                        .HasColumnType("datetime2");

                    b.Property<int>("TenantHouseNo")
                        .HasColumnType("int");

                    b.Property<string>("TenantName")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("TenantPhone")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<decimal>("Water")
                        .HasColumnType("decimal(18,2)");

                    b.HasKey("Id");

                    b.ToTable("TenantTable", (string)null);
                });

            modelBuilder.Entity("RentAutomation.Models.UploadedFile", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("Description")
                        .HasColumnType("nvarchar(max)");

                    b.Property<byte[]>("FileContent")
                        .IsRequired()
                        .HasColumnType("varbinary(max)");

                    b.Property<string>("FileName")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<long>("FileSize")
                        .HasColumnType("bigint");

                    b.Property<string>("MimeType")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("TenantHouseNo")
                        .HasColumnType("int");

                    b.Property<DateTime>("UploadDate")
                        .HasColumnType("datetime2");

                    b.HasKey("Id");

                    b.ToTable("FilesTable");
                });

            modelBuilder.Entity("RentAutomation.Models.Bill", b =>
                {
                    b.HasOne("RentAutomation.Models.Tenant", "Tenant")
                        .WithMany()
                        .HasForeignKey("TenantId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Tenant");
                });
#pragma warning restore 612, 618
        }
    }
}

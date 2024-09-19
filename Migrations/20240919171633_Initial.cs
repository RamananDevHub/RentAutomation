using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RentAutomation.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TenantTable",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenantName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TenantPhone = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TenantHouseNo = table.Column<int>(type: "int", nullable: false),
                    Rent = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Deposit = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    EbPerUnit = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Water = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PreviousMonthUnit = table.Column<int>(type: "int", nullable: false),
                    CurrentMonthUnit = table.Column<int>(type: "int", nullable: false),
                    BillingPeriod = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RentBillGenerationDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsBillGenerated = table.Column<bool>(type: "bit", nullable: false),
                    BillGenerationDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastEBCalculationDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TenantTable", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BillTable",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenantId = table.Column<int>(type: "int", nullable: false),
                    BillingDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    BillGenerationDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PreviousMonthUnit = table.Column<int>(type: "int", nullable: false),
                    CurrentMonthUnit = table.Column<int>(type: "int", nullable: false),
                    UnitsUsed = table.Column<int>(type: "int", nullable: false),
                    EbPerUnit = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    EbBill = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Rent = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Water = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalBill = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BillTable", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BillTable_TenantTable_TenantId",
                        column: x => x.TenantId,
                        principalTable: "TenantTable",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BillTable_TenantId",
                table: "BillTable",
                column: "TenantId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BillTable");

            migrationBuilder.DropTable(
                name: "TenantTable");
        }
    }
}

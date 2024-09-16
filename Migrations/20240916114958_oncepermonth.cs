using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RentAutomation.Migrations
{
    /// <inheritdoc />
    public partial class oncepermonth : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "LastEBCalculationDate",
                table: "TenantTable",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastEBCalculationDate",
                table: "TenantTable");
        }
    }
}

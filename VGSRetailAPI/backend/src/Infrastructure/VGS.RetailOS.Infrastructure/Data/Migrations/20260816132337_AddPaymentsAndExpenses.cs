using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VGS.RetailOS.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddPaymentsAndExpenses : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Expenses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    StoreId = table.Column<Guid>(type: "uuid", nullable: false),
                    Category = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    ExpenseDate = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    PaymentMethod = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Expenses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Payments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    StoreId = table.Column<Guid>(type: "uuid", nullable: false),
                    PaymentType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ReferenceId = table.Column<Guid>(type: "uuid", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    PaymentDate = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    PaymentMethod = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ReferenceNumber = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Notes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Payments", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Expenses_TenantId_StoreId_ExpenseDate",
                table: "Expenses",
                columns: new[] { "TenantId", "StoreId", "ExpenseDate" });

            migrationBuilder.CreateIndex(
                name: "IX_Payments_TenantId_StoreId_ReferenceId",
                table: "Payments",
                columns: new[] { "TenantId", "StoreId", "ReferenceId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Expenses");

            migrationBuilder.DropTable(
                name: "Payments");
        }
    }
}

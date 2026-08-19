using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VGS.RetailOS.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddInventoryFoundation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "InventoryLedger",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    StoreId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductId = table.Column<Guid>(type: "uuid", nullable: false),
                    ChangeQuantity = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    BalanceAfter = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    TransactionType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ReferenceId = table.Column<Guid>(type: "uuid", nullable: false),
                    Reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InventoryLedger", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "StockBalances",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    StoreId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductId = table.Column<Guid>(type: "uuid", nullable: false),
                    Quantity = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    LastUpdated = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StockBalances", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_InventoryLedger_TenantId_ReferenceId",
                table: "InventoryLedger",
                columns: new[] { "TenantId", "ReferenceId" });

            migrationBuilder.CreateIndex(
                name: "IX_InventoryLedger_TenantId_StoreId_ProductId",
                table: "InventoryLedger",
                columns: new[] { "TenantId", "StoreId", "ProductId" });

            migrationBuilder.CreateIndex(
                name: "IX_StockBalances_TenantId_StoreId_ProductId",
                table: "StockBalances",
                columns: new[] { "TenantId", "StoreId", "ProductId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "InventoryLedger");

            migrationBuilder.DropTable(
                name: "StockBalances");
        }
    }
}

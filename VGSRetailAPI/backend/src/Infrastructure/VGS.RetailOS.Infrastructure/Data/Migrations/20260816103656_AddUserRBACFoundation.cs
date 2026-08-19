using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VGS.RetailOS.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddUserRBACFoundation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string[]>(
                name: "Permissions",
                table: "roles",
                type: "text[]",
                nullable: false,
                defaultValue: new string[0]);

            migrationBuilder.AddColumn<string>(
                name: "TenantId",
                table: "roles",
                type: "text",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "TenantUserMemberships",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    JoinedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TenantUserMemberships", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TenantUserMemberships_users_UserId",
                        column: x => x.UserId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TenantUserMemberships_UserId_TenantId",
                table: "TenantUserMemberships",
                columns: new[] { "UserId", "TenantId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TenantUserMemberships");

            migrationBuilder.DropColumn(
                name: "Permissions",
                table: "roles");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "roles");
        }
    }
}

using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SaaSPlatform.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class seedsetup : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TenantUsages");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TenantUsageLedger",
                table: "TenantUsageLedger");

            migrationBuilder.DropPrimaryKey(
                name: "PK_RefreshToken",
                table: "RefreshToken");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Payment",
                table: "Payment");

            migrationBuilder.RenameTable(
                name: "TenantUsageLedger",
                newName: "TenantUsageLedgers");

            migrationBuilder.RenameTable(
                name: "RefreshToken",
                newName: "RefreshTokens");

            migrationBuilder.RenameTable(
                name: "Payment",
                newName: "Payments");

            migrationBuilder.RenameIndex(
                name: "IX_TenantUsageLedger_TenantId_UsageMonthYear",
                table: "TenantUsageLedgers",
                newName: "IX_TenantUsageLedgers_TenantId_UsageMonthYear");

            migrationBuilder.RenameIndex(
                name: "IX_RefreshToken_UserId_IsRevoked",
                table: "RefreshTokens",
                newName: "IX_RefreshTokens_UserId_IsRevoked");

            migrationBuilder.RenameIndex(
                name: "IX_RefreshToken_Token",
                table: "RefreshTokens",
                newName: "IX_RefreshTokens_Token");

            migrationBuilder.RenameIndex(
                name: "IX_Payment_TransactionId",
                table: "Payments",
                newName: "IX_Payments_TransactionId");

            migrationBuilder.RenameIndex(
                name: "IX_Payment_TenantId_Status",
                table: "Payments",
                newName: "IX_Payments_TenantId_Status");

            migrationBuilder.AddPrimaryKey(
                name: "PK_TenantUsageLedgers",
                table: "TenantUsageLedgers",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_RefreshTokens",
                table: "RefreshTokens",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Payments",
                table: "Payments",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "Stores",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Phone = table.Column<string>(type: "text", nullable: true),
                    Address = table.Column<string>(type: "text", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    Slug = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Stores", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RestaurantTables",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    StoreId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Capacity = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    QRCode = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RestaurantTables", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RestaurantTables_Stores_StoreId",
                        column: x => x.StoreId,
                        principalTable: "Stores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RestaurantTables_StoreId",
                table: "RestaurantTables",
                column: "StoreId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RestaurantTables");

            migrationBuilder.DropTable(
                name: "Stores");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TenantUsageLedgers",
                table: "TenantUsageLedgers");

            migrationBuilder.DropPrimaryKey(
                name: "PK_RefreshTokens",
                table: "RefreshTokens");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Payments",
                table: "Payments");

            migrationBuilder.RenameTable(
                name: "TenantUsageLedgers",
                newName: "TenantUsageLedger");

            migrationBuilder.RenameTable(
                name: "RefreshTokens",
                newName: "RefreshToken");

            migrationBuilder.RenameTable(
                name: "Payments",
                newName: "Payment");

            migrationBuilder.RenameIndex(
                name: "IX_TenantUsageLedgers_TenantId_UsageMonthYear",
                table: "TenantUsageLedger",
                newName: "IX_TenantUsageLedger_TenantId_UsageMonthYear");

            migrationBuilder.RenameIndex(
                name: "IX_RefreshTokens_UserId_IsRevoked",
                table: "RefreshToken",
                newName: "IX_RefreshToken_UserId_IsRevoked");

            migrationBuilder.RenameIndex(
                name: "IX_RefreshTokens_Token",
                table: "RefreshToken",
                newName: "IX_RefreshToken_Token");

            migrationBuilder.RenameIndex(
                name: "IX_Payments_TransactionId",
                table: "Payment",
                newName: "IX_Payment_TransactionId");

            migrationBuilder.RenameIndex(
                name: "IX_Payments_TenantId_Status",
                table: "Payment",
                newName: "IX_Payment_TenantId_Status");

            migrationBuilder.AddPrimaryKey(
                name: "PK_TenantUsageLedger",
                table: "TenantUsageLedger",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_RefreshToken",
                table: "RefreshToken",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Payment",
                table: "Payment",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "TenantUsages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CategoryCount = table.Column<int>(type: "integer", nullable: false),
                    LastUpdated = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    OrderCountThisMonth = table.Column<int>(type: "integer", nullable: false),
                    ProductCount = table.Column<int>(type: "integer", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TenantUsages", x => x.Id);
                });
        }
    }
}

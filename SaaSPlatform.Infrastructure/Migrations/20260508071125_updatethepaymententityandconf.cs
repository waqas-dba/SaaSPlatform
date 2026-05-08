using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SaaSPlatform.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class updatethepaymententityandconf : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "StoreId",
                table: "Payments",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Payments_StoreId",
                table: "Payments",
                column: "StoreId");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_TenantId_StoreId",
                table: "Payments",
                columns: new[] { "TenantId", "StoreId" });

            migrationBuilder.AddForeignKey(
                name: "FK_Payments_Stores_StoreId",
                table: "Payments",
                column: "StoreId",
                principalTable: "Stores",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Payments_Stores_StoreId",
                table: "Payments");

            migrationBuilder.DropIndex(
                name: "IX_Payments_StoreId",
                table: "Payments");

            migrationBuilder.DropIndex(
                name: "IX_Payments_TenantId_StoreId",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "StoreId",
                table: "Payments");
        }
    }
}

using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SaaSPlatform.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class addonsgroupentrymakeiteasy : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProductAddons");

            migrationBuilder.DropIndex(
                name: "IX_Products_TenantId_CategoryId",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_FeaturedProducts_TenantId_Section_CategoryId",
                table: "FeaturedProducts");

            migrationBuilder.DropIndex(
                name: "IX_FeaturedProducts_TenantId_Section_ProductId",
                table: "FeaturedProducts");

            migrationBuilder.DropIndex(
                name: "IX_Categories_TenantId_ParentCategoryId",
                table: "Categories");

            migrationBuilder.DropIndex(
                name: "IX_Addons_TenantId_Name",
                table: "Addons");

            migrationBuilder.AddColumn<Guid>(
                name: "StoreId",
                table: "Products",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "StoreId",
                table: "FeaturedProducts",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "StoreId",
                table: "Categories",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AlterColumn<decimal>(
                name: "Price",
                table: "Addons",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric");

            migrationBuilder.AddColumn<Guid>(
                name: "StoreId",
                table: "Addons",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateTable(
                name: "AddonGroups",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    StoreId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    IsRequired = table.Column<bool>(type: "boolean", nullable: false),
                    MinSelect = table.Column<int>(type: "integer", nullable: false),
                    MaxSelect = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AddonGroups", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AddonGroupItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AddonGroupId = table.Column<Guid>(type: "uuid", nullable: false),
                    AddonId = table.Column<Guid>(type: "uuid", nullable: false),
                    PriceAdjustment = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    IsDefault = table.Column<bool>(type: "boolean", nullable: false),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AddonGroupItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AddonGroupItems_AddonGroups_AddonGroupId",
                        column: x => x.AddonGroupId,
                        principalTable: "AddonGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AddonGroupItems_Addons_AddonId",
                        column: x => x.AddonId,
                        principalTable: "Addons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ProductAddonGroups",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductId = table.Column<Guid>(type: "uuid", nullable: false),
                    AddonGroupId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsRequired = table.Column<bool>(type: "boolean", nullable: false),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductAddonGroups", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductAddonGroups_AddonGroups_AddonGroupId",
                        column: x => x.AddonGroupId,
                        principalTable: "AddonGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProductAddonGroups_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Products_StoreId_Name",
                table: "Products",
                columns: new[] { "StoreId", "Name" });

            migrationBuilder.CreateIndex(
                name: "IX_Products_TenantId_StoreId_CategoryId",
                table: "Products",
                columns: new[] { "TenantId", "StoreId", "CategoryId" });

            migrationBuilder.CreateIndex(
                name: "IX_FeaturedProducts_StoreId_Section_CategoryId",
                table: "FeaturedProducts",
                columns: new[] { "StoreId", "Section", "CategoryId" });

            migrationBuilder.CreateIndex(
                name: "IX_FeaturedProducts_TenantId_StoreId_Section_ProductId",
                table: "FeaturedProducts",
                columns: new[] { "TenantId", "StoreId", "Section", "ProductId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Categories_StoreId_Name",
                table: "Categories",
                columns: new[] { "StoreId", "Name" });

            migrationBuilder.CreateIndex(
                name: "IX_Categories_TenantId_StoreId_ParentCategoryId",
                table: "Categories",
                columns: new[] { "TenantId", "StoreId", "ParentCategoryId" });

            migrationBuilder.CreateIndex(
                name: "IX_Addons_StoreId",
                table: "Addons",
                column: "StoreId");

            migrationBuilder.CreateIndex(
                name: "IX_Addons_TenantId_StoreId_Name",
                table: "Addons",
                columns: new[] { "TenantId", "StoreId", "Name" });

            migrationBuilder.CreateIndex(
                name: "IX_AddonGroupItems_AddonGroupId_AddonId",
                table: "AddonGroupItems",
                columns: new[] { "AddonGroupId", "AddonId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AddonGroupItems_AddonId",
                table: "AddonGroupItems",
                column: "AddonId");

            migrationBuilder.CreateIndex(
                name: "IX_AddonGroups_TenantId_StoreId_Name",
                table: "AddonGroups",
                columns: new[] { "TenantId", "StoreId", "Name" });

            migrationBuilder.CreateIndex(
                name: "IX_ProductAddonGroups_AddonGroupId",
                table: "ProductAddonGroups",
                column: "AddonGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductAddonGroups_ProductId_AddonGroupId",
                table: "ProductAddonGroups",
                columns: new[] { "ProductId", "AddonGroupId" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Addons_Stores_StoreId",
                table: "Addons",
                column: "StoreId",
                principalTable: "Stores",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Categories_Stores_StoreId",
                table: "Categories",
                column: "StoreId",
                principalTable: "Stores",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_FeaturedProducts_Stores_StoreId",
                table: "FeaturedProducts",
                column: "StoreId",
                principalTable: "Stores",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Products_Stores_StoreId",
                table: "Products",
                column: "StoreId",
                principalTable: "Stores",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ProductVariants_Products_ProductId",
                table: "ProductVariants",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Addons_Stores_StoreId",
                table: "Addons");

            migrationBuilder.DropForeignKey(
                name: "FK_Categories_Stores_StoreId",
                table: "Categories");

            migrationBuilder.DropForeignKey(
                name: "FK_FeaturedProducts_Stores_StoreId",
                table: "FeaturedProducts");

            migrationBuilder.DropForeignKey(
                name: "FK_Products_Stores_StoreId",
                table: "Products");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductVariants_Products_ProductId",
                table: "ProductVariants");

            migrationBuilder.DropTable(
                name: "AddonGroupItems");

            migrationBuilder.DropTable(
                name: "ProductAddonGroups");

            migrationBuilder.DropTable(
                name: "AddonGroups");

            migrationBuilder.DropIndex(
                name: "IX_Products_StoreId_Name",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_Products_TenantId_StoreId_CategoryId",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_FeaturedProducts_StoreId_Section_CategoryId",
                table: "FeaturedProducts");

            migrationBuilder.DropIndex(
                name: "IX_FeaturedProducts_TenantId_StoreId_Section_ProductId",
                table: "FeaturedProducts");

            migrationBuilder.DropIndex(
                name: "IX_Categories_StoreId_Name",
                table: "Categories");

            migrationBuilder.DropIndex(
                name: "IX_Categories_TenantId_StoreId_ParentCategoryId",
                table: "Categories");

            migrationBuilder.DropIndex(
                name: "IX_Addons_StoreId",
                table: "Addons");

            migrationBuilder.DropIndex(
                name: "IX_Addons_TenantId_StoreId_Name",
                table: "Addons");

            migrationBuilder.DropColumn(
                name: "StoreId",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "StoreId",
                table: "FeaturedProducts");

            migrationBuilder.DropColumn(
                name: "StoreId",
                table: "Categories");

            migrationBuilder.DropColumn(
                name: "StoreId",
                table: "Addons");

            migrationBuilder.AlterColumn<decimal>(
                name: "Price",
                table: "Addons",
                type: "numeric",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(18,2)",
                oldPrecision: 18,
                oldScale: 2);

            migrationBuilder.CreateTable(
                name: "ProductAddons",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AddonId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductAddons", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Products_TenantId_CategoryId",
                table: "Products",
                columns: new[] { "TenantId", "CategoryId" });

            migrationBuilder.CreateIndex(
                name: "IX_FeaturedProducts_TenantId_Section_CategoryId",
                table: "FeaturedProducts",
                columns: new[] { "TenantId", "Section", "CategoryId" });

            migrationBuilder.CreateIndex(
                name: "IX_FeaturedProducts_TenantId_Section_ProductId",
                table: "FeaturedProducts",
                columns: new[] { "TenantId", "Section", "ProductId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Categories_TenantId_ParentCategoryId",
                table: "Categories",
                columns: new[] { "TenantId", "ParentCategoryId" });

            migrationBuilder.CreateIndex(
                name: "IX_Addons_TenantId_Name",
                table: "Addons",
                columns: new[] { "TenantId", "Name" });

            migrationBuilder.CreateIndex(
                name: "IX_ProductAddons_ProductId_AddonId",
                table: "ProductAddons",
                columns: new[] { "ProductId", "AddonId" });
        }
    }
}

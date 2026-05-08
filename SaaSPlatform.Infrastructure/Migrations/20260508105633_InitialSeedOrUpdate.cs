using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SaaSPlatform.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialSeedOrUpdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Categories_Stores_StoreId",
                table: "Categories");

            migrationBuilder.DropForeignKey(
                name: "FK_FeaturedProducts_Products_ProductId",
                table: "FeaturedProducts");

            migrationBuilder.DropForeignKey(
                name: "FK_FeaturedProducts_Stores_StoreId",
                table: "FeaturedProducts");

            migrationBuilder.DropForeignKey(
                name: "FK_OrderItemAddons_OrderItems_OrderItemId",
                table: "OrderItemAddons");

            migrationBuilder.DropForeignKey(
                name: "FK_OrderItems_Orders_OrderId",
                table: "OrderItems");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductAddonGroups_AddonGroups_AddonGroupId",
                table: "ProductAddonGroups");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductAddonGroups_Products_ProductId",
                table: "ProductAddonGroups");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductImages_Products_ProductId",
                table: "ProductImages");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductVariants_Products_ProductId",
                table: "ProductVariants");

            migrationBuilder.DropForeignKey(
                name: "FK_TenantDomains_Tenants_TenantId",
                table: "TenantDomains");

            migrationBuilder.DropTable(
                name: "AgentCommissions");

            migrationBuilder.DropTable(
                name: "Agents");

            migrationBuilder.DropTable(
                name: "RestaurantTables");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TenantUsageLedgers",
                table: "TenantUsageLedgers");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TenantDomains",
                table: "TenantDomains");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ProductVariants",
                table: "ProductVariants");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ProductImages",
                table: "ProductImages");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ProductAddonGroups",
                table: "ProductAddonGroups");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Orders",
                table: "Orders");

            migrationBuilder.DropPrimaryKey(
                name: "PK_OrderItems",
                table: "OrderItems");

            migrationBuilder.DropPrimaryKey(
                name: "PK_OrderItemAddons",
                table: "OrderItemAddons");

            migrationBuilder.DropPrimaryKey(
                name: "PK_FeaturedProducts",
                table: "FeaturedProducts");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Categories",
                table: "Categories");

            migrationBuilder.RenameTable(
                name: "TenantUsageLedgers",
                newName: "TenantUsageLedger");

            migrationBuilder.RenameTable(
                name: "TenantDomains",
                newName: "TenantDomain");

            migrationBuilder.RenameTable(
                name: "ProductVariants",
                newName: "ProductVariant");

            migrationBuilder.RenameTable(
                name: "ProductImages",
                newName: "ProductImage");

            migrationBuilder.RenameTable(
                name: "ProductAddonGroups",
                newName: "ProductAddonGroup");

            migrationBuilder.RenameTable(
                name: "Orders",
                newName: "Order");

            migrationBuilder.RenameTable(
                name: "OrderItems",
                newName: "OrderItem");

            migrationBuilder.RenameTable(
                name: "OrderItemAddons",
                newName: "OrderItemAddon");

            migrationBuilder.RenameTable(
                name: "FeaturedProducts",
                newName: "FeaturedProduct");

            migrationBuilder.RenameTable(
                name: "Categories",
                newName: "Category");

            migrationBuilder.RenameIndex(
                name: "IX_TenantUsageLedgers_TenantId_UsageMonthYear",
                table: "TenantUsageLedger",
                newName: "IX_TenantUsageLedger_TenantId_UsageMonthYear");

            migrationBuilder.RenameIndex(
                name: "IX_TenantDomains_TenantId",
                table: "TenantDomain",
                newName: "IX_TenantDomain_TenantId");

            migrationBuilder.RenameIndex(
                name: "IX_ProductVariants_ProductId",
                table: "ProductVariant",
                newName: "IX_ProductVariant_ProductId");

            migrationBuilder.RenameIndex(
                name: "IX_ProductImages_ProductId_DisplayOrder",
                table: "ProductImage",
                newName: "IX_ProductImage_ProductId_DisplayOrder");

            migrationBuilder.RenameIndex(
                name: "IX_ProductAddonGroups_ProductId_AddonGroupId",
                table: "ProductAddonGroup",
                newName: "IX_ProductAddonGroup_ProductId_AddonGroupId");

            migrationBuilder.RenameIndex(
                name: "IX_ProductAddonGroups_AddonGroupId",
                table: "ProductAddonGroup",
                newName: "IX_ProductAddonGroup_AddonGroupId");

            migrationBuilder.RenameIndex(
                name: "IX_Orders_TenantId_Status",
                table: "Order",
                newName: "IX_Order_TenantId_Status");

            migrationBuilder.RenameIndex(
                name: "IX_Orders_TenantId_OrderNumber",
                table: "Order",
                newName: "IX_Order_TenantId_OrderNumber");

            migrationBuilder.RenameIndex(
                name: "IX_Orders_TenantId_CreatedAt",
                table: "Order",
                newName: "IX_Order_TenantId_CreatedAt");

            migrationBuilder.RenameIndex(
                name: "IX_Orders_StoreId",
                table: "Order",
                newName: "IX_Order_StoreId");

            migrationBuilder.RenameIndex(
                name: "IX_OrderItems_OrderId",
                table: "OrderItem",
                newName: "IX_OrderItem_OrderId");

            migrationBuilder.RenameIndex(
                name: "IX_OrderItemAddons_OrderItemId",
                table: "OrderItemAddon",
                newName: "IX_OrderItemAddon_OrderItemId");

            migrationBuilder.RenameIndex(
                name: "IX_FeaturedProducts_TenantId_StoreId_Section_ProductId",
                table: "FeaturedProduct",
                newName: "IX_FeaturedProduct_TenantId_StoreId_Section_ProductId");

            migrationBuilder.RenameIndex(
                name: "IX_FeaturedProducts_StoreId_Section_CategoryId",
                table: "FeaturedProduct",
                newName: "IX_FeaturedProduct_StoreId_Section_CategoryId");

            migrationBuilder.RenameIndex(
                name: "IX_FeaturedProducts_ProductId",
                table: "FeaturedProduct",
                newName: "IX_FeaturedProduct_ProductId");

            migrationBuilder.RenameIndex(
                name: "IX_Categories_TenantId_StoreId_ParentCategoryId",
                table: "Category",
                newName: "IX_Category_TenantId_StoreId_ParentCategoryId");

            migrationBuilder.RenameIndex(
                name: "IX_Categories_StoreId_Name",
                table: "Category",
                newName: "IX_Category_StoreId_Name");

            migrationBuilder.AddPrimaryKey(
                name: "PK_TenantUsageLedger",
                table: "TenantUsageLedger",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_TenantDomain",
                table: "TenantDomain",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ProductVariant",
                table: "ProductVariant",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ProductImage",
                table: "ProductImage",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ProductAddonGroup",
                table: "ProductAddonGroup",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Order",
                table: "Order",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_OrderItem",
                table: "OrderItem",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_OrderItemAddon",
                table: "OrderItemAddon",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_FeaturedProduct",
                table: "FeaturedProduct",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Category",
                table: "Category",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Category_Stores_StoreId",
                table: "Category",
                column: "StoreId",
                principalTable: "Stores",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_FeaturedProduct_Products_ProductId",
                table: "FeaturedProduct",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_FeaturedProduct_Stores_StoreId",
                table: "FeaturedProduct",
                column: "StoreId",
                principalTable: "Stores",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_OrderItem_Order_OrderId",
                table: "OrderItem",
                column: "OrderId",
                principalTable: "Order",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_OrderItemAddon_OrderItem_OrderItemId",
                table: "OrderItemAddon",
                column: "OrderItemId",
                principalTable: "OrderItem",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ProductAddonGroup_AddonGroups_AddonGroupId",
                table: "ProductAddonGroup",
                column: "AddonGroupId",
                principalTable: "AddonGroups",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ProductAddonGroup_Products_ProductId",
                table: "ProductAddonGroup",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ProductImage_Products_ProductId",
                table: "ProductImage",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ProductVariant_Products_ProductId",
                table: "ProductVariant",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TenantDomain_Tenants_TenantId",
                table: "TenantDomain",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Category_Stores_StoreId",
                table: "Category");

            migrationBuilder.DropForeignKey(
                name: "FK_FeaturedProduct_Products_ProductId",
                table: "FeaturedProduct");

            migrationBuilder.DropForeignKey(
                name: "FK_FeaturedProduct_Stores_StoreId",
                table: "FeaturedProduct");

            migrationBuilder.DropForeignKey(
                name: "FK_OrderItem_Order_OrderId",
                table: "OrderItem");

            migrationBuilder.DropForeignKey(
                name: "FK_OrderItemAddon_OrderItem_OrderItemId",
                table: "OrderItemAddon");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductAddonGroup_AddonGroups_AddonGroupId",
                table: "ProductAddonGroup");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductAddonGroup_Products_ProductId",
                table: "ProductAddonGroup");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductImage_Products_ProductId",
                table: "ProductImage");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductVariant_Products_ProductId",
                table: "ProductVariant");

            migrationBuilder.DropForeignKey(
                name: "FK_TenantDomain_Tenants_TenantId",
                table: "TenantDomain");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TenantUsageLedger",
                table: "TenantUsageLedger");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TenantDomain",
                table: "TenantDomain");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ProductVariant",
                table: "ProductVariant");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ProductImage",
                table: "ProductImage");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ProductAddonGroup",
                table: "ProductAddonGroup");

            migrationBuilder.DropPrimaryKey(
                name: "PK_OrderItemAddon",
                table: "OrderItemAddon");

            migrationBuilder.DropPrimaryKey(
                name: "PK_OrderItem",
                table: "OrderItem");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Order",
                table: "Order");

            migrationBuilder.DropPrimaryKey(
                name: "PK_FeaturedProduct",
                table: "FeaturedProduct");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Category",
                table: "Category");

            migrationBuilder.RenameTable(
                name: "TenantUsageLedger",
                newName: "TenantUsageLedgers");

            migrationBuilder.RenameTable(
                name: "TenantDomain",
                newName: "TenantDomains");

            migrationBuilder.RenameTable(
                name: "ProductVariant",
                newName: "ProductVariants");

            migrationBuilder.RenameTable(
                name: "ProductImage",
                newName: "ProductImages");

            migrationBuilder.RenameTable(
                name: "ProductAddonGroup",
                newName: "ProductAddonGroups");

            migrationBuilder.RenameTable(
                name: "OrderItemAddon",
                newName: "OrderItemAddons");

            migrationBuilder.RenameTable(
                name: "OrderItem",
                newName: "OrderItems");

            migrationBuilder.RenameTable(
                name: "Order",
                newName: "Orders");

            migrationBuilder.RenameTable(
                name: "FeaturedProduct",
                newName: "FeaturedProducts");

            migrationBuilder.RenameTable(
                name: "Category",
                newName: "Categories");

            migrationBuilder.RenameIndex(
                name: "IX_TenantUsageLedger_TenantId_UsageMonthYear",
                table: "TenantUsageLedgers",
                newName: "IX_TenantUsageLedgers_TenantId_UsageMonthYear");

            migrationBuilder.RenameIndex(
                name: "IX_TenantDomain_TenantId",
                table: "TenantDomains",
                newName: "IX_TenantDomains_TenantId");

            migrationBuilder.RenameIndex(
                name: "IX_ProductVariant_ProductId",
                table: "ProductVariants",
                newName: "IX_ProductVariants_ProductId");

            migrationBuilder.RenameIndex(
                name: "IX_ProductImage_ProductId_DisplayOrder",
                table: "ProductImages",
                newName: "IX_ProductImages_ProductId_DisplayOrder");

            migrationBuilder.RenameIndex(
                name: "IX_ProductAddonGroup_ProductId_AddonGroupId",
                table: "ProductAddonGroups",
                newName: "IX_ProductAddonGroups_ProductId_AddonGroupId");

            migrationBuilder.RenameIndex(
                name: "IX_ProductAddonGroup_AddonGroupId",
                table: "ProductAddonGroups",
                newName: "IX_ProductAddonGroups_AddonGroupId");

            migrationBuilder.RenameIndex(
                name: "IX_OrderItemAddon_OrderItemId",
                table: "OrderItemAddons",
                newName: "IX_OrderItemAddons_OrderItemId");

            migrationBuilder.RenameIndex(
                name: "IX_OrderItem_OrderId",
                table: "OrderItems",
                newName: "IX_OrderItems_OrderId");

            migrationBuilder.RenameIndex(
                name: "IX_Order_TenantId_Status",
                table: "Orders",
                newName: "IX_Orders_TenantId_Status");

            migrationBuilder.RenameIndex(
                name: "IX_Order_TenantId_OrderNumber",
                table: "Orders",
                newName: "IX_Orders_TenantId_OrderNumber");

            migrationBuilder.RenameIndex(
                name: "IX_Order_TenantId_CreatedAt",
                table: "Orders",
                newName: "IX_Orders_TenantId_CreatedAt");

            migrationBuilder.RenameIndex(
                name: "IX_Order_StoreId",
                table: "Orders",
                newName: "IX_Orders_StoreId");

            migrationBuilder.RenameIndex(
                name: "IX_FeaturedProduct_TenantId_StoreId_Section_ProductId",
                table: "FeaturedProducts",
                newName: "IX_FeaturedProducts_TenantId_StoreId_Section_ProductId");

            migrationBuilder.RenameIndex(
                name: "IX_FeaturedProduct_StoreId_Section_CategoryId",
                table: "FeaturedProducts",
                newName: "IX_FeaturedProducts_StoreId_Section_CategoryId");

            migrationBuilder.RenameIndex(
                name: "IX_FeaturedProduct_ProductId",
                table: "FeaturedProducts",
                newName: "IX_FeaturedProducts_ProductId");

            migrationBuilder.RenameIndex(
                name: "IX_Category_TenantId_StoreId_ParentCategoryId",
                table: "Categories",
                newName: "IX_Categories_TenantId_StoreId_ParentCategoryId");

            migrationBuilder.RenameIndex(
                name: "IX_Category_StoreId_Name",
                table: "Categories",
                newName: "IX_Categories_StoreId_Name");

            migrationBuilder.AddPrimaryKey(
                name: "PK_TenantUsageLedgers",
                table: "TenantUsageLedgers",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_TenantDomains",
                table: "TenantDomains",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ProductVariants",
                table: "ProductVariants",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ProductImages",
                table: "ProductImages",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ProductAddonGroups",
                table: "ProductAddonGroups",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_OrderItemAddons",
                table: "OrderItemAddons",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_OrderItems",
                table: "OrderItems",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Orders",
                table: "Orders",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_FeaturedProducts",
                table: "FeaturedProducts",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Categories",
                table: "Categories",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "AgentCommissions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AgentId = table.Column<Guid>(type: "uuid", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    IsPaid = table.Column<bool>(type: "boolean", nullable: false),
                    PaidAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    Type = table.Column<string>(type: "text", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AgentCommissions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Agents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CommissionRate = table.Column<decimal>(type: "numeric", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    Email = table.Column<string>(type: "text", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Phone = table.Column<string>(type: "text", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Agents", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RestaurantTables",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Capacity = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    QRCode = table.Column<string>(type: "text", nullable: true),
                    StoreId = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RestaurantTables", x => x.Id);
                });

            migrationBuilder.AddForeignKey(
                name: "FK_Categories_Stores_StoreId",
                table: "Categories",
                column: "StoreId",
                principalTable: "Stores",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_FeaturedProducts_Products_ProductId",
                table: "FeaturedProducts",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_FeaturedProducts_Stores_StoreId",
                table: "FeaturedProducts",
                column: "StoreId",
                principalTable: "Stores",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_OrderItemAddons_OrderItems_OrderItemId",
                table: "OrderItemAddons",
                column: "OrderItemId",
                principalTable: "OrderItems",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_OrderItems_Orders_OrderId",
                table: "OrderItems",
                column: "OrderId",
                principalTable: "Orders",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ProductAddonGroups_AddonGroups_AddonGroupId",
                table: "ProductAddonGroups",
                column: "AddonGroupId",
                principalTable: "AddonGroups",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ProductAddonGroups_Products_ProductId",
                table: "ProductAddonGroups",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ProductImages_Products_ProductId",
                table: "ProductImages",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ProductVariants_Products_ProductId",
                table: "ProductVariants",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TenantDomains_Tenants_TenantId",
                table: "TenantDomains",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}

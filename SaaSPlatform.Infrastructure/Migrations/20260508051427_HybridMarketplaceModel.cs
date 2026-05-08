using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SaaSPlatform.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class HybridMarketplaceModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RestaurantTables_Stores_StoreId",
                table: "RestaurantTables");

            migrationBuilder.DropIndex(
                name: "IX_RestaurantTables_StoreId",
                table: "RestaurantTables");

            migrationBuilder.RenameColumn(
                name: "Phone",
                table: "Stores",
                newName: "ZoneName");

            migrationBuilder.RenameColumn(
                name: "IsActive",
                table: "Stores",
                newName: "SupportsPickup");

            migrationBuilder.AlterColumn<double>(
                name: "Longitude",
                table: "Stores",
                type: "double precision",
                precision: 9,
                scale: 6,
                nullable: false,
                defaultValue: 0.0,
                oldClrType: typeof(decimal),
                oldType: "numeric(9,6)",
                oldPrecision: 9,
                oldScale: 6,
                oldNullable: true);

            migrationBuilder.AlterColumn<double>(
                name: "Latitude",
                table: "Stores",
                type: "double precision",
                precision: 9,
                scale: 6,
                nullable: false,
                defaultValue: 0.0,
                oldClrType: typeof(decimal),
                oldType: "numeric(9,6)",
                oldPrecision: 9,
                oldScale: 6,
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "City",
                table: "Stores",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CoverPhotoUrl",
                table: "Stores",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsOnline",
                table: "Stores",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "LogoUrl",
                table: "Stores",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MaxPreparingTime",
                table: "Stores",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "MinPreparingTime",
                table: "Stores",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "SupportsDelivery",
                table: "Stores",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "SupportsDineIn",
                table: "Stores",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "DisplayOrder",
                table: "Products",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "MainImageUrl",
                table: "Products",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeliveryAddress",
                table: "Orders",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "OrderType",
                table: "Orders",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "DisplayOrder",
                table: "Categories",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Icon",
                table: "Categories",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PhotoUrl",
                table: "Categories",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Cuisines",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cuisines", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FeaturedProducts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductId = table.Column<Guid>(type: "uuid", nullable: false),
                    Section = table.Column<int>(type: "integer", nullable: false),
                    CategoryId = table.Column<Guid>(type: "uuid", nullable: true),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false),
                    FeaturedFrom = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    FeaturedUntil = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FeaturedProducts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FeaturedProducts_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ProductImages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductId = table.Column<Guid>(type: "uuid", nullable: false),
                    ImageUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false),
                    IsPrimary = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductImages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductImages_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TenantLegalInfos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    CnicNumber = table.Column<string>(type: "text", nullable: false),
                    NtnNumber = table.Column<string>(type: "text", nullable: true),
                    HasFoodLicense = table.Column<bool>(type: "boolean", nullable: false),
                    CnicFrontImageUrl = table.Column<string>(type: "text", nullable: true),
                    CnicBackImageUrl = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TenantLegalInfos", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Zones",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    City = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Zones", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "StoreCuisines",
                columns: table => new
                {
                    StoreId = table.Column<Guid>(type: "uuid", nullable: false),
                    CuisineId = table.Column<Guid>(type: "uuid", nullable: false),
                    Id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StoreCuisines", x => new { x.StoreId, x.CuisineId });
                    table.ForeignKey(
                        name: "FK_StoreCuisines_Cuisines_CuisineId",
                        column: x => x.CuisineId,
                        principalTable: "Cuisines",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_StoreCuisines_Stores_StoreId",
                        column: x => x.StoreId,
                        principalTable: "Stores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StoreDeliveryZones",
                columns: table => new
                {
                    StoreId = table.Column<Guid>(type: "uuid", nullable: false),
                    ZoneId = table.Column<Guid>(type: "uuid", nullable: false),
                    Id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StoreDeliveryZones", x => new { x.StoreId, x.ZoneId });
                    table.ForeignKey(
                        name: "FK_StoreDeliveryZones_Stores_StoreId",
                        column: x => x.StoreId,
                        principalTable: "Stores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_StoreDeliveryZones_Zones_ZoneId",
                        column: x => x.ZoneId,
                        principalTable: "Zones",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Subscriptions_PlanId",
                table: "Subscriptions",
                column: "PlanId");

            migrationBuilder.CreateIndex(
                name: "IX_Stores_City",
                table: "Stores",
                column: "City");

            migrationBuilder.CreateIndex(
                name: "IX_Stores_IsOnline",
                table: "Stores",
                column: "IsOnline");

            migrationBuilder.CreateIndex(
                name: "IX_Stores_ZoneName",
                table: "Stores",
                column: "ZoneName");

            migrationBuilder.CreateIndex(
                name: "IX_Cuisines_Name",
                table: "Cuisines",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FeaturedProducts_ProductId",
                table: "FeaturedProducts",
                column: "ProductId");

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
                name: "IX_ProductImages_ProductId_DisplayOrder",
                table: "ProductImages",
                columns: new[] { "ProductId", "DisplayOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_StoreCuisines_CuisineId",
                table: "StoreCuisines",
                column: "CuisineId");

            migrationBuilder.CreateIndex(
                name: "IX_StoreDeliveryZones_ZoneId",
                table: "StoreDeliveryZones",
                column: "ZoneId");

            migrationBuilder.CreateIndex(
                name: "IX_TenantLegalInfos_TenantId",
                table: "TenantLegalInfos",
                column: "TenantId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Zones_City_Name",
                table: "Zones",
                columns: new[] { "City", "Name" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Subscriptions_Plans_PlanId",
                table: "Subscriptions",
                column: "PlanId",
                principalTable: "Plans",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Subscriptions_Plans_PlanId",
                table: "Subscriptions");

            migrationBuilder.DropTable(
                name: "FeaturedProducts");

            migrationBuilder.DropTable(
                name: "ProductImages");

            migrationBuilder.DropTable(
                name: "StoreCuisines");

            migrationBuilder.DropTable(
                name: "StoreDeliveryZones");

            migrationBuilder.DropTable(
                name: "TenantLegalInfos");

            migrationBuilder.DropTable(
                name: "Cuisines");

            migrationBuilder.DropTable(
                name: "Zones");

            migrationBuilder.DropIndex(
                name: "IX_Subscriptions_PlanId",
                table: "Subscriptions");

            migrationBuilder.DropIndex(
                name: "IX_Stores_City",
                table: "Stores");

            migrationBuilder.DropIndex(
                name: "IX_Stores_IsOnline",
                table: "Stores");

            migrationBuilder.DropIndex(
                name: "IX_Stores_ZoneName",
                table: "Stores");

            migrationBuilder.DropColumn(
                name: "City",
                table: "Stores");

            migrationBuilder.DropColumn(
                name: "CoverPhotoUrl",
                table: "Stores");

            migrationBuilder.DropColumn(
                name: "IsOnline",
                table: "Stores");

            migrationBuilder.DropColumn(
                name: "LogoUrl",
                table: "Stores");

            migrationBuilder.DropColumn(
                name: "MaxPreparingTime",
                table: "Stores");

            migrationBuilder.DropColumn(
                name: "MinPreparingTime",
                table: "Stores");

            migrationBuilder.DropColumn(
                name: "SupportsDelivery",
                table: "Stores");

            migrationBuilder.DropColumn(
                name: "SupportsDineIn",
                table: "Stores");

            migrationBuilder.DropColumn(
                name: "DisplayOrder",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "MainImageUrl",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "DeliveryAddress",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "OrderType",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "DisplayOrder",
                table: "Categories");

            migrationBuilder.DropColumn(
                name: "Icon",
                table: "Categories");

            migrationBuilder.DropColumn(
                name: "PhotoUrl",
                table: "Categories");

            migrationBuilder.RenameColumn(
                name: "ZoneName",
                table: "Stores",
                newName: "Phone");

            migrationBuilder.RenameColumn(
                name: "SupportsPickup",
                table: "Stores",
                newName: "IsActive");

            migrationBuilder.AlterColumn<decimal>(
                name: "Longitude",
                table: "Stores",
                type: "numeric(9,6)",
                precision: 9,
                scale: 6,
                nullable: true,
                oldClrType: typeof(double),
                oldType: "double precision",
                oldPrecision: 9,
                oldScale: 6);

            migrationBuilder.AlterColumn<decimal>(
                name: "Latitude",
                table: "Stores",
                type: "numeric(9,6)",
                precision: 9,
                scale: 6,
                nullable: true,
                oldClrType: typeof(double),
                oldType: "double precision",
                oldPrecision: 9,
                oldScale: 6);

            migrationBuilder.CreateIndex(
                name: "IX_RestaurantTables_StoreId",
                table: "RestaurantTables",
                column: "StoreId");

            migrationBuilder.AddForeignKey(
                name: "FK_RestaurantTables_Stores_StoreId",
                table: "RestaurantTables",
                column: "StoreId",
                principalTable: "Stores",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}

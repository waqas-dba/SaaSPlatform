using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SaaSPlatform.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class moduleidtopermissionmoduleid : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Permissions_PermissionModules_ModuleId",
                table: "Permissions");

            migrationBuilder.RenameColumn(
                name: "ModuleId",
                table: "Permissions",
                newName: "PermissionModuleId");

            migrationBuilder.RenameIndex(
                name: "IX_Permissions_ModuleId",
                table: "Permissions",
                newName: "IX_Permissions_PermissionModuleId");

            migrationBuilder.AddForeignKey(
                name: "FK_Permissions_PermissionModules_PermissionModuleId",
                table: "Permissions",
                column: "PermissionModuleId",
                principalTable: "PermissionModules",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Permissions_PermissionModules_PermissionModuleId",
                table: "Permissions");

            migrationBuilder.RenameColumn(
                name: "PermissionModuleId",
                table: "Permissions",
                newName: "ModuleId");

            migrationBuilder.RenameIndex(
                name: "IX_Permissions_PermissionModuleId",
                table: "Permissions",
                newName: "IX_Permissions_ModuleId");

            migrationBuilder.AddForeignKey(
                name: "FK_Permissions_PermissionModules_ModuleId",
                table: "Permissions",
                column: "ModuleId",
                principalTable: "PermissionModules",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}

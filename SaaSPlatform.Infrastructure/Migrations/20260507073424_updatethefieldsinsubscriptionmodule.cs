using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SaaSPlatform.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class updatethefieldsinsubscriptionmodule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Payments_TransactionId",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "GraceDaysUsed",
                table: "Subscriptions");

            migrationBuilder.DropColumn(
                name: "MaxGraceDays",
                table: "Subscriptions");

            migrationBuilder.RenameColumn(
                name: "TransactionId",
                table: "Payments",
                newName: "ReferenceNumber");

            migrationBuilder.RenameColumn(
                name: "FailureReason",
                table: "Payments",
                newName: "Notes");

            migrationBuilder.AddColumn<bool>(
                name: "AllowAdminAccess",
                table: "Plans",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "AllowOrders",
                table: "Plans",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "AllowProductCreation",
                table: "Plans",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "GraceDays",
                table: "Plans",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TrialDays",
                table: "Plans",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AllowAdminAccess",
                table: "Plans");

            migrationBuilder.DropColumn(
                name: "AllowOrders",
                table: "Plans");

            migrationBuilder.DropColumn(
                name: "AllowProductCreation",
                table: "Plans");

            migrationBuilder.DropColumn(
                name: "GraceDays",
                table: "Plans");

            migrationBuilder.DropColumn(
                name: "TrialDays",
                table: "Plans");

            migrationBuilder.RenameColumn(
                name: "ReferenceNumber",
                table: "Payments",
                newName: "TransactionId");

            migrationBuilder.RenameColumn(
                name: "Notes",
                table: "Payments",
                newName: "FailureReason");

            migrationBuilder.AddColumn<int>(
                name: "GraceDaysUsed",
                table: "Subscriptions",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "MaxGraceDays",
                table: "Subscriptions",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Payments_TransactionId",
                table: "Payments",
                column: "TransactionId");
        }
    }
}

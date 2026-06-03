using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Moveo_backend.Migrations
{
    /// <inheritdoc />
    public partial class AddSupportTicketDamageAndDisputeFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AdditionalProofJson",
                table: "SupportTickets",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "AdditionalProofMessage",
                table: "SupportTickets",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<DateTime>(
                name: "AdditionalProofSentAt",
                table: "SupportTickets",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AttachmentsJson",
                table: "SupportTickets",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "DisputeDescription",
                table: "SupportTickets",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "DisputeReason",
                table: "SupportTickets",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "DisputeStatus",
                table: "SupportTickets",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<DateTime>(
                name: "DisputedAt",
                table: "SupportTickets",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DisputedBy",
                table: "SupportTickets",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "EstimatedCost",
                table: "SupportTickets",
                type: "decimal(65,30)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "PaidAt",
                table: "SupportTickets",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PaymentStatus",
                table: "SupportTickets",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "RentalId",
                table: "SupportTickets",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RenterId",
                table: "SupportTickets",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RenterName",
                table: "SupportTickets",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "VehicleId",
                table: "SupportTickets",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VehicleName",
                table: "SupportTickets",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AdditionalProofJson",
                table: "SupportTickets");

            migrationBuilder.DropColumn(
                name: "AdditionalProofMessage",
                table: "SupportTickets");

            migrationBuilder.DropColumn(
                name: "AdditionalProofSentAt",
                table: "SupportTickets");

            migrationBuilder.DropColumn(
                name: "AttachmentsJson",
                table: "SupportTickets");

            migrationBuilder.DropColumn(
                name: "DisputeDescription",
                table: "SupportTickets");

            migrationBuilder.DropColumn(
                name: "DisputeReason",
                table: "SupportTickets");

            migrationBuilder.DropColumn(
                name: "DisputeStatus",
                table: "SupportTickets");

            migrationBuilder.DropColumn(
                name: "DisputedAt",
                table: "SupportTickets");

            migrationBuilder.DropColumn(
                name: "DisputedBy",
                table: "SupportTickets");

            migrationBuilder.DropColumn(
                name: "EstimatedCost",
                table: "SupportTickets");

            migrationBuilder.DropColumn(
                name: "PaidAt",
                table: "SupportTickets");

            migrationBuilder.DropColumn(
                name: "PaymentStatus",
                table: "SupportTickets");

            migrationBuilder.DropColumn(
                name: "RentalId",
                table: "SupportTickets");

            migrationBuilder.DropColumn(
                name: "RenterId",
                table: "SupportTickets");

            migrationBuilder.DropColumn(
                name: "RenterName",
                table: "SupportTickets");

            migrationBuilder.DropColumn(
                name: "VehicleId",
                table: "SupportTickets");

            migrationBuilder.DropColumn(
                name: "VehicleName",
                table: "SupportTickets");
        }
    }
}

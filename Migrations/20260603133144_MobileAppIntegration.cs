using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Moveo_backend.Migrations
{
    /// <inheritdoc />
    public partial class MobileAppIntegration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BodyType",
                table: "Vehicles",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "Community",
                table: "AdventureRoutes",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<DateTime>(
                name: "DepartureDate",
                table: "AdventureRoutes",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DepartureTime",
                table: "AdventureRoutes",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<double>(
                name: "Lat",
                table: "AdventureRoutes",
                type: "double",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "Lng",
                table: "AdventureRoutes",
                type: "double",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "OnlyWomen",
                table: "AdventureRoutes",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "PricePerSeat",
                table: "AdventureRoutes",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SeatsAvailable",
                table: "AdventureRoutes",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SeatsTotal",
                table: "AdventureRoutes",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "AdventureRoutes",
                type: "longtext",
                nullable: false,
                defaultValue: "active")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Messages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    SenderId = table.Column<int>(type: "int", nullable: false),
                    ReceiverId = table.Column<int>(type: "int", nullable: false),
                    Content = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Read = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    ReadAt = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Messages", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_Messages_SenderId_ReceiverId",
                table: "Messages",
                columns: new[] { "SenderId", "ReceiverId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Messages");

            migrationBuilder.DropColumn(
                name: "BodyType",
                table: "Vehicles");

            migrationBuilder.DropColumn(
                name: "Community",
                table: "AdventureRoutes");

            migrationBuilder.DropColumn(
                name: "DepartureDate",
                table: "AdventureRoutes");

            migrationBuilder.DropColumn(
                name: "DepartureTime",
                table: "AdventureRoutes");

            migrationBuilder.DropColumn(
                name: "Lat",
                table: "AdventureRoutes");

            migrationBuilder.DropColumn(
                name: "Lng",
                table: "AdventureRoutes");

            migrationBuilder.DropColumn(
                name: "OnlyWomen",
                table: "AdventureRoutes");

            migrationBuilder.DropColumn(
                name: "PricePerSeat",
                table: "AdventureRoutes");

            migrationBuilder.DropColumn(
                name: "SeatsAvailable",
                table: "AdventureRoutes");

            migrationBuilder.DropColumn(
                name: "SeatsTotal",
                table: "AdventureRoutes");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "AdventureRoutes");
        }
    }
}

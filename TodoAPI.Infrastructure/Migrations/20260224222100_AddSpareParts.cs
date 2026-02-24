using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace TodoAPI.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSpareParts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SpareParts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Reference = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    VhuCenter = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Price = table.Column<decimal>(type: "TEXT", nullable: false),
                    StockQuantity = table.Column<int>(type: "INTEGER", nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    ReservedByChannel = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    ReservedByBuyer = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    ReservedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "TEXT", maxLength: 36, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SpareParts", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "SpareParts",
                columns: new[] { "Id", "ConcurrencyStamp", "CreatedAt", "Description", "Price", "Reference", "ReservedAt", "ReservedByBuyer", "ReservedByChannel", "Status", "StockQuantity", "VhuCenter" },
                values: new object[,]
                {
                    { 1, "seed-stamp-part-1", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Alternateur Renault Clio 3 1.5 dCi", 89.90m, "8200162474", null, null, null, 0, 1, "AutoCasse Toulouse #42" },
                    { 2, "seed-stamp-part-2", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Rétroviseur gauche Peugeot 308", 45.50m, "7701054753", null, null, null, 0, 1, "VHU Lyon #18" },
                    { 3, "seed-stamp-part-3", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Phare avant droit Renault Megane 3", 120.00m, "8200768913", null, null, null, 0, 1, "VHU Nantes #7" }
                });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$IjkCzhBhhCdcVY/ViKQ7FeMyMnuKZgMNG7h8LS1XOU3mlHHyw6fFq");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SpareParts");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$hQUL.niWtQc58jYcBHr2IOpn3i08GhJTFNWIN/MGyi8gMqs2o/Di6");
        }
    }
}

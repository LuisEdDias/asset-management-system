using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace AssetManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SeedData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "asset_types",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { 1L, "NOTEBOOK" },
                    { 2L, "MONITOR" },
                    { 3L, "DESKTOP" },
                    { 4L, "PERIFÉRICOS" }
                });

            migrationBuilder.InsertData(
                table: "users",
                columns: new[] { "Id", "Email", "Name" },
                values: new object[,]
                {
                    { 1L, "ana@mail.com", "Ana" },
                    { 2L, "luis@mail.com", "Luís" },
                    { 3L, "jose@mail.com", "José" }
                });

            migrationBuilder.InsertData(
                table: "assets",
                columns: new[] { "Id", "AssetTypeId", "AssignedAtUtc", "AssignedToUserId", "Name", "SerialNumber", "Status", "Value" },
                values: new object[,]
                {
                    { 1L, 1L, null, null, "MacBook Pro M3", "SN123", "Available", 15000.00m },
                    { 2L, 2L, null, null, "Acer Aspire 5", "SN456", "Available", 3500.00m },
                    { 3L, 2L, null, null, "Dell UltraSharp 27", "SN789", "Maintenance", 3500.00m },
                    { 4L, 3L, null, null, "Dell Tower Plus Intel Core Ultra 5", "SN987", "Available", 9999.99m },
                    { 5L, 4L, null, null, "Mouse Logitech", "SN654", "Available", 30.00m },
                    { 6L, 4L, null, null, "Multifuncional HP", "SN321", "Maintenance", 1500.00m }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "assets",
                keyColumn: "Id",
                keyValue: 1L);

            migrationBuilder.DeleteData(
                table: "assets",
                keyColumn: "Id",
                keyValue: 2L);

            migrationBuilder.DeleteData(
                table: "assets",
                keyColumn: "Id",
                keyValue: 3L);

            migrationBuilder.DeleteData(
                table: "assets",
                keyColumn: "Id",
                keyValue: 4L);

            migrationBuilder.DeleteData(
                table: "assets",
                keyColumn: "Id",
                keyValue: 5L);

            migrationBuilder.DeleteData(
                table: "assets",
                keyColumn: "Id",
                keyValue: 6L);

            migrationBuilder.DeleteData(
                table: "users",
                keyColumn: "Id",
                keyValue: 1L);

            migrationBuilder.DeleteData(
                table: "users",
                keyColumn: "Id",
                keyValue: 2L);

            migrationBuilder.DeleteData(
                table: "users",
                keyColumn: "Id",
                keyValue: 3L);

            migrationBuilder.DeleteData(
                table: "asset_types",
                keyColumn: "Id",
                keyValue: 1L);

            migrationBuilder.DeleteData(
                table: "asset_types",
                keyColumn: "Id",
                keyValue: 2L);

            migrationBuilder.DeleteData(
                table: "asset_types",
                keyColumn: "Id",
                keyValue: 3L);

            migrationBuilder.DeleteData(
                table: "asset_types",
                keyColumn: "Id",
                keyValue: 4L);
        }
    }
}

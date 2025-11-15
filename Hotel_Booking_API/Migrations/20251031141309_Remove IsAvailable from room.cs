using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Hotel_Booking_API.Migrations
{
    /// <inheritdoc />
    public partial class RemoveIsAvailablefromroom : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Rooms",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Rooms",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Rooms",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Hotels",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Hotels",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DropColumn(
                name: "IsAvailable",
                table: "Rooms");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsAvailable",
                table: "Rooms",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.InsertData(
                table: "Hotels",
                columns: new[] { "Id", "Address", "City", "Country", "CreatedAt", "Description", "IsDeleted", "Name", "Rating", "UpdatedAt" },
                values: new object[,]
                {
                    { 1, "123 Main Street", "New York", "USA", new DateTime(2025, 10, 17, 18, 20, 20, 542, DateTimeKind.Utc).AddTicks(6143), "Luxurious 5-star hotel in the heart of the city", false, "Grand Palace Hotel", 4.5m, null },
                    { 2, "456 Beach Road", "Miami", "USA", new DateTime(2025, 10, 17, 18, 20, 20, 542, DateTimeKind.Utc).AddTicks(6146), "Beautiful beachfront resort with stunning ocean views", false, "Ocean View Resort", 4.2m, null }
                });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "CreatedAt", "Email", "FirstName", "IsDeleted", "LastName", "PasswordHash", "Role", "UpdatedAt" },
                values: new object[] { 1, new DateTime(2025, 10, 17, 18, 20, 20, 542, DateTimeKind.Utc).AddTicks(5355), "admin@hotelbooking.com", "Admin", false, "User", "$2a$11$ZIfS0jV/psoyJRf2Cyo/Mu8HTY3nC8ryumK2JuLYiPEAb6JW3jUZK", 1, null });

            migrationBuilder.InsertData(
                table: "Rooms",
                columns: new[] { "Id", "Capacity", "CreatedAt", "Description", "HotelId", "IsAvailable", "IsDeleted", "Price", "RoomNumber", "Type", "UpdatedAt" },
                values: new object[,]
                {
                    { 1, 2, new DateTime(2025, 10, 17, 18, 20, 20, 542, DateTimeKind.Utc).AddTicks(6352), "Comfortable standard room with city view", 1, true, false, 150.00m, "101", 0, null },
                    { 2, 2, new DateTime(2025, 10, 17, 18, 20, 20, 542, DateTimeKind.Utc).AddTicks(6365), "Spacious deluxe room with premium amenities", 1, true, false, 250.00m, "201", 1, null },
                    { 3, 4, new DateTime(2025, 10, 17, 18, 20, 20, 542, DateTimeKind.Utc).AddTicks(6367), "Luxurious suite with ocean view", 2, true, false, 400.00m, "301", 2, null }
                });
        }
    }
}

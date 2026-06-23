using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hotel_Booking_API.Migrations
{
    /// <inheritdoc />
    public partial class AddAiAnalysisToReviews : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AiSummary",
                table: "Reviews",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Issues",
                table: "Reviews",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Positives",
                table: "Reviews",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Sentiment",
                table: "Reviews",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AiSummary",
                table: "Reviews");

            migrationBuilder.DropColumn(
                name: "Issues",
                table: "Reviews");

            migrationBuilder.DropColumn(
                name: "Positives",
                table: "Reviews");

            migrationBuilder.DropColumn(
                name: "Sentiment",
                table: "Reviews");
        }
    }
}

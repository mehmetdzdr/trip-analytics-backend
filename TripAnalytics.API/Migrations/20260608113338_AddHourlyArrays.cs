using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TripAnalytics.API.Migrations
{
    /// <inheritdoc />
    public partial class AddHourlyArrays : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int[]>(
                name: "DropoffsByHour",
                table: "TripSummaries",
                type: "integer[]",
                nullable: false,
                defaultValue: new int[0]);

            migrationBuilder.AddColumn<int[]>(
                name: "PickupsByHour",
                table: "TripSummaries",
                type: "integer[]",
                nullable: false,
                defaultValue: new int[0]);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DropoffsByHour",
                table: "TripSummaries");

            migrationBuilder.DropColumn(
                name: "PickupsByHour",
                table: "TripSummaries");
        }
    }
}

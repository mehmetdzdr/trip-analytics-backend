using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TripAnalytics.API.Migrations
{
    /// <inheritdoc />
    public partial class AddZonePairSummary : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ZonePairSummaries",
                columns: table => new
                {
                    PickupZip = table.Column<string>(type: "text", nullable: false),
                    DropoffZip = table.Column<string>(type: "text", nullable: false),
                    TripCount = table.Column<int>(type: "integer", nullable: false),
                    AvgDuration = table.Column<double>(type: "double precision", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ZonePairSummaries", x => new { x.PickupZip, x.DropoffZip });
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ZonePairSummaries");
        }
    }
}

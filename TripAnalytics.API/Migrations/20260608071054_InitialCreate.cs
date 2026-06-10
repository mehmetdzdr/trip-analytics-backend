using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TripAnalytics.API.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ZipZones",
                columns: table => new
                {
                    PostalCode = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    Borough = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    AreaKm2 = table.Column<double>(type: "double precision", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ZipZones", x => x.PostalCode);
                });

            migrationBuilder.CreateTable(
                name: "TripSummaries",
                columns: table => new
                {
                    PostalCode = table.Column<string>(type: "character varying(10)", nullable: false),
                    PickupCount = table.Column<int>(type: "integer", nullable: false),
                    DropoffCount = table.Column<int>(type: "integer", nullable: false),
                    AvgFare = table.Column<double>(type: "double precision", nullable: false),
                    AvgDistance = table.Column<double>(type: "double precision", nullable: false),
                    DensityPerKm2 = table.Column<double>(type: "double precision", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TripSummaries", x => x.PostalCode);
                    table.ForeignKey(
                        name: "FK_TripSummaries_ZipZones_PostalCode",
                        column: x => x.PostalCode,
                        principalTable: "ZipZones",
                        principalColumn: "PostalCode",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TripSummaries");

            migrationBuilder.DropTable(
                name: "ZipZones");
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Data.Migrations
{
    /// <inheritdoc />
    public partial class RenameModels : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Workers_Locations_LocationId",
                table: "Workers");

            migrationBuilder.DropForeignKey(
                name: "FK_Workers_Services_ServiceId",
                table: "Workers");

            migrationBuilder.CreateIndex(
                name: "IX_Services_Name",
                table: "Services",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Locations_City",
                table: "Locations",
                column: "City",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Workers_Locations_LocationId",
                table: "Workers",
                column: "Id",
                principalTable: "Locations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Workers_Services_ServiceId",
                table: "Workers",
                column: "ServiceId",
                principalTable: "Services",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Workers_Locations_LocationId",
                table: "Workers");

            migrationBuilder.DropForeignKey(
                name: "FK_Workers_Services_ServiceId",
                table: "Workers");

            migrationBuilder.DropIndex(
                name: "IX_Services_Name",
                table: "Services");

            migrationBuilder.DropIndex(
                name: "IX_Locations_City",
                table: "Locations");

            migrationBuilder.AddForeignKey(
                name: "FK_Workers_Locations_LocationId",
                table: "Workers",
                column: "Id",
                principalTable: "Locations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Workers_Services_ServiceId",
                table: "Workers",
                column: "ServiceId",
                principalTable: "Services",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}

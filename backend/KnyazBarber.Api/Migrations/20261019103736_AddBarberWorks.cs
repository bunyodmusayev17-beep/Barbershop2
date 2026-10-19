using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KnyazBarber.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddBarberWorks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "BarberId",
                table: "GalleryImages",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_GalleryImages_BarberId",
                table: "GalleryImages",
                column: "BarberId");

            migrationBuilder.AddForeignKey(
                name: "FK_GalleryImages_Barbers_BarberId",
                table: "GalleryImages",
                column: "BarberId",
                principalTable: "Barbers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GalleryImages_Barbers_BarberId",
                table: "GalleryImages");

            migrationBuilder.DropIndex(
                name: "IX_GalleryImages_BarberId",
                table: "GalleryImages");

            migrationBuilder.DropColumn(
                name: "BarberId",
                table: "GalleryImages");
        }
    }
}

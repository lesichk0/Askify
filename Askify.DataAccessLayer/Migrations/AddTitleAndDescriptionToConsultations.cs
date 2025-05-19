using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Askify.DataAccessLayer.Migrations
{
    /// <inheritdoc />
    public partial class AddTitleAndDescriptionToConsultations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Add Title column with a default value for existing rows
            migrationBuilder.AddColumn<string>(
                name: "Title",
                table: "Consultations",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "Untitled Consultation");

            // Add Description column with a default value for existing rows
            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Consultations",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "No description provided.");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Title",
                table: "Consultations");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "Consultations");
        }
    }
}

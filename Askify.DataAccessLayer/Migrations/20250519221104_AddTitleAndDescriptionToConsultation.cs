using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Askify.DataAccessLayer.Migrations
{
    /// <inheritdoc />
    public partial class AddTitleAndDescriptionToConsultation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Consultations",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Title",
                table: "Consultations",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Description",
                table: "Consultations");

            migrationBuilder.DropColumn(
                name: "Title",
                table: "Consultations");
        }
    }
}

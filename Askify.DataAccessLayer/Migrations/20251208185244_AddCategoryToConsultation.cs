using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Askify.DataAccessLayer.Migrations
{
    /// <inheritdoc />
    public partial class AddCategoryToConsultation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Category",
                table: "Consultations",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Category",
                table: "Consultations");
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Askify.DataAccessLayer.Migrations
{
    /// <inheritdoc />
    public partial class AddConsultationIdToFeedback : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ConsultationId",
                table: "Feedbacks",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Feedbacks_ConsultationId",
                table: "Feedbacks",
                column: "ConsultationId");

            migrationBuilder.AddForeignKey(
                name: "FK_Feedbacks_Consultations_ConsultationId",
                table: "Feedbacks",
                column: "ConsultationId",
                principalTable: "Consultations",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Feedbacks_Consultations_ConsultationId",
                table: "Feedbacks");

            migrationBuilder.DropIndex(
                name: "IX_Feedbacks_ConsultationId",
                table: "Feedbacks");

            migrationBuilder.DropColumn(
                name: "ConsultationId",
                table: "Feedbacks");
        }
    }
}

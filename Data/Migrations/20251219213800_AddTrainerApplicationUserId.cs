using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebProjeGym.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddTrainerApplicationUserId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ApplicationUserId",
                table: "Trainers",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Trainers_ApplicationUserId",
                table: "Trainers",
                column: "ApplicationUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Trainers_AspNetUsers_ApplicationUserId",
                table: "Trainers",
                column: "ApplicationUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Trainers_AspNetUsers_ApplicationUserId",
                table: "Trainers");

            migrationBuilder.DropIndex(
                name: "IX_Trainers_ApplicationUserId",
                table: "Trainers");

            migrationBuilder.DropColumn(
                name: "ApplicationUserId",
                table: "Trainers");
        }
    }
}


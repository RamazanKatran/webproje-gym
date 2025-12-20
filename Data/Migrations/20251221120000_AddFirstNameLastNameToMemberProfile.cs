using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebProjeGym.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddFirstNameLastNameToMemberProfile : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FirstName",
                table: "MemberProfiles",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LastName",
                table: "MemberProfiles",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FirstName",
                table: "MemberProfiles");

            migrationBuilder.DropColumn(
                name: "LastName",
                table: "MemberProfiles");
        }
    }
}


using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RegistrationPortal.Server.Migrations
{
    /// <inheritdoc />
    public partial class AddedBranchInUsers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BRANCH",
                table: "USERS",
                type: "NVARCHAR2(50)",
                maxLength: 50,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BRANCH",
                table: "USERS");
        }
    }
}

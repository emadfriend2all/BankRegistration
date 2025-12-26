using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RegistrationPortal.Server.Migrations
{
    /// <inheritdoc />
    public partial class AddReviewFieldsToCustMast : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "review_status",
                schema: "SSDBONLINE",
                table: "cust_mast",
                type: "NVARCHAR2(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "reviewed_by",
                schema: "SSDBONLINE",
                table: "cust_mast",
                type: "NVARCHAR2(100)",
                maxLength: 100,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "review_status",
                schema: "SSDBONLINE",
                table: "cust_mast");

            migrationBuilder.DropColumn(
                name: "reviewed_by",
                schema: "SSDBONLINE",
                table: "cust_mast");
        }
    }
}

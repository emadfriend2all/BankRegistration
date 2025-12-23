using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RegistrationPortal.Server.Migrations
{
    /// <inheritdoc />
    public partial class AddStatusColumnToCustMast : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "sole_sudan_residency_confirmed",
                schema: "SSDBONLINE",
                table: "customer_fatca",
                type: "CHAR(10)",
                maxLength: 10,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "CHAR(1)",
                oldMaxLength: 1,
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "file_size",
                schema: "SSDBONLINE",
                table: "customer_documents",
                type: "NUMBER(20,0)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "NUMBER");

            migrationBuilder.AddColumn<string>(
                name: "status",
                schema: "SSDBONLINE",
                table: "cust_mast",
                type: "NVARCHAR2(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "cust_i_no",
                schema: "SSDBONLINE",
                table: "account_mast",
                type: "DECIMAL(18, 2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "DECIMAL(18,2)");

            migrationBuilder.AlterColumn<decimal>(
                name: "act_i_trbrcode",
                schema: "SSDBONLINE",
                table: "account_mast",
                type: "DECIMAL(18, 2)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "DECIMAL(18,2)",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "act_i_introid",
                schema: "SSDBONLINE",
                table: "account_mast",
                type: "DECIMAL(18, 2)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "DECIMAL(18,2)",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_cust_mast_status",
                schema: "SSDBONLINE",
                table: "cust_mast",
                column: "status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_cust_mast_status",
                schema: "SSDBONLINE",
                table: "cust_mast");

            migrationBuilder.DropColumn(
                name: "status",
                schema: "SSDBONLINE",
                table: "cust_mast");

            migrationBuilder.AlterColumn<string>(
                name: "sole_sudan_residency_confirmed",
                schema: "SSDBONLINE",
                table: "customer_fatca",
                type: "CHAR(1)",
                maxLength: 1,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "CHAR(10)",
                oldMaxLength: 10,
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "file_size",
                schema: "SSDBONLINE",
                table: "customer_documents",
                type: "NUMBER",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "NUMBER(20,0)");

            migrationBuilder.AlterColumn<decimal>(
                name: "cust_i_no",
                schema: "SSDBONLINE",
                table: "account_mast",
                type: "DECIMAL(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "DECIMAL(18, 2)");

            migrationBuilder.AlterColumn<decimal>(
                name: "act_i_trbrcode",
                schema: "SSDBONLINE",
                table: "account_mast",
                type: "DECIMAL(18,2)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "DECIMAL(18, 2)",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "act_i_introid",
                schema: "SSDBONLINE",
                table: "account_mast",
                type: "DECIMAL(18,2)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "DECIMAL(18, 2)",
                oldNullable: true);
        }
    }
}

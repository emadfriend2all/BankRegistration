using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RegistrationPortal.Server.Migrations
{
    /// <inheritdoc />
    public partial class AddCustomerFatcaTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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

            migrationBuilder.CreateTable(
                name: "customer_fatca",
                schema: "SSDBONLINE",
                columns: table => new
                {
                    id = table.Column<string>(type: "VARCHAR2(50)", maxLength: 50, nullable: false),
                    customer_id = table.Column<string>(type: "VARCHAR2(50)", maxLength: 50, nullable: false),
                    is_us_person = table.Column<string>(type: "CHAR(1)", maxLength: 1, nullable: true),
                    citizenship_countries = table.Column<string>(type: "VARCHAR2(500)", maxLength: 500, nullable: true),
                    has_immigrant_visa = table.Column<string>(type: "CHAR(1)", maxLength: 1, nullable: true),
                    permanent_residency_states = table.Column<string>(type: "VARCHAR2(500)", maxLength: 500, nullable: true),
                    has_other_countries_residency = table.Column<string>(type: "CHAR(1)", maxLength: 1, nullable: true),
                    sole_sudan_residency_confirmed = table.Column<string>(type: "CHAR(1)", maxLength: 1, nullable: true),
                    ssn = table.Column<string>(type: "VARCHAR2(20)", maxLength: 20, nullable: true),
                    itin = table.Column<string>(type: "VARCHAR2(20)", maxLength: 20, nullable: true),
                    atin = table.Column<string>(type: "VARCHAR2(20)", maxLength: 20, nullable: true),
                    country1_tax_jurisdiction = table.Column<string>(type: "VARCHAR2(100)", maxLength: 100, nullable: true),
                    country1_tin = table.Column<string>(type: "VARCHAR2(50)", maxLength: 50, nullable: true),
                    country1_no_tin_reason = table.Column<string>(type: "VARCHAR2(1)", maxLength: 1, nullable: true),
                    country1_no_tin_explanation = table.Column<string>(type: "VARCHAR2(500)", maxLength: 500, nullable: true),
                    country2_tax_jurisdiction = table.Column<string>(type: "VARCHAR2(100)", maxLength: 100, nullable: true),
                    country2_tin = table.Column<string>(type: "VARCHAR2(50)", maxLength: 50, nullable: true),
                    country2_no_tin_reason = table.Column<string>(type: "VARCHAR2(1)", maxLength: 1, nullable: true),
                    country2_no_tin_explanation = table.Column<string>(type: "VARCHAR2(500)", maxLength: 500, nullable: true),
                    country3_tax_jurisdiction = table.Column<string>(type: "VARCHAR2(100)", maxLength: 100, nullable: true),
                    country3_tin = table.Column<string>(type: "VARCHAR2(50)", maxLength: 50, nullable: true),
                    country3_no_tin_reason = table.Column<string>(type: "VARCHAR2(1)", maxLength: 1, nullable: true),
                    country3_no_tin_explanation = table.Column<string>(type: "VARCHAR2(500)", maxLength: 500, nullable: true),
                    created_at = table.Column<DateTime>(type: "TIMESTAMP(6)", nullable: false),
                    updated_at = table.Column<DateTime>(type: "TIMESTAMP(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_customer_fatca", x => x.id);
                    table.ForeignKey(
                        name: "fk_fatca_customer",
                        column: x => x.customer_id,
                        principalSchema: "SSDBONLINE",
                        principalTable: "cust_mast",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_fatca_customer_id",
                schema: "SSDBONLINE",
                table: "customer_fatca",
                column: "customer_id",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "customer_fatca",
                schema: "SSDBONLINE");

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

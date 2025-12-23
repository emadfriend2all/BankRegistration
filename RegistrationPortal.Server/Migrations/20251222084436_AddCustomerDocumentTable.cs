using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RegistrationPortal.Server.Migrations
{
    /// <inheritdoc />
    public partial class AddCustomerDocumentTable : Migration
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
                name: "customer_documents",
                schema: "SSDBONLINE",
                columns: table => new
                {
                    id = table.Column<string>(type: "VARCHAR2(50)", maxLength: 50, nullable: false),
                    customer_id = table.Column<string>(type: "VARCHAR2(50)", maxLength: 50, nullable: false),
                    document_type = table.Column<string>(type: "VARCHAR2(50)", maxLength: 50, nullable: false),
                    file_url = table.Column<string>(type: "VARCHAR2(500)", maxLength: 500, nullable: false),
                    original_file_name = table.Column<string>(type: "VARCHAR2(255)", maxLength: 255, nullable: true),
                    file_size = table.Column<decimal>(type: "NUMBER(20,0)", nullable: false),
                    mime_type = table.Column<string>(type: "VARCHAR2(100)", maxLength: 100, nullable: true),
                    created_at = table.Column<DateTime>(type: "TIMESTAMP(6)", nullable: false),
                    updated_at = table.Column<DateTime>(type: "TIMESTAMP(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_customer_documents", x => x.id);
                    table.ForeignKey(
                        name: "fk_document_customer",
                        column: x => x.customer_id,
                        principalSchema: "SSDBONLINE",
                        principalTable: "cust_mast",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_document_customer_id",
                schema: "SSDBONLINE",
                table: "customer_documents",
                column: "customer_id");

            migrationBuilder.CreateIndex(
                name: "ix_document_customer_type",
                schema: "SSDBONLINE",
                table: "customer_documents",
                columns: new[] { "customer_id", "document_type" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "customer_documents",
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

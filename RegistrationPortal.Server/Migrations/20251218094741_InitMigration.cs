using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RegistrationPortal.Server.Migrations
{
    /// <inheritdoc />
    public partial class InitMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "SSDBONLINE");

            migrationBuilder.CreateTable(
                name: "cust_mast",
                schema: "SSDBONLINE",
                columns: table => new
                {
                    id = table.Column<string>(type: "VARCHAR2(50)", maxLength: 50, nullable: false),
                    branch_c_code = table.Column<string>(type: "VARCHAR2(10)", maxLength: 10, nullable: false),
                    cust_i_id = table.Column<long>(type: "NUMBER(18,0)", nullable: false),
                    cust_c_name = table.Column<string>(type: "VARCHAR2(250)", maxLength: 250, nullable: false),
                    cust_d_entrydt = table.Column<DateTime>(type: "DATE", nullable: false),
                    cust_c_fname = table.Column<string>(type: "VARCHAR2(250)", maxLength: 250, nullable: true),
                    cust_c_sname = table.Column<string>(type: "VARCHAR2(250)", maxLength: 250, nullable: true),
                    cust_c_tname = table.Column<string>(type: "VARCHAR2(250)", maxLength: 250, nullable: true),
                    cust_c_foname = table.Column<string>(type: "VARCHAR2(250)", maxLength: 250, nullable: true),
                    cust_c_mname = table.Column<string>(type: "VARCHAR2(250)", maxLength: 250, nullable: true),
                    cust_d_bdate = table.Column<DateTime>(type: "DATE", nullable: true),
                    cust_c_sex = table.Column<string>(type: "VARCHAR2(10)", maxLength: 10, nullable: true),
                    cust_c_religion = table.Column<string>(type: "VARCHAR2(20)", maxLength: 20, nullable: true),
                    cust_c_caste = table.Column<string>(type: "VARCHAR2(50)", maxLength: 50, nullable: true),
                    cust_c_maritalsts = table.Column<string>(type: "VARCHAR2(20)", maxLength: 20, nullable: true),
                    cust_c_padd1 = table.Column<string>(type: "VARCHAR2(100)", maxLength: 100, nullable: true),
                    cust_c_pcity = table.Column<string>(type: "VARCHAR2(50)", maxLength: 50, nullable: true),
                    mobile_c_no = table.Column<string>(type: "VARCHAR2(18)", maxLength: 18, nullable: true),
                    email_c_add = table.Column<string>(type: "VARCHAR2(50)", maxLength: 50, nullable: true),
                    id_c_type = table.Column<string>(type: "VARCHAR2(50)", maxLength: 50, nullable: true),
                    id_c_no = table.Column<string>(type: "VARCHAR2(50)", maxLength: 50, nullable: true),
                    id_d_issdate = table.Column<DateTime>(type: "DATE", nullable: true),
                    id_c_issplace = table.Column<string>(type: "VARCHAR2(50)", maxLength: 50, nullable: true),
                    id_d_expdate = table.Column<DateTime>(type: "DATE", nullable: true),
                    cust_c_authority = table.Column<string>(type: "VARCHAR2(50)", maxLength: 50, nullable: true),
                    husb_c_name = table.Column<string>(type: "VARCHAR2(100)", maxLength: 100, nullable: true),
                    country_c_code = table.Column<string>(type: "VARCHAR2(50)", maxLength: 50, nullable: true),
                    place_c_birth = table.Column<string>(type: "VARCHAR2(100)", maxLength: 100, nullable: true),
                    cust_c_nationality = table.Column<string>(type: "VARCHAR2(100)", maxLength: 100, nullable: true),
                    cust_c_wife1 = table.Column<string>(type: "VARCHAR2(250)", maxLength: 250, nullable: true),
                    id_c_type2 = table.Column<string>(type: "VARCHAR2(10)", maxLength: 10, nullable: true),
                    id_c_no2 = table.Column<string>(type: "VARCHAR2(50)", maxLength: 50, nullable: true),
                    cust_c_occupation = table.Column<string>(type: "VARCHAR2(100)", maxLength: 100, nullable: true),
                    home_i_number = table.Column<long>(type: "NUMBER(18,0)", nullable: true),
                    cust_i_identify = table.Column<string>(type: "VARCHAR2(20)", maxLength: 20, nullable: true),
                    cust_c_countrybrith = table.Column<string>(type: "VARCHAR2(50)", maxLength: 50, nullable: true),
                    cust_c_statebrith = table.Column<string>(type: "VARCHAR2(10)", maxLength: 10, nullable: true),
                    cust_c_citizenship = table.Column<string>(type: "VARCHAR2(50)", maxLength: 50, nullable: true),
                    cust_c_employer = table.Column<string>(type: "VARCHAR2(50)", maxLength: 50, nullable: true),
                    cust_f_income = table.Column<decimal>(type: "NUMBER(18,2)", nullable: true),
                    cust_c_higheducation = table.Column<string>(type: "VARCHAR2(50)", maxLength: 50, nullable: true),
                    cust_f_avgmonth = table.Column<decimal>(type: "NUMBER(18,2)", nullable: true),
                    trade_c_nameenglish = table.Column<string>(type: "VARCHAR2(250)", maxLength: 250, nullable: true),
                    cust_c_engfname = table.Column<string>(type: "VARCHAR2(250)", maxLength: 250, nullable: true),
                    cust_c_engsname = table.Column<string>(type: "VARCHAR2(250)", maxLength: 250, nullable: true),
                    cust_c_engtname = table.Column<string>(type: "VARCHAR2(250)", maxLength: 250, nullable: true),
                    cust_c_engfoname = table.Column<string>(type: "VARCHAR2(250)", maxLength: 250, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_cust_mast", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "account_mast",
                schema: "SSDBONLINE",
                columns: table => new
                {
                    id = table.Column<string>(type: "NVARCHAR2(50)", maxLength: 50, nullable: false),
                    branch_c_code = table.Column<string>(type: "NVARCHAR2(10)", maxLength: 10, nullable: false),
                    act_c_type = table.Column<string>(type: "NVARCHAR2(10)", maxLength: 10, nullable: false),
                    cust_i_no = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: false),
                    currency_c_code = table.Column<string>(type: "NVARCHAR2(10)", maxLength: 10, nullable: true),
                    cust_id = table.Column<string>(type: "VARCHAR2(50)", maxLength: 50, nullable: false),
                    act_c_name = table.Column<string>(type: "NVARCHAR2(250)", maxLength: 250, nullable: true),
                    act_c_occcode = table.Column<string>(type: "NVARCHAR2(10)", maxLength: 10, nullable: true),
                    act_d_opdate = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: true),
                    act_c_orgn = table.Column<string>(type: "NVARCHAR2(10)", maxLength: 10, nullable: true),
                    act_c_actsts = table.Column<string>(type: "NVARCHAR2(50)", maxLength: 50, nullable: true),
                    act_c_odsts = table.Column<string>(type: "NVARCHAR2(50)", maxLength: 50, nullable: true),
                    act_c_chqfac = table.Column<string>(type: "NVARCHAR2(50)", maxLength: 50, nullable: true),
                    act_c_introtype = table.Column<string>(type: "NVARCHAR2(50)", maxLength: 50, nullable: true),
                    act_c_opmode = table.Column<string>(type: "NVARCHAR2(50)", maxLength: 50, nullable: true),
                    act_i_introid = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: true),
                    act_i_trbrcode = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: true),
                    act_d_closdt = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: true),
                    act_c_abbsts = table.Column<string>(type: "VARCHAR2(50)", maxLength: 50, nullable: true),
                    withdraw_c_flag = table.Column<string>(type: "VARCHAR2(50)", maxLength: 50, nullable: true),
                    intro_c_rem = table.Column<string>(type: "NVARCHAR2(100)", maxLength: 100, nullable: true),
                    act_c_atm = table.Column<string>(type: "VARCHAR2(50)", maxLength: 50, nullable: true),
                    act_c_internet = table.Column<string>(type: "VARCHAR2(50)", maxLength: 50, nullable: true),
                    act_c_telebnk = table.Column<string>(type: "VARCHAR2(50)", maxLength: 50, nullable: true),
                    group_c_code = table.Column<string>(type: "NVARCHAR2(50)", maxLength: 50, nullable: true),
                    entered_c_by = table.Column<string>(type: "NVARCHAR2(50)", maxLength: 50, nullable: true),
                    auth_c_by = table.Column<string>(type: "NVARCHAR2(50)", maxLength: 50, nullable: true),
                    close_c_reason = table.Column<string>(type: "NVARCHAR2(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_account_mast", x => x.id);
                    table.ForeignKey(
                        name: "fk_acc_cust",
                        column: x => x.cust_id,
                        principalSchema: "SSDBONLINE",
                        principalTable: "cust_mast",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_account_mast_cust_id",
                schema: "SSDBONLINE",
                table: "account_mast",
                column: "cust_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "account_mast",
                schema: "SSDBONLINE");

            migrationBuilder.DropTable(
                name: "cust_mast",
                schema: "SSDBONLINE");
        }
    }
}

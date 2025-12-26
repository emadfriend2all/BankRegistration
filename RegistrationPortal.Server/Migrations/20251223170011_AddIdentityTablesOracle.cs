using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RegistrationPortal.Server.Migrations
{
    /// <inheritdoc />
    public partial class AddIdentityTablesOracle : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "file_size",
                schema: "SSDBONLINE",
                table: "customer_documents",
                type: "NUMBER(20,0)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "NUMBER");

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

            // Create sequences (drop if they exist first)
            migrationBuilder.Sql("BEGIN EXECUTE IMMEDIATE 'DROP SEQUENCE \"SSDBONLINE\".\"PERMISSIONS_SEQ\"'; EXCEPTION WHEN OTHERS THEN NULL; END;");
            migrationBuilder.Sql("BEGIN EXECUTE IMMEDIATE 'DROP SEQUENCE \"SSDBONLINE\".\"ROLES_SEQ\"'; EXCEPTION WHEN OTHERS THEN NULL; END;");
            migrationBuilder.Sql("BEGIN EXECUTE IMMEDIATE 'DROP SEQUENCE \"SSDBONLINE\".\"USERS_SEQ\"'; EXCEPTION WHEN OTHERS THEN NULL; END;");
            migrationBuilder.Sql("BEGIN EXECUTE IMMEDIATE 'DROP SEQUENCE \"SSDBONLINE\".\"ROLE_PERMISSIONS_SEQ\"'; EXCEPTION WHEN OTHERS THEN NULL; END;");
            migrationBuilder.Sql("BEGIN EXECUTE IMMEDIATE 'DROP SEQUENCE \"SSDBONLINE\".\"USER_ROLES_SEQ\"'; EXCEPTION WHEN OTHERS THEN NULL; END;");
            
            // Drop tables if they exist (in reverse order of creation due to dependencies)
            migrationBuilder.Sql("BEGIN EXECUTE IMMEDIATE 'DROP TABLE \"SSDBONLINE\".\"USER_ROLES\" CASCADE CONSTRAINTS'; EXCEPTION WHEN OTHERS THEN NULL; END;");
            migrationBuilder.Sql("BEGIN EXECUTE IMMEDIATE 'DROP TABLE \"SSDBONLINE\".\"ROLE_PERMISSIONS\" CASCADE CONSTRAINTS'; EXCEPTION WHEN OTHERS THEN NULL; END;");
            migrationBuilder.Sql("BEGIN EXECUTE IMMEDIATE 'DROP TABLE \"SSDBONLINE\".\"USERS\" CASCADE CONSTRAINTS'; EXCEPTION WHEN OTHERS THEN NULL; END;");
            migrationBuilder.Sql("BEGIN EXECUTE IMMEDIATE 'DROP TABLE \"SSDBONLINE\".\"ROLES\" CASCADE CONSTRAINTS'; EXCEPTION WHEN OTHERS THEN NULL; END;");
            migrationBuilder.Sql("BEGIN EXECUTE IMMEDIATE 'DROP TABLE \"SSDBONLINE\".\"PERMISSIONS\" CASCADE CONSTRAINTS'; EXCEPTION WHEN OTHERS THEN NULL; END;");
            
            migrationBuilder.Sql("CREATE SEQUENCE \"SSDBONLINE\".\"PERMISSIONS_SEQ\" START WITH 1 INCREMENT BY 1 NOCACHE NOCYCLE");
            migrationBuilder.Sql("CREATE SEQUENCE \"SSDBONLINE\".\"ROLES_SEQ\" START WITH 1 INCREMENT BY 1 NOCACHE NOCYCLE");
            migrationBuilder.Sql("CREATE SEQUENCE \"SSDBONLINE\".\"USERS_SEQ\" START WITH 1 INCREMENT BY 1 NOCACHE NOCYCLE");
            migrationBuilder.Sql("CREATE SEQUENCE \"SSDBONLINE\".\"ROLE_PERMISSIONS_SEQ\" START WITH 1 INCREMENT BY 1 NOCACHE NOCYCLE");
            migrationBuilder.Sql("CREATE SEQUENCE \"SSDBONLINE\".\"USER_ROLES_SEQ\" START WITH 1 INCREMENT BY 1 NOCACHE NOCYCLE");

            migrationBuilder.CreateTable(
                name: "PERMISSIONS",
                columns: table => new
                {
                    PERMISSION_ID = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    NAME = table.Column<string>(type: "NVARCHAR2(100)", maxLength: 100, nullable: false),
                    DESCRIPTION = table.Column<string>(type: "NVARCHAR2(200)", maxLength: 200, nullable: true),
                    MODULE = table.Column<string>(type: "NVARCHAR2(50)", maxLength: 50, nullable: true),
                    ACTION = table.Column<string>(type: "NVARCHAR2(50)", maxLength: 50, nullable: true),
                    IS_ACTIVE = table.Column<bool>(type: "NUMBER(1)", nullable: false, defaultValue: true),
                    CREATED_AT = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false, defaultValueSql: "SYSDATE")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PERMISSIONS", x => x.PERMISSION_ID);
                });

            migrationBuilder.CreateTable(
                name: "ROLES",
                columns: table => new
                {
                    ROLE_ID = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    NAME = table.Column<string>(type: "NVARCHAR2(50)", maxLength: 50, nullable: false),
                    DESCRIPTION = table.Column<string>(type: "NVARCHAR2(200)", maxLength: 200, nullable: true),
                    IS_ACTIVE = table.Column<bool>(type: "NUMBER(1)", nullable: false, defaultValue: true),
                    CREATED_AT = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false, defaultValueSql: "SYSDATE"),
                    UPDATED_AT = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ROLES", x => x.ROLE_ID);
                });

            migrationBuilder.CreateTable(
                name: "USERS",
                columns: table => new
                {
                    USER_ID = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    USERNAME = table.Column<string>(type: "NVARCHAR2(50)", maxLength: 50, nullable: false),
                    EMAIL = table.Column<string>(type: "NVARCHAR2(100)", maxLength: 100, nullable: false),
                    PASSWORD_HASH = table.Column<string>(type: "NVARCHAR2(255)", maxLength: 255, nullable: false),
                    FIRST_NAME = table.Column<string>(type: "NVARCHAR2(50)", maxLength: 50, nullable: true),
                    LAST_NAME = table.Column<string>(type: "NVARCHAR2(50)", maxLength: 50, nullable: true),
                    IS_ACTIVE = table.Column<bool>(type: "NUMBER(1)", nullable: false, defaultValue: true),
                    CREATED_AT = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false, defaultValueSql: "SYSDATE"),
                    UPDATED_AT = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: true),
                    LAST_LOGIN_AT = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_USERS", x => x.USER_ID);
                });

            migrationBuilder.CreateTable(
                name: "ROLE_PERMISSIONS",
                columns: table => new
                {
                    ROLE_PERMISSION_ID = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    ROLE_ID = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    PERMISSION_ID = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    GRANTED_AT = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false, defaultValueSql: "SYSDATE"),
                    GRANTED_BY = table.Column<int>(type: "NUMBER(10)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ROLE_PERMISSIONS", x => x.ROLE_PERMISSION_ID);
                    table.ForeignKey(
                        name: "FK_ROLE_PERM_PERM",
                        column: x => x.PERMISSION_ID,
                        principalTable: "PERMISSIONS",
                        principalColumn: "PERMISSION_ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ROLE_PERM_ROLE",
                        column: x => x.ROLE_ID,
                        principalTable: "ROLES",
                        principalColumn: "ROLE_ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "USER_ROLES",
                columns: table => new
                {
                    USER_ROLE_ID = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    USER_ID = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    ROLE_ID = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    ASSIGNED_AT = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false, defaultValueSql: "SYSDATE"),
                    ASSIGNED_BY = table.Column<int>(type: "NUMBER(10)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_USER_ROLES", x => x.USER_ROLE_ID);
                    table.ForeignKey(
                        name: "FK_USER_ROLES_ROLE",
                        column: x => x.ROLE_ID,
                        principalTable: "ROLES",
                        principalColumn: "ROLE_ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_USER_ROLES_USER",
                        column: x => x.USER_ID,
                        principalTable: "USERS",
                        principalColumn: "USER_ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PERM_MOD_ACT",
                table: "PERMISSIONS",
                columns: new[] { "MODULE", "ACTION" },
                unique: true,
                filter: "MODULE IS NOT NULL AND ACTION IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_PERM_NAME",
                table: "PERMISSIONS",
                column: "NAME",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ROLE_PERM_PERM",
                table: "ROLE_PERMISSIONS",
                column: "PERMISSION_ID");

            migrationBuilder.CreateIndex(
                name: "IX_ROLE_PERM_RP",
                table: "ROLE_PERMISSIONS",
                columns: new[] { "ROLE_ID", "PERMISSION_ID" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ROLES_NAME",
                table: "ROLES",
                column: "NAME",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_USER_ROLES_ROLE",
                table: "USER_ROLES",
                column: "ROLE_ID");

            migrationBuilder.CreateIndex(
                name: "IX_USER_ROLES_UR",
                table: "USER_ROLES",
                columns: new[] { "USER_ID", "ROLE_ID" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_USERS_EMAIL",
                table: "USERS",
                column: "EMAIL",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_USERS_USERNAME",
                table: "USERS",
                column: "USERNAME",
                unique: true);

            // Create triggers for auto-increment functionality
            migrationBuilder.Sql(@"
                CREATE OR REPLACE TRIGGER ""SSDBONLINE"".""PERMISSIONS_BIR"" 
                BEFORE INSERT ON ""SSDBONLINE"".""PERMISSIONS"" 
                FOR EACH ROW 
                BEGIN 
                    IF :NEW.""PERMISSION_ID"" IS NULL THEN 
                        :NEW.""PERMISSION_ID"" := ""SSDBONLINE"".""PERMISSIONS_SEQ"".NEXTVAL; 
                    END IF; 
                END;");
            
            migrationBuilder.Sql(@"
                CREATE OR REPLACE TRIGGER ""SSDBONLINE"".""ROLES_BIR"" 
                BEFORE INSERT ON ""SSDBONLINE"".""ROLES"" 
                FOR EACH ROW 
                BEGIN 
                    IF :NEW.""ROLE_ID"" IS NULL THEN 
                        :NEW.""ROLE_ID"" := ""SSDBONLINE"".""ROLES_SEQ"".NEXTVAL; 
                    END IF; 
                END;");
            
            migrationBuilder.Sql(@"
                CREATE OR REPLACE TRIGGER ""SSDBONLINE"".""USERS_BIR"" 
                BEFORE INSERT ON ""SSDBONLINE"".""USERS"" 
                FOR EACH ROW 
                BEGIN 
                    IF :NEW.""USER_ID"" IS NULL THEN 
                        :NEW.""USER_ID"" := ""SSDBONLINE"".""USERS_SEQ"".NEXTVAL; 
                    END IF; 
                END;");
            
            migrationBuilder.Sql(@"
                CREATE OR REPLACE TRIGGER ""SSDBONLINE"".""ROLE_PERMISSIONS_BIR"" 
                BEFORE INSERT ON ""SSDBONLINE"".""ROLE_PERMISSIONS"" 
                FOR EACH ROW 
                BEGIN 
                    IF :NEW.""ROLE_PERMISSION_ID"" IS NULL THEN 
                        :NEW.""ROLE_PERMISSION_ID"" := ""SSDBONLINE"".""ROLE_PERMISSIONS_SEQ"".NEXTVAL; 
                    END IF; 
                END;");
            
            migrationBuilder.Sql(@"
                CREATE OR REPLACE TRIGGER ""SSDBONLINE"".""USER_ROLES_BIR"" 
                BEFORE INSERT ON ""SSDBONLINE"".""USER_ROLES"" 
                FOR EACH ROW 
                BEGIN 
                    IF :NEW.""USER_ROLE_ID"" IS NULL THEN 
                        :NEW.""USER_ROLE_ID"" := ""SSDBONLINE"".""USER_ROLES_SEQ"".NEXTVAL; 
                    END IF; 
                END;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Drop triggers first
            migrationBuilder.Sql("DROP TRIGGER \"SSDBONLINE\".\"PERMISSIONS_BIR\"");
            migrationBuilder.Sql("DROP TRIGGER \"SSDBONLINE\".\"ROLES_BIR\"");
            migrationBuilder.Sql("DROP TRIGGER \"SSDBONLINE\".\"USERS_BIR\"");
            migrationBuilder.Sql("DROP TRIGGER \"SSDBONLINE\".\"ROLE_PERMISSIONS_BIR\"");
            migrationBuilder.Sql("DROP TRIGGER \"SSDBONLINE\".\"USER_ROLES_BIR\"");

            migrationBuilder.DropTable(
                name: "ROLE_PERMISSIONS");

            migrationBuilder.DropTable(
                name: "USER_ROLES");

            migrationBuilder.DropTable(
                name: "PERMISSIONS");

            migrationBuilder.DropTable(
                name: "ROLES");

            migrationBuilder.DropTable(
                name: "USERS");

            // Drop sequences
            migrationBuilder.Sql("DROP SEQUENCE \"SSDBONLINE\".\"PERMISSIONS_SEQ\"");
            migrationBuilder.Sql("DROP SEQUENCE \"SSDBONLINE\".\"ROLES_SEQ\"");
            migrationBuilder.Sql("DROP SEQUENCE \"SSDBONLINE\".\"USERS_SEQ\"");
            migrationBuilder.Sql("DROP SEQUENCE \"SSDBONLINE\".\"ROLE_PERMISSIONS_SEQ\"");
            migrationBuilder.Sql("DROP SEQUENCE \"SSDBONLINE\".\"USER_ROLES_SEQ\"");

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

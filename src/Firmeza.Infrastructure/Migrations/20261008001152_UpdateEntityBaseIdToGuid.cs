using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Firmeza.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateEntityBaseIdToGuid : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP TABLE IF EXISTS sale_details CASCADE;");
            migrationBuilder.Sql("DROP TABLE IF EXISTS sales CASCADE;");
            migrationBuilder.Sql("DROP TABLE IF EXISTS trabajadores CASCADE;");
            migrationBuilder.Sql("DROP TABLE IF EXISTS products CASCADE;");
            migrationBuilder.Sql("DROP TABLE IF EXISTS employees CASCADE;");

            migrationBuilder.Sql("ALTER TABLE empresas ALTER COLUMN \"Id\" DROP DEFAULT;");
            migrationBuilder.Sql("ALTER TABLE empresas ALTER COLUMN \"Id\" TYPE uuid USING gen_random_uuid();");

            migrationBuilder.Sql("ALTER TABLE clientes ALTER COLUMN \"Id\" DROP DEFAULT;");
            migrationBuilder.Sql("ALTER TABLE clientes ALTER COLUMN \"Id\" TYPE uuid USING gen_random_uuid();");

            migrationBuilder.Sql("DROP SEQUENCE IF EXISTS \"EntityBaseSequence\" CASCADE;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateSequence(
                name: "EntityBaseSequence");

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "empresas",
                type: "integer",
                nullable: false,
                defaultValueSql: "nextval('\"EntityBaseSequence\"')",
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "clientes",
                type: "integer",
                nullable: false,
                defaultValueSql: "nextval('\"EntityBaseSequence\"')",
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.CreateTable(
                name: "products",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false, defaultValueSql: "nextval('\"EntityBaseSequence\"')"),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    EmpresaId = table.Column<int>(type: "integer", nullable: true),
                    Category = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Price = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    Stock = table.Column<int>(type: "integer", nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_products", x => x.Id);
                    table.ForeignKey(
                        name: "FK_products_empresas_EmpresaId",
                        column: x => x.EmpresaId,
                        principalTable: "empresas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "sales",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false, defaultValueSql: "nextval('\"EntityBaseSequence\"')"),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    ClienteId = table.Column<int>(type: "integer", nullable: false),
                    Date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ReceiptPath = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    SaleNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Subtotal = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Tax = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Total = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_sales", x => x.Id);
                    table.ForeignKey(
                        name: "FK_sales_clientes_ClienteId",
                        column: x => x.ClienteId,
                        principalTable: "clientes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "trabajadores",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false, defaultValueSql: "nextval('\"EntityBaseSequence\"')"),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    Address = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    DocumentNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Email = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Phone = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Position = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Salary = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_trabajadores", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "sale_details",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false, defaultValueSql: "nextval('\"EntityBaseSequence\"')"),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    ProductId = table.Column<int>(type: "integer", nullable: false),
                    SaleId = table.Column<int>(type: "integer", nullable: false),
                    LineTotal = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Quantity = table.Column<int>(type: "integer", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_sale_details", x => x.Id);
                    table.ForeignKey(
                        name: "FK_sale_details_products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_sale_details_sales_SaleId",
                        column: x => x.SaleId,
                        principalTable: "sales",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_products_EmpresaId",
                table: "products",
                column: "EmpresaId");

            migrationBuilder.CreateIndex(
                name: "IX_sale_details_ProductId",
                table: "sale_details",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_sale_details_SaleId",
                table: "sale_details",
                column: "SaleId");

            migrationBuilder.CreateIndex(
                name: "IX_sales_ClienteId",
                table: "sales",
                column: "ClienteId");

            migrationBuilder.CreateIndex(
                name: "IX_sales_SaleNumber",
                table: "sales",
                column: "SaleNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_trabajadores_DocumentNumber",
                table: "trabajadores",
                column: "DocumentNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_trabajadores_Email",
                table: "trabajadores",
                column: "Email",
                unique: true);
        }
    }
}

using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Firmeza.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RenameCustomerAndAddEmpresa : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameTable(
                name: "customers",
                newName: "clientes");

            migrationBuilder.RenameColumn(
                name: "FullName",
                table: "clientes",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "CustomerId",
                table: "sales",
                newName: "ClienteId");

            migrationBuilder.RenameIndex(
                name: "IX_sales_CustomerId",
                table: "sales",
                newName: "IX_sales_ClienteId");

            migrationBuilder.RenameIndex(
                name: "IX_customers_DocumentNumber",
                table: "clientes",
                newName: "IX_clientes_DocumentNumber");

            migrationBuilder.RenameIndex(
                name: "IX_customers_Email",
                table: "clientes",
                newName: "IX_clientes_Email");

            migrationBuilder.Sql(
                "ALTER TABLE clientes RENAME CONSTRAINT \"PK_customers\" TO \"PK_clientes\";");

            migrationBuilder.Sql(
                "ALTER TABLE sales RENAME CONSTRAINT \"FK_sales_customers_CustomerId\" TO \"FK_sales_clientes_ClienteId\";");

            migrationBuilder.AddColumn<int>(
                name: "EmpresaId",
                table: "products",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "empresas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Email = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Phone = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Address = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_empresas", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_products_EmpresaId",
                table: "products",
                column: "EmpresaId");

            migrationBuilder.AddForeignKey(
                name: "FK_products_empresas_EmpresaId",
                table: "products",
                column: "EmpresaId",
                principalTable: "empresas",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_products_empresas_EmpresaId",
                table: "products");

            migrationBuilder.DropTable(
                name: "empresas");

            migrationBuilder.DropIndex(
                name: "IX_products_EmpresaId",
                table: "products");

            migrationBuilder.DropColumn(
                name: "EmpresaId",
                table: "products");

            migrationBuilder.Sql(
                "ALTER TABLE sales RENAME CONSTRAINT \"FK_sales_clientes_ClienteId\" TO \"FK_sales_customers_CustomerId\";");

            migrationBuilder.RenameIndex(
                name: "IX_sales_ClienteId",
                table: "sales",
                newName: "IX_sales_CustomerId");

            migrationBuilder.RenameIndex(
                name: "IX_clientes_DocumentNumber",
                table: "clientes",
                newName: "IX_customers_DocumentNumber");

            migrationBuilder.RenameIndex(
                name: "IX_clientes_Email",
                table: "clientes",
                newName: "IX_customers_Email");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "clientes",
                newName: "FullName");

            migrationBuilder.Sql(
                "ALTER TABLE clientes RENAME CONSTRAINT \"PK_clientes\" TO \"PK_customers\";");

            migrationBuilder.RenameTable(
                name: "clientes",
                newName: "customers");

            migrationBuilder.RenameColumn(
                name: "ClienteId",
                table: "sales",
                newName: "CustomerId");
        }
    }
}

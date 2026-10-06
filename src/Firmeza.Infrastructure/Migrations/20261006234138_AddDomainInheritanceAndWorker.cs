using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Firmeza.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddDomainInheritanceAndWorker : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "IsAvailable",
                table: "products",
                newName: "LegacyIsAvailable");

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "products",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "products",
                type: "integer",
                nullable: true);

            migrationBuilder.Sql(
                "UPDATE products SET \"Status\" = CASE WHEN \"LegacyIsAvailable\" THEN 1 ELSE 2 END;");

            migrationBuilder.AlterColumn<int>(
                name: "Status",
                table: "products",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.DropColumn(
                name: "LegacyIsAvailable",
                table: "products");

            migrationBuilder.CreateSequence(
                name: "EntityBaseSequence");

            migrationBuilder.Sql(
                """
                DO $migration$
                BEGIN
                    IF to_regclass('public.employees') IS NOT NULL
                       AND to_regclass('public.trabajadores') IS NULL THEN
                        ALTER TABLE public.employees RENAME TO trabajadores;

                        IF EXISTS (
                            SELECT 1 FROM information_schema.columns
                            WHERE table_schema = 'public' AND table_name = 'trabajadores' AND column_name = 'FirstName'
                        ) AND NOT EXISTS (
                            SELECT 1 FROM information_schema.columns
                            WHERE table_schema = 'public' AND table_name = 'trabajadores' AND column_name = 'Name'
                        ) THEN
                            ALTER TABLE trabajadores RENAME COLUMN "FirstName" TO "Name";
                        END IF;

                        IF EXISTS (
                            SELECT 1 FROM information_schema.columns
                            WHERE table_schema = 'public' AND table_name = 'trabajadores' AND column_name = 'LastName'
                        ) THEN
                            UPDATE trabajadores
                            SET "Name" = concat_ws(' ', NULLIF(btrim("Name"), ''), NULLIF(btrim("LastName"), ''));
                        END IF;

                        IF NOT EXISTS (
                            SELECT 1 FROM information_schema.columns
                            WHERE table_schema = 'public' AND table_name = 'trabajadores' AND column_name = 'Address'
                        ) THEN
                            ALTER TABLE trabajadores ADD COLUMN "Address" character varying(250) NOT NULL DEFAULT '';
                        END IF;

                        IF NOT EXISTS (
                            SELECT 1 FROM information_schema.columns
                            WHERE table_schema = 'public' AND table_name = 'trabajadores' AND column_name = 'IsActive'
                        ) THEN
                            ALTER TABLE trabajadores ADD COLUMN "IsActive" boolean NOT NULL DEFAULT TRUE;
                        ELSE
                            ALTER TABLE trabajadores ALTER COLUMN "IsActive" SET DEFAULT TRUE;
                        END IF;

                        ALTER TABLE trabajadores ALTER COLUMN "Id" DROP IDENTITY IF EXISTS;
                        ALTER TABLE trabajadores ALTER COLUMN "Id" SET DEFAULT nextval('"EntityBaseSequence"');

                        ALTER INDEX IF EXISTS "IX_employees_DocumentNumber" RENAME TO "IX_trabajadores_DocumentNumber";
                        ALTER INDEX IF EXISTS "IX_employees_Email" RENAME TO "IX_trabajadores_Email";

                        IF EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'PK_employees') THEN
                            ALTER TABLE trabajadores RENAME CONSTRAINT "PK_employees" TO "PK_trabajadores";
                        END IF;
                    ELSIF to_regclass('public.trabajadores') IS NULL THEN
                        CREATE TABLE trabajadores (
                            "Id" integer NOT NULL DEFAULT nextval('"EntityBaseSequence"'),
                            "IsActive" boolean NOT NULL DEFAULT TRUE,
                            "DocumentNumber" character varying(50) NOT NULL,
                            "Position" character varying(100) NOT NULL,
                            "Salary" numeric(18,2) NOT NULL,
                            "Name" character varying(150) NOT NULL,
                            "Email" character varying(150) NOT NULL,
                            "Phone" character varying(30) NOT NULL DEFAULT '',
                            "Address" character varying(250) NOT NULL DEFAULT '',
                            CONSTRAINT "PK_trabajadores" PRIMARY KEY ("Id")
                        );
                    END IF;
                END
                $migration$;
                """);

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "sales",
                type: "integer",
                nullable: false,
                defaultValueSql: "nextval('\"EntityBaseSequence\"')",
                oldClrType: typeof(int),
                oldType: "integer")
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "sales",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "sale_details",
                type: "integer",
                nullable: false,
                defaultValueSql: "nextval('\"EntityBaseSequence\"')",
                oldClrType: typeof(int),
                oldType: "integer")
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "sale_details",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "products",
                type: "integer",
                nullable: false,
                defaultValueSql: "nextval('\"EntityBaseSequence\"')",
                oldClrType: typeof(int),
                oldType: "integer")
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "empresas",
                type: "integer",
                nullable: false,
                defaultValueSql: "nextval('\"EntityBaseSequence\"')",
                oldClrType: typeof(int),
                oldType: "integer")
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "empresas",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<string>(
                name: "Nit",
                table: "empresas",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "clientes",
                type: "integer",
                nullable: false,
                defaultValueSql: "nextval('\"EntityBaseSequence\"')",
                oldClrType: typeof(int),
                oldType: "integer")
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "clientes",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.CreateIndex(
                name: "IX_empresas_Nit",
                table: "empresas",
                column: "Nit",
                unique: true);

            migrationBuilder.Sql(
                "CREATE UNIQUE INDEX IF NOT EXISTS \"IX_trabajadores_DocumentNumber\" ON trabajadores (\"DocumentNumber\");");

            migrationBuilder.Sql(
                "CREATE UNIQUE INDEX IF NOT EXISTS \"IX_trabajadores_Email\" ON trabajadores (\"Email\");");

            migrationBuilder.Sql(
                "SELECT setval('\"EntityBaseSequence\"', GREATEST(COALESCE((SELECT MAX(\"Id\") FROM (SELECT \"Id\" FROM clientes UNION ALL SELECT \"Id\" FROM empresas UNION ALL SELECT \"Id\" FROM products UNION ALL SELECT \"Id\" FROM sales UNION ALL SELECT \"Id\" FROM sale_details UNION ALL SELECT \"Id\" FROM trabajadores) AS ids), 0) + 1, 1), false);");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                DO $migration$
                BEGIN
                    IF to_regclass('public.trabajadores') IS NOT NULL
                       AND to_regclass('public.employees') IS NULL THEN
                        ALTER TABLE trabajadores ALTER COLUMN "Id" DROP DEFAULT;
                        ALTER TABLE trabajadores ALTER COLUMN "Id" ADD GENERATED BY DEFAULT AS IDENTITY;
                        IF EXISTS (
                            SELECT 1 FROM information_schema.columns
                            WHERE table_schema = 'public' AND table_name = 'trabajadores' AND column_name = 'Name'
                        ) AND NOT EXISTS (
                            SELECT 1 FROM information_schema.columns
                            WHERE table_schema = 'public' AND table_name = 'trabajadores' AND column_name = 'FirstName'
                        ) THEN
                            ALTER TABLE trabajadores RENAME COLUMN "Name" TO "FirstName";
                        END IF;
                        ALTER TABLE trabajadores RENAME TO employees;
                        ALTER INDEX IF EXISTS "IX_trabajadores_DocumentNumber" RENAME TO "IX_employees_DocumentNumber";
                        ALTER INDEX IF EXISTS "IX_trabajadores_Email" RENAME TO "IX_employees_Email";
                        IF EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'PK_trabajadores') THEN
                            ALTER TABLE employees RENAME CONSTRAINT "PK_trabajadores" TO "PK_employees";
                        END IF;
                    END IF;
                END
                $migration$;
                """);

            migrationBuilder.DropIndex(
                name: "IX_empresas_Nit",
                table: "empresas");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "sales");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "sale_details");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "products");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "empresas");

            migrationBuilder.DropColumn(
                name: "Nit",
                table: "empresas");

            migrationBuilder.AddColumn<bool>(
                name: "IsAvailable",
                table: "products",
                type: "boolean",
                nullable: true);

            migrationBuilder.Sql(
                "UPDATE products SET \"IsAvailable\" = (\"Status\" = 1);");

            migrationBuilder.AlterColumn<bool>(
                name: "IsAvailable",
                table: "products",
                type: "boolean",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "sales",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer",
                oldDefaultValueSql: "nextval('\"EntityBaseSequence\"')")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "sale_details",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer",
                oldDefaultValueSql: "nextval('\"EntityBaseSequence\"')")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "products",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer",
                oldDefaultValueSql: "nextval('\"EntityBaseSequence\"')")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "empresas",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer",
                oldDefaultValueSql: "nextval('\"EntityBaseSequence\"')")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "clientes",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer",
                oldDefaultValueSql: "nextval('\"EntityBaseSequence\"')")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);
        }
    }
}

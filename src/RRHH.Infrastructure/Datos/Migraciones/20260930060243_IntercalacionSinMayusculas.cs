using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RRHH.Infrastructure.Datos.Migraciones;

/// <inheritdoc />
public partial class IntercalacionSinMayusculas : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AlterColumn<string>(
            name: "Nombre",
            table: "Pais",
            type: "nvarchar(100)",
            maxLength: 100,
            nullable: false,
            collation: "Modern_Spanish_CI_AS",
            oldClrType: typeof(string),
            oldType: "nvarchar(100)",
            oldMaxLength: 100);

        migrationBuilder.AlterColumn<string>(
            name: "CodigoIso2",
            table: "Pais",
            type: "nchar(2)",
            fixedLength: true,
            maxLength: 2,
            nullable: false,
            collation: "Modern_Spanish_CI_AS",
            oldClrType: typeof(string),
            oldType: "nchar(2)",
            oldFixedLength: true,
            oldMaxLength: 2);

        migrationBuilder.AlterColumn<string>(
            name: "Nombre",
            table: "Municipio",
            type: "nvarchar(100)",
            maxLength: 100,
            nullable: false,
            collation: "Modern_Spanish_CI_AS",
            oldClrType: typeof(string),
            oldType: "nvarchar(100)",
            oldMaxLength: 100);

        migrationBuilder.AlterColumn<string>(
            name: "Puesto",
            table: "EmpresaColaborador",
            type: "nvarchar(100)",
            maxLength: 100,
            nullable: true,
            collation: "Modern_Spanish_CI_AS",
            oldClrType: typeof(string),
            oldType: "nvarchar(100)",
            oldMaxLength: 100,
            oldNullable: true);

        migrationBuilder.AlterColumn<string>(
            name: "Telefono",
            table: "Empresa",
            type: "nvarchar(20)",
            maxLength: 20,
            nullable: false,
            collation: "Modern_Spanish_CI_AS",
            oldClrType: typeof(string),
            oldType: "nvarchar(20)",
            oldMaxLength: 20);

        migrationBuilder.AlterColumn<string>(
            name: "RazonSocial",
            table: "Empresa",
            type: "nvarchar(200)",
            maxLength: 200,
            nullable: false,
            collation: "Modern_Spanish_CI_AS",
            oldClrType: typeof(string),
            oldType: "nvarchar(200)",
            oldMaxLength: 200);

        migrationBuilder.AlterColumn<string>(
            name: "NombreComercial",
            table: "Empresa",
            type: "nvarchar(200)",
            maxLength: 200,
            nullable: false,
            collation: "Modern_Spanish_CI_AS",
            oldClrType: typeof(string),
            oldType: "nvarchar(200)",
            oldMaxLength: 200);

        migrationBuilder.AlterColumn<string>(
            name: "Nit",
            table: "Empresa",
            type: "nvarchar(20)",
            maxLength: 20,
            nullable: false,
            collation: "Modern_Spanish_CI_AS",
            oldClrType: typeof(string),
            oldType: "nvarchar(20)",
            oldMaxLength: 20);

        migrationBuilder.AlterColumn<string>(
            name: "Correo",
            table: "Empresa",
            type: "nvarchar(254)",
            maxLength: 254,
            nullable: false,
            collation: "Modern_Spanish_CI_AS",
            oldClrType: typeof(string),
            oldType: "nvarchar(254)",
            oldMaxLength: 254);

        migrationBuilder.AlterColumn<string>(
            name: "Nombre",
            table: "Departamento",
            type: "nvarchar(100)",
            maxLength: 100,
            nullable: false,
            collation: "Modern_Spanish_CI_AS",
            oldClrType: typeof(string),
            oldType: "nvarchar(100)",
            oldMaxLength: 100);

        migrationBuilder.AlterColumn<string>(
            name: "Telefono",
            table: "Colaborador",
            type: "nvarchar(20)",
            maxLength: 20,
            nullable: false,
            collation: "Modern_Spanish_CI_AS",
            oldClrType: typeof(string),
            oldType: "nvarchar(20)",
            oldMaxLength: 20);

        migrationBuilder.AlterColumn<string>(
            name: "NombreCompleto",
            table: "Colaborador",
            type: "nvarchar(200)",
            maxLength: 200,
            nullable: false,
            collation: "Modern_Spanish_CI_AS",
            oldClrType: typeof(string),
            oldType: "nvarchar(200)",
            oldMaxLength: 200);

        migrationBuilder.AlterColumn<string>(
            name: "Correo",
            table: "Colaborador",
            type: "nvarchar(254)",
            maxLength: 254,
            nullable: false,
            collation: "Modern_Spanish_CI_AS",
            oldClrType: typeof(string),
            oldType: "nvarchar(254)",
            oldMaxLength: 254);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AlterColumn<string>(
            name: "Nombre",
            table: "Pais",
            type: "nvarchar(100)",
            maxLength: 100,
            nullable: false,
            oldClrType: typeof(string),
            oldType: "nvarchar(100)",
            oldMaxLength: 100,
            oldCollation: "Modern_Spanish_CI_AS");

        migrationBuilder.AlterColumn<string>(
            name: "CodigoIso2",
            table: "Pais",
            type: "nchar(2)",
            fixedLength: true,
            maxLength: 2,
            nullable: false,
            oldClrType: typeof(string),
            oldType: "nchar(2)",
            oldFixedLength: true,
            oldMaxLength: 2,
            oldCollation: "Modern_Spanish_CI_AS");

        migrationBuilder.AlterColumn<string>(
            name: "Nombre",
            table: "Municipio",
            type: "nvarchar(100)",
            maxLength: 100,
            nullable: false,
            oldClrType: typeof(string),
            oldType: "nvarchar(100)",
            oldMaxLength: 100,
            oldCollation: "Modern_Spanish_CI_AS");

        migrationBuilder.AlterColumn<string>(
            name: "Puesto",
            table: "EmpresaColaborador",
            type: "nvarchar(100)",
            maxLength: 100,
            nullable: true,
            oldClrType: typeof(string),
            oldType: "nvarchar(100)",
            oldMaxLength: 100,
            oldNullable: true,
            oldCollation: "Modern_Spanish_CI_AS");

        migrationBuilder.AlterColumn<string>(
            name: "Telefono",
            table: "Empresa",
            type: "nvarchar(20)",
            maxLength: 20,
            nullable: false,
            oldClrType: typeof(string),
            oldType: "nvarchar(20)",
            oldMaxLength: 20,
            oldCollation: "Modern_Spanish_CI_AS");

        migrationBuilder.AlterColumn<string>(
            name: "RazonSocial",
            table: "Empresa",
            type: "nvarchar(200)",
            maxLength: 200,
            nullable: false,
            oldClrType: typeof(string),
            oldType: "nvarchar(200)",
            oldMaxLength: 200,
            oldCollation: "Modern_Spanish_CI_AS");

        migrationBuilder.AlterColumn<string>(
            name: "NombreComercial",
            table: "Empresa",
            type: "nvarchar(200)",
            maxLength: 200,
            nullable: false,
            oldClrType: typeof(string),
            oldType: "nvarchar(200)",
            oldMaxLength: 200,
            oldCollation: "Modern_Spanish_CI_AS");

        migrationBuilder.AlterColumn<string>(
            name: "Nit",
            table: "Empresa",
            type: "nvarchar(20)",
            maxLength: 20,
            nullable: false,
            oldClrType: typeof(string),
            oldType: "nvarchar(20)",
            oldMaxLength: 20,
            oldCollation: "Modern_Spanish_CI_AS");

        migrationBuilder.AlterColumn<string>(
            name: "Correo",
            table: "Empresa",
            type: "nvarchar(254)",
            maxLength: 254,
            nullable: false,
            oldClrType: typeof(string),
            oldType: "nvarchar(254)",
            oldMaxLength: 254,
            oldCollation: "Modern_Spanish_CI_AS");

        migrationBuilder.AlterColumn<string>(
            name: "Nombre",
            table: "Departamento",
            type: "nvarchar(100)",
            maxLength: 100,
            nullable: false,
            oldClrType: typeof(string),
            oldType: "nvarchar(100)",
            oldMaxLength: 100,
            oldCollation: "Modern_Spanish_CI_AS");

        migrationBuilder.AlterColumn<string>(
            name: "Telefono",
            table: "Colaborador",
            type: "nvarchar(20)",
            maxLength: 20,
            nullable: false,
            oldClrType: typeof(string),
            oldType: "nvarchar(20)",
            oldMaxLength: 20,
            oldCollation: "Modern_Spanish_CI_AS");

        migrationBuilder.AlterColumn<string>(
            name: "NombreCompleto",
            table: "Colaborador",
            type: "nvarchar(200)",
            maxLength: 200,
            nullable: false,
            oldClrType: typeof(string),
            oldType: "nvarchar(200)",
            oldMaxLength: 200,
            oldCollation: "Modern_Spanish_CI_AS");

        migrationBuilder.AlterColumn<string>(
            name: "Correo",
            table: "Colaborador",
            type: "nvarchar(254)",
            maxLength: 254,
            nullable: false,
            oldClrType: typeof(string),
            oldType: "nvarchar(254)",
            oldMaxLength: 254,
            oldCollation: "Modern_Spanish_CI_AS");
    }
}

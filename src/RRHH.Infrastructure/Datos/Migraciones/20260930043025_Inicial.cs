using System;

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace RRHH.Infrastructure.Datos.Migraciones;

/// <inheritdoc />
public partial class Inicial : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "Colaborador",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                NombreCompleto = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                FechaNacimiento = table.Column<DateOnly>(type: "date", nullable: false),
                Telefono = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                Correo = table.Column<string>(type: "nvarchar(254)", maxLength: 254, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Colaborador", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "Pais",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                Nombre = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                CodigoIso2 = table.Column<string>(type: "nchar(2)", fixedLength: true, maxLength: 2, nullable: false),
                EdadMinima = table.Column<int>(type: "int", nullable: false),
                EdadMaxima = table.Column<int>(type: "int", nullable: false),
                Regla29Febrero = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Pais", x => x.Id);
                table.CheckConstraint("CK_Pais_RangoEdad", "EdadMinima >= 0 AND EdadMinima <= EdadMaxima");
            });

        migrationBuilder.CreateTable(
            name: "Departamento",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                PaisId = table.Column<int>(type: "int", nullable: false),
                Nombre = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Departamento", x => x.Id);
                table.ForeignKey(
                    name: "FK_Departamento_Pais",
                    column: x => x.PaisId,
                    principalTable: "Pais",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "Municipio",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                DepartamentoId = table.Column<int>(type: "int", nullable: false),
                Nombre = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Municipio", x => x.Id);
                table.ForeignKey(
                    name: "FK_Municipio_Departamento",
                    column: x => x.DepartamentoId,
                    principalTable: "Departamento",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "Empresa",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                MunicipioId = table.Column<int>(type: "int", nullable: false),
                Nit = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                RazonSocial = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                NombreComercial = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                Telefono = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                Correo = table.Column<string>(type: "nvarchar(254)", maxLength: 254, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Empresa", x => x.Id);
                table.ForeignKey(
                    name: "FK_Empresa_Municipio",
                    column: x => x.MunicipioId,
                    principalTable: "Municipio",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "EmpresaColaborador",
            columns: table => new
            {
                EmpresaId = table.Column<int>(type: "int", nullable: false),
                ColaboradorId = table.Column<int>(type: "int", nullable: false),
                FechaIngreso = table.Column<DateOnly>(type: "date", nullable: false),
                Puesto = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_EmpresaColaborador", x => new { x.EmpresaId, x.ColaboradorId });
                table.ForeignKey(
                    name: "FK_EmpresaColaborador_Colaborador",
                    column: x => x.ColaboradorId,
                    principalTable: "Colaborador",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_EmpresaColaborador_Empresa",
                    column: x => x.EmpresaId,
                    principalTable: "Empresa",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.InsertData(
            table: "Pais",
            columns: new[] { "Id", "CodigoIso2", "EdadMaxima", "EdadMinima", "Nombre", "Regla29Febrero" },
            values: new object[] { 1, "GT", 100, 18, "Guatemala", "VeintiochoDeFebrero" });

        migrationBuilder.InsertData(
            table: "Departamento",
            columns: new[] { "Id", "Nombre", "PaisId" },
            values: new object[,]
            {
                { 1, "Guatemala", 1 },
                { 2, "El Progreso", 1 },
                { 3, "Sacatepéquez", 1 },
                { 4, "Chimaltenango", 1 },
                { 5, "Escuintla", 1 },
                { 6, "Santa Rosa", 1 },
                { 7, "Sololá", 1 },
                { 8, "Totonicapán", 1 },
                { 9, "Quetzaltenango", 1 },
                { 10, "Suchitepéquez", 1 },
                { 11, "Retalhuleu", 1 },
                { 12, "San Marcos", 1 },
                { 13, "Huehuetenango", 1 },
                { 14, "Quiché", 1 },
                { 15, "Baja Verapaz", 1 },
                { 16, "Alta Verapaz", 1 },
                { 17, "Petén", 1 },
                { 18, "Izabal", 1 },
                { 19, "Zacapa", 1 },
                { 20, "Chiquimula", 1 },
                { 21, "Jalapa", 1 },
                { 22, "Jutiapa", 1 }
            });

        migrationBuilder.InsertData(
            table: "Municipio",
            columns: new[] { "Id", "DepartamentoId", "Nombre" },
            values: new object[,]
            {
                { 1, 1, "Guatemala" },
                { 2, 2, "Guastatoya" },
                { 3, 3, "Antigua Guatemala" },
                { 4, 4, "Chimaltenango" },
                { 5, 5, "Escuintla" },
                { 6, 6, "Cuilapa" },
                { 7, 7, "Sololá" },
                { 8, 8, "Totonicapán" },
                { 9, 9, "Quetzaltenango" },
                { 10, 10, "Mazatenango" },
                { 11, 11, "Retalhuleu" },
                { 12, 12, "San Marcos" },
                { 13, 13, "Huehuetenango" },
                { 14, 14, "Santa Cruz del Quiché" },
                { 15, 15, "Salamá" },
                { 16, 16, "Cobán" },
                { 17, 17, "Flores" },
                { 18, 18, "Puerto Barrios" },
                { 19, 19, "Zacapa" },
                { 20, 20, "Chiquimula" },
                { 21, 21, "Jalapa" },
                { 22, 22, "Jutiapa" }
            });

        migrationBuilder.CreateIndex(
            name: "UQ_Colaborador_Correo",
            table: "Colaborador",
            column: "Correo",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "UQ_Departamento_PaisId_Nombre",
            table: "Departamento",
            columns: new[] { "PaisId", "Nombre" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_Empresa_MunicipioId",
            table: "Empresa",
            column: "MunicipioId");

        migrationBuilder.CreateIndex(
            name: "IX_Empresa_Nit",
            table: "Empresa",
            column: "Nit");

        migrationBuilder.CreateIndex(
            name: "IX_EmpresaColaborador_ColaboradorId",
            table: "EmpresaColaborador",
            column: "ColaboradorId");

        migrationBuilder.CreateIndex(
            name: "UQ_Municipio_DepartamentoId_Nombre",
            table: "Municipio",
            columns: new[] { "DepartamentoId", "Nombre" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "UQ_Pais_CodigoIso2",
            table: "Pais",
            column: "CodigoIso2",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "UQ_Pais_Nombre",
            table: "Pais",
            column: "Nombre",
            unique: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "EmpresaColaborador");

        migrationBuilder.DropTable(
            name: "Colaborador");

        migrationBuilder.DropTable(
            name: "Empresa");

        migrationBuilder.DropTable(
            name: "Municipio");

        migrationBuilder.DropTable(
            name: "Departamento");

        migrationBuilder.DropTable(
            name: "Pais");
    }
}

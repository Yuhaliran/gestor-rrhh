using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;

using RRHH.Domain.Entidades;
using RRHH.Domain.Reglas;

namespace RRHH.UnitTests.Datos;

public class ConfiguracionesTests : IDisposable
{
    private readonly BaseDatosPrueba _bd = new();

    // 1. Nombres de tablas
    [Fact]
    public void Modelo_Tablas_EstanEnSingular()
    {
        // Arrange
        var modelo = _bd.Contexto.Model;

        // Act & Assert
        Assert.Equal("Pais", modelo.FindEntityType(typeof(Pais))!.GetTableName());
        Assert.Equal("Departamento", modelo.FindEntityType(typeof(Departamento))!.GetTableName());
        Assert.Equal("Municipio", modelo.FindEntityType(typeof(Municipio))!.GetTableName());
        Assert.Equal("Empresa", modelo.FindEntityType(typeof(Empresa))!.GetTableName());
        Assert.Equal("Colaborador", modelo.FindEntityType(typeof(Colaborador))!.GetTableName());
        Assert.Equal("EmpresaColaborador", modelo.FindEntityType(typeof(EmpresaColaborador))!.GetTableName());
    }

    // 2. Nombres de restricciones PK y FK
    [Fact]
    public void Modelo_RestriccionesPkFk_TienenNombresCorrectos()
    {
        // Arrange
        var modelo = _bd.Contexto.Model;

        // Act & Assert
        var pais = modelo.FindEntityType(typeof(Pais))!;
        Assert.Equal("PK_Pais", pais.FindPrimaryKey()!.GetName());

        var depto = modelo.FindEntityType(typeof(Departamento))!;
        Assert.Equal("PK_Departamento", depto.FindPrimaryKey()!.GetName());
        Assert.Contains(depto.GetForeignKeys(), fk => fk.GetConstraintName() == "FK_Departamento_Pais");

        var muni = modelo.FindEntityType(typeof(Municipio))!;
        Assert.Equal("PK_Municipio", muni.FindPrimaryKey()!.GetName());
        Assert.Contains(muni.GetForeignKeys(), fk => fk.GetConstraintName() == "FK_Municipio_Departamento");

        var emp = modelo.FindEntityType(typeof(Empresa))!;
        Assert.Equal("PK_Empresa", emp.FindPrimaryKey()!.GetName());
        Assert.Contains(emp.GetForeignKeys(), fk => fk.GetConstraintName() == "FK_Empresa_Municipio");

        var col = modelo.FindEntityType(typeof(Colaborador))!;
        Assert.Equal("PK_Colaborador", col.FindPrimaryKey()!.GetName());

        var empCol = modelo.FindEntityType(typeof(EmpresaColaborador))!;
        Assert.Equal("PK_EmpresaColaborador", empCol.FindPrimaryKey()!.GetName());
        Assert.Contains(empCol.GetForeignKeys(), fk => fk.GetConstraintName() == "FK_EmpresaColaborador_Empresa");
        Assert.Contains(empCol.GetForeignKeys(), fk => fk.GetConstraintName() == "FK_EmpresaColaborador_Colaborador");
    }

    // 3. Nombres de restricciones únicas e índices
    [Fact]
    public void Modelo_RestriccionesUnicasEIndices_TienenNombresCorrectos()
    {
        // Arrange
        var modelo = _bd.Contexto.Model;

        // Act & Assert
        var pais = modelo.FindEntityType(typeof(Pais))!;
        Assert.Contains(pais.GetIndexes(), i => i.GetDatabaseName() == "UQ_Pais_Nombre" && i.IsUnique);
        Assert.Contains(pais.GetIndexes(), i => i.GetDatabaseName() == "UQ_Pais_CodigoIso2" && i.IsUnique);

        var depto = modelo.FindEntityType(typeof(Departamento))!;
        Assert.Contains(depto.GetIndexes(), i => i.GetDatabaseName() == "UQ_Departamento_PaisId_Nombre" && i.IsUnique);

        var muni = modelo.FindEntityType(typeof(Municipio))!;
        Assert.Contains(muni.GetIndexes(), i => i.GetDatabaseName() == "UQ_Municipio_DepartamentoId_Nombre" && i.IsUnique);

        var emp = modelo.FindEntityType(typeof(Empresa))!;
        Assert.Contains(emp.GetIndexes(), i => i.GetDatabaseName() == "IX_Empresa_Nit" && !i.IsUnique);

        var col = modelo.FindEntityType(typeof(Colaborador))!;
        Assert.Contains(col.GetIndexes(), i => i.GetDatabaseName() == "UQ_Colaborador_Correo" && i.IsUnique);
    }

    // 4. CHECK constraint
    [Fact]
    public void Modelo_Pais_TieneCheckConstraintRangoEdad()
    {
        // Arrange
        var modelo = _bd.Contexto.GetService<IDesignTimeModel>().Model;

        // Act
        var pais = modelo.FindEntityType(typeof(Pais))!;
        var checks = pais.GetCheckConstraints();

        // Assert
        Assert.Contains(checks, c => c.Name == "CK_Pais_RangoEdad");
    }

    // 5. Largos máximos
    [Fact]
    public void Modelo_Campos_TienenLargosMaximos()
    {
        // Arrange
        var modelo = _bd.Contexto.Model;

        // Act & Assert
        var pais = modelo.FindEntityType(typeof(Pais))!;
        Assert.Equal(100, pais.FindProperty("Nombre")!.GetMaxLength());
        Assert.Equal(2, pais.FindProperty("CodigoIso2")!.GetMaxLength());
        Assert.True(pais.FindProperty("CodigoIso2")!.IsFixedLength());
        Assert.Equal(20, pais.FindProperty("Regla29Febrero")!.GetMaxLength());

        var depto = modelo.FindEntityType(typeof(Departamento))!;
        Assert.Equal(100, depto.FindProperty("Nombre")!.GetMaxLength());

        var muni = modelo.FindEntityType(typeof(Municipio))!;
        Assert.Equal(100, muni.FindProperty("Nombre")!.GetMaxLength());

        var emp = modelo.FindEntityType(typeof(Empresa))!;
        Assert.Equal(20, emp.FindProperty("Nit")!.GetMaxLength());
        Assert.Equal(200, emp.FindProperty("RazonSocial")!.GetMaxLength());
        Assert.Equal(200, emp.FindProperty("NombreComercial")!.GetMaxLength());
        Assert.Equal(20, emp.FindProperty("Telefono")!.GetMaxLength());
        Assert.Equal(254, emp.FindProperty("Correo")!.GetMaxLength());

        var col = modelo.FindEntityType(typeof(Colaborador))!;
        Assert.Equal(200, col.FindProperty("NombreCompleto")!.GetMaxLength());
        Assert.Equal(20, col.FindProperty("Telefono")!.GetMaxLength());
        Assert.Equal(254, col.FindProperty("Correo")!.GetMaxLength());

        var empCol = modelo.FindEntityType(typeof(EmpresaColaborador))!;
        Assert.Equal(100, empCol.FindProperty("Puesto")!.GetMaxLength());
    }

    // 6. Campos obligatorios
    [Fact]
    public void Modelo_Campos_SonObligatoriosSalvoPuesto()
    {
        // Arrange
        var modelo = _bd.Contexto.Model;

        // Act & Assert
        var empCol = modelo.FindEntityType(typeof(EmpresaColaborador))!;
        Assert.True(empCol.FindProperty("Puesto")!.IsNullable);

        var pais = modelo.FindEntityType(typeof(Pais))!;
        Assert.False(pais.FindProperty("Nombre")!.IsNullable);
        Assert.False(pais.FindProperty("CodigoIso2")!.IsNullable);

        var depto = modelo.FindEntityType(typeof(Departamento))!;
        Assert.False(depto.FindProperty("Nombre")!.IsNullable);

        var muni = modelo.FindEntityType(typeof(Municipio))!;
        Assert.False(muni.FindProperty("Nombre")!.IsNullable);

        var emp = modelo.FindEntityType(typeof(Empresa))!;
        Assert.False(emp.FindProperty("Nit")!.IsNullable);
        Assert.False(emp.FindProperty("RazonSocial")!.IsNullable);
        Assert.False(emp.FindProperty("NombreComercial")!.IsNullable);
        Assert.False(emp.FindProperty("Telefono")!.IsNullable);
        Assert.False(emp.FindProperty("Correo")!.IsNullable);

        var col = modelo.FindEntityType(typeof(Colaborador))!;
        Assert.False(col.FindProperty("NombreCompleto")!.IsNullable);
        Assert.False(col.FindProperty("Telefono")!.IsNullable);
        Assert.False(col.FindProperty("Correo")!.IsNullable);
    }

    // 7. Regla29Febrero guardada como texto
    [Fact]
    public void Modelo_Regla29Febrero_SeGuardaComoTexto()
    {
        // Arrange
        var pais = _bd.Contexto.Model.FindEntityType(typeof(Pais))!;

        // Act
        var property = pais.FindProperty("Regla29Febrero")!;

        // Assert
        Assert.Equal(typeof(string), property.GetProviderClrType());
    }

    public void Dispose() => _bd.Dispose();
}

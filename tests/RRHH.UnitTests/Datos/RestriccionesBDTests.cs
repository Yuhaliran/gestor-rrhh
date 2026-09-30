using Microsoft.EntityFrameworkCore;

using RRHH.Domain.Entidades;

namespace RRHH.UnitTests.Datos;

public class RestriccionesBDTests : IDisposable
{
    private readonly BaseDatosPrueba _bd = new();

    // -- 1. Únicos --

    [Fact]
    public async Task Insertar_PaisMismoNombre_LanzaExcepcion()
    {
        _bd.Contexto.Paises.Add(new Pais { Nombre = "Narnia", CodigoIso2 = "N1" });
        await _bd.Contexto.SaveChangesAsync(TestContext.Current.CancellationToken);
        _bd.Contexto.ChangeTracker.Clear();

        _bd.Contexto.Paises.Add(new Pais { Nombre = "Narnia", CodigoIso2 = "N2" });
        await Assert.ThrowsAsync<DbUpdateException>(() => _bd.Contexto.SaveChangesAsync(TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task Insertar_PaisMismoCodigo_LanzaExcepcion()
    {
        _bd.Contexto.Paises.Add(new Pais { Nombre = "Narnia", CodigoIso2 = "N1" });
        await _bd.Contexto.SaveChangesAsync(TestContext.Current.CancellationToken);
        _bd.Contexto.ChangeTracker.Clear();

        _bd.Contexto.Paises.Add(new Pais { Nombre = "Gondor", CodigoIso2 = "N1" });
        await Assert.ThrowsAsync<DbUpdateException>(() => _bd.Contexto.SaveChangesAsync(TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task Insertar_DepartamentoMismoNombreEnPais_LanzaExcepcion()
    {
        var pais = new Pais { Nombre = "Narnia", CodigoIso2 = "N1" };
        pais.Departamentos.Add(new Departamento { Nombre = "Norte" });
        _bd.Contexto.Paises.Add(pais);
        await _bd.Contexto.SaveChangesAsync(TestContext.Current.CancellationToken);
        _bd.Contexto.ChangeTracker.Clear();

        _bd.Contexto.Departamentos.Add(new Departamento { PaisId = pais.Id, Nombre = "Norte" });
        await Assert.ThrowsAsync<DbUpdateException>(() => _bd.Contexto.SaveChangesAsync(TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task Insertar_DepartamentoMismoNombreEnOtroPais_Permitido()
    {
        var pais1 = new Pais { Nombre = "Narnia", CodigoIso2 = "N1" };
        pais1.Departamentos.Add(new Departamento { Nombre = "Norte" });
        var pais2 = new Pais { Nombre = "Gondor", CodigoIso2 = "G1" };
        _bd.Contexto.Paises.AddRange(pais1, pais2);
        await _bd.Contexto.SaveChangesAsync(TestContext.Current.CancellationToken);
        _bd.Contexto.ChangeTracker.Clear();

        _bd.Contexto.Departamentos.Add(new Departamento { PaisId = pais2.Id, Nombre = "Norte" });
        var ex = await Record.ExceptionAsync(() => _bd.Contexto.SaveChangesAsync(TestContext.Current.CancellationToken));
        Assert.Null(ex);
    }

    [Fact]
    public async Task Insertar_MunicipioMismoNombreEnDepartamento_LanzaExcepcion()
    {
        var pais = new Pais { Nombre = "Narnia", CodigoIso2 = "N1" };
        var depto = new Departamento { Nombre = "Norte" };
        depto.Municipios.Add(new Municipio { Nombre = "Capital" });
        pais.Departamentos.Add(depto);
        _bd.Contexto.Paises.Add(pais);
        await _bd.Contexto.SaveChangesAsync(TestContext.Current.CancellationToken);
        _bd.Contexto.ChangeTracker.Clear();

        _bd.Contexto.Municipios.Add(new Municipio { DepartamentoId = depto.Id, Nombre = "Capital" });
        await Assert.ThrowsAsync<DbUpdateException>(() => _bd.Contexto.SaveChangesAsync(TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task Insertar_ColaboradorMismoCorreo_LanzaExcepcion()
    {
        _bd.Contexto.Colaboradores.Add(new Colaborador { NombreCompleto = "A", Correo = "test@test.com", Telefono = "1", FechaNacimiento = new DateOnly(2000, 1, 1) });
        await _bd.Contexto.SaveChangesAsync(TestContext.Current.CancellationToken);
        _bd.Contexto.ChangeTracker.Clear();

        _bd.Contexto.Colaboradores.Add(new Colaborador { NombreCompleto = "B", Correo = "test@test.com", Telefono = "2", FechaNacimiento = new DateOnly(2000, 1, 1) });
        await Assert.ThrowsAsync<DbUpdateException>(() => _bd.Contexto.SaveChangesAsync(TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task Insertar_EmpresaMismoNit_Permitido()
    {
        var pais = new Pais { Nombre = "Narnia", CodigoIso2 = "N1" };
        var depto = new Departamento { Nombre = "Norte" };
        var muni = new Municipio { Nombre = "Capital" };
        depto.Municipios.Add(muni);
        pais.Departamentos.Add(depto);
        _bd.Contexto.Paises.Add(pais);
        await _bd.Contexto.SaveChangesAsync(TestContext.Current.CancellationToken);
        _bd.Contexto.ChangeTracker.Clear();

        var emp1 = new Empresa { MunicipioId = muni.Id, Nit = "123", RazonSocial = "A", NombreComercial = "A", Correo = "a@a.com", Telefono = "1" };
        _bd.Contexto.Empresas.Add(emp1);
        await _bd.Contexto.SaveChangesAsync(TestContext.Current.CancellationToken);
        _bd.Contexto.ChangeTracker.Clear();

        var emp2 = new Empresa { MunicipioId = muni.Id, Nit = "123", RazonSocial = "B", NombreComercial = "B", Correo = "b@b.com", Telefono = "2" };
        _bd.Contexto.Empresas.Add(emp2);
        var ex = await Record.ExceptionAsync(() => _bd.Contexto.SaveChangesAsync(TestContext.Current.CancellationToken));
        Assert.Null(ex);
    }

    // -- 2. Borrado --

    [Fact]
    public async Task Eliminar_PaisConDepartamentos_LanzaExcepcion()
    {
        var pais = new Pais { Nombre = "Narnia", CodigoIso2 = "N1" };
        pais.Departamentos.Add(new Departamento { Nombre = "Norte" });
        _bd.Contexto.Paises.Add(pais);
        await _bd.Contexto.SaveChangesAsync(TestContext.Current.CancellationToken);
        _bd.Contexto.ChangeTracker.Clear();

        var p = await _bd.Contexto.Paises.FirstAsync(TestContext.Current.CancellationToken);
        _bd.Contexto.Paises.Remove(p);
        await Assert.ThrowsAsync<DbUpdateException>(() => _bd.Contexto.SaveChangesAsync(TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task Eliminar_DepartamentoConMunicipios_LanzaExcepcion()
    {
        var pais = new Pais { Nombre = "Narnia", CodigoIso2 = "N1" };
        var depto = new Departamento { Nombre = "Norte" };
        depto.Municipios.Add(new Municipio { Nombre = "Capital" });
        pais.Departamentos.Add(depto);
        _bd.Contexto.Paises.Add(pais);
        await _bd.Contexto.SaveChangesAsync(TestContext.Current.CancellationToken);
        _bd.Contexto.ChangeTracker.Clear();

        var d = await _bd.Contexto.Departamentos.FirstAsync(TestContext.Current.CancellationToken);
        _bd.Contexto.Departamentos.Remove(d);
        await Assert.ThrowsAsync<DbUpdateException>(() => _bd.Contexto.SaveChangesAsync(TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task Eliminar_MunicipioConEmpresas_LanzaExcepcion()
    {
        var pais = new Pais { Nombre = "Narnia", CodigoIso2 = "N1" };
        var depto = new Departamento { Nombre = "Norte" };
        var muni = new Municipio { Nombre = "Capital" };
        depto.Municipios.Add(muni);
        pais.Departamentos.Add(depto);
        muni.Empresas.Add(new Empresa { Nit = "1", RazonSocial = "A", NombreComercial = "A", Correo = "a@a.com", Telefono = "1" });
        _bd.Contexto.Paises.Add(pais);
        await _bd.Contexto.SaveChangesAsync(TestContext.Current.CancellationToken);
        _bd.Contexto.ChangeTracker.Clear();

        var m = await _bd.Contexto.Municipios.FirstAsync(TestContext.Current.CancellationToken);
        _bd.Contexto.Municipios.Remove(m);
        await Assert.ThrowsAsync<DbUpdateException>(() => _bd.Contexto.SaveChangesAsync(TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task Eliminar_EmpresaConColaboradores_LanzaExcepcion()
    {
        var pais = new Pais { Nombre = "Narnia", CodigoIso2 = "N1" };
        var depto = new Departamento { Nombre = "Norte" };
        var muni = new Municipio { Nombre = "Capital" };
        depto.Municipios.Add(muni);
        pais.Departamentos.Add(depto);
        var emp = new Empresa { Nit = "1", RazonSocial = "A", NombreComercial = "A", Correo = "a@a.com", Telefono = "1" };
        muni.Empresas.Add(emp);
        var col = new Colaborador { NombreCompleto = "C", Correo = "c@c.com", Telefono = "1", FechaNacimiento = new DateOnly(2000, 1, 1) };
        _bd.Contexto.Paises.Add(pais);
        _bd.Contexto.Colaboradores.Add(col);
        await _bd.Contexto.SaveChangesAsync(TestContext.Current.CancellationToken);
        _bd.Contexto.ChangeTracker.Clear();

        _bd.Contexto.EmpresasColaboradores.Add(new EmpresaColaborador { EmpresaId = emp.Id, ColaboradorId = col.Id, FechaIngreso = new DateOnly(2020, 1, 1) });
        await _bd.Contexto.SaveChangesAsync(TestContext.Current.CancellationToken);
        _bd.Contexto.ChangeTracker.Clear();

        var e = await _bd.Contexto.Empresas.FirstAsync(TestContext.Current.CancellationToken);
        _bd.Contexto.Empresas.Remove(e);
        await Assert.ThrowsAsync<DbUpdateException>(() => _bd.Contexto.SaveChangesAsync(TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task Eliminar_Colaborador_BorraRelacionEnCascada()
    {
        var pais = new Pais { Nombre = "Narnia", CodigoIso2 = "N1" };
        var depto = new Departamento { Nombre = "Norte" };
        var muni = new Municipio { Nombre = "Capital" };
        depto.Municipios.Add(muni);
        pais.Departamentos.Add(depto);
        var emp = new Empresa { Nit = "1", RazonSocial = "A", NombreComercial = "A", Correo = "a@a.com", Telefono = "1" };
        muni.Empresas.Add(emp);
        var col = new Colaborador { NombreCompleto = "C", Correo = "c@c.com", Telefono = "1", FechaNacimiento = new DateOnly(2000, 1, 1) };
        _bd.Contexto.Paises.Add(pais);
        _bd.Contexto.Colaboradores.Add(col);
        await _bd.Contexto.SaveChangesAsync(TestContext.Current.CancellationToken);

        _bd.Contexto.EmpresasColaboradores.Add(new EmpresaColaborador { EmpresaId = emp.Id, ColaboradorId = col.Id, FechaIngreso = new DateOnly(2020, 1, 1) });
        await _bd.Contexto.SaveChangesAsync(TestContext.Current.CancellationToken);
        _bd.Contexto.ChangeTracker.Clear();

        var c = await _bd.Contexto.Colaboradores.FirstAsync(TestContext.Current.CancellationToken);
        _bd.Contexto.Colaboradores.Remove(c);
        await _bd.Contexto.SaveChangesAsync(TestContext.Current.CancellationToken);
        _bd.Contexto.ChangeTracker.Clear();

        var relaciones = await _bd.Contexto.EmpresasColaboradores.ToListAsync(TestContext.Current.CancellationToken);
        Assert.Empty(relaciones);
    }

    public void Dispose() => _bd.Dispose();
}

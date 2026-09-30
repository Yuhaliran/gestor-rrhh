using RRHH.Application.Excepciones;
using RRHH.Application.Servicios;
using RRHH.Contratos.Comun;
using RRHH.Contratos.Empresas;
using RRHH.Domain.Entidades;
using RRHH.Domain.Reglas;
using RRHH.UnitTests.Datos;

using Xunit;

namespace RRHH.UnitTests.Application;

public class EmpresasServicioTests : IDisposable
{
    private readonly BaseDatosPrueba _bd;
    private readonly EmpresasServicio _servicio;

    public EmpresasServicioTests()
    {
        _bd = new BaseDatosPrueba();
        _servicio = new EmpresasServicio(_bd.Contexto);
    }

    public void Dispose()
    {
        _bd.Dispose();
        GC.SuppressFinalize(this);
    }

    private static GuardarEmpresaDto DtoBase(int municipioId) => new()
    {
        MunicipioId = municipioId,
        Nit = "123456",
        RazonSocial = "Razon Social",
        NombreComercial = "Nombre Comercial",
        Telefono = "5555-1234",
        Correo = "test@empresa.com"
    };

    private async Task<Pais> CrearPaisPruebaAsync(string nombre, string codigo)
    {
        var pais = new Pais
        {
            Nombre = nombre,
            CodigoIso2 = codigo,
            EdadMinima = 18,
            EdadMaxima = 100,
            Regla29Febrero = Regla29Febrero.VeintiochoDeFebrero
        };
        _bd.Contexto.Set<Pais>().Add(pais);
        await _bd.Contexto.SaveChangesAsync(TestContext.Current.CancellationToken);
        return pais;
    }

    private async Task<Departamento> CrearDepartamentoPruebaAsync(Pais pais, string nombre)
    {
        var depto = new Departamento
        {
            PaisId = pais.Id,
            Nombre = nombre
        };
        _bd.Contexto.Set<Departamento>().Add(depto);
        await _bd.Contexto.SaveChangesAsync(TestContext.Current.CancellationToken);
        return depto;
    }

    private async Task<Municipio> CrearMunicipioPruebaAsync(Departamento depto, string nombre)
    {
        var muni = new Municipio
        {
            DepartamentoId = depto.Id,
            Nombre = nombre
        };
        _bd.Contexto.Set<Municipio>().Add(muni);
        await _bd.Contexto.SaveChangesAsync(TestContext.Current.CancellationToken);
        return muni;
    }

    private async Task<Colaborador> CrearColaboradorPruebaAsync(string correo)
    {
        var col = new Colaborador
        {
            NombreCompleto = "Colaborador Prueba",
            FechaNacimiento = new DateOnly(1990, 1, 1),
            Telefono = "12345678",
            Correo = correo
        };
        _bd.Contexto.Set<Colaborador>().Add(col);
        await _bd.Contexto.SaveChangesAsync(TestContext.Current.CancellationToken);
        return col;
    }

    [Fact]
    public async Task ListarAsync_SinBusqueda_OrdenadoPorNombreComercial()
    {
        // Arrange
        var p = await CrearPaisPruebaAsync("Pais L", "PL");
        var d = await CrearDepartamentoPruebaAsync(p, "Depto L");
        var m = await CrearMunicipioPruebaAsync(d, "Muni L");

        await _servicio.CrearAsync(DtoBase(m.Id) with { NombreComercial = "Empresa Z", Nit = "111" }, TestContext.Current.CancellationToken);
        await _servicio.CrearAsync(DtoBase(m.Id) with { NombreComercial = "Empresa A", Nit = "222" }, TestContext.Current.CancellationToken);
        await _servicio.CrearAsync(DtoBase(m.Id) with { NombreComercial = "Empresa M", Nit = "333" }, TestContext.Current.CancellationToken);

        var pagina = await _servicio.ListarAsync(new Consulta { Pagina = 1, Tamanio = 100 }, TestContext.Current.CancellationToken);
        // Act
        var res = pagina.Elementos.Where(e => e.MunicipioId == m.Id).ToList();
        // Assert
        Assert.Equal(3, res.Count);
        Assert.Equal("Empresa A", res[0].NombreComercial);
        Assert.Equal("Empresa M", res[1].NombreComercial);
        Assert.Equal("Empresa Z", res[2].NombreComercial);
    }

    [Fact]
    public async Task ListarAsync_BuscarPorNit_NoDistingueMayusculas()
    {
        // Arrange
        var p = await CrearPaisPruebaAsync("Pais B NIT", "BN");
        var d = await CrearDepartamentoPruebaAsync(p, "Depto B NIT");
        var m = await CrearMunicipioPruebaAsync(d, "Muni B NIT");

        await _servicio.CrearAsync(DtoBase(m.Id) with { Nit = "NIT123A" }, TestContext.Current.CancellationToken);
        await _servicio.CrearAsync(DtoBase(m.Id) with { Nit = "OTRO" }, TestContext.Current.CancellationToken);

        var pagina = await _servicio.ListarAsync(new Consulta { Buscar = "nit123a" }, TestContext.Current.CancellationToken);
        // Act
        var res = pagina.Elementos.Where(e => e.MunicipioId == m.Id).ToList();
        // Assert
        Assert.Single(res);
        Assert.Equal("NIT123A", res[0].Nit);
    }

    [Fact]
    public async Task ListarAsync_BuscarPorRazonSocial_NoDistingueMayusculas()
    {
        // Arrange
        var p = await CrearPaisPruebaAsync("Pais B RS", "BR");
        var d = await CrearDepartamentoPruebaAsync(p, "Depto B RS");
        var m = await CrearMunicipioPruebaAsync(d, "Muni B RS");

        await _servicio.CrearAsync(DtoBase(m.Id) with { Nit = "R1", RazonSocial = "Razon Especial" }, TestContext.Current.CancellationToken);
        await _servicio.CrearAsync(DtoBase(m.Id) with { Nit = "R2", RazonSocial = "Otra" }, TestContext.Current.CancellationToken);

        var pagina = await _servicio.ListarAsync(new Consulta { Buscar = "ESPECIAL" }, TestContext.Current.CancellationToken);
        // Act
        var res = pagina.Elementos.Where(e => e.MunicipioId == m.Id).ToList();
        // Assert
        Assert.Single(res);
        Assert.Equal("Razon Especial", res[0].RazonSocial);
    }

    [Fact]
    public async Task ListarAsync_BuscarPorNombreComercial_NoDistingueMayusculas()
    {
        // Arrange
        var p = await CrearPaisPruebaAsync("Pais B NC", "BC");
        var d = await CrearDepartamentoPruebaAsync(p, "Depto B NC");
        var m = await CrearMunicipioPruebaAsync(d, "Muni B NC");

        await _servicio.CrearAsync(DtoBase(m.Id) with { Nit = "N1", NombreComercial = "Comercial Alfa" }, TestContext.Current.CancellationToken);
        await _servicio.CrearAsync(DtoBase(m.Id) with { Nit = "N2", NombreComercial = "Beta" }, TestContext.Current.CancellationToken);

        var pagina = await _servicio.ListarAsync(new Consulta { Buscar = "alfa" }, TestContext.Current.CancellationToken);
        // Act
        var res = pagina.Elementos.Where(e => e.MunicipioId == m.Id).ToList();
        // Assert
        Assert.Single(res);
        Assert.Equal("Comercial Alfa", res[0].NombreComercial);
    }

    [Fact]
    public async Task ObtenerAsync_Existente_DevuelveDatosConGeografiaCompleta()
    {
        // Arrange
        var p = await CrearPaisPruebaAsync("Pais Obt", "OB");
        var d = await CrearDepartamentoPruebaAsync(p, "Depto Obt");
        var m = await CrearMunicipioPruebaAsync(d, "Muni Obt");

        var creado = await _servicio.CrearAsync(DtoBase(m.Id) with { Nit = "OBT123", RazonSocial = "RS O", NombreComercial = "NC O", Telefono = "1111-1111", Correo = "o@o.com" }, TestContext.Current.CancellationToken);
        // Act
        var obtenido = await _servicio.ObtenerAsync(creado.Id, TestContext.Current.CancellationToken);
        // Assert
        Assert.Equal(creado.Id, obtenido.Id);
        Assert.Equal("OBT123", obtenido.Nit);
        Assert.Equal("RS O", obtenido.RazonSocial);
        Assert.Equal("NC O", obtenido.NombreComercial);
        Assert.Equal("1111-1111", obtenido.Telefono);
        Assert.Equal("o@o.com", obtenido.Correo);
        Assert.Equal(m.Id, obtenido.MunicipioId);
        Assert.Equal("Muni Obt", obtenido.MunicipioNombre);
        Assert.Equal(d.Id, obtenido.DepartamentoId);
        Assert.Equal("Depto Obt", obtenido.DepartamentoNombre);
        Assert.Equal(p.Id, obtenido.PaisId);
        Assert.Equal("Pais Obt", obtenido.PaisNombre);
    }

    [Fact]
    public async Task ObtenerAsync_Inexistente_LanzaNoEncontradoException()
    {
        // Arrange
        // Act & Assert
        await Assert.ThrowsAsync<NoEncontradoException>(() => _servicio.ObtenerAsync(9999, TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task CrearAsync_MunicipioInexistente_LanzaValidacionExceptionEnMunicipioId()
    {
        // Arrange
        // Act
        var ex = await Assert.ThrowsAsync<ValidacionException>(() => _servicio.CrearAsync(DtoBase(9999), TestContext.Current.CancellationToken));
        // Assert
        Assert.Equal("MunicipioId", ex.Campo);
    }

    [Fact]
    public async Task CrearAsync_NitRepetidoEnOtroDepartamentoDelMismoPais_LanzaConflictoException()
    {
        // Arrange
        var p = await CrearPaisPruebaAsync("Pais C 409", "C9");
        var d1 = await CrearDepartamentoPruebaAsync(p, "Depto 1");
        var d2 = await CrearDepartamentoPruebaAsync(p, "Depto 2");
        var m1 = await CrearMunicipioPruebaAsync(d1, "Muni 1");
        var m2 = await CrearMunicipioPruebaAsync(d2, "Muni 2");

        await _servicio.CrearAsync(DtoBase(m1.Id) with { Nit = "REPETIDO" }, TestContext.Current.CancellationToken);
        // Act & Assert
        await Assert.ThrowsAsync<ConflictoException>(() => _servicio.CrearAsync(DtoBase(m2.Id) with { Nit = "REPETIDO" }, TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task CrearAsync_NitRepetidoDiferentesMayusculasYEspacios_LanzaConflictoException()
    {
        // Arrange
        var p = await CrearPaisPruebaAsync("Pais K", "PK");
        var d = await CrearDepartamentoPruebaAsync(p, "Depto K");
        var m = await CrearMunicipioPruebaAsync(d, "Muni K");

        await _servicio.CrearAsync(DtoBase(m.Id) with { Nit = "NIT123k" }, TestContext.Current.CancellationToken);
        // Act & Assert
        await Assert.ThrowsAsync<ConflictoException>(() => _servicio.CrearAsync(DtoBase(m.Id) with { Nit = "  NIT123K " }, TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task CrearAsync_MismoNitEnOtroPais_Permitido()
    {
        // Arrange
        var p1 = await CrearPaisPruebaAsync("Pais P1", "P1");
        var p2 = await CrearPaisPruebaAsync("Pais P2", "P2");
        var d1 = await CrearDepartamentoPruebaAsync(p1, "Depto P1");
        var d2 = await CrearDepartamentoPruebaAsync(p2, "Depto P2");
        var m1 = await CrearMunicipioPruebaAsync(d1, "Muni P1");
        var m2 = await CrearMunicipioPruebaAsync(d2, "Muni P2");

        await _servicio.CrearAsync(DtoBase(m1.Id) with { Nit = "GLOBAL" }, TestContext.Current.CancellationToken);
        // Act
        var creado = await _servicio.CrearAsync(DtoBase(m2.Id) with { Nit = "GLOBAL" }, TestContext.Current.CancellationToken);
        // Assert
        Assert.Equal("GLOBAL", creado.Nit);
    }

    [Fact]
    public async Task CrearAsync_GuardaTextosSinEspaciosYNitEnMayusculas()
    {
        // Arrange
        var p = await CrearPaisPruebaAsync("Pais T", "PT");
        var d = await CrearDepartamentoPruebaAsync(p, "Depto T");
        var m = await CrearMunicipioPruebaAsync(d, "Muni T");

        var dto = DtoBase(m.Id) with
        {
            Nit = " nitmin ",
            RazonSocial = "  Razon  ",
            NombreComercial = "  Comercial  ",
            Correo = "  correo@c.com  ",
            Telefono = "  5555-1234  "
        };

        var creado = await _servicio.CrearAsync(dto, TestContext.Current.CancellationToken);
        // Act
        var obtenido = await _servicio.ObtenerAsync(creado.Id, TestContext.Current.CancellationToken);
        // Assert
        Assert.Equal("NITMIN", obtenido.Nit);
        Assert.Equal("Razon", obtenido.RazonSocial);
        Assert.Equal("Comercial", obtenido.NombreComercial);
        Assert.Equal("correo@c.com", obtenido.Correo); // Correo no se definen mayúsculas, solo Trim
        Assert.Equal("5555-1234", obtenido.Telefono);
    }

    [Fact]
    public async Task ActualizarAsync_Inexistente_LanzaNoEncontradoException()
    {
        // Arrange
        var p = await CrearPaisPruebaAsync("Pais 404 A", "AA");
        var d = await CrearDepartamentoPruebaAsync(p, "Depto 404");
        var m = await CrearMunicipioPruebaAsync(d, "Muni 404");
        // Act & Assert
        await Assert.ThrowsAsync<NoEncontradoException>(() => _servicio.ActualizarAsync(9999, DtoBase(m.Id), TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task ActualizarAsync_EmpresaInexistenteYMunicipioInexistente_LanzaNoEncontradoExceptionAntesQue400()
    {
        // Arrange
        // Act & Assert
        await Assert.ThrowsAsync<NoEncontradoException>(() => _servicio.ActualizarAsync(9999, DtoBase(9999), TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task ActualizarAsync_MunicipioInexistente_LanzaValidacionExceptionEnMunicipioId()
    {
        // Arrange
        var p = await CrearPaisPruebaAsync("Pais 400 M", "AM");
        var d = await CrearDepartamentoPruebaAsync(p, "Depto 400");
        var m = await CrearMunicipioPruebaAsync(d, "Muni 400");
        var creado = await _servicio.CrearAsync(DtoBase(m.Id) with { Nit = "AM1" }, TestContext.Current.CancellationToken);

        // Act
        var ex = await Assert.ThrowsAsync<ValidacionException>(() => _servicio.ActualizarAsync(creado.Id, DtoBase(9999), TestContext.Current.CancellationToken));
        // Assert
        Assert.Equal("MunicipioId", ex.Campo);
    }

    [Fact]
    public async Task ActualizarAsync_MunicipioDeOtroPais_LanzaValidacionExceptionEnMunicipioIdSinCambiarNada()
    {
        // Arrange
        var p1 = await CrearPaisPruebaAsync("Pais 400 O1", "O1");
        var p2 = await CrearPaisPruebaAsync("Pais 400 O2", "O2");
        var d1 = await CrearDepartamentoPruebaAsync(p1, "Depto O1");
        var d2 = await CrearDepartamentoPruebaAsync(p2, "Depto O2");
        var m1 = await CrearMunicipioPruebaAsync(d1, "Muni O1");
        var m2 = await CrearMunicipioPruebaAsync(d2, "Muni O2");

        var creado = await _servicio.CrearAsync(DtoBase(m1.Id) with { Nit = "NITO1" }, TestContext.Current.CancellationToken);

        // Act
        var ex = await Assert.ThrowsAsync<ValidacionException>(() => _servicio.ActualizarAsync(creado.Id, DtoBase(m2.Id) with { Nit = "NITO2" }, TestContext.Current.CancellationToken));
        // Assert
        Assert.Equal("MunicipioId", ex.Campo);

        var obtenido = await _servicio.ObtenerAsync(creado.Id, TestContext.Current.CancellationToken);
        Assert.Equal(m1.Id, obtenido.MunicipioId);
        Assert.Equal("NITO1", obtenido.Nit);
    }

    [Fact]
    public async Task ActualizarAsync_MismoPaisOtroDepartamento_FuncionaYDevuelveGeografiaNueva()
    {
        // Arrange
        var p = await CrearPaisPruebaAsync("Pais Mismo", "PM");
        var d1 = await CrearDepartamentoPruebaAsync(p, "Depto PM 1");
        var d2 = await CrearDepartamentoPruebaAsync(p, "Depto PM 2");
        var m1 = await CrearMunicipioPruebaAsync(d1, "Muni PM 1");
        var m2 = await CrearMunicipioPruebaAsync(d2, "Muni PM 2");

        var creado = await _servicio.CrearAsync(DtoBase(m1.Id) with { Nit = "N1" }, TestContext.Current.CancellationToken);

        // Act
        var actualizado = await _servicio.ActualizarAsync(creado.Id, DtoBase(m2.Id) with { Nit = "N1" }, TestContext.Current.CancellationToken);
        // Assert
        Assert.Equal(m2.Id, actualizado.MunicipioId);
        Assert.Equal("Muni PM 2", actualizado.MunicipioNombre);
        Assert.Equal(d2.Id, actualizado.DepartamentoId);
        Assert.Equal("Depto PM 2", actualizado.DepartamentoNombre);

        var obtenido = await _servicio.ObtenerAsync(creado.Id, TestContext.Current.CancellationToken);
        Assert.Equal(m2.Id, obtenido.MunicipioId);
        Assert.Equal("Muni PM 2", obtenido.MunicipioNombre);
        Assert.Equal(d2.Id, obtenido.DepartamentoId);
        Assert.Equal("Depto PM 2", obtenido.DepartamentoNombre);
    }

    [Fact]
    public async Task ActualizarAsync_PropioNitPermitido()
    {
        // Arrange
        var p = await CrearPaisPruebaAsync("Pais Nit", "PN");
        var d = await CrearDepartamentoPruebaAsync(p, "Depto Nit");
        var m = await CrearMunicipioPruebaAsync(d, "Muni Nit");

        var emp1 = await _servicio.CrearAsync(DtoBase(m.Id) with { Nit = "E1" }, TestContext.Current.CancellationToken);
        var emp2 = await _servicio.CrearAsync(DtoBase(m.Id) with { Nit = "E2" }, TestContext.Current.CancellationToken);

        // Act
        var act = await _servicio.ActualizarAsync(emp1.Id, DtoBase(m.Id) with { Nit = "E1", RazonSocial = "Cambio" }, TestContext.Current.CancellationToken);
        // Assert
        Assert.Equal("Cambio", act.RazonSocial);
    }

    [Fact]
    public async Task ActualizarAsync_NitDeOtraEmpresa_LanzaConflictoException()
    {
        // Arrange
        var p = await CrearPaisPruebaAsync("Pais Nit 2", "P2");
        var d = await CrearDepartamentoPruebaAsync(p, "Depto Nit 2");
        var m = await CrearMunicipioPruebaAsync(d, "Muni Nit 2");

        var emp1 = await _servicio.CrearAsync(DtoBase(m.Id) with { Nit = "E1" }, TestContext.Current.CancellationToken);
        var emp2 = await _servicio.CrearAsync(DtoBase(m.Id) with { Nit = "E2" }, TestContext.Current.CancellationToken);
        // Act & Assert
        await Assert.ThrowsAsync<ConflictoException>(() => _servicio.ActualizarAsync(emp1.Id, DtoBase(m.Id) with { Nit = "E2" }, TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task ActualizarAsync_MudarAOtroPaisDondeNitYaExiste_LanzaValidacionExceptionAntesQueConflicto()
    {
        // Arrange
        var p1 = await CrearPaisPruebaAsync("Pais M400 A", "MA");
        var p2 = await CrearPaisPruebaAsync("Pais M400 B", "MB");
        var d1 = await CrearDepartamentoPruebaAsync(p1, "Depto MA");
        var d2 = await CrearDepartamentoPruebaAsync(p2, "Depto MB");
        var m1 = await CrearMunicipioPruebaAsync(d1, "Muni MA");
        var m2 = await CrearMunicipioPruebaAsync(d2, "Muni MB");

        var empA = await _servicio.CrearAsync(DtoBase(m1.Id) with { Nit = "IGUAL" }, TestContext.Current.CancellationToken);
        var empB = await _servicio.CrearAsync(DtoBase(m2.Id) with { Nit = "IGUAL" }, TestContext.Current.CancellationToken);

        // Al intentar mover empA (que tiene NIT IGUAL) al municipio m2 del pais p2 donde ya hay un NIT IGUAL.
        // Debe saltar el error de RN8 (no se puede cambiar de país) ANTES que el error de RN5 (NIT duplicado en el país destino).
        // Act
        var ex = await Assert.ThrowsAsync<ValidacionException>(() => _servicio.ActualizarAsync(empA.Id, DtoBase(m2.Id) with { Nit = "IGUAL" }, TestContext.Current.CancellationToken));
        // Assert
        Assert.Equal("MunicipioId", ex.Campo);
    }

    [Fact]
    public async Task EliminarAsync_Inexistente_LanzaNoEncontradoException()
    {
        // Arrange
        // Act & Assert
        await Assert.ThrowsAsync<NoEncontradoException>(() => _servicio.EliminarAsync(9999, TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task EliminarAsync_ConColaboradorAsociado_LanzaConflictoException()
    {
        // Arrange
        var p = await CrearPaisPruebaAsync("Pais Col", "PC");
        var d = await CrearDepartamentoPruebaAsync(p, "Depto Col");
        var m = await CrearMunicipioPruebaAsync(d, "Muni Col");
        var emp = await _servicio.CrearAsync(DtoBase(m.Id) with { Nit = "EMPCOL" }, TestContext.Current.CancellationToken);

        var col = await CrearColaboradorPruebaAsync("c@c.com");
        _bd.Contexto.Set<EmpresaColaborador>().Add(new EmpresaColaborador
        {
            EmpresaId = emp.Id,
            ColaboradorId = col.Id,
            FechaIngreso = new DateOnly(2020, 1, 1)
        });
        await _bd.Contexto.SaveChangesAsync(TestContext.Current.CancellationToken);
        // Act & Assert
        await Assert.ThrowsAsync<ConflictoException>(() => _servicio.EliminarAsync(emp.Id, TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task EliminarAsync_SinColaboradores_SeElimina()
    {
        // Arrange
        var p = await CrearPaisPruebaAsync("Pais Sin", "PS");
        var d = await CrearDepartamentoPruebaAsync(p, "Depto Sin");
        var m = await CrearMunicipioPruebaAsync(d, "Muni Sin");
        var emp = await _servicio.CrearAsync(DtoBase(m.Id) with { Nit = "EMPSIN" }, TestContext.Current.CancellationToken);

        await _servicio.EliminarAsync(emp.Id, TestContext.Current.CancellationToken);
        // Act & Assert
        await Assert.ThrowsAsync<NoEncontradoException>(() => _servicio.ObtenerAsync(emp.Id, TestContext.Current.CancellationToken));
    }
}

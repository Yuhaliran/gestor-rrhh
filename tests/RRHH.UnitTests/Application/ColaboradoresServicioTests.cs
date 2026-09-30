using RRHH.Application.Excepciones;
using RRHH.Application.Servicios;
using RRHH.Contratos.Colaboradores;
using RRHH.Contratos.Comun;
using RRHH.Domain.Entidades;
using RRHH.Domain.Reglas;
using RRHH.UnitTests.Comun;
using RRHH.UnitTests.Datos;

using Xunit;

namespace RRHH.UnitTests.Application;

public class ColaboradoresServicioTests : IDisposable
{
    private readonly BaseDatosPrueba _bd;
    private readonly ColaboradoresServicio _servicio;
    private readonly RelojFijo _reloj;

    public ColaboradoresServicioTests()
    {
        _bd = new BaseDatosPrueba();
        _reloj = new RelojFijo();
        _servicio = new ColaboradoresServicio(_bd.Contexto, _reloj);
    }

    public void Dispose()
    {
        _bd.Dispose();
        GC.SuppressFinalize(this);
    }

    private static CrearColaboradorDto DtoCrearBase(int empresaId, DateOnly fechaIngreso) => new()
    {
        NombreCompleto = "Colaborador Prueba",
        FechaNacimiento = new DateOnly(2000, 1, 1),
        Telefono = "5555-1234",
        Correo = "prueba@colaborador.com",
        Empresas = new List<AsociarEmpresaDto>
        {
            new() { EmpresaId = empresaId, FechaIngreso = fechaIngreso, Puesto = "Puesto A" }
        }
    };

    private static GuardarColaboradorDto DtoGuardarBase() => new()
    {
        NombreCompleto = "Colaborador Act",
        FechaNacimiento = new DateOnly(2000, 1, 1),
        Telefono = "1111-2222",
        Correo = "act@colaborador.com"
    };

    private async Task<Pais> CrearPaisPruebaAsync(string nombre, string codigo, int min = 18, int max = 100, Regla29Febrero regla = Regla29Febrero.VeintiochoDeFebrero)
    {
        var pais = new Pais
        {
            Nombre = nombre,
            CodigoIso2 = codigo,
            EdadMinima = min,
            EdadMaxima = max,
            Regla29Febrero = regla
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

    private async Task<Empresa> CrearEmpresaPruebaAsync(Municipio muni, string nit, string razon, string comercial)
    {
        var emp = new Empresa
        {
            MunicipioId = muni.Id,
            Nit = nit,
            RazonSocial = razon,
            NombreComercial = comercial,
            Telefono = "1234567",
            Correo = "empresa@test.com"
        };
        _bd.Contexto.Set<Empresa>().Add(emp);
        await _bd.Contexto.SaveChangesAsync(TestContext.Current.CancellationToken);
        return emp;
    }

    private async Task<Colaborador> CrearColaboradorPruebaAsync(string correo, DateOnly nacimiento)
    {
        var col = new Colaborador
        {
            NombreCompleto = "Colaborador BD",
            FechaNacimiento = nacimiento,
            Telefono = "12345678",
            Correo = correo
        };
        _bd.Contexto.Set<Colaborador>().Add(col);
        await _bd.Contexto.SaveChangesAsync(TestContext.Current.CancellationToken);
        return col;
    }

    [Fact]
    public async Task CrearAsync_Valido_DevuelveEdadYEmpresasGuardandoTextosSinEspacios()
    {
        // Arrange
        var p = await CrearPaisPruebaAsync("Pais C", "PC");
        var d = await CrearDepartamentoPruebaAsync(p, "Depto C");
        var m = await CrearMunicipioPruebaAsync(d, "Muni C");
        var e = await CrearEmpresaPruebaAsync(m, "NIT-C", "Razon C", "Comercial C");

        var dto = new CrearColaboradorDto
        {
            NombreCompleto = "  Juan Perez  ",
            FechaNacimiento = new DateOnly(2000, 1, 1),
            Telefono = "  5555-1234  ",
            Correo = "  juan@c.com  ",
            Empresas = new List<AsociarEmpresaDto>
            {
                new() { EmpresaId = e.Id, FechaIngreso = new DateOnly(2025, 1, 1), Puesto = "  Puesto C  " }
            }
        };

        // Act
        var res = await _servicio.CrearAsync(dto, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal("Juan Perez", res.NombreCompleto);
        Assert.Equal("juan@c.com", res.Correo);
        Assert.Equal("5555-1234", res.Telefono);
        Assert.Equal(27, res.Edad); // 2027 - 2000 = 27
        Assert.Single(res.Empresas);
        Assert.Equal(e.Id, res.Empresas[0].EmpresaId);
        Assert.Equal("Comercial C", res.Empresas[0].NombreComercial);
        Assert.Equal("Pais C", res.Empresas[0].PaisNombre);
        Assert.Equal(new DateOnly(2025, 1, 1), res.Empresas[0].FechaIngreso);
        Assert.Equal("Puesto C", res.Empresas[0].Puesto);
    }

    [Fact]
    public async Task CrearAsync_EmpresaInexistenteEnPosicion1_LanzaValidacionExceptionEnEmpresaId()
    {
        // Arrange
        var p = await CrearPaisPruebaAsync("Pais V4", "P4");
        var d = await CrearDepartamentoPruebaAsync(p, "Depto V4");
        var m = await CrearMunicipioPruebaAsync(d, "Muni V4");
        var e = await CrearEmpresaPruebaAsync(m, "NIT-V4", "Razon V4", "Comercial V4");

        var dto = DtoCrearBase(e.Id, new DateOnly(2020, 1, 1)) with
        {
            Empresas = new List<AsociarEmpresaDto>
            {
                new() { EmpresaId = e.Id, FechaIngreso = new DateOnly(2020, 1, 1) },
                new() { EmpresaId = 9999, FechaIngreso = new DateOnly(2020, 1, 1) } // Inexistente en pos 1
            }
        };

        // Act & Assert
        var ex = await Assert.ThrowsAsync<ValidacionException>(() => _servicio.CrearAsync(dto, TestContext.Current.CancellationToken));
        Assert.Equal("Empresas[1].EmpresaId", ex.Campo);
    }

    [Fact]
    public async Task CrearAsync_FechaIngresoManiana_LanzaValidacionExceptionEnFechaIngreso()
    {
        // Arrange
        var p = await CrearPaisPruebaAsync("Pais RN9 M", "9M");
        var d = await CrearDepartamentoPruebaAsync(p, "Depto RN9 M");
        var m = await CrearMunicipioPruebaAsync(d, "Muni RN9 M");
        var e = await CrearEmpresaPruebaAsync(m, "NIT-9M", "R 9M", "C 9M");

        var dto = DtoCrearBase(e.Id, new DateOnly(2027, 3, 1)); // Mañana (reloj fijo es 2027-02-28)

        // Act & Assert
        var ex = await Assert.ThrowsAsync<ValidacionException>(() => _servicio.CrearAsync(dto, TestContext.Current.CancellationToken));
        Assert.Equal("Empresas[0].FechaIngreso", ex.Campo);
    }

    [Fact]
    public async Task CrearAsync_FechaIngresoHoy_Permitido()
    {
        // Arrange
        var p = await CrearPaisPruebaAsync("Pais RN9 H", "9H");
        var d = await CrearDepartamentoPruebaAsync(p, "Depto RN9 H");
        var m = await CrearMunicipioPruebaAsync(d, "Muni RN9 H");
        var e = await CrearEmpresaPruebaAsync(m, "NIT-9H", "R 9H", "C 9H");

        var dto = DtoCrearBase(e.Id, new DateOnly(2027, 2, 28)); // Hoy

        // Act
        var res = await _servicio.CrearAsync(dto, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(new DateOnly(2027, 2, 28), res.Empresas[0].FechaIngreso);
    }

    [Fact]
    public async Task CrearAsync_FechaIngresoUnDiaAntesDeNacimiento_LanzaValidacionException()
    {
        // Arrange
        var p = await CrearPaisPruebaAsync("Pais RN9 A", "9A");
        var d = await CrearDepartamentoPruebaAsync(p, "Depto RN9 A");
        var m = await CrearMunicipioPruebaAsync(d, "Muni RN9 A");
        var e = await CrearEmpresaPruebaAsync(m, "NIT-9A", "R 9A", "C 9A");

        var dto = DtoCrearBase(e.Id, new DateOnly(1999, 12, 31)) with { FechaNacimiento = new DateOnly(2000, 1, 1) };

        // Act & Assert
        var ex = await Assert.ThrowsAsync<ValidacionException>(() => _servicio.CrearAsync(dto, TestContext.Current.CancellationToken));
        Assert.Equal("Empresas[0].FechaIngreso", ex.Campo);
    }

    [Fact]
    public async Task CrearAsync_FechaIngresoMismoDiaDeNacimiento_Permitido()
    {
        // Arrange
        var p = await CrearPaisPruebaAsync("Pais RN9 D", "9D");
        var d = await CrearDepartamentoPruebaAsync(p, "Depto RN9 D");
        var m = await CrearMunicipioPruebaAsync(d, "Muni RN9 D");
        var e = await CrearEmpresaPruebaAsync(m, "NIT-9D", "R 9D", "C 9D");

        var dto = DtoCrearBase(e.Id, new DateOnly(2000, 1, 1)) with { FechaNacimiento = new DateOnly(2000, 1, 1) };

        // Act
        var res = await _servicio.CrearAsync(dto, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(new DateOnly(2000, 1, 1), res.Empresas[0].FechaIngreso);
    }

    [Fact]
    public async Task CrearAsync_EdadMinimaMenosUno_LanzaValidacionExceptionEnFechaNacimiento()
    {
        // Arrange
        var p = await CrearPaisPruebaAsync("Pais RN4 M1", "4M", min: 18, max: 100);
        var d = await CrearDepartamentoPruebaAsync(p, "Depto RN4 M1");
        var m = await CrearMunicipioPruebaAsync(d, "Muni RN4 M1");
        var e = await CrearEmpresaPruebaAsync(m, "NIT-4M", "R 4M", "C 4M");

        // Edad de hoy (2027-02-28) con nacimiento 2009-03-01 -> 17 años (le falta 1 día)
        var dto = DtoCrearBase(e.Id, new DateOnly(2025, 1, 1)) with { FechaNacimiento = new DateOnly(2009, 3, 1) };

        // Act & Assert
        var ex = await Assert.ThrowsAsync<ValidacionException>(() => _servicio.CrearAsync(dto, TestContext.Current.CancellationToken));
        Assert.Equal("FechaNacimiento", ex.Campo);
    }

    [Fact]
    public async Task CrearAsync_EdadMaximaMasUno_LanzaValidacionExceptionEnFechaNacimiento()
    {
        // Arrange
        var p = await CrearPaisPruebaAsync("Pais RN4 M2", "4N", min: 18, max: 100);
        var d = await CrearDepartamentoPruebaAsync(p, "Depto RN4 M2");
        var m = await CrearMunicipioPruebaAsync(d, "Muni RN4 M2");
        var e = await CrearEmpresaPruebaAsync(m, "NIT-4N", "R 4N", "C 4N");

        // Edad de hoy (2027-02-28) con nacimiento 1926-02-28 -> 101 años
        var dto = DtoCrearBase(e.Id, new DateOnly(2025, 1, 1)) with { FechaNacimiento = new DateOnly(1926, 2, 28) };

        // Act & Assert
        var ex = await Assert.ThrowsAsync<ValidacionException>(() => _servicio.CrearAsync(dto, TestContext.Current.CancellationToken));
        Assert.Equal("FechaNacimiento", ex.Campo);
    }

    [Fact]
    public async Task CrearAsync_EdadMinima_Permitido()
    {
        // Arrange
        var p = await CrearPaisPruebaAsync("Pais RN4 Min", "4I", min: 18, max: 100);
        var d = await CrearDepartamentoPruebaAsync(p, "Depto RN4 Min");
        var m = await CrearMunicipioPruebaAsync(d, "Muni RN4 Min");
        var e = await CrearEmpresaPruebaAsync(m, "NIT-4I", "R 4I", "C 4I");

        // 18 años exactos el 2027-02-28
        var dto = DtoCrearBase(e.Id, new DateOnly(2025, 1, 1)) with { FechaNacimiento = new DateOnly(2009, 2, 28) };

        // Act
        var res = await _servicio.CrearAsync(dto, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(18, res.Edad);
    }

    [Fact]
    public async Task CrearAsync_EdadMaxima_Permitido()
    {
        // Arrange
        var p = await CrearPaisPruebaAsync("Pais RN4 Max", "4X", min: 18, max: 100);
        var d = await CrearDepartamentoPruebaAsync(p, "Depto RN4 Max");
        var m = await CrearMunicipioPruebaAsync(d, "Muni RN4 Max");
        var e = await CrearEmpresaPruebaAsync(m, "NIT-4X", "R 4X", "C 4X");

        // 100 años exactos el 2027-02-28
        var dto = DtoCrearBase(e.Id, new DateOnly(2025, 1, 1)) with { FechaNacimiento = new DateOnly(1927, 2, 28) };

        // Act
        var res = await _servicio.CrearAsync(dto, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(100, res.Edad);
    }

    [Fact]
    public async Task CrearAsync_DosEmpresasConRangosDistintos_SiEdadEntraEnUnoSoloLanzaValidacionException()
    {
        // Arrange
        var p1 = await CrearPaisPruebaAsync("Pais RN4 R1", "R1", min: 18, max: 100);
        var p2 = await CrearPaisPruebaAsync("Pais RN4 R2", "R2", min: 21, max: 100);
        var d1 = await CrearDepartamentoPruebaAsync(p1, "Depto RN4 R1");
        var d2 = await CrearDepartamentoPruebaAsync(p2, "Depto RN4 R2");
        var m1 = await CrearMunicipioPruebaAsync(d1, "Muni RN4 R1");
        var m2 = await CrearMunicipioPruebaAsync(d2, "Muni RN4 R2");
        var e1 = await CrearEmpresaPruebaAsync(m1, "NIT-R1", "R R1", "C R1");
        var e2 = await CrearEmpresaPruebaAsync(m2, "NIT-R2", "R R2", "C R2");

        var dto = DtoCrearBase(e1.Id, new DateOnly(2025, 1, 1)) with
        {
            FechaNacimiento = new DateOnly(2007, 1, 1), // 20 años: entra en R1 pero no en R2
            Empresas = new List<AsociarEmpresaDto>
            {
                new() { EmpresaId = e1.Id, FechaIngreso = new DateOnly(2025, 1, 1) },
                new() { EmpresaId = e2.Id, FechaIngreso = new DateOnly(2025, 1, 1) }
            }
        };

        // Act & Assert
        var ex = await Assert.ThrowsAsync<ValidacionException>(() => _servicio.CrearAsync(dto, TestContext.Current.CancellationToken));
        Assert.Equal("FechaNacimiento", ex.Campo);
    }

    [Fact]
    public async Task CrearAsync_FechaNacimientoFutura_LanzaValidacionException()
    {
        // Arrange
        var p = await CrearPaisPruebaAsync("Pais RN4 F", "4F");
        var d = await CrearDepartamentoPruebaAsync(p, "Depto RN4 F");
        var m = await CrearMunicipioPruebaAsync(d, "Muni RN4 F");
        var e = await CrearEmpresaPruebaAsync(m, "NIT-4F", "R 4F", "C 4F");

        var dto = DtoCrearBase(e.Id, new DateOnly(2025, 1, 1)) with { FechaNacimiento = new DateOnly(2027, 3, 1) }; // Mañana

        // Act & Assert
        var ex = await Assert.ThrowsAsync<ValidacionException>(() => _servicio.CrearAsync(dto, TestContext.Current.CancellationToken));
        Assert.Equal("FechaNacimiento", ex.Campo);
    }

    [Fact]
    public async Task CrearAsync_ValidacionExceptionAntesQueConflictoExceptionY_V4AntesQueRN4()
    {
        // Arrange
        var p = await CrearPaisPruebaAsync("Pais Ord", "OR", min: 30, max: 100);
        var d = await CrearDepartamentoPruebaAsync(p, "Depto Ord");
        var m = await CrearMunicipioPruebaAsync(d, "Muni Ord");
        var e = await CrearEmpresaPruebaAsync(m, "NIT-OR", "R OR", "C OR");

        await CrearColaboradorPruebaAsync("dup@ord.com", new DateOnly(2000, 1, 1));

        // Correo duplicado (409), Empresa[1] inexistente (V4 -> 400), Edad 20 años (RN4 -> 400 en FechaNacimiento)
        var dto = new CrearColaboradorDto
        {
            NombreCompleto = "Juan Ord",
            FechaNacimiento = new DateOnly(2007, 1, 1),
            Telefono = "5555-1234",
            Correo = "dup@ord.com",
            Empresas = new List<AsociarEmpresaDto>
            {
                new() { EmpresaId = e.Id, FechaIngreso = new DateOnly(2025, 1, 1) },
                new() { EmpresaId = 9999, FechaIngreso = new DateOnly(2025, 1, 1) }
            }
        };

        // Act & Assert
        // Debe saltar V4 (Empresas[1].EmpresaId) antes que RN4 (FechaNacimiento) y antes que 409 (Correo).
        var ex = await Assert.ThrowsAsync<ValidacionException>(() => _servicio.CrearAsync(dto, TestContext.Current.CancellationToken));
        Assert.Equal("Empresas[1].EmpresaId", ex.Campo);
    }

    [Fact]
    public async Task CrearAsync_CorreoDeOtroColaboradorConOtrasMayusculasYEspacios_LanzaConflictoException()
    {
        // Arrange
        var p = await CrearPaisPruebaAsync("Pais Dup C", "DC");
        var d = await CrearDepartamentoPruebaAsync(p, "Depto Dup C");
        var m = await CrearMunicipioPruebaAsync(d, "Muni Dup C");
        var e = await CrearEmpresaPruebaAsync(m, "NIT-DC", "R DC", "C DC");

        await CrearColaboradorPruebaAsync("unico@test.com", new DateOnly(2000, 1, 1));

        var dto = DtoCrearBase(e.Id, new DateOnly(2025, 1, 1)) with { Correo = "  UNICO@TEST.COM  " };

        // Act & Assert
        await Assert.ThrowsAsync<ConflictoException>(() => _servicio.CrearAsync(dto, TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task CrearAsync_MismaEmpresaDosVeces_LanzaConflictoException()
    {
        // Arrange
        var p = await CrearPaisPruebaAsync("Pais Dup E", "DE");
        var d = await CrearDepartamentoPruebaAsync(p, "Depto Dup E");
        var m = await CrearMunicipioPruebaAsync(d, "Muni Dup E");
        var e = await CrearEmpresaPruebaAsync(m, "NIT-DE", "R DE", "C DE");

        var dto = DtoCrearBase(e.Id, new DateOnly(2025, 1, 1)) with
        {
            Empresas = new List<AsociarEmpresaDto>
            {
                new() { EmpresaId = e.Id, FechaIngreso = new DateOnly(2025, 1, 1) },
                new() { EmpresaId = e.Id, FechaIngreso = new DateOnly(2026, 1, 1) }
            }
        };

        // Act & Assert
        await Assert.ThrowsAsync<ConflictoException>(() => _servicio.CrearAsync(dto, TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task CrearAsync_Nacido29FebreroEnAnioBisiesto_CalculaEdadMostradaSegunPaisDeIngresoMasAntiguo_SiHayEmpateSegunMenorId()
    {
        // Arrange
        // Pais 1 usa 28 Feb
        var p1 = await CrearPaisPruebaAsync("Pais 28F", "F8", regla: Regla29Febrero.VeintiochoDeFebrero);
        var d1 = await CrearDepartamentoPruebaAsync(p1, "Depto 28F");
        var m1 = await CrearMunicipioPruebaAsync(d1, "Muni 28F");
        var e1 = await CrearEmpresaPruebaAsync(m1, "NIT-28F", "R 28F", "C 28F");

        // Pais 2 usa 1 Mar
        var p2 = await CrearPaisPruebaAsync("Pais 1M", "M1", regla: Regla29Febrero.PrimeroDeMarzo);
        var d2 = await CrearDepartamentoPruebaAsync(p2, "Depto 1M");
        var m2 = await CrearMunicipioPruebaAsync(d2, "Muni 1M");
        var e2 = await CrearEmpresaPruebaAsync(m2, "NIT-1M", "R 1M", "C 1M");

        // Nacido 2004-02-29 (bisiesto). Hoy es 2027-02-28.
        // Si usa p1 (28Feb), hoy ya cumplió 23 años.
        // Si usa p2 (1Mar), hoy tiene 22 años (cumple mañana).
        var dto = DtoCrearBase(e1.Id, new DateOnly(2025, 1, 1)) with
        {
            FechaNacimiento = new DateOnly(2004, 2, 29),
            Empresas = new List<AsociarEmpresaDto>
            {
                new() { EmpresaId = e1.Id, FechaIngreso = new DateOnly(2022, 1, 1) }, // Más antigua -> manda p1
                new() { EmpresaId = e2.Id, FechaIngreso = new DateOnly(2023, 1, 1) }
            }
        };

        // Act
        var res1 = await _servicio.CrearAsync(dto, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(23, res1.Edad);

        // Arrange (invertido)
        var dto2 = dto with
        {
            Correo = "otro@test.com",
            Empresas = new List<AsociarEmpresaDto>
            {
                new() { EmpresaId = e1.Id, FechaIngreso = new DateOnly(2023, 1, 1) },
                new() { EmpresaId = e2.Id, FechaIngreso = new DateOnly(2022, 1, 1) } // Más antigua -> manda p2
            }
        };

        // Act
        var res2 = await _servicio.CrearAsync(dto2, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(22, res2.Edad);

        // Arrange (empate)
        var dto3 = dto with
        {
            Correo = "empate@test.com",
            Empresas = new List<AsociarEmpresaDto>
            {
                new() { EmpresaId = e2.Id, FechaIngreso = new DateOnly(2022, 1, 1) }, // Id mayor
                new() { EmpresaId = e1.Id, FechaIngreso = new DateOnly(2022, 1, 1) }  // Id menor -> manda e1 (p1)
            }
        };

        // Act
        var res3 = await _servicio.CrearAsync(dto3, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(23, res3.Edad);
    }

    [Fact]
    public async Task ListarAsync_SinBusqueda_OrdenadoPorNombre_DevuelveEdadYEmpresas()
    {
        // Arrange
        var p = await CrearPaisPruebaAsync("Pais LS", "LS");
        var d = await CrearDepartamentoPruebaAsync(p, "Depto LS");
        var m = await CrearMunicipioPruebaAsync(d, "Muni LS");
        var e = await CrearEmpresaPruebaAsync(m, "NIT-LS", "R LS", "C LS");

        await _servicio.CrearAsync(DtoCrearBase(e.Id, new DateOnly(2025, 1, 1)) with { NombreCompleto = "Zeta", Correo = "z@ls.com" }, TestContext.Current.CancellationToken);
        await _servicio.CrearAsync(DtoCrearBase(e.Id, new DateOnly(2025, 1, 1)) with { NombreCompleto = "Alfa", Correo = "a@ls.com" }, TestContext.Current.CancellationToken);
        await _servicio.CrearAsync(DtoCrearBase(e.Id, new DateOnly(2025, 1, 1)) with { NombreCompleto = "Epsilon", Correo = "e@ls.com" }, TestContext.Current.CancellationToken);

        // Act
        var res = await _servicio.ListarAsync(new Consulta { Pagina = 1, Tamanio = 100 }, TestContext.Current.CancellationToken);
        var ls = res.Elementos.Where(x => x.Correo.EndsWith("@ls.com")).ToList();

        // Assert
        Assert.Equal(3, ls.Count);
        Assert.Equal("Alfa", ls[0].NombreCompleto);
        Assert.Equal("Epsilon", ls[1].NombreCompleto);
        Assert.Equal("Zeta", ls[2].NombreCompleto);
        Assert.Equal(27, ls[0].Edad);
        Assert.Single(ls[0].Empresas);
        Assert.Equal(e.Id, ls[0].Empresas[0].EmpresaId);
    }

    [Fact]
    public async Task ListarAsync_BuscarPorNombreOCorreo_DevuelveConjuntoExacto()
    {
        // Arrange
        var p = await CrearPaisPruebaAsync("Pais LB", "LB");
        var d = await CrearDepartamentoPruebaAsync(p, "Depto LB");
        var m = await CrearMunicipioPruebaAsync(d, "Muni LB");
        var e = await CrearEmpresaPruebaAsync(m, "NIT-LB", "R LB", "C LB");

        await _servicio.CrearAsync(DtoCrearBase(e.Id, new DateOnly(2025, 1, 1)) with { NombreCompleto = "Buscado Uno", Correo = "x@lb.com" }, TestContext.Current.CancellationToken);
        await _servicio.CrearAsync(DtoCrearBase(e.Id, new DateOnly(2025, 1, 1)) with { NombreCompleto = "Otro", Correo = "bUsCado@lb.com" }, TestContext.Current.CancellationToken);
        await _servicio.CrearAsync(DtoCrearBase(e.Id, new DateOnly(2025, 1, 1)) with { NombreCompleto = "Nada", Correo = "n@lb.com" }, TestContext.Current.CancellationToken);

        // Act
        var res = await _servicio.ListarAsync(new Consulta { Buscar = "BUSCADO" }, TestContext.Current.CancellationToken);
        var ls = res.Elementos.Where(x => x.Correo.EndsWith("@lb.com")).ToList();

        // Assert
        Assert.Equal(2, ls.Count);
        Assert.Contains(ls, x => x.NombreCompleto == "Buscado Uno");
        Assert.Contains(ls, x => x.NombreCompleto == "Otro");
    }

    [Fact]
    public async Task ObtenerAsync_Existente_DevuelveTodosLosCampos()
    {
        // Arrange
        var p = await CrearPaisPruebaAsync("Pais OE", "OE");
        var d = await CrearDepartamentoPruebaAsync(p, "Depto OE");
        var m = await CrearMunicipioPruebaAsync(d, "Muni OE");
        var e = await CrearEmpresaPruebaAsync(m, "NIT-OE", "R OE", "C OE");

        var creado = await _servicio.CrearAsync(DtoCrearBase(e.Id, new DateOnly(2025, 1, 1)), TestContext.Current.CancellationToken);

        // Act
        var res = await _servicio.ObtenerAsync(creado.Id, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(creado.Id, res.Id);
        Assert.Equal(creado.NombreCompleto, res.NombreCompleto);
        Assert.Equal(creado.Correo, res.Correo);
        Assert.Equal(creado.Telefono, res.Telefono);
        Assert.Equal(creado.Edad, res.Edad);
        Assert.Single(res.Empresas);
        Assert.Equal(e.Id, res.Empresas[0].EmpresaId);
        Assert.Equal(new DateOnly(2025, 1, 1), res.Empresas[0].FechaIngreso);
    }

    [Fact]
    public async Task ObtenerAsync_Inexistente_LanzaNoEncontradoException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<NoEncontradoException>(() => _servicio.ObtenerAsync(9999, TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task ActualizarAsync_Inexistente_LanzaNoEncontradoException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<NoEncontradoException>(() => _servicio.ActualizarAsync(9999, DtoGuardarBase(), TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task ActualizarAsync_CambiaDatosPersonalesYNoTocaEmpresas_GuardaSinEspacios()
    {
        // Arrange
        var p = await CrearPaisPruebaAsync("Pais AC", "AC");
        var d = await CrearDepartamentoPruebaAsync(p, "Depto AC");
        var m = await CrearMunicipioPruebaAsync(d, "Muni AC");
        var e = await CrearEmpresaPruebaAsync(m, "NIT-AC", "R AC", "C AC");

        var creado = await _servicio.CrearAsync(DtoCrearBase(e.Id, new DateOnly(2025, 1, 1)), TestContext.Current.CancellationToken);

        var dto = new GuardarColaboradorDto
        {
            NombreCompleto = "  Nuevo Nombre  ",
            FechaNacimiento = new DateOnly(2005, 1, 1),
            Telefono = "  9999-9999  ",
            Correo = "  nuevo@ac.com  "
        };

        // Act
        var res = await _servicio.ActualizarAsync(creado.Id, dto, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal("Nuevo Nombre", res.NombreCompleto);
        Assert.Equal("nuevo@ac.com", res.Correo);
        Assert.Equal("9999-9999", res.Telefono);
        Assert.Equal(22, res.Edad); // 2027 - 2005 = 22
        Assert.Single(res.Empresas);
        Assert.Equal(e.Id, res.Empresas[0].EmpresaId);
    }

    [Fact]
    public async Task ActualizarAsync_CambiaFechaNacimientoYQuedaFueraDeRango_LanzaValidacionException()
    {
        // Arrange
        var p = await CrearPaisPruebaAsync("Pais AFR", "FR", min: 18, max: 100);
        var d = await CrearDepartamentoPruebaAsync(p, "Depto AFR");
        var m = await CrearMunicipioPruebaAsync(d, "Muni AFR");
        var e = await CrearEmpresaPruebaAsync(m, "NIT-FR", "R FR", "C FR");

        var creado = await _servicio.CrearAsync(DtoCrearBase(e.Id, new DateOnly(2025, 1, 1)) with { FechaNacimiento = new DateOnly(2000, 1, 1) }, TestContext.Current.CancellationToken);

        var dto = DtoGuardarBase() with { FechaNacimiento = new DateOnly(2020, 1, 1) }; // 7 años -> fuera de rango (18)

        // Act & Assert
        var ex = await Assert.ThrowsAsync<ValidacionException>(() => _servicio.ActualizarAsync(creado.Id, dto, TestContext.Current.CancellationToken));
        Assert.Equal("FechaNacimiento", ex.Campo);
    }

    [Fact]
    public async Task ActualizarAsync_MismaFechaNacimiento_SePuedeEditarAunqueEdadHoyFueraDeRango()
    {
        // Arrange
        // Pais con maxima 25.
        var p = await CrearPaisPruebaAsync("Pais AED", "ED", min: 18, max: 25);
        var d = await CrearDepartamentoPruebaAsync(p, "Depto AED");
        var m = await CrearMunicipioPruebaAsync(d, "Muni AED");
        var e = await CrearEmpresaPruebaAsync(m, "NIT-ED", "R ED", "C ED");

        // Creamos colaborador directo en BD para evitar validación de Create (ya que hoy en 2027 tiene 27 años, > 25)
        var col = await CrearColaboradorPruebaAsync("viejo@ed.com", new DateOnly(2000, 1, 1));
        var rel = new EmpresaColaborador { EmpresaId = e.Id, ColaboradorId = col.Id, FechaIngreso = new DateOnly(2025, 1, 1) };
        _bd.Contexto.Set<EmpresaColaborador>().Add(rel);
        await _bd.Contexto.SaveChangesAsync(TestContext.Current.CancellationToken);

        var dto = DtoGuardarBase() with { NombreCompleto = "Cambio Nombre", FechaNacimiento = new DateOnly(2000, 1, 1) };

        // Act
        var res = await _servicio.ActualizarAsync(col.Id, dto, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal("Cambio Nombre", res.NombreCompleto); // Funciona
    }

    [Fact]
    public async Task ActualizarAsync_NuevaFechaNacimientoPosteriorAFechaIngreso_LanzaValidacionException()
    {
        // Arrange
        var p = await CrearPaisPruebaAsync("Pais API", "PI");
        var d = await CrearDepartamentoPruebaAsync(p, "Depto API");
        var m = await CrearMunicipioPruebaAsync(d, "Muni API");
        var e = await CrearEmpresaPruebaAsync(m, "NIT-PI", "R PI", "C PI");

        // Ingreso en 2025
        var creado = await _servicio.CrearAsync(DtoCrearBase(e.Id, new DateOnly(2025, 1, 1)) with { FechaNacimiento = new DateOnly(2000, 1, 1) }, TestContext.Current.CancellationToken);

        // Nueva fecha de nacimiento: 2026 -> posterior a ingreso (2025)
        var dto = DtoGuardarBase() with { FechaNacimiento = new DateOnly(2026, 1, 1) };

        // Act & Assert
        var ex = await Assert.ThrowsAsync<ValidacionException>(() => _servicio.ActualizarAsync(creado.Id, dto, TestContext.Current.CancellationToken));
        Assert.Equal("FechaNacimiento", ex.Campo);
    }

    [Fact]
    public async Task ActualizarAsync_CorreoDeOtroColaborador_LanzaConflictoExceptionYPropioPermitido()
    {
        // Arrange
        var p = await CrearPaisPruebaAsync("Pais ACD", "CD");
        var d = await CrearDepartamentoPruebaAsync(p, "Depto ACD");
        var m = await CrearMunicipioPruebaAsync(d, "Muni ACD");
        var e = await CrearEmpresaPruebaAsync(m, "NIT-CD", "R CD", "C CD");

        var col1 = await _servicio.CrearAsync(DtoCrearBase(e.Id, new DateOnly(2025, 1, 1)) with { Correo = "c1@cd.com" }, TestContext.Current.CancellationToken);
        var col2 = await _servicio.CrearAsync(DtoCrearBase(e.Id, new DateOnly(2025, 1, 1)) with { Correo = "c2@cd.com" }, TestContext.Current.CancellationToken);

        // Act
        // Propio permitido
        var res = await _servicio.ActualizarAsync(col1.Id, DtoGuardarBase() with { Correo = "c1@cd.com", NombreCompleto = "Cambio" }, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal("Cambio", res.NombreCompleto);

        // Act & Assert
        // De otro lanza 409
        await Assert.ThrowsAsync<ConflictoException>(() => _servicio.ActualizarAsync(col1.Id, DtoGuardarBase() with { Correo = "c2@cd.com" }, TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task EliminarAsync_Inexistente_LanzaNoEncontradoException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<NoEncontradoException>(() => _servicio.EliminarAsync(9999, TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task EliminarAsync_Existente_BorraColaboradorYSusRelaciones()
    {
        // Arrange
        var p = await CrearPaisPruebaAsync("Pais ELI", "EL");
        var d = await CrearDepartamentoPruebaAsync(p, "Depto ELI");
        var m = await CrearMunicipioPruebaAsync(d, "Muni ELI");
        var e = await CrearEmpresaPruebaAsync(m, "NIT-ELI", "R ELI", "C ELI");

        var creado = await _servicio.CrearAsync(DtoCrearBase(e.Id, new DateOnly(2025, 1, 1)), TestContext.Current.CancellationToken);

        // Act
        await _servicio.EliminarAsync(creado.Id, TestContext.Current.CancellationToken);

        // Assert
        await Assert.ThrowsAsync<NoEncontradoException>(() => _servicio.ObtenerAsync(creado.Id, TestContext.Current.CancellationToken));

        // Verificamos que se borraron las relaciones
        var relacion = _bd.Contexto.Set<EmpresaColaborador>().FirstOrDefault(x => x.ColaboradorId == creado.Id);
        Assert.Null(relacion);
    }

}

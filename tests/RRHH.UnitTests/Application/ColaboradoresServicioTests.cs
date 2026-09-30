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
    public async Task CrearAsync_FechaIngresoInvalidaEnSegundaEmpresa_LanzaValidacionExceptionConIndice()
    {
        // Arrange
        var p = await CrearPaisPruebaAsync("Pais RN9 B", "9B");
        var d = await CrearDepartamentoPruebaAsync(p, "Depto RN9 B");
        var m = await CrearMunicipioPruebaAsync(d, "Muni RN9 B");
        var e1 = await CrearEmpresaPruebaAsync(m, "N1", "R1", "C1");
        var e2 = await CrearEmpresaPruebaAsync(m, "N2", "R2", "C2");

        var dto = DtoCrearBase(e1.Id, new DateOnly(2025, 1, 1)) with
        {
            Empresas = new List<AsociarEmpresaDto>
            {
                new() { EmpresaId = e1.Id, FechaIngreso = new DateOnly(2025, 1, 1) },
                new() { EmpresaId = e2.Id, FechaIngreso = new DateOnly(2028, 1, 1) } // Futuro
            }
        };

        // Act & Assert
        var ex = await Assert.ThrowsAsync<ValidacionException>(() => _servicio.CrearAsync(dto, TestContext.Current.CancellationToken));
        Assert.Equal("Empresas[1].FechaIngreso", ex.Campo);
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
    public async Task CrearAsync_ErroresMultiples_LanzaV4AntesQueRN4Y409()
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
    public async Task CrearAsync_Nacido29Febrero_EdadSegunEmpresaMasAntigua_MandaRegla28()
    {
        // Arrange
        var p1 = await CrearPaisPruebaAsync("Pais 28F 1", "F1", regla: Regla29Febrero.VeintiochoDeFebrero);
        var d1 = await CrearDepartamentoPruebaAsync(p1, "Depto 28F 1");
        var m1 = await CrearMunicipioPruebaAsync(d1, "Muni 28F 1");
        var e1 = await CrearEmpresaPruebaAsync(m1, "NIT-2F1", "R 2F1", "C 2F1");

        var p2 = await CrearPaisPruebaAsync("Pais 1M1", "M11", regla: Regla29Febrero.PrimeroDeMarzo);
        var d2 = await CrearDepartamentoPruebaAsync(p2, "Depto 1M1");
        var m2 = await CrearMunicipioPruebaAsync(d2, "Muni 1M1");
        var e2 = await CrearEmpresaPruebaAsync(m2, "NIT-1M1", "R 1M1", "C 1M1");

        var dto = DtoCrearBase(e1.Id, new DateOnly(2025, 1, 1)) with
        {
            FechaNacimiento = new DateOnly(2004, 2, 29),
            Empresas = new List<AsociarEmpresaDto>
            {
                new() { EmpresaId = e1.Id, FechaIngreso = new DateOnly(2022, 1, 1) },
                new() { EmpresaId = e2.Id, FechaIngreso = new DateOnly(2023, 1, 1) }
            }
        };

        // Act
        var res1 = await _servicio.CrearAsync(dto, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(23, res1.Edad);
    }

    [Fact]
    public async Task CrearAsync_Nacido29Febrero_EdadSegunEmpresaMasAntigua_MandaRegla1Marzo()
    {
        // Arrange
        var p1 = await CrearPaisPruebaAsync("Pais 28F 2", "F2", regla: Regla29Febrero.VeintiochoDeFebrero);
        var d1 = await CrearDepartamentoPruebaAsync(p1, "Depto 28F 2");
        var m1 = await CrearMunicipioPruebaAsync(d1, "Muni 28F 2");
        var e1 = await CrearEmpresaPruebaAsync(m1, "NIT-2F2", "R 2F2", "C 2F2");

        var p2 = await CrearPaisPruebaAsync("Pais 1M 2", "M12", regla: Regla29Febrero.PrimeroDeMarzo);
        var d2 = await CrearDepartamentoPruebaAsync(p2, "Depto 1M 2");
        var m2 = await CrearMunicipioPruebaAsync(d2, "Muni 1M 2");
        var e2 = await CrearEmpresaPruebaAsync(m2, "NIT-1M2", "R 1M2", "C 1M2");

        var dto = DtoCrearBase(e1.Id, new DateOnly(2025, 1, 1)) with
        {
            FechaNacimiento = new DateOnly(2004, 2, 29),
            Empresas = new List<AsociarEmpresaDto>
            {
                new() { EmpresaId = e1.Id, FechaIngreso = new DateOnly(2023, 1, 1) },
                new() { EmpresaId = e2.Id, FechaIngreso = new DateOnly(2022, 1, 1) }
            }
        };

        // Act
        var res2 = await _servicio.CrearAsync(dto, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(22, res2.Edad);
    }

    [Fact]
    public async Task CrearAsync_Nacido29Febrero_EdadSegunEmpresaMasAntigua_EmpateMandaMenorId()
    {
        // Arrange
        var p1 = await CrearPaisPruebaAsync("Pais 28F 3", "F3", regla: Regla29Febrero.VeintiochoDeFebrero);
        var d1 = await CrearDepartamentoPruebaAsync(p1, "Depto 28F 3");
        var m1 = await CrearMunicipioPruebaAsync(d1, "Muni 28F 3");
        var e1 = await CrearEmpresaPruebaAsync(m1, "NIT-2F3", "R 2F3", "C 2F3");

        var p2 = await CrearPaisPruebaAsync("Pais 1M 3", "M13", regla: Regla29Febrero.PrimeroDeMarzo);
        var d2 = await CrearDepartamentoPruebaAsync(p2, "Depto 1M 3");
        var m2 = await CrearMunicipioPruebaAsync(d2, "Muni 1M 3");
        var e2 = await CrearEmpresaPruebaAsync(m2, "NIT-1M3", "R 1M3", "C 1M3");

        var dto = DtoCrearBase(e1.Id, new DateOnly(2025, 1, 1)) with
        {
            FechaNacimiento = new DateOnly(2004, 2, 29),
            Empresas = new List<AsociarEmpresaDto>
            {
                new() { EmpresaId = e2.Id, FechaIngreso = new DateOnly(2022, 1, 1) },
                new() { EmpresaId = e1.Id, FechaIngreso = new DateOnly(2022, 1, 1) }
            }
        };

        // Act
        var res3 = await _servicio.CrearAsync(dto, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(23, res3.Edad);
    }

    [Fact]
    public async Task CrearAsync_EdadMinimaAlBorde29Febrero_ConReglaPrimeroDeMarzo_Rechaza()
    {
        // Arrange
        var p = await CrearPaisPruebaAsync("Pais 1M 19", "P1", min: 19, max: 100, regla: Regla29Febrero.PrimeroDeMarzo);
        var d = await CrearDepartamentoPruebaAsync(p, "Depto 1M 19");
        var m = await CrearMunicipioPruebaAsync(d, "Muni 1M 19");
        var e = await CrearEmpresaPruebaAsync(m, "N1", "R1", "C1");

        var dto = DtoCrearBase(e.Id, new DateOnly(2027, 1, 1)) with
        {
            FechaNacimiento = new DateOnly(2008, 2, 29)
        };

        // Act & Assert
        var ex = await Assert.ThrowsAsync<ValidacionException>(() => _servicio.CrearAsync(dto, TestContext.Current.CancellationToken));
        Assert.Equal("FechaNacimiento", ex.Campo);
    }

    [Fact]
    public async Task CrearAsync_EdadMinimaAlBorde29Febrero_ConReglaVeintiocho_Permitido()
    {
        // Arrange
        var p = await CrearPaisPruebaAsync("Pais 28F 19", "P2", min: 19, max: 100, regla: Regla29Febrero.VeintiochoDeFebrero);
        var d = await CrearDepartamentoPruebaAsync(p, "Depto 28F 19");
        var m = await CrearMunicipioPruebaAsync(d, "Muni 28F 19");
        var e = await CrearEmpresaPruebaAsync(m, "N2", "R2", "C2");

        var dto = DtoCrearBase(e.Id, new DateOnly(2027, 1, 1)) with
        {
            FechaNacimiento = new DateOnly(2008, 2, 29)
        };

        // Act
        var res = await _servicio.CrearAsync(dto, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(19, res.Edad);
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
        Assert.Equal(p.Id, res.Empresas[0].PaisId);
        Assert.Equal("Pais AC", res.Empresas[0].PaisNombre);
        Assert.Equal(new DateOnly(2025, 1, 1), res.Empresas[0].FechaIngreso);
        Assert.Equal("Puesto A", res.Empresas[0].Puesto);
        Assert.Equal(new DateOnly(2005, 1, 1), res.FechaNacimiento);

        var guardado = await _servicio.ObtenerAsync(creado.Id, TestContext.Current.CancellationToken);
        Assert.Equal("Nuevo Nombre", guardado.NombreCompleto);
        Assert.Equal("nuevo@ac.com", guardado.Correo);
        Assert.Equal("9999-9999", guardado.Telefono);
        Assert.Equal(new DateOnly(2005, 1, 1), guardado.FechaNacimiento);
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

        // Ingreso en 2005, nacimiento original en 1980
        var creado = await _servicio.CrearAsync(DtoCrearBase(e.Id, new DateOnly(2005, 1, 1)) with { FechaNacimiento = new DateOnly(1980, 1, 1) }, TestContext.Current.CancellationToken);

        // Nueva fecha de nacimiento: 2006 -> posterior a ingreso (2005), pero mayor a 18 años hoy (2027-2006=21)
        var dto = DtoGuardarBase() with { FechaNacimiento = new DateOnly(2006, 1, 1) };

        // Act & Assert
        var ex = await Assert.ThrowsAsync<ValidacionException>(() => _servicio.ActualizarAsync(creado.Id, dto, TestContext.Current.CancellationToken));
        Assert.Equal("FechaNacimiento", ex.Campo);
    }

    [Fact]
    public async Task ActualizarAsync_CorreoPropio_Permitido()
    {
        // Arrange
        var p = await CrearPaisPruebaAsync("Pais ACD 1", "C1");
        var d = await CrearDepartamentoPruebaAsync(p, "Depto ACD 1");
        var m = await CrearMunicipioPruebaAsync(d, "Muni ACD 1");
        var e = await CrearEmpresaPruebaAsync(m, "NIT-CD1", "R CD1", "C CD1");

        var col1 = await _servicio.CrearAsync(DtoCrearBase(e.Id, new DateOnly(2025, 1, 1)) with { Correo = "c1@cd.com" }, TestContext.Current.CancellationToken);

        // Act
        var res = await _servicio.ActualizarAsync(col1.Id, DtoGuardarBase() with { Correo = "c1@cd.com", NombreCompleto = "Cambio" }, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal("Cambio", res.NombreCompleto);
    }

    [Fact]
    public async Task ActualizarAsync_CorreoDeOtroColaborador_LanzaConflictoException()
    {
        // Arrange
        var p = await CrearPaisPruebaAsync("Pais ACD 2", "C2");
        var d = await CrearDepartamentoPruebaAsync(p, "Depto ACD 2");
        var m = await CrearMunicipioPruebaAsync(d, "Muni ACD 2");
        var e = await CrearEmpresaPruebaAsync(m, "NIT-CD2", "R CD2", "C CD2");

        var col1 = await _servicio.CrearAsync(DtoCrearBase(e.Id, new DateOnly(2025, 1, 1)) with { Correo = "c3@cd.com" }, TestContext.Current.CancellationToken);
        var col2 = await _servicio.CrearAsync(DtoCrearBase(e.Id, new DateOnly(2025, 1, 1)) with { Correo = "c4@cd.com" }, TestContext.Current.CancellationToken);

        // Act & Assert
        await Assert.ThrowsAsync<ConflictoException>(() => _servicio.ActualizarAsync(col1.Id, DtoGuardarBase() with { Correo = "c4@cd.com" }, TestContext.Current.CancellationToken));
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

    [Fact]
    public async Task ListarPorEmpresaAsync_EmpresaInexistente_LanzaNoEncontradoException()
    {
        // Arrange
        // Act & Assert
        await Assert.ThrowsAsync<NoEncontradoException>(() => _servicio.ListarPorEmpresaAsync(9999, new Consulta(), TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task ListarPorEmpresaAsync_SinBusqueda_SoloColaboradoresDeEmpresaOrdenadosPorNombre()
    {
        // Arrange
        var p = await CrearPaisPruebaAsync("Pais L14", "L1");
        var d = await CrearDepartamentoPruebaAsync(p, "Depto L14");
        var m = await CrearMunicipioPruebaAsync(d, "Muni L14");
        var e1 = await CrearEmpresaPruebaAsync(m, "NL14-1", "RL14-1", "CL14-1");
        var e2 = await CrearEmpresaPruebaAsync(m, "NL14-2", "RL14-2", "CL14-2");

        var dtoZ = DtoCrearBase(e1.Id, new DateOnly(2025, 1, 1)) with { NombreCompleto = "Z", Correo = "z@t.com" };
        var dtoA = DtoCrearBase(e1.Id, new DateOnly(2025, 1, 1)) with { NombreCompleto = "A", Correo = "a@t.com" };
        var dtoE2 = DtoCrearBase(e2.Id, new DateOnly(2025, 1, 1)) with { NombreCompleto = "B", Correo = "b@t.com" };

        await _servicio.CrearAsync(dtoZ, TestContext.Current.CancellationToken);
        await _servicio.CrearAsync(dtoA, TestContext.Current.CancellationToken);
        await _servicio.CrearAsync(dtoE2, TestContext.Current.CancellationToken);

        // Act
        var pagina = await _servicio.ListarPorEmpresaAsync(e1.Id, new Consulta(), TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(2, pagina.Total);
        Assert.Equal("A", pagina.Elementos[0].NombreCompleto);
        Assert.Equal("Z", pagina.Elementos[1].NombreCompleto);
    }

    [Fact]
    public async Task ListarPorEmpresaAsync_Buscar_FiltraPorNombreOCorreo()
    {
        // Arrange
        var p = await CrearPaisPruebaAsync("Pais L14 B", "LB");
        var d = await CrearDepartamentoPruebaAsync(p, "Depto L14 B");
        var m = await CrearMunicipioPruebaAsync(d, "Muni L14 B");
        var e = await CrearEmpresaPruebaAsync(m, "NLB", "RLB", "CLB");

        var dto1 = DtoCrearBase(e.Id, new DateOnly(2025, 1, 1)) with { NombreCompleto = "Juan Perez", Correo = "j@t.com" };
        var dto2 = DtoCrearBase(e.Id, new DateOnly(2025, 1, 1)) with { NombreCompleto = "Ana", Correo = "perez@t.com" };
        var dto3 = DtoCrearBase(e.Id, new DateOnly(2025, 1, 1)) with { NombreCompleto = "Luis", Correo = "l@t.com" };

        await _servicio.CrearAsync(dto1, TestContext.Current.CancellationToken);
        await _servicio.CrearAsync(dto2, TestContext.Current.CancellationToken);
        await _servicio.CrearAsync(dto3, TestContext.Current.CancellationToken);

        // Act
        var pagina = await _servicio.ListarPorEmpresaAsync(e.Id, new Consulta { Buscar = "PeReZ" }, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(2, pagina.Total);
    }

    [Fact]
    public async Task AsociarEmpresaAsync_ColaboradorInexistente_LanzaNoEncontradoException()
    {
        // Arrange
        var dto = new AsociarEmpresaDto { EmpresaId = 1, FechaIngreso = new DateOnly(2025, 1, 1) };
        // Act & Assert
        await Assert.ThrowsAsync<NoEncontradoException>(() => _servicio.AsociarEmpresaAsync(9999, dto, TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task AsociarEmpresaAsync_EmpresaInexistente_LanzaValidacionExceptionEnEmpresaId()
    {
        // Arrange
        var p = await CrearPaisPruebaAsync("Pais A14 2", "A2");
        var d = await CrearDepartamentoPruebaAsync(p, "Depto A14 2");
        var m = await CrearMunicipioPruebaAsync(d, "Muni A14 2");
        var e = await CrearEmpresaPruebaAsync(m, "NA2", "RA2", "CA2");
        var c = await _servicio.CrearAsync(DtoCrearBase(e.Id, new DateOnly(2025, 1, 1)), TestContext.Current.CancellationToken);

        var dto = new AsociarEmpresaDto { EmpresaId = 9999, FechaIngreso = new DateOnly(2025, 1, 1) };
        // Act & Assert
        var ex = await Assert.ThrowsAsync<ValidacionException>(() => _servicio.AsociarEmpresaAsync(c.Id, dto, TestContext.Current.CancellationToken));
        Assert.Equal("EmpresaId", ex.Campo);
    }

    [Fact]
    public async Task AsociarEmpresaAsync_EdadFueraDeRango_LanzaValidacionExceptionEnEmpresaId()
    {
        // Arrange
        var p1 = await CrearPaisPruebaAsync("Pais AE 1", "A3", min: 18, max: 100);
        var p2 = await CrearPaisPruebaAsync("Pais AE 2", "A4", min: 30, max: 100);
        var d1 = await CrearDepartamentoPruebaAsync(p1, "Depto AE 1");
        var d2 = await CrearDepartamentoPruebaAsync(p2, "Depto AE 2");
        var m1 = await CrearMunicipioPruebaAsync(d1, "Muni AE 1");
        var m2 = await CrearMunicipioPruebaAsync(d2, "Muni AE 2");
        var e1 = await CrearEmpresaPruebaAsync(m1, "NA3", "RA3", "CA3");
        var e2 = await CrearEmpresaPruebaAsync(m2, "NA4", "RA4", "CA4");

        var dtoCrear = DtoCrearBase(e1.Id, new DateOnly(2025, 1, 1)) with { FechaNacimiento = new DateOnly(2007, 1, 1) };
        var c = await _servicio.CrearAsync(dtoCrear, TestContext.Current.CancellationToken);

        var dto = new AsociarEmpresaDto { EmpresaId = e2.Id, FechaIngreso = new DateOnly(2025, 1, 1) };

        // Act & Assert
        var ex = await Assert.ThrowsAsync<ValidacionException>(() => _servicio.AsociarEmpresaAsync(c.Id, dto, TestContext.Current.CancellationToken));
        Assert.Equal("EmpresaId", ex.Campo);
    }

    [Fact]
    public async Task AsociarEmpresaAsync_FechaIngresoFutura_LanzaValidacionExceptionEnFechaIngreso()
    {
        // Arrange
        var p = await CrearPaisPruebaAsync("Pais AI 1", "A5");
        var d = await CrearDepartamentoPruebaAsync(p, "Depto AI 1");
        var m = await CrearMunicipioPruebaAsync(d, "Muni AI 1");
        var e1 = await CrearEmpresaPruebaAsync(m, "NA5", "RA5", "CA5");
        var e2 = await CrearEmpresaPruebaAsync(m, "NA6", "RA6", "CA6");

        var c = await _servicio.CrearAsync(DtoCrearBase(e1.Id, new DateOnly(2025, 1, 1)), TestContext.Current.CancellationToken);

        var dto = new AsociarEmpresaDto { EmpresaId = e2.Id, FechaIngreso = new DateOnly(2028, 1, 1) };

        // Act & Assert
        var ex = await Assert.ThrowsAsync<ValidacionException>(() => _servicio.AsociarEmpresaAsync(c.Id, dto, TestContext.Current.CancellationToken));
        Assert.Equal("FechaIngreso", ex.Campo);
    }

    [Fact]
    public async Task AsociarEmpresaAsync_YaAsociada_LanzaConflictoException()
    {
        // Arrange
        var p = await CrearPaisPruebaAsync("Pais AY 1", "A7");
        var d = await CrearDepartamentoPruebaAsync(p, "Depto AY 1");
        var m = await CrearMunicipioPruebaAsync(d, "Muni AY 1");
        var e = await CrearEmpresaPruebaAsync(m, "NA7", "RA7", "CA7");

        var c = await _servicio.CrearAsync(DtoCrearBase(e.Id, new DateOnly(2025, 1, 1)), TestContext.Current.CancellationToken);

        var dto = new AsociarEmpresaDto { EmpresaId = e.Id, FechaIngreso = new DateOnly(2026, 1, 1) };

        // Act & Assert
        await Assert.ThrowsAsync<ConflictoException>(() => _servicio.AsociarEmpresaAsync(c.Id, dto, TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task AsociarEmpresaAsync_Valido_AgregaEmpresaYCambiaEdadMostrada()
    {
        // Arrange
        var p1 = await CrearPaisPruebaAsync("Pais AV 1", "A8", regla: Regla29Febrero.VeintiochoDeFebrero);
        var p2 = await CrearPaisPruebaAsync("Pais AV 2", "A9", regla: Regla29Febrero.PrimeroDeMarzo);
        var d1 = await CrearDepartamentoPruebaAsync(p1, "Depto AV 1");
        var d2 = await CrearDepartamentoPruebaAsync(p2, "Depto AV 2");
        var m1 = await CrearMunicipioPruebaAsync(d1, "Muni AV 1");
        var m2 = await CrearMunicipioPruebaAsync(d2, "Muni AV 2");
        var e1 = await CrearEmpresaPruebaAsync(m1, "NA8", "RA8", "CA8");
        var e2 = await CrearEmpresaPruebaAsync(m2, "NA9", "RA9", "CA9");

        var dtoCrear = DtoCrearBase(e1.Id, new DateOnly(2025, 1, 1)) with { FechaNacimiento = new DateOnly(2004, 2, 29) };
        var c = await _servicio.CrearAsync(dtoCrear, TestContext.Current.CancellationToken);

        var dto = new AsociarEmpresaDto { EmpresaId = e2.Id, FechaIngreso = new DateOnly(2024, 1, 1), Puesto = "X" };

        // Act
        var res = await _servicio.AsociarEmpresaAsync(c.Id, dto, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(23, c.Edad);
        Assert.Equal(2, res.Empresas.Count);
        Assert.Equal(22, res.Edad);

        var guardado = await _servicio.ObtenerAsync(c.Id, TestContext.Current.CancellationToken);
        var emp2 = guardado.Empresas.First(x => x.EmpresaId == e2.Id);
        Assert.Equal("X", emp2.Puesto);
    }

    [Fact]
    public async Task ActualizarEmpresaAsync_ColaboradorInexistente_LanzaNoEncontradoException()
    {
        // Arrange
        var dto = new GuardarEmpresaColaboradorDto { FechaIngreso = new DateOnly(2025, 1, 1) };
        // Act & Assert
        await Assert.ThrowsAsync<NoEncontradoException>(() => _servicio.ActualizarEmpresaAsync(9999, 1, dto, TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task ActualizarEmpresaAsync_NoAsociada_LanzaNoEncontradoException()
    {
        // Arrange
        var p = await CrearPaisPruebaAsync("Pais AC 1", "B1");
        var d = await CrearDepartamentoPruebaAsync(p, "Depto AC 1");
        var m = await CrearMunicipioPruebaAsync(d, "Muni AC 1");
        await CrearEmpresaPruebaAsync(m, "DUMMY1", "D1", "D1");
        var e1 = await CrearEmpresaPruebaAsync(m, "NB1", "RB1", "CB1");
        var e2 = await CrearEmpresaPruebaAsync(m, "NB2", "RB2", "CB2");

        var c = await _servicio.CrearAsync(DtoCrearBase(e1.Id, new DateOnly(2025, 1, 1)), TestContext.Current.CancellationToken);

        var dto = new GuardarEmpresaColaboradorDto { FechaIngreso = new DateOnly(2025, 1, 1) };
        // Act & Assert
        await Assert.ThrowsAsync<NoEncontradoException>(() => _servicio.ActualizarEmpresaAsync(c.Id, e2.Id, dto, TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task ActualizarEmpresaAsync_FechaIngresoInvalida_LanzaValidacionExceptionEnFechaIngreso()
    {
        // Arrange
        var p = await CrearPaisPruebaAsync("Pais AC 2", "B2");
        var d = await CrearDepartamentoPruebaAsync(p, "Depto AC 2");
        var m = await CrearMunicipioPruebaAsync(d, "Muni AC 2");
        await CrearEmpresaPruebaAsync(m, "D2", "D2", "D2");
        var e = await CrearEmpresaPruebaAsync(m, "NB3", "RB3", "CB3");

        var c = await _servicio.CrearAsync(DtoCrearBase(e.Id, new DateOnly(2025, 1, 1)), TestContext.Current.CancellationToken);

        var dto = new GuardarEmpresaColaboradorDto { FechaIngreso = new DateOnly(2028, 1, 1) };
        // Act & Assert
        var ex = await Assert.ThrowsAsync<ValidacionException>(() => _servicio.ActualizarEmpresaAsync(c.Id, e.Id, dto, TestContext.Current.CancellationToken));
        Assert.Equal("FechaIngreso", ex.Campo);
    }

    [Fact]
    public async Task ActualizarEmpresaAsync_Valido_GuardaCambios()
    {
        // Arrange
        var p = await CrearPaisPruebaAsync("Pais AC 3", "B3");
        var d = await CrearDepartamentoPruebaAsync(p, "Depto AC 3");
        var m = await CrearMunicipioPruebaAsync(d, "Muni AC 3");
        await CrearEmpresaPruebaAsync(m, "D3", "D3", "D3");
        var e = await CrearEmpresaPruebaAsync(m, "NB4", "RB4", "CB4");

        var c = await _servicio.CrearAsync(DtoCrearBase(e.Id, new DateOnly(2025, 1, 1)), TestContext.Current.CancellationToken);

        var dto = new GuardarEmpresaColaboradorDto { FechaIngreso = new DateOnly(2026, 1, 1), Puesto = "Nuevo" };

        // Act
        var res = await _servicio.ActualizarEmpresaAsync(c.Id, e.Id, dto, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(new DateOnly(2026, 1, 1), res.Empresas[0].FechaIngreso);
        Assert.Equal("Nuevo", res.Empresas[0].Puesto);

        var obtenido = await _servicio.ObtenerAsync(c.Id, TestContext.Current.CancellationToken);
        Assert.Equal(new DateOnly(2026, 1, 1), obtenido.Empresas[0].FechaIngreso);
        Assert.Equal("Nuevo", obtenido.Empresas[0].Puesto);
    }

    [Fact]
    public async Task QuitarEmpresaAsync_ColaboradorInexistente_LanzaNoEncontradoException()
    {
        // Arrange
        // Act & Assert
        await Assert.ThrowsAsync<NoEncontradoException>(() => _servicio.QuitarEmpresaAsync(9999, 1, TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task QuitarEmpresaAsync_NoAsociada_LanzaNoEncontradoException()
    {
        // Arrange
        var p = await CrearPaisPruebaAsync("Pais Q 1", "Q1");
        var d = await CrearDepartamentoPruebaAsync(p, "Depto Q 1");
        var m = await CrearMunicipioPruebaAsync(d, "Muni Q 1");
        await CrearEmpresaPruebaAsync(m, "D4", "D4", "D4");
        var e1 = await CrearEmpresaPruebaAsync(m, "NQ1", "RQ1", "CQ1");
        var e2 = await CrearEmpresaPruebaAsync(m, "NQ2", "RQ2", "CQ2");

        var c = await _servicio.CrearAsync(DtoCrearBase(e1.Id, new DateOnly(2025, 1, 1)), TestContext.Current.CancellationToken);

        // Act & Assert
        await Assert.ThrowsAsync<NoEncontradoException>(() => _servicio.QuitarEmpresaAsync(c.Id, e2.Id, TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task QuitarEmpresaAsync_UltimaEmpresa_LanzaConflictoException()
    {
        // Arrange
        var p = await CrearPaisPruebaAsync("Pais Q 2", "Q2");
        var d = await CrearDepartamentoPruebaAsync(p, "Depto Q 2");
        var m = await CrearMunicipioPruebaAsync(d, "Muni Q 2");
        await CrearEmpresaPruebaAsync(m, "D5", "D5", "D5");
        var e = await CrearEmpresaPruebaAsync(m, "NQ3", "RQ3", "CQ3");

        var c = await _servicio.CrearAsync(DtoCrearBase(e.Id, new DateOnly(2025, 1, 1)), TestContext.Current.CancellationToken);

        // Act & Assert
        await Assert.ThrowsAsync<ConflictoException>(() => _servicio.QuitarEmpresaAsync(c.Id, e.Id, TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task QuitarEmpresaAsync_ConDosEmpresas_QuitaUnaYQuedaOtra()
    {
        // Arrange
        var p = await CrearPaisPruebaAsync("Pais Q 3", "Q3");
        var d = await CrearDepartamentoPruebaAsync(p, "Depto Q 3");
        var m = await CrearMunicipioPruebaAsync(d, "Muni Q 3");
        await CrearEmpresaPruebaAsync(m, "D6", "D6", "D6");
        var e1 = await CrearEmpresaPruebaAsync(m, "NQ4", "RQ4", "CQ4");
        var e2 = await CrearEmpresaPruebaAsync(m, "NQ5", "RQ5", "CQ5");

        var dtoCrear = DtoCrearBase(e1.Id, new DateOnly(2025, 1, 1)) with
        {
            Empresas = new List<AsociarEmpresaDto>
            {
                new() { EmpresaId = e1.Id, FechaIngreso = new DateOnly(2025, 1, 1) },
                new() { EmpresaId = e2.Id, FechaIngreso = new DateOnly(2025, 1, 1) }
            }
        };

        var c = await _servicio.CrearAsync(dtoCrear, TestContext.Current.CancellationToken);

        // Act
        await _servicio.QuitarEmpresaAsync(c.Id, e1.Id, TestContext.Current.CancellationToken);

        // Assert
        var obtenido = await _servicio.ObtenerAsync(c.Id, TestContext.Current.CancellationToken);
        Assert.Single(obtenido.Empresas);
        Assert.Equal(e2.Id, obtenido.Empresas[0].EmpresaId);
    }

    [Fact]
    public async Task AsociarEmpresaAsync_EdadEntraEnNuevaPeroNoEnAntigua_Permitido()
    {
        // Arrange
        var p1 = await CrearPaisPruebaAsync("Pais AE N1", "N1", min: 18, max: 25);
        var p2 = await CrearPaisPruebaAsync("Pais AE N2", "N2", min: 18, max: 100);
        var d1 = await CrearDepartamentoPruebaAsync(p1, "Depto AE N1");
        var d2 = await CrearDepartamentoPruebaAsync(p2, "Depto AE N2");
        var m1 = await CrearMunicipioPruebaAsync(d1, "Muni AE N1");
        var m2 = await CrearMunicipioPruebaAsync(d2, "Muni AE N2");
        var e1 = await CrearEmpresaPruebaAsync(m1, "NN1", "RN1", "CN1");
        var e2 = await CrearEmpresaPruebaAsync(m2, "NN2", "RN2", "CN2");

        var col = await CrearColaboradorPruebaAsync("entra@nueva.com", new DateOnly(2000, 1, 1));
        var rel = new EmpresaColaborador { EmpresaId = e1.Id, ColaboradorId = col.Id, FechaIngreso = new DateOnly(2025, 1, 1) };
        _bd.Contexto.Set<EmpresaColaborador>().Add(rel);
        await _bd.Contexto.SaveChangesAsync(TestContext.Current.CancellationToken);

        var dto = new AsociarEmpresaDto { EmpresaId = e2.Id, FechaIngreso = new DateOnly(2025, 1, 1) };

        // Act
        var res = await _servicio.AsociarEmpresaAsync(col.Id, dto, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(2, res.Empresas.Count);
    }

    [Fact]
    public async Task AsociarEmpresaAsync_ErroresMultiples_LanzaRN4AntesQueRN9()
    {
        // Arrange
        var p = await CrearPaisPruebaAsync("Pais AE M", "M1", min: 30, max: 100);
        var d = await CrearDepartamentoPruebaAsync(p, "Depto AE M");
        var m = await CrearMunicipioPruebaAsync(d, "Muni AE M");
        var e = await CrearEmpresaPruebaAsync(m, "NM1", "RM1", "CM1");

        var col = await _servicio.CrearAsync(DtoCrearBase(e.Id, new DateOnly(2025, 1, 1)) with { FechaNacimiento = new DateOnly(1990, 1, 1) }, TestContext.Current.CancellationToken);

        var p2 = await CrearPaisPruebaAsync("Pais AE M2", "M2", min: 50, max: 100);
        var d2 = await CrearDepartamentoPruebaAsync(p2, "Depto AE M2");
        var m2 = await CrearMunicipioPruebaAsync(d2, "Muni AE M2");
        var e2 = await CrearEmpresaPruebaAsync(m2, "NM2", "RM2", "CM2");

        var dto = new AsociarEmpresaDto { EmpresaId = e2.Id, FechaIngreso = new DateOnly(2028, 1, 1) }; // Futuro

        // Act & Assert
        var ex = await Assert.ThrowsAsync<ValidacionException>(() => _servicio.AsociarEmpresaAsync(col.Id, dto, TestContext.Current.CancellationToken));
        Assert.Equal("EmpresaId", ex.Campo);
    }

    [Fact]
    public async Task AsociarEmpresaAsync_EmpresaYaAsociadaConFechaIngresoFutura_LanzaRN9AntesQueConflicto()
    {
        // Arrange
        var p = await CrearPaisPruebaAsync("Pais AE E", "E1");
        var d = await CrearDepartamentoPruebaAsync(p, "Depto AE E");
        var m = await CrearMunicipioPruebaAsync(d, "Muni AE E");
        var e = await CrearEmpresaPruebaAsync(m, "NE1", "RE1", "CE1");

        var col = await _servicio.CrearAsync(DtoCrearBase(e.Id, new DateOnly(2025, 1, 1)), TestContext.Current.CancellationToken);

        var dto = new AsociarEmpresaDto { EmpresaId = e.Id, FechaIngreso = new DateOnly(2028, 1, 1) };

        // Act & Assert
        var ex = await Assert.ThrowsAsync<ValidacionException>(() => _servicio.AsociarEmpresaAsync(col.Id, dto, TestContext.Current.CancellationToken));
        Assert.Equal("FechaIngreso", ex.Campo);
    }

    [Fact]
    public async Task AsociarEmpresaAsync_FechaIngresoAnteriorANacimiento_LanzaValidacionException()
    {
        // Arrange
        var p = await CrearPaisPruebaAsync("Pais AE N", "N3");
        var d = await CrearDepartamentoPruebaAsync(p, "Depto AE N");
        var m = await CrearMunicipioPruebaAsync(d, "Muni AE N");
        var e1 = await CrearEmpresaPruebaAsync(m, "NN3", "RN3", "CN3");
        var e2 = await CrearEmpresaPruebaAsync(m, "NN4", "RN4", "CN4");

        var col = await _servicio.CrearAsync(DtoCrearBase(e1.Id, new DateOnly(2025, 1, 1)) with { FechaNacimiento = new DateOnly(2000, 1, 1) }, TestContext.Current.CancellationToken);

        var dto = new AsociarEmpresaDto { EmpresaId = e2.Id, FechaIngreso = new DateOnly(1999, 12, 31) };

        // Act & Assert
        var ex = await Assert.ThrowsAsync<ValidacionException>(() => _servicio.AsociarEmpresaAsync(col.Id, dto, TestContext.Current.CancellationToken));
        Assert.Equal("FechaIngreso", ex.Campo);
    }

    [Fact]
    public async Task ActualizarEmpresaAsync_FechaIngresoAnteriorANacimiento_LanzaValidacionException()
    {
        // Arrange
        var p = await CrearPaisPruebaAsync("Pais AE A", "A4");
        var d = await CrearDepartamentoPruebaAsync(p, "Depto AE A");
        var m = await CrearMunicipioPruebaAsync(d, "Muni AE A");
        var e = await CrearEmpresaPruebaAsync(m, "NA4", "RA4", "CA4");

        var col = await _servicio.CrearAsync(DtoCrearBase(e.Id, new DateOnly(2025, 1, 1)) with { FechaNacimiento = new DateOnly(2000, 1, 1) }, TestContext.Current.CancellationToken);

        var dto = new GuardarEmpresaColaboradorDto { FechaIngreso = new DateOnly(1999, 12, 31) };

        // Act & Assert
        var ex = await Assert.ThrowsAsync<ValidacionException>(() => _servicio.ActualizarEmpresaAsync(col.Id, e.Id, dto, TestContext.Current.CancellationToken));
        Assert.Equal("FechaIngreso", ex.Campo);
    }
}

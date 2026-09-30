# Pautas de codificación

Etapa del ciclo de vida: **codificación**. Requisitos (`ESPECIFICACION.md`) y diseño
(`PLAN.md`, `PLAN_PRUEBAS.md`, `AGENTES.md`) ya están cerrados.

Este documento define **cómo** se escribe el código, con un patrón de referencia completo
para Países. Las demás entidades siguen el mismo patrón: el evaluador debe ver una
solución consistente, no cinco estilos distintos.

> Los fragmentos son un patrón de referencia escrito antes de tener el proyecto: se adaptan
> y se compilan en la tarea correspondiente. Si algo no compila con .NET 10, se corrige
> el código y este documento.

## 1. Configuración común de los proyectos
`Directory.Build.props` en la raíz, para que todos los proyectos compartan la configuración:

```xml
<Project>
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <TreatWarningsAsErrors>true</TreatWarningsAsErrors>
    <RestorePackagesWithLockFile>true</RestorePackagesWithLockFile>
  </PropertyGroup>
</Project>
```

Además: `.editorconfig` con namespaces de archivo (`file_scoped`), y `dotnet format` antes de cada commit.

## 2. Nombres
| Elemento | Convención | Ejemplo |
|---|---|---|
| Entidad | Singular, PascalCase | `Pais`, `EmpresaColaborador` |
| DTO de lectura | `<Entidad>Dto` | `PaisDto`, `EmpresaDetalleDto` |
| DTO de escritura | `Guardar<Entidad>Dto` | `GuardarPaisDto` |
| DTO de alta con más datos que la edición | `Crear<Entidad>Dto`, hereda de `Guardar<Entidad>Dto` | `CrearColaboradorDto` (con sus empresas) |
| Interfaz de servicio | `I<Entidades>Servicio` | `IPaisesServicio` |
| Controlador | `<Entidades>Controller` | `PaisesController` |
| Configuración de EF | `<Entidad>Configuracion` | `PaisConfiguracion` |
| Métodos asíncronos | Sufijo `Async`, reciben `CancellationToken` | `CrearAsync(dto, ct)` |
| Campos privados | `_camelCase` | `_contexto` |
| Pruebas | `Metodo_Escenario_ResultadoEsperado` | `Eliminar_PaisConDepartamentos_LanzaConflicto` |

Código y base en español sin tildes; mensajes al usuario en español con tildes.

## 3. Estructura y dependencias
```
RRHH.Contratos        DTOs y consulta de paginación. Sin dependencias.
RRHH.Domain           Entidades y reglas propias. Sin dependencias.
RRHH.Application      Interfaces de servicios, servicios, excepciones → Domain, Contratos
RRHH.Infrastructure   RrhhDbContext, configuraciones, migraciones → Application, Domain
RRHH.Api              Controladores, manejo de errores → Application, Infrastructure, Contratos
RRHH.Web              Razor Pages, cliente HTTP → Contratos (sólo)
```

## 4. Patrón de referencia: Países

### Entidad (Domain)
```csharp
namespace RRHH.Domain.Entidades;

public class Pais
{
    public int Id { get; set; }
    public string Nombre { get; set; } = "";
    public string CodigoIso2 { get; set; } = "";
    public int EdadMinima { get; set; } = 18;                  // legislación de cada país (RN4)
    public int EdadMaxima { get; set; } = 100;
    public Regla29Febrero Regla29Febrero { get; set; } = Regla29Febrero.VeintiochoDeFebrero;   // RN7
    public ICollection<Departamento> Departamentos { get; set; } = [];
}
```

Regla de dominio con la fecha actual como parámetro (así se prueba sin depender del día). Los
servicios la obtienen de `TimeProvider` (incluido en .NET), inyectado por constructor:
`DateOnly.FromDateTime(reloj.GetLocalNow().DateTime)`. En las pruebas, una subclase de
`TimeProvider` con una fecha fija.
Sólo la firma: la implementación es de la tarea 4 y el tester no debe verla (pruebas de caja negra).
```csharp
namespace RRHH.Domain.Reglas;

public enum Regla29Febrero { VeintiochoDeFebrero, PrimeroDeMarzo }

public static class Edad
{
    // Años cumplidos a la fecha `hoy` (RN7). Negativa si la fecha de nacimiento es posterior.
    public static int Calcular(DateOnly fechaNacimiento, DateOnly hoy, Regla29Febrero regla);
}
```

### Configuración de EF (Infrastructure)
```csharp
public class PaisConfiguracion : IEntityTypeConfiguration<Pais>
{
    public void Configure(EntityTypeBuilder<Pais> b)
    {
        b.ToTable("Pais");
        b.HasKey(p => p.Id).HasName("PK_Pais");
        b.Property(p => p.Nombre).HasMaxLength(100).IsRequired();
        b.Property(p => p.CodigoIso2).HasMaxLength(2).IsFixedLength().IsRequired();
        b.HasIndex(p => p.Nombre).IsUnique().HasDatabaseName("UQ_Pais_Nombre");
        b.HasIndex(p => p.CodigoIso2).IsUnique().HasDatabaseName("UQ_Pais_CodigoIso2");
    }
}

// En DepartamentoConfiguracion: nunca borrado en cascada en la geografía (RN1)
b.HasOne(d => d.Pais).WithMany(p => p.Departamentos)
 .HasForeignKey(d => d.PaisId)
 .OnDelete(DeleteBehavior.Restrict)
 .HasConstraintName("FK_Departamento_Pais");
```

### DTOs (Contratos)
Las validaciones de formato van en el DTO; las reglas de negocio, en el servicio.
Los DTOs que se validan (los de escritura y `Consulta`) son records con propiedades `init`, con los
atributos en las propiedades: así los valida la API y también `Validator.TryValidateObject` en las
pruebas unitarias. En un record posicional los atributos quedan en los parámetros y ese validador
no los ve (E-012). Los DTOs de lectura pueden ser posicionales.
Un número obligatorio va como `int?` con `[Required]`: con `int`, un dato no enviado llega como 0
(que puede ser un valor válido) y no se puede responder 400.
```csharp
namespace RRHH.Contratos.Paises;

public record PaisDto(int Id, string Nombre, string CodigoIso2);

public record GuardarPaisDto
{
    [Required, StringLength(100)]
    public string? Nombre { get; init; }

    [Required, RegularExpression("^[A-Za-z]{2}$", ErrorMessage = "Debe tener 2 letras.")]
    public string? CodigoIso2 { get; init; }
}

// Comunes (RRHH.Contratos.Comun)
public record Consulta
{
    [Range(1, int.MaxValue)] public int Pagina { get; init; } = 1;
    [Range(1, 100)] public int Tamanio { get; init; } = 20;
    [StringLength(100)] public string? Buscar { get; init; }
}
public record Pagina<T>(IReadOnlyList<T> Elementos, int Total, int Numero, int Tamanio);
```

### Contrato del servicio (Application)
Es lo que se aprueba en el paso 1 del ciclo, y lo único que ve el tester.
```csharp
public interface IPaisesServicio
{
    Task<Pagina<PaisDto>> ListarAsync(Consulta consulta, CancellationToken ct);
    Task<PaisDto> ObtenerAsync(int id, CancellationToken ct);                          // 404 si no existe
    Task<PaisDto> CrearAsync(GuardarPaisDto dto, CancellationToken ct);                // 409 si duplica (RN5)
    Task<PaisDto> ActualizarAsync(int id, GuardarPaisDto dto, CancellationToken ct);   // 404, 409
    Task EliminarAsync(int id, CancellationToken ct);                                  // 404; 409 con departamentos (RN1)
}
```

Errores de negocio como excepciones propias, traducidas a ProblemDetails en un solo lugar:
```csharp
public class NoEncontradoException(string recurso, int id)
    : Exception($"No existe {recurso} con id {id}.");
public class ConflictoException(string mensaje) : Exception(mensaje);
public class ValidacionException(string campo, string mensaje) : Exception(mensaje)   // 400 en ese campo
{
    public string Campo { get; } = campo;
}
```

Acceso a datos desde Application, a través de una interfaz (para no depender de Infrastructure):
```csharp
public interface IRrhhDbContext
{
    DbSet<Pais> Paises { get; }
    DbSet<Departamento> Departamentos { get; }
    // ...
    Task<int> SaveChangesAsync(CancellationToken ct);
}
```

### Servicio (Application)
```csharp
public class PaisesServicio(IRrhhDbContext db) : IPaisesServicio
{
    public Task<Pagina<PaisDto>> ListarAsync(Consulta c, CancellationToken ct) =>
        db.Paises.AsNoTracking()
          .Where(p => c.Buscar == null
                      || EF.Functions.Like(p.Nombre, Busqueda.PatronContiene(c.Buscar), Busqueda.Escape))
          .OrderBy(p => p.Nombre)
          .Select(p => new PaisDto(p.Id, p.Nombre, p.CodigoIso2))
          .PaginarAsync(c, ct);                     // extensión común para Skip/Take y total

    public async Task<PaisDto> CrearAsync(GuardarPaisDto dto, CancellationToken ct)
    {
        var nombre = dto.Nombre!.Trim();
        var codigo = dto.CodigoIso2!.Trim().ToUpperInvariant();
        // Sin UPPER(): la intercalación de la columna ya no distingue mayúsculas, y así se usa el
        // índice. Una consulta por dato, para que el mensaje diga cuál se repite.
        if (await db.Paises.AnyAsync(p => p.Nombre == nombre, ct))
            throw new ConflictoException($"Ya existe un país con el nombre «{nombre}».");
        if (await db.Paises.AnyAsync(p => p.CodigoIso2 == codigo, ct))
            throw new ConflictoException($"Ya existe un país con el código ISO «{codigo}».");

        var pais = new Pais { Nombre = nombre, CodigoIso2 = codigo };
        db.Paises.Add(pais);
        await db.SaveChangesAsync(ct);
        return new PaisDto(pais.Id, pais.Nombre, pais.CodigoIso2);
    }

    public async Task EliminarAsync(int id, CancellationToken ct)
    {
        var pais = await db.Paises.FindAsync([id], ct) ?? throw new NoEncontradoException("un país", id);
        if (await db.Departamentos.AnyAsync(d => d.PaisId == id, ct))
            throw new ConflictoException("El país tiene departamentos y no se puede eliminar.");
        db.Paises.Remove(pais);
        await db.SaveChangesAsync(ct);
    }
    // ObtenerAsync y ActualizarAsync con el mismo estilo
}
```
Lecturas: `AsNoTracking()` y proyección directa al DTO. Nunca devolver entidades.
Comparaciones de texto: nunca `ToUpper()`/`ToLower()` sobre columnas (impiden usar los índices).
Las columnas de texto tienen intercalación sin mayúsculas (`Modern_Spanish_CI_AS` en SQL Server,
`NOCASE` en SQLite), definida en `RrhhDbContext`; las búsquedas usan `EF.Functions.Like` con
`Busqueda.PatronContiene`, que escapa los comodines del texto buscado.

### Controlador (Api)
Delgado: sin lógica de negocio.
```csharp
[ApiController]
[Route("api/paises")]
public class PaisesController(IPaisesServicio servicio) : ControllerBase
{
    [HttpGet]
    public Task<Pagina<PaisDto>> Listar([FromQuery] Consulta consulta, CancellationToken ct)
        => servicio.ListarAsync(consulta, ct);

    [HttpGet("{id:int}")]
    public Task<PaisDto> Obtener(int id, CancellationToken ct) => servicio.ObtenerAsync(id, ct);

    [HttpPost]
    public async Task<ActionResult<PaisDto>> Crear(GuardarPaisDto dto, CancellationToken ct)
    {
        var creado = await servicio.CrearAsync(dto, ct);
        return CreatedAtAction(nameof(Obtener), new { id = creado.Id }, creado);   // 201 + Location
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Eliminar(int id, CancellationToken ct)
    {
        await servicio.EliminarAsync(id, ct);
        return NoContent();                                                        // 204
    }
}
```

### Manejo de errores (Api)
```csharp
public class ManejadorExcepciones(IProblemDetailsService problemas) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext ctx, Exception ex, CancellationToken ct)
    {
        ProblemDetails? problema = ex switch
        {
            NoEncontradoException => new() { Status = 404, Title = "Recurso no encontrado", Detail = ex.Message },
            ConflictoException    => new() { Status = 409, Title = "Conflicto", Detail = ex.Message },
            // Violación de un único o de una clave foránea que se coló a la validación del servicio
            DbUpdateException     => new() { Status = 409, Title = "Conflicto",
                                             Detail = "La operación entra en conflicto con datos existentes." },
            // Regla del servicio sobre un campo (V4, RN4): mismo formato que los errores del DTO
            ValidacionException v => new ValidationProblemDetails(
                                         new Dictionary<string, string[]> { [v.Campo] = [v.Message] })
                                     { Status = 400, Title = "Datos inválidos" },
            _ => null
        };
        if (problema is null) return false;     // lo no previsto: 500 genérico, sin detalles internos

        ctx.Response.StatusCode = problema.Status!.Value;
        return await problemas.TryWriteAsync(new ProblemDetailsContext { HttpContext = ctx, ProblemDetails = problema });
    }
}

// Program.cs
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<ManejadorExcepciones>();
// ...
app.UseExceptionHandler();

// No hace falta agregar `public partial class Program;` para WebApplicationFactory<Program>:
// desde .NET 10 el SDK genera la clase Program como pública.
```
Los errores de formato de los DTOs los devuelve `[ApiController]` solo, como `ValidationProblem` (400).

## 5. Pruebas

### Base de datos para pruebas unitarias
```csharp
public sealed class BaseDatosPrueba : IDisposable
{
    private readonly SqliteConnection _conexion = new("DataSource=:memory:");
    public RrhhDbContext Contexto { get; }

    public BaseDatosPrueba()
    {
        _conexion.Open();                    // la base en memoria vive mientras la conexión esté abierta
        var opciones = new DbContextOptionsBuilder<RrhhDbContext>().UseSqlite(_conexion).Options;
        Contexto = new RrhhDbContext(opciones);
        Contexto.Database.EnsureCreated();   // esquema desde el modelo (las migraciones son de SQL Server)
    }

    public void Dispose() { Contexto.Dispose(); _conexion.Dispose(); }
}
```

### Ejemplo de prueba unitaria
Ojo: `EnsureCreated` también carga los datos iniciales (Guatemala). Las pruebas usan
datos propios que no choquen con ellos.
```csharp
public class PaisesServicioTests : IDisposable
{
    private readonly BaseDatosPrueba _bd = new();
    private PaisesServicio Servicio => new(_bd.Contexto);

    [Fact]
    public async Task Eliminar_PaisConDepartamentos_LanzaConflicto()
    {
        // Arrange
        var pais = new Pais { Nombre = "País de prueba", CodigoIso2 = "ZZ" };
        pais.Departamentos.Add(new Departamento { Nombre = "Departamento de prueba" });
        _bd.Contexto.Paises.Add(pais);
        await _bd.Contexto.SaveChangesAsync();

        // Act
        var accion = () => Servicio.EliminarAsync(pais.Id, CancellationToken.None);

        // Assert
        await Assert.ThrowsAsync<ConflictoException>(accion);
    }

    public void Dispose() => _bd.Dispose();
}
```

### Fábrica para pruebas de integración
```csharp
public class FabricaApi : WebApplicationFactory<Program>
{
    private readonly SqliteConnection _conexion = new("DataSource=:memory:");

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        _conexion.Open();
        builder.ConfigureServices(services =>
        {
            // Quitar la configuración de SQL Server. Desde EF Core 9 también hay que quitar
            // IDbContextOptionsConfiguration<T>; si no, quedan los dos proveedores registrados.
            services.RemoveAll<DbContextOptions<RrhhDbContext>>();
            services.RemoveAll<IDbContextOptionsConfiguration<RrhhDbContext>>();
            services.AddDbContext<RrhhDbContext>(o => o.UseSqlite(_conexion));

            using var scope = services.BuildServiceProvider().CreateScope();
            scope.ServiceProvider.GetRequiredService<RrhhDbContext>().Database.EnsureCreated();
        });
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        _conexion.Dispose();
    }
}
```

## 6. Web (Razor Pages)
- Sin lógica de negocio: todo pasa por la API con un `HttpClient` tipado.
- Los errores de la API (`ProblemDetails` y `ValidationProblemDetails`) se muestran en el
  formulario, agregándolos a `ModelState`.
- Listas en cascada país → departamento → municipio con los endpoints
  `/api/paises/{id}/departamentos` y `/api/departamentos/{id}/municipios`.

```csharp
public class ClienteRrhh(HttpClient http)
{
    public async Task<(PaisDto? Creado, ValidationProblemDetails? Error)> CrearPaisAsync(GuardarPaisDto dto)
    {
        var r = await http.PostAsJsonAsync("api/paises", dto);
        return r.IsSuccessStatusCode
            ? (await r.Content.ReadFromJsonAsync<PaisDto>(), null)
            : (null, await r.Content.ReadFromJsonAsync<ValidationProblemDetails>());
    }
}
```

## 7. Terminado de cada tarea
- [ ] Compila sin advertencias (`TreatWarningsAsErrors`)
- [ ] `dotnet format` sin cambios pendientes
- [ ] Pruebas en verde; reglas nuevas en la tabla de trazabilidad
- [ ] Sin secretos ni cadenas de conexión con contraseña
- [ ] Postman actualizado si cambió un endpoint
- [ ] Commit con el mensaje de `TAREAS.md`, en la rama de la fase

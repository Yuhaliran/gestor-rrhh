using System.Linq.Expressions;

using Microsoft.EntityFrameworkCore;

using RRHH.Application.Comun;
using RRHH.Application.Datos;
using RRHH.Application.Excepciones;
using RRHH.Application.Interfaces;
using RRHH.Contratos.Colaboradores;
using RRHH.Contratos.Comun;
using RRHH.Domain.Entidades;
using RRHH.Domain.Reglas;

namespace RRHH.Application.Servicios;

public class ColaboradoresServicio(IRrhhDbContext db, TimeProvider reloj) : IColaboradoresServicio
{
    // Lo que se lee de la base para armar el DTO. La edad se calcula en memoria: Edad.Calcular no
    // se traduce a SQL, y necesita la regla del 29 de febrero del país de cada empresa.
    private sealed record Fila(int Id, string NombreCompleto, DateOnly FechaNacimiento, string Telefono,
        string Correo, List<FilaEmpresa> Empresas);

    private sealed record FilaEmpresa(int EmpresaId, string NombreComercial, int PaisId, string PaisNombre,
        DateOnly FechaIngreso, string? Puesto, Regla29Febrero Regla);

    // Rango de edad del país de una empresa (RN4)
    private sealed record RangoPais(int EmpresaId, string Nombre, int EdadMinima, int EdadMaxima, Regla29Febrero Regla);

    // Las empresas van de la más antigua a la más nueva; con la misma fecha, por id (RN7)
    private static readonly Expression<Func<Colaborador, Fila>> AFila = c => new Fila(
        c.Id, c.NombreCompleto, c.FechaNacimiento, c.Telefono, c.Correo,
        c.Empresas
            .OrderBy(ec => ec.FechaIngreso).ThenBy(ec => ec.EmpresaId)
            .Select(ec => new FilaEmpresa(ec.EmpresaId, ec.Empresa.NombreComercial,
                ec.Empresa.Municipio.Departamento.PaisId, ec.Empresa.Municipio.Departamento.Pais.Nombre,
                ec.FechaIngreso, ec.Puesto, ec.Empresa.Municipio.Departamento.Pais.Regla29Febrero))
            .ToList());

    private static readonly Expression<Func<Empresa, RangoPais>> ARango = e => new RangoPais(
        e.Id, e.Municipio.Departamento.Pais.Nombre, e.Municipio.Departamento.Pais.EdadMinima,
        e.Municipio.Departamento.Pais.EdadMaxima, e.Municipio.Departamento.Pais.Regla29Febrero);

    public async Task<Pagina<ColaboradorDto>> ListarAsync(Consulta consulta, CancellationToken ct)
    {
        var colaboradores = db.Colaboradores.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(consulta.Buscar))
        {
            var patron = Busqueda.PatronContiene(consulta.Buscar);
            colaboradores = colaboradores.Where(c => EF.Functions.Like(c.NombreCompleto, patron, Busqueda.Escape)
                || EF.Functions.Like(c.Correo, patron, Busqueda.Escape));
        }
        // El nombre no es único: el id desempata para que la paginación sea estable
        var pagina = await colaboradores.OrderBy(c => c.NombreCompleto).ThenBy(c => c.Id)
            .Select(AFila).PaginarAsync(consulta, ct);
        var hoy = Hoy();
        return new Pagina<ColaboradorDto>(pagina.Elementos.Select(f => ADto(f, hoy)).ToList(),
            pagina.Total, pagina.Numero, pagina.Tamanio);
    }

    public async Task<ColaboradorDto> ObtenerAsync(int id, CancellationToken ct)
    {
        var fila = await db.Colaboradores.AsNoTracking().Where(c => c.Id == id).Select(AFila).FirstOrDefaultAsync(ct)
            ?? throw new NoEncontradoException("un colaborador", id);
        return ADto(fila, Hoy());
    }

    public async Task<ColaboradorDto> CrearAsync(CrearColaboradorDto dto, CancellationToken ct)
    {
        var hoy = Hoy();
        var fechaNacimiento = dto.FechaNacimiento!.Value;
        var empresas = dto.Empresas!;
        var ids = empresas.Select(e => e.EmpresaId!.Value).ToList();
        var rangos = await db.Empresas.Where(e => ids.Contains(e.Id))
            .Select(ARango)
            .ToDictionaryAsync(r => r.EmpresaId, ct);

        // Primero los 400: V4, RN4 y RN9. RN4 antes que RN9 para que un nacimiento futuro (edad
        // negativa, RN7) se marque en FechaNacimiento y no en las fechas de ingreso.
        for (var i = 0; i < empresas.Count; i++)
        {
            if (!rangos.ContainsKey(ids[i]))
            {
                throw new ValidacionException($"Empresas[{i}].EmpresaId", $"No existe una empresa con id {ids[i]}.");
            }
        }
        ValidarEdad(fechaNacimiento, hoy, rangos.Values);
        for (var i = 0; i < empresas.Count; i++)
        {
            ValidarFechaIngreso(empresas[i].FechaIngreso!.Value, fechaNacimiento, hoy, $"Empresas[{i}].FechaIngreso");
        }

        // Después los 409 (RN5)
        if (ids.Distinct().Count() != ids.Count)
        {
            throw new ConflictoException("Una misma empresa no puede estar dos veces en un colaborador.");
        }
        var correo = dto.Correo!.Trim();
        await ValidarCorreoAsync(correo, idPropio: 0, ct);

        var colaborador = new Colaborador();
        Copiar(dto, correo, colaborador);
        foreach (var empresa in empresas)
        {
            colaborador.Empresas.Add(new EmpresaColaborador
            {
                EmpresaId = empresa.EmpresaId!.Value,
                FechaIngreso = empresa.FechaIngreso!.Value,
                Puesto = string.IsNullOrWhiteSpace(empresa.Puesto) ? null : empresa.Puesto.Trim()
            });
        }
        db.Colaboradores.Add(colaborador);
        await db.SaveChangesAsync(ct);
        return await ObtenerAsync(colaborador.Id, ct);
    }

    public async Task<ColaboradorDto> ActualizarAsync(int id, GuardarColaboradorDto dto, CancellationToken ct)
    {
        var colaborador = await db.Colaboradores.FindAsync([id], ct)
            ?? throw new NoEncontradoException("un colaborador", id);

        // RN4 y RN9 sólo si cambia la fecha de nacimiento (ESPECIFICACION.md, «RN4 al editar»)
        var fechaNacimiento = dto.FechaNacimiento!.Value;
        if (fechaNacimiento != colaborador.FechaNacimiento)
        {
            var rangos = await db.EmpresasColaboradores
                .Where(ec => ec.ColaboradorId == id)
                .Select(ec => ec.Empresa)
                .Select(ARango)
                .ToListAsync(ct);
            ValidarEdad(fechaNacimiento, Hoy(), rangos);
            if (await db.EmpresasColaboradores.AnyAsync(ec => ec.ColaboradorId == id && ec.FechaIngreso < fechaNacimiento, ct))
            {
                throw new ValidacionException(nameof(dto.FechaNacimiento),
                    "La fecha de nacimiento no puede ser posterior a una fecha de ingreso.");   // RN9
            }
        }

        var correo = dto.Correo!.Trim();
        await ValidarCorreoAsync(correo, idPropio: id, ct);

        Copiar(dto, correo, colaborador);
        await db.SaveChangesAsync(ct);
        return await ObtenerAsync(id, ct);
    }

    public async Task EliminarAsync(int id, CancellationToken ct)
    {
        var colaborador = await db.Colaboradores.FindAsync([id], ct)
            ?? throw new NoEncontradoException("un colaborador", id);
        db.Colaboradores.Remove(colaborador);   // sus relaciones con empresas se borran en cascada
        await db.SaveChangesAsync(ct);
    }

    // Contrato de la tarea 14: se implementa después de las pruebas del tester.
    public Task<Pagina<ColaboradorDto>> ListarPorEmpresaAsync(int empresaId, Consulta consulta, CancellationToken ct) =>
        throw Pendiente();

    public Task<ColaboradorDto> AsociarEmpresaAsync(int id, AsociarEmpresaDto dto, CancellationToken ct) => throw Pendiente();

    public Task<ColaboradorDto> ActualizarEmpresaAsync(int id, int empresaId, GuardarEmpresaColaboradorDto dto, CancellationToken ct) =>
        throw Pendiente();

    public Task QuitarEmpresaAsync(int id, int empresaId, CancellationToken ct) => throw Pendiente();

    private static NotImplementedException Pendiente() =>
        new("Contrato de la tarea 14: se implementa después de las pruebas.");

    private DateOnly Hoy() => DateOnly.FromDateTime(reloj.GetLocalNow().DateTime);

    // La edad que se muestra usa la regla del país de la empresa más antigua (RN7): Empresas ya
    // viene ordenada. Sin empresas (no debería pasar, RN3), la regla por defecto.
    private static ColaboradorDto ADto(Fila f, DateOnly hoy)
    {
        var regla = f.Empresas.Count > 0 ? f.Empresas[0].Regla : Regla29Febrero.VeintiochoDeFebrero;
        var empresas = f.Empresas
            .Select(e => new EmpresaColaboradorDto(e.EmpresaId, e.NombreComercial, e.PaisId, e.PaisNombre, e.FechaIngreso, e.Puesto))
            .ToList();
        return new ColaboradorDto(f.Id, f.NombreCompleto, f.FechaNacimiento, Edad.Calcular(f.FechaNacimiento, hoy, regla),
            f.Telefono, f.Correo, empresas);
    }

    // RN9: la fecha de ingreso no es futura ni anterior al nacimiento
    private static void ValidarFechaIngreso(DateOnly fechaIngreso, DateOnly fechaNacimiento, DateOnly hoy, string campo)
    {
        if (fechaIngreso > hoy)
        {
            throw new ValidacionException(campo, "La fecha de ingreso no puede ser posterior a hoy.");
        }
        if (fechaIngreso < fechaNacimiento)
        {
            throw new ValidacionException(campo, "La fecha de ingreso no puede ser anterior a la fecha de nacimiento.");
        }
    }

    // RN4: la edad de hoy, con la regla de cada país, dentro del rango de cada país
    private static void ValidarEdad(DateOnly fechaNacimiento, DateOnly hoy, IEnumerable<RangoPais> rangos)
    {
        foreach (var rango in rangos)
        {
            var edad = Edad.Calcular(fechaNacimiento, hoy, rango.Regla);
            if (edad < rango.EdadMinima || edad > rango.EdadMaxima)
            {
                throw new ValidacionException("FechaNacimiento",
                    $"La edad ({edad}) está fuera del rango de {rango.Nombre} ({rango.EdadMinima} a {rango.EdadMaxima}).");
            }
        }
    }

    // RN5: el correo no se repite (sin distinguir mayúsculas, por la intercalación de la columna)
    private async Task ValidarCorreoAsync(string correo, int idPropio, CancellationToken ct)
    {
        if (await db.Colaboradores.AnyAsync(c => c.Id != idPropio && c.Correo == correo, ct))
        {
            throw new ConflictoException($"Ya existe un colaborador con el correo «{correo}».");
        }
    }

    // Datos personales, ya validados por la API, sin espacios en los extremos
    private static void Copiar(GuardarColaboradorDto dto, string correo, Colaborador colaborador)
    {
        colaborador.NombreCompleto = dto.NombreCompleto!.Trim();
        colaborador.FechaNacimiento = dto.FechaNacimiento!.Value;
        colaborador.Telefono = dto.Telefono!.Trim();
        colaborador.Correo = correo;
    }
}

using System.Linq.Expressions;

using Microsoft.EntityFrameworkCore;

using RRHH.Application.Comun;
using RRHH.Application.Datos;
using RRHH.Application.Excepciones;
using RRHH.Application.Interfaces;
using RRHH.Contratos.Comun;
using RRHH.Contratos.Municipios;
using RRHH.Domain.Entidades;

namespace RRHH.Application.Servicios;

public class MunicipiosServicio(IRrhhDbContext db) : IMunicipiosServicio
{
    // Proyección para las consultas (EF la traduce, con los JOIN al departamento y al país) y su
    // versión compilada para una entidad en memoria (con su departamento y país cargados)
    private static readonly Expression<Func<Municipio, MunicipioDto>> ADto = m =>
        new MunicipioDto(m.Id, m.Nombre, m.DepartamentoId, m.Departamento.Nombre,
            m.Departamento.PaisId, m.Departamento.Pais.Nombre);
    private static readonly Func<Municipio, MunicipioDto> ADtoEnMemoria = ADto.Compile();

    public Task<Pagina<MunicipioDto>> ListarAsync(Consulta consulta, CancellationToken ct)
    {
        var municipios = db.Municipios.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(consulta.Buscar))
        {
            var patron = Busqueda.PatronContiene(consulta.Buscar);
            municipios = municipios.Where(m => EF.Functions.Like(m.Nombre, patron, Busqueda.Escape));
        }
        // El nombre es único sólo dentro del departamento, y el del departamento dentro del país:
        // con los tres, el orden es total y la paginación, estable
        return municipios
            .OrderBy(m => m.Departamento.Pais.Nombre)
            .ThenBy(m => m.Departamento.Nombre)
            .ThenBy(m => m.Nombre)
            .Select(ADto)
            .PaginarAsync(consulta, ct);
    }

    public async Task<IReadOnlyList<MunicipioDto>> ListarPorDepartamentoAsync(int departamentoId, CancellationToken ct)
    {
        if (!await db.Departamentos.AnyAsync(d => d.Id == departamentoId, ct))
        {
            throw new NoEncontradoException("un departamento", departamentoId);
        }
        return await db.Municipios.AsNoTracking()
            .Where(m => m.DepartamentoId == departamentoId)
            .OrderBy(m => m.Nombre)
            .Select(ADto)
            .ToListAsync(ct);
    }

    public async Task<MunicipioDto> ObtenerAsync(int id, CancellationToken ct) =>
        await db.Municipios.AsNoTracking().Where(m => m.Id == id).Select(ADto).FirstOrDefaultAsync(ct)
        ?? throw new NoEncontradoException("un municipio", id);

    public async Task<MunicipioDto> CrearAsync(GuardarMunicipioDto dto, CancellationToken ct)
    {
        var municipio = new Municipio();
        await CopiarAsync(dto, municipio, ct);
        db.Municipios.Add(municipio);
        await db.SaveChangesAsync(ct);
        return ADtoEnMemoria(municipio);
    }

    public async Task<MunicipioDto> ActualizarAsync(int id, GuardarMunicipioDto dto, CancellationToken ct)
    {
        var municipio = await db.Municipios.FindAsync([id], ct)
            ?? throw new NoEncontradoException("un municipio", id);
        if (dto.DepartamentoId != municipio.DepartamentoId)
        {
            throw new ValidacionException(nameof(dto.DepartamentoId), "El departamento de un municipio no se puede cambiar.");   // RN8
        }
        await CopiarAsync(dto, municipio, ct);
        await db.SaveChangesAsync(ct);
        return ADtoEnMemoria(municipio);
    }

    public async Task EliminarAsync(int id, CancellationToken ct)
    {
        var municipio = await db.Municipios.FindAsync([id], ct)
            ?? throw new NoEncontradoException("un municipio", id);
        if (await db.Empresas.AnyAsync(e => e.MunicipioId == id, ct))
        {
            throw new ConflictoException("El municipio tiene empresas y no se puede eliminar.");
        }
        db.Municipios.Remove(municipio);
        await db.SaveChangesAsync(ct);
    }

    // Valida el departamento (V4) y el nombre dentro del departamento (RN5), y copia los datos del
    // DTO (ya validado por la API). Los duplicados no distinguen mayúsculas (intercalación de la
    // columna) ni espacios en los extremos.
    private async Task CopiarAsync(GuardarMunicipioDto dto, Municipio municipio, CancellationToken ct)
    {
        var departamentoId = dto.DepartamentoId!.Value;
        var nombre = dto.Nombre!.Trim();

        var departamento = await db.Departamentos.Include(d => d.Pais).FirstOrDefaultAsync(d => d.Id == departamentoId, ct)
            ?? throw new ValidacionException(nameof(dto.DepartamentoId), $"No existe un departamento con id {departamentoId}.");
        if (await db.Municipios.AnyAsync(m => m.Id != municipio.Id && m.DepartamentoId == departamentoId && m.Nombre == nombre, ct))
        {
            throw new ConflictoException($"Ya existe un municipio con el nombre «{nombre}» en {departamento.Nombre}.");
        }

        municipio.Nombre = nombre;
        municipio.DepartamentoId = departamentoId;
        municipio.Departamento = departamento;   // cargado con su país: el DTO devuelto lleva los nombres
    }
}

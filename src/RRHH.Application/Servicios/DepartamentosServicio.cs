using System.Linq.Expressions;

using Microsoft.EntityFrameworkCore;

using RRHH.Application.Comun;
using RRHH.Application.Datos;
using RRHH.Application.Excepciones;
using RRHH.Application.Interfaces;
using RRHH.Contratos.Comun;
using RRHH.Contratos.Departamentos;
using RRHH.Domain.Entidades;

namespace RRHH.Application.Servicios;

public class DepartamentosServicio(IRrhhDbContext db) : IDepartamentosServicio
{
    // Proyección para las consultas (EF la traduce, con el JOIN al país) y su versión compilada
    // para una entidad en memoria (con su país cargado)
    private static readonly Expression<Func<Departamento, DepartamentoDto>> ADto = d =>
        new DepartamentoDto(d.Id, d.Nombre, d.PaisId, d.Pais.Nombre);
    private static readonly Func<Departamento, DepartamentoDto> ADtoEnMemoria = ADto.Compile();

    public Task<Pagina<DepartamentoDto>> ListarAsync(Consulta consulta, CancellationToken ct)
    {
        var departamentos = db.Departamentos.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(consulta.Buscar))
        {
            var patron = Busqueda.PatronContiene(consulta.Buscar);
            departamentos = departamentos.Where(d => EF.Functions.Like(d.Nombre, patron, Busqueda.Escape));
        }
        // El nombre es único sólo dentro del país: con el país primero, el orden es total y la
        // paginación, estable
        return departamentos.OrderBy(d => d.Pais.Nombre).ThenBy(d => d.Nombre).Select(ADto).PaginarAsync(consulta, ct);
    }

    public async Task<IReadOnlyList<DepartamentoDto>> ListarPorPaisAsync(int paisId, CancellationToken ct)
    {
        if (!await db.Paises.AnyAsync(p => p.Id == paisId, ct))
        {
            throw new NoEncontradoException("un país", paisId);
        }
        return await db.Departamentos.AsNoTracking()
            .Where(d => d.PaisId == paisId)
            .OrderBy(d => d.Nombre)
            .Select(ADto)
            .ToListAsync(ct);
    }

    public async Task<DepartamentoDto> ObtenerAsync(int id, CancellationToken ct) =>
        await db.Departamentos.AsNoTracking().Where(d => d.Id == id).Select(ADto).FirstOrDefaultAsync(ct)
        ?? throw new NoEncontradoException("un departamento", id);

    public async Task<DepartamentoDto> CrearAsync(GuardarDepartamentoDto dto, CancellationToken ct)
    {
        var departamento = new Departamento();
        await CopiarAsync(dto, departamento, ct);
        db.Departamentos.Add(departamento);
        await db.SaveChangesAsync(ct);
        return ADtoEnMemoria(departamento);
    }

    public async Task<DepartamentoDto> ActualizarAsync(int id, GuardarDepartamentoDto dto, CancellationToken ct)
    {
        var departamento = await db.Departamentos.FindAsync([id], ct)
            ?? throw new NoEncontradoException("un departamento", id);
        if (dto.PaisId != departamento.PaisId)
        {
            throw new ValidacionException(nameof(dto.PaisId), "El país de un departamento no se puede cambiar.");   // RN8
        }
        await CopiarAsync(dto, departamento, ct);
        await db.SaveChangesAsync(ct);
        return ADtoEnMemoria(departamento);
    }

    public async Task EliminarAsync(int id, CancellationToken ct)
    {
        var departamento = await db.Departamentos.FindAsync([id], ct)
            ?? throw new NoEncontradoException("un departamento", id);
        if (await db.Municipios.AnyAsync(m => m.DepartamentoId == id, ct))
        {
            throw new ConflictoException("El departamento tiene municipios y no se puede eliminar.");
        }
        db.Departamentos.Remove(departamento);
        await db.SaveChangesAsync(ct);
    }

    // Valida el país (V4) y el nombre dentro del país (RN5), y copia los datos del DTO (ya validado
    // por la API). Los duplicados no distinguen mayúsculas (intercalación de la columna) ni espacios
    // en los extremos.
    private async Task CopiarAsync(GuardarDepartamentoDto dto, Departamento departamento, CancellationToken ct)
    {
        var paisId = dto.PaisId!.Value;
        var nombre = dto.Nombre!.Trim();

        var pais = await db.Paises.FindAsync([paisId], ct)
            ?? throw new ValidacionException(nameof(dto.PaisId), $"No existe un país con id {paisId}.");
        if (await db.Departamentos.AnyAsync(d => d.Id != departamento.Id && d.PaisId == paisId && d.Nombre == nombre, ct))
        {
            throw new ConflictoException($"Ya existe un departamento con el nombre «{nombre}» en {pais.Nombre}.");
        }

        departamento.Nombre = nombre;
        departamento.PaisId = paisId;
        departamento.Pais = pais;   // cargado: el DTO devuelto lleva el nombre del país
    }
}

using System.Linq.Expressions;

using Microsoft.EntityFrameworkCore;

using RRHH.Application.Comun;
using RRHH.Application.Datos;
using RRHH.Application.Excepciones;
using RRHH.Application.Interfaces;
using RRHH.Contratos.Comun;
using RRHH.Contratos.Paises;
using RRHH.Domain.Entidades;

using ReglaContrato = RRHH.Contratos.Paises.Regla29Febrero;
using ReglaDominio = RRHH.Domain.Reglas.Regla29Febrero;

namespace RRHH.Application.Servicios;

public class PaisesServicio(IRrhhDbContext db) : IPaisesServicio
{
    // Proyección para las consultas (EF la traduce) y su versión compilada para una entidad en memoria
    private static readonly Expression<Func<Pais, PaisDto>> ADto = p =>
        new PaisDto(p.Id, p.Nombre, p.CodigoIso2, p.EdadMinima, p.EdadMaxima, AContrato(p.Regla29Febrero));
    private static readonly Func<Pais, PaisDto> ADtoEnMemoria = ADto.Compile();

    public Task<Pagina<PaisDto>> ListarAsync(Consulta consulta, CancellationToken ct)
    {
        var paises = db.Paises.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(consulta.Buscar))
        {
            var buscar = consulta.Buscar.Trim().ToUpperInvariant();
            paises = paises.Where(p => p.Nombre.ToUpper().Contains(buscar) || p.CodigoIso2.Contains(buscar));
        }
        return paises.OrderBy(p => p.Nombre).Select(ADto).PaginarAsync(consulta, ct);
    }

    public async Task<PaisDto> ObtenerAsync(int id, CancellationToken ct) =>
        await db.Paises.AsNoTracking().Where(p => p.Id == id).Select(ADto).FirstOrDefaultAsync(ct)
        ?? throw new NoEncontradoException("un país", id);

    public async Task<PaisDto> CrearAsync(GuardarPaisDto dto, CancellationToken ct)
    {
        var pais = new Pais();
        await CopiarAsync(dto, pais, ct);
        db.Paises.Add(pais);
        await db.SaveChangesAsync(ct);
        return ADtoEnMemoria(pais);
    }

    public async Task<PaisDto> ActualizarAsync(int id, GuardarPaisDto dto, CancellationToken ct)
    {
        var pais = await db.Paises.FindAsync([id], ct) ?? throw new NoEncontradoException("un país", id);
        await CopiarAsync(dto, pais, ct);
        await db.SaveChangesAsync(ct);
        return ADtoEnMemoria(pais);
    }

    public async Task EliminarAsync(int id, CancellationToken ct)
    {
        var pais = await db.Paises.FindAsync([id], ct) ?? throw new NoEncontradoException("un país", id);
        if (await db.Departamentos.AnyAsync(d => d.PaisId == id, ct))
        {
            throw new ConflictoException("El país tiene departamentos y no se puede eliminar.");
        }
        db.Paises.Remove(pais);
        await db.SaveChangesAsync(ct);
    }

    // Normaliza los datos del DTO (ya validado por la API), revisa duplicados (RN5) y los copia.
    // Los duplicados no distinguen mayúsculas ni espacios en los extremos; sí tildes.
    private async Task CopiarAsync(GuardarPaisDto dto, Pais pais, CancellationToken ct)
    {
        var nombre = dto.Nombre!.Trim();
        var nombreComparable = nombre.ToUpperInvariant();
        var codigo = dto.CodigoIso2!.Trim().ToUpperInvariant();   // se guarda siempre en mayúsculas

        if (await db.Paises.AnyAsync(p => p.Id != pais.Id && p.Nombre.ToUpper() == nombreComparable, ct))
        {
            throw new ConflictoException($"Ya existe un país con el nombre «{nombre}».");
        }
        if (await db.Paises.AnyAsync(p => p.Id != pais.Id && p.CodigoIso2 == codigo, ct))
        {
            throw new ConflictoException($"Ya existe un país con el código ISO «{codigo}».");
        }

        pais.Nombre = nombre;
        pais.CodigoIso2 = codigo;
        pais.EdadMinima = dto.EdadMinima!.Value;
        pais.EdadMaxima = dto.EdadMaxima!.Value;
        pais.Regla29Febrero = ADominio(dto.Regla29Febrero!.Value);
    }

    // Traducción explícita entre el enum del contrato y el del dominio (no por el número del valor)
    private static ReglaContrato AContrato(ReglaDominio regla) => regla switch
    {
        ReglaDominio.VeintiochoDeFebrero => ReglaContrato.VeintiochoDeFebrero,
        ReglaDominio.PrimeroDeMarzo => ReglaContrato.PrimeroDeMarzo,
        _ => throw new ArgumentOutOfRangeException(nameof(regla), regla, "Regla del 29 de febrero desconocida.")
    };

    private static ReglaDominio ADominio(ReglaContrato regla) => regla switch
    {
        ReglaContrato.VeintiochoDeFebrero => ReglaDominio.VeintiochoDeFebrero,
        ReglaContrato.PrimeroDeMarzo => ReglaDominio.PrimeroDeMarzo,
        _ => throw new ArgumentOutOfRangeException(nameof(regla), regla, "Regla del 29 de febrero desconocida.")
    };
}

using System.Linq.Expressions;

using Microsoft.EntityFrameworkCore;

using RRHH.Application.Comun;
using RRHH.Application.Datos;
using RRHH.Application.Excepciones;
using RRHH.Application.Interfaces;
using RRHH.Contratos.Comun;
using RRHH.Contratos.Empresas;
using RRHH.Domain.Entidades;

namespace RRHH.Application.Servicios;

public class EmpresasServicio(IRrhhDbContext db) : IEmpresasServicio
{
    // Proyección para las consultas (EF la traduce, con los JOIN a la geografía) y su versión
    // compilada para una entidad en memoria (con su municipio, departamento y país cargados)
    private static readonly Expression<Func<Empresa, EmpresaDto>> ADto = e =>
        new EmpresaDto(e.Id, e.Nit, e.RazonSocial, e.NombreComercial, e.Telefono, e.Correo,
            e.MunicipioId, e.Municipio.Nombre,
            e.Municipio.DepartamentoId, e.Municipio.Departamento.Nombre,
            e.Municipio.Departamento.PaisId, e.Municipio.Departamento.Pais.Nombre);
    private static readonly Func<Empresa, EmpresaDto> ADtoEnMemoria = ADto.Compile();

    public Task<Pagina<EmpresaDto>> ListarAsync(Consulta consulta, CancellationToken ct)
    {
        var empresas = db.Empresas.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(consulta.Buscar))
        {
            var patron = Busqueda.PatronContiene(consulta.Buscar);
            empresas = empresas.Where(e => EF.Functions.Like(e.Nit, patron, Busqueda.Escape)
                || EF.Functions.Like(e.RazonSocial, patron, Busqueda.Escape)
                || EF.Functions.Like(e.NombreComercial, patron, Busqueda.Escape));
        }
        // El nombre comercial no es único: el id desempata para que la paginación sea estable
        return empresas.OrderBy(e => e.NombreComercial).ThenBy(e => e.Id).Select(ADto).PaginarAsync(consulta, ct);
    }

    public async Task<EmpresaDto> ObtenerAsync(int id, CancellationToken ct) =>
        await db.Empresas.AsNoTracking().Where(e => e.Id == id).Select(ADto).FirstOrDefaultAsync(ct)
        ?? throw new NoEncontradoException("una empresa", id);

    public async Task<EmpresaDto> CrearAsync(GuardarEmpresaDto dto, CancellationToken ct)
    {
        var empresa = new Empresa();
        await CopiarAsync(dto, empresa, paisActualId: null, ct);
        db.Empresas.Add(empresa);
        await db.SaveChangesAsync(ct);
        return ADtoEnMemoria(empresa);
    }

    public async Task<EmpresaDto> ActualizarAsync(int id, GuardarEmpresaDto dto, CancellationToken ct)
    {
        var empresa = await db.Empresas.FindAsync([id], ct)
            ?? throw new NoEncontradoException("una empresa", id);
        var paisActualId = await db.Municipios
            .Where(m => m.Id == empresa.MunicipioId)
            .Select(m => m.Departamento.PaisId)
            .FirstAsync(ct);
        await CopiarAsync(dto, empresa, paisActualId, ct);
        await db.SaveChangesAsync(ct);
        return ADtoEnMemoria(empresa);
    }

    public async Task EliminarAsync(int id, CancellationToken ct)
    {
        var empresa = await db.Empresas.FindAsync([id], ct)
            ?? throw new NoEncontradoException("una empresa", id);
        if (await db.EmpresasColaboradores.AnyAsync(ec => ec.EmpresaId == id, ct))
        {
            throw new ConflictoException("La empresa tiene colaboradores y no se puede eliminar.");
        }
        db.Empresas.Remove(empresa);
        await db.SaveChangesAsync(ct);
    }

    // Valida el municipio (V4) y, al editar, que sea del país actual (RN8: la empresa puede mudarse
    // dentro de su país); después, el NIT dentro del país (RN5). Copia los datos del DTO (ya
    // validado por la API) sin espacios en los extremos, y el NIT en mayúsculas. paisActualId es
    // null al crear.
    private async Task CopiarAsync(GuardarEmpresaDto dto, Empresa empresa, int? paisActualId, CancellationToken ct)
    {
        var municipioId = dto.MunicipioId!.Value;
        var nit = dto.Nit!.Trim().ToUpperInvariant();

        var municipio = await db.Municipios
            .Include(m => m.Departamento).ThenInclude(d => d.Pais)
            .FirstOrDefaultAsync(m => m.Id == municipioId, ct)
            ?? throw new ValidacionException(nameof(dto.MunicipioId), $"No existe un municipio con id {municipioId}.");
        var pais = municipio.Departamento.Pais;
        if (paisActualId is not null && pais.Id != paisActualId)
        {
            throw new ValidacionException(nameof(dto.MunicipioId),
                "Una empresa no puede cambiar de país: el municipio tiene que ser de su país actual.");
        }
        if (await db.Empresas.AnyAsync(e => e.Id != empresa.Id && e.Nit == nit && e.Municipio.Departamento.PaisId == pais.Id, ct))
        {
            throw new ConflictoException($"Ya existe una empresa con el NIT «{nit}» en {pais.Nombre}.");
        }

        empresa.MunicipioId = municipioId;
        empresa.Municipio = municipio;   // cargado con su geografía: el DTO devuelto lleva los nombres
        empresa.Nit = nit;
        empresa.RazonSocial = dto.RazonSocial!.Trim();
        empresa.NombreComercial = dto.NombreComercial!.Trim();
        empresa.Telefono = dto.Telefono!.Trim();
        empresa.Correo = dto.Correo!.Trim();
    }
}

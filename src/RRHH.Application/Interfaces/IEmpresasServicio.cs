using RRHH.Contratos.Comun;
using RRHH.Contratos.Empresas;

namespace RRHH.Application.Interfaces;

// Mantenimiento de empresas. El NIT es único dentro del país de la empresa (RN5): no distingue
// mayúsculas ni espacios en los extremos.
public interface IEmpresasServicio
{
    // Ordenado por nombre comercial y después por id. Buscar filtra por NIT, razón social o nombre
    // comercial, sin distinguir mayúsculas.
    Task<Pagina<EmpresaDto>> ListarAsync(Consulta consulta, CancellationToken ct);

    // NoEncontradoException (404) si no existe.
    Task<EmpresaDto> ObtenerAsync(int id, CancellationToken ct);

    // ValidacionException (400) en el campo MunicipioId si el municipio no existe (V4);
    // ConflictoException (409) si el NIT ya existe en el país del municipio (RN5). Guarda los textos
    // sin espacios en los extremos y el NIT en mayúsculas.
    Task<EmpresaDto> CrearAsync(GuardarEmpresaDto dto, CancellationToken ct);

    // Se revisa en este orden: NoEncontradoException (404) si la empresa no existe;
    // ValidacionException (400) en el campo MunicipioId si el municipio no existe (V4) o es de otro
    // país (RN8: puede mudarse dentro de su país); ConflictoException (409) si el NIT ya existe en el
    // país (RN5), sin contar a la propia empresa. Guarda como CrearAsync.
    Task<EmpresaDto> ActualizarAsync(int id, GuardarEmpresaDto dto, CancellationToken ct);

    // NoEncontradoException (404); ConflictoException (409) si tiene colaboradores (RN2).
    Task EliminarAsync(int id, CancellationToken ct);
}

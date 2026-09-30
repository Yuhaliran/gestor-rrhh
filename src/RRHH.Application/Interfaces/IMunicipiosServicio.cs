using RRHH.Contratos.Comun;
using RRHH.Contratos.Municipios;

namespace RRHH.Application.Interfaces;

// Mantenimiento de municipios. El nombre es único dentro del departamento (RN5): no distingue
// mayúsculas ni espacios en los extremos, y sí tildes.
public interface IMunicipiosServicio
{
    // Ordenado por nombre del país, del departamento y del municipio. Buscar filtra por el nombre
    // del municipio, sin distinguir mayúsculas.
    Task<Pagina<MunicipioDto>> ListarAsync(Consulta consulta, CancellationToken ct);

    // Todos los municipios del departamento, sin paginar y ordenados por nombre (listas en cascada,
    // RN6). NoEncontradoException (404) si el departamento no existe; lista vacía si no tiene municipios.
    Task<IReadOnlyList<MunicipioDto>> ListarPorDepartamentoAsync(int departamentoId, CancellationToken ct);

    // NoEncontradoException (404) si no existe.
    Task<MunicipioDto> ObtenerAsync(int id, CancellationToken ct);

    // ValidacionException (400) en el campo DepartamentoId si el departamento no existe (V4);
    // ConflictoException (409) si el nombre ya existe en el departamento (RN5). Guarda el nombre sin
    // espacios en los extremos.
    Task<MunicipioDto> CrearAsync(GuardarMunicipioDto dto, CancellationToken ct);

    // Se revisa en este orden: NoEncontradoException (404) si el municipio no existe;
    // ValidacionException (400) en el campo DepartamentoId si es distinto del actual (RN8: el
    // departamento no cambia al editar); ConflictoException (409) si el nombre ya existe en el
    // departamento (RN5), sin contar al propio municipio. Guarda el nombre sin espacios en los extremos.
    Task<MunicipioDto> ActualizarAsync(int id, GuardarMunicipioDto dto, CancellationToken ct);

    // NoEncontradoException (404); ConflictoException (409) si tiene empresas (RN1).
    Task EliminarAsync(int id, CancellationToken ct);
}

using RRHH.Contratos.Comun;
using RRHH.Contratos.Paises;

namespace RRHH.Application.Interfaces;

// Mantenimiento de países. Los duplicados (RN5) no distinguen mayúsculas ni espacios en los
// extremos, y sí tildes.
public interface IPaisesServicio
{
    // Ordenado por nombre. Buscar filtra por nombre o código ISO, sin distinguir mayúsculas.
    Task<Pagina<PaisDto>> ListarAsync(Consulta consulta, CancellationToken ct);

    // NoEncontradoException (404) si no existe.
    Task<PaisDto> ObtenerAsync(int id, CancellationToken ct);

    // ConflictoException (409) si el nombre o el código ISO ya existen (RN5).
    // Guarda el nombre sin espacios en los extremos y el código ISO en mayúsculas.
    Task<PaisDto> CrearAsync(GuardarPaisDto dto, CancellationToken ct);

    // NoEncontradoException (404); ConflictoException (409) si duplica a otro país (RN5): el propio
    // país no cuenta. Cambiar el rango de edad no revalida a los colaboradores ya registrados (RN4).
    Task<PaisDto> ActualizarAsync(int id, GuardarPaisDto dto, CancellationToken ct);

    // NoEncontradoException (404); ConflictoException (409) si tiene departamentos (RN1).
    Task EliminarAsync(int id, CancellationToken ct);
}

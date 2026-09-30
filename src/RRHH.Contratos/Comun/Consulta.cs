using System.ComponentModel.DataAnnotations;

namespace RRHH.Contratos.Comun;

// Parámetros de los listados: ?pagina=1&tamanio=20&buscar=texto (PLAN.md, «API»).
// Record con propiedades init para que la validación la vean la API y las pruebas (E-012).
public record Consulta
{
    [Range(1, int.MaxValue, ErrorMessage = "La página debe ser 1 o mayor.")]
    public int Pagina { get; init; } = 1;

    [Range(1, 100, ErrorMessage = "El tamaño de página debe estar entre 1 y 100.")]
    public int Tamanio { get; init; } = 20;

    [StringLength(100, ErrorMessage = "La búsqueda admite hasta 100 caracteres.")]
    public string? Buscar { get; init; }
}

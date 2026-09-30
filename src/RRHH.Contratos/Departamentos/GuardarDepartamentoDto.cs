using System.ComponentModel.DataAnnotations;

namespace RRHH.Contratos.Departamentos;

// Datos para crear o editar un departamento. Que el país exista (V4) y que no cambie al editar
// (RN8) lo valida el servicio.
public record GuardarDepartamentoDto
{
    [Required(ErrorMessage = "El país es obligatorio.")]
    public int? PaisId { get; init; }

    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(100, ErrorMessage = "El nombre admite hasta 100 caracteres.")]
    public string? Nombre { get; init; }
}

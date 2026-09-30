using System.ComponentModel.DataAnnotations;

namespace RRHH.Contratos.Municipios;

// Datos para crear o editar un municipio. Que el departamento exista (V4) y que no cambie al
// editar (RN8) lo valida el servicio.
public record GuardarMunicipioDto
{
    [Required(ErrorMessage = "El departamento es obligatorio.")]
    public int? DepartamentoId { get; init; }

    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(100, ErrorMessage = "El nombre admite hasta 100 caracteres.")]
    public string? Nombre { get; init; }
}

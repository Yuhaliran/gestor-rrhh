using System.ComponentModel.DataAnnotations;

namespace RRHH.Contratos.Colaboradores;

// Datos editables de la relación de un colaborador con una empresa (la empresa no cambia).
// La fecha de ingreso (RN9) la valida el servicio.
public record GuardarEmpresaColaboradorDto
{
    [Required(ErrorMessage = "La fecha de ingreso es obligatoria.")]
    public DateOnly? FechaIngreso { get; init; }

    [StringLength(100, ErrorMessage = "El puesto admite hasta 100 caracteres.")]
    public string? Puesto { get; init; }
}

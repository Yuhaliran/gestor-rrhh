using System.ComponentModel.DataAnnotations;

namespace RRHH.Contratos.Colaboradores;

// Una empresa del colaborador, al crearlo o al asociarlo después. Que la empresa exista (V4) y la
// fecha de ingreso (RN9) los valida el servicio.
public record AsociarEmpresaDto
{
    [Required(ErrorMessage = "La empresa es obligatoria.")]
    public int? EmpresaId { get; init; }

    [Required(ErrorMessage = "La fecha de ingreso es obligatoria.")]
    public DateOnly? FechaIngreso { get; init; }

    [StringLength(100, ErrorMessage = "El puesto admite hasta 100 caracteres.")]
    public string? Puesto { get; init; }
}

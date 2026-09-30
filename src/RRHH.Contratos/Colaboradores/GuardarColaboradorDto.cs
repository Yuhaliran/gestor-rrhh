using System.ComponentModel.DataAnnotations;

using RRHH.Contratos.Comun;

namespace RRHH.Contratos.Colaboradores;

// Datos personales de un colaborador: es lo que cambia al editarlo. Sus empresas se manejan con
// endpoints propios. RN4, RN9 y que el correo no se repita (RN5) los valida el servicio.
public record GuardarColaboradorDto
{
    [Required(ErrorMessage = "El nombre completo es obligatorio.")]
    [StringLength(200, ErrorMessage = "El nombre completo admite hasta 200 caracteres.")]
    public string? NombreCompleto { get; init; }

    [Required(ErrorMessage = "La fecha de nacimiento es obligatoria.")]
    public DateOnly? FechaNacimiento { get; init; }

    [Required(ErrorMessage = "El teléfono es obligatorio.")]
    [RegularExpression(Formatos.Telefono,
        ErrorMessage = "El teléfono admite dígitos, espacios, +, - y paréntesis, de 7 a 20 caracteres.")]
    public string? Telefono { get; init; }

    [Required(ErrorMessage = "El correo es obligatorio.")]
    [EmailAddress(ErrorMessage = "El correo no tiene un formato válido.")]
    [StringLength(254, ErrorMessage = "El correo admite hasta 254 caracteres.")]
    public string? Correo { get; init; }
}

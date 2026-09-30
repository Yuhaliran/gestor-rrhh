using System.ComponentModel.DataAnnotations;

namespace RRHH.Contratos.Paises;

// Datos para crear o editar un país. Todos obligatorios: la API no completa valores por defecto
// (ESPECIFICACION.md, «País»). Los números van como int? para que uno no enviado dé 400 y no 0.
public record GuardarPaisDto : IValidatableObject
{
    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(100, ErrorMessage = "El nombre admite hasta 100 caracteres.")]
    public string? Nombre { get; init; }

    [Required(ErrorMessage = "El código ISO es obligatorio.")]
    [RegularExpression("^[A-Za-z]{2}$", ErrorMessage = "El código ISO debe tener 2 letras.")]
    public string? CodigoIso2 { get; init; }

    [Required(ErrorMessage = "La edad mínima es obligatoria.")]
    [Range(0, int.MaxValue, ErrorMessage = "La edad mínima no puede ser negativa.")]
    public int? EdadMinima { get; init; }

    [Required(ErrorMessage = "La edad máxima es obligatoria.")]
    [Range(0, int.MaxValue, ErrorMessage = "La edad máxima no puede ser negativa.")]
    public int? EdadMaxima { get; init; }

    // EnumDataType rechaza un número que no corresponde a ningún valor (por ejemplo, 5)
    [Required(ErrorMessage = "La regla del 29 de febrero es obligatoria.")]
    [EnumDataType(typeof(Regla29Febrero), ErrorMessage = "La regla del 29 de febrero no es válida.")]
    public Regla29Febrero? Regla29Febrero { get; init; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (EdadMinima > EdadMaxima)
        {
            yield return new ValidationResult("La edad mínima no puede ser mayor que la máxima.", [nameof(EdadMaxima)]);
        }
    }
}

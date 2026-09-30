using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;

namespace RRHH.Web.Pages.Shared;

// Los mensajes de ASP.NET cuando un valor del formulario no se puede leer (un número con letras,
// una fecha inválida), en español. Van junto al campo, así que no lo nombran. Los de las reglas de
// los DTOs ya están en español en RRHH.Contratos.
public static class MensajesDeEnlace
{
    private const string Invalido = "El valor no es válido.";
    private const string Obligatorio = "Este dato es obligatorio.";
    private const string Numero = "Tiene que ser un número.";

    public static void EnEspaniol(DefaultModelBindingMessageProvider mensajes)
    {
        mensajes.SetAttemptedValueIsInvalidAccessor((_, _) => Invalido);
        mensajes.SetNonPropertyAttemptedValueIsInvalidAccessor(_ => Invalido);
        mensajes.SetValueIsInvalidAccessor(_ => Invalido);
        mensajes.SetUnknownValueIsInvalidAccessor(_ => Invalido);
        mensajes.SetNonPropertyUnknownValueIsInvalidAccessor(() => Invalido);
        mensajes.SetValueMustBeANumberAccessor(_ => Numero);
        mensajes.SetNonPropertyValueMustBeANumberAccessor(() => Numero);
        mensajes.SetValueMustNotBeNullAccessor(_ => Obligatorio);
        mensajes.SetMissingBindRequiredValueAccessor(_ => Obligatorio);
        mensajes.SetMissingKeyOrValueAccessor(() => Obligatorio);
        mensajes.SetMissingRequestBodyRequiredValueAccessor(() => "Faltan los datos del formulario.");
    }
}

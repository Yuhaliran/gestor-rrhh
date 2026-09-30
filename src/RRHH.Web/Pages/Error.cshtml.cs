using System.Diagnostics;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace RRHH.Web.Pages;

// Página de las excepciones (fuera de desarrollo) y de los códigos sin cuerpo que reejecuta
// UseStatusCodePagesWithReExecute: el mensaje depende del código. No tiene handlers para que
// también se muestre cuando el pedido original era un POST.
[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
[IgnoreAntiforgeryToken]
public class ErrorModel(IConfiguration configuracion) : PageModel
{
    public string RequestId => Activity.Current?.Id ?? HttpContext.TraceIdentifier;

    // El identificador sirve para buscar el error en el registro: sólo en los errores del servidor
    public bool MostrarId => HttpContext.Response.StatusCode >= StatusCodes.Status500InternalServerError;

    public (string Titulo, string Mensaje) Explicacion => HttpContext.Response.StatusCode switch
    {
        StatusCodes.Status404NotFound =>
            ("No existe", "La página o el registro que buscás no existe: puede que se haya eliminado."),
        StatusCodes.Status503ServiceUnavailable =>
            ("La API no responde", $"No se pudo conectar con la API en {configuracion["Api:UrlBase"]}. Verificá que esté en ejecución y volvé a intentarlo."),
        StatusCodes.Status502BadGateway =>
            ("La API respondió con un error", "La API no pudo completar la operación; el detalle está en su registro."),
        StatusCodes.Status400BadRequest =>
            ("Pedido no válido", "Volvé a cargar la página e intentá de nuevo."),
        _ => ("No se pudo completar la operación", "Ocurrió un error inesperado. Volvé a intentarlo.")
    };
}

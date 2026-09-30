using Microsoft.AspNetCore.Diagnostics;

namespace RRHH.Api.Errores;

// Traduce las excepciones a ProblemDetails en un solo lugar (PLAN.md, «API»):
// NoEncontradoException → 404; ConflictoException → 409; DbUpdateException (un único o una clave
// foránea que se coló a la validación del servicio) → 409 con un mensaje genérico, sin detalles
// internos; ValidacionException → 400 con el error en su campo. Otra excepción no se maneja
// (devuelve false) y la API responde 500 genérico.
public class ManejadorExcepciones(IProblemDetailsService problemas) : IExceptionHandler
{
    // Contrato de la tarea 8: todavía no maneja nada; se implementa después de las pruebas.
    public ValueTask<bool> TryHandleAsync(HttpContext ctx, Exception ex, CancellationToken ct)
    {
        _ = problemas;
        return ValueTask.FromResult(false);
    }
}

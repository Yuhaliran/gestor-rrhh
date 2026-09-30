using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using RRHH.Application.Excepciones;

namespace RRHH.Api.Errores;

// Traduce las excepciones a ProblemDetails en un solo lugar (PLAN.md, «API»).
public class ManejadorExcepciones(IProblemDetailsService problemas) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext ctx, Exception ex, CancellationToken ct)
    {
        ProblemDetails? problema = ex switch
        {
            NoEncontradoException => new ProblemDetails { Status = StatusCodes.Status404NotFound, Title = "Recurso no encontrado", Detail = ex.Message },
            ConflictoException => new ProblemDetails { Status = StatusCodes.Status409Conflict, Title = "Conflicto", Detail = ex.Message },
            // Un único o una clave foránea que se coló a la validación del servicio (dos pedidos
            // simultáneos). El mensaje de la base no se muestra: puede tener detalles internos.
            DbUpdateException => new ProblemDetails
            {
                Status = StatusCodes.Status409Conflict,
                Title = "Conflicto",
                Detail = "La operación entra en conflicto con datos existentes."
            },
            // Regla del servicio sobre un campo (V4, RN4): mismo formato que los errores del DTO
            ValidacionException v => new ValidationProblemDetails(new Dictionary<string, string[]> { [v.Campo] = [v.Message] })
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Datos inválidos"
            },
            _ => null
        };
        if (problema is null) return false;     // lo no previsto: 500 genérico, sin detalles internos

        ctx.Response.StatusCode = problema.Status!.Value;
        return await problemas.TryWriteAsync(new ProblemDetailsContext { HttpContext = ctx, ProblemDetails = problema });
    }
}

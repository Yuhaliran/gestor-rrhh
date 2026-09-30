using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace RRHH.Web.Api;

// La API no responde (apagada, sin red) o respondió con un error no previsto: en lugar de la
// excepción, un 503 o un 502 que UseStatusCodePagesWithReExecute muestra con la página de error.
// Las demás excepciones siguen su camino (en desarrollo, la página de excepciones).
public class ApiNoDisponibleFiltro(ILogger<ApiNoDisponibleFiltro> log) : IAsyncPageFilter
{
    public Task OnPageHandlerSelectionAsync(PageHandlerSelectedContext context) => Task.CompletedTask;

    public async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
    {
        var ejecutado = await next();
        if (ejecutado.Exception is HttpRequestException error && !ejecutado.ExceptionHandled)
        {
            log.LogError(error, "La API no respondió al pedido de {Ruta}", context.HttpContext.Request.Path);
            // Sin código: no hubo respuesta. Con código: la API respondió con un error (500)
            ejecutado.Result = new StatusCodeResult(error.StatusCode is null
                ? StatusCodes.Status503ServiceUnavailable
                : StatusCodes.Status502BadGateway);
            ejecutado.ExceptionHandled = true;
        }
    }
}

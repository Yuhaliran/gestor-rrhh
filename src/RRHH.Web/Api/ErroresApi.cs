using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace RRHH.Web.Api;

public static class ErroresApi
{
    // Muestra en el formulario los errores que devolvió la API (CODIFICACION.md, «Web»). Los de un
    // campo (400) van a ese campo, con el prefijo del modelo del formulario («Datos.Nombre»,
    // «Datos.Empresas[0].FechaIngreso»); los que no tienen campo (404, 409), al resumen.
    public static void AgregarProblema(this ModelStateDictionary modelState, ValidationProblemDetails problema, string prefijo)
    {
        foreach (var (campo, mensajes) in problema.Errors)
        {
            var clave = string.IsNullOrEmpty(campo) ? string.Empty : $"{prefijo}.{campo}";
            foreach (var mensaje in mensajes)
            {
                modelState.AddModelError(clave, mensaje);
            }
        }
        if (problema.Errors.Count == 0)
        {
            modelState.AddModelError(string.Empty, problema.Detail ?? problema.Title ?? "La API rechazó la operación.");
        }
    }
}

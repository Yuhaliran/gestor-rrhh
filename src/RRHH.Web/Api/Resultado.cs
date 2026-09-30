using Microsoft.AspNetCore.Mvc;

namespace RRHH.Web.Api;

// Resultado de una operación que la API puede rechazar: 400 (errores por campo), 404 o 409, con el
// ProblemDetails que devolvió. Un 404 o 409 llega sin errores por campo, con el detalle.
public record Resultado(ValidationProblemDetails? Problema)
{
    public bool Exitoso => Problema is null;
}

public record Resultado<T>(T? Valor, ValidationProblemDetails? Problema) : Resultado(Problema);

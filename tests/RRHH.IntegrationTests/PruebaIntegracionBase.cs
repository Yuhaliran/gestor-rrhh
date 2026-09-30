using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

using Microsoft.AspNetCore.Mvc;

namespace RRHH.IntegrationTests;

public abstract class PruebaIntegracionBase : IClassFixture<FabricaApi>
{
    protected HttpClient Cliente { get; }
    protected JsonSerializerOptions OpcionesJson { get; }

    protected PruebaIntegracionBase(FabricaApi fabrica)
    {
        Cliente = fabrica.CreateClient();
        OpcionesJson = new JsonSerializerOptions(JsonSerializerDefaults.Web);
        OpcionesJson.Converters.Add(new JsonStringEnumConverter());
    }

    protected async Task<ValidationProblemDetails?> ExtraerValidationProblemAsync(HttpResponseMessage respuesta)
    {
        return await respuesta.Content.ReadFromJsonAsync<ValidationProblemDetails>(OpcionesJson, TestContext.Current.CancellationToken);
    }

    protected async Task<ProblemDetails?> ExtraerProblemDetailsAsync(HttpResponseMessage respuesta)
    {
        return await respuesta.Content.ReadFromJsonAsync<ProblemDetails>(OpcionesJson, TestContext.Current.CancellationToken);
    }
}

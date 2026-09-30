using System.Text.Json.Serialization;

using RRHH.Api.Errores;
using RRHH.Application;
using RRHH.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AgregarInfraestructura(builder.Configuration.GetConnectionString("Rrhh"));
builder.Services.AgregarAplicacion();

// Errores como ProblemDetails, traducidos en un solo lugar (PLAN.md, «API»)
builder.Services.AddProblemDetails(o => o.CustomizeProblemDetails = c =>
{
    // Mismo título en español para los 400 del DTO (automáticos) y los del servicio
    if (c.ProblemDetails.Status == StatusCodes.Status400BadRequest) c.ProblemDetails.Title = "Datos inválidos";
});
builder.Services.AddExceptionHandler<ManejadorExcepciones>();

// Enums como texto en JSON ("VeintiochoDeFebrero"), igual que en la base
builder.Services.AddControllers()
    .AddJsonOptions(o => o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health");   // la API responde y llega a la base

app.Run();

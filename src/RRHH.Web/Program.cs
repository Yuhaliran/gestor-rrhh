using RRHH.Web.Api;
using RRHH.Web.Pages.Shared;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages()
    .AddMvcOptions(o =>
    {
        o.Filters.Add<ApiNoDisponibleFiltro>();              // la API caída: página de error, no la excepción
        MensajesDeEnlace.EnEspaniol(o.ModelBindingMessageProvider);
    });

// La web habla con la API sólo por HTTP (ARQ3); la dirección está en Api:UrlBase
var urlApi = builder.Configuration["Api:UrlBase"]
    ?? throw new InvalidOperationException("Falta la configuración Api:UrlBase (dirección de la API).");
builder.Services.AddHttpClient<ClienteRrhh>(cliente => cliente.BaseAddress = new Uri(urlApi));

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

// Un 404, 502 o 503 sin cuerpo (una página que no existe, la API caída) se muestra con la página
// de error, que explica cada caso
app.UseStatusCodePagesWithReExecute("/Error");

app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorPages().WithStaticAssets();
app.MapCascada();

app.Run();

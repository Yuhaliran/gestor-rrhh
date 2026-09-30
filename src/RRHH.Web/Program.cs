using RRHH.Web.Api;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();

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

app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorPages().WithStaticAssets();
app.MapCascada();

app.Run();

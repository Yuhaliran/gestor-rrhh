using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

using RRHH.Contratos.Colaboradores;
using RRHH.Contratos.Empresas;
using RRHH.Web.Api;

namespace RRHH.Web.Pages.Colaboradores;

// Alta de un colaborador con sus empresas, al menos una (RN3). Agregar o quitar una fila de empresa
// vuelve a mostrar el formulario sin llamar a la API: así no hace falta armar filas con JavaScript.
public class CrearModel(ClienteRrhh api) : PageModel
{
    [BindProperty]
    public CrearColaboradorDto Datos { get; set; } = new() { Empresas = [new AsociarEmpresaDto()] };

    public IReadOnlyList<EmpresaDto> Empresas { get; private set; } = [];

    public Task<IActionResult> OnGetAsync(CancellationToken ct) => MostrarAsync(ct);

    public async Task<IActionResult> OnPostAsync(CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            return await MostrarAsync(ct);
        }
        var resultado = await api.CrearColaboradorAsync(Datos, ct);
        if (!resultado.Exitoso)
        {
            // Los errores de una empresa llegan como «Empresas[1].FechaIngreso»: quedan en su fila
            ModelState.AgregarProblema(resultado.Problema!, nameof(Datos));
            return await MostrarAsync(ct);
        }
        TempData["Mensaje"] = "Colaborador creado.";
        return RedirectToPage("Detalle", new { id = resultado.Valor!.Id });
    }

    public Task<IActionResult> OnPostAgregarEmpresaAsync(CancellationToken ct)
    {
        Datos = Datos with { Empresas = [.. Datos.Empresas ?? [], new AsociarEmpresaDto()] };
        ModelState.Clear();   // el formulario muestra Datos, no lo que llegó en el pedido
        return MostrarAsync(ct);
    }

    public Task<IActionResult> OnPostQuitarEmpresaAsync(int indice, CancellationToken ct)
    {
        var empresas = (Datos.Empresas ?? []).ToList();
        if (empresas.Count > 1 && indice >= 0 && indice < empresas.Count)
        {
            empresas.RemoveAt(indice);
        }
        Datos = Datos with { Empresas = empresas };
        ModelState.Clear();
        return MostrarAsync(ct);
    }

    private async Task<IActionResult> MostrarAsync(CancellationToken ct)
    {
        if (Datos.Empresas is null or { Count: 0 })
        {
            Datos = Datos with { Empresas = [new AsociarEmpresaDto()] };
        }
        Empresas = await api.TodasLasEmpresasAsync(ct);
        return Page();
    }
}

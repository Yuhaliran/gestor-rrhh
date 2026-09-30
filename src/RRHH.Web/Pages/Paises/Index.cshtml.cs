using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

using RRHH.Contratos.Comun;
using RRHH.Contratos.Paises;
using RRHH.Web.Api;
using RRHH.Web.Pages.Shared;

namespace RRHH.Web.Pages.Paises;

public class IndexModel(ClienteRrhh api) : PageModel
{
    [BindProperty(SupportsGet = true, Name = "pagina")]
    public int NumeroPagina { get; set; } = 1;

    [BindProperty(SupportsGet = true)]
    public string? Buscar { get; set; }

    public Pagina<PaisDto> Paises { get; private set; } = null!;

    public async Task OnGetAsync(CancellationToken ct) =>
        Paises = await api.ListarPaisesAsync(Listados.Consulta(NumeroPagina, Buscar), ct);

    // RN1: la API responde 409 si el país tiene departamentos
    public async Task<IActionResult> OnPostEliminarAsync(int id, CancellationToken ct)
    {
        var resultado = await api.EliminarPaisAsync(id, ct);
        if (resultado.Exitoso)
        {
            TempData["Mensaje"] = "País eliminado.";
        }
        else
        {
            TempData["Error"] = resultado.Problema!.Detail;
        }
        return RedirectToPage(new { pagina = NumeroPagina, buscar = Buscar });
    }
}

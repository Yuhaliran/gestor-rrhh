using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

using RRHH.Contratos.Comun;
using RRHH.Contratos.Municipios;
using RRHH.Web.Api;
using RRHH.Web.Pages.Shared;

namespace RRHH.Web.Pages.Municipios;

public class IndexModel(ClienteRrhh api) : PageModel
{
    [BindProperty(SupportsGet = true, Name = "pagina")]
    public int NumeroPagina { get; set; } = 1;

    [BindProperty(SupportsGet = true)]
    public string? Buscar { get; set; }

    public Pagina<MunicipioDto> Municipios { get; private set; } = null!;

    public async Task OnGetAsync(CancellationToken ct) =>
        Municipios = await api.ListarMunicipiosAsync(Listados.Consulta(NumeroPagina, Buscar), ct);

    // RN1: la API responde 409 si el municipio tiene empresas
    public async Task<IActionResult> OnPostEliminarAsync(int id, CancellationToken ct)
    {
        var resultado = await api.EliminarMunicipioAsync(id, ct);
        if (resultado.Exitoso)
        {
            TempData["Mensaje"] = "Municipio eliminado.";
        }
        else
        {
            TempData["Error"] = resultado.Problema!.Detail;
        }
        return RedirectToPage(new { pagina = NumeroPagina, buscar = Buscar });
    }
}

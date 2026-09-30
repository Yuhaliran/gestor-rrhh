using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

using RRHH.Contratos.Comun;
using RRHH.Contratos.Empresas;
using RRHH.Web.Api;
using RRHH.Web.Pages.Shared;

namespace RRHH.Web.Pages.Empresas;

public class IndexModel(ClienteRrhh api) : PageModel
{
    [BindProperty(SupportsGet = true, Name = "pagina")]
    public int NumeroPagina { get; set; } = 1;

    [BindProperty(SupportsGet = true)]
    public string? Buscar { get; set; }

    public Pagina<EmpresaDto> Empresas { get; private set; } = null!;

    public async Task OnGetAsync(CancellationToken ct) =>
        Empresas = await api.ListarEmpresasAsync(Listados.Consulta(NumeroPagina, Buscar), ct);

    // RN2: la API responde 409 si la empresa tiene colaboradores
    public async Task<IActionResult> OnPostEliminarAsync(int id, CancellationToken ct)
    {
        var resultado = await api.EliminarEmpresaAsync(id, ct);
        if (resultado.Exitoso)
        {
            TempData["Mensaje"] = "Empresa eliminada.";
        }
        else
        {
            TempData["Error"] = resultado.Problema!.Detail;
        }
        return RedirectToPage(new { pagina = NumeroPagina, buscar = Buscar });
    }
}

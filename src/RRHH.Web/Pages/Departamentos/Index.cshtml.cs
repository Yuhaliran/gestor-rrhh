using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

using RRHH.Contratos.Comun;
using RRHH.Contratos.Departamentos;
using RRHH.Web.Api;
using RRHH.Web.Pages.Shared;

namespace RRHH.Web.Pages.Departamentos;

public class IndexModel(ClienteRrhh api) : PageModel
{
    [BindProperty(SupportsGet = true, Name = "pagina")]
    public int NumeroPagina { get; set; } = 1;

    [BindProperty(SupportsGet = true)]
    public string? Buscar { get; set; }

    public Pagina<DepartamentoDto> Departamentos { get; private set; } = null!;

    public async Task OnGetAsync(CancellationToken ct) =>
        Departamentos = await api.ListarDepartamentosAsync(Listados.Consulta(NumeroPagina, Buscar), ct);

    // RN1: la API responde 409 si el departamento tiene municipios
    public async Task<IActionResult> OnPostEliminarAsync(int id, CancellationToken ct)
    {
        var resultado = await api.EliminarDepartamentoAsync(id, ct);
        if (resultado.Exitoso)
        {
            TempData["Mensaje"] = "Departamento eliminado.";
        }
        else
        {
            TempData["Error"] = resultado.Problema!.Detail;
        }
        return RedirectToPage(new { pagina = NumeroPagina, buscar = Buscar });
    }
}

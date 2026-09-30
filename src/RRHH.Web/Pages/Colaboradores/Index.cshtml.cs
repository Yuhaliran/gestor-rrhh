using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

using RRHH.Contratos.Colaboradores;
using RRHH.Contratos.Comun;
using RRHH.Web.Api;
using RRHH.Web.Pages.Shared;

namespace RRHH.Web.Pages.Colaboradores;

public class IndexModel(ClienteRrhh api) : PageModel
{
    [BindProperty(SupportsGet = true, Name = "pagina")]
    public int NumeroPagina { get; set; } = 1;

    [BindProperty(SupportsGet = true)]
    public string? Buscar { get; set; }

    public Pagina<ColaboradorDto> Colaboradores { get; private set; } = null!;

    public async Task OnGetAsync(CancellationToken ct) =>
        Colaboradores = await api.ListarColaboradoresAsync(Listados.Consulta(NumeroPagina, Buscar), ct);

    // Borra también sus relaciones con empresas
    public async Task<IActionResult> OnPostEliminarAsync(int id, CancellationToken ct)
    {
        var resultado = await api.EliminarColaboradorAsync(id, ct);
        if (resultado.Exitoso)
        {
            TempData["Mensaje"] = "Colaborador eliminado.";
        }
        else
        {
            TempData["Error"] = resultado.Problema!.Texto();
        }
        return RedirectToPage(new { pagina = NumeroPagina, buscar = Buscar });
    }
}

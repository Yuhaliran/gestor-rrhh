using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

using RRHH.Contratos.Colaboradores;
using RRHH.Contratos.Comun;
using RRHH.Contratos.Empresas;
using RRHH.Web.Api;
using RRHH.Web.Pages.Shared;

namespace RRHH.Web.Pages.Empresas;

// Colaboradores de una empresa, paginados
public class ColaboradoresModel(ClienteRrhh api) : PageModel
{
    [BindProperty(SupportsGet = true, Name = "pagina")]
    public int NumeroPagina { get; set; } = 1;

    [BindProperty(SupportsGet = true)]
    public string? Buscar { get; set; }

    public EmpresaDto Empresa { get; private set; } = null!;
    public Pagina<ColaboradorDto> Colaboradores { get; private set; } = null!;

    public async Task<IActionResult> OnGetAsync(int id, CancellationToken ct)
    {
        var empresa = await api.ObtenerEmpresaAsync(id, ct);
        var colaboradores = await api.ColaboradoresDeEmpresaAsync(id, Listados.Consulta(NumeroPagina, Buscar), ct);
        if (empresa is null || colaboradores is null)
        {
            return NotFound();
        }
        Empresa = empresa;
        Colaboradores = colaboradores;
        return Page();
    }
}

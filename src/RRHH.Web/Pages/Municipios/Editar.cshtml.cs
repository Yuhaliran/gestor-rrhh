using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

using RRHH.Contratos.Departamentos;
using RRHH.Contratos.Municipios;
using RRHH.Contratos.Paises;
using RRHH.Web.Api;

namespace RRHH.Web.Pages.Municipios;

// Alta (sin id) y edición (con id) de un municipio. En el alta, el departamento se elige en cascada
// después del país (RN6); al editar, el departamento no cambia (RN8).
public class EditarModel(ClienteRrhh api) : PageModel
{
    [BindProperty]
    public GuardarMunicipioDto Datos { get; set; } = new();

    // Sólo para la cascada del alta: el municipio guarda el departamento, no el país
    [BindProperty]
    public int? PaisId { get; set; }

    public int? Id { get; private set; }

    public IReadOnlyList<PaisDto> Paises { get; private set; } = [];
    public IReadOnlyList<DepartamentoDto> Departamentos { get; private set; } = [];

    // Edición: la ubicación, que se muestra sin poder cambiarla
    public MunicipioDto? Actual { get; private set; }

    public async Task<IActionResult> OnGetAsync(int? id, CancellationToken ct)
    {
        if (id is not null)
        {
            var municipio = await api.ObtenerMunicipioAsync(id.Value, ct);
            if (municipio is null)
            {
                return NotFound();
            }
            Datos = new GuardarMunicipioDto { DepartamentoId = municipio.DepartamentoId, Nombre = municipio.Nombre };
        }
        return await MostrarAsync(id, ct);
    }

    public async Task<IActionResult> OnPostAsync(int? id, CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            return await MostrarAsync(id, ct);
        }
        var resultado = id is null
            ? await api.CrearMunicipioAsync(Datos, ct)
            : await api.ActualizarMunicipioAsync(id.Value, Datos, ct);
        if (!resultado.Exitoso)
        {
            ModelState.AgregarProblema(resultado.Problema!, nameof(Datos));
            return await MostrarAsync(id, ct);
        }
        TempData["Mensaje"] = id is null ? "Municipio creado." : "Municipio actualizado.";
        return RedirectToPage("Index");
    }

    private async Task<IActionResult> MostrarAsync(int? id, CancellationToken ct)
    {
        Id = id;
        if (id is not null)
        {
            Actual = await api.ObtenerMunicipioAsync(id.Value, ct);
            return Page();
        }
        Paises = await api.TodosLosPaisesAsync(ct);
        if (PaisId is not null)
        {
            Departamentos = await api.DepartamentosDePaisAsync(PaisId.Value, ct);
        }
        return Page();
    }
}

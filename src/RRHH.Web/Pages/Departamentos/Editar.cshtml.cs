using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

using RRHH.Contratos.Departamentos;
using RRHH.Contratos.Paises;
using RRHH.Web.Api;

namespace RRHH.Web.Pages.Departamentos;

// Alta (sin id) y edición (con id) de un departamento. Al editar, el país no cambia (RN8).
public class EditarModel(ClienteRrhh api) : PageModel
{
    [BindProperty]
    public GuardarDepartamentoDto Datos { get; set; } = new();

    public int? Id { get; private set; }

    // Alta: los países para elegir. Edición: el nombre del país, que se muestra sin poder cambiarlo.
    public IReadOnlyList<PaisDto> Paises { get; private set; } = [];
    public string? PaisNombre { get; private set; }

    public async Task<IActionResult> OnGetAsync(int? id, CancellationToken ct)
    {
        if (id is not null)
        {
            var departamento = await api.ObtenerDepartamentoAsync(id.Value, ct);
            if (departamento is null)
            {
                return NotFound();
            }
            Datos = new GuardarDepartamentoDto { PaisId = departamento.PaisId, Nombre = departamento.Nombre };
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
            ? await api.CrearDepartamentoAsync(Datos, ct)
            : await api.ActualizarDepartamentoAsync(id.Value, Datos, ct);
        if (!resultado.Exitoso)
        {
            ModelState.AgregarProblema(resultado.Problema!, nameof(Datos));
            return await MostrarAsync(id, ct);
        }
        TempData["Mensaje"] = id is null ? "Departamento creado." : "Departamento actualizado.";
        return RedirectToPage("Index");
    }

    private async Task<IActionResult> MostrarAsync(int? id, CancellationToken ct)
    {
        Id = id;
        if (id is null)
        {
            Paises = await api.TodosLosPaisesAsync(ct);
        }
        else
        {
            PaisNombre = (await api.ObtenerDepartamentoAsync(id.Value, ct))?.PaisNombre;
        }
        return Page();
    }
}

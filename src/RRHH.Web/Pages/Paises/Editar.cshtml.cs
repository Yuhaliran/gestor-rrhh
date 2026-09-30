using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

using RRHH.Contratos.Paises;
using RRHH.Web.Api;

namespace RRHH.Web.Pages.Paises;

// Alta (sin id) y edición (con id) de un país
public class EditarModel(ClienteRrhh api) : PageModel
{
    // El formulario propone 18, 100 y el 28 de febrero (ESPECIFICACION.md, «País»)
    [BindProperty]
    public GuardarPaisDto Datos { get; set; } = new()
    {
        EdadMinima = 18,
        EdadMaxima = 100,
        Regla29Febrero = Regla29Febrero.VeintiochoDeFebrero
    };

    public int? Id { get; private set; }

    public async Task<IActionResult> OnGetAsync(int? id, CancellationToken ct)
    {
        Id = id;
        if (id is null)
        {
            return Page();
        }
        var pais = await api.ObtenerPaisAsync(id.Value, ct);
        if (pais is null)
        {
            return NotFound();
        }
        Datos = new GuardarPaisDto
        {
            Nombre = pais.Nombre,
            CodigoIso2 = pais.CodigoIso2,
            EdadMinima = pais.EdadMinima,
            EdadMaxima = pais.EdadMaxima,
            Regla29Febrero = pais.Regla29Febrero
        };
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int? id, CancellationToken ct)
    {
        Id = id;
        if (!ModelState.IsValid)
        {
            return Page();
        }
        var resultado = id is null
            ? await api.CrearPaisAsync(Datos, ct)
            : await api.ActualizarPaisAsync(id.Value, Datos, ct);
        if (!resultado.Exitoso)
        {
            ModelState.AgregarProblema(resultado.Problema!, nameof(Datos));
            return Page();
        }
        TempData["Mensaje"] = id is null ? "País creado." : "País actualizado.";
        return RedirectToPage("Index");
    }
}

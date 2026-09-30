using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

using RRHH.Contratos.Colaboradores;
using RRHH.Web.Api;

namespace RRHH.Web.Pages.Colaboradores;

// Edición de los datos personales. Sus empresas se manejan desde el detalle.
public class EditarModel(ClienteRrhh api) : PageModel
{
    [BindProperty]
    public GuardarColaboradorDto Datos { get; set; } = new();

    public int Id { get; private set; }

    public async Task<IActionResult> OnGetAsync(int id, CancellationToken ct)
    {
        var colaborador = await api.ObtenerColaboradorAsync(id, ct);
        if (colaborador is null)
        {
            return NotFound();
        }
        Id = id;
        Datos = new GuardarColaboradorDto
        {
            NombreCompleto = colaborador.NombreCompleto,
            FechaNacimiento = colaborador.FechaNacimiento,
            Telefono = colaborador.Telefono,
            Correo = colaborador.Correo
        };
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int id, CancellationToken ct)
    {
        Id = id;
        if (!ModelState.IsValid)
        {
            return Page();
        }
        var resultado = await api.ActualizarColaboradorAsync(id, Datos, ct);
        if (!resultado.Exitoso)
        {
            ModelState.AgregarProblema(resultado.Problema!, nameof(Datos));
            return Page();
        }
        TempData["Mensaje"] = "Colaborador actualizado.";
        return RedirectToPage("Detalle", new { id });
    }
}

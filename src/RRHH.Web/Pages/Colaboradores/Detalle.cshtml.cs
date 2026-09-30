using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

using RRHH.Contratos.Colaboradores;
using RRHH.Contratos.Empresas;
using RRHH.Web.Api;

namespace RRHH.Web.Pages.Colaboradores;

// Detalle del colaborador, con su edad y sus empresas: asociar otra, editar la fecha de ingreso y
// el puesto de cada una, y quitarla (no la última, RN3).
public class DetalleModel(ClienteRrhh api) : PageModel
{
    public ColaboradorDto Colaborador { get; private set; } = null!;

    // Las empresas a las que todavía no está asociado
    public IReadOnlyList<EmpresaDto> EmpresasDisponibles { get; private set; } = [];

    [BindProperty]
    public AsociarEmpresaDto Nueva { get; set; } = new();

    public Task<IActionResult> OnGetAsync(int id, CancellationToken ct) => MostrarAsync(id, ct);

    public async Task<IActionResult> OnPostAsociarAsync(int id, CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            return await MostrarAsync(id, ct);
        }
        var resultado = await api.AsociarEmpresaAsync(id, Nueva, ct);
        if (!resultado.Exitoso)
        {
            ModelState.AgregarProblema(resultado.Problema!, nameof(Nueva));
            return await MostrarAsync(id, ct);
        }
        TempData["Mensaje"] = "Empresa asociada.";
        return RedirectToPage(new { id });
    }

    // Los campos de cada fila se envían con el atributo form: una tabla no puede contener un formulario
    public async Task<IActionResult> OnPostEditarEmpresaAsync(int id, int empresaId, DateOnly? fechaIngreso, string? puesto,
        CancellationToken ct)
    {
        var datos = new GuardarEmpresaColaboradorDto { FechaIngreso = fechaIngreso, Puesto = puesto };
        var resultado = await api.ActualizarEmpresaDeColaboradorAsync(id, empresaId, datos, ct);
        Informar(resultado, "Empresa actualizada.");
        return RedirectToPage(new { id });
    }

    public async Task<IActionResult> OnPostQuitarEmpresaAsync(int id, int empresaId, CancellationToken ct)
    {
        Informar(await api.QuitarEmpresaAsync(id, empresaId, ct), "Empresa quitada.");
        return RedirectToPage(new { id });
    }

    private void Informar(Resultado resultado, string exito)
    {
        if (resultado.Exitoso)
        {
            TempData["Mensaje"] = exito;
        }
        else
        {
            TempData["Error"] = resultado.Problema!.Texto();
        }
    }

    private async Task<IActionResult> MostrarAsync(int id, CancellationToken ct)
    {
        var colaborador = await api.ObtenerColaboradorAsync(id, ct);
        if (colaborador is null)
        {
            return NotFound();
        }
        Colaborador = colaborador;
        var asociadas = colaborador.Empresas.Select(e => e.EmpresaId).ToHashSet();
        EmpresasDisponibles = (await api.TodasLasEmpresasAsync(ct)).Where(e => !asociadas.Contains(e.Id)).ToList();
        return Page();
    }
}

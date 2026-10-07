import { ChangeDetectionStrategy, Component, input } from '@angular/core';

// RF4 a RF8, RF11 · Alta de un municipio con país y departamento en cascada; al editar, los dos fijos (RN8). Esqueleto hasta la tarea 39 (E-014).
@Component({
  selector: 'rrhh-municipio-formulario',
  templateUrl: './municipio-formulario.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class MunicipioFormulario {
  // :id de la ruta (withComponentInputBinding); vacío al crear
  readonly id = input<string>();
}

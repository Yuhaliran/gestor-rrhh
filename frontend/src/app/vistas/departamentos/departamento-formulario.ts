import { ChangeDetectionStrategy, Component, input } from '@angular/core';

// RF4 a RF8, RF10 · Alta y edición de un departamento; el país, fijo al editar (RN8). Esqueleto hasta la tarea 39 (E-014).
@Component({
  selector: 'rrhh-departamento-formulario',
  templateUrl: './departamento-formulario.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class DepartamentoFormulario {
  // :id de la ruta (withComponentInputBinding); vacío al crear
  readonly id = input<string>();
}

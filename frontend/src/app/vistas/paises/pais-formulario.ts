import { ChangeDetectionStrategy, Component, input } from '@angular/core';

// RF4 a RF9 · Alta y edición de un país. Esqueleto hasta la tarea 38 (E-014).
@Component({
  selector: 'rrhh-pais-formulario',
  templateUrl: './pais-formulario.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class PaisFormulario {
  // :id de la ruta (withComponentInputBinding); vacío al crear
  readonly id = input<string>();
}

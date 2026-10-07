import { ChangeDetectionStrategy, Component, input } from '@angular/core';

// RF4 a RF8, RF13 · Edición de los datos personales de un colaborador (sin sus empresas).
// Esqueleto hasta la tarea 41 (E-014).
@Component({
  selector: 'rrhh-colaborador-editar',
  templateUrl: './colaborador-editar.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ColaboradorEditar {
  // :id de la ruta (withComponentInputBinding)
  readonly id = input<string>();
}

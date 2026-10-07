import { ChangeDetectionStrategy, Component, input } from '@angular/core';

// RF4 a RF8, RF12 · Empresa con la geografía en cascada (RN6); al editar, el país queda fijo
// (RN8). Esqueleto hasta la tarea 40 (E-014).
@Component({
  selector: 'rrhh-empresa-formulario',
  templateUrl: './empresa-formulario.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class EmpresaFormulario {
  // :id de la ruta (withComponentInputBinding); vacío al crear
  readonly id = input<string>();
}

import { ChangeDetectionStrategy, Component, input } from '@angular/core';

// RF2, RF7, RF15 · Colaboradores de una empresa, con enlace al detalle de cada uno. Esqueleto
// hasta la tarea 40 (E-014).
@Component({
  selector: 'rrhh-empresa-colaboradores',
  templateUrl: './empresa-colaboradores.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class EmpresaColaboradores {
  // :id de la ruta (withComponentInputBinding)
  readonly id = input<string>();
}

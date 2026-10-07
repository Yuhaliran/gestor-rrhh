import { ChangeDetectionStrategy, Component, input } from '@angular/core';

// RF4, RF7, RF14 · Detalle del colaborador: sus datos, la edad que calcula la API (RN7) y sus
// empresas, que se asocian, editan y quitan desde aquí. Esqueleto hasta la tarea 42 (E-014).
@Component({
  selector: 'rrhh-colaborador-detalle',
  templateUrl: './colaborador-detalle.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ColaboradorDetalle {
  // :id de la ruta (withComponentInputBinding)
  readonly id = input<string>();
}

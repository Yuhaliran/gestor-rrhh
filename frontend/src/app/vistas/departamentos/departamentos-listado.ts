import { ChangeDetectionStrategy, Component } from '@angular/core';

// RF2, RF3, RF10 · Listado de departamentos. Esqueleto hasta la tarea 39 (E-014).
@Component({
  selector: 'rrhh-departamentos-listado',
  templateUrl: './departamentos-listado.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class DepartamentosListado {}

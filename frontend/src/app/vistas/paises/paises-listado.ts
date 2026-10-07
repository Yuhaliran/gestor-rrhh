import { ChangeDetectionStrategy, Component } from '@angular/core';

// RF2, RF3, RF9 · Listado de países. Esqueleto hasta la tarea 38 (E-014).
@Component({
  selector: 'rrhh-paises-listado',
  templateUrl: './paises-listado.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class PaisesListado {}

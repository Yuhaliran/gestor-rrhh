import { ChangeDetectionStrategy, Component } from '@angular/core';

// RF2, RF3, RF11 · Listado de municipios. Esqueleto hasta la tarea 39 (E-014).
@Component({
  selector: 'rrhh-municipios-listado',
  templateUrl: './municipios-listado.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class MunicipiosListado {}

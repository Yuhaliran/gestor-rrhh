import { ChangeDetectionStrategy, Component } from '@angular/core';

// RF2, RF3, RF13 · Listado de colaboradores, con su edad y sus empresas. Esqueleto hasta la
// tarea 41 (E-014).
@Component({
  selector: 'rrhh-colaboradores-listado',
  templateUrl: './colaboradores-listado.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ColaboradoresListado {}

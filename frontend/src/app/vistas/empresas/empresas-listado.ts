import { ChangeDetectionStrategy, Component } from '@angular/core';

// RF2, RF3, RF12 · Listado de empresas, con acceso a sus colaboradores (RF15). Esqueleto hasta la
// tarea 40 (E-014).
@Component({
  selector: 'rrhh-empresas-listado',
  templateUrl: './empresas-listado.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class EmpresasListado {}

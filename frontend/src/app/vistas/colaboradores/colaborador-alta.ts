import { ChangeDetectionStrategy, Component } from '@angular/core';

// RF4 a RF8, RF13 · Alta de un colaborador con sus datos personales y una o varias empresas
// (RN3). Esqueleto hasta la tarea 41 (E-014).
@Component({
  selector: 'rrhh-colaborador-alta',
  templateUrl: './colaborador-alta.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ColaboradorAlta {}

import { ChangeDetectionStrategy, Component } from '@angular/core';
import { RouterLink } from '@angular/router';

import { MANTENIMIENTOS } from '../servicios/mantenimientos';

// RF1 · Página de inicio, con acceso a cada mantenimiento.
@Component({
  selector: 'rrhh-inicio',
  imports: [RouterLink],
  templateUrl: './inicio.html',
  styleUrl: './inicio.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class Inicio {
  protected readonly mantenimientos = MANTENIMIENTOS;
}

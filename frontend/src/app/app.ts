import { ChangeDetectionStrategy, Component } from '@angular/core';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';

import { MANTENIMIENTOS } from './servicios/mantenimientos';

// Layout: menú (RF1) y la pantalla de la ruta. Los avisos y la confirmación los abre Angular
// Material en su propia capa (AvisosMaterial, ConfirmacionMaterial): no van en la plantilla.
@Component({
  imports: [RouterLink, RouterLinkActive, RouterOutlet],
  selector: 'rrhh-root',
  styleUrl: './app.css',
  templateUrl: './app.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class App {
  protected readonly mantenimientos = MANTENIMIENTOS;
}

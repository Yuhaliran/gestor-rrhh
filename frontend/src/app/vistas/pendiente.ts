import { ChangeDetectionStrategy, Component } from '@angular/core';
import { RouterLink } from '@angular/router';

// Lugar de las pantallas que llegan en las tareas 38 a 42; cada una reemplaza su ruta.
@Component({
  selector: 'rrhh-pendiente',
  imports: [RouterLink],
  templateUrl: './pendiente.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class Pendiente {}

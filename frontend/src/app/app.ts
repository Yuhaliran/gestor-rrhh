import { Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';

// Layout de la aplicación. El menú (RF1), los avisos y la confirmación llegan en la tarea 37.
@Component({
  imports: [RouterOutlet],
  selector: 'rrhh-root',
  styleUrl: './app.css',
  templateUrl: './app.html',
})
export class App {}

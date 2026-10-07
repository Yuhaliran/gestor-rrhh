import { Injectable, inject } from '@angular/core';
import { MatSnackBar } from '@angular/material/snack-bar';

import type { Avisos } from '../contratos/pantallas';

// Avisos (RF3, RF4, RF6 a RF8) con el snackbar de Angular Material. Un error queda más tiempo y
// se anuncia de inmediato a los lectores de pantalla.
@Injectable()
export class AvisosMaterial implements Avisos {
  private readonly snackbar = inject(MatSnackBar);

  exito(mensaje: string): void {
    this.snackbar.open(mensaje, 'Cerrar', { duration: 4000 });
  }

  error(mensaje: string): void {
    this.snackbar.open(mensaje, 'Cerrar', {
      duration: 8000,
      politeness: 'assertive',
      panelClass: 'aviso-error',
    });
  }
}

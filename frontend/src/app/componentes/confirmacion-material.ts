import { Injectable, inject } from '@angular/core';
import { MatDialog } from '@angular/material/dialog';
import { map, type Observable } from 'rxjs';

import type { Confirmacion } from '../contratos/pantallas';
import { DialogoConfirmacion } from './dialogo-confirmacion';

// Confirmación (RF3) con el diálogo de Angular Material. Cerrarlo sin elegir cuenta como cancelar.
@Injectable()
export class ConfirmacionMaterial implements Confirmacion {
  private readonly dialogo = inject(MatDialog);

  confirmar(mensaje: string): Observable<boolean> {
    return this.dialogo
      .open<DialogoConfirmacion, { mensaje: string }, boolean>(DialogoConfirmacion, {
        data: { mensaje },
      })
      .afterClosed()
      .pipe(map((respuesta) => respuesta === true));
  }
}

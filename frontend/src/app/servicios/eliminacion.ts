import { DestroyRef, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { filter, switchMap, take, type Observable } from 'rxjs';

import type { ErrorApi } from '../contratos/errores';
import type { CrearEliminacion, Eliminacion } from '../contratos/pantallas';
import { AVISOS } from './avisos';
import { CONFIRMACION } from './confirmacion';
import { mensajeDeError } from './formatos';

// RF3, RF4 · Eliminación con confirmación.
export const crearEliminacion: CrearEliminacion = <T>(
  eliminar: (elemento: T) => Observable<void>,
  alTerminar: () => void,
): Eliminacion<T> => {
  const confirmacion = inject(CONFIRMACION);
  const avisos = inject(AVISOS);
  const destruccion = inject(DestroyRef);

  const eliminando = signal(false);

  return {
    eliminando: eliminando.asReadonly(),
    eliminar: (elemento) => {
      let confirmado = false;
      confirmacion
        .confirmar('¿Eliminar este registro?')
        .pipe(
          take(1),
          filter((respuesta) => respuesta),
          switchMap(() => {
            confirmado = true;
            eliminando.set(true);
            return eliminar(elemento);
          }),
          takeUntilDestroyed(destruccion),
        )
        .subscribe({
          // 204: sin cuerpo; lo que cuenta es que termine sin error. Cancelar también termina,
          // pero sin haber eliminado nada.
          complete: () => {
            if (confirmado) {
              eliminando.set(false);
              avisos.exito('Se eliminó correctamente.');
              alTerminar();
            }
          },
          error: (error: ErrorApi) => {
            eliminando.set(false);
            avisos.error(mensajeDeError(error));
          },
        });
    },
  };
};

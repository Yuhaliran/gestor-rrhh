import { DestroyRef, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { EMPTY, Subject, catchError, debounceTime, switchMap, type Observable } from 'rxjs';

import type { Consulta, Pagina } from '../contratos/dtos';
import type { ErrorApi } from '../contratos/errores';
import type { CrearListado, Listado } from '../contratos/pantallas';

const TAMANIO_POR_DEFECTO = 20;
const ESPERA_BUSQUEDA_MS = 300;

// RF2 · Listado paginado por la API.
export const crearListado: CrearListado = <T>(
  cargar: (consulta: Consulta) => Observable<Pagina<T>>,
  opciones?: { tamanio?: number },
): Listado<T> => {
  const destruccion = inject(DestroyRef);

  const elementos = signal<T[]>([]);
  const total = signal(0);
  const pagina = signal(1);
  const tamanio = signal(opciones?.tamanio ?? TAMANIO_POR_DEFECTO);
  const buscar = signal('');
  const cargando = signal(false);
  const error = signal<ErrorApi | null>(null);

  const consultas = new Subject<void>();
  const busquedas = new Subject<string>();

  // switchMap: una respuesta vieja que llega después de pedir otra página se descarta
  consultas
    .pipe(
      switchMap(() => {
        cargando.set(true);
        const buscado = buscar();
        return cargar({ pagina: pagina(), tamanio: tamanio(), buscar: buscado || undefined }).pipe(
          // Un error no termina el listado: se guarda y se conservan los elementos anteriores
          catchError((motivo: ErrorApi) => {
            error.set(motivo);
            cargando.set(false);
            return EMPTY;
          }),
        );
      }),
      takeUntilDestroyed(destruccion),
    )
    .subscribe((respuesta) => {
      elementos.set(respuesta.elementos);
      total.set(respuesta.total);
      error.set(null);
      cargando.set(false);
    });

  busquedas
    .pipe(debounceTime(ESPERA_BUSQUEDA_MS), takeUntilDestroyed(destruccion))
    .subscribe((texto) => {
      buscar.set(texto);
      pagina.set(1);
      consultas.next();
    });

  consultas.next();

  return {
    elementos: elementos.asReadonly(),
    total: total.asReadonly(),
    pagina: pagina.asReadonly(),
    tamanio: tamanio.asReadonly(),
    buscar: buscar.asReadonly(),
    cargando: cargando.asReadonly(),
    error: error.asReadonly(),
    cambiarPagina: (numero) => {
      pagina.set(numero);
      consultas.next();
    },
    cambiarTamanio: (cantidad) => {
      tamanio.set(cantidad);
      pagina.set(1);
      consultas.next();
    },
    cambiarBusqueda: (texto) => busquedas.next(texto),
    recargar: () => consultas.next(),
  };
};

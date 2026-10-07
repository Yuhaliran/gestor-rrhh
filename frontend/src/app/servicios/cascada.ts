import { DestroyRef, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { Subject, catchError, of, switchMap, type Observable } from 'rxjs';

import type { DepartamentoDto, MunicipioDto, PaisDto } from '../contratos/dtos';
import type { ErrorApi } from '../contratos/errores';
import type { CrearCascada, SeleccionGeografica } from '../contratos/pantallas';
import { CLIENTE_RRHH } from './cliente';

// El máximo de la API: las listas para elegir no se paginan (docs/frontend/PLAN.md)
const TAMANIO_LISTAS = 100;

const SIN_SELECCION: SeleccionGeografica = {
  paisId: null,
  departamentoId: null,
  municipioId: null,
};

// RN6, RF11, RF12 · Geografía en cascada.
export const crearCascada: CrearCascada = (inicial) => {
  const cliente = inject(CLIENTE_RRHH);
  const destruccion = inject(DestroyRef);

  const paises = signal<PaisDto[]>([]);
  const departamentos = signal<DepartamentoDto[]>([]);
  const municipios = signal<MunicipioDto[]>([]);
  const seleccion = signal<SeleccionGeografica>(inicial ?? SIN_SELECCION);
  const error = signal<ErrorApi | null>(null);

  // Un error deja la lista vacía y queda en error
  const sinError = <T>(pedido: Observable<T[]>) =>
    pedido.pipe(
      catchError((motivo: ErrorApi) => {
        error.set(motivo);
        return of<T[]>([]);
      }),
    );

  // switchMap: si se cambia de país (o de departamento) antes de la respuesta, gana el último
  const paisesElegidos = new Subject<number | null>();
  paisesElegidos
    .pipe(
      switchMap((paisId) =>
        paisId === null ? of([]) : sinError(cliente.paises.departamentos(paisId)),
      ),
      takeUntilDestroyed(destruccion),
    )
    .subscribe((lista) => departamentos.set(lista));

  const departamentosElegidos = new Subject<number | null>();
  departamentosElegidos
    .pipe(
      switchMap((departamentoId) =>
        departamentoId === null
          ? of([])
          : sinError(cliente.departamentos.municipios(departamentoId)),
      ),
      takeUntilDestroyed(destruccion),
    )
    .subscribe((lista) => municipios.set(lista));

  cliente.paises
    .listar({ pagina: 1, tamanio: TAMANIO_LISTAS })
    .pipe(
      catchError((motivo: ErrorApi) => {
        error.set(motivo);
        return of(null);
      }),
      takeUntilDestroyed(destruccion),
    )
    .subscribe((pagina) => paises.set(pagina?.elementos ?? []));

  // Al editar, las tres listas arrancan con la selección de la entidad
  if (inicial?.paisId != null) {
    paisesElegidos.next(inicial.paisId);
  }
  if (inicial?.departamentoId != null) {
    departamentosElegidos.next(inicial.departamentoId);
  }

  return {
    paises: paises.asReadonly(),
    departamentos: departamentos.asReadonly(),
    municipios: municipios.asReadonly(),
    seleccion: seleccion.asReadonly(),
    error: error.asReadonly(),
    elegirPais: (paisId) => {
      seleccion.set({ paisId, departamentoId: null, municipioId: null });
      paisesElegidos.next(paisId);
      departamentosElegidos.next(null);
    },
    elegirDepartamento: (departamentoId) => {
      seleccion.update((actual) => ({ ...actual, departamentoId, municipioId: null }));
      departamentosElegidos.next(departamentoId);
    },
    elegirMunicipio: (municipioId) => {
      seleccion.update((actual) => ({ ...actual, municipioId }));
    },
  };
};

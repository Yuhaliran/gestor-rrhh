import { ChangeDetectionStrategy, Component, computed, input } from '@angular/core';
import { MatFormField, MatLabel } from '@angular/material/form-field';
import { MatInput } from '@angular/material/input';
import { MatPaginator, MatPaginatorIntl, type PageEvent } from '@angular/material/paginator';
import { MatProgressBar } from '@angular/material/progress-bar';

import type { Listado } from '../contratos/pantallas';
import { mensajeDeError } from '../servicios/formatos';
import { PaginadorEnEspanol } from './paginador-en-espanol';

// RF2, RF8 · Lo que es igual en todos los listados, alrededor de la tabla de la vista: el campo
// «Buscar», el total, la carga, el error y el paginador (docs/frontend/CODIFICACION.md).
@Component({
  selector: 'rrhh-listado-paginado',
  imports: [MatFormField, MatLabel, MatInput, MatPaginator, MatProgressBar],
  templateUrl: './listado-paginado.html',
  styleUrl: './listado-paginado.css',
  providers: [{ provide: MatPaginatorIntl, useClass: PaginadorEnEspanol }],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ListadoPaginado {
  readonly listado = input.required<Listado<unknown>>();

  protected readonly tamanios = [10, 20, 50];

  // «N registros» (Contrato de interfaz)
  protected readonly textoTotal = computed(() => {
    const total = this.listado().total();
    return total === 1 ? '1 registro' : `${total} registros`;
  });

  // RF8: el error del listado, sin detalles técnicos
  protected readonly mensajeError = computed(() => {
    const error = this.listado().error();
    return error === null ? null : mensajeDeError(error);
  });

  protected buscar(evento: Event): void {
    this.listado().cambiarBusqueda((evento.target as HTMLInputElement).value);
  }

  // El paginador avisa en un mismo evento el cambio de página o de tamaño
  protected cambiarPagina(evento: PageEvent): void {
    const listado = this.listado();
    if (evento.pageSize !== listado.tamanio()) {
      listado.cambiarTamanio(evento.pageSize);
    } else {
      listado.cambiarPagina(evento.pageIndex + 1);
    }
  }
}

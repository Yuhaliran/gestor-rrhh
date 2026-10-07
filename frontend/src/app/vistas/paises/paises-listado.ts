import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { MatButton } from '@angular/material/button';
import { MatTableModule } from '@angular/material/table';
import { RouterLink } from '@angular/router';

import { ListadoPaginado } from '../../componentes/listado-paginado';
import type { PaisDto } from '../../contratos/dtos';
import { CLIENTE_RRHH } from '../../servicios/cliente';
import { crearEliminacion } from '../../servicios/eliminacion';
import { crearListado } from '../../servicios/listado';

// RF2, RF3, RF9 · Listado de países (patrón de referencia, docs/frontend/CODIFICACION.md).
@Component({
  selector: 'rrhh-paises-listado',
  imports: [RouterLink, MatButton, MatTableModule, ListadoPaginado],
  templateUrl: './paises-listado.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class PaisesListado {
  private readonly cliente = inject(CLIENTE_RRHH);

  protected readonly listado = crearListado((consulta) => this.cliente.paises.listar(consulta));
  protected readonly eliminacion = crearEliminacion(
    (pais: PaisDto) => this.cliente.paises.eliminar(pais.id),
    () => this.listado.recargar(),
  );
  protected readonly columnas = ['nombre', 'codigoIso2', 'edadMinima', 'edadMaxima', 'acciones'];
}

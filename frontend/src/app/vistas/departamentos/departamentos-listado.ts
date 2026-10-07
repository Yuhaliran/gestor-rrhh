import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { MatButton } from '@angular/material/button';
import { MatTableModule } from '@angular/material/table';
import { RouterLink } from '@angular/router';

import { ListadoPaginado } from '../../componentes/listado-paginado';
import type { DepartamentoDto } from '../../contratos/dtos';
import { CLIENTE_RRHH } from '../../servicios/cliente';
import { crearEliminacion } from '../../servicios/eliminacion';
import { crearListado } from '../../servicios/listado';

// RF2, RF3, RF10 · Listado de departamentos, con su país.
@Component({
  selector: 'rrhh-departamentos-listado',
  imports: [RouterLink, MatButton, MatTableModule, ListadoPaginado],
  templateUrl: './departamentos-listado.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class DepartamentosListado {
  private readonly cliente = inject(CLIENTE_RRHH);

  protected readonly listado = crearListado((consulta) =>
    this.cliente.departamentos.listar(consulta),
  );
  protected readonly eliminacion = crearEliminacion(
    (departamento: DepartamentoDto) => this.cliente.departamentos.eliminar(departamento.id),
    () => this.listado.recargar(),
  );
  protected readonly columnas = ['nombre', 'pais', 'acciones'];
}

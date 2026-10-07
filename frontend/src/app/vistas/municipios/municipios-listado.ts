import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { MatButton } from '@angular/material/button';
import { MatTableModule } from '@angular/material/table';
import { RouterLink } from '@angular/router';

import { ListadoPaginado } from '../../componentes/listado-paginado';
import type { MunicipioDto } from '../../contratos/dtos';
import { CLIENTE_RRHH } from '../../servicios/cliente';
import { crearEliminacion } from '../../servicios/eliminacion';
import { crearListado } from '../../servicios/listado';

// RF2, RF3, RF11 · Listado de municipios, con su departamento y su país.
@Component({
  selector: 'rrhh-municipios-listado',
  imports: [RouterLink, MatButton, MatTableModule, ListadoPaginado],
  templateUrl: './municipios-listado.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class MunicipiosListado {
  private readonly cliente = inject(CLIENTE_RRHH);

  protected readonly listado = crearListado((consulta) => this.cliente.municipios.listar(consulta));
  protected readonly eliminacion = crearEliminacion(
    (municipio: MunicipioDto) => this.cliente.municipios.eliminar(municipio.id),
    () => this.listado.recargar(),
  );
  protected readonly columnas = ['nombre', 'departamento', 'pais', 'acciones'];
}

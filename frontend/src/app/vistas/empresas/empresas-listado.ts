import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { MatButton } from '@angular/material/button';
import { MatTableModule } from '@angular/material/table';
import { RouterLink } from '@angular/router';

import { ListadoPaginado } from '../../componentes/listado-paginado';
import type { EmpresaDto } from '../../contratos/dtos';
import { CLIENTE_RRHH } from '../../servicios/cliente';
import { crearEliminacion } from '../../servicios/eliminacion';
import { crearListado } from '../../servicios/listado';

// RF2, RF3, RF12 · Listado de empresas, con su ubicación y acceso a sus colaboradores (RF15).
@Component({
  selector: 'rrhh-empresas-listado',
  imports: [RouterLink, MatButton, MatTableModule, ListadoPaginado],
  templateUrl: './empresas-listado.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class EmpresasListado {
  private readonly cliente = inject(CLIENTE_RRHH);

  protected readonly listado = crearListado((consulta) => this.cliente.empresas.listar(consulta));
  protected readonly eliminacion = crearEliminacion(
    (empresa: EmpresaDto) => this.cliente.empresas.eliminar(empresa.id),
    () => this.listado.recargar(),
  );
  protected readonly columnas = [
    'nombreComercial',
    'nit',
    'municipio',
    'departamento',
    'pais',
    'acciones',
  ];
}

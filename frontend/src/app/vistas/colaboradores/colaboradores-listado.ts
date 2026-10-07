import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { MatButton } from '@angular/material/button';
import { MatTableModule } from '@angular/material/table';
import { RouterLink } from '@angular/router';

import { ListadoPaginado } from '../../componentes/listado-paginado';
import type { ColaboradorDto } from '../../contratos/dtos';
import { CLIENTE_RRHH } from '../../servicios/cliente';
import { crearEliminacion } from '../../servicios/eliminacion';
import { textoEdad } from '../../servicios/formatos';
import { crearListado } from '../../servicios/listado';

// RF2, RF3, RF13 · Listado de colaboradores, con su edad (RN7) y sus empresas.
@Component({
  selector: 'rrhh-colaboradores-listado',
  imports: [RouterLink, MatButton, MatTableModule, ListadoPaginado],
  templateUrl: './colaboradores-listado.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ColaboradoresListado {
  private readonly cliente = inject(CLIENTE_RRHH);

  protected readonly listado = crearListado((consulta) =>
    this.cliente.colaboradores.listar(consulta),
  );
  protected readonly eliminacion = crearEliminacion(
    (colaborador: ColaboradorDto) => this.cliente.colaboradores.eliminar(colaborador.id),
    () => this.listado.recargar(),
  );
  protected readonly textoEdad = textoEdad;
  protected readonly columnas = ['nombreCompleto', 'correo', 'edad', 'empresas', 'acciones'];

  // Los nombres comerciales de sus empresas, separados por «, »
  protected empresas(colaborador: ColaboradorDto): string {
    return colaborador.empresas.map((empresa) => empresa.nombreComercial).join(', ');
  }
}

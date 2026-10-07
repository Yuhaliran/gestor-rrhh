import {
  ChangeDetectionStrategy,
  Component,
  DestroyRef,
  Injector,
  type OnInit,
  inject,
  input,
  runInInjectionContext,
  signal,
} from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { MatButton } from '@angular/material/button';
import { MatTableModule } from '@angular/material/table';
import { RouterLink } from '@angular/router';

import { ListadoPaginado } from '../../componentes/listado-paginado';
import type { ColaboradorDto, EmpresaColaboradorDto, EmpresaDto } from '../../contratos/dtos';
import type { ErrorApi } from '../../contratos/errores';
import type { Listado } from '../../contratos/pantallas';
import { CLIENTE_RRHH } from '../../servicios/cliente';
import { fechaParaMostrar, mensajeDeError, textoEdad } from '../../servicios/formatos';
import { crearListado } from '../../servicios/listado';

// RF2, RF7, RF15 · Colaboradores de una empresa, con su fecha de ingreso y su puesto en ella, y
// enlace al detalle de cada uno.
@Component({
  selector: 'rrhh-empresa-colaboradores',
  imports: [RouterLink, MatButton, MatTableModule, ListadoPaginado],
  templateUrl: './empresa-colaboradores.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class EmpresaColaboradores implements OnInit {
  private readonly cliente = inject(CLIENTE_RRHH);
  private readonly destruccion = inject(DestroyRef);
  private readonly inyector = inject(Injector);

  // :id de la ruta (withComponentInputBinding)
  readonly id = input<string>();

  protected readonly empresa = signal<EmpresaDto | null>(null);
  protected readonly noExiste = signal(false);
  protected readonly errorCarga = signal<string | null>(null);
  // Se crea al iniciar: necesita el :id, que todavía no llegó cuando se construye el componente
  protected readonly listado = signal<Listado<ColaboradorDto> | null>(null);
  protected readonly textoEdad = textoEdad;
  protected readonly columnas = ['nombreCompleto', 'edad', 'fechaIngreso', 'puesto', 'acciones'];

  ngOnInit(): void {
    const id = Number(this.id());
    this.cliente.empresas
      .obtener(id)
      .pipe(takeUntilDestroyed(this.destruccion))
      .subscribe({
        next: (empresa) => this.empresa.set(empresa),
        error: (error: ErrorApi) => {
          // RF7: la empresa no existe; cualquier otro error, con su texto (RF8)
          this.noExiste.set(error.estado === 404);
          this.errorCarga.set(error.estado === 404 ? null : mensajeDeError(error));
        },
      });
    this.listado.set(
      runInInjectionContext(this.inyector, () =>
        crearListado((consulta) => this.cliente.empresas.colaboradores(id, consulta)),
      ),
    );
  }

  // La fecha de ingreso, en esta empresa: el colaborador puede tener otras
  protected fechaIngreso(colaborador: ColaboradorDto): string {
    const relacion = this.relacion(colaborador);
    return relacion ? fechaParaMostrar(relacion.fechaIngreso) : '';
  }

  // El puesto en esta empresa (opcional)
  protected puesto(colaborador: ColaboradorDto): string {
    return this.relacion(colaborador)?.puesto ?? '';
  }

  private relacion(colaborador: ColaboradorDto): EmpresaColaboradorDto | undefined {
    const empresaId = Number(this.id());
    return colaborador.empresas.find((empresa) => empresa.empresaId === empresaId);
  }
}

import {
  ChangeDetectionStrategy,
  Component,
  DestroyRef,
  type OnInit,
  computed,
  inject,
  input,
  signal,
} from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { MatButton } from '@angular/material/button';
import { MatDialog } from '@angular/material/dialog';
import { MatTableModule } from '@angular/material/table';
import { RouterLink } from '@angular/router';

import type { ColaboradorDto, EmpresaColaboradorDto } from '../../contratos/dtos';
import type { ErrorApi } from '../../contratos/errores';
import { CLIENTE_RRHH } from '../../servicios/cliente';
import { crearEliminacion } from '../../servicios/eliminacion';
import { fechaParaMostrar, mensajeDeError, textoEdad } from '../../servicios/formatos';
import { type DatosDialogoEmpresa, EmpresaColaboradorDialogo } from './empresa-colaborador-dialogo';

// RF4, RF7, RF14 · Detalle del colaborador: sus datos, la edad que calcula la API (RN7) y sus
// empresas, que se asocian y editan en un diálogo y se quitan con confirmación (RN3).
@Component({
  selector: 'rrhh-colaborador-detalle',
  imports: [RouterLink, MatButton, MatTableModule],
  templateUrl: './colaborador-detalle.html',
  styleUrl: './colaborador-detalle.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ColaboradorDetalle implements OnInit {
  private readonly cliente = inject(CLIENTE_RRHH);
  private readonly destruccion = inject(DestroyRef);
  private readonly dialogos = inject(MatDialog);

  // :id de la ruta (withComponentInputBinding)
  readonly id = input<string>();

  protected readonly colaborador = signal<ColaboradorDto | null>(null);
  protected readonly noExiste = signal(false);
  protected readonly errorCarga = signal<string | null>(null);
  // RN3: la única empresa no se quita
  protected readonly unica = computed(() => (this.colaborador()?.empresas.length ?? 0) <= 1);
  protected readonly textoEdad = textoEdad;
  protected readonly fechaParaMostrar = fechaParaMostrar;
  protected readonly columnas = ['empresa', 'pais', 'fechaIngreso', 'puesto', 'acciones'];

  // RF3: la misma confirmación que eliminar; después se vuelve a pedir el colaborador
  protected readonly quitar = crearEliminacion(
    (relacion: EmpresaColaboradorDto) =>
      this.cliente.colaboradores.quitarEmpresa(Number(this.id()), relacion.empresaId),
    () => this.cargar(),
  );

  ngOnInit(): void {
    this.cargar();
  }

  protected asociarEmpresa(): void {
    this.abrirDialogo();
  }

  protected editarEmpresa(relacion: EmpresaColaboradorDto): void {
    this.abrirDialogo(relacion);
  }

  private cargar(): void {
    this.cliente.colaboradores
      .obtener(Number(this.id()))
      .pipe(takeUntilDestroyed(this.destruccion))
      .subscribe({
        next: (colaborador) => this.colaborador.set(colaborador),
        error: (error: ErrorApi) => {
          // RF7: el registro no existe; cualquier otro error, con su texto (RF8)
          this.noExiste.set(error.estado === 404);
          this.errorCarga.set(error.estado === 404 ? null : mensajeDeError(error));
        },
      });
  }

  // Al guardar, el diálogo se cierra con el colaborador que devolvió la API (RF4)
  private abrirDialogo(relacion?: EmpresaColaboradorDto): void {
    const colaborador = this.colaborador();
    if (colaborador === null) {
      return;
    }
    this.dialogos
      .open<EmpresaColaboradorDialogo, DatosDialogoEmpresa, ColaboradorDto>(
        EmpresaColaboradorDialogo,
        {
          data: {
            colaboradorId: colaborador.id,
            relacion,
            asociadas: colaborador.empresas.map((empresa) => empresa.empresaId),
          },
          width: '32rem',
        },
      )
      .afterClosed()
      .pipe(takeUntilDestroyed(this.destruccion))
      .subscribe((actualizado) => {
        if (actualizado) {
          this.colaborador.set(actualizado);
        }
      });
  }
}

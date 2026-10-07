import {
  ChangeDetectionStrategy,
  Component,
  DestroyRef,
  type OnInit,
  inject,
  signal,
} from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { NonNullableFormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButton } from '@angular/material/button';
import {
  MAT_DIALOG_DATA,
  MatDialogActions,
  MatDialogClose,
  MatDialogContent,
  MatDialogRef,
  MatDialogTitle,
} from '@angular/material/dialog';
import { MatError, MatFormField, MatLabel } from '@angular/material/form-field';
import { MatInput } from '@angular/material/input';
import { MatOption, MatSelect } from '@angular/material/select';

import type {
  AsociarEmpresaDto,
  ColaboradorDto,
  EmpresaColaboradorDto,
} from '../../contratos/dtos';
import type { ErrorApi } from '../../contratos/errores';
import { CLIENTE_RRHH } from '../../servicios/cliente';
import { fechaAIso, mensajeDeError, mensajeDeValidacion } from '../../servicios/formatos';
import { crearFormulario } from '../../servicios/formulario';

// Las listas para elegir no se paginan: el máximo de la API (docs/frontend/PLAN.md)
const TAMANIO_LISTAS = 100;

export interface DatosDialogoEmpresa {
  colaboradorId: number;
  // Al editar, la relación con la empresa; al asociar, no hay
  relacion?: EmpresaColaboradorDto;
  // Al asociar, las empresas que ya tiene: no se ofrecen
  asociadas: number[];
}

interface Opcion {
  id: number;
  nombre: string;
}

// RF14 · Diálogo para asociar una empresa al colaborador o editar la fecha de ingreso y el puesto
// de una relación. Se cierra con el colaborador que devuelve la API, o sin nada si se cancela.
@Component({
  selector: 'rrhh-empresa-colaborador-dialogo',
  imports: [
    ReactiveFormsModule,
    MatButton,
    MatDialogActions,
    MatDialogClose,
    MatDialogContent,
    MatDialogTitle,
    MatError,
    MatFormField,
    MatInput,
    MatLabel,
    MatOption,
    MatSelect,
  ],
  templateUrl: './empresa-colaborador-dialogo.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class EmpresaColaboradorDialogo implements OnInit {
  private readonly cliente = inject(CLIENTE_RRHH);
  private readonly destruccion = inject(DestroyRef);
  private readonly dialogo =
    inject<MatDialogRef<EmpresaColaboradorDialogo, ColaboradorDto>>(MatDialogRef);
  private readonly datos = inject<DatosDialogoEmpresa>(MAT_DIALOG_DATA);

  protected readonly titulo = this.datos.relacion ? 'Editar empresa' : 'Asociar empresa';
  protected readonly empresas = signal<Opcion[]>([]);
  protected readonly errorCarga = signal<string | null>(null);
  protected readonly mensajeDe = mensajeDeValidacion;
  // VC5: la fecha de ingreso no puede ser futura
  protected readonly hoy = fechaAIso(new Date());

  protected readonly grupo = inject(NonNullableFormBuilder).group({
    empresaId: [null as number | null, Validators.required],
    fechaIngreso: ['', Validators.required],
    puesto: ['', Validators.maxLength(100)],
  });

  protected readonly formulario = crearFormulario(
    this.grupo,
    ({ empresaId, fechaIngreso, puesto }: AsociarEmpresaDto) => {
      const { colaboradorId, relacion } = this.datos;
      // El puesto es opcional: vacío viaja como null
      const datos = { fechaIngreso, puesto: puesto || null };
      return relacion
        ? this.cliente.colaboradores.actualizarEmpresa(colaboradorId, relacion.empresaId, datos)
        : this.cliente.colaboradores.asociarEmpresa(colaboradorId, { empresaId, ...datos });
    },
    // RF4: el detalle muestra lo que devolvió la API
    { alGuardar: (colaborador: ColaboradorDto) => this.dialogo.close(colaborador) },
  );

  ngOnInit(): void {
    const { relacion, asociadas } = this.datos;
    if (relacion) {
      // Al editar, la empresa se muestra, pero no se puede cambiar
      this.empresas.set([{ id: relacion.empresaId, nombre: relacion.nombreComercial }]);
      this.grupo.setValue({
        empresaId: relacion.empresaId,
        fechaIngreso: relacion.fechaIngreso,
        puesto: relacion.puesto ?? '',
      });
      this.grupo.controls.empresaId.disable();
      return;
    }

    this.cliente.empresas
      .listar({ pagina: 1, tamanio: TAMANIO_LISTAS })
      .pipe(takeUntilDestroyed(this.destruccion))
      .subscribe({
        next: (pagina) =>
          this.empresas.set(
            pagina.elementos
              .filter((empresa) => !asociadas.includes(empresa.id))
              .map((empresa) => ({ id: empresa.id, nombre: empresa.nombreComercial })),
          ),
        error: (error: ErrorApi) => this.errorCarga.set(mensajeDeError(error)),
      });
  }
}

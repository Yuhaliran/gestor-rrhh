import {
  ChangeDetectionStrategy,
  Component,
  DestroyRef,
  type OnInit,
  inject,
  input,
  signal,
} from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { NonNullableFormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButton } from '@angular/material/button';
import { MatError, MatFormField, MatLabel } from '@angular/material/form-field';
import { MatInput } from '@angular/material/input';
import { MatOption, MatSelect } from '@angular/material/select';
import { RouterLink } from '@angular/router';

import type { GuardarDepartamentoDto } from '../../contratos/dtos';
import type { ErrorApi } from '../../contratos/errores';
import { CLIENTE_RRHH } from '../../servicios/cliente';
import { mensajeDeError, mensajeDeValidacion } from '../../servicios/formatos';
import { crearFormulario } from '../../servicios/formulario';

// Las listas para elegir no se paginan: el máximo de la API (docs/frontend/PLAN.md)
const TAMANIO_LISTAS = 100;

// RF4 a RF8, RF10 · Alta y edición de un departamento. Al editar, el país queda fijo (RN8).
@Component({
  selector: 'rrhh-departamento-formulario',
  imports: [
    ReactiveFormsModule,
    RouterLink,
    MatButton,
    MatError,
    MatFormField,
    MatInput,
    MatLabel,
    MatOption,
    MatSelect,
  ],
  templateUrl: './departamento-formulario.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class DepartamentoFormulario implements OnInit {
  private readonly cliente = inject(CLIENTE_RRHH);
  private readonly destruccion = inject(DestroyRef);

  // :id de la ruta (withComponentInputBinding); vacío al crear
  readonly id = input<string>();

  protected readonly noExiste = signal(false);
  protected readonly errorCarga = signal<string | null>(null);
  // Al crear, los países para elegir; al editar, sólo el del departamento
  protected readonly paises = signal<{ id: number; nombre: string }[]>([]);
  protected readonly mensajeDe = mensajeDeValidacion;

  protected readonly grupo = inject(NonNullableFormBuilder).group({
    paisId: [null as number | null, Validators.required],
    nombre: ['', [Validators.required, Validators.maxLength(100)]],
  });

  protected readonly formulario = crearFormulario(
    this.grupo,
    (datos: GuardarDepartamentoDto) => {
      const id = Number(this.id());
      return id
        ? this.cliente.departamentos.actualizar(id, datos)
        : this.cliente.departamentos.crear(datos);
    },
    { volverA: '/departamentos' },
  );

  ngOnInit(): void {
    const id = Number(this.id());
    if (!id) {
      this.cliente.paises
        .listar({ pagina: 1, tamanio: TAMANIO_LISTAS })
        .pipe(takeUntilDestroyed(this.destruccion))
        .subscribe({
          next: (pagina) => this.paises.set(pagina.elementos),
          error: (error: ErrorApi) => this.errorCarga.set(mensajeDeError(error)),
        });
      return;
    }

    // RN8: el país se muestra, pero no se puede cambiar (getRawValue lo envía igual)
    this.grupo.controls.paisId.disable();
    this.cliente.departamentos
      .obtener(id)
      .pipe(takeUntilDestroyed(this.destruccion))
      .subscribe({
        next: (departamento) => {
          this.paises.set([{ id: departamento.paisId, nombre: departamento.paisNombre }]);
          this.grupo.patchValue(departamento);
        },
        error: (error: ErrorApi) => {
          // RF7: el registro no existe; cualquier otro error, con su texto (RF8)
          this.noExiste.set(error.estado === 404);
          this.errorCarga.set(error.estado === 404 ? null : mensajeDeError(error));
        },
      });
  }
}

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
import { NonNullableFormBuilder, ReactiveFormsModule } from '@angular/forms';
import { MatButton } from '@angular/material/button';
import { MatError, MatFormField, MatLabel } from '@angular/material/form-field';
import { MatInput } from '@angular/material/input';
import { RouterLink } from '@angular/router';

import type { GuardarColaboradorDto } from '../../contratos/dtos';
import type { ErrorApi } from '../../contratos/errores';
import { CLIENTE_RRHH } from '../../servicios/cliente';
import { fechaAIso, mensajeDeError, mensajeDeValidacion } from '../../servicios/formatos';
import { crearFormulario } from '../../servicios/formulario';
import { datosPersonales } from './datos-personales';

// RF4 a RF8, RF13 · Edición de los datos personales de un colaborador. Sus empresas se manejan
// desde el detalle (RF14).
@Component({
  selector: 'rrhh-colaborador-editar',
  imports: [ReactiveFormsModule, RouterLink, MatButton, MatError, MatFormField, MatInput, MatLabel],
  templateUrl: './colaborador-editar.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ColaboradorEditar implements OnInit {
  private readonly cliente = inject(CLIENTE_RRHH);
  private readonly destruccion = inject(DestroyRef);
  private readonly fb = inject(NonNullableFormBuilder);

  // :id de la ruta (withComponentInputBinding)
  readonly id = input<string>();

  protected readonly noExiste = signal(false);
  protected readonly errorCarga = signal<string | null>(null);
  protected readonly mensajeDe = mensajeDeValidacion;
  // VC5: la fecha de nacimiento no puede ser futura
  protected readonly hoy = fechaAIso(new Date());

  protected readonly grupo = this.fb.group(datosPersonales(this.fb));

  protected readonly formulario = crearFormulario(
    this.grupo,
    (datos: GuardarColaboradorDto) =>
      this.cliente.colaboradores.actualizar(Number(this.id()), datos),
    { volverA: '/colaboradores' },
  );

  ngOnInit(): void {
    this.cliente.colaboradores
      .obtener(Number(this.id()))
      .pipe(takeUntilDestroyed(this.destruccion))
      .subscribe({
        // patchValue toma sólo los datos personales: ignora el id, la edad y las empresas
        next: (colaborador) => this.grupo.patchValue(colaborador),
        error: (error: ErrorApi) => {
          // RF7: el registro no existe; cualquier otro error, con su texto (RF8)
          this.noExiste.set(error.estado === 404);
          this.errorCarga.set(error.estado === 404 ? null : mensajeDeError(error));
        },
      });
  }
}

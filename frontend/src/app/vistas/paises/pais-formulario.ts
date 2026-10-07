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
import {
  NonNullableFormBuilder,
  ReactiveFormsModule,
  Validators,
  type AbstractControl,
  type FormGroupDirective,
  type NgForm,
} from '@angular/forms';
import { MatButton } from '@angular/material/button';
import type { ErrorStateMatcher } from '@angular/material/core';
import { MatError, MatFormField, MatLabel } from '@angular/material/form-field';
import { MatInput } from '@angular/material/input';
import { MatOption, MatSelect } from '@angular/material/select';
import { RouterLink } from '@angular/router';

import type { GuardarPaisDto, Regla29Febrero } from '../../contratos/dtos';
import type { ErrorApi } from '../../contratos/errores';
import { CLIENTE_RRHH } from '../../servicios/cliente';
import {
  OPCIONES_REGLA_29_FEBRERO,
  PATRON_CODIGO_ISO,
  edadMinimaNoMayorQueMaxima,
  mensajeDeError,
  mensajeDeValidacion,
} from '../../servicios/formatos';
import { crearFormulario } from '../../servicios/formulario';

// VC4 es un error del grupo: «Edad máxima» lo muestra como propio (como la API, que lo da en ese campo)
const ERROR_DE_EDADES: ErrorStateMatcher = {
  isErrorState: (control: AbstractControl | null, formulario: FormGroupDirective | NgForm | null) =>
    !!control &&
    (control.touched || !!formulario?.submitted) &&
    (control.invalid || !!control.parent?.hasError('edadMinimaMayorQueMaxima')),
};

// RF4 a RF9 y VC · Alta y edición de un país (patrón de referencia, docs/frontend/CODIFICACION.md).
@Component({
  selector: 'rrhh-pais-formulario',
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
  templateUrl: './pais-formulario.html',
  styleUrl: './pais-formulario.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class PaisFormulario implements OnInit {
  private readonly cliente = inject(CLIENTE_RRHH);
  private readonly destruccion = inject(DestroyRef);

  // :id de la ruta (withComponentInputBinding); vacío al crear
  readonly id = input<string>();

  protected readonly noExiste = signal(false);
  protected readonly errorCarga = signal<string | null>(null);
  protected readonly opcionesRegla = OPCIONES_REGLA_29_FEBRERO;
  protected readonly mensajeDe = mensajeDeValidacion;
  protected readonly errorDeEdades = ERROR_DE_EDADES;

  // RF9: al crear propone 18, 100 y «28 de febrero»
  protected readonly grupo = inject(NonNullableFormBuilder).group(
    {
      nombre: ['', [Validators.required, Validators.maxLength(100)]],
      codigoIso2: ['', [Validators.required, Validators.pattern(PATRON_CODIGO_ISO)]],
      edadMinima: [18, [Validators.required, Validators.min(0)]],
      edadMaxima: [100, [Validators.required, Validators.min(0)]],
      regla29Febrero: ['VeintiochoDeFebrero' as Regla29Febrero, Validators.required],
    },
    { validators: edadMinimaNoMayorQueMaxima },
  );

  protected readonly formulario = crearFormulario(
    this.grupo,
    (datos: GuardarPaisDto) => {
      // RF9: el código ISO viaja en mayúsculas
      const pais = { ...datos, codigoIso2: datos.codigoIso2.toUpperCase() };
      const id = Number(this.id());
      return id ? this.cliente.paises.actualizar(id, pais) : this.cliente.paises.crear(pais);
    },
    { volverA: '/paises' },
  );

  ngOnInit(): void {
    const id = Number(this.id());
    if (!id) {
      return;
    }
    this.cliente.paises
      .obtener(id)
      .pipe(takeUntilDestroyed(this.destruccion))
      .subscribe({
        // patchValue ignora el id
        next: (pais) => this.grupo.patchValue(pais),
        error: (error: ErrorApi) => {
          // RF7: el registro no existe; cualquier otro error, con su texto (RF8)
          this.noExiste.set(error.estado === 404);
          this.errorCarga.set(error.estado === 404 ? null : mensajeDeError(error));
        },
      });
  }
}

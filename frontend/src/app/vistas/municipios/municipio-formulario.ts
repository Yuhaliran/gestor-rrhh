import {
  ChangeDetectionStrategy,
  Component,
  DestroyRef,
  Injector,
  type OnInit,
  computed,
  inject,
  input,
  runInInjectionContext,
  signal,
} from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { NonNullableFormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButton } from '@angular/material/button';
import { MatError, MatFormField, MatLabel } from '@angular/material/form-field';
import { MatInput } from '@angular/material/input';
import { MatOption, MatSelect } from '@angular/material/select';
import { RouterLink } from '@angular/router';

import type { GuardarMunicipioDto } from '../../contratos/dtos';
import type { ErrorApi } from '../../contratos/errores';
import type { Cascada } from '../../contratos/pantallas';
import { crearCascada } from '../../servicios/cascada';
import { CLIENTE_RRHH } from '../../servicios/cliente';
import { mensajeDeError, mensajeDeValidacion } from '../../servicios/formatos';
import { crearFormulario } from '../../servicios/formulario';

interface Opcion {
  id: number;
  nombre: string;
}

// RF4 a RF8, RF11 · Alta de un municipio con país y departamento en cascada (RN6); al editar,
// los dos quedan fijos (RN8).
@Component({
  selector: 'rrhh-municipio-formulario',
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
  templateUrl: './municipio-formulario.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class MunicipioFormulario implements OnInit {
  private readonly cliente = inject(CLIENTE_RRHH);
  private readonly destruccion = inject(DestroyRef);
  private readonly inyector = inject(Injector);

  // :id de la ruta (withComponentInputBinding); vacío al crear
  readonly id = input<string>();

  protected readonly noExiste = signal(false);
  protected readonly errorCarga = signal<string | null>(null);
  protected readonly mensajeDe = mensajeDeValidacion;

  // Al crear, la cascada; al editar, el país y el departamento del municipio
  private readonly cascada = signal<Cascada | null>(null);
  private readonly padres = signal<{ pais: Opcion; departamento: Opcion } | null>(null);

  protected readonly paises = computed<Opcion[]>(() => {
    const padres = this.padres();
    return padres ? [padres.pais] : (this.cascada()?.paises() ?? []);
  });
  protected readonly departamentos = computed<Opcion[]>(() => {
    const padres = this.padres();
    return padres ? [padres.departamento] : (this.cascada()?.departamentos() ?? []);
  });
  // RF8: una lista de la cascada que no se pudo cargar
  protected readonly errorCascada = computed(() => {
    const error = this.cascada()?.error();
    return error ? mensajeDeError(error) : null;
  });

  // El país sólo elige los departamentos: no viaja a la API. Es un control obligatorio para que
  // VC1 lo exija mientras el departamento está deshabilitado; un control deshabilitado no se
  // valida (E-022).
  protected readonly grupo = inject(NonNullableFormBuilder).group({
    paisId: [null as number | null, Validators.required],
    departamentoId: [{ value: null as number | null, disabled: true }, Validators.required],
    nombre: ['', [Validators.required, Validators.maxLength(100)]],
  });

  protected readonly formulario = crearFormulario(
    this.grupo,
    ({ departamentoId, nombre }: GuardarMunicipioDto) => {
      const datos = { departamentoId, nombre };
      const id = Number(this.id());
      return id
        ? this.cliente.municipios.actualizar(id, datos)
        : this.cliente.municipios.crear(datos);
    },
    { volverA: '/municipios' },
  );

  ngOnInit(): void {
    const id = Number(this.id());
    if (!id) {
      // La cascada se crea acá, cuando ya se sabe que es un alta: al editar no hace falta
      this.cascada.set(runInInjectionContext(this.inyector, () => crearCascada()));
      return;
    }

    // RN8: país y departamento se muestran, pero no se pueden cambiar
    this.grupo.controls.paisId.disable();
    this.cliente.municipios
      .obtener(id)
      .pipe(takeUntilDestroyed(this.destruccion))
      .subscribe({
        next: (municipio) => {
          this.padres.set({
            pais: { id: municipio.paisId, nombre: municipio.paisNombre },
            departamento: { id: municipio.departamentoId, nombre: municipio.departamentoNombre },
          });
          this.grupo.patchValue(municipio);
        },
        error: (error: ErrorApi) => {
          // RF7: el registro no existe; cualquier otro error, con su texto (RF8)
          this.noExiste.set(error.estado === 404);
          this.errorCarga.set(error.estado === 404 ? null : mensajeDeError(error));
        },
      });
  }

  // RN6: cambiar el país vacía el departamento, carga los de ese país y lo habilita
  protected elegirPais(paisId: number): void {
    this.cascada()?.elegirPais(paisId);
    const departamento = this.grupo.controls.departamentoId;
    departamento.reset(null);
    departamento.enable();
  }
}

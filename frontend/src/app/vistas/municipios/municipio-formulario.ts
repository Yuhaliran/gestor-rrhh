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

  protected readonly edicion = computed(() => Number(this.id()) > 0);
  protected readonly paises = computed<Opcion[]>(() => {
    const padres = this.padres();
    return padres ? [padres.pais] : (this.cascada()?.paises() ?? []);
  });
  protected readonly departamentos = computed<Opcion[]>(() => {
    const padres = this.padres();
    return padres ? [padres.departamento] : (this.cascada()?.departamentos() ?? []);
  });
  protected readonly paisElegido = computed(
    () => this.padres()?.pais.id ?? this.cascada()?.seleccion().paisId ?? null,
  );

  protected readonly grupo = inject(NonNullableFormBuilder).group({
    departamentoId: [null as number | null, Validators.required],
    nombre: ['', [Validators.required, Validators.maxLength(100)]],
  });

  protected readonly formulario = crearFormulario(
    this.grupo,
    (datos: GuardarMunicipioDto) => {
      const id = Number(this.id());
      return id
        ? this.cliente.municipios.actualizar(id, datos)
        : this.cliente.municipios.crear(datos);
    },
    { volverA: '/municipios' },
  );

  ngOnInit(): void {
    // El departamento se habilita al elegir el país (alta) o queda fijo (edición, RN8)
    this.grupo.controls.departamentoId.disable();

    const id = Number(this.id());
    if (!id) {
      // La cascada se crea acá, cuando ya se sabe que es un alta: al editar no hace falta
      this.cascada.set(runInInjectionContext(this.inyector, () => crearCascada()));
      return;
    }

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

  // RN6: cambiar el país vacía el departamento y carga los de ese país
  protected elegirPais(paisId: number | null): void {
    this.cascada()?.elegirPais(paisId);
    const departamento = this.grupo.controls.departamentoId;
    departamento.reset(null);
    if (paisId === null) {
      departamento.disable();
    } else {
      departamento.enable();
    }
  }
}

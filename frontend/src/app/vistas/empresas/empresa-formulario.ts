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

import type { GuardarEmpresaDto } from '../../contratos/dtos';
import type { ErrorApi } from '../../contratos/errores';
import type { Cascada, SeleccionGeografica } from '../../contratos/pantallas';
import { crearCascada } from '../../servicios/cascada';
import { CLIENTE_RRHH } from '../../servicios/cliente';
import { PATRON_TELEFONO, mensajeDeError, mensajeDeValidacion } from '../../servicios/formatos';
import { crearFormulario } from '../../servicios/formulario';

interface Opcion {
  id: number;
  nombre: string;
}

// RF4 a RF8, RF12 · Empresa con la geografía en cascada (RN6). Al editar, la cascada arranca con
// los valores de la empresa y el país queda fijo (RN8); departamento y municipio pueden cambiar.
@Component({
  selector: 'rrhh-empresa-formulario',
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
  templateUrl: './empresa-formulario.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class EmpresaFormulario implements OnInit {
  private readonly cliente = inject(CLIENTE_RRHH);
  private readonly destruccion = inject(DestroyRef);
  private readonly inyector = inject(Injector);

  // :id de la ruta (withComponentInputBinding); vacío al crear
  readonly id = input<string>();

  protected readonly noExiste = signal(false);
  protected readonly errorCarga = signal<string | null>(null);
  protected readonly mensajeDe = mensajeDeValidacion;

  // Al crear, se crea al iniciar; al editar, cuando llega la empresa, con sus valores
  private readonly cascada = signal<Cascada | null>(null);
  // Al editar, el país de la empresa, aunque no esté entre los países de la lista
  private readonly paisFijo = signal<Opcion | null>(null);

  protected readonly paises = computed<Opcion[]>(() => {
    const fijo = this.paisFijo();
    return fijo ? [fijo] : (this.cascada()?.paises() ?? []);
  });
  protected readonly departamentos = computed<Opcion[]>(
    () => this.cascada()?.departamentos() ?? [],
  );
  protected readonly municipios = computed<Opcion[]>(() => this.cascada()?.municipios() ?? []);
  // RF8: una lista de la cascada que no se pudo cargar
  protected readonly errorCascada = computed(() => {
    const error = this.cascada()?.error();
    return error ? mensajeDeError(error) : null;
  });

  // El país y el departamento sólo eligen las listas: a la API va el municipio. Son controles
  // obligatorios para que VC1 exija el primer nivel sin elegir; un control deshabilitado no se
  // valida (E-022). Cada nivel se habilita cuando se elige su padre.
  protected readonly grupo = inject(NonNullableFormBuilder).group({
    paisId: [null as number | null, Validators.required],
    departamentoId: [{ value: null as number | null, disabled: true }, Validators.required],
    municipioId: [{ value: null as number | null, disabled: true }, Validators.required],
    nit: ['', [Validators.required, Validators.maxLength(20)]],
    razonSocial: ['', [Validators.required, Validators.maxLength(200)]],
    nombreComercial: ['', [Validators.required, Validators.maxLength(200)]],
    telefono: ['', [Validators.required, Validators.pattern(PATRON_TELEFONO)]],
    correo: ['', [Validators.required, Validators.email, Validators.maxLength(254)]],
  });

  protected readonly formulario = crearFormulario(
    this.grupo,
    ({ municipioId, nit, razonSocial, nombreComercial, telefono, correo }: GuardarEmpresaDto) => {
      const datos = { municipioId, nit, razonSocial, nombreComercial, telefono, correo };
      const id = Number(this.id());
      return id ? this.cliente.empresas.actualizar(id, datos) : this.cliente.empresas.crear(datos);
    },
    { volverA: '/empresas' },
  );

  ngOnInit(): void {
    const id = Number(this.id());
    if (!id) {
      this.cascada.set(this.nuevaCascada());
      return;
    }

    // RN8: el país se muestra, pero no se puede cambiar
    this.grupo.controls.paisId.disable();
    this.cliente.empresas
      .obtener(id)
      .pipe(takeUntilDestroyed(this.destruccion))
      .subscribe({
        next: (empresa) => {
          this.paisFijo.set({ id: empresa.paisId, nombre: empresa.paisNombre });
          this.grupo.patchValue(empresa);
          this.grupo.controls.departamentoId.enable();
          this.grupo.controls.municipioId.enable();
          this.cascada.set(
            this.nuevaCascada({
              paisId: empresa.paisId,
              departamentoId: empresa.departamentoId,
              municipioId: empresa.municipioId,
            }),
          );
        },
        error: (error: ErrorApi) => {
          // RF7: el registro no existe; cualquier otro error, con su texto (RF8)
          this.noExiste.set(error.estado === 404);
          this.errorCarga.set(error.estado === 404 ? null : mensajeDeError(error));
        },
      });
  }

  // RN6: cambiar el país vacía departamento y municipio, y habilita el departamento
  protected elegirPais(paisId: number): void {
    this.cascada()?.elegirPais(paisId);
    const { departamentoId, municipioId } = this.grupo.controls;
    departamentoId.reset(null);
    departamentoId.enable();
    municipioId.reset(null);
    municipioId.disable();
  }

  // RN6: cambiar el departamento vacía el municipio y lo habilita
  protected elegirDepartamento(departamentoId: number): void {
    this.cascada()?.elegirDepartamento(departamentoId);
    const municipio = this.grupo.controls.municipioId;
    municipio.reset(null);
    municipio.enable();
  }

  // La cascada usa inject(): se crea en el contexto de inyección del componente
  private nuevaCascada(inicial?: SeleccionGeografica): Cascada {
    return runInInjectionContext(this.inyector, () => crearCascada(inicial));
  }
}

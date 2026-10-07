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
import { MatError, MatFormField, MatLabel } from '@angular/material/form-field';
import { MatInput } from '@angular/material/input';
import { MatOption, MatSelect } from '@angular/material/select';
import { RouterLink } from '@angular/router';

import type { CrearColaboradorDto, EmpresaDto } from '../../contratos/dtos';
import type { ErrorApi } from '../../contratos/errores';
import { CLIENTE_RRHH } from '../../servicios/cliente';
import { fechaAIso, mensajeDeError, mensajeDeValidacion } from '../../servicios/formatos';
import { crearFormulario } from '../../servicios/formulario';
import { datosPersonales } from './datos-personales';

// Las listas para elegir no se paginan: el máximo de la API (docs/frontend/PLAN.md)
const TAMANIO_LISTAS = 100;

// RF4 a RF8, RF13 · Alta de un colaborador con sus datos personales y una o varias empresas: al
// menos una (RN3). Después, sus empresas se manejan desde el detalle (RF14).
@Component({
  selector: 'rrhh-colaborador-alta',
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
  templateUrl: './colaborador-alta.html',
  styleUrl: './colaborador-alta.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ColaboradorAlta implements OnInit {
  private readonly cliente = inject(CLIENTE_RRHH);
  private readonly destruccion = inject(DestroyRef);
  private readonly fb = inject(NonNullableFormBuilder);

  protected readonly empresas = signal<EmpresaDto[]>([]);
  protected readonly errorCarga = signal<string | null>(null);
  protected readonly mensajeDe = mensajeDeValidacion;
  // VC5: las fechas no pueden ser futuras
  protected readonly hoy = fechaAIso(new Date());

  // Empieza con una fila de empresa (RN3)
  protected readonly grupo = this.fb.group({
    ...datosPersonales(this.fb),
    empresas: this.fb.array([this.nuevaFila()]),
  });
  protected readonly filas = this.grupo.controls.empresas;

  protected readonly formulario = crearFormulario(
    this.grupo,
    (datos: CrearColaboradorDto) =>
      this.cliente.colaboradores.crear({
        ...datos,
        // El puesto es opcional: vacío viaja como null
        empresas: datos.empresas.map((empresa) => ({ ...empresa, puesto: empresa.puesto || null })),
      }),
    { volverA: '/colaboradores' },
  );

  ngOnInit(): void {
    this.cliente.empresas
      .listar({ pagina: 1, tamanio: TAMANIO_LISTAS })
      .pipe(takeUntilDestroyed(this.destruccion))
      .subscribe({
        next: (pagina) => this.empresas.set(pagina.elementos),
        error: (error: ErrorApi) => this.errorCarga.set(mensajeDeError(error)),
      });
  }

  protected agregarEmpresa(): void {
    this.filas.push(this.nuevaFila());
  }

  // RN3: la última fila no se quita («Quitar» está deshabilitado)
  protected quitarEmpresa(indice: number): void {
    if (this.filas.length > 1) {
      this.filas.removeAt(indice);
    }
  }

  private nuevaFila() {
    return this.fb.group({
      empresaId: [null as number | null, Validators.required],
      fechaIngreso: ['', Validators.required],
      puesto: ['', Validators.maxLength(100)],
    });
  }
}

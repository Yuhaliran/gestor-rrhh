import { DestroyRef, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormArray, FormGroup, type AbstractControl } from '@angular/forms';
import { Router } from '@angular/router';
import type { Observable } from 'rxjs';

import type { ErrorApi } from '../contratos/errores';
import type { CrearFormulario, Formulario } from '../contratos/pantallas';
import { AVISOS } from './avisos';
import { mensajeDeError } from './formatos';

const VALOR_NO_VALIDO = 'El valor no es válido.';

// RF4 a RF8 y VC · Formulario.
export const crearFormulario: CrearFormulario = <TDatos, TRespuesta>(
  grupo: FormGroup,
  guardar: (datos: TDatos) => Observable<TRespuesta>,
  opciones: { volverA: string },
): Formulario => {
  const avisos = inject(AVISOS);
  const router = inject(Router);
  const destruccion = inject(DestroyRef);

  const errorGeneral = signal<string | null>(null);
  const guardando = signal(false);

  // RF5: cada mensaje en su control; lo que no tiene control va al aviso general
  const mostrarError = (error: ErrorApi) => {
    if (error.estado !== 400 || Object.keys(error.errores ?? {}).length === 0) {
      errorGeneral.set(mensajeDeError(error));
      return;
    }
    const sinControl: string[] = [];
    for (const [clave, mensajes] of Object.entries(error.errores)) {
      // El interceptor ya normaliza 'dto' y '$.campo'; se repite por si llega un error sin pasar por él
      if (clave === 'dto') {
        continue;
      }
      const ilegible = clave.startsWith('$.');
      const control = controlDe(grupo, ilegible ? clave.slice(2) : clave);
      const mensaje = ilegible ? VALOR_NO_VALIDO : mensajes.join(' ');
      if (control === null) {
        sinControl.push(mensaje);
      } else {
        control.setErrors({ api: mensaje });
        control.markAsTouched();
      }
    }
    errorGeneral.set(sinControl.length > 0 ? sinControl.join(' ') : null);
  };

  return {
    errorGeneral: errorGeneral.asReadonly(),
    guardando: guardando.asReadonly(),
    enviar: () => {
      errorGeneral.set(null);
      if (grupo.invalid) {
        grupo.markAllAsTouched();
        return;
      }
      guardando.set(true);
      guardar(grupo.getRawValue() as TDatos)
        .pipe(takeUntilDestroyed(destruccion))
        .subscribe({
          next: () => {
            guardando.set(false);
            avisos.exito('Se guardó correctamente.');
            void router.navigateByUrl(opciones.volverA);
          },
          error: (error: ErrorApi) => {
            guardando.set(false);
            mostrarError(error);
          },
        });
    },
  };
};

// Control de una clave de la API ('empresas[1].fechaIngreso'), sin distinguir mayúsculas (RF5),
// o null si el formulario no tiene ese campo.
function controlDe(grupo: FormGroup, clave: string): AbstractControl | null {
  let actual: AbstractControl | null = grupo;
  for (const parte of clave.split(/[.[\]]/).filter((p) => p !== '')) {
    if (actual instanceof FormArray) {
      const indice = Number(parte);
      actual = Number.isInteger(indice) ? (actual.at(indice) ?? null) : null;
    } else if (actual instanceof FormGroup) {
      const controles: Record<string, AbstractControl> = actual.controls;
      const nombre = Object.keys(controles).find((n) => n.toLowerCase() === parte.toLowerCase());
      actual = nombre === undefined ? null : controles[nombre];
    } else {
      return null;
    }
    if (actual === null) {
      return null;
    }
  }
  return actual === grupo ? null : actual;
}

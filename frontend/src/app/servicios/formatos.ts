import type { ValidatorFn } from '@angular/forms';

import type {
  FechaAIso,
  FechaDesdeIso,
  FechaParaMostrar,
  MensajeDeError,
  MensajeDeValidacion,
  OpcionRegla,
  TextoRegla29Febrero,
} from '../contratos/pantallas';

// RNF2, RF5 a RF8 y VC · Formatos y textos. Las funciones son esqueletos hasta la tarea 37 (E-014).
const noImplementado = (): never => {
  throw new Error('No implementado');
};

// Opciones del campo «Cumpleaños del 29 de febrero» (Contrato de interfaz)
export const OPCIONES_REGLA_29_FEBRERO: readonly OpcionRegla[] = [
  { valor: 'VeintiochoDeFebrero', texto: '28 de febrero' },
  { valor: 'PrimeroDeMarzo', texto: '1 de marzo' },
];

export const fechaParaMostrar: FechaParaMostrar = noImplementado;
export const fechaDesdeIso: FechaDesdeIso = noImplementado;
export const fechaAIso: FechaAIso = noImplementado;
export const textoRegla29Febrero: TextoRegla29Febrero = noImplementado;
export const mensajeDeError: MensajeDeError = noImplementado;
export const mensajeDeValidacion: MensajeDeValidacion = noImplementado;

// VC4: validador del grupo de País; { edadMinimaMayorQueMaxima: true } si la mínima supera a la máxima
export const edadMinimaNoMayorQueMaxima: ValidatorFn = noImplementado;

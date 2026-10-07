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

// RNF2, RF5 a RF8 y VC · Formatos y textos de la interfaz.

// V5 y VC3 · Teléfono: dígitos, espacios, +, - y paréntesis, de 7 a 20 caracteres
// (el mismo de src/RRHH.Contratos/Comun/Formatos.cs)
export const PATRON_TELEFONO = /^[0-9+()\- ]{7,20}$/;

// VC4 · Código ISO del país: 2 letras (el mismo de GuardarPaisDto)
export const PATRON_CODIGO_ISO = /^[A-Za-z]{2}$/;

// Opciones del campo «Cumpleaños del 29 de febrero» (Contrato de interfaz)
export const OPCIONES_REGLA_29_FEBRERO: readonly OpcionRegla[] = [
  { valor: 'VeintiochoDeFebrero', texto: '28 de febrero' },
  { valor: 'PrimeroDeMarzo', texto: '1 de marzo' },
];

const dosDigitos = (numero: number) => String(numero).padStart(2, '0');

// Año, mes y día de 'aaaa-mm-dd' (también de 'aaaa-mm-ddThh:mm', por si llega con hora)
const partesIso = (iso: string) => iso.slice(0, 10).split('-').map(Number);

// '2026-10-07' → '07/10/2026'
export const fechaParaMostrar: FechaParaMostrar = (iso) => {
  const [anio, mes, dia] = partesIso(iso);
  return `${dosDigitos(dia)}/${dosDigitos(mes)}/${anio}`;
};

// Medianoche local. new Date('aaaa-mm-dd') la tomaría en UTC: en UTC−6 sería el día anterior.
export const fechaDesdeIso: FechaDesdeIso = (iso) => {
  const [anio, mes, dia] = partesIso(iso);
  return new Date(anio, mes - 1, dia);
};

// Con el año, mes y día locales; toISOString() pasaría a UTC y podría correr el día
export const fechaAIso: FechaAIso = (fecha) =>
  `${fecha.getFullYear()}-${dosDigitos(fecha.getMonth() + 1)}-${dosDigitos(fecha.getDate())}`;

export const textoRegla29Febrero: TextoRegla29Febrero = (regla) =>
  OPCIONES_REGLA_29_FEBRERO.find((opcion) => opcion.valor === regla)?.texto ?? regla;

// RF6 a RF8: nunca un detalle técnico
export const mensajeDeError: MensajeDeError = (error) => {
  switch (error.estado) {
    case 0:
      return 'No se pudo conectar con la API.';
    case 404:
      return 'El registro no existe.';
    case 409:
      return error.detalle ?? 'Ocurrió un error inesperado.';
    default:
      return 'Ocurrió un error inesperado.';
  }
};

// El error de la API (setErrors({ api })) tiene prioridad: es el que llegó al guardar
export const mensajeDeValidacion: MensajeDeValidacion = (errores) => {
  if (errores === null) {
    return null;
  }
  if (typeof errores['api'] === 'string') {
    return errores['api'];
  }
  if (errores['required']) {
    return 'Este campo es obligatorio.';
  }
  if (errores['maxlength']) {
    return `Admite hasta ${errores['maxlength'].requiredLength} caracteres.`;
  }
  if (errores['pattern'] || errores['email']) {
    return 'El formato no es válido.';
  }
  if (errores['min']) {
    return 'No puede ser negativa.';
  }
  if (errores['edadMinimaMayorQueMaxima']) {
    return 'La edad mínima no puede ser mayor que la máxima.';
  }
  return null;
};

// VC4: validador del grupo de País; { edadMinimaMayorQueMaxima: true } si la mínima supera a la máxima
export const edadMinimaNoMayorQueMaxima: ValidatorFn = (grupo) => {
  const minima: unknown = grupo.get('edadMinima')?.value;
  const maxima: unknown = grupo.get('edadMaxima')?.value;
  return typeof minima === 'number' && typeof maxima === 'number' && minima > maxima
    ? { edadMinimaMayorQueMaxima: true }
    : null;
};

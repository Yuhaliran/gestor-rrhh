import { HttpErrorResponse, type HttpInterceptorFn } from '@angular/common/http';
import { catchError, throwError } from 'rxjs';

import type { ErrorApi, ProblemDetails } from '../contratos/errores';

// Mensaje para un campo que la API no pudo leer del JSON: el suyo es técnico y en inglés (RF5, RF8)
const VALOR_NO_VALIDO = 'El valor no es válido.';

// Traduce toda respuesta no exitosa a un ErrorApi (docs/frontend/PLAN.md, «Errores»).
export const erroresInterceptor: HttpInterceptorFn = (pedido, siguiente) =>
  siguiente(pedido).pipe(
    catchError((error: unknown) =>
      throwError(() => (error instanceof HttpErrorResponse ? aErrorApi(error) : error)),
    ),
  );

function aErrorApi(respuesta: HttpErrorResponse): ErrorApi {
  // Sin conexión (estado 0) o un 500 no traen ProblemDetails
  const cuerpo: ProblemDetails =
    typeof respuesta.error === 'object' && respuesta.error !== null ? respuesta.error : {};
  return {
    estado: respuesta.status,
    detalle: typeof cuerpo.detail === 'string' ? cuerpo.detail : undefined,
    errores: respuesta.status === 400 && cuerpo.errors ? normalizarErrores(cuerpo.errors) : {},
  };
}

// Claves en camelCase, con los índices. Del JSON ilegible, 'dto' (el parámetro de la acción) se
// descarta y '$.campo' pasa a 'campo' con un mensaje propio.
function normalizarErrores(errores: Record<string, string[]>): Record<string, string[]> {
  const resultado: Record<string, string[]> = {};
  for (const [clave, mensajes] of Object.entries(errores)) {
    if (clave === 'dto') {
      continue;
    }
    const ilegible = clave.startsWith('$.');
    resultado[normalizarClave(ilegible ? clave.slice(2) : clave)] = ilegible
      ? [VALOR_NO_VALIDO]
      : mensajes;
  }
  return resultado;
}

// 'Empresas[0].FechaIngreso' → 'empresas[0].fechaIngreso'
function normalizarClave(clave: string): string {
  return clave
    .split('.')
    .map((segmento) => segmento.charAt(0).toLowerCase() + segmento.slice(1))
    .join('.');
}

import type { HttpInterceptorFn } from '@angular/common/http';

// Traduce toda respuesta no exitosa a un ErrorApi (docs/frontend/PLAN.md, «Errores»).
// Esqueleto hasta la tarea 37 (E-014).
export const erroresInterceptor: HttpInterceptorFn = () => {
  throw new Error('No implementado');
};

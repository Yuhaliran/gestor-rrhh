import { InjectionToken } from '@angular/core';

import type { Avisos } from '../contratos/pantallas';

// Los avisos al usuario, pedidos por su interfaz. app.config.ts los registra con AvisosMaterial
// (componentes/); en las pruebas se reemplazan por un doble.
export const AVISOS = new InjectionToken<Avisos>('Avisos');

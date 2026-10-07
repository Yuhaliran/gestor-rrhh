import { InjectionToken } from '@angular/core';

import type { Confirmacion } from '../contratos/pantallas';

// La confirmación antes de eliminar, pedida por su interfaz. app.config.ts la registra con
// ConfirmacionMaterial (componentes/).
export const CONFIRMACION = new InjectionToken<Confirmacion>('Confirmacion');

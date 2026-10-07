import { InjectionToken } from '@angular/core';

import type { ClienteRrhh } from '../contratos/cliente';

// El cliente de la API, pedido por su interfaz. app.config.ts lo registra con ClienteRrhhHttp (api/).
export const CLIENTE_RRHH = new InjectionToken<ClienteRrhh>('ClienteRrhh');

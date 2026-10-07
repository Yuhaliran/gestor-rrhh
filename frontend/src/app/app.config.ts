import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { ApplicationConfig, provideBrowserGlobalErrorListeners } from '@angular/core';
import { provideRouter, withComponentInputBinding } from '@angular/router';

import { environment } from '../environments/environment';
import { ClienteRrhhHttp } from './api/cliente-rrhh-http';
import { erroresInterceptor } from './api/errores.interceptor';
import { URL_API } from './api/url-api';
import { routes } from './app.routes';
import { AvisosMaterial } from './componentes/avisos-material';
import { ConfirmacionMaterial } from './componentes/confirmacion-material';
import { AVISOS } from './servicios/avisos';
import { CLIENTE_RRHH } from './servicios/cliente';
import { CONFIRMACION } from './servicios/confirmacion';

// Raíz de composición (docs/frontend/PLAN.md, «Arquitectura»): el único lugar que conoce api/ y
// environments/. La lógica pide el cliente, los avisos y la confirmación por su interfaz.
export const appConfig: ApplicationConfig = {
  providers: [
    provideBrowserGlobalErrorListeners(),
    // withComponentInputBinding: el :id de la ruta llega como input() del componente
    provideRouter(routes, withComponentInputBinding()),
    provideHttpClient(withInterceptors([erroresInterceptor])),
    { provide: URL_API, useValue: environment.urlApi },
    { provide: CLIENTE_RRHH, useClass: ClienteRrhhHttp },
    { provide: AVISOS, useClass: AvisosMaterial },
    { provide: CONFIRMACION, useClass: ConfirmacionMaterial },
  ],
};

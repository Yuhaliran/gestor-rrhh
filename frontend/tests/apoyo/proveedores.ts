import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { EnvironmentProviders, Provider } from '@angular/core';
import { provideRouter } from '@angular/router';
import { ClienteRrhhHttp } from '../../src/app/api/cliente-rrhh-http';
import { erroresInterceptor } from '../../src/app/api/errores.interceptor';
import { URL_API } from '../../src/app/api/url-api';
import { CLIENTE_RRHH } from '../../src/app/servicios/cliente';
import { AVISOS } from '../../src/app/servicios/avisos';
import { AvisosMaterial } from '../../src/app/componentes/avisos-material';
import { CONFIRMACION } from '../../src/app/servicios/confirmacion';
import { ConfirmacionMaterial } from '../../src/app/componentes/confirmacion-material';

export function proveedoresDePrueba(): (Provider | EnvironmentProviders)[] {
  return [
    provideHttpClient(withInterceptors([erroresInterceptor])),
    provideHttpClientTesting(),
    provideRouter([]),
    { provide: URL_API, useValue: 'http://api.prueba' },
    { provide: CLIENTE_RRHH, useClass: ClienteRrhhHttp },
    { provide: AVISOS, useClass: AvisosMaterial },
    { provide: CONFIRMACION, useClass: ConfirmacionMaterial },
  ];
}

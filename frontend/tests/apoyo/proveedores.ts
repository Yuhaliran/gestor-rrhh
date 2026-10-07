import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { EnvironmentProviders, Provider } from '@angular/core';
import { provideRouter } from '@angular/router';
import { ConfirmationService, MessageService } from 'primeng/api';
import { ClienteRrhhHttp } from '../../src/app/api/cliente-rrhh-http';
import { erroresInterceptor } from '../../src/app/api/errores.interceptor';
import { URL_API } from '../../src/app/api/url-api';
import { CLIENTE_RRHH } from '../../src/app/servicios/cliente';

export function proveedoresDePrueba(): (Provider | EnvironmentProviders)[] {
  return [
    provideHttpClient(withInterceptors([erroresInterceptor])),
    provideHttpClientTesting(),
    provideRouter([]),
    { provide: URL_API, useValue: 'http://api.prueba' },
    { provide: CLIENTE_RRHH, useClass: ClienteRrhhHttp },
    MessageService,
    ConfirmationService,
  ];
}

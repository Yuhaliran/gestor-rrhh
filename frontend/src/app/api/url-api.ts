import { InjectionToken } from '@angular/core';

// URL base de la API (RNF1). app.config.ts la registra desde environments/.
export const URL_API = new InjectionToken<string>('UrlApi');

// Errores de la API (docs/frontend/PLAN.md, «Errores»).

// Cuerpo de una respuesta de error de la API (ProblemDetails y ValidationProblemDetails de ASP.NET Core)
export interface ProblemDetails {
  status?: number;
  title?: string;
  detail?: string;
  errors?: Record<string, string[]>;
}

// Toda respuesta no exitosa, ya traducida por el interceptor de api/.
export interface ErrorApi {
  // Código HTTP; 0 si no hubo respuesta (RF8)
  estado: number;
  // detail de la API (409, 404)
  detalle?: string;
  // Errores por campo de un 400, con las claves normalizadas: 'nombre', 'empresas[0].fechaIngreso'.
  // Una clave $.campo (JSON ilegible) llega como 'campo' con «El valor no es válido.»; 'dto' no llega.
  errores: Record<string, string[]>;
}

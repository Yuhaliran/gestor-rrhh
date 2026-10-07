# Rol: implementador

- Implementás el código de `src/` (backend y `RRHH.Web`) y de `frontend/` (frontend Angular) para
  que pasen las pruebas escritas por el tester.
- Antes de implementar, proponé el **contrato** (interfaz del servicio y DTOs; en el frontend,
  `frontend/src/app/contratos/`) y esperá aprobación.
- No modifiques nada en `tests/`, `frontend/tests/` ni `postman/`. Si una prueba parece incorrecta,
  detenete y explicá por qué, citando la especificación.
- Fuera de `src/` y `frontend/`, sólo podés agregar entradas a `docs/ERRORES_RECURRENTES.md`.
- Seguí `docs/PLAN.md` para la arquitectura del backend, `docs/frontend/PLAN.md` para la del
  frontend y las convenciones de `AGENTS.md`.
- Al terminar: en el backend, `dotnet build` y `dotnet test` en verde; en el frontend,
  `npm run lint`, `npm run format:check`, `npm test` y `npm run build`. Proponé el mensaje de commit.

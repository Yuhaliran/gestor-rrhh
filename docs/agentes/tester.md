# Rol: tester

Herramienta: Antigravity CLI (`agy`) con un modelo Gemini, u otra de un proveedor distinto al del implementador.

- Escribís pruebas a partir de `docs/ESPECIFICACION.md`, `docs/PLAN_PRUEBAS.md` y el
  **contrato** (interfaces y DTOs). Son pruebas de caja negra.
- **No leas** la implementación de los servicios (`src/RRHH.Application/Servicios/`,
  `src/RRHH.Infrastructure/`), aunque exista. Si necesitás algo que el contrato no define,
  pedilo en lugar de deducirlo del código.
- Sólo modificás `tests/`, `postman/` y, de `docs/PLAN_PRUEBAS.md`, la tabla de trazabilidad.
  También podés agregar entradas a `docs/ERRORES_RECURRENTES.md`.
- Cubrí cada regla (RN) y validación (V) de la tarea, con valores límite y casos de error,
  y actualizá la tabla de trazabilidad de `docs/PLAN_PRUEBAS.md`.
- Las pruebas nuevas deben compilar y quedar en rojo hasta que se implemente la tarea.
- Nombres: `Metodo_Escenario_ResultadoEsperado`, estructura Arrange-Act-Assert.

## Frontend Angular
- Fuentes: `docs/frontend/ESPECIFICACION.md` (incluido el «Contrato de interfaz»),
  `docs/frontend/PLAN.md` («Errores» y «Lógica de las pantallas»), `docs/frontend/PLAN_PRUEBAS.md`,
  `docs/frontend/CODIFICACION.md` (sección Pruebas), la API de `docs/PLAN.md` y el contrato en
  `frontend/src/app/contratos/`.
- **No leas** `frontend/src/app/api/`, `servicios/`, `vistas/` ni `componentes/`. Las pruebas los
  importan por los nombres que fijan el plan y el contrato.
- Escribís en `frontend/tests/` y en la tabla de trazabilidad de `docs/frontend/PLAN_PRUEBAS.md`.
- Comandos, en `frontend/`: `npm test`, `npm test -- --include <ruta>` (desde `frontend/src`, por
  ejemplo `../tests/unitarias/listado.spec.ts`), `npm run lint`, `npm run build`,
  `npm run format:check` y `npm run format:pruebas` (formatea sólo tus pruebas).

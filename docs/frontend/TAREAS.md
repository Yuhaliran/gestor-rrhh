# Tareas del frontend Angular

La numeración sigue la del backend (`docs/TAREAS.md`: fases 0 a 8, tareas 1 a 28).

Mismo flujo de `docs/TAREAS.md`: una tarea por vez, un commit por tarea con el mensaje sugerido,
una rama por fase y un PR a `main` al cerrarla (`main` está protegida). Al terminar cada tarea:
`npm run lint`, `npm run format:check`, `npm test` y `npm run build` en `frontend/` (y
`dotnet test` si se tocó la API).

Cada commit indica el rol en el cuerpo, como en el backend (`docs/AGENTES.md`, «Registro»):
`Rol: tester (Gemini 3.1 Pro). Revisado por: <responsable>.` Si se agota la cuota de Gemini, el
tester sigue con GPT-OSS 120B (Medium) y la línea lo indica.

En las tareas con lógica nueva se sigue el ciclo de `docs/AGENTES.md`:
contrato → pruebas (tester) → implementación (implementador) → revisión. Por el plazo, la revisión
(sesión nueva del revisor) se hace **una vez por fase**, antes del PR, y no por tarea.

## Calendario
Entrega: miércoles 7 de octubre de 2026, 23:59.

| Cuándo | Fase | Resultado |
|---|---|---|
| Martes, hasta ~23:30 | 9 | Documentos, CORS y proyecto creado |
| Miércoles, 8:00 a 13:00 | 9 y 10 | Reglas de arquitectura, permisos y CI; cliente, lógica de pantallas, layout; países, departamentos y municipios |
| Miércoles, 13:00 a 19:00 | 11 | Empresas y colaboradores |
| Miércoles, 19:00 a 22:00 | 12 | Aceptación, README, tag. Quedan 2 horas de margen |

El martes se hacen las tareas 29, 30 y 31; la 29b, la 32, la 33 y la 34 quedan para el miércoles
temprano, antes de la 35. La 33 tiene que estar antes de la 36, la primera del tester en el
frontend.

Si el tiempo no alcanza, el orden de recorte es: las pruebas de componentes de la tarea 39 se
reducen a las específicas (padre fijo); Playwright no se hace. No se recorta: CORS, la paridad
funcional (CAF1 a CAF4) ni las pruebas unitarias de lo genérico.

## Fase 9 · rama `chore/frontend-estructura`
- [x] 29. Agregar `docs/frontend/` (especificación, plan, plan de pruebas y tareas) y referenciarlo
       desde `AGENTS.md` (documentos, estructura y roles) y desde el README; ampliar el rol en
       `docs/agentes/implementador.md` (`docs(frontend): especificación, planes y tareas`)
- [x] 29b. `docs/frontend/CODIFICACION.md`: convenciones (nombres, componentes, estilo) y el
        patrón de referencia de Países en Angular (cliente, lógica de pantalla, listado y
        formulario), como `docs/CODIFICACION.md` en el backend. Se escribe antes de codificar y se
        ajusta en la tarea 38 si algo no funciona igual (`docs(frontend): pautas de codificación`)
- [x] 30. CORS en la API (CORS1). El tester escribe primero las dos pruebas de integración del
       preflight (`test(api): CORS para el frontend`); luego el implementador configura
       `Program.cs` y los appsettings (`feat(api): CORS para el frontend`). Postman sigue pasando
- [x] 31. Crear `frontend/` con `npx @angular/cli@22 new` (sin SSR, sin zone.js, con CSS y
       Vitest); `ng add angular-eslint`; agregar PrimeNG, `@angular/cdk`, `@primeuix/themes` y
       PrimeIcons (en la tarea 37 se reemplazaron por Angular Material: E-021),
       @testing-library/angular, @testing-library/dom y user-event (Prettier ya viene
       con el CLI); scripts de `package.json`; `environments/`; puerto 4200 fijo en
       `angular.json`; `strict` y `strictTemplates` explícitos; Prettier con `endOfLine: auto`
       (`core.autocrlf` deja CRLF en Windows); borrar el `.editorconfig` que genera el CLI y
       agregar `[*.{ts,html,css,js,mjs}]` al de la raíz; comandos del frontend en `AGENTS.md`;
       una prueba de componente trivial en `frontend/tests/`, con Testing Library, que confirma
       que `ng test` encuentra las pruebas fuera de `src/` (opción `include` de `angular.json`) y
       que Testing Library funciona con esta versión de Angular. Las carpetas de
       `docs/frontend/PLAN.md`, «Estructura», se crean con su contenido en la tarea 35
       (`chore(frontend): estructura del proyecto Angular`)
- [x] 32. Reglas ARQF1 a ARQF3 en `eslint.config.js`, verificadas con un import prohibido a
       propósito (`test(frontend): reglas de dependencia entre capas`)
- [x] 33. Permisos por rol para el frontend: `.claude/settings.json`, hook
       `permisos-por-rol.mjs` y roles del tester y del revisor en `docs/agentes/`
       (`docs/frontend/PLAN.md`, «Agentes») (`chore: permisos de los agentes para el frontend`)
- [x] 34. Job del frontend en la CI; agregarlo como check obligatorio del ruleset
       (`ci: lint, pruebas y build del frontend`)

## Fase 10 · rama `feature/frontend-base-geografia`
- [x] 35. Contrato: `contratos/` completo (DTOs, `ErrorApi`, `ClienteRrhh` y `pantallas.ts`),
       revisado contra `src/RRHH.Contratos`; tokens `CLIENTE_RRHH` y `URL_API`; esqueletos de
       `api/` y `servicios/` que lanzan `No implementado` (`feat(frontend): contrato`)
- [x] 36. Tester: unitarias del cliente HTTP, la lógica de pantallas y los formatos, y las
       respuestas de `tests/apoyo/` para toda la API, desde la especificación
       (`test(frontend): pruebas del cliente y la lógica de pantallas`)
- [x] 37. Implementación de `api/` y `servicios/`, `app.config.ts`, layout con menú, inicio y
       router con todas las rutas (las pantallas pendientes, vacías) hasta que las pruebas de la
       tarea 36 queden en verde (`feat(frontend): cliente de la API, lógica de pantallas y layout`)
- [x] 38. Países, patrón de referencia para las demás pantallas: tester (componentes) →
        implementador (`test(frontend): países` · `feat(frontend): mantenimiento de países`)
- [x] 39. Departamentos y municipios, con padre fijo al editar y cascada en el alta de municipio
        (`test(frontend): departamentos y municipios` ·
        `feat(frontend): departamentos y municipios`)

## Fase 11 · rama `feature/frontend-empresas-colaboradores`
- [ ] 40. Empresas con cascada y país fijo al editar; colaboradores de una empresa
        (`test(frontend): empresas` · `feat(frontend): mantenimiento de empresas`)
- [ ] 41. Colaboradores: listado, alta con empresas, editar datos personales
        (`test(frontend): colaboradores` · `feat(frontend): mantenimiento de colaboradores`)
- [ ] 42. Detalle del colaborador: edad y empresas; asociar, editar y quitar
        (`test(frontend): detalle del colaborador` · `feat(frontend): empresas de un colaborador`)

## Fase 12 · rama `docs/frontend-entrega`
- [ ] 43. Lista de aceptación de `docs/frontend/PLAN_PRUEBAS.md` contra la API real; anotar el
        resultado en el PR; corregir lo que falle en commits propios (`fix(frontend): ...`)
- [ ] 44. README: sección «Frontend Angular» (por qué Angular frente a Flutter y Vue) y el
        frontend integrado en las secciones existentes: «Cómo ejecutar» (tercera terminal con
        `npm start`), «Pruebas», «Arquitectura», «Estructura del repositorio», «Uso de IA» y
        «Mejoras futuras»; revisión final sin advertencias ni secretos
        (`docs: frontend Angular en el README`)
- [ ] 45. PR de la fase 12 a `main`; después, tag sobre `main`:
        `git tag -a v1.1.0 -m "Frontend Angular"`

Mejoras futuras (al README): E2E con Playwright; tipos generados desde OpenAPI; búsqueda en las
listas de países y empresas con más de 100 elementos; despliegue del frontend compilado.

## Prompts
Los de `docs/PROMPTS.md` sirven cambiando las rutas. Para el frontend:

**Claude Code · contrato (tarea 35)**
```
Leé AGENTS.md y docs/frontend/. Tarea 35 de docs/frontend/TAREAS.md. Proponé sólo el contrato
en frontend/src/app/contratos/, revisado contra src/RRHH.Contratos y docs/frontend/PLAN.md
(«Contrato del cliente», «Errores» y «Lógica de las pantallas»). No implementes nada. Esperá mi
aprobación.
```

**Antigravity CLI · pruebas** (`$env:RRHH_ROL = "tester"; agy`)
```
Actuás con el rol de docs/agentes/tester.md. Leé AGENTS.md, docs/frontend/ESPECIFICACION.md,
docs/frontend/PLAN.md, docs/frontend/PLAN_PRUEBAS.md, docs/frontend/CODIFICACION.md (sección
Pruebas), docs/PLAN.md (sección API), docs/ERRORES_RECURRENTES.md y frontend/src/app/contratos/.
No leas la implementación.
Escribí las pruebas de la tarea N en frontend/tests/: deben compilar y quedar en rojo.
Actualizá la tabla de trazabilidad de docs/frontend/PLAN_PRUEBAS.md.
```

**Claude Code · implementación**
```
Tarea N de docs/frontend/TAREAS.md. El contrato está aprobado y las pruebas del tester están en
frontend/tests/. Implementá hasta que npm test, lint y build queden en verde, sin tocar
frontend/tests/. Si una prueba contradice docs/frontend/ESPECIFICACION.md, detenete y explicá por qué.
```

## Retomar en otra computadora
1. Instalar: Git, .NET 10 SDK, Node.js 24 LTS (Angular 22 pide `^22.22.3 || ^24.15.0`), SQL
   Server LocalDB o Express (`docs/ENTORNO.md`) y Antigravity CLI para el tester. El Angular CLI
   no se instala: viene con el proyecto.
2. `git clone https://github.com/Yuhaliran/gestor-rrhh` y cambiar a la rama de la fase en curso.
3. Configurar la base **desde tu terminal**, no desde Claude Code (E-020): `dotnet tool restore`,
   `dotnet build`, user-secrets y `dotnet ef database update` (`docs/ENTORNO.md`). Comprobar
   `GET http://localhost:5279/health` con `dotnet run --project src/RRHH.Api --launch-profile http`.
   Si es la computadora prestada, revisar que `agy -p "/hooks"` liste `permisos-por-rol`.
4. En `frontend/`, `npm ci` (después de la tarea 31).
5. Abrir Claude Code en la carpeta del repositorio: lee `CLAUDE.md` → `AGENTS.md` → `docs/frontend/`.

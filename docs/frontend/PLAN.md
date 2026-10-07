# Plan técnico del frontend Angular

Requisitos: `docs/frontend/ESPECIFICACION.md`. Pruebas: `docs/frontend/PLAN_PRUEBAS.md`.
Tareas: `docs/frontend/TAREAS.md`. La API que se consume está descrita en `docs/PLAN.md`, «API».

## Modelo de proceso
El mismo del backend: **modelo en V con implementación incremental**. Los requisitos son fijos
(paridad con `RRHH.Web`) y el riesgo principal es entregar algo incompleto en un plazo corto. Cada
nivel de diseño tiene su nivel de prueba (`docs/frontend/PLAN_PRUEBAS.md`), y la implementación
avanza entidad por entidad, cada una terminada y probada, para que lo entregado funcione en
cualquier punto.

Diferencia con el backend: la mayor parte de la lógica del frontend es **genérica** (cliente HTTP,
listado paginado, cascada, errores de formulario). Se especifica, prueba e implementa una vez, en la
fase 10; después, cada pantalla es una composición delgada de esas piezas.

## Ubicación
Carpeta `frontend/` en la raíz del repositorio, fuera de `src/` (que es la solución .NET). La
solución `RRHH.slnx` no lo incluye; tiene su propio `package.json` y su propio job en la CI.

## Arquitectura
La arquitectura limpia liviana del backend, trasladada al frontend: capas con dependencias hacia
adentro y la implementación del cliente registrada en la raíz de composición, con la inyección de
dependencias de Angular.

```
vistas/ ──► servicios/ ──► contratos/ ◄── api/
   │              ▲                         ▲
   └──► componentes/       app.config.ts ───┘  (registra ClienteRrhhHttp como CLIENTE_RRHH)
```

| Capa | Equivale en el backend | Responsabilidad |
|---|---|---|
| `contratos/` | `RRHH.Contratos` | Tipos de los DTOs, `Pagina`, `Consulta`, `ErrorApi`, interfaz `ClienteRrhh` e interfaces de la lógica de pantallas. Sin lógica |
| `api/` | `ClienteRrhh` de `RRHH.Web` | `ClienteRrhhHttp`, que implementa `ClienteRrhh` con `HttpClient`, y el interceptor que traduce los errores a `ErrorApi` |
| `servicios/` | `RRHH.Application` | Lógica de las pantallas: listado, cascada, formulario, eliminación, formatos. Estado con signals. Declara los tokens `CLIENTE_RRHH`, `AVISOS` y `CONFIRMACION` |
| `vistas/`, `componentes/` | Razor Pages | Componentes standalone con Angular Material. Usan `servicios/`; nunca `api/` ni `HttpClient`. `componentes/` tiene además los adaptadores de Material para `Avisos` y `Confirmacion` |
| `app.config.ts` | `Program.cs` | Raíz de composición: router, `HttpClient` con el interceptor, la URL de la API y la implementación de cada token (`ClienteRrhhHttp`, `AvisosMaterial`, `ConfirmacionMaterial`) |

Los tokens (`InjectionToken`) viven en `servicios/` y no en `contratos/`, para que `contratos/` no
dependa de Angular. La lógica pide `inject(CLIENTE_RRHH)`, `inject(AVISOS)` e
`inject(CONFIRMACION)`, igual que un servicio inyectado por su interfaz en .NET: no conoce ni el
`HttpClient` ni la biblioteca de componentes (E-021). Así las pruebas de `servicios/` usan dobles, y
las de vistas, el cliente real con `HttpTestingController` en lugar de la red.

**Reglas de dependencia** (se verifican con ESLint, ver «Pruebas de arquitectura» en el plan de
pruebas):
- **ARQF1.** `contratos/` no importa nada del proyecto; de bibliotecas, sólo tipos (`import type`)
  de `rxjs`, `@angular/core` y `@angular/forms` (`Observable`, `Signal`, `FormGroup`).
- **ARQF2.** `api/` sólo importa `contratos/`. Es la única carpeta que usa `HttpClient`
  (`@angular/common/http`), además de `app.config.ts`, que lo registra. Nadie usa `fetch`.
- **ARQF3.** `vistas/`, `componentes/` y `servicios/` no importan `api/` ni `environments/`; sólo
  `app.config.ts` lo hace.

## Estructura
```
frontend/
  package.json, package-lock.json, angular.json, tsconfig*.json, eslint.config.js, .prettierrc,
  .prettierignore
  src/
    index.html, main.ts, styles.css
    environments/
      environment.ts              urlApi del build de producción (se configura al desplegar)
      environment.development.ts  urlApi: 'http://localhost:5279' (ng serve)
    app/
      app.config.ts         raíz de composición
      app.routes.ts         rutas de la especificación, «Contrato de interfaz»
      app.ts, app.html      layout: menú (RF1) y <router-outlet>
      contratos/
        dtos.ts             un tipo por record de RRHH.Contratos, con el mismo nombre
        errores.ts          ProblemDetails y ErrorApi
        cliente.ts          interfaz ClienteRrhh
        pantallas.ts        interfaces de listado, cascada, formulario, eliminación, avisos y
                            confirmación (contrato para el tester)
      api/
        cliente-rrhh-http.ts     ClienteRrhhHttp: URL base, parámetros, cuerpo JSON
        errores.interceptor.ts   HttpErrorResponse → ErrorApi, con las claves normalizadas
        url-api.ts               token URL_API
      servicios/
        cliente.ts          token CLIENTE_RRHH
        avisos.ts           token AVISOS
        confirmacion.ts     token CONFIRMACION
        listado.ts          RF2
        cascada.ts          RN6, RF11, RF12
        formulario.ts       RF5 a RF8, VC1 a VC5
        eliminacion.ts      RF3, RF4
        formatos.ts         RNF2: fechas y regla del 29 de febrero; validadores y textos de VC
        mantenimientos.ts   RF1: los mantenimientos del menú y del inicio
      componentes/
        avisos-material.ts       Avisos con MatSnackBar
        confirmacion-material.ts Confirmacion con MatDialog (y dialogo-confirmacion.ts)
        listado-paginado.ts      alrededor de la tabla de cada vista: «Buscar», total, carga,
                                 error y mat-paginator (RF2)
      vistas/
        inicio.ts
        pendiente.ts        lugar de las pantallas que todavía no están (tareas 38 a 42)
        paises/             paises-listado.ts, pais-formulario.ts
        departamentos/      departamentos-listado.ts, departamento-formulario.ts
        municipios/         municipios-listado.ts, municipio-formulario.ts
        empresas/           empresas-listado.ts, empresa-formulario.ts, empresa-colaboradores.ts
        colaboradores/      colaboradores-listado.ts, colaborador-alta.ts, colaborador-editar.ts,
                            colaborador-detalle.ts
  tests/
    unitarias/              api/ (HttpTestingController) y servicios/ (cliente falso)
    componentes/            vistas con @testing-library/angular y HttpTestingController
    apoyo/                  respuestas de ejemplo y ayudas que simulan la API según docs/PLAN.md, «API»
```

Cada componente tiene su `.ts` y su `.html`; los nombres de archivo siguen el estilo actual de
Angular, sin el sufijo `.component`. Un formulario sirve para crear y editar (según haya `:id` en la
ruta), salvo el colaborador: el alta incluye las empresas y la edición no (como en la API).

## Tecnologías
| Uso | Elección | Por qué |
|---|---|---|
| Marco | Angular 22 (componentes standalone, signals, control de flujo `@if`/`@for`) + TypeScript estricto | Es lo que el responsable mejor revisa (README, «Frontend Angular») |
| Compilación | Angular CLI (`ng serve`, `ng build`) | Estándar de Angular |
| Rutas | Angular Router | Rutas de la especificación |
| HTTP | `HttpClient` con un interceptor funcional | Traducción de errores en un solo lugar |
| Formularios | Formularios reactivos tipados | Validadores de VC; errores de la API en cada control con `setErrors` |
| Componentes | Angular Material (tema `azure-blue`) | `mat-table` y `mat-paginator`, `mat-select`, `mat-form-field`, `MatSnackBar` y `MatDialog`, de Google, con la misma versión que Angular y licencia MIT. Se descartó PrimeNG, que ahora es comercial (E-021) |
| Fechas | `<input matInput type="date">` | Su valor ya es `aaaa-mm-dd`, el formato de la API: sin conversiones ni riesgo de correr un día; `max` cumple VC5 |
| Estado | Signals en cada pantalla; sin store global (sin NgRx) | El estado es de cada pantalla; un store no aporta nada aquí |
| Pruebas | Vitest (el ejecutor de `ng test`), @testing-library/angular, user-event, `HttpTestingController` | Ver plan de pruebas |
| Calidad | angular-eslint (trae typescript-eslint), Prettier | RNF6 y reglas ARQF: `@typescript-eslint/no-restricted-imports` por carpeta, con `allowTypeImports` para ARQF1. `eslint-plugin-import` no es compatible con ESLint 10 |

Node.js ya está en el entorno (`docs/ENTORNO.md`, por Newman). `.editorconfig` de la raíz agrega
`[*.{ts,html,css,js,mjs}]` con `indent_size = 2`, y se borra el que genera el CLI dentro de
`frontend/`; Prettier lo respeta: el mismo estilo en todo el repositorio, como hace `dotnet format`
en .NET.

Versiones: las estables al crear el proyecto (hoy, Angular y Angular Material 22.2), fijadas en
`package-lock.json` (versionado). La CI instala con `npm ci`, que falla si el lock no coincide, como
`--locked-mode` en .NET. Node.js: Angular 22 pide `^22.22.3 || ^24.15.0`; se usa la LTS 24, indicada
en `package.json` (`engines`) y en la CI. Agregar una dependencia se avisa y se justifica, como con
los paquetes NuGet.

El Angular CLI **no se instala global**: viene como dependencia del proyecto. Se usa
`npx @angular/cli@22 new` una sola vez y, después, los scripts de npm. Instalado global quedaría en
`%APPDATA%\npm`, que Claude Code no comparte con el responsable (E-020).

Scripts de `package.json`: `start` (`ng serve`), `build` (`ng build`), `test`
(`ng test --watch=false`), `lint` (`ng lint`), `format` (`prettier --write .`) y `format:check`
(`prettier --check .`).

## Licencias
Las dependencias de terceros, con la licencia que declaran (E-021: se revisa al empezar, al
agregar una dependencia y al cambiar de versión mayor). Todas permiten usar y distribuir el
proyecto.

| Dependencia | Licencia |
|---|---|
| Angular (`@angular/*`, incluidos Material, CDK, CLI y build) | MIT |
| RxJS · TypeScript | Apache-2.0 |
| tslib | 0BSD |
| ESLint, typescript-eslint, angular-eslint, Prettier | MIT |
| Vitest, jsdom, Testing Library (`@testing-library/*`) | MIT |
| Backend: paquetes de Microsoft (ASP.NET Core, EF Core), coverlet | MIT |
| Backend: xunit.v3 · Newman (Postman) | Apache-2.0 |
| Backend: NetArchTest.Rules | MIT (lo declara su repositorio, no el paquete) |

## Contrato del cliente
Los tipos de `contratos/dtos.ts` se escriben a mano, uno por record de `RRHH.Contratos`, con un
comentario que nombra el archivo de origen. Las propiedades van en camelCase, como las serializa
la API. Las fechas son `string` (`aaaa-mm-dd`); los enums, uniones de texto
(`'VeintiochoDeFebrero' | 'PrimeroDeMarzo'`). Generarlos desde OpenAPI queda como mejora futura.

```ts
interface Mantenimiento<TDto, TGuardar> {
  listar(consulta: Consulta): Observable<Pagina<TDto>>
  obtener(id: number): Observable<TDto>
  crear(datos: TGuardar): Observable<TDto>                  // 201
  actualizar(id: number, datos: TGuardar): Observable<TDto> // 200
  eliminar(id: number): Observable<void>                    // 204
}

interface ClienteRrhh {
  paises: Mantenimiento<PaisDto, GuardarPaisDto> & {
    departamentos(paisId: number): Observable<DepartamentoDto[]> }
  departamentos: Mantenimiento<DepartamentoDto, GuardarDepartamentoDto> & {
    municipios(departamentoId: number): Observable<MunicipioDto[]> }
  municipios: Mantenimiento<MunicipioDto, GuardarMunicipioDto>
  empresas: Mantenimiento<EmpresaDto, GuardarEmpresaDto> & {
    colaboradores(empresaId: number, consulta: Consulta): Observable<Pagina<ColaboradorDto>> }
  colaboradores: Omit<Mantenimiento<ColaboradorDto, GuardarColaboradorDto>, 'crear'> & {
    crear(datos: CrearColaboradorDto): Observable<ColaboradorDto>
    asociarEmpresa(id: number, datos: AsociarEmpresaDto): Observable<ColaboradorDto>
    actualizarEmpresa(id: number, empresaId: number, datos: GuardarEmpresaColaboradorDto): Observable<ColaboradorDto>
    quitarEmpresa(id: number, empresaId: number): Observable<void> }
}
```

Mientras no esté implementado, cada método del cliente y cada función de `servicios/` lanza
`new Error('No implementado')`, el equivalente de `NotImplementedException`: así las pruebas del
tester compilan y quedan en rojo por el motivo correcto (E-014).

Las listas para elegir (países, empresas) piden la primera página con `tamanio=100`, el máximo de
la API. Límite conocido: con más de 100 países o empresas, no aparecerían todos; una búsqueda
dentro de la lista queda como mejora futura.

## Errores
`api/errores.interceptor.ts` convierte toda respuesta no exitosa en un `ErrorApi`:

| Respuesta | `ErrorApi` | Dónde se muestra |
|---|---|---|
| 400 con `errors` | `estado 400`, `errores` con las claves normalizadas | Debajo de cada campo (RF5); claves sin campo, aviso general |
| 400 de JSON ilegible (claves `$.campo` y `dto`) | `estado 400`; `dto` se descarta; `$.campo` lleva «El valor no es válido.» | Debajo del campo (RF5); nunca el texto de la API |
| 404 | `estado 404`, `detalle` | Pantalla con «El registro no existe.» (RF7) |
| 409 | `estado 409`, `detalle` | Aviso general del formulario o de la acción (RF6, RF3) |
| 500 u otra | `estado` recibido | «Ocurrió un error inesperado.» (RF8) |
| Sin respuesta (`HttpErrorResponse` con `status 0`) | `estado 0` | «No se pudo conectar con la API.» (RF8) |

Normalización de claves (RF5): la API usa el nombre de la propiedad en PascalCase. Cada segmento
pasa a camelCase y se conservan los índices: `Empresas[0].FechaIngreso` → `empresas[0].fechaIngreso`.
El prefijo `$.` (errores de lectura del JSON) se quita, y el mensaje se reemplaza como indica la
tabla.

## Lógica de las pantallas
Las firmas exactas quedan en `contratos/pantallas.ts` en la tarea 35. Son funciones que se llaman
al inicializar un componente (usan `inject()`) y devuelven su estado en signals:

- **`crearListado(cargar, opciones)`**: `elementos`, `total`, `pagina`, `tamanio`, `buscar`,
  `cargando`, `error`, `recargar()`. Cambiar `buscar` espera 300 ms sin cambios (`debounceTime`),
  vuelve a la página 1 y consulta. Una respuesta vieja que llega después de una nueva se descarta
  (`switchMap`).
- **`crearCascada(inicial?)`**: listas y selección de país, departamento y municipio. Cambiar el
  país vacía departamento y municipio y carga los departamentos; cambiar el departamento vacía el
  municipio y carga los municipios. Con `inicial` (editar) carga las tres listas sin vaciar la
  selección.
- **`crearFormulario(grupo, guardar, opciones)`**: `errorGeneral`, `guardando`, `enviar()`.
  `enviar` marca los controles y, si el grupo es inválido (VC), no llama a la API. Al guardar,
  avisa «Se guardó correctamente.» y navega a `opciones.volverA` (RF4). Un `ErrorApi` 400 pone cada
  mensaje en su control (`setErrors({ api: mensaje })`), también dentro de un `FormArray`
  (`empresas[1].fechaIngreso`); las claves sin control y cualquier otro error van a `errorGeneral`.
- **`crearEliminacion(eliminar, alTerminar)`**: confirma (`CONFIRMACION`), elimina, avisa
  (`AVISOS`) y recarga; un 409 se muestra como aviso con el `detalle`. En las pruebas, los dos
  tokens se reemplazan por dobles.
- **`Avisos` y `Confirmacion`**: interfaces propias, para que la lógica no dependa de la biblioteca
  de componentes (E-021). Las implementan `AvisosMaterial` (snackbar) y `ConfirmacionMaterial`
  (diálogo con «Sí, eliminar» y «Cancelar»), en `componentes/`. Como Material abre los dos en su
  propia capa, sobre el `body`, ni el layout ni las vistas los incluyen en la plantilla.

## Cambio en la API (CORS1)
En `src/RRHH.Api/Program.cs`:

```csharp
var origenes = builder.Configuration.GetSection("Cors:OrigenesPermitidos").Get<string[]>() ?? [];
builder.Services.AddCors(o => o.AddDefaultPolicy(p => p
    .WithOrigins(origenes)
    .WithMethods("GET", "POST", "PUT", "DELETE")
    .WithHeaders("Content-Type")
    .WithExposedHeaders("Location")));
// ...
app.UseCors();            // antes de UseAuthorization y MapControllers
```

`appsettings.Development.json`: `"Cors": { "OrigenesPermitidos": [ "http://localhost:4200" ] }`.
En `appsettings.json` la lista queda vacía: en otros entornos se configura explícitamente.

**Puerto:** `ng serve` usa el puerto fijado en `angular.json` (`serve.options.port: 4200`), que es
el origen permitido. Si se cambia uno, se cambia el otro: con otro puerto, el navegador rechaza
todas las respuestas por CORS.

**HTTPS:** el frontend usa el perfil `http` de la API (`http://localhost:5279`, el mismo del entorno
de Postman). Si la API corre con el perfil `https`, `urlApi` tiene que ser
`https://localhost:7279`: `UseHttpsRedirection` respondería la consulta previa (preflight) de CORS
con una redirección, y el navegador la rechaza.

## Agentes
Se reutiliza el esquema de `docs/AGENTES.md` sin cambiar el principio: el tester y el revisor usan
un modelo de otro proveedor, y las pruebas se escriben desde la especificación antes de la
implementación. Modelos: Gemini 3.1 Pro; si se agota su cuota, GPT-OSS 120B (Medium), como en la
fase 5. Los dos corren en `agy`, sin otra suscripción.

El rol del implementador se amplía en lugar de crear otro agente: sería el mismo modelo, trabajando
en secuencia, y la independencia que importa es la del tester. Lo que cambia entre backend y
frontend son las convenciones, y eso va en `docs/frontend/CODIFICACION.md` (tarea 29b).

Ajustes:

| Qué | Cambio | Tarea |
|---|---|---|
| `AGENTS.md` («Roles») y `docs/agentes/implementador.md` | El implementador modifica `src/` y `frontend/`, salvo `frontend/tests/` | 29 |
| `.claude/settings.json` | Agregar `Edit(/frontend/tests/**)` a las denegaciones del implementador | 33 |
| Hook `permisos-por-rol.mjs`, escritura del tester | Agregar `frontend/tests/` y `docs/frontend/PLAN_PRUEBAS.md` | 33 |
| Hook, lectura bloqueada del tester | Agregar `frontend/src/app/api/`, `servicios/`, `vistas/` y `componentes/`. Lee `contratos/` y `app.routes.ts` | 33 |
| Hook, comandos del tester sin aprobación | `npm test`, `npm run lint`, `npm run build`, `npm test -- --include <archivo>` | 33 |
| Hook, comandos con aprobación | `npm install` y `npm ci` (el tester no agrega dependencias) | 33 |
| `docs/agentes/tester.md` | Fuentes del frontend: especificación, plan de pruebas, `contratos/` y la API de `docs/PLAN.md` | 33 |
| `docs/agentes/revisor.md` | Tres verificaciones más: errores de la API visibles; sin reglas de negocio duplicadas en el navegador; sin URLs ni secretos en el código | 33 |

La prueba trivial de la tarea 31 la escribe el implementador al crear el proyecto, antes de que
rijan los permisos del frontend: es la única excepción.

Las pruebas del frontend no necesitan la API (usan `HttpTestingController`), así que corren en el
entorno aislado de `agy` sin el problema de E-017. La lista de aceptación sí la necesita: la API la
levanta el responsable en su terminal (`--launch-profile http`), no un agente (E-017, E-020).

Los intérpretes siguen bloqueados para el tester (E-015): `ng test` corre sobre Node, pero a través
de los scripts de `package.json`, que el tester no puede modificar.

## Integración continua
Job nuevo en `.github/workflows/ci.yml`, «Frontend: lint, pruebas y build», con
`working-directory: frontend`: `actions/setup-node` con caché de npm, `npm ci`, `npm run lint`,
`npm run format:check`, `npm test` y `npm run build`. Se agrega como check obligatorio en el ruleset
de `main` (lo hace el responsable). Las acciones se fijan por SHA, como las existentes.

## Repositorio y ramas
Mismas reglas de `docs/PLAN.md`: una rama por fase, commits pequeños con Conventional Commits y
ámbito `frontend` (`feat(frontend): ...`), PR a `main` con la plantilla, merge commit. Al terminar:
tag `v1.1.0`.

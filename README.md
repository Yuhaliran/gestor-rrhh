# Plataforma de Recursos Humanos

Gestión de colaboradores de varias empresas ubicadas en distintos países: mantenimiento de la
geografía (país, departamento, municipio), de las empresas y de sus colaboradores, con la edad
calculada según la legislación de cada país.

.NET 10 · ASP.NET Core (API REST y Razor Pages) · Entity Framework Core · SQL Server · xUnit · Postman

## Cómo ejecutar

### Requisitos
- .NET 10 SDK (`global.json` fija la versión 10).
- SQL Server Express o LocalDB. No hace falta SSMS: las migraciones crean la base.
- Para las pruebas de aceptación: Node.js y `npm install -g newman`.

Preparación detallada del entorno en Windows: `docs/ENTORNO.md`. Los agentes de IA no hacen falta
para ejecutar ni probar la aplicación; para trabajar con ellos, ver «Uso de IA».

### Primera vez
Desde la carpeta del repositorio:
```powershell
dotnet tool restore     # dotnet-ef con la versión fijada en dotnet-tools.json
dotnet build            # restaura los paquetes; dotnet ef lo necesita
dotnet user-secrets set "ConnectionStrings:Rrhh" "Server=(localdb)\MSSQLLocalDB;Database=Rrhh;Trusted_Connection=True;TrustServerCertificate=True" --project src/RRHH.Api
dotnet ef database update -p src/RRHH.Infrastructure -s src/RRHH.Api
```
La cadena de conexión queda en user-secrets, fuera del repositorio. `database update` crea la base
`Rrhh` con las tablas y los datos iniciales: Guatemala, sus 22 departamentos y la cabecera de cada
uno.

### Levantar la aplicación
En dos terminales:
```powershell
dotnet run --project src/RRHH.Api --launch-profile http    # API en http://localhost:5279
dotnet run --project src/RRHH.Web --launch-profile http    # web en http://localhost:5177
```
- La web habla con la API por HTTP, en la dirección de `Api:UrlBase` (`src/RRHH.Web/appsettings.json`).
  Si la API no está corriendo, la web lo indica en lugar de fallar.
- `GET /health` verifica que la API responde y llega a la base. Si responde 503, la API no llega a
  la base de su cadena de conexión: ver `docs/ERRORES_RECURRENTES.md`, E-020.
- En desarrollo, la especificación OpenAPI está en `http://localhost:5279/openapi/v1.json`.

## Pruebas
Modelo en V: cada nivel de la especificación tiene su nivel de prueba. Detalle y tabla de
trazabilidad (cada regla con sus pruebas) en `docs/PLAN_PRUEBAS.md`.

| Nivel | Proyecto | Pruebas | Comando | Requiere |
|---|---|---|---|---|
| Unitarias | `tests/RRHH.UnitTests` | 293 | `dotnet test --filter "Categoria!=E2E&Categoria!=SqlServer"` (las tres juntas) | Nada |
| Integración | `tests/RRHH.IntegrationTests` | 84 | (ídem) | Nada |
| Arquitectura | `tests/RRHH.ArchitectureTests` | 4 | (ídem) | Nada |
| Migraciones (MIG2) | `tests/RRHH.IntegrationTests` | 1 | `dotnet test --filter "Categoria=SqlServer"` | LocalDB |
| Aceptación | `postman/` | 125 pedidos, 198 verificaciones | `newman run postman/RRHH.postman_collection.json -e postman/local.postman_environment.json` | API corriendo |

- **Unitarias:** cálculo de edad con sus casos límite (29 de febrero, cumpleaños hoy, mañana y
  ayer), validaciones de los DTOs, servicios contra SQLite en memoria y restricciones de la base.
- **Integración:** la API completa con `WebApplicationFactory`, SQLite en memoria y un reloj fijo:
  códigos, `Location`, ProblemDetails, paginación y búsqueda de cada endpoint.
- **Arquitectura:** las dependencias entre capas (NetArchTest).
- **Migraciones:** MIG1 (`dotnet ef migrations has-pending-model-changes`) corre en la integración
  continua; MIG2 aplica las migraciones reales sobre una base nueva de SQL Server y verifica tablas,
  datos iniciales e intercalación.
- **Aceptación:** la colección de Postman cubre cada endpoint, con pruebas en cada pedido. Cada
  carpeta crea y borra sus datos, así que se puede correr varias veces seguidas.

**Las pruebas, probadas.** Además de quedar en verde, se comprobó que fallen cuando el código está
mal: el implementador modificó a propósito servicios, controladores y migraciones (pruebas de
mutación) y verificó que alguna prueba lo detecte. Así aparecieron pruebas que pasaban sin
verificar la regla, por ejemplo una búsqueda que devolvía el dato esperado aunque la API ignorara
el filtro (`docs/ERRORES_RECURRENTES.md`, E-018); se corrigieron hasta detectar cada mutación.

**Integración continua** (`.github/workflows/ci.yml`), en cada pull request: restauración con
`--locked-mode`, compilación sin advertencias, MIG1, `dotnet format --verify-no-changes` y pruebas.

## Arquitectura
Arquitectura limpia liviana, con las dependencias hacia adentro:
```
RRHH.Web (Razor Pages) ──HTTP──► RRHH.Api ──► RRHH.Application ──► RRHH.Domain
        │                            │                ▲
        └──────► RRHH.Contratos ◄────┴──► RRHH.Infrastructure (EF Core, SQL Server)
```
| Proyecto | Responsabilidad |
|---|---|
| `RRHH.Contratos` | DTOs y paginación, compartidos por la API y la web; sin dependencias |
| `RRHH.Domain` | Entidades y reglas propias (cálculo de edad) |
| `RRHH.Application` | Servicios con las reglas de negocio, sobre una interfaz del contexto de datos |
| `RRHH.Infrastructure` | `RrhhDbContext`, configuraciones de EF Core, migraciones y datos iniciales |
| `RRHH.Api` | Endpoints REST y errores con ProblemDetails |
| `RRHH.Web` | Razor Pages que consumen la API con un `HttpClient` tipado; sólo conoce los contratos |

Modelo de datos, endpoints y convenciones: `docs/PLAN.md` y `docs/CODIFICACION.md`.

## Decisiones de diseño
- **Edad calculada, no guardada.** Se guarda la fecha de nacimiento: una edad guardada queda
  desactualizada. La fecha actual la da `TimeProvider`, inyectado, para que las pruebas usen un
  reloj fijo.
- **Legislación por país.** El rango de edad laboral (RN4) y el cumpleaños de los nacidos el 29 de
  febrero en años no bisiestos (28 de febrero o 1 de marzo, RN7) son datos de cada país, con
  valores por defecto. La edad que se muestra usa la regla del país de la empresa más antigua del
  colaborador.
- **Geografía normalizada.** La empresa guarda sólo su municipio, para que país, departamento y
  municipio nunca queden inconsistentes. Al editar, un departamento o un municipio no cambia de
  padre, y una empresa sólo se muda de municipio dentro de su país (RN8).
- **NIT único por país.** Cada país emite sus identificadores tributarios: el servicio valida la
  unicidad con el país del municipio. Límite conocido: la base no lo garantiza ante dos altas
  simultáneas (ver «Mejoras futuras»).
- **Colaboradores en varias empresas.** Relación muchos a muchos con fecha de ingreso y puesto. Un
  colaborador siempre tiene al menos una empresa (RN3): se eligen al crearlo y después se asocian,
  editan o quitan con endpoints propios.
- **Duplicados sin distinguir mayúsculas, pero sí tildes** («Petén» ≠ «Peten»). La intercalación
  `Modern_Spanish_CI_AS` hace que los índices únicos de la base cumplan la misma regla que los
  servicios.
- **Errores con ProblemDetails.** 400 para toda validación (del DTO o de una regla sobre un campo,
  con el error en ese campo), 404 si no existe, 409 por reglas de negocio o un duplicado. Un error
  de la base por un único o una clave foránea también es 409; uno no previsto, 500 sin detalles
  internos.
- **Sin repositorio genérico.** `DbContext` ya es una unidad de trabajo y `DbSet` un repositorio;
  los servicios los usan a través de una interfaz, para poder probarlos.
- **Eliminación física protegida.** No se borra lo que tiene dependencias (RN1, RN2). El borrado
  lógico abriría casos que la evaluación no pide.
- **Pruebas sin instalar nada.** Unitarias e integración usan SQLite en memoria; las migraciones
  reales se verifican aparte contra SQL Server (MIG2).
- **Web sin lógica de negocio.** Todo pasa por la API. Las listas en cascada se piden a la web, que
  las pide a la API, así el navegador no necesita conocerla. Los errores de la API se muestran en
  su campo del formulario.

Requisitos interpretados y su justificación: `docs/ESPECIFICACION.md`, «Decisiones sobre los
requisitos».

## Estructura del repositorio

### Código y pruebas
| Ruta | Qué contiene |
|---|---|
| `src/` | Los seis proyectos de la aplicación (ver «Arquitectura») |
| `tests/` | Pruebas unitarias, de integración y de arquitectura (ver «Pruebas») |
| `postman/` | Colección de Postman con todos los endpoints y sus pruebas, y el entorno local (`baseUrl`). Para usarla en Postman, importar los dos archivos |
| `RRHH.slnx` | La solución, con todos los proyectos |

### Documentación (`docs/`)
| Documento | Qué contiene y para qué leerlo |
|---|---|
| `ESPECIFICACION.md` | **Qué hace el sistema.** Entidades y sus campos, reglas de negocio (RN1–RN9), validaciones (V1–V5) y criterios de aceptación (CA1–CA8). Incluye las decisiones tomadas donde el enunciado dejaba dudas, con su justificación |
| `PLAN.md` | **Cómo está construido.** Arquitectura, modelo de datos con su diagrama, datos iniciales, endpoints de la API con sus respuestas, base de datos y ramas |
| `PLAN_PRUEBAS.md` | **Cómo se prueba.** Modelo en V, qué cubre cada nivel y la tabla de trazabilidad: qué pruebas verifican cada regla y criterio |
| `CODIFICACION.md` | **Cómo se escribe el código.** Convenciones y un patrón de referencia completo (países) que siguen todas las entidades |
| `TAREAS.md` | **En qué orden se hizo.** Las tareas en 9 fases, cada fase en su rama y con su pull request |
| `AGENTES.md` | **Cómo se trabajó con IA.** Los roles (implementador, tester, revisor y responsable), por qué con modelos de distintos proveedores, el ciclo de cada tarea, cómo se imponen los permisos de cada agente y qué necesita otra herramienta para tomar un rol |
| `agentes/` | Las instrucciones de cada rol, que el agente lee al empezar |
| `PROMPTS.md` | Los mensajes que se usaron con los agentes en cada paso del ciclo |
| `ERRORES_RECURRENTES.md` | Los problemas encontrados durante el desarrollo (de herramientas, de las pruebas y de los agentes), su causa y cómo evitarlos |
| `ENTORNO.md` | Instalación del entorno en Windows, paso a paso, incluidos los agentes |

### Configuración
| Archivo | Qué contiene |
|---|---|
| `AGENTS.md`, `CLAUDE.md` | Instrucciones que los agentes de IA leen al abrir el repositorio: proyecto, comandos, convenciones y lo que no deben hacer. `CLAUDE.md` las importa para Claude Code |
| `.claude/settings.json` | Permisos del implementador en Claude Code: no puede editar las pruebas ni hacer `git push` |
| `.agents/` | Hook de permisos del tester y del revisor en Antigravity CLI: qué puede leer, escribir y ejecutar cada uno |
| `.github/` | Integración continua (`workflows/ci.yml`) y plantilla de los pull requests |
| `global.json` | Versión del SDK de .NET (10) y uso de Microsoft.Testing.Platform para las pruebas |
| `Directory.Build.props` | Configuración común de todos los proyectos: .NET 10, advertencias como errores y versiones de paquetes fijadas (lock files) |
| `.editorconfig` | Estilo de código, que verifica `dotnet format` |
| `dotnet-tools.json` | Versión fijada de `dotnet-ef`, que instala `dotnet tool restore` |

## Uso de IA
El proyecto se desarrolló con agentes de IA en roles separados y modelos de distintos proveedores,
con una persona responsable de cada decisión (detalle en `docs/AGENTES.md`).

| Rol | Herramienta y modelo | Qué hizo |
|---|---|---|
| Implementador | Claude Code (Claude, de Anthropic) | Propuso cada contrato (interfaz y DTOs), implementó `src/` y verificó las pruebas con mutaciones |
| Tester | Antigravity CLI con Gemini 3.1 Pro (Google) | Escribió las pruebas desde la especificación y el contrato, antes de la implementación y sin leerla |
| Revisor | Antigravity CLI, en una sesión nueva: Gemini 3.1 Pro; GPT-OSS 120B (OpenAI) en la fase 5 | Revisó cada fase contra la especificación, sin modificar nada |
| Responsable | Persona | Decidió el diseño, aprobó los contratos, resolvió las discrepancias y revisó el código |

**Por qué proveedores distintos.** Si el mismo modelo escribe el código y sus pruebas, tiende a
interpretar el requisito igual en los dos, y las pruebas pasan porque comparten el error. Por eso
el tester y el revisor usan modelos de otro proveedor que el implementador. Cuando Gemini llegó a su
límite de uso:
- Se descartó otro modelo de Anthropic (Claude Opus) para el tester, porque es el mismo proveedor
  que el implementador y se perdía esa independencia.
- La revisión de la fase 5 (la web) se hizo con GPT-OSS 120B, de OpenAI.
- En la fase 6, GPT-OSS 120B también se probó como tester, pero cortaba sin editar la colección de
  Postman (un archivo de unas 2000 líneas); la tarea se hizo con Gemini cuando recuperó su cuota.

Para reproducir el flujo con agentes: instalación y primer inicio en `docs/ENTORNO.md` («Agentes»),
roles y permisos en `docs/AGENTES.md`, y los mensajes de cada paso en `docs/PROMPTS.md`.

**Ciclo de cada tarea de la API:** contrato (aprobado por el responsable) → pruebas en rojo
(tester) → implementación hasta el verde, sin tocar las pruebas → revisión → decisión del
responsable. Cada paso es un commit que indica qué rol lo hizo; cada fase cierra con un pull
request con los hallazgos de la revisión y sus decisiones.

**Permisos impuestos, no sólo pedidos.** Claude Code no puede editar `tests/` ni `postman/` ni
hacer `git push` (`.claude/settings.json`). Un hook de Antigravity (`.agents/hooks/permisos-por-rol.mjs`)
limita al tester a escribir en sus carpetas, le impide leer la implementación de los servicios y
bloquea los intérpretes de scripts; el revisor sólo puede leer.

**Qué funcionó.**
- Las pruebas de caja negra sacaron a la luz ambigüedades de la especificación antes de
  implementar. Por ejemplo, el orden de los errores al crear un colaborador (referencia
  inexistente, edad, fecha de ingreso) o qué pasa al mudar una empresa de país; el responsable
  decidió y se actualizó la especificación (RN8 ampliada, RN9 nueva).
- Las pruebas de mutación encontraron pruebas que no probaban lo que decían; el tester las corrigió
  hasta detectar cada mutación.

**Qué hubo que corregir** (registrado en `docs/ERRORES_RECURRENTES.md`):
- Pruebas que pasaban sin verificar la regla, o con datos que no distinguían la respuesta correcta
  de la incorrecta (E-018).
- Informes del tester que daban por hechas verificaciones que no se habían hecho (E-014).
- El tester editaba archivos con scripts, a veces en base64, que se aprobaban sin poder leerlos y
  rompían pruebas (E-015): el hook pasó a bloquear los intérpretes.
- Desde el entorno aislado del tester no se llega a SQL Server: la API para Newman se levanta fuera
  de él (E-017).

**Hallazgos del revisor**, por ejemplo: comparar textos con `ToUpper` sobre las columnas impedía usar
los índices (aceptado: se resolvió con la intercalación de la base); la paginación inválida probada
sólo en una entidad de la colección de Postman (aceptado); un 404 que «faltaba» en Postman pero ya
existía (rechazado). Cada pull request de fase tiene la tabla completa.

## Mejoras futuras
- Autenticación y roles; auditoría de cambios; carga masiva de colaboradores.
- Borrado lógico (estado activo/inactivo, con historial de la relación laboral).
- Unicidad del NIT por país garantizada por la base, con una vista indexada de SQL Server.
- Pruebas E2E con Playwright de los flujos principales de la web.
- `docker-compose` con SQL Server, y la integración continua corriendo MIG2 y Newman contra él.

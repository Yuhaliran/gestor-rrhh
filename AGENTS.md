# AGENTS.md

Guía para agentes de IA (y personas) que trabajen en este repositorio.

## Proyecto
Plataforma web de Recursos Humanos para gestionar colaboradores de varias empresas,
ubicadas en distintos países. Evaluación técnica: se evalúa la calidad de la solución,
no sólo que funcione.

- Especificación:  `docs/ESPECIFICACION.md` (fuente de verdad de los requisitos)
- Plan técnico:    `docs/PLAN.md` (arquitectura, datos, API, repositorio y ramas)
- Plan de pruebas: `docs/PLAN_PRUEBAS.md` (modelo en V, niveles y trazabilidad)
- Tareas:          `docs/TAREAS.md` (orden de implementación, por fases y ramas)
- Agentes:         `docs/AGENTES.md` (roles implementador, tester y revisor; ciclo por tarea)
- Entorno:         `docs/ENTORNO.md` (software necesario y verificación)
- Codificación:    `docs/CODIFICACION.md` (convenciones y patrón de referencia; seguirlo en todas las entidades)
- Prompts:         `docs/PROMPTS.md` (mensajes para cada paso del ciclo)
- Errores:         `docs/ERRORES_RECURRENTES.md` (errores ya resueltos y cómo evitarlos; leer antes de cada tarea)

## Tecnologías
.NET 10 · ASP.NET Core (API + Razor Pages) · Entity Framework Core · SQL Server · xUnit · Git · Postman

## Estructura
```
src/
  RRHH.Contratos/       DTOs compartidos por la API y la web; sin dependencias
  RRHH.Domain/          entidades y reglas del dominio; no referencia a otros proyectos
  RRHH.Application/     DTOs, servicios, validaciones, interfaces
  RRHH.Infrastructure/  DbContext, configuraciones de EF, migraciones, datos iniciales
  RRHH.Api/             API REST
  RRHH.Web/             frontend Razor Pages; consume la API por HttpClient; sólo referencia a Contratos
tests/
  RRHH.UnitTests/
  RRHH.IntegrationTests/
  RRHH.ArchitectureTests/
  RRHH.E2ETests/        opcional, Playwright
postman/
docs/
```

## Comandos
- Compilar:            `dotnet build`
- Pruebas:             `dotnet test --filter "Categoria!=E2E&Categoria!=SqlServer"` (antes de cada commit)
- Pruebas E2E:         `dotnet test --filter "Categoria=E2E"` (con API y web corriendo)
- Postman:             `newman run postman/RRHH.postman_collection.json -e postman/local.postman_environment.json`
- Cambios de modelo sin migración: `dotnet ef migrations has-pending-model-changes -p src/RRHH.Infrastructure -s src/RRHH.Api`
- Formato:             `dotnet format`
- Nueva migración:     `dotnet ef migrations add <Nombre> -p src/RRHH.Infrastructure -s src/RRHH.Api`
- Aplicar migraciones: `dotnet ef database update -p src/RRHH.Infrastructure -s src/RRHH.Api`
- Correr la API:       `dotnet run --project src/RRHH.Api`
- Correr la web:       `dotnet run --project src/RRHH.Web`

## Convenciones
- Arquitectura limpia liviana: Domain no referencia a nadie; Api y Web no acceden al DbContext directamente.
- La API nunca expone entidades de EF: siempre DTOs.
- Errores con `ProblemDetails`: 400 validación, 404 no existe, 409 conflicto (duplicado o en uso).
- Consultas de lectura con `AsNoTracking()` y proyección a DTO.
- Nombres de código y de base en español, sin tildes (Pais, Municipio, Colaborador).
- Restricciones con nombre en la base (PK_, FK_, UQ_, IX_).
- Commits pequeños con Conventional Commits: `feat(empresas): ...`, `test(colaboradores): ...`.

## Ramas
- Trabajar en la rama de la fase actual (ver `docs/TAREAS.md`), nunca directo en `main`.
- Un commit por tarea terminada, con el mensaje sugerido en TAREAS.md.
- Cada fase se cierra con un pull request a `main` (plantilla en `.github/`); `main` está protegida.

## Roles
Este proyecto separa roles entre agentes de distintos modelos (ver `docs/AGENTES.md`):
- Implementador: sólo modifica `src/`.
- Tester: sólo modifica `tests/`, `postman/` y la tabla de trazabilidad de `docs/PLAN_PRUEBAS.md`.
- Revisor: no modifica nada.
- Implementador y tester pueden agregar entradas a `docs/ERRORES_RECURRENTES.md`.

## Reglas de trabajo
- Antes de implementar una tarea, leer los archivos relevantes y proponer un plan breve.
- Implementar una tarea de `docs/TAREAS.md` por vez; al terminar, correr `dotnet test`.
- Antes de empezar, revisar `docs/ERRORES_RECURRENTES.md`; al resolver un error que pueda
  repetirse, agregar una entrada en el mismo commit de la tarea.
- Cada tarea se entrega con sus pruebas, siguiendo `docs/PLAN_PRUEBAS.md`; las escribe el tester
  antes de la implementación (ver `docs/AGENTES.md`).
- Nombres de pruebas: `Metodo_Escenario_ResultadoEsperado`, estructura Arrange-Act-Assert.
- Cada regla nueva (RN, V) debe quedar en la tabla de trazabilidad con su prueba.
- Si un requisito es ambiguo, detenerse y preguntar; no inventar reglas de negocio.

## No hacer
- No modificar migraciones ya aplicadas: crear una nueva.
- No agregar paquetes NuGet sin avisar y justificar.
- No poner cadenas de conexión con contraseñas en el repositorio (usar user-secrets).
- No modificar pruebas para que pasen: si una prueba parece incorrecta, explicar por qué.
- No hacer `git push`.

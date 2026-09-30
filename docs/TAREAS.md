# Tareas

Una tarea por vez. Al terminar cada una: `dotnet build`, `dotnet test`, y un commit
con el mensaje sugerido. Al terminar cada fase: pull request de la rama a `main` con la
plantilla de `.github/`. `main` está protegida: no se hace merge local.

```
git switch main && git pull && git switch -c <rama-de-la-fase>
... tareas y commits ...
git push -u origin <rama-de-la-fase>        # lo hace el responsable
PR a main con la plantilla → revisión → merge commit (no squash)
git switch main && git pull
```

Convenciones y patrón de referencia: `docs/CODIFICACION.md`. Prompts de cada paso: `docs/PROMPTS.md`.

En las tareas de la API (9 a 14) se sigue el ciclo de `docs/AGENTES.md`:
contrato → pruebas (tester) → implementación (implementador) → revisión.
Desde la tarea 8, el tester agrega a `postman/` los requests de los endpoints nuevos; la tarea 22
completa la colección y la corre con Newman.
Cada paso es un commit propio; el merge se hace al cerrar la fase, con el PR.

## Fase 0 · rama `chore/estructura`
- [x] 1. Crear la solución, los proyectos de src/ y tests/ y sus referencias, Directory.Build.props
       y .editorconfig (ver docs/CODIFICACION.md); .gitignore de .NET
       (`chore: estructura inicial de la solución`)
- [x] 2. Agregar AGENTS.md, CLAUDE.md, README y docs/ (`docs: especificación, planes y tareas`)
- [x] 2b. Configurar permisos por rol: `.claude/settings.json` (implementador sin acceso a tests/)
        y permisos del tester en Antigravity CLI: sólo tests/, postman/, docs/PLAN_PRUEBAS.md
        y docs/ERRORES_RECURRENTES.md (`chore: permisos de los agentes por rol`)
- [x] 3. Pruebas de arquitectura ARQ1–ARQ4 (`test(arquitectura): reglas de dependencia entre capas`)
- [x] 3b. Integración continua con GitHub Actions: `dotnet build`, `dotnet test` y
        `dotnet format --verify-no-changes` en cada PR; exigirla en el ruleset de `main`
        (`ci: build, pruebas y formato en cada PR`)

## Fase 1 · rama `feature/dominio-datos`
- [x] 4. Entidades del dominio y cálculo de edad con la regla del 29 de febrero por país (RN7),
       con pruebas de valores límite
       (`feat(dominio): entidades y cálculo de edad`). Con la primera prueba unitaria, quitar el
       `--ignore-exit-code 8` de RRHH.UnitTests.csproj (E-002)
- [x] 5. RrhhDbContext y configuraciones con Fluent API (tipos, largos, únicos, FK con Restrict)
       (`feat(datos): contexto y configuraciones de EF Core`). Sus pruebas usan la base SQLite de la
       tarea 7, que el tester arma en el mismo paso
- [x] 6. Migración inicial y datos iniciales de Guatemala; verificar MIG1
       (`feat(datos): migración inicial y datos de Guatemala`)
- [x] 7. Infraestructura de pruebas: fábrica de contexto SQLite en memoria para las unitarias
       (`test: base de pruebas con SQLite en memoria`). Se hace dentro de la tarea 5

## Fase 2 · rama `feature/api-geografia`
- [x] 8. Manejo global de errores con ProblemDetails, paginación común y /health
       (`feat(api): errores, paginación y health check`)
- [x] 9. Países: servicio, endpoints y pruebas (`feat(api): mantenimiento de países`)
- [x] 10. Departamentos, con listado por país (`feat(api): mantenimiento de departamentos`)
- [x] 11. Municipios, con listado por departamento (`feat(api): mantenimiento de municipios`)

## Fase 3 · rama `feature/api-empresas-colaboradores`
- [x] 12. Empresas, con geografía completa en el detalle (`feat(api): mantenimiento de empresas`)
- [x] 13. Colaboradores, con edad calculada y al menos una empresa
        (`feat(api): mantenimiento de colaboradores`)
- [x] 14. Asociar y quitar empresas de un colaborador (`feat(api): empresas de un colaborador`)

## Fase 4 · rama `test/integracion`
- [x] 15. WebApplicationFactory con SQLite y pruebas de integración de los endpoints
        (`test(api): pruebas de integración`). Quitar el `--ignore-exit-code 8` de
        RRHH.IntegrationTests.csproj, si no se quitó antes (E-002)
- [x] 16. Revisar la tabla de trazabilidad de PLAN_PRUEBAS.md: cada regla con su prueba
        (`test: completar trazabilidad de reglas`)

## Fase 5 · rama `feature/web`
- [x] 17. Cliente HTTP tipado y layout base (`feat(web): cliente de la API y layout`)
- [x] 18. Mantenimientos de país, departamento y municipio (`feat(web): geografía`)
- [x] 19. Mantenimiento de empresas con listas en cascada (`feat(web): empresas`)
- [x] 20. Mantenimiento de colaboradores con selección de empresas (`feat(web): colaboradores`)
- [x] 21. Mensajes de error de la API en los formularios (`feat(web): validaciones`)

## Fase 6 · rama `docs/postman`
- [ ] 22. Colección y entorno de Postman con pruebas en cada request; correr con Newman
        (`docs(postman): colección con pruebas de aceptación`)

## Fase 7 (opcional) · rama `test/e2e`
- [ ] 23. Proyecto E2E con Playwright y los 3 flujos de PLAN_PRUEBAS.md, categoría E2E
        (`test(e2e): flujos principales con Playwright`)
- [ ] 24. Prueba MIG2 contra LocalDB, categoría SqlServer (`test(datos): migraciones sobre SQL Server`)
- [ ] 25. docker-compose.yml con SQL Server (`chore: docker-compose para SQL Server`)

## Fase 8 · rama `docs/entrega`
- [ ] 26. README final: ejecución, decisiones, pruebas por categoría y uso de IA (`docs: README`)
- [ ] 27. Revisión final: `dotnet format`, sin advertencias, sin secretos, todo en verde
        (`chore: revisión final`)
- [ ] 28. PR de la fase 8 a `main`; después, tag sobre `main`:
        `git tag -a v1.0.0 -m "Entrega de la evaluación"`

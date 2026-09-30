# Plataforma de Recursos Humanos

Gestión de colaboradores de varias empresas en distintos países.
.NET 10 · ASP.NET Core · Entity Framework Core · SQL Server · Razor Pages · xUnit

## Cómo ejecutar
1. Requisitos: .NET 10 SDK, SQL Server (Express o LocalDB), `dotnet tool install --global dotnet-ef`;
   para las pruebas de aceptación, Node.js y `npm install -g newman`
2. Configurar la cadena de conexión (ver docs/PLAN.md, sección Entorno)
3. `dotnet ef database update -p src/RRHH.Infrastructure -s src/RRHH.Api`
4. `dotnet run --project src/RRHH.Api` y `dotnet run --project src/RRHH.Web`

## Pruebas
Modelo en V; detalle y trazabilidad en `docs/PLAN_PRUEBAS.md`.

| Tipo | Comando | Requiere |
|---|---|---|
| Unitarias, integración y arquitectura | `dotnet test --filter "Categoria!=E2E&Categoria!=SqlServer"` | Nada |
| Aceptación de la API | `newman run postman/RRHH.postman_collection.json -e postman/local.postman_environment.json` | API corriendo |
| E2E (opcional) | `dotnet test --filter "Categoria=E2E"` | API y web corriendo |

## Documentación de la API
Colección de Postman en `postman/`. Importar la colección y el entorno.

## Decisiones de diseño
(Completar: arquitectura, edad calculada, geografía normalizada, relación muchos a muchos,
SQLite en memoria para las pruebas, sin repositorio genérico sobre EF.)

## Uso de IA
El proyecto se desarrolló con agentes de IA con roles separados y modelos de distintos
proveedores (detalle en `docs/AGENTES.md`):

- **Implementador:** Claude Code, sobre `src/`.
- **Tester:** Antigravity CLI con un modelo Gemini, que escribió las pruebas desde la especificación,
  antes de la implementación y sin leerla (verificación independiente, modelo en V).
- **Revisor:** revisión de cada rama contra la especificación.
- **Responsable:** todas las decisiones de diseño, la aprobación de contratos, la resolución
  de discrepancias entre pruebas e implementación y la revisión final del código.

(Completar al final: qué funcionó, qué hubo que corregir y ejemplos de hallazgos del revisor.)

## Mejoras futuras
Autenticación y roles, auditoría de cambios, carga masiva, despliegue en contenedores,
borrado lógico (estado activo/inactivo, con historial de la relación laboral), unicidad del NIT
por país garantizada por la base (vista indexada de SQL Server).

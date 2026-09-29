## Resumen
<!-- Qué entrega este PR y por qué, en 2 o 3 líneas. -->

## Tareas
<!-- Números de docs/TAREAS.md que cierra este PR. -->
- Tarea N · `mensaje del commit`

## Reglas cubiertas
<!-- Ids de docs/ESPECIFICACION.md y la prueba que los verifica.
     Debe coincidir con la tabla de trazabilidad de docs/PLAN_PRUEBAS.md.
     Si la fase no toca reglas, escribir «No aplica». -->
| Id | Prueba |
|---|---|
| RN1 | `Eliminar_PaisConDepartamentos_LanzaConflicto` |

## Roles
<!-- Quién hizo cada parte (ver docs/AGENTES.md). -->
- Pruebas: tester (Gemini) · Implementación: implementador (Claude) · Revisión: revisor (Gemini, sesión nueva)

## Revisión
<!-- Hallazgos del revisor y qué decidió el responsable con cada uno. -->
| Hallazgo | Gravedad | Decisión |
|---|---|---|

## Terminado
<!-- docs/CODIFICACION.md §7. Marcar lo que corresponda; lo que no aplique, aclararlo. -->
- [ ] `dotnet build` sin advertencias
- [ ] `dotnet test --filter "Categoria!=E2E&Categoria!=SqlServer"` en verde
- [ ] `dotnet format --verify-no-changes` sin cambios
- [ ] Sin secretos ni cadenas de conexión con contraseña
- [ ] Postman actualizado si cambió un endpoint
- [ ] Trazabilidad y `docs/ERRORES_RECURRENTES.md` actualizados si corresponde

## Desvíos y pendientes
<!-- Cambios respecto del plan, decisiones nuevas o lo que queda para otra fase. -->

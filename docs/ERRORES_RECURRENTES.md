# Errores recurrentes

Errores ya resueltos y cómo evitarlos, para no tropezar dos veces con lo mismo.

- **Antes de empezar una tarea**, leer este archivo.
- **Al resolver un error que pueda repetirse**, agregar una entrada en el mismo commit de la tarea.
  Pueden escribir aquí el implementador, el tester y el responsable (ver `docs/AGENTES.md`).
- No es un registro de bugs: un bug se corrige y queda cubierto por una prueba. Aquí va lo que
  hay que saber para no volver a causarlo.
- Numeración correlativa (`E-001`, `E-002`, …), sin reutilizar números.

Formato de cada entrada:
```
### E-000 · Título corto
- Síntoma:       qué se ve (mensaje de error o comportamiento)
- Causa:         por qué pasa
- Solución:      qué se hizo
- Cómo evitarlo: regla práctica para la próxima vez
- Origen:        tarea N · fecha · rol que lo encontró
```

## Código y herramientas
Compilación, paquetes, pruebas, EF Core, git.

### E-001 · `dotnet test` falla con «Testing with VSTest target is no longer supported»
- Síntoma:       `dotnet test` falla en cada proyecto de pruebas con ese mensaje, aunque todo compila.
- Causa:         xunit.v3 4.x usa Microsoft.Testing.Platform (MTP) v2, que ya no admite el modo
                 VSTest de `dotnet test` en el SDK de .NET 10.
- Solución:      `global.json` en la raíz con `"test": { "runner": "Microsoft.Testing.Platform" }`.
                 Los proyectos de prueba usan sólo `xunit.v3` y `coverlet.MTP`.
- Cómo evitarlo: no agregar paquetes de VSTest (`Microsoft.NET.Test.Sdk`, `xunit.runner.visualstudio`,
                 `coverlet.collector`) ni borrar `global.json`. El filtro
                 `--filter "Categoria!=E2E&Categoria!=SqlServer"` sigue funcionando: xunit.v3 lo acepta en MTP.
- Origen:        tarea 1 · 2026-09-29 · implementador

### E-002 · `dotnet test` termina con código 8
- Síntoma:       «No se ejecutaron pruebas», código de salida 8.
- Causa:         MTP devuelve 8 cuando no se ejecutó ninguna prueba: un proyecto vacío o un filtro
                 que excluye todo.
- Solución:      hasta la tarea 3 es esperado, porque los proyectos de pruebas están vacíos.
- Cómo evitarlo: no usar `--ignore-exit-code 8` en forma permanente, porque escondería un filtro mal
                 escrito. Si aparece cuando ya hay pruebas, revisar el filtro y los `[Trait]`.
- Origen:        tarea 1 · 2026-09-29 · implementador

### E-003 · `dotnet format` marca CHARSET o WHITESPACE en archivos de plantillas
- Síntoma:       `dotnet format --verify-no-changes` falla en archivos que genera `dotnet new`.
- Causa:         algunas plantillas traen BOM o saltos de línea que no respetan `.editorconfig`
                 (UTF-8 sin BOM).
- Solución:      correr `dotnet format` y volver a verificar.
- Cómo evitarlo: después de cada `dotnet new` (proyecto o elemento), correr `dotnet format`.
- Origen:        tarea 1 · 2026-09-29 · implementador

### E-004 · Avisos «LF will be replaced by CRLF» al hacer `git add`
- Síntoma:       decenas de avisos de git al agregar archivos.
- Causa:         el repositorio mezclaba archivos con finales de línea LF y CRLF, y git en Windows
                 los convierte.
- Solución:      `.gitattributes` con `* text=auto`: git guarda LF en el repositorio y cada
                 máquina usa su propio formato.
- Cómo evitarlo: no borrar `.gitattributes`. Los avisos son informativos y no rompen nada.
- Origen:        tarea 1 · 2026-09-29 · implementador

## Funcionalidad
Comportamiento que no cumplía la especificación, detectado por pruebas o revisión.

(Sin entradas todavía.)

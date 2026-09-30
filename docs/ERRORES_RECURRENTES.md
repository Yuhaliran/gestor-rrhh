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
- Solución:      los proyectos que todavía no tienen pruebas (RRHH.UnitTests y RRHH.IntegrationTests)
                 ignoran el código 8 en su `.csproj`, con `TestingPlatformCommandLineArguments`
                 (tarea 3b). RRHH.ArchitectureTests no lo ignora.
- Cómo evitarlo: ignorar el 8 sólo por proyecto y mientras esté vacío. Ignorarlo en toda la solución
                 (`dotnet test --ignore-exit-code 8` o `TESTINGPLATFORM_EXITCODE_IGNORE`) haría que
                 un filtro mal escrito dé verde sin ejecutar ninguna prueba. Al agregar la primera
                 prueba a un proyecto, quitar esa propiedad de su `.csproj` (recordatorio en las
                 tareas 4 y 15). Si el 8 aparece en un proyecto con pruebas, revisar el filtro y los
                 `[Trait]`.
- Origen:        tarea 1 · 2026-09-29 · implementador; solución en la tarea 3b

### E-003 · `dotnet format` marca CHARSET o WHITESPACE en archivos de plantillas
- Síntoma:       `dotnet format --verify-no-changes` falla en archivos que genera `dotnet new`.
- Causa:         algunas plantillas traen BOM o saltos de línea que no respetan `.editorconfig`
                 (UTF-8 sin BOM).
- Solución:      correr `dotnet format` y volver a verificar.
- Cómo evitarlo: después de cada `dotnet new` (proyecto o elemento), correr `dotnet format`.
                 Otras herramientas también escriben BOM: `dotnet user-secrets init` y
                 `dotnet add package` en los `.csproj`, y `dotnet ef migrations add` en los archivos
                 generados. `dotnet format` no revisa ninguno de esos, así que hay que quitarlo a mano
                 (en Git Bash: `sed -i '1s/^\xEF\xBB\xBF//' <archivo>`).
- Origen:        tarea 1 · 2026-09-29 · implementador; ampliado en la revisión de la fase 1

### E-004 · Avisos «LF will be replaced by CRLF» al hacer `git add`
- Síntoma:       decenas de avisos de git al agregar archivos.
- Causa:         git en Windows (`core.autocrlf=true`) guarda LF en el repositorio y usa CRLF en la
                 copia de trabajo. Los archivos escritos con LF (por herramientas o agentes)
                 generan el aviso porque en el próximo checkout van a pasar a CRLF.
- Solución:      no hace falta hacer nada: el repositorio queda con LF. El `.gitattributes`
                 (`* text=auto`) asegura lo mismo en máquinas sin esa configuración (Mac, Linux).
- Cómo evitarlo: los avisos son informativos y no rompen nada; no borrar `.gitattributes`.
- Origen:        tarea 1 · 2026-09-29 · implementador

### E-005 · Una regla `Write(...)` en los permisos de Claude Code no bloquea nada
- Síntoma:       una regla como `"deny": ["Write(tests/**)"]` se acepta, pero Claude Code puede
                 seguir escribiendo en esa ruta (al iniciar avisa que la regla no se consulta).
- Causa:         para archivos, Claude Code sólo aplica reglas `Edit(...)` y `Read(...)`; las de
                 `Write`, `NotebookEdit` o `MultiEdit` con ruta se ignoran.
- Solución:      `.claude/settings.json` con `Edit(/tests/**)`, que cubre todas las herramientas
                 que modifican archivos.
- Cómo evitarlo: para archivos, sólo reglas `Edit(...)` y `Read(...)`, con `/` al inicio para
                 anclarlas a la raíz del repositorio. Los comandos se niegan en cada terminal:
                 `Bash(...)` y `PowerShell(...)`.
- Origen:        tarea 2b · 2026-09-29 · implementador

### E-006 · El hook de `agy` falla con «Cannot find module ...\.agents\.agents\hooks\...»
- Síntoma:       en `agy`, todas las herramientas fallan con `jsonhook__permisos-por-rol_PreToolUse`
                 `failed: exit status 1` y `MODULE_NOT_FOUND` sobre una ruta con `.agents` repetido.
- Causa:         `agy` ejecuta los comandos de `.agents/hooks.json` desde la carpeta `.agents/`, no
                 desde la raíz del repositorio; una ruta `.agents/hooks/...` queda duplicada.
- Solución:      comando `node hooks/permisos-por-rol.mjs`, relativo a `.agents/`.
- Cómo evitarlo: en `.agents/hooks.json`, escribir las rutas relativas a `.agents/`. Para probar un
                 hook a mano, ejecutarlo desde esa carpeta.
- Origen:        tarea 2b · 2026-09-29 · responsable

### E-007 · `agy` bloquea todo con «tool call denied by pre-tool hook:» y sin motivo
- Síntoma:       el tester no puede leer `docs/`, ejecutar `git status` ni escribir en `tests/`; el
                 mensaje de bloqueo termina en «:» sin explicación.
- Causa:         `agy` trata como `deny` una respuesta del hook sin `decision` (por ejemplo `{}`).
- Solución:      el hook responde siempre una decisión: `allow` donde `agy` ya permitía por defecto,
                 `ask` en los comandos y fuera de sus carpetas, `deny` con `reason` en lo prohibido.
- Cómo evitarlo: en un hook `PreToolUse`, nunca devolver `{}`. Un bloqueo sin motivo indica que el
                 hook no decidió.
- Origen:        tarea 2b · 2026-09-29 · responsable

### E-008 · Un `packages.lock.json` menciona un proyecto que no existe
- Síntoma:       el lock de un proyecto de pruebas tiene un proyecto o una dependencia entre
                 proyectos que no están en la solución (por ejemplo `rrhh.temporal`).
- Causa:         pruebas de mutación: con la arquitectura rota a propósito, `dotnet test` restaura
                 y reescribe los `packages.lock.json` de los proyectos de pruebas. Al deshacer sólo
                 `src/`, el lock de `tests/` quedó con la mutación y se commiteó.
- Solución:      `dotnet restore <proyecto> --force-evaluate` regenera el lock desde el estado real.
- Cómo evitarlo: hacer las mutaciones con el árbol de trabajo limpio y deshacerlas con
                 `git checkout -- .`, que incluye los lock files. Antes de commitear, revisar el
                 diff de cada `packages.lock.json`.
- Origen:        tarea 3 · 2026-09-29 · implementador

### E-009 · «The process cannot access the file ... because it is being used by another process»
- Síntoma:       `dotnet build` o `dotnet test` falla con MSB4024 u otro error de archivo bloqueado en
                 `obj/`, justo después de cambiar un `.csproj`.
- Causa:         Visual Studio tiene la solución abierta y restaura o compila al detectar el cambio,
                 al mismo tiempo que el comando del agente.
- Solución:      esperar unos segundos y volver a correr el comando.
- Cómo evitarlo: no compilar en Visual Studio mientras un agente compila, aceptar la recarga de
                 archivos cuando Visual Studio la pida y no editar ahí los archivos que un agente
                 está modificando. Si el error se repite, cerrar Visual Studio durante la tarea.
- Origen:        tarea 3 · 2026-09-29 · implementador

### E-010 · Visual Studio 2022 deja los `packages.lock.json` sin paquetes
- Síntoma:       después de cambiar de rama con la solución abierta, los lock files de RRHH.Api y de
                 los proyectos de pruebas pierden todos sus paquetes (quedan sólo los proyectos).
- Causa:         Visual Studio 2022 (17.14) no es compatible con .NET 10 (el soporte llega con
                 Visual Studio 2026); al restaurar, evalúa mal los proyectos y reescribe los lock.
- Solución:      `git checkout -- <lock files>`; la versión commiteada es la correcta.
- Cómo evitarlo: no abrir la solución con Visual Studio 2022: usar VS Code con C# Dev Kit
                 (docs/ENTORNO.md) o Visual Studio 2026. Antes de commitear, revisar el diff de los
                 lock files (E-008); la integración continua los verifica con `--locked-mode`.
- Origen:        tarea 4 · 2026-09-29 · implementador

### E-011 · «The requested configuration is not stored in the read-optimized model»
- Síntoma:       una prueba que lee las restricciones CHECK desde `Contexto.Model` lanza
                 `InvalidOperationException`, con o sin la configuración hecha (parece un rojo esperado).
- Causa:         en ejecución, EF Core usa un modelo optimizado que no guarda los CHECK ni otros datos
                 que sólo sirven para crear la base.
- Solución:      leerlos del modelo de diseño:
                 `Contexto.GetService<IDesignTimeModel>().Model.FindEntityType(...)!.GetCheckConstraints()`.
                 `IDesignTimeModel` está en `Microsoft.EntityFrameworkCore.Metadata` (EF Core, sin el
                 paquete Design) y `GetService<T>()` en `Microsoft.EntityFrameworkCore.Infrastructure`.
- Cómo evitarlo: una prueba en rojo tiene que fallar en su `Assert`, no con una excepción; revisar el
                 motivo antes de darla por buena. Para comprobar un CHECK, preferir la prueba de
                 comportamiento: SQLite sí aplica los CHECK.
- Origen:        tarea 5 · 2026-09-29 · implementador

### E-012 · Una prueba de validación da por válido un DTO con datos inválidos
- Síntoma:       `Validator.TryValidateObject` devuelve `true` para un DTO que la API rechazaría (por
                 ejemplo, un tamaño de página 0).
- Causa:         en un record posicional, los atributos (`[Range]`, `[Required]`…) quedan en los
                 parámetros del constructor. La API los usa, pero `Validator.TryValidateObject` sólo
                 mira las propiedades. Tampoco sirve `[property: …]`: la API rechaza atributos en las
                 propiedades de un record posicional.
- Solución:      los DTOs que se validan son records con propiedades `init` y los atributos en ellas
                 (docs/CODIFICACION.md, «DTOs»).
- Cómo evitarlo: no declarar DTOs de escritura como records posicionales. Una prueba de validación
                 tiene que incluir al menos un caso inválido que falle.
- Origen:        tarea 8 · 2026-09-30 · implementador

### E-013 · Colección de Postman inválida tras edición con PowerShell
- Síntoma:       el JSON tiene saltos de línea reales dentro de un texto («Bad control character in string literal») y textos corruptos («PaÃƒÂses», «mayÃƒÂºsculas»).
- Causa:         PowerShell 5.1 rompe la codificación UTF-8 al leer y escribir archivos, y los reemplazos pueden meter saltos de línea dentro de un JSON.
- Solución:      corregir el JSON editándolo con la herramienta de edición de archivos. En los scripts de prueba, cada línea de código va como un texto separado del arreglo "exec". Validar el resultado con `node -e "JSON.parse(require('fs').readFileSync('...','utf8'))"`.
- Cómo evitarlo: usar la herramienta de edición de archivos (write_to_file o replace_file_content) en lugar de Get-Content/Set-Content o reemplazos de PowerShell; y validar siempre el JSON después de editarlo.
- Origen:        tarea 9 · 2026-09-29 · tester

### E-014 · El informe del tester da por hechas verificaciones que no se hicieron
- Síntoma:       el informe dice que la trazabilidad se actualizó, que todo el rojo es por
                 `NotImplementedException` o que `dotnet format` pasa, y no es así: la fila no está
                 en el commit, hay pruebas que fallan en el Arrange (datos repetidos que violan un
                 único) o el formato falla por un cambio posterior a la verificación.
- Causa:         se verificó antes del último cambio, o se miró que el archivo estuviera en el commit
                 y no su contenido.
- Solución:      corregir y volver a verificar después del último cambio.
- Cómo evitarlo: como último paso antes del commit, correr `dotnet format --verify-no-changes` y
                 `dotnet test`, y revisar el motivo de cada prueba en rojo: tiene que ser
                 `NotImplementedException` (o el `Assert`), nunca una excepción del Arrange. Revisar
                 con `git diff --cached` que la trazabilidad tenga cada prueba nueva en su fila. En
                 los datos de prueba, generar nombres y códigos únicos para cada entidad creada.
- Origen:        tareas 10 y 11 · 2026-09-30 · implementador

### E-015 · El tester edita archivos con scripts y rompe las pruebas
- Síntoma:       pruebas con comentarios Arrange-Act-Assert insertados en cualquier lugar, código
                 borrado o valores esperados copiados de otra prueba; comandos
                 `node -e "eval(Buffer.from('…','base64'))"` que el responsable aprueba sin poder
                 leerlos; archivos temporales (`inject.py`, `postman/update.js`).
- Causa:         para editar muchas líneas, el tester genera scripts (a veces en base64, para
                 esquivar el escapado de PowerShell) en lugar de usar la herramienta de edición.
                 Un script no pasa por el control de escritura del hook, y como todos los comandos
                 pedían aprobación, la de un script se perdía entre las demás.
- Solución:      el hook bloquea los intérpretes para el tester (salvo validar un JSON) y permite
                 sin aprobación sus comandos de cada tarea (docs/AGENTES.md).
- Cómo evitarlo: editar sólo con la herramienta de edición, prueba por prueba. No aprobar un
                 comando que no se puede leer.
- Origen:        tareas 12 y 13 · 2026-09-30 · implementador

### E-016 · agy pide permiso para comandos que el hook permite
- Síntoma:       el tester pide aprobación para `dotnet format --include tests/`, `dotnet test` o
                 una edición en `tests/`, aunque el registro del hook (`%TEMP%\rrhh-permisos.log`)
                 dice `allow`. Algunos comandos no preguntan (`git status`) y otros sí.
- Causa:         agy no toma el `allow` del hook como aprobación: sólo deja de preguntar si el
                 permiso está concedido, y los concedidos son los que se aprobaron con «permitir
                 siempre» (en `settings.json` de agy), con el comando exacto.
- Solución:      el hook devuelve con cada `allow` el permiso de esa llamada
                 (`permissionOverrides`), y con cada pregunta al tester, `force_ask`, para que un
                 permiso recordado no apruebe solo un comando riesgoso.
- Cómo evitarlo: no usar «permitir siempre» con comandos del tester; revisar los permisos
                 recordados en `%USERPROFILE%\.gemini\antigravity-cli\settings.json`.
- Origen:        tarea 14 · 2026-09-30 · implementador

### E-017 · La API que levanta el tester responde 503 en /health
- Síntoma:       con `dotnet run --project src/RRHH.Api` desde agy, `/health` responde 503
                 (`Unhealthy`) y cada intento de conexión a la base tarda unos 18 segundos. Newman
                 falla desde el principio. La misma API levantada desde una terminal común anda.
- Causa:         agy ejecuta los comandos del tester en un entorno aislado, desde el que no se llega
                 a LocalDB.
- Solución:      la API la levanta el responsable o el implementador, fuera de agy
                 (`dotnet run --project src/RRHH.Api --launch-profile http`); el tester sólo corre
                 Newman contra `http://localhost:5279`.
- Cómo evitarlo: antes de correr Newman, comprobar `GET /health`. Con 503, no esperar: pedir que
                 se levante la API fuera de agy.
- Origen:        tarea 14 · 2026-09-30 · implementador

## Funcionalidad
Comportamiento que no cumplía la especificación, detectado por pruebas o revisión.

(Sin entradas todavía.)

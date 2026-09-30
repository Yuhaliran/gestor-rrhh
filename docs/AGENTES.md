# Uso de agentes de IA: roles y modelos

## Principio: verificación independiente
Si el mismo modelo escribe el código y sus pruebas, tiende a interpretar el requisito de la
misma manera en ambos: las pruebas pasan porque comparten el mismo error. Por eso quien
prueba no es quien implementa, igual que en un equipo de personas.

En este proyecto, las pruebas se escriben **a partir de la especificación, antes que la
implementación y con un modelo de otro proveedor**. Esto reduce los errores correlacionados,
pero no los elimina: la revisión humana decide ante cualquier discrepancia.

Es el lado derecho del modelo en V aplicado a agentes: cada prueba se diseña desde su
especificación, no desde el código.

## Roles
| Rol | Herramienta y modelo | Puede modificar | No puede |
|---|---|---|---|
| **Implementador** | Claude Code (Claude) | `src/`; entradas en `docs/ERRORES_RECURRENTES.md` | Modificar `tests/` |
| **Tester** | Antigravity CLI (`agy`) con un modelo Gemini | `tests/`, `postman/`, tabla de trazabilidad de `docs/PLAN_PRUEBAS.md`; entradas en `docs/ERRORES_RECURRENTES.md` | Leer la implementación de los servicios; modificar `src/` |
| **Revisor** | El agente tester, en una sesión nueva | Nada: sólo informa | Aprobar o hacer merge |
| **Responsable** | La persona | Todo | — |

Instrucciones de cada rol: `docs/agentes/implementador.md`, `docs/agentes/tester.md`,
`docs/agentes/revisor.md`.

## Ciclo por tarea (tareas de la API)
```
1. Contrato        Implementador propone interfaz del servicio y DTOs  → Responsable aprueba
                   commit: feat(x): contrato del servicio
2. Pruebas         Tester escribe las pruebas desde ESPECIFICACION.md,
                   PLAN_PRUEBAS.md y el contrato. Quedan en rojo.
                   commit: test(x): pruebas desde la especificación
3. Implementación  Implementador implementa hasta que todo quede en verde,
                   sin tocar las pruebas.
                   commit: feat(x): implementación
4. Revisión        Revisor compara el diff con la especificación y lista hallazgos.
5. Decisión        Responsable resuelve discrepancias. Al cerrar la fase, los hallazgos
                   van a la sección Revisión del PR y hace el merge.
```

Si el implementador cree que una prueba está mal, **no la cambia**: lo explica. El
responsable decide comparando con la especificación. Si la prueba estaba mal, la corrige el
tester; si la especificación era ambigua, se aclara primero la especificación.

## Por qué el contrato va primero
El tester necesita saber qué métodos y DTOs existen para que sus pruebas compilen, pero no
debe ver cómo están implementados (pruebas de caja negra). El contrato es la frontera:
interfaces y DTOs sí; servicios, no.

## Cómo se imponen las reglas
Las restricciones no dependen sólo de las instrucciones: se configuran en cada herramienta,
para que el agente **no pueda** hacer lo que no debe. Las dos configuraciones están versionadas.

**Implementador (Claude Code): `.claude/settings.json`**
```json
{
  "permissions": {
    "deny": [
      "Edit(/tests/**)",
      "Edit(/postman/**)",
      "Read(./.env)",
      "Bash(git push *)",
      "PowerShell(git push *)"
    ]
  }
}
```
Para archivos, Claude Code sólo aplica reglas `Edit(...)` y `Read(...)` (ver E-005 en
`docs/ERRORES_RECURRENTES.md`); la `/` inicial ancla la ruta a la raíz del repositorio.
`git push` se niega en Bash y en PowerShell, las dos terminales que usa en Windows.

**Tester y revisor (Antigravity CLI): hook `.agents/hooks.json`**
Antigravity permite por defecto escribir en todo el proyecto y, en sus reglas de permisos,
una denegación gana siempre: «sólo estas carpetas» no se puede expresar con reglas. Por eso
un hook del proyecto (`.agents/hooks/permisos-por-rol.mjs`, en Node) revisa cada herramienta
de archivos y cada comando antes de que se ejecuten:

| Rol (`RRHH_ROL`) | Escritura en el repositorio | Lectura | Comandos |
|---|---|---|---|
| `tester` (por defecto) | Sólo `tests/`, `postman/`, `docs/PLAN_PRUEBAS.md` y `docs/ERRORES_RECURRENTES.md` | Todo menos `src/RRHH.Application/Servicios/` y `src/RRHH.Infrastructure/` | Cualquiera, con aprobación |
| `revisor` | Nada | Todo | Sólo de lectura, con aprobación |

Comandos de lectura del revisor: `git` de consulta (`diff`, `log`, `show`, `status`, `blame`,
`grep`, `branch` sin modificar…), `dotnet build`, `test`, `format --verify-no-changes` y
`ef migrations has-pending-model-changes`, y los de la terminal que sólo leen (`Get-Content`,
`Select-String`, `Get-ChildItem`…). Se bloquea cualquier otro, y también un comando de lectura
que redirija la salida a un archivo (`>`) o que incluya uno que escribe (`Set-Content`,
`Remove-Item`…). En la revisión de la fase 1, el revisor había creado volcados del diff en la
raíz del repositorio con `>` y `Set-Content`.

Para los dos roles quedan bloqueados `git push` y los subagentes, porque las herramientas de un
subagente no pasan por el hook. Un valor desconocido de `RRHH_ROL` bloquea toda escritura y
limita los comandos como al revisor. Lo demás sigue como Antigravity lo hace por defecto: los
comandos piden aprobación y, fuera del repositorio, se permiten sólo sus propias carpetas
(artefactos, temporales y ayuda incorporada); para el resto, pregunta.

El rol se define en PowerShell antes de abrir `agy` (escrito en el chat no llega al hook):
```powershell
$env:RRHH_ROL = "tester";  agy     # sesión del tester
$env:RRHH_ROL = "revisor"; agy     # sesión del revisor
```

Antigravity carga el hook cuando se confía en la carpeta del proyecto (primer inicio). Ejecuta
el comando desde la carpeta `.agents/` (por eso la ruta es `hooks/...`, ver E-006) y, en
Windows, con `cmd /C`, que no interpreta comillas escapadas: el comando va sin comillas. Si el
hook falla o responde sin decisión, `agy` bloquea la herramienta (E-007).

Límites: con el tester, los permisos controlan sus herramientas de archivos, no lo que hacen los
programas que ejecuta (un `dotnet format` o un `type` desde la terminal); una búsqueda sobre todo
el repositorio puede mostrarle líneas de la implementación. Con el revisor, `dotnet build` y
`dotnet test` escriben en `bin/` y `obj/`, que git ignora. La revisión humana sigue siendo la
última barrera.

```
Mensaje inicial de una sesión del tester:
«Actuás con el rol de docs/agentes/tester.md. Leé AGENTS.md, docs/ESPECIFICACION.md,
 docs/PLAN_PRUEBAS.md y el contrato de la tarea N. No leas la implementación.»
```

Nota (septiembre de 2026): Gemini CLI dejó de funcionar con cuentas Google AI Pro el
18 de junio de 2026; su reemplazo es Antigravity CLI. Alternativa: OpenCode con una
clave de API de Gemini de Google AI Studio.

## Modelos por rol
Cada rol usa un modelo distinto, definido por configuración de cada herramienta, no en el
código del proyecto. Cambiar el modelo de un rol no requiere cambiar nada más.
En una plataforma en la nube, lo equivalente sería un servicio multimodelo como
Amazon Bedrock, Azure AI Foundry o Vertex AI, o un gateway como LiteLLM.

## Registro
Cada commit indica qué rol lo produjo en el cuerpo del mensaje:

```
test(empresas): pruebas desde la especificación

Rol: tester (Gemini). Revisado por: <responsable>.
```

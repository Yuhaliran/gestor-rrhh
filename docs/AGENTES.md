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
5. Decisión        Responsable resuelve discrepancias y hace merge.
```

Si el implementador cree que una prueba está mal, **no la cambia**: lo explica. El
responsable decide comparando con la especificación. Si la prueba estaba mal, la corrige el
tester; si la especificación era ambigua, se aclara primero la especificación.

## Por qué el contrato va primero
El tester necesita saber qué métodos y DTOs existen para que sus pruebas compilen, pero no
debe ver cómo están implementados (pruebas de caja negra). El contrato es la frontera:
interfaces y DTOs sí; servicios, no.

## Cómo se imponen las reglas
Las restricciones no dependen sólo de las instrucciones: se configuran como permisos de
cada herramienta, para que el agente **no pueda** hacer lo que no debe.

```
// .claude/settings.json — el implementador no puede editar pruebas
{
  "permissions": {
    "deny": ["Edit(tests/**)", "Write(tests/**)", "Read(./.env)", "Bash(git push:*)"]
  }
}
```

Para el tester, las instrucciones del rol no dependen de la herramienta: cada sesión
empieza indicando el rol, y los límites se configuran en los permisos de la herramienta
(sólo escritura en `tests/`, `postman/`, `docs/PLAN_PRUEBAS.md` y `docs/ERRORES_RECURRENTES.md`).

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

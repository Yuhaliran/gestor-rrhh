# Prompts para cada paso

Mensajes listos para pegar. Reemplazar `N` por el número de tarea de `docs/TAREAS.md`.
Cada sesión empieza limpia: una tarea, una sesión.

## Claude Code · implementador

**Inicio de la fase 0**
```
Leé AGENTS.md y todos los archivos de docs/. Vamos con la tarea 1 de docs/TAREAS.md.
Proponé el plan (comandos y estructura) y esperá mi confirmación antes de ejecutar.
```

**Paso 1 · contrato (tareas de la API)**
```
Tarea N. Siguiendo docs/CODIFICACION.md, proponé sólo el contrato:
DTOs en RRHH.Contratos e interfaz del servicio en RRHH.Application, con un comentario
por método indicando los errores posibles y la regla (RN/V) que aplica.
No implementes el servicio. Esperá mi aprobación.
```

**Paso 3 · implementación**
```
Tarea N. El contrato está aprobado y las pruebas del tester están en tests/.
Implementá el servicio, el controlador y la configuración de EF que falte hasta que
`dotnet test` quede en verde. No modifiques nada en tests/. Si una prueba contradice
la especificación, detenete y explicá por qué, citando docs/ESPECIFICACION.md.
```

## Antigravity CLI · tester
Iniciar desde PowerShell con `$env:RRHH_ROL = "tester"; agy`, no en el chat (permisos del rol
en `docs/AGENTES.md`).

**Paso 2 · pruebas**
```
Actuás con el rol de docs/agentes/tester.md. Leé AGENTS.md, docs/ESPECIFICACION.md,
docs/PLAN_PRUEBAS.md, docs/CODIFICACION.md (sección Pruebas), docs/ERRORES_RECURRENTES.md
y el contrato de la tarea N (DTOs en src/RRHH.Contratos e interfaz en src/RRHH.Application).
No leas implementaciones.
Escribí las pruebas unitarias de la tarea: todas las reglas y validaciones que aplican,
con valores límite y casos de error. Deben compilar y quedar en rojo.
Actualizá la tabla de trazabilidad de docs/PLAN_PRUEBAS.md.
```

## Antigravity CLI · revisor (sesión nueva)
Iniciar desde PowerShell con `$env:RRHH_ROL = "revisor"; agy`: el hook bloquea toda escritura y
sólo permite comandos de lectura (ver `docs/AGENTES.md`).

**Paso 4 · revisión**
```
Actuás con el rol de docs/agentes/revisor.md. Revisá `git diff main...HEAD` contra
docs/ESPECIFICACION.md y docs/CODIFICACION.md. No modifiques archivos ni crees archivos
temporales: leé el diff por partes (git diff main...HEAD -- <ruta>).
Devolvé la lista de hallazgos con archivo, gravedad, qué pasa y qué dice la especificación.
```

## Cuando algo sale mal
**El agente se traba o repite el mismo error**
```
Detenete. Resumí en 5 líneas qué intentaste, qué falla y cuál creés que es la causa.
No cambies nada más.
```
Después: volver al último commit bueno y empezar una sesión nueva con una tarea más chica
y el dato que faltaba.

**Discrepancia entre prueba e implementación**
Decidís vos, comparando con la especificación. Si la prueba estaba mal, la corrige el
tester; si la especificación era ambigua, se aclara primero la especificación.

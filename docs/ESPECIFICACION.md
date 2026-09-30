# Especificación funcional

Fuente de verdad de los requisitos. Cada criterio (CA) y regla (RN, V) tiene un
identificador, que se usa en `docs/PLAN_PRUEBAS.md` para rastrear qué prueba lo verifica.

## Objetivo
Que el departamento de Recursos Humanos pueda registrar a sus colaboradores
y tener el detalle de la empresa (o empresas) a la que pertenecen.

## Entidades y mantenimientos
Cada entidad tiene su mantenimiento: listar (con paginación y búsqueda), ver detalle,
crear, editar y eliminar.

### País
- Nombre (obligatorio, único)
- Código ISO de 2 letras (obligatorio, único; p. ej. GT)
- Edad mínima y edad máxima de los colaboradores (obligatorias, enteros no negativos, la mínima
  no mayor que la máxima; por defecto 18 y 100)
- Fecha de cumpleaños de los nacidos el 29 de febrero en años no bisiestos: 28 de febrero
  (por defecto) o 1 de marzo

### Departamento
- País (obligatorio)
- Nombre (obligatorio, único dentro del país)

### Municipio
- Departamento (obligatorio)
- Nombre (obligatorio, único dentro del departamento)

### Empresa
- Geografía: municipio (obligatorio). El departamento y el país se obtienen del municipio.
- NIT (obligatorio, único dentro del país de la empresa)
- Razón social (obligatoria)
- Nombre comercial (obligatorio)
- Teléfono (obligatorio)
- Correo electrónico (obligatorio, formato válido)

### Colaborador
- Nombre completo (obligatorio)
- Fecha de nacimiento (obligatoria); la **edad** se calcula a partir de ella
- Teléfono (obligatorio)
- Correo electrónico (obligatorio, formato válido, único)
- Empresas: una o varias (al menos una)

### Relación Empresa – Colaborador
- Un colaborador puede pertenecer a una o varias empresas.
- Datos de la relación: fecha de ingreso (obligatoria) y puesto (opcional).

## Reglas de negocio
- **RN1.** No se puede eliminar un país con departamentos, un departamento con municipios,
  ni un municipio con empresas. Respuesta: 409 Conflict.
- **RN2.** No se puede eliminar una empresa con colaboradores asociados (409).
- **RN3.** Un colaborador siempre tiene al menos una empresa: al crearlo se exige una,
  y no se puede quitar la última (409).
- **RN4.** La edad del colaborador debe estar dentro del rango (edad mínima y máxima) del país
  de cada una de sus empresas. Se valida al crear o editar un colaborador y al asociarlo a una
  empresa (400). Cambiar el rango de un país no afecta a los colaboradores ya registrados.
- **RN5.** No se permiten duplicados: NIT dentro del mismo país, código ISO y nombre de país,
  correo de colaborador, nombres dentro de su padre, y la misma empresa dos veces en un
  colaborador (409).
- **RN6.** La geografía de la empresa se elige en cascada: país → departamento → municipio.
- **RN7.** La edad se calcula a partir de la fecha de nacimiento y la fecha actual. Los nacidos
  el 29 de febrero cumplen años, en los años no bisiestos, el 28 de febrero o el 1 de marzo,
  según el país. Para RN4 se usa la regla de cada país; la edad que se muestra usa la del país de
  la empresa con la fecha de ingreso más antigua (si hay varias, la de menor id). Una fecha de
  nacimiento posterior a la fecha actual da una edad negativa, fuera de cualquier rango (RN4).

## Validaciones de entrada
- **V1.** Campos obligatorios presentes (400).
- **V2.** Correos con formato válido (400).
- **V3.** Largos máximos respetados (400).
- **V4.** Referencias existentes: país, departamento, municipio o empresa inexistente (400).

## Criterios de aceptación de la evaluación
- **CA1.** Empresas con geografía (país, departamento, municipio), NIT, razón social,
  nombre comercial, teléfono y correo.
- **CA2.** Colaboradores con empresa, nombre completo, edad, teléfono y correo.
- **CA3.** Colaboradores en una o varias empresas.
- **CA4.** Migraciones con Entity Framework.
- **CA5.** Pruebas unitarias implementadas correctamente.
- **CA6.** Arquitectura y patrones a criterio del desarrollador (documentados en PLAN.md).
- **CA7.** Todos los servicios documentados en una colección de Postman.
- **CA8.** Versionamiento con Git.

## Decisiones sobre los requisitos
- **Edad:** se guarda la fecha de nacimiento y la edad se calcula; una edad guardada
  queda desactualizada. La API devuelve ambas.
- **Legislación por país:** la edad laboral y la fecha de cumpleaños de los nacidos el 29 de
  febrero dependen de la legislación de cada país. Por eso son datos del país, con valores por
  defecto (18 a 100 y 28 de febrero) que se ajustan en su mantenimiento.
- **NIT único por país:** cada país emite sus identificadores tributarios, así que dos empresas
  de países distintos pueden tener el mismo número. El servicio valida la unicidad con el país del
  municipio de la empresa; el país no se guarda en la empresa para no duplicar el dato (podría
  contradecir al municipio). Límites conocidos: la base no lo garantiza ante dos altas simultáneas
  del mismo NIT ni al cambiar el país de un departamento. Una vista indexada de SQL Server lo
  garantizaría (mejora futura).
- **Eliminación física:** eliminar borra el registro, protegido por RN1 y RN2 (409). Un estado
  activo/inactivo (borrado lógico) abriría casos de uso que la evaluación no pide (reactivar,
  unicidad entre inactivos, historial de la relación laboral); queda como mejora futura.
- **Geografía de la empresa:** se guarda sólo el municipio (tercera forma normal), para
  que país, departamento y municipio nunca queden inconsistentes.
- **"Poseen empresa" y "una o varias empresas":** relación muchos a muchos, con al menos una.
- **Base de datos:** la aplicación usa SQL Server (LocalDB o Express en desarrollo; opcionalmente
  Docker). Las pruebas automatizadas usan SQLite en memoria para correr sin instalar nada.

## Fuera de alcance
- Autenticación y roles (mejora futura en el README).
- Carga masiva de colaboradores.
- Borrado lógico (estado activo/inactivo), ver «Eliminación física».

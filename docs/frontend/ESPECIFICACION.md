# Especificación funcional del frontend Angular

Fuente de verdad de los requisitos del frontend Angular (`frontend/`). Cada requisito tiene un
identificador, que se usa en `docs/frontend/PLAN_PRUEBAS.md` para rastrear qué prueba lo verifica.

Las reglas de negocio (RN1 a RN9) y las validaciones de entrada (V1 a V5) **no se repiten aquí**:
están en `docs/ESPECIFICACION.md` y las impone la API. El frontend no las reimplementa; las hace
visibles al usuario.

## Solicitud
Además del frontend existente (`src/RRHH.Web`, Razor Pages), se pide un **proyecto paralelo** con
otra tecnología (Angular, Flutter o Vue), en el mismo repositorio, que consuma el mismo backend. Se
eligió **Angular** (comparación con Flutter y Vue en el README, «Frontend Angular»). Fecha de
entrega: miércoles 7 de octubre de 2026, 23:59.

## Objetivo
Que el departamento de Recursos Humanos pueda hacer, desde una aplicación web de una sola página,
todo lo que hoy hace en `RRHH.Web`: mantener la geografía, las empresas y los colaboradores, y ver
las empresas de cada colaborador. **Paridad funcional** con `RRHH.Web`, consumiendo la API sin
cambiar sus reglas ni sus endpoints.

## Alcance
| Dentro | Fuera |
|---|---|
| Los 5 mantenimientos: listar (paginado, con búsqueda), crear, editar y eliminar | Autenticación y roles (igual que el backend) |
| Geografía de la empresa en cascada (RN6) | Aplicación móvil nativa o PWA |
| Alta de colaborador con una o varias empresas (RN3) | Despliegue en la nube |
| Detalle del colaborador con edad y empresas; asociar, editar y quitar empresas | Cambios a reglas o endpoints de la API |
| Colaboradores de una empresa | Varios idiomas |
| Errores de la API visibles en el campo o como aviso | Reemplazar `RRHH.Web`: los dos frontends conviven |
| CORS en la API (único cambio en el backend, ver CORS1) | Generar los tipos desde OpenAPI (mejora futura) |

## Requisitos funcionales

### Generales
- **RF1. Navegación.** Menú con Países, Departamentos, Municipios, Empresas y Colaboradores, visible
  en todas las pantallas, y una página de inicio con acceso a cada mantenimiento.
- **RF2. Listados.** Cada listado muestra una tabla paginada **por la API** (no se pagina en el
  navegador): `?pagina&tamanio&buscar`. Tamaños de página 10, 20 (por defecto) y 50. Un campo
  «Buscar» filtra al escribir (espera 300 ms sin teclear antes de consultar) y vuelve a la página 1.
  Se muestra el total de registros. Mientras carga, la tabla lo indica.
- **RF3. Eliminar.** Pide confirmación antes de eliminar. Si la API responde 409 (RN1, RN2), se
  muestra su `detail` y el registro sigue en la tabla.
- **RF4. Avisos de éxito.** Al crear, editar o eliminar, un aviso breve: «Se guardó correctamente.»
  o «Se eliminó correctamente.»; después de guardar, se vuelve al listado (o al detalle, en las
  empresas de un colaborador).

### Errores de la API
- **RF5. Errores por campo (400).** Cada mensaje de `errors` se muestra debajo de su campo. La API
  usa como clave el nombre de la propiedad en PascalCase (`Nombre`, `FechaNacimiento`,
  `Empresas[0].FechaIngreso`); el frontend la compara sin distinguir mayúsculas, también con índice.
  Un error cuya clave no corresponde a ningún campo del formulario se muestra como aviso general.
  **Excepción, JSON que la API no pudo leer:** ASP.NET Core responde 400 con una clave `$.campo`,
  con un mensaje técnico en inglés, y otra con el nombre del parámetro (`dto`, «The dto field is
  required.»). El frontend descarta la clave `dto` y muestra «El valor no es válido.» debajo del
  campo de `$.campo`, nunca el texto de la API (RF8). Las validaciones VC1 y VC5 evitan el caso: un
  campo vacío no se envía.
- **RF6. Conflicto (409).** Se muestra el `detail` de la API como aviso general del formulario o
  de la acción.
- **RF7. No encontrado (404).** Al abrir para editar o ver un registro que no existe: mensaje
  «El registro no existe.» y enlace al listado.
- **RF8. Sin conexión o error no previsto.** Sin respuesta de la API: «No se pudo conectar con la
  API.». Respuesta 500: «Ocurrió un error inesperado.». Nunca se muestran detalles técnicos.

### Por entidad
- **RF9. Países.** Listado (nombre, código ISO, edad mínima y máxima). Formulario con los cinco
  campos; al crear propone 18, 100 y «28 de febrero» (la API exige los tres: el formulario siempre
  los envía). El código ISO se muestra y envía en mayúsculas.
- **RF10. Departamentos.** Listado (nombre, país). Al crear se elige el país; al editar el país se
  muestra pero no se puede cambiar (RN8).
- **RF11. Municipios.** Listado (nombre, departamento, país). Al crear se elige país y luego
  departamento (cascada); al editar, país y departamento se muestran sin poder cambiarse (RN8).
- **RF12. Empresas.** Listado (nombre comercial, NIT, municipio, departamento, país). Formulario con
  geografía en cascada (RN6): al cambiar el país se vacían departamento y municipio; al cambiar el
  departamento se vacía el municipio; cada lista se habilita cuando su padre está elegido. Al
  editar, la cascada arranca con los valores de la empresa y el país no se puede cambiar (RN8);
  departamento y municipio sí. Desde el listado se accede a sus colaboradores (RF15).
- **RF13. Colaboradores.** Listado (nombre completo, correo, edad, empresas). Alta con datos
  personales y una lista de empresas: empieza con una fila, se pueden agregar más («Agregar
  empresa») y quitar mientras quede más de una (RN3). Cada fila: empresa, fecha de ingreso y puesto
  (opcional). Editar cambia sólo los datos personales.
- **RF14. Detalle del colaborador.** Muestra datos personales, **edad** (la que calcula la API,
  RN7) y sus empresas con país, fecha de ingreso y puesto. Desde aquí: asociar otra empresa,
  editar fecha de ingreso y puesto de una relación, y quitar una empresa. «Quitar» se deshabilita
  si es la única (RN3); si la API responde 409 igual, se muestra el aviso.
- **RF15. Colaboradores de una empresa.** Lista de los colaboradores de una empresa, con enlace al
  detalle de cada uno.

## Validaciones en el navegador
Sólo para la experiencia de usuario: evitan un viaje a la API con datos claramente incompletos. La
API sigue siendo la fuente de verdad y repite todas las validaciones.

- **VC1.** Campos obligatorios (los mismos que V1; el puesto es opcional).
- **VC2.** Largos máximos de V3 como `maxlength` en los campos.
- **VC3.** Formato de correo (V2) y de teléfono (V5: `^[0-9+()\- ]{7,20}$`).
- **VC4.** País: código ISO de 2 letras; edades enteras no negativas, mínima no mayor que máxima.
- **VC5.** Los campos de fecha no ofrecen fechas futuras (fecha de nacimiento y de ingreso).

**No** se validan en el navegador RN4 (edad dentro del rango del país), RN5 (duplicados) ni el
resto de RN9: dependen de datos que sólo tiene la API. El frontend no calcula la edad.

## Requisitos no funcionales
- **RNF1. Configuración.** La URL de la API se lee de `urlApi` en `frontend/src/environments/`
  (`environment.development.ts` en desarrollo, que `ng serve` usa en lugar de `environment.ts`). No
  está escrita en el resto del código: sólo la raíz de composición (`app.config.ts`) la lee.
- **RNF2. Formatos.** Fechas: se muestran `dd/mm/aaaa` y se envían `aaaa-mm-dd`. La conversión
  usa la fecha local (año, mes y día): `new Date('aaaa-mm-dd')` la interpreta en UTC y, en
  Guatemala (UTC−6), el calendario mostraría el día anterior; `toISOString()` tiene el problema
  inverso. Enums: se envían como texto (`VeintiochoDeFebrero`, `PrimeroDeMarzo`) y se muestran como
  «28 de febrero» y «1 de marzo».
- **RNF3. Accesibilidad básica.** Cada campo tiene su etiqueta asociada; los errores quedan
  asociados a su campo; las acciones son botones con texto.
- **RNF4. Diseño adaptable.** Se puede usar en el ancho de un teléfono (tablas con desplazamiento
  horizontal propio, sin desplazar la página).
- **RNF5. Seguridad.** Sin secretos en el código; no se guardan datos en el almacenamiento del
  navegador; el texto que llega de la API se muestra con interpolación (`{{ }}`), que Angular
  escapa, nunca con `[innerHTML]`.
- **RNF6. Calidad.** TypeScript estricto y `strictTemplates`; ESLint y Prettier sin errores;
  `npm run build` sin errores (compila y verifica los tipos de las plantillas).

## Cambio en la API
- **CORS1.** La API permite peticiones del navegador desde los orígenes configurados en
  `Cors:OrigenesPermitidos` (appsettings). En desarrollo: `http://localhost:4200` (`ng serve`, con
  el puerto fijado en `angular.json`). Sin orígenes configurados no se permite ninguno. Métodos GET,
  POST, PUT y DELETE; encabezado `Content-Type`; expone `Location`. No cambia ninguna regla ni
  respuesta de la API.

## Contrato de interfaz
Las pruebas de componentes buscan los elementos por su **etiqueta o texto visible**, como un
usuario, no por detalles de implementación. Por eso los textos son parte del contrato: cambiarlos
es cambiar la especificación.

**Rutas**

| Ruta | Pantalla |
|---|---|
| `/` | Inicio |
| `/paises`, `/paises/nuevo`, `/paises/:id/editar` | Países |
| `/departamentos`, `/departamentos/nuevo`, `/departamentos/:id/editar` | Departamentos |
| `/municipios`, `/municipios/nuevo`, `/municipios/:id/editar` | Municipios |
| `/empresas`, `/empresas/nuevo`, `/empresas/:id/editar` | Empresas |
| `/empresas/:id/colaboradores` | Colaboradores de una empresa |
| `/colaboradores`, `/colaboradores/nuevo`, `/colaboradores/:id/editar` | Colaboradores |
| `/colaboradores/:id` | Detalle del colaborador |

**Etiquetas de los campos**

| Formulario | Etiquetas |
|---|---|
| País | Nombre · Código ISO · Edad mínima · Edad máxima · Cumpleaños del 29 de febrero (opciones «28 de febrero» y «1 de marzo») |
| Departamento | País · Nombre |
| Municipio | País · Departamento · Nombre |
| Empresa | País · Departamento · Municipio · NIT · Razón social · Nombre comercial · Teléfono · Correo |
| Colaborador | Nombre completo · Fecha de nacimiento · Teléfono · Correo |
| Empresa de un colaborador (fila o diálogo) | Empresa · Fecha de ingreso · Puesto (opcional) |

**Botones y textos**
- Listados: «Nuevo», campo «Buscar»; por fila «Editar» y «Eliminar» (empresas: además
  «Colaboradores»; colaboradores: además «Ver detalle»).
- Formularios: «Guardar» y «Cancelar». Alta de colaborador: «Agregar empresa» y, por fila, «Quitar».
- Detalle del colaborador: «Editar datos», «Asociar empresa»; por empresa «Editar» y «Quitar».
- Confirmación de eliminar o quitar: «Sí, eliminar» y «Cancelar».
- Pantalla de RF7: enlace «Volver al listado».
- Mensajes: los de RF4, RF5 («El valor no es válido.»), RF7 y RF8.
- Mensajes de las validaciones en el navegador, debajo del campo: «Este campo es obligatorio.»
  (VC1), «Admite hasta N caracteres.» (VC2, con el largo), «El formato no es válido.» (VC3 y el
  código ISO de VC4), «No puede ser negativa.» y «La edad mínima no puede ser mayor que la
  máxima.» (VC4, este último debajo de «Edad máxima», como en la API).
- Listados (RF2): el campo de búsqueda tiene la etiqueta «Buscar»; el total se muestra como
  «N registros» («1 registro», «0 registros»); mientras carga, una barra de progreso (rol
  `progressbar`); un error del listado, con el texto de RF8. El paginador, en español:
  «Registros por página» (10, 20 o 50), «Página anterior», «Página siguiente», «Primera página»,
  «Última página» y el rango «1 – 10 de 45».
- Confirmación (RF3): título «Confirmar» y el mensaje «¿Eliminar este registro?».
- Padre fijo al editar (RN8, RF10 a RF12): el campo conserva su etiqueta («País»,
  «Departamento») y se muestra deshabilitado, con el nombre del padre. Al crear, el mismo campo es
  una lista para elegir (rol `combobox`); en la cascada, cada lista está deshabilitada hasta que
  se elige su padre. Guardar sin elegir un nivel de la cascada muestra «Este campo es
  obligatorio.» debajo del primero sin valor (VC1) y no llama a la API, aunque el país y el
  departamento no viajen a la API.
- Colaboradores de una empresa (RF15): título «Colaboradores de» y el nombre comercial; por fila,
  nombre completo, edad, fecha de ingreso en esa empresa, puesto y el enlace «Ver detalle» (a
  `/colaboradores/:id`); enlace «Volver al listado» (a `/empresas`). Búsqueda, total y paginador
  como en los demás listados (RF2). Si la empresa no existe, la pantalla de RF7.
- Edad (RF13 a RF15): «N años» («1 año»), tal como la devuelve la API (RN7).

**Enlaces y botones.** Lo que navega es un enlace (rol `link`): «Nuevo», «Editar» de los listados,
«Cancelar» de los formularios, «Colaboradores», «Ver detalle», «Editar datos» y «Volver al
listado». Lo que actúa es un botón (rol `button`): «Guardar», «Eliminar», «Quitar», «Agregar
empresa», «Asociar empresa», el «Editar» de una empresa en el detalle (abre un diálogo) y los dos
de la confirmación.

## Criterios de aceptación del frontend
- **CAF1.** Los 5 mantenimientos funcionan contra la API real (crear, listar, buscar, paginar,
  editar, eliminar).
- **CAF2.** Una empresa se crea y edita con la geografía en cascada; el país no cambia al editar.
- **CAF3.** Un colaborador se crea con dos empresas, se ve su edad en el detalle, y se le asocia,
  edita y quita una empresa.
- **CAF4.** Los errores de la API (400, 404, 409, sin conexión) se ven donde corresponde.
- **CAF5.** Pruebas automatizadas del frontend en verde (`npm test`).
- **CAF6.** La arquitectura por capas se respeta (`npm run lint`, ARQF1 a ARQF3).
- **CAF7.** Git: rama por fase, un PR por fase con la plantilla, integración continua en verde.
- **CAF8.** README con cómo ejecutar el frontend y las decisiones tomadas.

## Decisiones sobre los requisitos
- **Paridad, no rediseño:** las pantallas siguen los flujos de `RRHH.Web`, que ya resolvieron los
  casos de la especificación (por ejemplo, las empresas del colaborador se eligen al crearlo y
  después se manejan desde su detalle).
- **Validar en el navegador sólo lo que no depende de datos:** duplicar RN4, RN5 o RN9 obligaría a
  mantener la misma regla en dos lugares y podría contradecir a la API.
- **Edad:** la muestra el frontend tal como la calcula la API (RN7); no se recalcula.
- **CORS en vez del proxy de desarrollo:** el proxy de `ng serve` (`proxy.conf.json`) sólo existe en
  desarrollo; con CORS, el frontend compilado funciona servido desde cualquier origen permitido.
- **El JSON ilegible se resuelve en el frontend:** cambiar esa respuesta de la API (con
  `InvalidModelStateResponseFactory`) sería un segundo cambio en el backend, y el único previsto es
  CORS.

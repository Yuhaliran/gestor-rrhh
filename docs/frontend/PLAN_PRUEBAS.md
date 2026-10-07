# Plan de pruebas del frontend Angular

## Modelo de proceso: V con implementación incremental
Igual que en el backend (`docs/PLAN_PRUEBAS.md`): cada nivel de especificación tiene su nivel de
prueba, diseñado al mismo tiempo que la especificación.

```
Requisitos (frontend/ESPECIFICACION.md) ◄─────────► Aceptación: lista CAF contra la API real (Playwright: mejora futura)
   Diseño (pantallas y flujos) ◄───────────────► Componentes: vistas con Testing Library + HttpTestingController
      Arquitectura (capas) ◄─────────────────► Arquitectura: reglas de importación con ESLint
         Módulos ◄─────────────────────────► Unitarias: cliente HTTP, lógica de pantallas y formatos
                             Código
```

## Independencia de las pruebas
Mismo principio que en el backend (`docs/AGENTES.md`): las pruebas las escribe el agente tester,
con un modelo de otro proveedor, **desde la especificación y antes de la implementación**, sin leer
la implementación. En el frontend eso se logra así:

- **Unitarias:** el tester parte de las interfaces de `frontend/src/app/contratos/` (incluida
  `pantallas.ts`) y del comportamiento descrito en `docs/frontend/PLAN.md` («Errores» y «Lógica de
  las pantallas»).
- **Componentes:** el tester busca los elementos como un usuario, por etiqueta, rol o texto
  (`getByLabelText('Nombre')`, `getByRole('button', { name: 'Guardar' })`), según el «Contrato de
  interfaz» de la especificación. No depende de clases CSS, de la estructura de Angular Material
  ni de nombres internos.
- **API simulada:** las respuestas que se entregan con `HttpTestingController` (en
  `frontend/tests/apoyo/`) se escriben desde `docs/PLAN.md`, «API» (rutas, códigos y formato de
  `ProblemDetails`), no desde el cliente del frontend.

## Niveles
| Nivel | Carpeta | Herramientas | Qué necesita para correr |
|---|---|---|---|
| Unitarias | `frontend/tests/unitarias/` | Vitest (`ng test`); `HttpTestingController` o cliente falso | Nada |
| Componentes | `frontend/tests/componentes/` | Vitest, @testing-library/angular, @testing-library/user-event, `HttpTestingController`, jsdom | Nada |
| Arquitectura | `frontend/eslint.config.js` | ESLint (`@typescript-eslint/no-restricted-imports` por carpeta, `no-restricted-globals`) | Nada |
| Aceptación | Lista CAF de este documento | El navegador, a mano | API y frontend corriendo |
| E2E (mejora futura) | `frontend/tests/e2e/` | Playwright | API, frontend y navegadores |

`npm test` corre unitarias y componentes; `npm run lint`, las reglas de arquitectura. Las dos
corren en la CI.

Lecciones del backend que valen igual aquí (`docs/ERRORES_RECURRENTES.md`):
- **Datos que distinguen la respuesta correcta (E-018).** En cada prueba, preguntarse qué vería un
  frontend que ignore ese dato. Por ejemplo: en una búsqueda, la prueba verifica el parámetro
  enviado (`req.request.params`) y responde algo distinto según `buscar`, y verifica lo que se
  muestra; los ids de colaborador y de empresa no coinciden; el error 400 llega en un campo concreto
  y se verifica que aparezca en ese campo y no en otro.
- **Rojo por el motivo correcto (E-014).** Antes de la implementación, cada prueba falla por
  `No implementado` o por su aserción, nunca por un error del Arrange. El informe del tester se
  verifica después de su último cambio.
- **Mutaciones.** El implementador comprueba las pruebas rompiendo a propósito el código (ignorar
  `buscar`, no vaciar la cascada, mostrar el error en otro campo) y viendo que alguna falle.

Convención de nombres, la del backend: `describe` con la unidad o pantalla, e `it` con
`Metodo_Escenario_ResultadoEsperado` (por ejemplo,
`it('cambiarPais_ConDepartamentoElegido_VaciaDepartamentoYMunicipio')`). Estructura
Arrange-Act-Assert.

## Pruebas unitarias
- **Cliente HTTP (`api/`)**: URL base desde `URL_API`; cuerpo JSON y `Content-Type`; parámetros de
  la consulta (`pagina`, `tamanio`, `buscar` sólo si tiene texto); 204 sin cuerpo; cada fila de la
  tabla «Errores» de `docs/frontend/PLAN.md` (400, JSON ilegible, 404, 409, 500, sin conexión);
  normalización de claves (`Nombre` → `nombre`, `Empresas[1].EmpresaId` → `empresas[1].empresaId`,
  prefijo `$.`).
- **`crearListado`**: carga inicial; cambio de página y de tamaño; búsqueda con espera de 300 ms
  (temporizadores falsos de Vitest) que vuelve a la página 1; respuesta vieja descartada; error
  guardado sin romper el listado.
- **`crearCascada`**: carga de países; cambiar país vacía y recarga; cambiar departamento vacía y
  recarga; valores iniciales al editar sin vaciar.
- **`crearFormulario`**: VC bloquea el envío; un 400 pone el mensaje en su control, también dentro
  de un `FormArray`; `dto` se descarta y `$.campo` muestra «El valor no es válido.»; una clave sin
  control y un 409 van a `errorGeneral`; `guardando` mientras espera.
- **`crearEliminacion`**: sin confirmar no elimina; 204 avisa y recarga; 409 avisa y no recarga.
- **Formatos**: fecha `aaaa-mm-dd` ↔ `dd/mm/aaaa`; fecha del calendario ↔ `aaaa-mm-dd` sin correrse
  un día; enum ↔ texto; validadores de VC con valores límite (teléfono de 6, 7, 20 y 21
  caracteres y código ISO de 1, 2 y 3 letras, con `PATRON_TELEFONO` y `PATRON_CODIGO_ISO`; edad
  mínima igual y mayor que la máxima). La zona horaria no la cambian las pruebas: la CI corre con
  `TZ=America/Guatemala` (UTC−6) y las máquinas del equipo ya están en esa zona. En UTC, un
  `new Date('aaaa-mm-dd')` que corre la fecha un día pasaría.

## Pruebas de componentes
Por pantalla, sólo lo propio de ella (lo genérico ya está cubierto por las unitarias):
- Listado: muestra las filas que devuelve la API y el total; «Buscar» consulta con el texto;
  «Eliminar» pide confirmación; un 409 muestra el `detail`.
- Formulario: precarga al editar; 400 muestra el mensaje debajo del campo; guardar vuelve al
  listado con el aviso; 404 muestra «El registro no existe.».
- Específicas: padre deshabilitado al editar (departamento, municipio, país de la empresa); cascada
  en empresas; filas de empresas en el alta de colaborador; «Quitar» deshabilitado con una sola
  empresa en el detalle.

## Pruebas de arquitectura
- **ARQF1.** `contratos/` no importa módulos del proyecto; de bibliotecas, sólo tipos de `rxjs`,
  `@angular/core` y `@angular/forms`.
- **ARQF2.** `api/` sólo importa `contratos/`; `@angular/common/http` no se importa fuera de `api/`
  y `app.config.ts`; `fetch` no se usa.
- **ARQF3.** `vistas/`, `componentes/` y `servicios/` no importan `api/` ni `environments/`.

Se verifican con `npm run lint`; para comprobar que la regla funciona, se introduce a propósito un
import prohibido y se ve fallar el lint (como la verificación por mutación de E-018).

## Aceptación (CAF, a mano contra la API real)
Con la API (`dotnet run --project src/RRHH.Api --launch-profile http`, base con los datos
iniciales) y el frontend (`npm start` en `frontend/`, `http://localhost:4200`). El resultado se
anota en el PR de la fase 12.

1. Crear, buscar, editar y eliminar un país; intentar eliminar Guatemala → aviso RN1 (CAF1, CAF4).
2. Crear un departamento y un municipio nuevos; al editarlos, el padre no se puede cambiar (CAF1).
3. Crear una empresa eligiendo país → departamento → municipio; editarla y cambiar el municipio; el
   país no se puede cambiar (CAF2).
4. Crear un colaborador con dos empresas; ver su edad en el detalle; asociar una tercera, editar la
   fecha de ingreso y quitar una; «Quitar» se deshabilita con una sola (CAF3).
5. Crear un colaborador menor de 18 → mensaje RN4 en «Fecha de nacimiento»; correo repetido →
   aviso RN5 (CAF4).
6. Eliminar una empresa con colaboradores → aviso RN2 (CAF4).
7. Detener la API y recargar un listado → «No se pudo conectar con la API.» (CAF4).
8. Abrir `/paises/9999/editar` → «El registro no existe.» (CAF4).

## Trazabilidad: requisito → prueba
U = unitaria · C = componentes · R = arquitectura · A = aceptación a mano · I = integración del backend

La columna «Pruebas» la completa el tester en cada tarea, con el nombre de cada prueba.

| Id | Qué se verifica | Nivel | Pruebas |
|---|---|---|---|
| RF1 | Menú y página de inicio | C, A | |
| RF2 | Listado paginado por la API, búsqueda con espera | U, C, A | Listado_cargaInicial_PidePagina1YTamanioOpciones, Listado_cambiarPagina_PideNuevaPagina, Listado_cambiarTamanio_VuelveAPagina1YPideNuevoTamanio, Listado_cambiarBusqueda_Espera300msYVuelveAPagina1, Listado_respuestasMismaBusqueda_DescartaRespuestaVieja, ClienteRrhhHttp_listar_ConBusqueda_EnviaPaginaTamanioYBuscar, ClienteRrhhHttp_listar_SinBuscar_NoEnviaBuscar |
| RF3 | Confirmación al eliminar; 409 visible | U, C, A | Eliminacion_eliminar_SinConfirmar_NoElimina, Eliminacion_eliminar_409_AvisaYNoRecarga |
| RF4 | Avisos de éxito y regreso al listado | U, C | Formulario_enviar_Exito_AvisaYNavega, Formulario_enviar_MientrasEspera_GuardandoEsTrue, Eliminacion_eliminar_204_AvisaYRecarga, ClienteRrhhHttp_eliminar_204_NoDevuelveCuerpo, ClienteRrhhHttp_crear_CuerpoJsonYContentType_PoneEncabezado |
| RF5 | Errores 400 debajo de cada campo, también con índice; JSON ilegible | U, C, A | Formulario_enviar_Error400_PoneMensajeEnControlYFormArray, Formulario_enviar_Error400JsonIlegible_MuestraElValorNoEsValidoYDescartaDto, Formulario_enviar_ErrorSinControl_VaAErrorGeneral, erroresInterceptor_interceptor_Error400_NormalizaClavesYDevuelveErrorApi, erroresInterceptor_interceptor_Error400JsonIlegible_LimpiaClavesYMensaje |
| RF6 | 409 como aviso general | U, C, A | Formulario_enviar_Error409_VaAErrorGeneral, erroresInterceptor_interceptor_Error409_DevuelveErrorApiConDetalle |
| RF7 | 404 al editar o ver | U, C, A | erroresInterceptor_interceptor_Error404_DevuelveErrorApiConDetalle, Formatos_mensajeDeError_SegunEstado_MuestraTextoCorrecto |
| RF8 | Sin conexión y error no previsto | U, A | erroresInterceptor_interceptor_Error500_DevuelveErrorApiSoloConEstado, erroresInterceptor_interceptor_SinConexion_DevuelveErrorApiConEstado0, Listado_error_GuardaErrorSinRomperListado |
| RF9 | Países: valores propuestos, código ISO en mayúsculas | C, A | |
| RF10 | Departamentos: país fijo al editar | C, A | |
| RF11 | Municipios: cascada al crear, padres fijos al editar | C, A | Cascada_cargaInicial_SinInicial_CargaPaisesYVaciaSeleccion, Cascada_cargaInicial_ConInicial_CargaPaisesDepartamentosYMunicipiosSinVaciar, Cascada_elegirPais_CambiaPaisVaciaHijosYCargaDepartamentos, Cascada_elegirDepartamento_CambiaDepartamentoVaciaMunicipioYCargaMunicipios, Cascada_elegirMunicipio_CambiaMunicipio |
| RF12 | Empresas: cascada, país fijo al editar | U, C, A | Cascada_cargaInicial_SinInicial_CargaPaisesYVaciaSeleccion, Cascada_cargaInicial_ConInicial_CargaPaisesDepartamentosYMunicipiosSinVaciar, Cascada_elegirPais_CambiaPaisVaciaHijosYCargaDepartamentos, Cascada_elegirDepartamento_CambiaDepartamentoVaciaMunicipioYCargaMunicipios, Cascada_elegirMunicipio_CambiaMunicipio |
| RF13 | Alta de colaborador con varias empresas; editar sólo datos personales | C, A | |
| RF14 | Detalle: edad y empresas; asociar, editar, quitar | C, A | |
| RF15 | Colaboradores de una empresa | C, A | |
| VC1–VC5 | Validaciones en el navegador | U, C | Formulario_enviar_Invalido_BloqueaEnvioYMarcaControles, Formatos_mensajeDeValidacion_SegunError_MuestraTextoCorrecto, ValidadoresLimite_edadMinimaNoMayorQueMaxima_Igual_EsValido, ValidadoresLimite_edadMinimaNoMayorQueMaxima_Mayor_EsInvalido, ValidadoresLimite_patronTelefono_ValoresLimite, ValidadoresLimite_patronCodigoIso_ValoresLimite |
| RNF1 | URL de la API por configuración | U | ClienteRrhhHttp_listar_ConUrlBase_UsaTokenUrlApi |
| RNF2 | Formatos de fecha (sin correrse un día) y enum | U | Formatos_fechaParaMostrar_ConvierteAFormatoLocal, Formatos_fechaDesdeIso_CreaFechaLocalSinCorrerseUnDia, Formatos_fechaAIso_ConvierteFechaLocalAFormatoIsoSinCorrerseUnDia, Formatos_textoRegla29Febrero_MuestraTextoCorrecto |
| CORS1 | Preflight con origen permitido y no permitido; métodos, Content-Type y Location | I (backend) | Preflight_OrigenPermitido_DevuelveAccessControlAllowOrigin, Preflight_OrigenNoPermitido_NoDevuelveAccessControlAllowOrigin, Preflight_MetodosModificacionConContentType_DevuelveOrigenMetodoYEncabezadosPermitidos, Get_ConOrigenPermitido_ExponeEncabezadoLocation |
| ARQF1–3 | Reglas de dependencia | R | npm run lint (frontend/eslint.config.js) |
| CAF1–4 | Aceptación | A | Lista de aceptación, puntos 1 a 8 |
| CAF5 | Pruebas en verde | U, C | `npm test` en la CI |
| CAF6 | Arquitectura | R | `npm run lint` en la CI |
| CAF7 | Git | — | Ramas por fase, PR con plantilla, CI en verde, tag v1.1.0 |
| CAF8 | README | — | Sección «Frontend Angular» |

CORS1 se prueba en `tests/RRHH.IntegrationTests` (xUnit, `WebApplicationFactory`): un `OPTIONS`
con `Origin: http://localhost:4200` devuelve `Access-Control-Allow-Origin`; con otro origen, no lo
devuelve. El preflight de POST, PUT y DELETE con `Content-Type` los permite, y un GET desde el
origen permitido expone `Location`: sin esas pruebas, quitar el encabezado, un método o `Location`
de la política no hacía fallar ninguna (se verificó con mutaciones).

## Criterio de terminado de cada tarea
- `npm run lint`, `npm run format:check`, `npm test` y `npm run build` sin errores.
- Los requisitos de la tarea tienen su prueba y figuran en la tabla de trazabilidad.
- Si se tocó la API: `dotnet build` y `dotnet test` en verde, y Postman sigue pasando.

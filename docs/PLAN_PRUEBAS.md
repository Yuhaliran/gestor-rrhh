# Plan de pruebas

## Modelo de proceso: V con implementación incremental
Los requisitos son fijos y claros, y el riesgo principal es entregar algo que no cumpla
un criterio. Por eso se sigue el **modelo en V**: cada nivel de especificación tiene su nivel
de prueba, diseñado al mismo tiempo que la especificación. La implementación es
**incremental** (entidad por entidad, cada una terminada y probada), para que lo entregado
funcione en cualquier punto.

```
Requisitos (ESPECIFICACION.md) ◄──────────────► Aceptación: Postman/Newman, E2E (opcional)
   Diseño (PLAN.md) ◄─────────────────────► Integración: API con WebApplicationFactory
      Arquitectura ◄────────────────────► Arquitectura: dependencias entre capas
         Módulos ◄───────────────────► Unitarias: dominio, validaciones, servicios
                        Código
```

## Independencia de las pruebas
Las pruebas se escriben **desde la especificación y antes de la implementación**, por un
agente con un modelo de otro proveedor (rol tester), sin leer la implementación. Así las
pruebas no heredan los errores de interpretación del código. Detalle en `docs/AGENTES.md`.

## Proyectos y niveles
| Nivel | Proyecto | Herramientas | Qué necesita para correr |
|---|---|---|---|
| Unitarias | tests/RRHH.UnitTests | xUnit, SQLite en memoria | Nada |
| Integración | tests/RRHH.IntegrationTests | xUnit, WebApplicationFactory, SQLite en memoria | Nada |
| Arquitectura | tests/RRHH.ArchitectureTests | xUnit, NetArchTest | Nada |
| Migraciones | comando de EF + prueba opcional | dotnet-ef, SQL Server | SQL Server sólo para la prueba opcional |
| Aceptación de la API | postman/ | Postman, Newman | La API corriendo |
| E2E (opcional) | tests/RRHH.E2ETests | xUnit, Playwright | API, web y navegadores instalados |

`dotnet test` corre unitarias, integración y arquitectura. Las E2E y la prueba de migraciones
sobre SQL Server están marcadas con `[Trait("Categoria", "E2E")]` y `[Trait("Categoria", "SqlServer")]`
y se excluyen por defecto:

```
dotnet test --filter "Categoria!=E2E&Categoria!=SqlServer"    # lo de siempre
dotnet test --filter "Categoria=E2E"                           # con la app corriendo
```

Convención de nombres: `Metodo_Escenario_ResultadoEsperado`, con estructura Arrange-Act-Assert.

## Nota sobre SQLite
SQLite en memoria respeta claves foráneas y restricciones únicas, pero no es idéntico a
SQL Server (tipos, intercalación, algunas funciones). Por eso las migraciones reales se
verifican aparte contra SQL Server (MIG2).

## Trazabilidad: requisito → prueba
U = unitaria · I = integración · A = aceptación (Postman) · E = E2E · R = arquitectura · M = migraciones

| Id | Qué se verifica | Nivel | Pruebas |
|---|---|---|---|
| CA1 | Detalle de empresa con país, departamento y municipio | I, A | `GetEmpresa_Existente_DevuelveGeografiaCompleta`; Postman «Empresas / Obtener» |
| CA2 | Colaborador con empresas, edad, teléfono y correo | U, I, A | `Calcular_CumpleaniosHoyManianaYAyer_DevuelveEdadSegunSiYaCumplio`, `Calcular_Nacido29FebreroEnAnioNoBisiesto_Regla28Febrero_CumpleEl28Febrero`, `Calcular_Nacido29FebreroEnAnioNoBisiesto_Regla1Marzo_CumpleEl1Marzo`, `Calcular_Nacido29FebreroEnAnioBisiesto_CumpleEl29FebreroIndependientementeDeRegla`, `Calcular_FechaNacimientoIgualAHoy_DevuelveCero`, `Calcular_FechaNacimientoPosteriorAHoy_DevuelveEdadNegativa`; `GetColaborador_Existente_DevuelveEdadYEmpresas` |
| CA3 | Un colaborador en varias empresas | I, A, E | `AsociarEmpresa_SegundaEmpresa_QuedaConDos`; Postman «Colaboradores / Asociar empresa» |
| CA4 | Migraciones completas y aplicables | M | MIG1, MIG2, `Semilla_Pais_CargaGuatemalaConValoresPorDefecto`, `Semilla_Departamentos_Carga22DepartamentosYCabeceras`, `Semilla_Departamentos_CargaExactamente22`, `Semilla_NoCargaEmpresasNiColaboradores` |
| CA5 | Pruebas unitarias correctas | U | Todo RRHH.UnitTests; cobertura con coverlet |
| CA6 | La arquitectura se respeta | R | `VerificarDependencias_DomainYContratos_NoDependenDeOtrosProyectos`, `VerificarDependencias_Application_NoDependeDeInfraestructuraApiNiWeb`, `VerificarDependencias_Web_SoloDependeDeContratos`, `VerificarDependencias_ControladoresApi_NoUsanTiposDeDominio` |
| CA7 | Todos los servicios en Postman | A | Cada endpoint del PLAN.md tiene su request con pruebas |
| CA8 | Git | — | Historial por ramas, un PR por fase con la plantilla, Conventional Commits, tag v1.0.0 |
| RN1 | No borrar geografía con dependencias | U, I | `Eliminar_PaisConDepartamentos_Conflicto` (y equivalentes por nivel), `Eliminar_PaisConDepartamentos_LanzaExcepcion`, `Eliminar_DepartamentoConMunicipios_LanzaExcepcion`, `Eliminar_MunicipioConEmpresas_LanzaExcepcion` |
| RN2 | No borrar empresa con colaboradores | U, I | `Eliminar_EmpresaConColaboradores_Conflicto`, `Eliminar_EmpresaConColaboradores_LanzaExcepcion` |
| RN3 | Al menos una empresa | U, I | `Crear_ColaboradorSinEmpresas_Error`; `QuitarEmpresa_UltimaEmpresa_Conflicto` |
| RN4 | Edad dentro del rango del país de cada empresa (por defecto 18 a 100) | U | Valores límite contra el rango del país (mínima − 1, mínima, máxima, máxima + 1) y colaborador en dos países con rangos distintos |
| RN5 | Sin duplicados (NIT por país) | U, I | `Crear_NitDuplicadoEnMismoPais_Conflicto`, `Crear_NitRepetidoEnOtroPais_Creada`, `Crear_CorreoDuplicado_Conflicto`, `AsociarEmpresa_YaAsociada_Conflicto`, `Insertar_PaisMismoNombre_LanzaExcepcion`, `Insertar_PaisMismoCodigo_LanzaExcepcion`, `Insertar_DepartamentoMismoNombreEnPais_LanzaExcepcion`, `Insertar_MunicipioMismoNombreEnDepartamento_LanzaExcepcion`, `Insertar_MunicipioMismoNombreEnOtroDepartamento_Permitido`, `Insertar_ColaboradorMismoCorreo_LanzaExcepcion` |
| RN6 | Geografía en cascada | I, E | `GetMunicipiosDeDepartamento_*`; E2E «Crear empresa eligiendo geografía» |
| RN7 | Cálculo de edad; 29 de febrero según el país | U | `Calcular_CumpleaniosHoyManianaYAyer_DevuelveEdadSegunSiYaCumplio`, `Calcular_Nacido29FebreroEnAnioNoBisiesto_Regla28Febrero_CumpleEl28Febrero`, `Calcular_Nacido29FebreroEnAnioNoBisiesto_Regla1Marzo_CumpleEl1Marzo`, `Calcular_Nacido29FebreroEnAnioBisiesto_CumpleEl29FebreroIndependientementeDeRegla`, `Calcular_FechaNacimientoIgualAHoy_DevuelveCero`, `Calcular_FechaNacimientoPosteriorAHoy_DevuelveEdadNegativa` |
| V1–V4 | Validaciones de entrada | U, I | Pruebas de validadores con `[Theory]`; `Post*_DatosInvalidos_Devuelve400ConDetalle`, `Modelo_Campos_SonObligatoriosSalvoPuesto`, `Modelo_Campos_TienenLargosMaximos`, `Insertar_PaisEdadMinimaNegativa_LanzaExcepcion`, `Insertar_PaisEdadMinimaMayorQueMaxima_LanzaExcepcion`, `Insertar_PaisEdadMinimaIgualAMaxima_Permitido` |

## Pruebas de dominio y validaciones (unitarias)
- **Cálculo de edad (RN7)**, con casos límite: cumpleaños hoy, mañana, ayer; nacidos el 29 de
  febrero en años no bisiestos, con las dos reglas (28 de febrero y 1 de marzo), y en años
  bisiestos; fecha de nacimiento igual a hoy (0) y posterior a hoy (negativa).
  El cálculo recibe la fecha actual como parámetro (o un `TimeProvider`) para que las
  pruebas no dependan del día en que se corren.
- **Validadores** con `[Theory]` e `[InlineData]` para cada campo.
- **Servicios** contra SQLite en memoria: reglas RN1 a RN5.

## Pruebas de arquitectura
- **ARQ1.** RRHH.Domain y RRHH.Contratos no dependen de ningún otro proyecto de la solución.
- **ARQ2.** RRHH.Application no depende de RRHH.Infrastructure, RRHH.Api ni RRHH.Web.
- **ARQ3.** RRHH.Web sólo depende de RRHH.Contratos (habla con la API por HTTP).
- **ARQ4.** Los controladores de la API no usan tipos de RRHH.Domain ni de RRHH.Infrastructure, ni
  Entity Framework Core (ni en sus respuestas ni en su código): trabajan con los DTOs de
  RRHH.Contratos y los servicios de RRHH.Application.

## Migraciones
- **MIG1.** `dotnet ef migrations has-pending-model-changes -p src/RRHH.Infrastructure -s src/RRHH.Api`
  no informa cambios pendientes (no necesita base de datos). Corre en la integración continua.
- **MIG2** (opcional, categoría SqlServer). Aplicar todas las migraciones sobre una base
  vacía de LocalDB y verificar que se crean las tablas y los datos iniciales.

## Aceptación con Postman
- Un request por endpoint, agrupado por entidad, con variables de entorno (`{{baseUrl}}`).
- Cada request tiene pruebas: código de estado, estructura de la respuesta y, en los
  casos de error, el `ProblemDetails` esperado.
- Los requests guardan ids en variables (el país creado se usa para crear el departamento).
- Ejecución de toda la colección:
  `newman run postman/RRHH.postman_collection.json -e postman/local.postman_environment.json`

## E2E (opcional, Playwright)
Sólo flujos clave, no toda la interfaz:
1. Crear una empresa eligiendo país, departamento y municipio en cascada.
2. Crear un colaborador asociado a dos empresas y ver su edad en el detalle.
3. Intentar eliminar una empresa con colaboradores y ver el mensaje de error.

## Criterio de terminado de cada tarea
- Compila sin advertencias nuevas.
- `dotnet test` en verde.
- Las reglas nuevas tienen su prueba y figuran en la tabla de trazabilidad.
- Postman actualizado si se agregó o cambió un endpoint.

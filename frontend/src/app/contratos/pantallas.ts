import type { Signal } from '@angular/core';
import type { FormGroup, ValidationErrors } from '@angular/forms';
import type { Observable } from 'rxjs';

import type {
  Consulta,
  DepartamentoDto,
  MunicipioDto,
  Pagina,
  PaisDto,
  Regla29Febrero,
} from './dtos';
import type { ErrorApi } from './errores';

// Contrato de la lógica de pantallas (docs/frontend/PLAN.md, «Lógica de las pantallas»). Cada tipo
// lo implementa una constante de servicios/ con el nombre en minúscula:
//   servicios/listado.ts      crearListado
//   servicios/cascada.ts      crearCascada
//   servicios/formulario.ts   crearFormulario
//   servicios/eliminacion.ts  crearEliminacion
//   servicios/formatos.ts     fechaParaMostrar, fechaDesdeIso, fechaAIso, textoRegla29Febrero,
//                             mensajeDeError, mensajeDeValidacion, OPCIONES_REGLA_29_FEBRERO
//                             (OpcionRegla[]), PATRON_TELEFONO y PATRON_CODIGO_ISO (RegExp, para
//                             Validators.pattern) y edadMinimaNoMayorQueMaxima (ValidatorFn del grupo
//                             de País: { edadMinimaMayorQueMaxima: true } si la mínima supera a la
//                             máxima, VC4)
// Las funciones crear* usan inject(): se llaman al inicializar un componente (o, en las pruebas,
// dentro de TestBed.runInInjectionContext). El cliente lo toman de CLIENTE_RRHH (servicios/cliente.ts).

// RF2 · Listado paginado por la API
export interface Listado<T> {
  readonly elementos: Signal<T[]>;
  readonly total: Signal<number>;
  readonly pagina: Signal<number>;
  // 10, 20 (por defecto) o 50
  readonly tamanio: Signal<number>;
  readonly buscar: Signal<string>;
  readonly cargando: Signal<boolean>;
  // El error de la última consulta; los elementos anteriores se conservan
  readonly error: Signal<ErrorApi | null>;
  cambiarPagina(pagina: number): void;
  // Vuelve a la página 1
  cambiarTamanio(tamanio: number): void;
  // Espera 300 ms sin cambios, vuelve a la página 1 y consulta
  cambiarBusqueda(texto: string): void;
  recargar(): void;
}

// Carga la página 1 al crearse. Una respuesta vieja que llega después de una nueva se descarta.
export type CrearListado = <T>(
  cargar: (consulta: Consulta) => Observable<Pagina<T>>,
  opciones?: { tamanio?: number },
) => Listado<T>;

// RN6, RF11, RF12 · Geografía en cascada
export interface SeleccionGeografica {
  paisId: number | null;
  departamentoId: number | null;
  municipioId: number | null;
}

export interface Cascada {
  readonly paises: Signal<PaisDto[]>;
  readonly departamentos: Signal<DepartamentoDto[]>;
  readonly municipios: Signal<MunicipioDto[]>;
  readonly seleccion: Signal<SeleccionGeografica>;
  readonly error: Signal<ErrorApi | null>;
  // Vacía departamento y municipio y carga los departamentos del país (ninguno si es null)
  elegirPais(paisId: number | null): void;
  // Vacía el municipio y carga los municipios del departamento (ninguno si es null)
  elegirDepartamento(departamentoId: number | null): void;
  elegirMunicipio(municipioId: number | null): void;
}

// Carga los países (página 1, tamanio=100) al crearse. Con inicial (editar) carga también los
// departamentos y municipios de esa selección, sin vaciarla.
export type CrearCascada = (inicial?: SeleccionGeografica) => Cascada;

// RF4 a RF8 y VC · Formulario
export interface Formulario {
  // RF5 (claves sin control), RF6, RF7 y RF8
  readonly errorGeneral: Signal<string | null>;
  readonly guardando: Signal<boolean>;
  enviar(): void;
}

// enviar(): si el grupo es inválido (VC), marca los controles y no llama a la API. Al guardar,
// avisa «Se guardó correctamente.» y navega a opciones.volverA (RF4). Un 400 pone cada mensaje en
// su control con setErrors({ api: mensaje }), también dentro de un FormArray
// ('empresas[1].fechaIngreso'); una clave sin control y cualquier otro error van a errorGeneral.
export type CrearFormulario = <TDatos, TRespuesta>(
  grupo: FormGroup,
  guardar: (datos: TDatos) => Observable<TRespuesta>,
  opciones: { volverA: string },
) => Formulario;

// RF3, RF4 · Eliminación con confirmación
export interface Eliminacion<T> {
  readonly eliminando: Signal<boolean>;
  // Pide confirmación (botones «Sí, eliminar» y «Cancelar»); si se confirma, elimina
  eliminar(elemento: T): void;
}

// Al eliminar (204) avisa «Se eliminó correctamente.» y llama a alTerminar. Un error se avisa
// con mensajeDeError (un 409, con su detalle) y no llama a alTerminar.
export type CrearEliminacion = <T>(
  eliminar: (elemento: T) => Observable<void>,
  alTerminar: () => void,
) => Eliminacion<T>;

// RNF2 · Formatos
// 'aaaa-mm-dd' → 'dd/mm/aaaa'
export type FechaParaMostrar = (iso: string) => string;
// 'aaaa-mm-dd' → fecha a la medianoche local (sin correrse un día en UTC−6)
export type FechaDesdeIso = (iso: string) => Date;
// Fecha del calendario → 'aaaa-mm-dd', con el año, mes y día locales
export type FechaAIso = (fecha: Date) => string;
// 'VeintiochoDeFebrero' → «28 de febrero»; 'PrimeroDeMarzo' → «1 de marzo»
export type TextoRegla29Febrero = (regla: Regla29Febrero) => string;

export interface OpcionRegla {
  valor: Regla29Febrero;
  texto: string;
}

// RF6 a RF8 · Texto de un error que no es de un campo: 409 → su detalle; 404 → «El registro no
// existe.»; 0 → «No se pudo conectar con la API.»; cualquier otro → «Ocurrió un error inesperado.»
export type MensajeDeError = (error: ErrorApi) => string;

// VC y RF5 · Mensaje de los errores de un control, o null si no tiene: required → «Este campo es
// obligatorio.»; maxlength → «Admite hasta N caracteres.»; pattern y email → «El formato no es
// válido.»; min → «No puede ser negativa.»; edadMinimaMayorQueMaxima → «La edad mínima no puede
// ser mayor que la máxima.»; api → el mensaje de la API.
export type MensajeDeValidacion = (errores: ValidationErrors | null) => string | null;

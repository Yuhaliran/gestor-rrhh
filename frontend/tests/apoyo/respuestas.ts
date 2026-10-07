import { Pagina, PaisDto, DepartamentoDto, MunicipioDto } from '../../src/app/contratos/dtos';
import { ProblemDetails } from '../../src/app/contratos/errores';

export function paginaDe<T>(elementos: T[]): Pagina<T> {
  return {
    elementos,
    total: elementos.length,
    numero: 1,
    tamanio: 20,
    totalPaginas: 1,
  };
}

export function conflicto(detalle: string): ProblemDetails {
  return {
    status: 409,
    detail: detalle,
  };
}

export function validacion(errores: Record<string, string[]>): ProblemDetails {
  return {
    status: 400,
    errors: errores,
  };
}

export function jsonIlegible(campo: string): ProblemDetails {
  return {
    status: 400,
    errors: {
      ['$.' + campo]: ['The JSON value could not be converted...'],
      dto: ['The dto field is required.'],
    },
  };
}

export function noEncontrado(): ProblemDetails {
  return {
    status: 404,
    detail: 'No existe.',
  };
}

export const guatemala: PaisDto = {
  id: 1,
  nombre: 'Guatemala',
  codigoIso2: 'GT',
  edadMinima: 18,
  edadMaxima: 100,
  regla29Febrero: 'VeintiochoDeFebrero',
};

export const guatemalaDepto: DepartamentoDto = {
  id: 1,
  nombre: 'Guatemala',
  paisId: 1,
  paisNombre: 'Guatemala',
};

export const guatemalaMuni: MunicipioDto = {
  id: 1,
  nombre: 'Guatemala',
  departamentoId: 1,
  departamentoNombre: 'Guatemala',
  paisId: 1,
  paisNombre: 'Guatemala',
};

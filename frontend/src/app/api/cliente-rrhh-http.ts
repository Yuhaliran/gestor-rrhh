import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';

import type { ClienteRrhh, Mantenimiento } from '../contratos/cliente';
import type {
  ColaboradorDto,
  Consulta,
  CrearColaboradorDto,
  DepartamentoDto,
  EmpresaDto,
  GuardarColaboradorDto,
  GuardarDepartamentoDto,
  GuardarEmpresaDto,
  GuardarMunicipioDto,
  GuardarPaisDto,
  MunicipioDto,
  Pagina,
  PaisDto,
} from '../contratos/dtos';
import { URL_API } from './url-api';

const JSON_ENCABEZADOS = new HttpHeaders({ 'Content-Type': 'application/json' });

// Cliente de la API con HttpClient (docs/frontend/PLAN.md, «Contrato del cliente»). Los errores los
// traduce el interceptor (errores.interceptor.ts): acá no se manejan.
@Injectable()
export class ClienteRrhhHttp implements ClienteRrhh {
  private readonly http = inject(HttpClient);
  private readonly url = inject(URL_API).replace(/\/+$/, '');

  readonly paises: ClienteRrhh['paises'] = {
    ...this.mantenimiento<PaisDto, GuardarPaisDto>('paises'),
    departamentos: (paisId) =>
      this.http.get<DepartamentoDto[]>(this.ruta(`paises/${paisId}/departamentos`)),
  };

  readonly departamentos: ClienteRrhh['departamentos'] = {
    ...this.mantenimiento<DepartamentoDto, GuardarDepartamentoDto>('departamentos'),
    municipios: (departamentoId) =>
      this.http.get<MunicipioDto[]>(this.ruta(`departamentos/${departamentoId}/municipios`)),
  };

  readonly municipios: ClienteRrhh['municipios'] = this.mantenimiento<
    MunicipioDto,
    GuardarMunicipioDto
  >('municipios');

  readonly empresas: ClienteRrhh['empresas'] = {
    ...this.mantenimiento<EmpresaDto, GuardarEmpresaDto>('empresas'),
    colaboradores: (empresaId, consulta) =>
      this.http.get<Pagina<ColaboradorDto>>(this.ruta(`empresas/${empresaId}/colaboradores`), {
        params: parametros(consulta),
      }),
  };

  readonly colaboradores: ClienteRrhh['colaboradores'] = {
    ...this.mantenimiento<ColaboradorDto, GuardarColaboradorDto>('colaboradores'),
    crear: (datos: CrearColaboradorDto) =>
      this.http.post<ColaboradorDto>(this.ruta('colaboradores'), datos, {
        headers: JSON_ENCABEZADOS,
      }),
    asociarEmpresa: (id, datos) =>
      this.http.post<ColaboradorDto>(this.ruta(`colaboradores/${id}/empresas`), datos, {
        headers: JSON_ENCABEZADOS,
      }),
    actualizarEmpresa: (id, empresaId, datos) =>
      this.http.put<ColaboradorDto>(this.ruta(`colaboradores/${id}/empresas/${empresaId}`), datos, {
        headers: JSON_ENCABEZADOS,
      }),
    quitarEmpresa: (id, empresaId) =>
      this.http.delete<void>(this.ruta(`colaboradores/${id}/empresas/${empresaId}`)),
  };

  // Las cinco operaciones comunes de /api/<recurso>
  private mantenimiento<TDto, TGuardar>(recurso: string): Mantenimiento<TDto, TGuardar> {
    return {
      listar: (consulta) =>
        this.http.get<Pagina<TDto>>(this.ruta(recurso), { params: parametros(consulta) }),
      obtener: (id) => this.http.get<TDto>(this.ruta(`${recurso}/${id}`)),
      crear: (datos) =>
        this.http.post<TDto>(this.ruta(recurso), datos, { headers: JSON_ENCABEZADOS }),
      actualizar: (id, datos) =>
        this.http.put<TDto>(this.ruta(`${recurso}/${id}`), datos, { headers: JSON_ENCABEZADOS }),
      eliminar: (id) => this.http.delete<void>(this.ruta(`${recurso}/${id}`)),
    };
  }

  private ruta(recurso: string): string {
    return `${this.url}/api/${recurso}`;
  }
}

// ?pagina&tamanio&buscar; buscar sólo viaja si tiene texto
function parametros(consulta: Consulta): HttpParams {
  let parametros = new HttpParams().set('pagina', consulta.pagina).set('tamanio', consulta.tamanio);
  const buscar = consulta.buscar?.trim();
  if (buscar) {
    parametros = parametros.set('buscar', buscar);
  }
  return parametros;
}

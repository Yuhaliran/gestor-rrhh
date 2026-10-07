import type { Observable } from 'rxjs';

import type {
  AsociarEmpresaDto,
  ColaboradorDto,
  Consulta,
  CrearColaboradorDto,
  DepartamentoDto,
  EmpresaDto,
  GuardarColaboradorDto,
  GuardarDepartamentoDto,
  GuardarEmpresaColaboradorDto,
  GuardarEmpresaDto,
  GuardarMunicipioDto,
  GuardarPaisDto,
  MunicipioDto,
  Pagina,
  PaisDto,
} from './dtos';

// Cliente de la API (docs/frontend/PLAN.md, «Contrato del cliente»; endpoints en docs/PLAN.md,
// «API»). Toda respuesta no exitosa termina el Observable con un ErrorApi.

// Las operaciones comunes de un mantenimiento
export interface Mantenimiento<TDto, TGuardar> {
  // GET /api/<recurso>?pagina&tamanio&buscar
  listar(consulta: Consulta): Observable<Pagina<TDto>>;
  // GET /api/<recurso>/{id}
  obtener(id: number): Observable<TDto>;
  // POST /api/<recurso> → 201
  crear(datos: TGuardar): Observable<TDto>;
  // PUT /api/<recurso>/{id} → 200
  actualizar(id: number, datos: TGuardar): Observable<TDto>;
  // DELETE /api/<recurso>/{id} → 204
  eliminar(id: number): Observable<void>;
}

export interface ClienteRrhh {
  paises: Mantenimiento<PaisDto, GuardarPaisDto> & {
    // GET /api/paises/{paisId}/departamentos
    departamentos(paisId: number): Observable<DepartamentoDto[]>;
  };
  departamentos: Mantenimiento<DepartamentoDto, GuardarDepartamentoDto> & {
    // GET /api/departamentos/{departamentoId}/municipios
    municipios(departamentoId: number): Observable<MunicipioDto[]>;
  };
  municipios: Mantenimiento<MunicipioDto, GuardarMunicipioDto>;
  empresas: Mantenimiento<EmpresaDto, GuardarEmpresaDto> & {
    // GET /api/empresas/{empresaId}/colaboradores?pagina&tamanio&buscar
    colaboradores(empresaId: number, consulta: Consulta): Observable<Pagina<ColaboradorDto>>;
  };
  colaboradores: Omit<Mantenimiento<ColaboradorDto, GuardarColaboradorDto>, 'crear'> & {
    // POST /api/colaboradores → 201, con sus empresas (RN3)
    crear(datos: CrearColaboradorDto): Observable<ColaboradorDto>;
    // POST /api/colaboradores/{id}/empresas → 200
    asociarEmpresa(id: number, datos: AsociarEmpresaDto): Observable<ColaboradorDto>;
    // PUT /api/colaboradores/{id}/empresas/{empresaId} → 200
    actualizarEmpresa(
      id: number,
      empresaId: number,
      datos: GuardarEmpresaColaboradorDto,
    ): Observable<ColaboradorDto>;
    // DELETE /api/colaboradores/{id}/empresas/{empresaId} → 204
    quitarEmpresa(id: number, empresaId: number): Observable<void>;
  };
}

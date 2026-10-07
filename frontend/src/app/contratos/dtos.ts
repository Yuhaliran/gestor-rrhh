// Tipos de los DTOs de la API, uno por record de src/RRHH.Contratos y con el mismo nombre
// (docs/frontend/PLAN.md, «Contrato del cliente»). Propiedades en camelCase, como las serializa
// la API; fechas como 'aaaa-mm-dd'; enums como texto.

// src/RRHH.Contratos/Comun/Consulta.cs
export interface Consulta {
  pagina: number;
  tamanio: number;
  buscar?: string;
}

// src/RRHH.Contratos/Comun/Pagina.cs (totalPaginas es una propiedad calculada del record)
export interface Pagina<T> {
  elementos: T[];
  total: number;
  numero: number;
  tamanio: number;
  totalPaginas: number;
}

// src/RRHH.Contratos/Paises/Regla29Febrero.cs
export type Regla29Febrero = 'VeintiochoDeFebrero' | 'PrimeroDeMarzo';

// src/RRHH.Contratos/Paises/PaisDto.cs
export interface PaisDto {
  id: number;
  nombre: string;
  codigoIso2: string;
  edadMinima: number;
  edadMaxima: number;
  regla29Febrero: Regla29Febrero;
}

// src/RRHH.Contratos/Paises/GuardarPaisDto.cs
export interface GuardarPaisDto {
  nombre: string;
  codigoIso2: string;
  edadMinima: number;
  edadMaxima: number;
  regla29Febrero: Regla29Febrero;
}

// src/RRHH.Contratos/Departamentos/DepartamentoDto.cs
export interface DepartamentoDto {
  id: number;
  nombre: string;
  paisId: number;
  paisNombre: string;
}

// src/RRHH.Contratos/Departamentos/GuardarDepartamentoDto.cs
export interface GuardarDepartamentoDto {
  paisId: number;
  nombre: string;
}

// src/RRHH.Contratos/Municipios/MunicipioDto.cs
export interface MunicipioDto {
  id: number;
  nombre: string;
  departamentoId: number;
  departamentoNombre: string;
  paisId: number;
  paisNombre: string;
}

// src/RRHH.Contratos/Municipios/GuardarMunicipioDto.cs
export interface GuardarMunicipioDto {
  departamentoId: number;
  nombre: string;
}

// src/RRHH.Contratos/Empresas/EmpresaDto.cs
export interface EmpresaDto {
  id: number;
  nit: string;
  razonSocial: string;
  nombreComercial: string;
  telefono: string;
  correo: string;
  municipioId: number;
  municipioNombre: string;
  departamentoId: number;
  departamentoNombre: string;
  paisId: number;
  paisNombre: string;
}

// src/RRHH.Contratos/Empresas/GuardarEmpresaDto.cs
export interface GuardarEmpresaDto {
  municipioId: number;
  nit: string;
  razonSocial: string;
  nombreComercial: string;
  telefono: string;
  correo: string;
}

// src/RRHH.Contratos/Colaboradores/EmpresaColaboradorDto.cs
export interface EmpresaColaboradorDto {
  empresaId: number;
  nombreComercial: string;
  paisId: number;
  paisNombre: string;
  fechaIngreso: string;
  puesto: string | null;
}

// src/RRHH.Contratos/Colaboradores/ColaboradorDto.cs (edad calculada por la API, RN7)
export interface ColaboradorDto {
  id: number;
  nombreCompleto: string;
  fechaNacimiento: string;
  edad: number;
  telefono: string;
  correo: string;
  empresas: EmpresaColaboradorDto[];
}

// src/RRHH.Contratos/Colaboradores/GuardarColaboradorDto.cs
export interface GuardarColaboradorDto {
  nombreCompleto: string;
  fechaNacimiento: string;
  telefono: string;
  correo: string;
}

// src/RRHH.Contratos/Colaboradores/AsociarEmpresaDto.cs
export interface AsociarEmpresaDto {
  empresaId: number;
  fechaIngreso: string;
  puesto?: string | null;
}

// src/RRHH.Contratos/Colaboradores/CrearColaboradorDto.cs (el alta incluye las empresas, RN3)
export interface CrearColaboradorDto extends GuardarColaboradorDto {
  empresas: AsociarEmpresaDto[];
}

// src/RRHH.Contratos/Colaboradores/GuardarEmpresaColaboradorDto.cs
export interface GuardarEmpresaColaboradorDto {
  fechaIngreso: string;
  puesto?: string | null;
}

import { Routes } from '@angular/router';

import { ColaboradorAlta } from './vistas/colaboradores/colaborador-alta';
import { ColaboradorEditar } from './vistas/colaboradores/colaborador-editar';
import { ColaboradoresListado } from './vistas/colaboradores/colaboradores-listado';
import { DepartamentoFormulario } from './vistas/departamentos/departamento-formulario';
import { DepartamentosListado } from './vistas/departamentos/departamentos-listado';
import { EmpresaColaboradores } from './vistas/empresas/empresa-colaboradores';
import { EmpresaFormulario } from './vistas/empresas/empresa-formulario';
import { EmpresasListado } from './vistas/empresas/empresas-listado';
import { Inicio } from './vistas/inicio';
import { MunicipioFormulario } from './vistas/municipios/municipio-formulario';
import { MunicipiosListado } from './vistas/municipios/municipios-listado';
import { PaisFormulario } from './vistas/paises/pais-formulario';
import { PaisesListado } from './vistas/paises/paises-listado';
import { Pendiente } from './vistas/pendiente';

// Rutas del «Contrato de interfaz» (docs/frontend/ESPECIFICACION.md). Las pantallas llegan en las
// tareas 38 a 42 y reemplazan a Pendiente. Las rutas fijas («nuevo») van antes que las de :id.
export const routes: Routes = [
  { path: '', component: Inicio, title: 'Recursos Humanos' },

  { path: 'paises', component: PaisesListado, title: 'Países' },
  { path: 'paises/nuevo', component: PaisFormulario, title: 'Nuevo país' },
  { path: 'paises/:id/editar', component: PaisFormulario, title: 'Editar país' },

  { path: 'departamentos', component: DepartamentosListado, title: 'Departamentos' },
  { path: 'departamentos/nuevo', component: DepartamentoFormulario, title: 'Nuevo departamento' },
  {
    path: 'departamentos/:id/editar',
    component: DepartamentoFormulario,
    title: 'Editar departamento',
  },

  { path: 'municipios', component: MunicipiosListado, title: 'Municipios' },
  { path: 'municipios/nuevo', component: MunicipioFormulario, title: 'Nuevo municipio' },
  { path: 'municipios/:id/editar', component: MunicipioFormulario, title: 'Editar municipio' },

  { path: 'empresas', component: EmpresasListado, title: 'Empresas' },
  { path: 'empresas/nuevo', component: EmpresaFormulario, title: 'Nueva empresa' },
  { path: 'empresas/:id/editar', component: EmpresaFormulario, title: 'Editar empresa' },
  {
    path: 'empresas/:id/colaboradores',
    component: EmpresaColaboradores,
    title: 'Colaboradores de la empresa',
  },

  { path: 'colaboradores', component: ColaboradoresListado, title: 'Colaboradores' },
  { path: 'colaboradores/nuevo', component: ColaboradorAlta, title: 'Nuevo colaborador' },
  { path: 'colaboradores/:id/editar', component: ColaboradorEditar, title: 'Editar colaborador' },
  { path: 'colaboradores/:id', component: Pendiente, title: 'Detalle del colaborador' },

  { path: '**', redirectTo: '' },
];

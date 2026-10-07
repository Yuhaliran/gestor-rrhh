import { Routes } from '@angular/router';

import { Inicio } from './vistas/inicio';
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

  { path: 'departamentos', component: Pendiente, title: 'Departamentos' },
  { path: 'departamentos/nuevo', component: Pendiente, title: 'Nuevo departamento' },
  { path: 'departamentos/:id/editar', component: Pendiente, title: 'Editar departamento' },

  { path: 'municipios', component: Pendiente, title: 'Municipios' },
  { path: 'municipios/nuevo', component: Pendiente, title: 'Nuevo municipio' },
  { path: 'municipios/:id/editar', component: Pendiente, title: 'Editar municipio' },

  { path: 'empresas', component: Pendiente, title: 'Empresas' },
  { path: 'empresas/nuevo', component: Pendiente, title: 'Nueva empresa' },
  { path: 'empresas/:id/editar', component: Pendiente, title: 'Editar empresa' },
  {
    path: 'empresas/:id/colaboradores',
    component: Pendiente,
    title: 'Colaboradores de la empresa',
  },

  { path: 'colaboradores', component: Pendiente, title: 'Colaboradores' },
  { path: 'colaboradores/nuevo', component: Pendiente, title: 'Nuevo colaborador' },
  { path: 'colaboradores/:id/editar', component: Pendiente, title: 'Editar colaborador' },
  { path: 'colaboradores/:id', component: Pendiente, title: 'Detalle del colaborador' },

  { path: '**', redirectTo: '' },
];

import { Routes } from '@angular/router';

// Rutas del «Contrato de interfaz» (docs/frontend/ESPECIFICACION.md). Las pantallas llegan en las
// tareas 38 a 42 y reemplazan a Pendiente. Las rutas fijas («nuevo») van antes que las de :id.
// Cada pantalla se carga al entrar en ella (loadComponent): el bundle inicial queda con el layout
// y lo común, por debajo del presupuesto de angular.json.
export const routes: Routes = [
  {
    path: '',
    loadComponent: () => import('./vistas/inicio').then((m) => m.Inicio),
    title: 'Recursos Humanos',
  },

  {
    path: 'paises',
    loadComponent: () => import('./vistas/paises/paises-listado').then((m) => m.PaisesListado),
    title: 'Países',
  },
  {
    path: 'paises/nuevo',
    loadComponent: () => import('./vistas/paises/pais-formulario').then((m) => m.PaisFormulario),
    title: 'Nuevo país',
  },
  {
    path: 'paises/:id/editar',
    loadComponent: () => import('./vistas/paises/pais-formulario').then((m) => m.PaisFormulario),
    title: 'Editar país',
  },

  {
    path: 'departamentos',
    loadComponent: () =>
      import('./vistas/departamentos/departamentos-listado').then((m) => m.DepartamentosListado),
    title: 'Departamentos',
  },
  {
    path: 'departamentos/nuevo',
    loadComponent: () =>
      import('./vistas/departamentos/departamento-formulario').then(
        (m) => m.DepartamentoFormulario,
      ),
    title: 'Nuevo departamento',
  },
  {
    path: 'departamentos/:id/editar',
    loadComponent: () =>
      import('./vistas/departamentos/departamento-formulario').then(
        (m) => m.DepartamentoFormulario,
      ),
    title: 'Editar departamento',
  },

  {
    path: 'municipios',
    loadComponent: () =>
      import('./vistas/municipios/municipios-listado').then((m) => m.MunicipiosListado),
    title: 'Municipios',
  },
  {
    path: 'municipios/nuevo',
    loadComponent: () =>
      import('./vistas/municipios/municipio-formulario').then((m) => m.MunicipioFormulario),
    title: 'Nuevo municipio',
  },
  {
    path: 'municipios/:id/editar',
    loadComponent: () =>
      import('./vistas/municipios/municipio-formulario').then((m) => m.MunicipioFormulario),
    title: 'Editar municipio',
  },

  {
    path: 'empresas',
    loadComponent: () =>
      import('./vistas/empresas/empresas-listado').then((m) => m.EmpresasListado),
    title: 'Empresas',
  },
  {
    path: 'empresas/nuevo',
    loadComponent: () =>
      import('./vistas/empresas/empresa-formulario').then((m) => m.EmpresaFormulario),
    title: 'Nueva empresa',
  },
  {
    path: 'empresas/:id/editar',
    loadComponent: () =>
      import('./vistas/empresas/empresa-formulario').then((m) => m.EmpresaFormulario),
    title: 'Editar empresa',
  },
  {
    path: 'empresas/:id/colaboradores',
    loadComponent: () =>
      import('./vistas/empresas/empresa-colaboradores').then((m) => m.EmpresaColaboradores),
    title: 'Colaboradores de la empresa',
  },

  {
    path: 'colaboradores',
    loadComponent: () =>
      import('./vistas/colaboradores/colaboradores-listado').then((m) => m.ColaboradoresListado),
    title: 'Colaboradores',
  },
  {
    path: 'colaboradores/nuevo',
    loadComponent: () =>
      import('./vistas/colaboradores/colaborador-alta').then((m) => m.ColaboradorAlta),
    title: 'Nuevo colaborador',
  },
  {
    path: 'colaboradores/:id/editar',
    loadComponent: () =>
      import('./vistas/colaboradores/colaborador-editar').then((m) => m.ColaboradorEditar),
    title: 'Editar colaborador',
  },
  {
    path: 'colaboradores/:id',
    loadComponent: () => import('./vistas/pendiente').then((m) => m.Pendiente),
    title: 'Detalle del colaborador',
  },

  { path: '**', redirectTo: '' },
];

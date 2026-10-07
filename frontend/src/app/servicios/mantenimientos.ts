// RF1 · Los mantenimientos, en el orden del menú y de la página de inicio.
export const MANTENIMIENTOS = [
  { nombre: 'Países', ruta: '/paises', descripcion: 'Edades y regla del 29 de febrero' },
  { nombre: 'Departamentos', ruta: '/departamentos', descripcion: 'De cada país' },
  { nombre: 'Municipios', ruta: '/municipios', descripcion: 'De cada departamento' },
  { nombre: 'Empresas', ruta: '/empresas', descripcion: 'Con su ubicación y colaboradores' },
  { nombre: 'Colaboradores', ruta: '/colaboradores', descripcion: 'Con su edad y sus empresas' },
] as const;

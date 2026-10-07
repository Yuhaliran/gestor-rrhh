import { render, screen } from '@testing-library/angular';
import { App } from '../../src/app/app';
import { Inicio } from '../../src/app/vistas/inicio';
import { proveedoresDePrueba } from '../apoyo/proveedores';

describe('App', () => {
  it('render_SinRuta_MuestraElTituloDeLaAplicacion', async () => {
    await render(App, { providers: proveedoresDePrueba() });
    expect(screen.getByRole('heading', { name: 'Recursos Humanos' })).toBeTruthy();
  });

  it('menu_TieneEnlacesAMantenimientos', async () => {
    await render(App, { providers: proveedoresDePrueba() });
    expect(screen.getByRole('link', { name: 'Países' }).getAttribute('href')).toBe('/paises');
    expect(screen.getByRole('link', { name: 'Departamentos' }).getAttribute('href')).toBe(
      '/departamentos',
    );
    expect(screen.getByRole('link', { name: 'Municipios' }).getAttribute('href')).toBe(
      '/municipios',
    );
    expect(screen.getByRole('link', { name: 'Empresas' }).getAttribute('href')).toBe('/empresas');
    expect(screen.getByRole('link', { name: 'Colaboradores' }).getAttribute('href')).toBe(
      '/colaboradores',
    );
  });
});

describe('Inicio', () => {
  it('inicio_TieneAccesosAMantenimientos', async () => {
    await render(Inicio, { providers: proveedoresDePrueba() });
    expect(screen.getByRole('link', { name: /Países/i }).getAttribute('href')).toBe('/paises');
    expect(screen.getByRole('link', { name: /Departamentos/i }).getAttribute('href')).toBe(
      '/departamentos',
    );
    expect(screen.getByRole('link', { name: /Municipios/i }).getAttribute('href')).toBe(
      '/municipios',
    );
    expect(screen.getByRole('link', { name: /Empresas/i }).getAttribute('href')).toBe('/empresas');
    expect(screen.getByRole('link', { name: /Colaboradores/i }).getAttribute('href')).toBe(
      '/colaboradores',
    );
  });
});

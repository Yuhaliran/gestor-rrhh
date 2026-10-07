import { TestBed } from '@angular/core/testing';
import { render, screen } from '@testing-library/angular';
import { userEvent } from '@testing-library/user-event';
import { HttpTestingController } from '@angular/common/http/testing';
import { Router } from '@angular/router';
import { describe, expect, it, vi } from 'vitest';

import { proveedoresDePrueba } from '../apoyo/proveedores';
import {
  conflicto,
  guatemala,
  guatemalaDepto,
  guatemalaMuni,
  miEmpresa,
  colaboradorConDosEmpresas,
  paginaDe,
  validacion,
  noEncontrado,
} from '../apoyo/respuestas';
import { EmpresasListado } from '../../src/app/vistas/empresas/empresas-listado';
import { EmpresaFormulario } from '../../src/app/vistas/empresas/empresa-formulario';
import { EmpresaColaboradores } from '../../src/app/vistas/empresas/empresa-colaboradores';

const otraMuni = {
  ...guatemalaMuni,
  id: 2,
  nombre: 'Mixco',
  departamentoId: 1,
  departamentoNombre: 'Guatemala',
  paisId: 1,
  paisNombre: 'Guatemala',
};

describe('EmpresasListado', () => {
  it('listar_MuestraFilasConNombreNitYGeografia_YEnlacesY409AlEliminar', async () => {
    const user = userEvent.setup();
    await render(EmpresasListado, { providers: proveedoresDePrueba() });
    const api = TestBed.inject(HttpTestingController);

    api.expectOne((r) => r.url.endsWith('/api/empresas')).flush(paginaDe([miEmpresa]));

    expect(await screen.findByText('Mi Empresa')).toBeTruthy();
    expect(screen.getByText('123456-7')).toBeTruthy();
    expect(screen.getAllByText('Guatemala').length).toBeGreaterThanOrEqual(3);

    // Links
    const editar = screen.getByRole('link', { name: 'Editar' });
    expect(editar.getAttribute('href')).toBe('/empresas/1/editar');

    const colaboradores = screen.getByRole('link', { name: 'Colaboradores' });
    expect(colaboradores.getAttribute('href')).toBe('/empresas/1/colaboradores');

    // Delete 409
    await user.click(screen.getByRole('button', { name: 'Eliminar' }));
    await user.click(await screen.findByRole('button', { name: 'Sí, eliminar' }));

    api
      .expectOne((r) => r.method === 'DELETE' && r.url.endsWith('/api/empresas/1'))
      .flush(conflicto('La empresa tiene colaboradores.'), { status: 409, statusText: 'Conflict' });

    expect(await screen.findByText('La empresa tiene colaboradores.')).toBeTruthy();
  });
});

describe('EmpresaFormulario', () => {
  it('crear_CascadaYEnviar_ValidaObligatoriosYHacePostConMunicipioId', async () => {
    const user = userEvent.setup();
    await render(EmpresaFormulario, { providers: proveedoresDePrueba() });
    const api = TestBed.inject(HttpTestingController);

    api
      .expectOne((r) => r.url.endsWith('/api/paises') && r.params.get('tamanio') === '100')
      .flush(paginaDe([guatemala]));

    // Departamento and Municipio disabled
    const deptoSelect = screen.getByLabelText('Departamento');
    expect(
      deptoSelect.getAttribute('aria-disabled') === 'true' || deptoSelect.hasAttribute('disabled'),
    ).toBeTruthy();

    const muniSelect = screen.getByLabelText('Municipio');
    expect(
      muniSelect.getAttribute('aria-disabled') === 'true' || muniSelect.hasAttribute('disabled'),
    ).toBeTruthy();

    // Guardar sin elegir país
    await user.click(screen.getByRole('button', { name: 'Guardar' }));

    // VC1
    const mensajes = await screen.findAllByText('Este campo es obligatorio.');
    expect(mensajes.length).toBeGreaterThan(0);
    api.expectNone((r) => r.method === 'POST');

    // Elige país
    await user.click(screen.getByLabelText('País'));
    await user.click(await screen.findByRole('option', { name: 'Guatemala' }));
    api.expectOne((r) => r.url.endsWith('/api/paises/1/departamentos')).flush([guatemalaDepto]);

    // Elige depto
    await user.click(screen.getByLabelText('Departamento'));
    await user.click(await screen.findByRole('option', { name: 'Guatemala' }));
    api.expectOne((r) => r.url.endsWith('/api/departamentos/1/municipios')).flush([guatemalaMuni]);

    // Elige muni
    await user.click(screen.getByLabelText('Municipio'));
    await user.click(await screen.findByRole('option', { name: 'Guatemala' }));

    // Llena los otros 5 datos
    await user.type(screen.getByLabelText('NIT'), '123456-7');
    await user.type(screen.getByLabelText('Razón social'), 'Mi Empresa S.A.');
    await user.type(screen.getByLabelText('Nombre comercial'), 'Mi Empresa');
    await user.type(screen.getByLabelText('Teléfono'), '12345678');
    await user.type(screen.getByLabelText('Correo'), 'info@miempresa.com');

    // Enviar y esperar error 400 en NIT
    await user.click(screen.getByRole('button', { name: 'Guardar' }));
    api
      .expectOne((r) => r.method === 'POST')
      .flush(validacion({ nit: ['El NIT ya existe.'] }), {
        status: 400,
        statusText: 'Bad Request',
      });

    expect(await screen.findByText('El NIT ya existe.')).toBeTruthy();

    // Cambiar país vacía depto y muni
    await user.click(screen.getByLabelText('País'));
    // Supongamos que reelegimos Guatemala para simular cambio
    await user.click(await screen.findByRole('option', { name: 'Guatemala' }));
    api.expectOne((r) => r.url.endsWith('/api/paises/1/departamentos')).flush([guatemalaDepto]);

    expect(
      screen.queryByText('Guatemala', {
        selector: 'mat-select[formControlName="departamentoId"] *',
      }),
    ).toBeFalsy();
  });

  it('editar_IniciaCascadaYPaisDeshabilitado_GuardaConMunicipioNuevo_YError404', async () => {
    const user = userEvent.setup();
    await render(EmpresaFormulario, { inputs: { id: '1' }, providers: proveedoresDePrueba() });
    const api = TestBed.inject(HttpTestingController);

    const router = TestBed.inject(Router);
    vi.spyOn(router, 'navigateByUrl').mockResolvedValue(true);

    api.expectOne((r) => r.method === 'GET' && r.url.endsWith('/api/empresas/1')).flush(miEmpresa);
    api
      .expectOne((r) => r.url.endsWith('/api/paises') && r.params.get('tamanio') === '100')
      .flush(paginaDe([guatemala]));
    api.expectOne((r) => r.url.endsWith('/api/paises/1/departamentos')).flush([guatemalaDepto]);
    api
      .expectOne((r) => r.url.endsWith('/api/departamentos/1/municipios'))
      .flush([guatemalaMuni, otraMuni]);

    const paisSelect = await screen.findByLabelText('País');
    expect(
      paisSelect.getAttribute('aria-disabled') === 'true' || paisSelect.hasAttribute('disabled'),
    ).toBeTruthy();

    const deptoSelect = screen.getByLabelText('Departamento');
    expect(
      deptoSelect.getAttribute('aria-disabled') === 'false' ||
        !deptoSelect.hasAttribute('disabled'),
    ).toBeTruthy();

    const muniSelect = screen.getByLabelText('Municipio');
    expect(
      muniSelect.getAttribute('aria-disabled') === 'false' || !muniSelect.hasAttribute('disabled'),
    ).toBeTruthy();

    // Cambia muni
    await user.click(muniSelect);
    await user.click(await screen.findByRole('option', { name: 'Mixco' }));

    await user.click(screen.getByRole('button', { name: 'Guardar' }));
    const pedido = api.expectOne((r) => r.method === 'PUT' && r.url.endsWith('/api/empresas/1'));
    expect(pedido.request.body.municipioId).toBe(2);
    pedido.flush(miEmpresa);
  });

  it('editar_EmpresaNoExiste_MuestraRF7', async () => {
    await render(EmpresaFormulario, { inputs: { id: '999' }, providers: proveedoresDePrueba() });
    const api = TestBed.inject(HttpTestingController);

    api
      .expectOne((r) => r.url.endsWith('/api/empresas/999'))
      .flush(noEncontrado(), { status: 404, statusText: 'Not Found' });

    expect(await screen.findByText('El registro no existe.')).toBeTruthy();
    expect(screen.getByRole('link', { name: 'Volver al listado' })).toBeTruthy();
  });
});

describe('EmpresaColaboradores', () => {
  it('listar_MuestraTitulo_FilasConDatosDeLaEmpresa_EnlacesYError404', async () => {
    await render(EmpresaColaboradores, { inputs: { id: '1' }, providers: proveedoresDePrueba() });
    const api = TestBed.inject(HttpTestingController);

    api.expectOne((r) => r.url.endsWith('/api/empresas/1')).flush(miEmpresa);
    api
      .expectOne((r) => r.url.endsWith('/api/empresas/1/colaboradores'))
      .flush(paginaDe([colaboradorConDosEmpresas]));

    // Titulo
    expect(await screen.findByText('Colaboradores de Mi Empresa')).toBeTruthy();

    // Fila del colaborador, usar los datos de "Mi Empresa" (empresa 1)
    expect(screen.getByText('Juan Pérez')).toBeTruthy();
    expect(screen.getByText('34 años')).toBeTruthy();

    // Convert 2020-01-01 to format according to the specification (dd/mm/aaaa)
    expect(screen.getByText('01/01/2020')).toBeTruthy();
    expect(screen.getByText('Desarrollador')).toBeTruthy();

    // Verify it doesn't show the data for the other company (E-018)
    expect(screen.queryByText('01/01/2022')).toBeFalsy();
    expect(screen.queryByText('Consultor')).toBeFalsy();

    // Enlaces
    const verDetalle = screen.getByRole('link', { name: 'Ver detalle' });
    expect(verDetalle.getAttribute('href')).toBe('/colaboradores/1');

    const volver = screen.getByRole('link', { name: 'Volver al listado' });
    expect(volver.getAttribute('href')).toBe('/empresas');
  });

  it('listar_EmpresaNoExiste_MuestraRF7', async () => {
    await render(EmpresaColaboradores, { inputs: { id: '999' }, providers: proveedoresDePrueba() });
    const api = TestBed.inject(HttpTestingController);

    api
      .expectOne((r) => r.url.endsWith('/api/empresas/999'))
      .flush(noEncontrado(), { status: 404, statusText: 'Not Found' });

    expect(await screen.findByText('El registro no existe.')).toBeTruthy();
    expect(screen.getByRole('link', { name: 'Volver al listado' })).toBeTruthy();
  });
});

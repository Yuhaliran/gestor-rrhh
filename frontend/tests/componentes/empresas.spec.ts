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

const otroDepto = {
  ...guatemalaDepto,
  id: 2,
  nombre: 'Sacatepéquez',
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
    expect(await screen.findByText('Antigua Guatemala')).toBeTruthy();
    expect(screen.getByText('Sacatepéquez')).toBeTruthy();
    expect(screen.getByText('Guatemala')).toBeTruthy();

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
    const router = TestBed.inject(Router);
    vi.spyOn(router, 'navigateByUrl').mockResolvedValue(true);

    api
      .expectOne((r) => r.url.endsWith('/api/paises') && r.params.get('tamanio') === '100')
      .flush(
        paginaDe([
          { ...guatemala, id: 1, nombre: 'Guatemala' },
          { ...guatemala, id: 9, nombre: 'El Salvador' },
        ]),
      );

    // Departamento and Municipio disabled
    const deptoSelect = screen.getByLabelText('Departamento');
    expect(
      deptoSelect.getAttribute('aria-disabled') === 'true' || deptoSelect.hasAttribute('disabled'),
    ).toBeTruthy();

    const muniSelect = screen.getByLabelText('Municipio');
    expect(
      muniSelect.getAttribute('aria-disabled') === 'true' || muniSelect.hasAttribute('disabled'),
    ).toBeTruthy();

    // Llena los otros 5 datos (valid format)
    await user.type(screen.getByLabelText('NIT'), '123456-7');
    await user.type(screen.getByLabelText('Razón social'), 'Mi Empresa S.A.');
    await user.type(screen.getByLabelText('Nombre comercial'), 'Mi Empresa');
    await user.type(screen.getByLabelText('Teléfono'), '12345678');
    await user.type(screen.getByLabelText('Correo'), 'info@miempresa.com');

    // Guardar sin elegir país (VC1)
    await user.click(screen.getByRole('button', { name: 'Guardar' }));
    let mensajes = await screen.findAllByText('Este campo es obligatorio.');
    expect(mensajes.length).toBe(1);
    api.expectNone((r) => r.method === 'POST');

    // Elige país
    await user.click(screen.getByLabelText('País'));
    await user.click(await screen.findByRole('option', { name: 'Guatemala' }));
    api
      .expectOne((r) => r.url.endsWith('/api/paises/1/departamentos'))
      .flush([guatemalaDepto, otroDepto]);

    // Guardar sin elegir depto (VC1)
    await user.click(screen.getByRole('button', { name: 'Guardar' }));
    mensajes = await screen.findAllByText('Este campo es obligatorio.');
    expect(mensajes.length).toBe(1);
    api.expectNone((r) => r.method === 'POST');

    // Elige depto
    await user.click(screen.getByLabelText('Departamento'));
    await user.click(await screen.findByRole('option', { name: 'Guatemala' }));
    api.expectOne((r) => r.url.endsWith('/api/departamentos/1/municipios')).flush([guatemalaMuni]);

    // Guardar sin elegir muni (VC1)
    await user.click(screen.getByRole('button', { name: 'Guardar' }));
    mensajes = await screen.findAllByText('Este campo es obligatorio.');
    expect(mensajes.length).toBe(1);
    api.expectNone((r) => r.method === 'POST');

    // Elige muni
    await user.click(screen.getByLabelText('Municipio'));
    await user.click(await screen.findByRole('option', { name: 'Guatemala' }));

    // Enviar y esperar error 400 en NIT
    await user.click(screen.getByRole('button', { name: 'Guardar' }));
    api
      .expectOne((r) => r.method === 'POST')
      .flush(validacion({ Nit: ['El NIT ya existe.'] }), {
        status: 400,
        statusText: 'Bad Request',
      });

    expect(await screen.findByText('El NIT ya existe.')).toBeTruthy();

    // POST Exitoso para verificar body
    await user.click(screen.getByRole('button', { name: 'Guardar' }));
    const pedidoExito = api.expectOne((r) => r.method === 'POST');
    expect(pedidoExito.request.body).toEqual({
      municipioId: 1,
      nit: '123456-7',
      razonSocial: 'Mi Empresa S.A.',
      nombreComercial: 'Mi Empresa',
      telefono: '12345678',
      correo: 'info@miempresa.com',
    });
    pedidoExito.flush(miEmpresa);

    // RF4: Navega
    expect(router.navigateByUrl).toHaveBeenCalledWith('/empresas');

    // Cambiar departamento vacía municipio
    await user.click(screen.getByLabelText('Departamento'));
    await user.click(await screen.findByRole('option', { name: 'Sacatepéquez' }));
    api.expectOne((r) => r.url.endsWith('/api/departamentos/2/municipios')).flush([]);

    expect(muniSelect.textContent?.includes('Guatemala')).toBeFalsy();

    // Cambiar país vacía depto y muni directamente, asegurando que muni estuviera elegido
    // El país ya es Guatemala.
    await user.click(screen.getByLabelText('Departamento'));
    await user.click(await screen.findByRole('option', { name: 'Guatemala' }));
    api.expectOne((r) => r.url.endsWith('/api/departamentos/1/municipios')).flush([guatemalaMuni]);
    await user.click(screen.getByLabelText('Municipio'));
    await user.click(await screen.findByRole('option', { name: 'Guatemala' }));

    await user.click(screen.getByLabelText('País'));
    await user.click(await screen.findByRole('option', { name: 'El Salvador' }));
    api.expectOne((r) => r.url.endsWith('/api/paises/9/departamentos')).flush([]);

    expect(deptoSelect.textContent?.includes('Guatemala')).toBeFalsy();
    expect(muniSelect.textContent?.includes('Guatemala')).toBeFalsy();
    expect(
      muniSelect.getAttribute('aria-disabled') === 'true' || muniSelect.hasAttribute('disabled'),
    ).toBeTruthy();
  });

  it('crear_FormatosInvalidos_MuestraMensajeYNoLlamaApi', async () => {
    const user = userEvent.setup();
    await render(EmpresaFormulario, { providers: proveedoresDePrueba() });
    const api = TestBed.inject(HttpTestingController);

    api.expectOne((r) => r.url.endsWith('/api/paises')).flush(paginaDe([guatemala]));

    const tel = screen.getByLabelText('Teléfono');
    await user.type(tel, '12'); // Invalido VC3
    const correo = screen.getByLabelText('Correo');
    await user.type(correo, 'correo-invalido'); // Invalido VC3

    await user.click(screen.getByRole('button', { name: 'Guardar' }));

    const mensajes = await screen.findAllByText('El formato no es válido.');
    expect(mensajes.length).toBeGreaterThanOrEqual(2);
    api.expectNone((r) => r.method === 'POST');
  });

  it('crear_ErrorDeRed_MuestraRF8', async () => {
    await render(EmpresaFormulario, { providers: proveedoresDePrueba() });
    const api = TestBed.inject(HttpTestingController);

    api
      .expectOne((r) => r.url.endsWith('/api/paises'))
      .error(new ProgressEvent('error'), { status: 0, statusText: 'Unknown Error' });

    expect(await screen.findByText('No se pudo conectar con la API.')).toBeTruthy();
  });

  it('editar_IniciaCascadaYPaisDeshabilitado_GuardaConMunicipioNuevo', async () => {
    const user = userEvent.setup();
    await render(EmpresaFormulario, { inputs: { id: '1' }, providers: proveedoresDePrueba() });
    const api = TestBed.inject(HttpTestingController);

    const router = TestBed.inject(Router);
    vi.spyOn(router, 'navigateByUrl').mockResolvedValue(true);

    api.expectOne((r) => r.method === 'GET' && r.url.endsWith('/api/empresas/1')).flush(miEmpresa);
    api
      .expectOne((r) => r.url.endsWith('/api/paises') && r.params.get('tamanio') === '100')
      .flush(paginaDe([guatemala]));
    // Edit starts with cascada loaded
    api
      .expectOne((r) => r.url.endsWith('/api/paises/1/departamentos'))
      .flush([guatemalaDepto, otroDepto]);
    api
      .expectOne((r) => r.url.endsWith('/api/departamentos/2/municipios'))
      .flush([
        {
          ...guatemalaMuni,
          nombre: 'Antigua Guatemala',
          departamentoId: 2,
          departamentoNombre: 'Sacatepéquez',
        },
        otraMuni,
      ]);

    const paisSelect = await screen.findByLabelText('País');
    expect(
      paisSelect.getAttribute('aria-disabled') === 'true' || paisSelect.hasAttribute('disabled'),
    ).toBeTruthy();

    // Verify all three names are visible in edit
    expect(await screen.findByText('Antigua Guatemala')).toBeTruthy();
    expect(screen.getByText('Sacatepéquez')).toBeTruthy();
    expect(screen.getAllByText('Guatemala').length).toBeGreaterThanOrEqual(1);

    const deptoSelect = screen.getByLabelText('Departamento');
    expect(deptoSelect.getAttribute('aria-disabled') === 'false').toBeTruthy();

    const muniSelect = screen.getByLabelText('Municipio');
    expect(muniSelect.getAttribute('aria-disabled') === 'false').toBeTruthy();

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
  it('listar_MuestraTitulo_FilasConDatosDeLaEmpresa_Enlaces', async () => {
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

    // Enlaces: id de colaboradorConDosEmpresas is 5
    const verDetalle = screen.getByRole('link', { name: 'Ver detalle' });
    expect(verDetalle.getAttribute('href')).toBe('/colaboradores/5');

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

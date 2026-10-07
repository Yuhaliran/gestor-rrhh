import { TestBed } from '@angular/core/testing';
import { render, screen, within } from '@testing-library/angular';
import { userEvent } from '@testing-library/user-event';
import { HttpTestingController } from '@angular/common/http/testing';
import { Router } from '@angular/router';
import { describe, expect, it, vi, afterEach } from 'vitest';

import { proveedoresDePrueba } from '../apoyo/proveedores';
import { conflicto, paginaDe, validacion, noEncontrado, miEmpresa } from '../apoyo/respuestas';
import { ColaboradoresListado } from '../../src/app/vistas/colaboradores/colaboradores-listado';
import { ColaboradorAlta } from '../../src/app/vistas/colaboradores/colaborador-alta';
import { ColaboradorEditar } from '../../src/app/vistas/colaboradores/colaborador-editar';

const colaboradorMock = {
  id: 1,
  nombreCompleto: 'Juan Pérez',
  fechaNacimiento: '1990-01-01',
  edad: 34,
  telefono: '12345678',
  correo: 'juan@example.com',
  empresas: [
    {
      empresaId: 1,
      nombreComercial: 'Empresa A',
      paisId: 1,
      paisNombre: 'Guatemala',
      fechaIngreso: '2020-01-01',
      puesto: 'Desarrollador',
    },
    {
      empresaId: 2,
      nombreComercial: 'Empresa B',
      paisId: 1,
      paisNombre: 'Guatemala',
      fechaIngreso: '2022-01-01',
      puesto: 'Consultor',
    },
  ],
};

const empresa2 = {
  ...miEmpresa,
  id: 2,
  nombreComercial: 'Empresa B',
};

describe('ColaboradoresListado', () => {
  it('listar_MuestraFilasConDatosYEnlaces_Y409AlEliminar', async () => {
    const user = userEvent.setup();
    await render(ColaboradoresListado, { providers: proveedoresDePrueba() });
    const api = TestBed.inject(HttpTestingController);

    api.expectOne((r) => r.url.endsWith('/api/colaboradores')).flush(paginaDe([colaboradorMock]));

    expect(await screen.findByText('Juan Pérez')).toBeTruthy();
    expect(screen.getByText('juan@example.com')).toBeTruthy();
    expect(screen.getByText('34 años')).toBeTruthy();
    // Verification of companies (might be joined or just listed)
    expect(await screen.findByText('Empresa A, Empresa B')).toBeTruthy();

    const verDetalle = screen.getByRole('link', { name: 'Ver detalle' });
    expect(verDetalle.getAttribute('href')).toBe('/colaboradores/1');
    const editar = screen.getByRole('link', { name: 'Editar' });
    expect(editar.getAttribute('href')).toBe('/colaboradores/1/editar');

    await user.click(screen.getByRole('button', { name: 'Eliminar' }));
    await user.click(await screen.findByRole('button', { name: 'Sí, eliminar' }));

    api
      .expectOne((r) => r.method === 'DELETE' && r.url.endsWith('/api/colaboradores/1'))
      .flush(conflicto('No se puede eliminar.'), { status: 409, statusText: 'Conflict' });

    expect(await screen.findByText('No se puede eliminar.')).toBeTruthy();
  });
});

describe('ColaboradorAlta', () => {
  afterEach(() => {
    vi.useRealTimers();
  });

  it('crear_ValidaYGuardaConMultiplesEmpresas_NavegaYReordena', async () => {
    vi.useFakeTimers({ toFake: ['Date'] });
    vi.setSystemTime(new Date('2026-10-07T12:00:00Z'));
    const user = userEvent.setup({ advanceTimers: vi.advanceTimersByTime });

    await render(ColaboradorAlta, { providers: proveedoresDePrueba() });
    const api = TestBed.inject(HttpTestingController);
    const router = TestBed.inject(Router);
    vi.spyOn(router, 'navigateByUrl').mockResolvedValue(true);

    api
      .expectOne((r) => r.url.endsWith('/api/empresas') && r.params.get('tamanio') === '100')
      .flush(paginaDe([miEmpresa, empresa2]));

    // Empresa 1 group
    const grupo1 = await screen.findByRole('group', { name: 'Empresa 1' });
    const quitar1 = within(grupo1).getByRole('button', { name: 'Quitar' });
    expect(
      quitar1.hasAttribute('disabled') || quitar1.getAttribute('aria-disabled') === 'true',
    ).toBeTruthy();

    // Fill personal data
    await user.type(screen.getByLabelText('Nombre completo'), 'Juan Pérez');
    await user.type(screen.getByLabelText('Fecha de nacimiento'), '1990-01-01');
    await user.type(screen.getByLabelText('Teléfono'), '12345678');
    await user.type(screen.getByLabelText('Correo'), 'juan@example.com');

    // Add company 2
    await user.click(screen.getByRole('button', { name: 'Agregar empresa' }));
    const grupo2 = await screen.findByRole('group', { name: 'Empresa 2' });
    const quitar2 = within(grupo2).getByRole('button', { name: 'Quitar' });
    expect(
      quitar2.hasAttribute('disabled') || quitar2.getAttribute('aria-disabled') === 'true',
    ).toBeFalsy();
    // Quitar1 should now be enabled
    expect(
      quitar1.hasAttribute('disabled') || quitar1.getAttribute('aria-disabled') === 'true',
    ).toBeFalsy();

    // Fill data for Empresa 2 (which will become Empresa 1 when we remove the first one)
    await user.click(within(grupo2).getByLabelText('Empresa'));
    await user.click(await screen.findByRole('option', { name: 'Empresa B' }));
    await user.type(within(grupo2).getByLabelText('Fecha de ingreso'), '2022-01-01');
    await user.type(within(grupo2).getByLabelText('Puesto (opcional)'), 'Consultor');

    // Fill data for Empresa 1 to differentiate
    await user.click(within(grupo1).getByLabelText('Empresa'));
    await user.click(await screen.findByRole('option', { name: 'Mi Empresa' }));
    await user.type(within(grupo1).getByLabelText('Fecha de ingreso'), '2020-01-01');
    await user.type(within(grupo1).getByLabelText('Puesto (opcional)'), 'Desarrollador');

    // Remove row 1
    await user.click(quitar1);

    // Now there is only Empresa 1 (which has Empresa 2's data)
    const grupoUnico = await screen.findByRole('group', { name: 'Empresa 1' });
    expect(screen.queryByRole('group', { name: 'Empresa 2' })).toBeFalsy();

    // Check it retained Empresa 2's data
    const inputPuesto = within(grupoUnico).getByLabelText('Puesto (opcional)') as HTMLInputElement;
    expect(inputPuesto.value).toBe('Consultor');

    // Re-add another to test 400 routing
    await user.click(screen.getByRole('button', { name: 'Agregar empresa' }));
    const nuevoGrupo2 = await screen.findByRole('group', { name: 'Empresa 2' });
    await user.click(within(nuevoGrupo2).getByLabelText('Empresa'));
    await user.click(await screen.findByRole('option', { name: 'Mi Empresa' }));
    await user.type(within(nuevoGrupo2).getByLabelText('Fecha de ingreso'), '2020-01-01');

    await user.click(screen.getByRole('button', { name: 'Guardar' }));
    const req1 = api.expectOne((r) => r.method === 'POST');
    req1.flush(validacion({ 'Empresas[1].FechaIngreso': ['Fecha inválida.'] }), {
      status: 400,
      statusText: 'Bad Request',
    });

    // Ensure error is in group 2
    expect(await within(nuevoGrupo2).findByText('Fecha inválida.')).toBeTruthy();
    expect(within(grupoUnico).queryByText('Fecha inválida.')).toBeFalsy();

    // 409 as general warning
    await user.click(screen.getByRole('button', { name: 'Guardar' }));
    const req2 = api.expectOne((r) => r.method === 'POST');
    req2.flush(conflicto('Conflicto general'), { status: 409, statusText: 'Conflict' });
    expect(await screen.findByText('Conflicto general')).toBeTruthy();

    // Success POST
    await user.click(screen.getByRole('button', { name: 'Guardar' }));
    const req3 = api.expectOne((r) => r.method === 'POST');
    expect(req3.request.body).toEqual({
      nombreCompleto: 'Juan Pérez',
      fechaNacimiento: '1990-01-01',
      telefono: '12345678',
      correo: 'juan@example.com',
      empresas: [
        {
          empresaId: 2,
          fechaIngreso: '2022-01-01',
          puesto: 'Consultor',
        },
        {
          empresaId: 1,
          fechaIngreso: '2020-01-01',
          puesto: null,
        },
      ],
    });
    req3.flush(colaboradorMock);

    expect(router.navigateByUrl).toHaveBeenCalledWith('/colaboradores');
  });

  it('crear_ValidaVC1yVC5', async () => {
    vi.useFakeTimers({ toFake: ['Date'] });
    vi.setSystemTime(new Date('2026-10-07T12:00:00Z'));
    const user = userEvent.setup({ advanceTimers: vi.advanceTimersByTime });

    await render(ColaboradorAlta, { providers: proveedoresDePrueba() });
    const api = TestBed.inject(HttpTestingController);
    api
      .expectOne((r) => r.url.endsWith('/api/empresas') && r.params.get('tamanio') === '100')
      .flush(paginaDe([miEmpresa]));

    const grupo1 = await screen.findByRole('group', { name: 'Empresa 1' });

    // Llenar todos los datos excepto empresa
    await user.type(screen.getByLabelText('Nombre completo'), 'Juan Pérez');
    await user.type(screen.getByLabelText('Fecha de nacimiento'), '1990-01-01');
    await user.type(screen.getByLabelText('Teléfono'), '12345678');
    await user.type(screen.getByLabelText('Correo'), 'juan@example.com');
    await user.type(within(grupo1).getByLabelText('Fecha de ingreso'), '2020-01-01');

    await user.click(screen.getByRole('button', { name: 'Guardar' }));
    api.expectNone((r) => r.method === 'POST');

    let mensajes = await within(grupo1).findAllByText('Este campo es obligatorio.');
    expect(mensajes.length).toBe(1);
    expect(screen.getAllByText('Este campo es obligatorio.').length).toBe(1);

    // Ahora completa la empresa y vacía la fecha
    await user.click(within(grupo1).getByLabelText('Empresa'));
    await user.click(await screen.findByRole('option', { name: 'Mi Empresa' }));
    const ingreso = within(grupo1).getByLabelText('Fecha de ingreso') as HTMLInputElement;
    await user.clear(ingreso);

    await user.click(screen.getByRole('button', { name: 'Guardar' }));
    api.expectNone((r) => r.method === 'POST');
    mensajes = await within(grupo1).findAllByText('Este campo es obligatorio.');
    expect(mensajes.length).toBe(1);
    expect(screen.getAllByText('Este campo es obligatorio.').length).toBe(1);

    // VC5
    const nacimiento = screen.getByLabelText('Fecha de nacimiento') as HTMLInputElement;
    expect(nacimiento.getAttribute('max')).toBe('2026-10-07');
    expect(ingreso.getAttribute('max')).toBe('2026-10-07');
  });

  it('crear_SinConexionEmpresas_MuestraRF8', async () => {
    await render(ColaboradorAlta, { providers: proveedoresDePrueba() });
    const api = TestBed.inject(HttpTestingController);

    api
      .expectOne((r) => r.url.endsWith('/api/empresas') && r.params.get('tamanio') === '100')
      .error(new ProgressEvent('error'), { status: 0, statusText: 'Unknown Error' });

    expect(await screen.findByText('No se pudo conectar con la API.')).toBeTruthy();
  });

  it('crear_DatosPersonalesInvalidos_MuestraMensajesYNoLlamaApi', async () => {
    const user = userEvent.setup();
    await render(ColaboradorAlta, { providers: proveedoresDePrueba() });
    const api = TestBed.inject(HttpTestingController);

    api
      .expectOne((r) => r.url.endsWith('/api/empresas') && r.params.get('tamanio') === '100')
      .flush(paginaDe([miEmpresa]));

    const tel = screen.getByLabelText('Teléfono');
    await user.type(tel, '12'); // Inválido VC3
    const correo = screen.getByLabelText('Correo');
    await user.type(correo, 'correo-invalido'); // Inválido VC3

    await user.click(screen.getByRole('button', { name: 'Guardar' }));

    const mensajes = await screen.findAllByText('El formato no es válido.');
    expect(mensajes.length).toBeGreaterThanOrEqual(2);
    api.expectNone((r) => r.method === 'POST');

    // Vaciar nombre completo con el resto completo
    await user.clear(tel);
    await user.type(tel, '12345678');
    await user.clear(correo);
    await user.type(correo, 'juan@example.com');
    await user.type(screen.getByLabelText('Fecha de nacimiento'), '1990-01-01');
    const grupo1 = await screen.findByRole('group', { name: 'Empresa 1' });
    await user.click(within(grupo1).getByLabelText('Empresa'));
    await user.click(await screen.findByRole('option', { name: 'Mi Empresa' }));
    await user.type(within(grupo1).getByLabelText('Fecha de ingreso'), '2020-01-01');

    const nombre = screen.getByLabelText('Nombre completo') as HTMLInputElement;
    await user.clear(nombre);

    await user.click(screen.getByRole('button', { name: 'Guardar' }));
    api.expectNone((r) => r.method === 'POST');
    expect(await screen.findByText('Este campo es obligatorio.')).toBeTruthy();
  });
});

describe('ColaboradorEditar', () => {
  it('editar_CargaSoloDatosPersonales_EnviaPUTSoloConEsosDatos_NavegaY404', async () => {
    vi.useFakeTimers({ toFake: ['Date'] });
    vi.setSystemTime(new Date('2026-10-07T12:00:00Z'));
    const user = userEvent.setup({ advanceTimers: vi.advanceTimersByTime });
    await render(ColaboradorEditar, { inputs: { id: '1' }, providers: proveedoresDePrueba() });
    const api = TestBed.inject(HttpTestingController);
    const router = TestBed.inject(Router);
    vi.spyOn(router, 'navigateByUrl').mockResolvedValue(true);

    api
      .expectOne((r) => r.method === 'GET' && r.url.endsWith('/api/colaboradores/1'))
      .flush(colaboradorMock);

    expect(screen.queryByRole('group', { name: 'Empresa 1' })).toBeFalsy();
    expect(screen.queryByText('Agregar empresa')).toBeFalsy();

    const nombre = screen.getByLabelText('Nombre completo') as HTMLInputElement;
    expect(nombre.value).toBe('Juan Pérez');

    const nacimiento = screen.getByLabelText('Fecha de nacimiento') as HTMLInputElement;
    expect(nacimiento.getAttribute('max')).toBe('2026-10-07'); // VC5 max

    await user.clear(nombre);
    await user.type(nombre, 'Juan Modificado');

    await user.click(screen.getByRole('button', { name: 'Guardar' }));
    const pedido = api.expectOne(
      (r) => r.method === 'PUT' && r.url.endsWith('/api/colaboradores/1'),
    );

    expect(pedido.request.body).toEqual({
      nombreCompleto: 'Juan Modificado',
      fechaNacimiento: '1990-01-01',
      telefono: '12345678',
      correo: 'juan@example.com',
    });

    pedido.flush(colaboradorMock);

    expect(router.navigateByUrl).toHaveBeenCalledWith('/colaboradores');
  });

  it('editar_NoExiste_MuestraRF7', async () => {
    await render(ColaboradorEditar, { inputs: { id: '999' }, providers: proveedoresDePrueba() });
    const api = TestBed.inject(HttpTestingController);

    api
      .expectOne((r) => r.url.endsWith('/api/colaboradores/999'))
      .flush(noEncontrado(), { status: 404, statusText: 'Not Found' });
    expect(await screen.findByText('El registro no existe.')).toBeTruthy();
  });
});

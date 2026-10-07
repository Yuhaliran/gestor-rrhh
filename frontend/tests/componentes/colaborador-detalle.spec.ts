import { TestBed } from '@angular/core/testing';
import { render, screen, within } from '@testing-library/angular';
import { userEvent } from '@testing-library/user-event';
import { HttpTestingController } from '@angular/common/http/testing';
import { describe, expect, it, vi, afterEach } from 'vitest';

import { proveedoresDePrueba } from '../apoyo/proveedores';
import { conflicto, noEncontrado, validacion, miEmpresa, paginaDe } from '../apoyo/respuestas';
import { ColaboradorDetalle } from '../../src/app/vistas/colaboradores/colaborador-detalle';

// Datos que distinguen (E-018): id 5 (no 1 ni 2), y 2 empresas con fechas/puestos distintos.
const colaboradorMock = {
  id: 5,
  nombreCompleto: 'Juan Pérez',
  fechaNacimiento: '1990-01-01',
  edad: 34,
  telefono: '12345678',
  correo: 'juan@example.com',
  empresas: [
    {
      empresaId: 2,
      nombreComercial: 'Otra Empresa',
      paisId: 1,
      paisNombre: 'Guatemala',
      fechaIngreso: '2022-01-01',
      puesto: 'Consultor',
    },
    {
      empresaId: 1,
      nombreComercial: 'Mi Empresa',
      paisId: 1,
      paisNombre: 'Guatemala',
      fechaIngreso: '2020-01-01',
      puesto: 'Desarrollador',
    },
  ],
};

const empresa3 = { ...miEmpresa, id: 3, nombreComercial: 'Tercera Empresa' };

describe('ColaboradorDetalle', () => {
  afterEach(() => {
    vi.useRealTimers();
  });

  it('verDetalle_Con404_MuestraRF7', async () => {
    await render(ColaboradorDetalle, { inputs: { id: '999' }, providers: proveedoresDePrueba() });
    const api = TestBed.inject(HttpTestingController);
    api
      .expectOne((r) => r.method === 'GET' && r.url.endsWith('/api/colaboradores/999'))
      .flush(noEncontrado(), { status: 404, statusText: 'Not Found' });
    expect(await screen.findByText('El registro no existe.')).toBeTruthy();
  });

  it('verDetalle_CargaDatos_MuestraElDetalleYBotones', async () => {
    await render(ColaboradorDetalle, { inputs: { id: '5' }, providers: proveedoresDePrueba() });
    const api = TestBed.inject(HttpTestingController);
    api
      .expectOne((r) => r.method === 'GET' && r.url.endsWith('/api/colaboradores/5'))
      .flush(colaboradorMock);

    // Título con el nombre
    expect(await screen.findByRole('heading', { name: 'Juan Pérez' })).toBeTruthy();
    expect(screen.getByText('34 años')).toBeTruthy();
    expect(screen.getByText('01/01/1990')).toBeTruthy(); // dd/mm/aaaa
    expect(screen.getByText('12345678')).toBeTruthy();
    expect(screen.getByText('juan@example.com')).toBeTruthy();

    // Enlaces
    const editar = screen.getByRole('link', { name: 'Editar datos' });
    expect(editar.getAttribute('href')).toBe('/colaboradores/5/editar');
    const volver = screen.getByRole('link', { name: 'Volver al listado' });
    expect(volver.getAttribute('href')).toBe('/colaboradores');

    // Empresas: nombre comercial, país, fecha y puesto
    expect(screen.getByText('Otra Empresa')).toBeTruthy();
    expect(screen.getAllByText('Guatemala').length).toBeGreaterThanOrEqual(2);
    expect(screen.getByText('01/01/2022')).toBeTruthy();
    expect(screen.getByText('Consultor')).toBeTruthy();

    expect(screen.getByText('Mi Empresa')).toBeTruthy();
    expect(screen.getByText('01/01/2020')).toBeTruthy();
    expect(screen.getByText('Desarrollador')).toBeTruthy();

    // Botón Quitar habilitado con dos empresas
    const quitarBtns = screen.getAllByRole('button', { name: 'Quitar' });
    expect(quitarBtns.length).toBe(2);
    expect(
      quitarBtns[0].hasAttribute('disabled') ||
        quitarBtns[0].getAttribute('aria-disabled') === 'true',
    ).toBeFalsy();
  });

  it('quitar_QuitaEmpresa_SeDeshabilitaQuitarConUnaSolaYActualiza', async () => {
    const user = userEvent.setup();
    await render(ColaboradorDetalle, { inputs: { id: '5' }, providers: proveedoresDePrueba() });
    const api = TestBed.inject(HttpTestingController);
    api
      .expectOne((r) => r.method === 'GET' && r.url.endsWith('/api/colaboradores/5'))
      .flush(colaboradorMock);

    const quitarBtns = await screen.findAllByRole('button', { name: 'Quitar' });
    await user.click(quitarBtns[0]);

    // 409
    await user.click(await screen.findByRole('button', { name: 'Sí, eliminar' }));
    const req = api.expectOne((r) => r.method === 'DELETE');
    expect(req.request.url.endsWith('/api/colaboradores/5/empresas/2')).toBeTruthy();
    req.flush(conflicto('No se puede quitar.'), { status: 409, statusText: 'Conflict' });
    expect(await screen.findByText('No se puede quitar.')).toBeTruthy();

    // Ahora quitar de verdad
    await user.click(quitarBtns[0]);
    await user.click(await screen.findByRole('button', { name: 'Sí, eliminar' }));
    const req2 = api.expectOne((r) => r.method === 'DELETE');
    expect(req2.request.url.endsWith('/api/colaboradores/5/empresas/2')).toBeTruthy();
    req2.flush({}); // 204
    expect(await screen.findByText('Se eliminó correctamente.')).toBeTruthy();

    // Actualiza detalle
    const colaboradorMockConUnaEmpresa = {
      ...colaboradorMock,
      empresas: [colaboradorMock.empresas[1]],
    };
    api
      .expectOne((r) => r.method === 'GET' && r.url.endsWith('/api/colaboradores/5'))
      .flush(colaboradorMockConUnaEmpresa);

    // Quitar debe estar deshabilitado con una sola empresa
    const nuevoQuitar = await screen.findByRole('button', { name: 'Quitar' });
    expect(
      nuevoQuitar.hasAttribute('disabled') || nuevoQuitar.getAttribute('aria-disabled') === 'true',
    ).toBeTruthy();
  });

  it('asociar_AbreDialogo_Asocia_MuestraErroresYCierra', async () => {
    vi.useFakeTimers({ toFake: ['Date'] });
    vi.setSystemTime(new Date(2023, 9, 25, 12, 0, 0));

    const user = userEvent.setup();
    await render(ColaboradorDetalle, { inputs: { id: '5' }, providers: proveedoresDePrueba() });
    const api = TestBed.inject(HttpTestingController);
    api.expectOne((r) => r.url.endsWith('/api/colaboradores/5')).flush(colaboradorMock);

    await user.click(await screen.findByRole('button', { name: 'Asociar empresa' }));
    api
      .expectOne((r) => r.url.endsWith('/api/empresas') && r.params.get('tamanio') === '100')
      .flush(
        paginaDe([miEmpresa, { ...miEmpresa, id: 2, nombreComercial: 'Otra Empresa' }, empresa3]),
      );

    const dialogo = await screen.findByRole('dialog', { name: 'Asociar empresa' });

    // Cancelar
    await user.click(within(dialogo).getByRole('button', { name: 'Cancelar' }));
    api.expectNone((r) => r.method === 'POST');
    expect(screen.queryByRole('dialog')).toBeFalsy();

    // Abrir de nuevo y hacer POST
    await user.click(screen.getByRole('button', { name: 'Asociar empresa' }));
    api
      .expectOne((r) => r.url.endsWith('/api/empresas'))
      .flush(paginaDe([miEmpresa, { ...miEmpresa, id: 2 }, empresa3]));
    const dialogoNuevo = await screen.findByRole('dialog', { name: 'Asociar empresa' });

    await user.click(within(dialogoNuevo).getByLabelText('Empresa'));
    expect(screen.queryByRole('option', { name: 'Mi Empresa' })).toBeFalsy();
    expect(screen.queryByRole('option', { name: 'Otra Empresa' })).toBeFalsy();
    await user.click(await screen.findByRole('option', { name: 'Tercera Empresa' }));

    // Diálogo, VC1: sin fecha de ingreso, Este campo es obligatorio. dentro del diálogo y no hay POST.
    await user.click(within(dialogoNuevo).getByRole('button', { name: 'Guardar' }));
    expect(await within(dialogoNuevo).findByText('Este campo es obligatorio.')).toBeTruthy();
    api.expectNone((r) => r.method === 'POST');

    // Diálogo, VC5: max de Fecha de ingreso es hoy
    const ingreso = within(dialogoNuevo).getByLabelText('Fecha de ingreso') as HTMLInputElement;
    expect(ingreso.max).toBe('2023-10-25');

    await user.type(ingreso, '2023-01-01');

    // Error 400
    await user.click(within(dialogoNuevo).getByRole('button', { name: 'Guardar' }));
    let req = api.expectOne(
      (r) => r.method === 'POST' && r.url.endsWith('/api/colaboradores/5/empresas'),
    );
    req.flush(validacion({ FechaIngreso: ['Inválida'] }), {
      status: 400,
      statusText: 'Bad Request',
    });
    expect(await within(dialogoNuevo).findByText('Inválida')).toBeTruthy();

    // Error 409
    await user.click(within(dialogoNuevo).getByRole('button', { name: 'Guardar' }));
    req = api.expectOne((r) => r.method === 'POST');
    req.flush(conflicto('Conflicto asoc'), { status: 409, statusText: 'Conflict' });
    expect(await within(dialogoNuevo).findByText('Conflicto asoc')).toBeTruthy();

    // Éxito
    await user.click(within(dialogoNuevo).getByRole('button', { name: 'Guardar' }));
    req = api.expectOne((r) => r.method === 'POST');
    expect(req.request.body).toEqual({
      empresaId: 3,
      fechaIngreso: '2023-01-01',
      puesto: null,
    });
    // La API responde 200 con el colaborador actualizado
    req.flush({
      ...colaboradorMock,
      empresas: [
        ...colaboradorMock.empresas,
        {
          empresaId: 3,
          nombreComercial: 'Tercera Empresa',
          paisId: 1,
          paisNombre: 'Guatemala',
          fechaIngreso: '2023-01-01',
          puesto: null,
        },
      ],
    });

    expect(await screen.findByText('Se guardó correctamente.')).toBeTruthy();
    expect(screen.queryByRole('dialog')).toBeFalsy();

    expect(await screen.findByText('Tercera Empresa')).toBeTruthy();
  });

  it('asociar_ListaEmpresasFalla_MuestraRF8', async () => {
    const user = userEvent.setup();
    await render(ColaboradorDetalle, { inputs: { id: '5' }, providers: proveedoresDePrueba() });
    const api = TestBed.inject(HttpTestingController);
    api.expectOne((r) => r.url.endsWith('/api/colaboradores/5')).flush(colaboradorMock);

    await user.click(await screen.findByRole('button', { name: 'Asociar empresa' }));

    // Fallo de conexión
    api
      .expectOne((r) => r.url.endsWith('/api/empresas') && r.params.get('tamanio') === '100')
      .error(new ProgressEvent('error'));

    const dialogo = await screen.findByRole('dialog', { name: 'Asociar empresa' });
    expect(await within(dialogo).findByText('No se pudo conectar con la API.')).toBeTruthy();
  });

  it('editar_AbreDialogo_Edita_ActualizaDetalle', async () => {
    const user = userEvent.setup();
    await render(ColaboradorDetalle, { inputs: { id: '5' }, providers: proveedoresDePrueba() });
    const api = TestBed.inject(HttpTestingController);
    api.expectOne((r) => r.url.endsWith('/api/colaboradores/5')).flush(colaboradorMock);

    const editarBtns = await screen.findAllByRole('button', { name: 'Editar' });
    await user.click(editarBtns[0]);

    const dialogo = await screen.findByRole('dialog', { name: 'Editar empresa' });

    const empresaSelect = within(dialogo).getByLabelText('Empresa');
    expect(
      empresaSelect.getAttribute('aria-disabled') === 'true' ||
        empresaSelect.hasAttribute('disabled'),
    ).toBeTruthy();
    expect(empresaSelect.textContent?.includes('Otra Empresa')).toBeTruthy();

    const ingreso = within(dialogo).getByLabelText('Fecha de ingreso') as HTMLInputElement;
    expect(ingreso.value).toBe('2022-01-01');

    const puesto = within(dialogo).getByLabelText('Puesto (opcional)') as HTMLInputElement;
    expect(puesto.value).toBe('Consultor');

    await user.clear(puesto);
    await user.type(puesto, 'Consultor Senior');

    await user.click(within(dialogo).getByRole('button', { name: 'Guardar' }));

    const req = api.expectOne(
      (r) => r.method === 'PUT' && r.url.endsWith('/api/colaboradores/5/empresas/2'),
    );
    expect(req.request.body).toEqual({
      fechaIngreso: '2022-01-01',
      puesto: 'Consultor Senior',
    });

    // La API responde 200 con el colaborador actualizado
    req.flush({
      ...colaboradorMock,
      empresas: [
        { ...colaboradorMock.empresas[0], puesto: 'Consultor Senior' },
        colaboradorMock.empresas[1],
      ],
    });

    expect(await screen.findByText('Se guardó correctamente.')).toBeTruthy();
    expect(screen.queryByRole('dialog')).toBeFalsy();

    expect(await screen.findByText('Consultor Senior')).toBeTruthy();
  });
});

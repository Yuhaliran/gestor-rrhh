import { HttpTestingController } from '@angular/common/http/testing';

import { TestBed } from '@angular/core/testing';
import { Router } from '@angular/router';
import { render, screen } from '@testing-library/angular';
import userEvent from '@testing-library/user-event';
import { vi, describe, it, expect } from 'vitest';
import { PaisFormulario } from '../../src/app/vistas/paises/pais-formulario';
import { PaisesListado } from '../../src/app/vistas/paises/paises-listado';
import { proveedoresDePrueba } from '../apoyo/proveedores';
import { conflicto, guatemala, noEncontrado, paginaDe, validacion } from '../apoyo/respuestas';

describe('PaisesListado', () => {
  it('listar_CargaInicial_MuestraFilasYTotal', async () => {
    await render(PaisesListado, { providers: proveedoresDePrueba() });
    const api = TestBed.inject(HttpTestingController);

    const elSalvador = { ...guatemala, id: 2, nombre: 'El Salvador', codigoIso2: 'SV' };
    api.expectOne((r) => r.url.endsWith('/api/paises')).flush(paginaDe([guatemala, elSalvador]));

    expect(await screen.findByText('Guatemala')).toBeTruthy();
    expect(screen.getByText('El Salvador')).toBeTruthy();
    expect(screen.getByText('2 registros')).toBeTruthy();
  });

  it('buscar_ConTexto_Espera300msYConsultaEnPagina1', async () => {
    vi.useFakeTimers({ shouldAdvanceTime: true });
    const user = userEvent.setup({ advanceTimers: vi.advanceTimersByTime });

    await render(PaisesListado, { providers: proveedoresDePrueba() });
    const api = TestBed.inject(HttpTestingController);

    api.expectOne((r) => r.url.endsWith('/api/paises')).flush(paginaDe([guatemala]));

    const buscar = await screen.findByLabelText('Buscar');
    await user.type(buscar, 'sal');

    api.expectNone((r) => r.params.get('buscar') === 'sal');

    vi.advanceTimersByTime(300);

    const pedido = api.expectOne((r) => r.params.get('buscar') === 'sal');
    expect(pedido.request.params.get('pagina')).toBe('1');
    expect(pedido.request.params.get('buscar')).toBe('sal');

    const elSalvador = { ...guatemala, id: 2, nombre: 'El Salvador', codigoIso2: 'SV' };
    pedido.flush(paginaDe([elSalvador]));

    expect(await screen.findByText('El Salvador')).toBeTruthy();

    vi.useRealTimers();
  });

  it('eliminar_Confirma_HaceDelete', async () => {
    await render(PaisesListado, { providers: proveedoresDePrueba() });
    const api = TestBed.inject(HttpTestingController);
    api.expectOne((r) => r.url.endsWith('/api/paises')).flush(paginaDe([guatemala]));

    await userEvent.click(await screen.findByRole('button', { name: 'Eliminar' }));
    await userEvent.click(await screen.findByRole('button', { name: 'Sí, eliminar' }));

    api.expectOne((r) => r.method === 'DELETE' && r.url.endsWith('/api/paises/1')).flush(null);

    expect(await screen.findByText('Se eliminó correctamente.')).toBeTruthy();

    // Verifica que el listado vuelve a consultar (recarga)
    api.expectOne((r) => r.method === 'GET' && r.url.endsWith('/api/paises'));
  });

  it('eliminar_ConConflicto_MuestraElDetalleYConservaLaFila', async () => {
    await render(PaisesListado, { providers: proveedoresDePrueba() });
    const api = TestBed.inject(HttpTestingController);
    api.expectOne((r) => r.url.endsWith('/api/paises')).flush(paginaDe([guatemala]));

    await userEvent.click(await screen.findByRole('button', { name: 'Eliminar' }));
    await userEvent.click(await screen.findByRole('button', { name: 'Sí, eliminar' }));

    api
      .expectOne((r) => r.method === 'DELETE')
      .flush(conflicto('El país tiene departamentos.'), { status: 409, statusText: 'Conflict' });

    expect(await screen.findByText('El país tiene departamentos.')).toBeTruthy();
    expect(screen.getByText('Guatemala')).toBeTruthy();
  });

  it('listar_ConError_MuestraOcurrioUnErrorInesperado', async () => {
    await render(PaisesListado, { providers: proveedoresDePrueba() });
    const api = TestBed.inject(HttpTestingController);

    api
      .expectOne((r) => r.url.endsWith('/api/paises'))
      .flush({}, { status: 500, statusText: 'Server Error' });

    expect(await screen.findByText('Ocurrió un error inesperado.')).toBeTruthy();
  });

  it('listar_Paginador_MuestraTotalYCambiaPagina', async () => {
    const user = userEvent.setup();
    await render(PaisesListado, { providers: proveedoresDePrueba() });
    const api = TestBed.inject(HttpTestingController);

    api
      .expectOne((r) => r.url.endsWith('/api/paises'))
      .flush({ ...paginaDe([guatemala]), total: 45 });

    expect(await screen.findByText('Registros por página')).toBeTruthy();
    expect(screen.getByText('1 – 20 de 45')).toBeTruthy();

    await user.click(screen.getByRole('button', { name: 'Página siguiente' }));

    const pedido = api.expectOne((r) => r.params.get('pagina') === '2');
    expect(pedido.request.params.get('pagina')).toBe('2');
    pedido.flush({ ...paginaDe([guatemala]), total: 45 });
  });
});

describe('PaisFormulario', () => {
  it('crear_ValoresPorDefecto_Propone18100Y28DeFebrero', async () => {
    await render(PaisFormulario, { providers: proveedoresDePrueba() });

    expect(((await screen.findByLabelText('Edad mínima')) as HTMLInputElement).value).toBe('18');
    expect((screen.getByLabelText('Edad máxima') as HTMLInputElement).value).toBe('100');
    expect(screen.getByText('28 de febrero')).toBeTruthy();
  });

  it('crear_CodigoIso_SeEnviaEnMayusculas', async () => {
    const user = userEvent.setup();
    await render(PaisFormulario, { providers: proveedoresDePrueba() });

    await user.type(await screen.findByLabelText('Nombre'), 'Belice');
    await user.type(screen.getByLabelText('Código ISO'), 'bz');

    await user.click(screen.getByRole('button', { name: 'Guardar' }));

    const api = TestBed.inject(HttpTestingController);
    const pedido = api.expectOne((r) => r.method === 'POST');

    expect(pedido.request.body.codigoIso2).toBe('BZ');
    expect(pedido.request.body.nombre).toBe('Belice');
  });

  it('crear_CampoObligatorioVacio_MuestraMensajeYNoLlamaApi', async () => {
    const user = userEvent.setup();
    await render(PaisFormulario, { providers: proveedoresDePrueba() });

    const edadMinima = await screen.findByLabelText('Edad mínima');
    await user.clear(edadMinima);

    await user.click(screen.getByRole('button', { name: 'Guardar' }));

    expect(await screen.findAllByText('Este campo es obligatorio.')).toBeTruthy();

    const api = TestBed.inject(HttpTestingController);
    api.expectNone((r) => r.method === 'POST');
  });

  it('crear_ConError400_MuestraElMensajeDebajoDelCampo', async () => {
    const user = userEvent.setup();
    await render(PaisFormulario, { providers: proveedoresDePrueba() });

    await user.type(await screen.findByLabelText('Nombre'), 'Belice');
    await user.type(screen.getByLabelText('Código ISO'), 'BZ');

    await user.click(screen.getByRole('button', { name: 'Guardar' }));

    const api = TestBed.inject(HttpTestingController);
    api
      .expectOne((r) => r.method === 'POST')
      .flush(validacion({ Nombre: ['El nombre ya existe.'] }), {
        status: 400,
        statusText: 'Bad Request',
      });

    expect(await screen.findByText('El nombre ya existe.')).toBeTruthy();
  });

  it('crear_GuardarExito_AvisaYNavegaAlListado', async () => {
    const user = userEvent.setup();
    await render(PaisFormulario, { providers: proveedoresDePrueba() });

    const router = TestBed.inject(Router);
    const routerSpy = vi.spyOn(router, 'navigateByUrl').mockResolvedValue(true);

    await user.type(await screen.findByLabelText('Nombre'), 'Belice');
    await user.type(screen.getByLabelText('Código ISO'), 'BZ');

    await user.click(screen.getByRole('button', { name: 'Guardar' }));

    const api = TestBed.inject(HttpTestingController);
    api
      .expectOne((r) => r.method === 'POST')
      .flush({ ...guatemala, id: 7, nombre: 'Belice', codigoIso2: 'BZ' });

    expect(await screen.findByText('Se guardó correctamente.')).toBeTruthy();
    expect(routerSpy).toHaveBeenCalled();
    const navArg = routerSpy.mock.calls[0][0];
    expect(navArg.toString()).toContain('/paises');
  });

  it('editar_Abre_PrecargaLosDatos', async () => {
    await render(PaisFormulario, { inputs: { id: '7' }, providers: proveedoresDePrueba() });

    const api = TestBed.inject(HttpTestingController);
    const belice = { ...guatemala, id: 7, nombre: 'Belice', codigoIso2: 'BZ' };
    api.expectOne((r) => r.method === 'GET' && r.url.endsWith('/api/paises/7')).flush(belice);

    expect(((await screen.findByLabelText('Nombre')) as HTMLInputElement).value).toBe('Belice');
    expect((screen.getByLabelText('Código ISO') as HTMLInputElement).value).toBe('BZ');
  });

  it('editar_ConError404_MuestraElRegistroNoExisteYVolver', async () => {
    await render(PaisFormulario, { inputs: { id: '7' }, providers: proveedoresDePrueba() });

    const api = TestBed.inject(HttpTestingController);
    api
      .expectOne((r) => r.method === 'GET' && r.url.endsWith('/api/paises/7'))
      .flush(noEncontrado(), { status: 404, statusText: 'Not Found' });

    expect(await screen.findByText('El registro no existe.')).toBeTruthy();
    expect(screen.getByRole('link', { name: 'Volver al listado' })).toBeTruthy();
  });

  it('editar_Guardar_EnviaPutConDatosNuevos', async () => {
    const user = userEvent.setup();
    await render(PaisFormulario, { inputs: { id: '7' }, providers: proveedoresDePrueba() });

    const api = TestBed.inject(HttpTestingController);
    const router = TestBed.inject(Router);
    vi.spyOn(router, 'navigateByUrl').mockResolvedValue(true);
    const belice = { ...guatemala, id: 7, nombre: 'Belice', codigoIso2: 'BZ' };
    api.expectOne((r) => r.method === 'GET' && r.url.endsWith('/api/paises/7')).flush(belice);

    const nombre = await screen.findByLabelText('Nombre');
    await user.clear(nombre);
    await user.type(nombre, 'Belice Editado');

    await user.click(screen.getByRole('button', { name: 'Guardar' }));

    const pedido = api.expectOne((r) => r.method === 'PUT' && r.url.endsWith('/api/paises/7'));
    expect(pedido.request.body.nombre).toBe('Belice Editado');
    pedido.flush({ ...belice, nombre: 'Belice Editado' });
  });

  it('crear_EdadMinimaMayorQueMaxima_MuestraErrorYNoLlamaApi', async () => {
    const user = userEvent.setup();
    await render(PaisFormulario, { providers: proveedoresDePrueba() });

    const min = await screen.findByLabelText('Edad mínima');
    const max = screen.getByLabelText('Edad máxima');

    await user.clear(min);
    await user.type(min, '30');
    await user.clear(max);
    await user.type(max, '20');

    await user.click(screen.getByRole('button', { name: 'Guardar' }));

    expect(
      await screen.findByText('La edad mínima no puede ser mayor que la máxima.'),
    ).toBeTruthy();

    const api = TestBed.inject(HttpTestingController);
    api.expectNone((r) => r.method === 'POST');
  });
});

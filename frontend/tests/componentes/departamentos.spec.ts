import { TestBed } from '@angular/core/testing';
import { render, screen } from '@testing-library/angular';
import { userEvent } from '@testing-library/user-event';
import { HttpTestingController } from '@angular/common/http/testing';
import { Router } from '@angular/router';
import { describe, expect, it, vi } from 'vitest';

import { proveedoresDePrueba } from '../apoyo/proveedores';
import { conflicto, guatemala, guatemalaDepto, noEncontrado, paginaDe } from '../apoyo/respuestas';
import { DepartamentosListado } from '../../src/app/vistas/departamentos/departamentos-listado';
import { DepartamentoFormulario } from '../../src/app/vistas/departamentos/departamento-formulario';

const deptoPrueba = {
  ...guatemalaDepto,
  id: 2,
  nombre: 'Petén',
  paisId: 1,
  paisNombre: 'Guatemala',
};

describe('DepartamentosListado', () => {
  it('listar_MuestraFilasConNombreYPaisYTotal', async () => {
    await render(DepartamentosListado, { providers: proveedoresDePrueba() });
    const api = TestBed.inject(HttpTestingController);

    api.expectOne((r) => r.url.endsWith('/api/departamentos')).flush(paginaDe([deptoPrueba]));

    expect(await screen.findByText('Petén')).toBeTruthy();
    expect(screen.getByText('Guatemala')).toBeTruthy();
    expect(screen.getByText('1 registro')).toBeTruthy();
  });

  it('eliminar_ConConflicto_MuestraElDetalle', async () => {
    const user = userEvent.setup();
    await render(DepartamentosListado, { providers: proveedoresDePrueba() });
    const api = TestBed.inject(HttpTestingController);

    api.expectOne((r) => r.url.endsWith('/api/departamentos')).flush(paginaDe([deptoPrueba]));

    await user.click(await screen.findByRole('button', { name: 'Eliminar' }));
    await user.click(await screen.findByRole('button', { name: 'Sí, eliminar' }));

    api
      .expectOne((r) => r.method === 'DELETE' && r.url.endsWith('/api/departamentos/2'))
      .flush(conflicto('El departamento tiene municipios.'), {
        status: 409,
        statusText: 'Conflict',
      });

    expect(await screen.findByText('El departamento tiene municipios.')).toBeTruthy();
  });
});

describe('DepartamentoFormulario', () => {
  it('crear_SeleccionarPaisYEscribirNombre_HacePostConPaisIdYNombre', async () => {
    const user = userEvent.setup();
    await render(DepartamentoFormulario, { providers: proveedoresDePrueba() });
    const api = TestBed.inject(HttpTestingController);

    const pedidoPaises = api.expectOne((r) => r.method === 'GET' && r.url.endsWith('/api/paises'));
    expect(pedidoPaises.request.params.get('tamanio')).toBe('100');
    pedidoPaises.flush(paginaDe([{ ...guatemala, id: 1, nombre: 'Guatemala' }]));

    const router = TestBed.inject(Router);
    vi.spyOn(router, 'navigateByUrl').mockResolvedValue(true);

    const paisSelect = await screen.findByLabelText('País');
    await user.click(paisSelect);

    const opcionGuatemala = await screen.findByRole('option', { name: 'Guatemala' });
    await user.click(opcionGuatemala);

    const nombre = screen.getByLabelText('Nombre');
    await user.type(nombre, 'Petén');

    await user.click(screen.getByRole('button', { name: 'Guardar' }));

    const pedido = api.expectOne((r) => r.method === 'POST');
    expect(pedido.request.body.paisId).toBe(1);
    expect(pedido.request.body.nombre).toBe('Petén');

    pedido.flush({ ...deptoPrueba, nombre: 'Petén' });
  });

  it('editar_Abre_PaisEstaDeshabilitadoYSeMuestraElNombre', async () => {
    await render(DepartamentoFormulario, { inputs: { id: '2' }, providers: proveedoresDePrueba() });
    const api = TestBed.inject(HttpTestingController);

    api
      .expectOne((r) => r.method === 'GET' && r.url.endsWith('/api/departamentos/2'))
      .flush(deptoPrueba);

    const nombreInput = (await screen.findByLabelText('Nombre')) as HTMLInputElement;
    expect(nombreInput.value).toBe('Petén');

    const paisSelect = screen.getByLabelText('País');
    expect(
      paisSelect.getAttribute('aria-disabled') === 'true' || paisSelect.hasAttribute('disabled'),
    ).toBeTruthy();
    expect(await screen.findByText('Guatemala')).toBeTruthy();
  });

  it('editar_Guardar_EnviaPutConElMismoPaisId', async () => {
    const user = userEvent.setup();
    await render(DepartamentoFormulario, { inputs: { id: '2' }, providers: proveedoresDePrueba() });
    const api = TestBed.inject(HttpTestingController);

    const router = TestBed.inject(Router);
    vi.spyOn(router, 'navigateByUrl').mockResolvedValue(true);

    api
      .expectOne((r) => r.method === 'GET' && r.url.endsWith('/api/departamentos/2'))
      .flush(deptoPrueba);

    const nombreInput = await screen.findByLabelText('Nombre');
    await user.clear(nombreInput);
    await user.type(nombreInput, 'Petén Editado');

    await user.click(screen.getByRole('button', { name: 'Guardar' }));

    const pedido = api.expectOne(
      (r) => r.method === 'PUT' && r.url.endsWith('/api/departamentos/2'),
    );
    expect(pedido.request.body.paisId).toBe(1);
    expect(pedido.request.body.nombre).toBe('Petén Editado');
    pedido.flush({ ...deptoPrueba, nombre: 'Petén Editado' });
  });

  it('editar_ConError404_MuestraElRegistroNoExiste', async () => {
    await render(DepartamentoFormulario, { inputs: { id: '2' }, providers: proveedoresDePrueba() });
    const api = TestBed.inject(HttpTestingController);

    api
      .expectOne((r) => r.method === 'GET' && r.url.endsWith('/api/departamentos/2'))
      .flush(noEncontrado(), { status: 404, statusText: 'Not Found' });

    expect(await screen.findByText('El registro no existe.')).toBeTruthy();
  });
});

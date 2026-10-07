import { TestBed } from '@angular/core/testing';
import { render, screen } from '@testing-library/angular';
import { userEvent } from '@testing-library/user-event';
import { HttpTestingController } from '@angular/common/http/testing';
import { Router } from '@angular/router';
import { describe, expect, it, vi } from 'vitest';

import { proveedoresDePrueba } from '../apoyo/proveedores';
import { conflicto, guatemala, guatemalaDepto, guatemalaMuni, paginaDe } from '../apoyo/respuestas';
import { MunicipiosListado } from '../../src/app/vistas/municipios/municipios-listado';
import { MunicipioFormulario } from '../../src/app/vistas/municipios/municipio-formulario';

const muniPrueba = {
  ...guatemalaMuni,
  id: 3,
  nombre: 'Flores',
  departamentoId: 2,
  departamentoNombre: 'Petén',
  paisId: 1,
  paisNombre: 'Guatemala',
};

const deptoPrueba = {
  ...guatemalaDepto,
  id: 2,
  nombre: 'Petén',
  paisId: 1,
  paisNombre: 'Guatemala',
};

describe('MunicipiosListado', () => {
  it('listar_MuestraFilasConNombreDepartamentoYPaisYTotal', async () => {
    await render(MunicipiosListado, { providers: proveedoresDePrueba() });
    const api = TestBed.inject(HttpTestingController);

    api.expectOne((r) => r.url.endsWith('/api/municipios')).flush(paginaDe([muniPrueba]));

    expect(await screen.findByText('Flores')).toBeTruthy();
    expect(screen.getByText('Petén')).toBeTruthy();
    expect(screen.getByText('Guatemala')).toBeTruthy();
    expect(screen.getByText('1 registro')).toBeTruthy();
  });

  it('eliminar_ConConflicto_MuestraElDetalle', async () => {
    const user = userEvent.setup();
    await render(MunicipiosListado, { providers: proveedoresDePrueba() });
    const api = TestBed.inject(HttpTestingController);

    api.expectOne((r) => r.url.endsWith('/api/municipios')).flush(paginaDe([muniPrueba]));

    await user.click(await screen.findByRole('button', { name: 'Eliminar' }));
    await user.click(await screen.findByRole('button', { name: 'Sí, eliminar' }));

    api
      .expectOne((r) => r.method === 'DELETE' && r.url.endsWith('/api/municipios/3'))
      .flush(conflicto('El municipio está en uso.'), {
        status: 409,
        statusText: 'Conflict',
      });

    expect(await screen.findByText('El municipio está en uso.')).toBeTruthy();
  });
});

describe('MunicipioFormulario', () => {
  it('crear_CampoObligatorioVacio_MuestraMensajeYNoLlamaApi', async () => {
    const user = userEvent.setup();
    await render(MunicipioFormulario, { providers: proveedoresDePrueba() });
    const api = TestBed.inject(HttpTestingController);

    api
      .expectOne((r) => r.url.endsWith('/api/paises') && r.params.get('tamanio') === '100')
      .flush(paginaDe([guatemala]));

    const botonGuardar = await screen.findByRole('button', { name: 'Guardar' });
    await user.click(botonGuardar);

    const mensajes = await screen.findAllByText('Este campo es obligatorio.');
    expect(mensajes.length).toBeGreaterThan(0);
    api.expectNone((r) => r.method === 'POST');

    api.verify();
  });

  it('crear_AltaEnCascada_ElegirPaisYDepto_HacePostConDepartamentoIdYNombre', async () => {
    const user = userEvent.setup();
    await render(MunicipioFormulario, { providers: proveedoresDePrueba() });
    const api = TestBed.inject(HttpTestingController);

    api
      .expectOne((r) => r.method === 'GET' && r.url.endsWith('/api/paises'))
      .flush(paginaDe([{ ...guatemala, id: 1, nombre: 'Guatemala' }]));

    const deptoSelect = await screen.findByLabelText('Departamento');
    expect(
      deptoSelect.getAttribute('aria-disabled') === 'true' || deptoSelect.hasAttribute('disabled'),
    ).toBeTruthy();

    const router = TestBed.inject(Router);
    vi.spyOn(router, 'navigateByUrl').mockResolvedValue(true);

    const paisSelect = screen.getByLabelText('País');
    await user.click(paisSelect);
    const opcionGuatemala = await screen.findByRole('option', { name: 'Guatemala' });
    await user.click(opcionGuatemala);

    api
      .expectOne((r) => r.method === 'GET' && r.url.endsWith('/api/paises/1/departamentos'))
      .flush([deptoPrueba]);

    await user.click(screen.getByLabelText('Departamento'));
    const opcionPeten = await screen.findByRole('option', { name: 'Petén' });
    await user.click(opcionPeten);

    const nombre = screen.getByLabelText('Nombre');
    await user.type(nombre, 'Flores');

    await user.click(screen.getByRole('button', { name: 'Guardar' }));

    const pedido = api.expectOne((r) => r.method === 'POST');
    expect(pedido.request.body.departamentoId).toBe(2);
    expect(pedido.request.body.nombre).toBe('Flores');

    pedido.flush({ ...muniPrueba, nombre: 'Flores' });
  });

  it('crear_CambiarDePais_VaciaElDepartamento', async () => {
    const user = userEvent.setup();
    await render(MunicipioFormulario, { providers: proveedoresDePrueba() });
    const api = TestBed.inject(HttpTestingController);

    api
      .expectOne((r) => r.url.endsWith('/api/paises'))
      .flush(
        paginaDe([
          { ...guatemala, id: 1, nombre: 'Guatemala' },
          { ...guatemala, id: 9, nombre: 'El Salvador' },
        ]),
      );

    const paisSelect = await screen.findByLabelText('País');
    await user.click(paisSelect);
    const opcionGuatemala = await screen.findByRole('option', { name: 'Guatemala' });
    await user.click(opcionGuatemala);

    api.expectOne((r) => r.url.endsWith('/api/paises/1/departamentos')).flush([deptoPrueba]);

    await user.click(screen.getByLabelText('Departamento'));
    const opcionPeten = await screen.findByRole('option', { name: 'Petén' });
    await user.click(opcionPeten);

    expect(screen.getByText('Petén')).toBeTruthy();

    await user.click(screen.getByLabelText('País'));
    const opcionElSalvador = await screen.findByRole('option', { name: 'El Salvador' });
    await user.click(opcionElSalvador);

    api.expectOne((r) => r.url.endsWith('/api/paises/9/departamentos')).flush([]);

    expect(screen.queryByText('Petén')).toBeFalsy();
  });

  it('editar_Abre_PaisYDepartamentoEstanDeshabilitadosYSeMuestranNombres', async () => {
    await render(MunicipioFormulario, { inputs: { id: '3' }, providers: proveedoresDePrueba() });
    const api = TestBed.inject(HttpTestingController);

    api
      .expectOne((r) => r.method === 'GET' && r.url.endsWith('/api/municipios/3'))
      .flush(muniPrueba);

    const nombreInput = (await screen.findByLabelText('Nombre')) as HTMLInputElement;
    expect(nombreInput.value).toBe('Flores');

    const paisSelect = screen.getByLabelText('País');
    expect(
      paisSelect.getAttribute('aria-disabled') === 'true' || paisSelect.hasAttribute('disabled'),
    ).toBeTruthy();
    expect(await screen.findByText('Guatemala')).toBeTruthy();

    const deptoSelect = screen.getByLabelText('Departamento');
    expect(
      deptoSelect.getAttribute('aria-disabled') === 'true' || deptoSelect.hasAttribute('disabled'),
    ).toBeTruthy();
    expect(await screen.findByText('Petén')).toBeTruthy();
  });

  it('editar_Guardar_EnviaPutConElMismoDepartamentoId', async () => {
    const user = userEvent.setup();
    await render(MunicipioFormulario, { inputs: { id: '3' }, providers: proveedoresDePrueba() });
    const api = TestBed.inject(HttpTestingController);

    const router = TestBed.inject(Router);
    vi.spyOn(router, 'navigateByUrl').mockResolvedValue(true);

    api
      .expectOne((r) => r.method === 'GET' && r.url.endsWith('/api/municipios/3'))
      .flush(muniPrueba);

    const nombreInput = await screen.findByLabelText('Nombre');
    await user.clear(nombreInput);
    await user.type(nombreInput, 'Flores Editado');

    await user.click(screen.getByRole('button', { name: 'Guardar' }));

    const pedido = api.expectOne((r) => r.method === 'PUT' && r.url.endsWith('/api/municipios/3'));
    expect(pedido.request.body.departamentoId).toBe(2);
    expect(pedido.request.body.nombre).toBe('Flores Editado');
    pedido.flush({ ...muniPrueba, nombre: 'Flores Editado' });
  });
});

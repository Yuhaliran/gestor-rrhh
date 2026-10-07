import { TestBed } from '@angular/core/testing';
import { HttpTestingController } from '@angular/common/http/testing';
import { CLIENTE_RRHH } from '../../src/app/servicios/cliente';
import { ErrorApi } from '../../src/app/contratos/errores';
import { proveedoresDePrueba } from '../apoyo/proveedores';
import { conflicto, guatemala, jsonIlegible, noEncontrado, validacion } from '../apoyo/respuestas';

describe('ClienteRrhhHttp', () => {
  it('listar_ConBusqueda_EnviaPaginaTamanioYBuscar', () => {
    TestBed.configureTestingModule({ providers: proveedoresDePrueba() });
    const cliente = TestBed.inject(CLIENTE_RRHH);
    const api = TestBed.inject(HttpTestingController);

    cliente.paises.listar({ pagina: 2, tamanio: 10, buscar: 'gua' }).subscribe();
    const pedido = api.expectOne((r) => r.url.startsWith('http://api.prueba/api/paises'));

    expect(pedido.request.params.get('pagina')).toBe('2');
    expect(pedido.request.params.get('tamanio')).toBe('10');
    expect(pedido.request.params.get('buscar')).toBe('gua');
    api.verify();
  });

  it('listar_SinBuscar_NoEnviaBuscar', () => {
    TestBed.configureTestingModule({ providers: proveedoresDePrueba() });
    const cliente = TestBed.inject(CLIENTE_RRHH);
    const api = TestBed.inject(HttpTestingController);

    cliente.paises.listar({ pagina: 1, tamanio: 10 }).subscribe();
    const pedido = api.expectOne('http://api.prueba/api/paises?pagina=1&tamanio=10');
    expect(pedido.request.params.has('buscar')).toBe(false);
    api.verify();
  });

  it('crear_CuerpoJsonYContentType_PoneEncabezado', () => {
    TestBed.configureTestingModule({ providers: proveedoresDePrueba() });
    const cliente = TestBed.inject(CLIENTE_RRHH);
    const api = TestBed.inject(HttpTestingController);

    cliente.paises.crear(guatemala).subscribe();
    const pedido = api.expectOne('http://api.prueba/api/paises');
    expect(pedido.request.body).toEqual(guatemala);
    expect(pedido.request.headers.get('Content-Type')).toBe('application/json');
    api.verify();
  });

  it('eliminar_204_NoDevuelveCuerpo', () => {
    TestBed.configureTestingModule({ providers: proveedoresDePrueba() });
    const cliente = TestBed.inject(CLIENTE_RRHH);
    const api = TestBed.inject(HttpTestingController);

    let completo = false;
    cliente.paises.eliminar(1).subscribe({ complete: () => (completo = true) });

    api
      .expectOne('http://api.prueba/api/paises/1')
      .flush(null, { status: 204, statusText: 'No Content' });
    expect(completo).toBe(true);
    api.verify();
  });
});

describe('erroresInterceptor', () => {
  it('interceptor_Error400_NormalizaClavesYDevuelveErrorApi', () => {
    TestBed.configureTestingModule({ providers: proveedoresDePrueba() });
    const cliente = TestBed.inject(CLIENTE_RRHH);
    const api = TestBed.inject(HttpTestingController);

    let error: ErrorApi | undefined;
    cliente.paises.crear(guatemala).subscribe({ error: (e) => (error = e) });

    api
      .expectOne('http://api.prueba/api/paises')
      .flush(validacion({ Nombre: ['Mensaje 1'], 'Empresas[1].EmpresaId': ['Mensaje 2'] }), {
        status: 400,
        statusText: 'Bad Request',
      });

    expect(error?.estado).toBe(400);
    expect(error?.errores['nombre']).toEqual(['Mensaje 1']);
    expect(error?.errores['empresas[1].empresaId']).toEqual(['Mensaje 2']);
  });

  it('interceptor_Error400JsonIlegible_LimpiaClavesYMensaje', () => {
    TestBed.configureTestingModule({ providers: proveedoresDePrueba() });
    const cliente = TestBed.inject(CLIENTE_RRHH);
    const api = TestBed.inject(HttpTestingController);

    let error: ErrorApi | undefined;
    cliente.paises.crear(guatemala).subscribe({ error: (e) => (error = e) });

    api
      .expectOne('http://api.prueba/api/paises')
      .flush(jsonIlegible('fechaNacimiento'), { status: 400, statusText: 'Bad Request' });

    expect(error?.estado).toBe(400);
    expect(error?.errores['dto']).toBeUndefined();
    expect(error?.errores['fechaNacimiento']).toEqual(['El valor no es válido.']);
  });

  it('interceptor_Error404_DevuelveErrorApiConDetalle', () => {
    TestBed.configureTestingModule({ providers: proveedoresDePrueba() });
    const cliente = TestBed.inject(CLIENTE_RRHH);
    const api = TestBed.inject(HttpTestingController);

    let error: ErrorApi | undefined;
    cliente.paises.obtener(999).subscribe({ error: (e) => (error = e) });

    api
      .expectOne('http://api.prueba/api/paises/999')
      .flush(noEncontrado(), { status: 404, statusText: 'Not Found' });

    expect(error?.estado).toBe(404);
    expect(error?.detalle).toBe('El registro no existe.');
  });

  it('interceptor_Error409_DevuelveErrorApiConDetalle', () => {
    TestBed.configureTestingModule({ providers: proveedoresDePrueba() });
    const cliente = TestBed.inject(CLIENTE_RRHH);
    const api = TestBed.inject(HttpTestingController);

    let error: ErrorApi | undefined;
    cliente.paises.eliminar(1).subscribe({ error: (e) => (error = e) });

    api
      .expectOne('http://api.prueba/api/paises/1')
      .flush(conflicto('En uso.'), { status: 409, statusText: 'Conflict' });

    expect(error?.estado).toBe(409);
    expect(error?.detalle).toBe('En uso.');
  });

  it('interceptor_Error500_DevuelveErrorApiSoloConEstado', () => {
    TestBed.configureTestingModule({ providers: proveedoresDePrueba() });
    const cliente = TestBed.inject(CLIENTE_RRHH);
    const api = TestBed.inject(HttpTestingController);

    let error: ErrorApi | undefined;
    cliente.paises.listar({ pagina: 1, tamanio: 10 }).subscribe({ error: (e) => (error = e) });

    api
      .expectOne('http://api.prueba/api/paises?pagina=1&tamanio=10')
      .flush('Error interno', { status: 500, statusText: 'Internal Server Error' });

    expect(error?.estado).toBe(500);
    expect(error?.detalle).toBeUndefined();
    expect(Object.keys(error?.errores || {}).length).toBe(0);
  });

  it('interceptor_SinConexion_DevuelveErrorApiConEstado0', () => {
    TestBed.configureTestingModule({ providers: proveedoresDePrueba() });
    const cliente = TestBed.inject(CLIENTE_RRHH);
    const api = TestBed.inject(HttpTestingController);

    let error: ErrorApi | undefined;
    cliente.paises.listar({ pagina: 1, tamanio: 10 }).subscribe({ error: (e) => (error = e) });

    api
      .expectOne('http://api.prueba/api/paises?pagina=1&tamanio=10')
      .error(new ProgressEvent('error'));

    expect(error?.estado).toBe(0);
  });
});

import { TestBed, fakeAsync, tick } from '@angular/core/testing';
import { of, throwError, Subject } from 'rxjs';
import { crearListado } from '../../src/app/servicios/listado';
import { paginaDe } from '../apoyo/respuestas';
import { Consulta, Pagina } from '../../src/app/contratos/dtos';

describe('Listado', () => {
  it('cargaInicial_PidePagina1YTamanioOpciones', () => {
    let consultaEnviada: Consulta | undefined;
    TestBed.runInInjectionContext(() => {
      crearListado(
        (c) => {
          consultaEnviada = c;
          return of(paginaDe([]));
        },
        { tamanio: 50 },
      );
    });
    expect(consultaEnviada!.pagina).toBe(1);
    expect(consultaEnviada!.tamanio).toBe(50);
  });

  it('cambiarPagina_PideNuevaPagina', () => {
    let consultaEnviada: Consulta | undefined;
    TestBed.runInInjectionContext(() => {
      const listado = crearListado((c) => {
        consultaEnviada = c;
        return of(paginaDe([]));
      });
      listado.cambiarPagina(3);
    });
    expect(consultaEnviada!.pagina).toBe(3);
  });

  it('cambiarTamanio_VuelveAPagina1YPideNuevoTamanio', () => {
    let consultaEnviada: Consulta | undefined;
    TestBed.runInInjectionContext(() => {
      const listado = crearListado((c) => {
        consultaEnviada = c;
        return of(paginaDe([]));
      });
      listado.cambiarPagina(3);
      listado.cambiarTamanio(50);
    });
    expect(consultaEnviada!.pagina).toBe(1);
    expect(consultaEnviada!.tamanio).toBe(50);
  });

  it('cambiarBusqueda_Espera300msYVuelveAPagina1', fakeAsync(() => {
    let llamadas = 0;
    let consultaEnviada: Consulta | undefined;
    TestBed.runInInjectionContext(() => {
      const listado = crearListado((c) => {
        llamadas++;
        consultaEnviada = c;
        return of(paginaDe([]));
      });

      listado.cambiarPagina(3);
      listado.cambiarBusqueda('a');
      tick(100);
      listado.cambiarBusqueda('ab');
      tick(300);
    });

    expect(llamadas).toBe(2); // 1 inicial + 1 por la búsqueda
    expect(consultaEnviada!.pagina).toBe(1);
    expect(consultaEnviada!.buscar).toBe('ab');
  }));

  it('respuestasMismaBusqueda_DescartaRespuestaVieja', fakeAsync(() => {
    const respuestas = new Subject<Pagina<string>>();
    TestBed.runInInjectionContext(() => {
      const listado = crearListado(() => respuestas.asObservable());
      listado.cambiarBusqueda('a');
      tick(300);

      listado.cambiarBusqueda('ab');
      tick(300);

      respuestas.next(paginaDe(['vieja']));
      expect(listado.elementos()).toEqual([]);

      respuestas.next(paginaDe(['nueva']));
      expect(listado.elementos()).toEqual(['nueva']);
    });
  }));

  it('error_GuardaErrorSinRomperListado', fakeAsync(() => {
    let fallar = true;
    TestBed.runInInjectionContext(() => {
      const listado = crearListado(() =>
        fallar ? throwError(() => ({ estado: 500 })) : of(paginaDe(['nueva'])),
      );

      expect(listado.error()?.estado).toBe(500);
      expect(listado.cargando()).toBe(false);

      fallar = false;
      listado.recargar();

      expect(listado.error()).toBeNull();
      expect(listado.elementos()).toEqual(['nueva']);
    });
  }));
});

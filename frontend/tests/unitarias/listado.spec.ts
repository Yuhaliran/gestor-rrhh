import { TestBed } from '@angular/core/testing';
import { of, throwError, Subject } from 'rxjs';
import { crearListado } from '../../src/app/servicios/listado';
import { paginaDe } from '../apoyo/respuestas';
import { Consulta, Pagina } from '../../src/app/contratos/dtos';

describe('Listado', () => {
  beforeEach(() => {
    vi.useFakeTimers();
  });

  afterEach(() => {
    vi.useRealTimers();
  });

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

  it('cambiarBusqueda_Espera300msYVuelveAPagina1', () => {
    let llamadas = 0;
    let consultaEnviada: Consulta | undefined;
    TestBed.runInInjectionContext(() => {
      const listado = crearListado((c) => {
        llamadas++;
        consultaEnviada = c;
        return of(paginaDe([]));
      });

      listado.cambiarPagina(3);

      // Contar desde después de cambiarPagina
      llamadas = 0;

      listado.cambiarBusqueda('a');
      vi.advanceTimersByTime(100);
      listado.cambiarBusqueda('ab');
      vi.advanceTimersByTime(300);
    });

    expect(llamadas).toBe(1); // 'a' no consulta, 'ab' consulta una vez
    expect(consultaEnviada!.pagina).toBe(1);
    expect(consultaEnviada!.buscar).toBe('ab');
  });

  it('respuestasMismaBusqueda_DescartaRespuestaVieja', () => {
    const respuestas: Subject<Pagina<string>>[] = [];

    TestBed.runInInjectionContext(() => {
      const listado = crearListado(() => {
        const subject = new Subject<Pagina<string>>();
        respuestas.push(subject);
        return subject.asObservable();
      });

      // carga inicial crea respuestas[0]
      respuestas[0].next(paginaDe(['inicial']));

      listado.cambiarBusqueda('a');
      vi.advanceTimersByTime(300);
      // cambiarBusqueda 'a' crea respuestas[1]

      listado.cambiarBusqueda('ab');
      vi.advanceTimersByTime(300);
      // cambiarBusqueda 'ab' crea respuestas[2]

      // Emitimos de la búsqueda vieja ('a')
      respuestas[1].next(paginaDe(['vieja']));
      expect(listado.elementos()).toEqual(['inicial']);

      // Emitimos de la búsqueda nueva ('ab')
      respuestas[2].next(paginaDe(['nueva']));
      expect(listado.elementos()).toEqual(['nueva']);
    });
  });

  it('error_GuardaErrorSinRomperListado', () => {
    let fallar = false;
    TestBed.runInInjectionContext(() => {
      const listado = crearListado(() =>
        fallar ? throwError(() => ({ estado: 500 })) : of(paginaDe(['correcta'])),
      );

      // Carga correcta primero
      expect(listado.elementos()).toEqual(['correcta']);
      expect(listado.error()).toBeNull();

      // Carga que falla
      fallar = true;
      listado.recargar();

      expect(listado.error()?.estado).toBe(500);
      expect(listado.elementos()).toEqual(['correcta']);
      expect(listado.cargando()).toBe(false);
    });
  });
});

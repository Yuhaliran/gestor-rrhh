/* eslint-disable @typescript-eslint/no-explicit-any */
import { TestBed } from '@angular/core/testing';
import { of } from 'rxjs';
import { crearCascada } from '../../src/app/servicios/cascada';
import { CLIENTE_RRHH } from '../../src/app/servicios/cliente';
import { guatemala, guatemalaDepto, guatemalaMuni, paginaDe } from '../apoyo/respuestas';

describe('Cascada', () => {
  let clienteFalso: any;

  beforeEach(() => {
    clienteFalso = {
      paises: {
        listar: () => of(paginaDe([guatemala])),
        departamentos: () => of([guatemalaDepto]),
      },
      departamentos: {
        municipios: () => of([guatemalaMuni]),
      },
    };

    TestBed.configureTestingModule({
      providers: [{ provide: CLIENTE_RRHH, useValue: clienteFalso }],
    });
  });

  it('cargaInicial_SinInicial_CargaPaisesYVaciaSeleccion', () => {
    TestBed.runInInjectionContext(() => {
      const cascada = crearCascada();

      expect(cascada.paises()).toEqual([guatemala]);
      expect(cascada.departamentos()).toEqual([]);
      expect(cascada.municipios()).toEqual([]);
      expect(cascada.seleccion()).toEqual({
        paisId: null,
        departamentoId: null,
        municipioId: null,
      });
    });
  });

  it('cargaInicial_ConInicial_CargaPaisesDepartamentosYMunicipiosSinVaciar', () => {
    TestBed.runInInjectionContext(() => {
      const cascada = crearCascada({ paisId: 1, departamentoId: 1, municipioId: 1 });

      expect(cascada.paises()).toEqual([guatemala]);
      expect(cascada.departamentos()).toEqual([guatemalaDepto]);
      expect(cascada.municipios()).toEqual([guatemalaMuni]);
      expect(cascada.seleccion()).toEqual({ paisId: 1, departamentoId: 1, municipioId: 1 });
    });
  });

  it('elegirPais_CambiaPaisVaciaHijosYCargaDepartamentos', () => {
    TestBed.runInInjectionContext(() => {
      const cascada = crearCascada({ paisId: 1, departamentoId: 1, municipioId: 1 });

      cascada.elegirPais(1);

      expect(cascada.seleccion()).toEqual({ paisId: 1, departamentoId: null, municipioId: null });
      expect(cascada.departamentos()).toEqual([guatemalaDepto]);
      expect(cascada.municipios()).toEqual([]);
    });
  });

  it('elegirDepartamento_CambiaDepartamentoVaciaMunicipioYCargaMunicipios', () => {
    TestBed.runInInjectionContext(() => {
      const cascada = crearCascada({ paisId: 1, departamentoId: 1, municipioId: 1 });

      cascada.elegirDepartamento(1);

      expect(cascada.seleccion()).toEqual({ paisId: 1, departamentoId: 1, municipioId: null });
      expect(cascada.municipios()).toEqual([guatemalaMuni]);
    });
  });

  it('elegirMunicipio_CambiaMunicipio', () => {
    TestBed.runInInjectionContext(() => {
      const cascada = crearCascada({ paisId: 1, departamentoId: 1, municipioId: null });

      cascada.elegirMunicipio(1);

      expect(cascada.seleccion()).toEqual({ paisId: 1, departamentoId: 1, municipioId: 1 });
    });
  });
});

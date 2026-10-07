/* eslint-disable @typescript-eslint/no-explicit-any */
import { TestBed } from '@angular/core/testing';
import { of } from 'rxjs';
import { crearCascada } from '../../src/app/servicios/cascada';
import { CLIENTE_RRHH } from '../../src/app/servicios/cliente';
import { paginaDe } from '../apoyo/respuestas';

describe('Cascada', () => {
  let clienteFalso: any;
  let listarTamanio: number | undefined;
  let deptoPaisId: number | undefined;
  let muniDeptoId: number | undefined;

  beforeEach(() => {
    listarTamanio = undefined;
    deptoPaisId = undefined;
    muniDeptoId = undefined;

    clienteFalso = {
      paises: {
        listar: (c: any) => {
          listarTamanio = c?.tamanio;
          return of(
            paginaDe([
              { id: 1, nombre: 'Guatemala', codigoIso2: 'GT', edadMinima: 18, edadMaxima: 100 },
            ]),
          );
        },
        departamentos: (id: number) => {
          deptoPaisId = id;
          return of([{ id: id * 10, paisId: id, nombre: `Depto ${id}` }]);
        },
      },
      departamentos: {
        municipios: (id: number) => {
          muniDeptoId = id;
          return of([{ id: id * 10, departamentoId: id, nombre: `Muni ${id}` }]);
        },
      },
    };

    TestBed.configureTestingModule({
      providers: [{ provide: CLIENTE_RRHH, useValue: clienteFalso }],
    });
  });

  it('cargaInicial_SinInicial_CargaPaisesYVaciaSeleccion', () => {
    TestBed.runInInjectionContext(() => {
      const cascada = crearCascada();

      expect(cascada.paises()).toEqual([
        { id: 1, nombre: 'Guatemala', codigoIso2: 'GT', edadMinima: 18, edadMaxima: 100 },
      ]);
      expect(listarTamanio).toBe(100);
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

      expect(cascada.paises()).toEqual([
        { id: 1, nombre: 'Guatemala', codigoIso2: 'GT', edadMinima: 18, edadMaxima: 100 },
      ]);
      expect(listarTamanio).toBe(100);
      expect(cascada.departamentos()).toEqual([{ id: 10, paisId: 1, nombre: 'Depto 1' }]);
      expect(deptoPaisId).toBe(1);
      expect(cascada.municipios()).toEqual([{ id: 10, departamentoId: 1, nombre: 'Muni 1' }]);
      expect(muniDeptoId).toBe(1);
      expect(cascada.seleccion()).toEqual({ paisId: 1, departamentoId: 1, municipioId: 1 });
    });
  });

  it('elegirPais_CambiaPaisVaciaHijosYCargaDepartamentos', () => {
    TestBed.runInInjectionContext(() => {
      const cascada = crearCascada({ paisId: 1, departamentoId: 10, municipioId: 100 });

      cascada.elegirPais(2);

      expect(cascada.seleccion()).toEqual({ paisId: 2, departamentoId: null, municipioId: null });
      expect(deptoPaisId).toBe(2);
      expect(cascada.departamentos()).toEqual([{ id: 20, paisId: 2, nombre: 'Depto 2' }]);
      expect(cascada.municipios()).toEqual([]);
    });
  });

  it('elegirPais_Null_VaciaDepartamentosYNoPide', () => {
    TestBed.runInInjectionContext(() => {
      const cascada = crearCascada({ paisId: 1, departamentoId: 10, municipioId: 100 });
      deptoPaisId = undefined;

      cascada.elegirPais(null);

      expect(cascada.seleccion()).toEqual({
        paisId: null,
        departamentoId: null,
        municipioId: null,
      });
      expect(deptoPaisId).toBeUndefined();
      expect(cascada.departamentos()).toEqual([]);
      expect(cascada.municipios()).toEqual([]);
    });
  });

  it('elegirDepartamento_CambiaDepartamentoVaciaMunicipioYCargaMunicipios', () => {
    TestBed.runInInjectionContext(() => {
      const cascada = crearCascada({ paisId: 1, departamentoId: 1, municipioId: 1 });

      cascada.elegirDepartamento(2);

      expect(cascada.seleccion()).toEqual({ paisId: 1, departamentoId: 2, municipioId: null });
      expect(muniDeptoId).toBe(2);
      expect(cascada.municipios()).toEqual([{ id: 20, departamentoId: 2, nombre: 'Muni 2' }]);
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

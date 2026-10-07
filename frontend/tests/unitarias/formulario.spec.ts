/* eslint-disable @typescript-eslint/no-explicit-any */
import { TestBed } from '@angular/core/testing';
import { FormArray, FormControl, FormGroup, Validators } from '@angular/forms';
import { of, throwError, Subject } from 'rxjs';
import { Router } from '@angular/router';
import { MessageService } from 'primeng/api';
import { crearFormulario } from '../../src/app/servicios/formulario';

describe('Formulario', () => {
  let router: any;
  let messageService: any;

  beforeEach(() => {
    router = { navigateByUrl: vi.fn() };
    messageService = { add: vi.fn() };

    TestBed.configureTestingModule({
      providers: [
        { provide: Router, useValue: router },
        { provide: MessageService, useValue: messageService },
      ],
    });
  });

  it('enviar_Invalido_BloqueaEnvioYMarcaControles', () => {
    const grupo = new FormGroup({ nombre: new FormControl('', Validators.required) });
    let guardado = false;

    TestBed.runInInjectionContext(() => {
      const formulario = crearFormulario(
        grupo,
        () => {
          guardado = true;
          return of({});
        },
        { volverA: '/lista' },
      );
      formulario.enviar();
    });

    expect(guardado).toBe(false);
    expect(grupo.controls['nombre'].touched).toBe(true);
  });

  it('enviar_Exito_AvisaYNavega', () => {
    const grupo = new FormGroup({ nombre: new FormControl('A') });
    let valoresGuardados: any;

    TestBed.runInInjectionContext(() => {
      const formulario = crearFormulario(
        grupo,
        (valores) => {
          valoresGuardados = valores;
          return of({});
        },
        { volverA: '/lista' },
      );
      formulario.enviar();
    });

    expect(valoresGuardados).toEqual({ nombre: 'A' });
    expect(messageService.add).toHaveBeenCalledWith(
      expect.objectContaining({ summary: 'Se guardó correctamente.' }),
    );
    expect(router.navigateByUrl).toHaveBeenCalledWith('/lista');
  });

  it('enviar_Error400_PoneMensajeEnControlYFormArray', () => {
    const grupo = new FormGroup({
      nombre: new FormControl('A'),
      empresas: new FormArray([
        new FormGroup({ fechaIngreso: new FormControl('B') }),
        new FormGroup({ fechaIngreso: new FormControl('C') }),
      ]),
    });

    TestBed.runInInjectionContext(() => {
      const formulario = crearFormulario(
        grupo,
        () =>
          throwError(() => ({
            estado: 400,
            errores: {
              nombre: ['Nombre inválido'],
              'empresas[1].fechaIngreso': ['Fecha inválida'],
            },
          })),
        { volverA: '/lista' },
      );

      formulario.enviar();
    });

    expect(grupo.get('nombre')?.errors).toEqual({ api: 'Nombre inválido' });
    expect(grupo.get('empresas.1.fechaIngreso')?.errors).toEqual({ api: 'Fecha inválida' });
  });

  it('enviar_Error400JsonIlegible_MuestraElValorNoEsValidoYDescartaDto', () => {
    const grupo = new FormGroup({ nombre: new FormControl('A') });

    TestBed.runInInjectionContext(() => {
      const formulario = crearFormulario(
        grupo,
        () =>
          throwError(() => ({
            estado: 400,
            errores: {
              '$.nombre': ['The JSON value...'],
              dto: ['The dto field is required.'],
            },
          })),
        { volverA: '/lista' },
      );

      formulario.enviar();
      expect(grupo.get('nombre')?.errors).toEqual({ api: 'El valor no es válido.' });
      expect(formulario.errorGeneral()).toBeNull();
    });
  });

  it('enviar_ErrorSinControl_VaAErrorGeneral', () => {
    const grupo = new FormGroup({ nombre: new FormControl('A') });

    TestBed.runInInjectionContext(() => {
      const formulario = crearFormulario(
        grupo,
        () =>
          throwError(() => ({
            estado: 400,
            errores: {
              otro: ['Error de otro'],
            },
          })),
        { volverA: '/lista' },
      );

      formulario.enviar();
      expect(formulario.errorGeneral()).toBe('Error de otro');
    });
  });

  it('enviar_Error409_VaAErrorGeneral', () => {
    const grupo = new FormGroup({ nombre: new FormControl('A') });

    TestBed.runInInjectionContext(() => {
      const formulario = crearFormulario(
        grupo,
        () =>
          throwError(() => ({
            estado: 409,
            detalle: 'Conflicto',
            errores: {},
          })),
        { volverA: '/lista' },
      );

      formulario.enviar();
      expect(formulario.errorGeneral()).toBe('Conflicto');
    });
  });

  it('enviar_MientrasEspera_GuardandoEsTrue', () => {
    const grupo = new FormGroup({ nombre: new FormControl('A') });
    const guardar$ = new Subject<any>();

    TestBed.runInInjectionContext(() => {
      const formulario = crearFormulario(grupo, () => guardar$.asObservable(), {
        volverA: '/lista',
      });

      formulario.enviar();
      expect(formulario.guardando()).toBe(true);

      guardar$.next({});
      expect(formulario.guardando()).toBe(false);
    });
  });
});

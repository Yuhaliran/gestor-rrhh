/* eslint-disable @typescript-eslint/no-explicit-any */
import { TestBed } from '@angular/core/testing';
import { of, throwError } from 'rxjs';
import { CONFIRMACION } from '../../src/app/servicios/confirmacion';
import { AVISOS } from '../../src/app/servicios/avisos';
import { crearEliminacion } from '../../src/app/servicios/eliminacion';

describe('Eliminacion', () => {
  let confirmacion: any;
  let avisos: any;
  let recargarSpy: any;
  let eliminarLlamado = false;

  beforeEach(() => {
    confirmacion = { confirmar: vi.fn(() => of(true)) };
    avisos = { exito: vi.fn(), error: vi.fn() };
    recargarSpy = vi.fn();
    eliminarLlamado = false;

    TestBed.configureTestingModule({
      providers: [
        { provide: CONFIRMACION, useValue: confirmacion },
        { provide: AVISOS, useValue: avisos },
      ],
    });
  });

  it('eliminar_SinConfirmar_NoElimina', () => {
    confirmacion.confirmar.mockReturnValue(of(false));

    TestBed.runInInjectionContext(() => {
      const eliminacion = crearEliminacion(() => {
        eliminarLlamado = true;
        return of(undefined);
      }, recargarSpy);

      eliminacion.eliminar(1);

      expect(confirmacion.confirmar).toHaveBeenCalledWith('¿Eliminar este registro?');
      expect(eliminarLlamado).toBe(false);
      expect(recargarSpy).not.toHaveBeenCalled();
    });
  });

  it('eliminar_204_AvisaYRecarga', () => {
    let elementoEliminado: any;
    TestBed.runInInjectionContext(() => {
      const eliminacion = crearEliminacion((elemento) => {
        eliminarLlamado = true;
        elementoEliminado = elemento;
        return of(undefined);
      }, recargarSpy);

      eliminacion.eliminar(1);

      expect(confirmacion.confirmar).toHaveBeenCalledWith('¿Eliminar este registro?');
      expect(eliminarLlamado).toBe(true);
      expect(elementoEliminado).toBe(1);
      expect(avisos.exito).toHaveBeenCalledWith('Se eliminó correctamente.');
      expect(recargarSpy).toHaveBeenCalled();
    });
  });

  it('eliminar_409_AvisaYNoRecarga', () => {
    TestBed.runInInjectionContext(() => {
      const eliminacion = crearEliminacion(
        () => throwError(() => ({ estado: 409, detalle: 'Conflicto de prueba' })),
        recargarSpy,
      );

      eliminacion.eliminar(1);

      expect(confirmacion.confirmar).toHaveBeenCalledWith('¿Eliminar este registro?');
      expect(avisos.error).toHaveBeenCalledWith('Conflicto de prueba');
      expect(recargarSpy).not.toHaveBeenCalled();
    });
  });
});

/* eslint-disable @typescript-eslint/no-explicit-any */
import { TestBed } from '@angular/core/testing';
import { of, throwError } from 'rxjs';
import { ConfirmationService, MessageService } from 'primeng/api';
import { crearEliminacion } from '../../src/app/servicios/eliminacion';

describe('Eliminacion', () => {
  let confirmationService: any;
  let messageService: any;
  let recargarSpy: any;
  let eliminarLlamado = false;

  beforeEach(() => {
    confirmationService = { confirm: vi.fn() };
    messageService = { add: vi.fn() };
    recargarSpy = vi.fn();
    eliminarLlamado = false;

    TestBed.configureTestingModule({
      providers: [
        { provide: ConfirmationService, useValue: confirmationService },
        { provide: MessageService, useValue: messageService },
      ],
    });
  });

  it('eliminar_SinConfirmar_NoElimina', () => {
    TestBed.runInInjectionContext(() => {
      const eliminacion = crearEliminacion(() => {
        eliminarLlamado = true;
        return of(undefined);
      }, recargarSpy);

      eliminacion.eliminar(1);

      expect(confirmationService.confirm).toHaveBeenCalled();
      const options = confirmationService.confirm.mock.calls[0][0];

      if (options.reject) {
        options.reject();
      }

      expect(eliminarLlamado).toBe(false);
      expect(recargarSpy).not.toHaveBeenCalled();
    });
  });

  it('eliminar_204_AvisaYRecarga', () => {
    TestBed.runInInjectionContext(() => {
      const eliminacion = crearEliminacion(() => {
        eliminarLlamado = true;
        return of(undefined);
      }, recargarSpy);

      eliminacion.eliminar(1);

      const options = confirmationService.confirm.mock.calls[0][0];
      options.accept();

      expect(eliminarLlamado).toBe(true);
      expect(messageService.add).toHaveBeenCalledWith(
        expect.objectContaining({ summary: 'Se eliminó correctamente.' }),
      );
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

      const options = confirmationService.confirm.mock.calls[0][0];
      options.accept();

      expect(messageService.add).toHaveBeenCalledWith(
        expect.objectContaining({ summary: 'Conflicto de prueba' }),
      );
      expect(recargarSpy).not.toHaveBeenCalled();
    });
  });
});

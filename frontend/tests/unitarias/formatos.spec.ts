import { FormControl, FormGroup, Validators } from '@angular/forms';
import {
  fechaParaMostrar,
  fechaDesdeIso,
  fechaAIso,
  textoRegla29Febrero,
  edadMinimaNoMayorQueMaxima,
  mensajeDeError,
  mensajeDeValidacion,
  PATRON_TELEFONO,
  PATRON_CODIGO_ISO,
  textoEdad,
} from '../../src/app/servicios/formatos';

describe('Formatos y Validadores', () => {
  it('fechaParaMostrar_ConvierteAFormatoLocal', () => {
    expect(fechaParaMostrar('2026-10-07')).toBe('07/10/2026');
  });

  it('fechaDesdeIso_CreaFechaLocalSinCorrerseUnDia', () => {
    const fecha = fechaDesdeIso('2026-10-07');
    expect(fecha.getFullYear()).toBe(2026);
    expect(fecha.getMonth()).toBe(9);
    expect(fecha.getDate()).toBe(7);
  });

  it('fechaAIso_ConvierteFechaLocalAFormatoIsoSinCorrerseUnDia', () => {
    const fecha = new Date(2026, 9, 7, 23, 0);
    expect(fechaAIso(fecha)).toBe('2026-10-07');
  });

  it('textoRegla29Febrero_MuestraTextoCorrecto', () => {
    expect(textoRegla29Febrero('VeintiochoDeFebrero')).toBe('28 de febrero');
    expect(textoRegla29Febrero('PrimeroDeMarzo')).toBe('1 de marzo');
  });

  it('mensajeDeError_SegunEstado_MuestraTextoCorrecto', () => {
    expect(mensajeDeError({ estado: 0, errores: {} })).toBe('No se pudo conectar con la API.');
    expect(mensajeDeError({ estado: 404, errores: {} })).toBe('El registro no existe.');
    expect(mensajeDeError({ estado: 409, detalle: 'Conflicto', errores: {} })).toBe('Conflicto');
    expect(mensajeDeError({ estado: 500, errores: {} })).toBe('Ocurrió un error inesperado.');
  });

  it('mensajeDeValidacion_SegunError_MuestraTextoCorrecto', () => {
    expect(mensajeDeValidacion({ required: true })).toBe('Este campo es obligatorio.');
    expect(mensajeDeValidacion({ maxlength: { requiredLength: 100 } })).toBe(
      'Admite hasta 100 caracteres.',
    );
    expect(mensajeDeValidacion({ pattern: true })).toBe('El formato no es válido.');
    expect(mensajeDeValidacion({ email: true })).toBe('El formato no es válido.');
    expect(mensajeDeValidacion({ min: true })).toBe('No puede ser negativa.');
    expect(mensajeDeValidacion({ edadMinimaMayorQueMaxima: true })).toBe(
      'La edad mínima no puede ser mayor que la máxima.',
    );
    expect(mensajeDeValidacion({ api: 'Mensaje de la API' })).toBe('Mensaje de la API');
    expect(mensajeDeValidacion(null)).toBeNull();
  });

  it('textoEdad_SegunCantidad_MuestraAnioOAnios', () => {
    expect(textoEdad(34)).toBe('34 años');
    expect(textoEdad(1)).toBe('1 año');
  });

  describe('Validadores Límite (VC)', () => {
    it('edadMinimaNoMayorQueMaxima_Igual_EsValido', () => {
      const grupo = new FormGroup({
        edadMinima: new FormControl(18),
        edadMaxima: new FormControl(18),
      });
      expect(edadMinimaNoMayorQueMaxima(grupo)).toBeNull();
    });

    it('edadMinimaNoMayorQueMaxima_Mayor_EsInvalido', () => {
      const grupo = new FormGroup({
        edadMinima: new FormControl(19),
        edadMaxima: new FormControl(18),
      });
      expect(edadMinimaNoMayorQueMaxima(grupo)).toEqual({ edadMinimaMayorQueMaxima: true });
    });

    it('patronTelefono_ValoresLimite', () => {
      const control = new FormControl('', Validators.pattern(PATRON_TELEFONO));

      control.setValue('123456'); // 6 caracteres
      expect(control.errors?.['pattern']).toBeTruthy();

      control.setValue('1234567'); // 7 caracteres
      expect(control.errors).toBeNull();

      control.setValue('12345678901234567890'); // 20 caracteres
      expect(control.errors).toBeNull();

      control.setValue('123456789012345678901'); // 21 caracteres
      expect(control.errors?.['pattern']).toBeTruthy();
    });

    it('patronCodigoIso_ValoresLimite', () => {
      const control = new FormControl('', Validators.pattern(PATRON_CODIGO_ISO));

      control.setValue('A'); // 1 letra
      expect(control.errors?.['pattern']).toBeTruthy();

      control.setValue('GT'); // 2 letras
      expect(control.errors).toBeNull();

      control.setValue('GTM'); // 3 letras
      expect(control.errors?.['pattern']).toBeTruthy();
    });
  });
});

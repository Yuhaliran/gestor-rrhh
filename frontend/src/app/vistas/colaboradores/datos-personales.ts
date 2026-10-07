import { type NonNullableFormBuilder, Validators } from '@angular/forms';

import { PATRON_TELEFONO } from '../../servicios/formatos';

// RF13 · Los datos personales del colaborador, con las validaciones de VC1 a VC3: los usan el
// alta y la edición.
export const datosPersonales = (fb: NonNullableFormBuilder) => ({
  nombreCompleto: fb.control('', [Validators.required, Validators.maxLength(200)]),
  fechaNacimiento: fb.control('', Validators.required),
  telefono: fb.control('', [Validators.required, Validators.pattern(PATRON_TELEFONO)]),
  correo: fb.control('', [Validators.required, Validators.email, Validators.maxLength(254)]),
});

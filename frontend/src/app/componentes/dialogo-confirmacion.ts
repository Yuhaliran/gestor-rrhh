import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { MatButton } from '@angular/material/button';
import {
  MAT_DIALOG_DATA,
  MatDialogActions,
  MatDialogClose,
  MatDialogContent,
  MatDialogTitle,
} from '@angular/material/dialog';

// Diálogo de ConfirmacionMaterial: el mensaje y los botones del Contrato de interfaz.
@Component({
  selector: 'rrhh-dialogo-confirmacion',
  imports: [MatButton, MatDialogActions, MatDialogClose, MatDialogContent, MatDialogTitle],
  templateUrl: './dialogo-confirmacion.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class DialogoConfirmacion {
  protected readonly datos = inject<{ mensaje: string }>(MAT_DIALOG_DATA);
}

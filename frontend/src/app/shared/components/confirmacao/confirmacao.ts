
import { Component, inject } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogModule } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';

export interface ConfirmacaoData {
  titulo: string;
  mensagem: string;
  textoConfirmar?: string;
  textoCancelar?: string;
}

@Component({
  selector: 'app-confirmacao',
  standalone: true,
  imports: [MatDialogModule, MatButtonModule],
  templateUrl: './confirmacao.html',
  styleUrl: './confirmacao.scss'
})
export class Confirmacao {
  readonly data = inject<ConfirmacaoData>(MAT_DIALOG_DATA);
}

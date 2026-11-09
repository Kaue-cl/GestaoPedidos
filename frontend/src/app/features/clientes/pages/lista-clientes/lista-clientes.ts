
import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import {
  FormBuilder,
  ReactiveFormsModule,
  Validators
} from '@angular/forms';
import { finalize, Observable } from 'rxjs';

import { MatCardModule } from '@angular/material/card';
import { MatTableModule } from '@angular/material/table';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';

import { ClienteService } from '../../services/cliente.service';
import {
  ClienteResponse,
  CriarClienteRequest,
  AtualizarClienteRequest
} from '../../models/cliente.model';

import { Confirmacao } from '../../../../shared/components/confirmacao/confirmacao';

@Component({
  selector: 'app-lista-clientes',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatCardModule,
    MatTableModule,
    MatButtonModule,
    MatIconModule,
    MatFormFieldModule,
    MatInputModule,
    MatProgressSpinnerModule,
    MatSnackBarModule,
    MatDialogModule
  ],
  templateUrl: './lista-clientes.html',
  styleUrl: './lista-clientes.scss'
})
export class ListaClientes implements OnInit {
  private readonly clienteService = inject(ClienteService);
  private readonly fb = inject(FormBuilder);
  private readonly snackBar = inject(MatSnackBar);
  private readonly dialog = inject(MatDialog);

  readonly clientes = signal<ClienteResponse[]>([]);
  readonly carregando = signal(false);
  readonly salvando = signal(false);
  readonly excluindoId = signal<number | null>(null);
  readonly erro = signal(false);

  readonly formularioAberto = signal(false);
  readonly clienteEmEdicao = signal<number | null>(null);

  readonly colunasExibidas = [
    'id',
    'nome',
    'email',
    'telefone',
    'acoes'
  ];

  readonly formulario = this.fb.nonNullable.group({
    nome: ['', [Validators.required, Validators.maxLength(150)]],
    email: ['', [Validators.required, Validators.email]],
    telefone: ['', [Validators.required]]
  });

  ngOnInit(): void {
    this.carregarClientes();
  }

  carregarClientes(): void {
    this.carregando.set(true);
    this.erro.set(false);

    this.clienteService.listar()
      .pipe(finalize(() => this.carregando.set(false)))
      .subscribe({
        next: clientes => this.clientes.set(clientes),
        error: () => {
          this.erro.set(true);
          this.clientes.set([]);
        }
      });
  }

  novoCliente(): void {
    this.clienteEmEdicao.set(null);
    this.formulario.reset();
    this.formularioAberto.set(true);
  }

  editarCliente(cliente: ClienteResponse): void {
    this.clienteEmEdicao.set(cliente.id);

    this.formulario.setValue({
      nome: cliente.nome,
      email: cliente.email,
      telefone: cliente.telefone
    });

    this.formularioAberto.set(true);
  }

  cancelarFormulario(): void {
    this.formularioAberto.set(false);
    this.clienteEmEdicao.set(null);
    this.formulario.reset();
  }

  salvarCliente(): void {
    if (this.formulario.invalid || this.salvando()) {
      this.formulario.markAllAsTouched();
      return;
    }

    const dados = this.formulario.getRawValue();
    const id = this.clienteEmEdicao();

    this.salvando.set(true);

    const requisicao: Observable<ClienteResponse | void> = id === null
      ? this.clienteService.criar(dados as CriarClienteRequest)
      : this.clienteService.atualizar(
          id,
          dados as AtualizarClienteRequest
        );

    requisicao
      .pipe(finalize(() => this.salvando.set(false)))
      .subscribe({
        next: () => {
          this.snackBar.open(
            id === null
              ? 'Cliente cadastrado com sucesso.'
              : 'Cliente atualizado com sucesso.',
            'Fechar',
            { duration: 3000 }
          );

          this.cancelarFormulario();
          this.carregarClientes();
        }
      });
  }

  excluirCliente(cliente: ClienteResponse): void {
    const dialogRef = this.dialog.open(Confirmacao, {
      width: '420px',
      data: {
        titulo: 'Excluir cliente',
        mensagem: `Deseja realmente excluir o cliente "${cliente.nome}"?`,
        textoConfirmar: 'Excluir',
        textoCancelar: 'Cancelar'
      }
    });

    dialogRef.afterClosed().subscribe(confirmado => {
      if (confirmado !== true) {
        return;
      }

      this.excluindoId.set(cliente.id);

      this.clienteService.excluir(cliente.id)
        .pipe(finalize(() => this.excluindoId.set(null)))
        .subscribe({
          next: () => {
            this.snackBar.open(
              'Cliente excluído com sucesso.',
              'Fechar',
              { duration: 3000 }
            );

            this.carregarClientes();
          }
        });
    });
  }
}

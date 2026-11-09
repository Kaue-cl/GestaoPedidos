
import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import {
  FormBuilder,
  ReactiveFormsModule,
  Validators
} from '@angular/forms';
import { Observable, finalize } from 'rxjs';

import { MatCardModule } from '@angular/material/card';
import { MatTableModule } from '@angular/material/table';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';

import { ProdutoService } from '../../services/produto.service';
import {
  ProdutoResponse,
  CriarProdutoRequest,
  AtualizarProdutoRequest
} from '../../models/produto.model';

import { Confirmacao } from '../../../../shared/components/confirmacao/confirmacao';

@Component({
  selector: 'app-lista-produtos',
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
    MatCheckboxModule,
    MatProgressSpinnerModule,
    MatSnackBarModule,
    MatDialogModule
  ],
  templateUrl: './lista-produtos.html',
  styleUrl: './lista-produtos.scss'
})
export class ListaProdutos implements OnInit {
  private readonly produtoService = inject(ProdutoService);
  private readonly fb = inject(FormBuilder);
  private readonly snackBar = inject(MatSnackBar);
  private readonly dialog = inject(MatDialog);

  readonly produtos = signal<ProdutoResponse[]>([]);
  readonly carregando = signal(false);
  readonly salvando = signal(false);
  readonly excluindoId = signal<number | null>(null);
  readonly erro = signal(false);

  readonly formularioAberto = signal(false);
  readonly produtoEmEdicao = signal<number | null>(null);

  readonly colunasExibidas = [
    'id',
    'nome',
    'preco',
    'estoque',
    'situacao',
    'acoes'
  ];

  readonly formulario = this.fb.nonNullable.group({
    nome: ['', [Validators.required, Validators.maxLength(150)]],
    preco: [0, [Validators.required, Validators.min(0.01)]],
    estoqueAtual: [
      0,
      [
        Validators.required,
        Validators.min(0),
        Validators.pattern(/^\d+$/)
      ]
    ],
    ativo: [true]
  });

  ngOnInit(): void {
    this.carregarProdutos();
  }

  carregarProdutos(): void {
    this.carregando.set(true);
    this.erro.set(false);

    this.produtoService.listar()
      .pipe(finalize(() => this.carregando.set(false)))
      .subscribe({
        next: produtos => {
          console.log('Produtos recebidos:', produtos);
          this.produtos.set(produtos);
        },
        error: () => {
          this.erro.set(true);
          this.produtos.set([]);
        }
      });
  }

  novoProduto(): void {
    this.produtoEmEdicao.set(null);

    this.formulario.reset({
      nome: '',
      preco: 0,
      estoqueAtual: 0,
      ativo: true
    });

    this.formularioAberto.set(true);
  }

  editarProduto(produto: ProdutoResponse): void {
    this.produtoEmEdicao.set(produto.id);

    this.formulario.setValue({
      nome: produto.nome,
      preco: produto.preco,
      estoqueAtual: produto.estoqueAtual,
      ativo: produto.ativo
    });

    this.formularioAberto.set(true);
  }

  cancelarFormulario(): void {
    this.formularioAberto.set(false);
    this.produtoEmEdicao.set(null);
    this.formulario.reset();
  }

  salvarProduto(): void {
    if (this.formulario.invalid || this.salvando()) {
      this.formulario.markAllAsTouched();
      return;
    }

    const dados = this.formulario.getRawValue();

    const request = {
      nome: dados.nome.trim(),
      preco: dados.preco,
      estoqueAtual: dados.estoqueAtual,
      ativo: dados.ativo
    };

    if (!request.nome) {
      this.formulario.controls.nome.setErrors({
        required: true
      });
      return;
    }

    const id = this.produtoEmEdicao();

    this.salvando.set(true);

    const requisicao: Observable<ProdutoResponse | void> =
      id === null
        ? this.produtoService.criar(
          request as CriarProdutoRequest
        )
        : this.produtoService.atualizar(
          id,
          request as AtualizarProdutoRequest
        );

    requisicao
      .pipe(finalize(() => this.salvando.set(false)))
      .subscribe({
        next: () => {
          this.snackBar.open(
            id === null
              ? 'Produto cadastrado com sucesso.'
              : 'Produto atualizado com sucesso.',
            'Fechar',
            { duration: 3000 }
          );

          this.cancelarFormulario();
          this.carregarProdutos();
        }
      });
  }

  excluirProduto(produto: ProdutoResponse): void {
    const dialogRef = this.dialog.open(Confirmacao, {
      width: '420px',
      data: {
        titulo: 'Excluir produto',
        mensagem:
          `Deseja realmente excluir o produto "${produto.nome}"?`,
        textoConfirmar: 'Excluir',
        textoCancelar: 'Cancelar'
      }
    });

    dialogRef.afterClosed().subscribe(confirmado => {
      if (confirmado !== true) {
        return;
      }

      this.excluindoId.set(produto.id);

      this.produtoService.excluir(produto.id)
        .pipe(finalize(() => this.excluindoId.set(null)))
        .subscribe({
          next: () => {
            this.snackBar.open(
              'Produto excluído com sucesso.',
              'Fechar',
              { duration: 3000 }
            );

            this.carregarProdutos();
          }
        });
    });
  }
}

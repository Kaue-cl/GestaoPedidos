
import { Component, OnInit, inject, signal, computed } from '@angular/core';
import { CurrencyPipe } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { forkJoin, finalize, switchMap, tap } from 'rxjs';

import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatSelectModule } from '@angular/material/select';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBar } from '@angular/material/snack-bar';

import { ClienteService } from '../../../clientes/services/cliente.service';
import { ProdutoService } from '../../../produtos/services/produto.service';
import { PedidoService } from '../../services/pedido.service';

import { ClienteResponse } from '../../../clientes/models/cliente.model';
import { ProdutoResponse } from '../../../produtos/models/produto.model';
import { CriarPedidoRequest } from '../../models/pedido.model';

interface ItemSelecionado {
  produto: ProdutoResponse;
  quantidade: number;
  subtotal: number;
}

@Component({
  selector: 'app-novo-pedido',
  standalone: true,
  imports: [
    CurrencyPipe,
    ReactiveFormsModule,
    RouterLink,
    MatCardModule,
    MatFormFieldModule,
    MatSelectModule,
    MatInputModule,
    MatButtonModule,
    MatIconModule,
    MatProgressSpinnerModule
  ],
  templateUrl: './novo-pedido.html',
  styleUrl: './novo-pedido.scss'
})
export class NovoPedido implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly clienteService = inject(ClienteService);
  private readonly produtoService = inject(ProdutoService);
  private readonly pedidoService = inject(PedidoService);
  private readonly router = inject(Router);
  private readonly snackBar = inject(MatSnackBar);

  readonly clientes = signal<ClienteResponse[]>([]);
  readonly produtos = signal<ProdutoResponse[]>([]);
  readonly itens = signal<ItemSelecionado[]>([]);

  readonly carregando = signal(false);
  readonly salvando = signal(false);
  readonly erroCarregamento = signal(false);
  readonly atualizandoEstoque = signal(false);

  readonly formulario = this.fb.nonNullable.group({
    clienteId: [0, [Validators.required, Validators.min(1)]],
    produtoId: [0, [Validators.required, Validators.min(1)]],
    quantidade: [1, [Validators.required, Validators.min(1)]]
  });

  readonly totalPedido = computed(() =>
    this.itens().reduce((total, item) => total + item.subtotal, 0)
  );

  readonly produtosDisponiveis = computed(() =>
    this.produtos().filter(
      produto =>
        produto.ativo &&
        produto.estoqueAtual > 0 &&
        !this.itens().some(item => item.produto.id === produto.id)
    )
  );

  ngOnInit(): void {
    this.carregarDados();
  }

  carregarDados(): void {
    this.carregando.set(true);
    this.erroCarregamento.set(false);

    forkJoin({
      clientes: this.clienteService.listar(),
      produtos: this.produtoService.listar()
    })
      .pipe(finalize(() => this.carregando.set(false)))
      .subscribe({
        next: ({ clientes, produtos }) => {
          this.clientes.set(clientes);
          this.produtos.set(produtos);
        },
        error: () => {
          this.erroCarregamento.set(true);
        }
      });
  }

  atualizarEstoque(): void {
  if (this.atualizandoEstoque() || this.salvando()) {
    return;
  }

  this.atualizandoEstoque.set(true);

  this.produtoService.listar()
    .pipe(finalize(() => this.atualizandoEstoque.set(false)))
    .subscribe({
      next: produtos => {
        this.produtos.set(produtos);
      },
      error: () => {
        // O interceptor HTTP já apresenta o erro.
      }
    });
}

  adicionarItem(): void {
    const produtoId = this.formulario.controls.produtoId.value;
    const quantidade = this.formulario.controls.quantidade.value;

    if (!Number.isInteger(quantidade) || quantidade < 1) {
      this.snackBar.open('Informe uma quantidade válida.', 'Fechar', {
        duration: 3000
      });
      return;
    }

    const produto = this.produtosDisponiveis().find(
      p => p.id === produtoId
    );

    if (!produto) {
      this.snackBar.open('Selecione um produto disponível.', 'Fechar', {
        duration: 3000
      });
      return;
    }

    if (quantidade > produto.estoqueAtual) {
      this.snackBar.open(
        `Estoque insuficiente. Disponível: ${produto.estoqueAtual}.`,
        'Fechar',
        { duration: 4000 }
      );
      return;
    }

    this.itens.update(itens => [
      ...itens,
      {
        produto,
        quantidade,
        subtotal: produto.preco * quantidade
      }
    ]);

    this.formulario.patchValue({
      produtoId: 0,
      quantidade: 1
    });

    this.formulario.controls.produtoId.markAsPristine();
    this.formulario.controls.produtoId.markAsUntouched();
  }

  removerItem(produtoId: number): void {
    this.itens.update(itens =>
      itens.filter(item => item.produto.id !== produtoId)
    );
  }

  criarPedido(): void {
    if (this.salvando()) {
      return;
    }

    const clienteId = this.formulario.controls.clienteId.value;

    if (!this.clientes().some(cliente => cliente.id === clienteId)) {
      this.snackBar.open('Selecione um cliente válido.', 'Fechar', {
        duration: 3000
      });
      return;
    }

    if (this.itens().length === 0) {
      this.snackBar.open('Adicione pelo menos um produto.', 'Fechar', {
        duration: 3000
      });
      return;
    }

    this.salvando.set(true);

    this.produtoService.listar()
      .pipe(
        tap(produtos => this.produtos.set(produtos)),
        switchMap(produtos => {
          const itensSelecionados = this.itens();

          for (const item of itensSelecionados) {
            const produtoAtual = produtos.find(
              p => p.id === item.produto.id
            );

            if (!produtoAtual ||
                !produtoAtual.ativo ||
                produtoAtual.estoqueAtual < item.quantidade) {
              this.snackBar.open(
                `Estoque indisponível para "${item.produto.nome}". Atualize os itens do pedido.`,
                'Fechar',
                { duration: 5000 }
              );

              throw new Error('Estoque desatualizado');
            }
          }

          const pedido: CriarPedidoRequest = {
            clienteId,
            itens: itensSelecionados.map(item => ({
              produtoId: item.produto.id,
              quantidade: item.quantidade
            }))
          };

          return this.pedidoService.criar(pedido);
        }),
        finalize(() => this.salvando.set(false))
      )
      .subscribe({
        next: () => {
          this.snackBar.open(
            'Pedido criado com sucesso!',
            'Fechar',
            { duration: 3000 }
          );

          this.router.navigate(['/pedidos']);
        },
        error: erro => {
          if (erro.message === 'Estoque desatualizado') {
            return;
          }

          // O interceptor HTTP apresenta os erros da API.
          // Atualiza o estoque caso outra requisição tenha consumido unidades.
          this.atualizarEstoque();
        }
      });
  }
}


import { Component, OnInit, inject, signal } from '@angular/core';
import { DatePipe, CurrencyPipe } from '@angular/common';
import { finalize } from 'rxjs';
import { RouterLink } from '@angular/router';

import { MatTableModule } from '@angular/material/table';
import { MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatMenuModule } from '@angular/material/menu';
import { MatDialog } from '@angular/material/dialog';
import { MatSnackBar } from '@angular/material/snack-bar';

import { PedidoService } from '../../services/pedido.service';
import { PedidoResponse } from '../../models/pedido.model';
import { StatusPedido } from '../../../../core/models/status-pedido.enum';

import { FormsModule } from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatSelectModule } from '@angular/material/select';
import { MatInputModule } from '@angular/material/input';

import { FiltroPedidos } from '../../models/pedido.model';

import {
  Confirmacao,
  ConfirmacaoData
} from '../../../../shared/components/confirmacao/confirmacao';

@Component({
  selector: 'app-lista-pedidos',
  standalone: true,
  imports: [
    DatePipe,
    CurrencyPipe,
    RouterLink,
    MatTableModule,
    MatPaginatorModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatProgressSpinnerModule,
    MatMenuModule,
    FormsModule,
    MatFormFieldModule,
    MatSelectModule,
    MatInputModule,
  ],
  templateUrl: './lista-pedidos.html',
  styleUrl: './lista-pedidos.scss'
})
export class ListaPedidos implements OnInit {
  private readonly pedidoService = inject(PedidoService);
  private readonly dialog = inject(MatDialog);
  private readonly snackBar = inject(MatSnackBar);

  readonly pedidos = signal<PedidoResponse[]>([]);
  readonly carregando = signal(false);
  readonly erro = signal(false);
  readonly processandoPedidoId = signal<number | null>(null);

  readonly totalRegistros = signal(0);
  readonly paginaAtual = signal(0);
  readonly tamanhoPagina = signal(10);

  readonly colunasExibidas = [
    'id',
    'cliente',
    'dataPedido',
    'status',
    'valorTotal',
    'acoes'
  ];

  readonly StatusPedido = StatusPedido;
    
  readonly filtroStatus = signal<StatusPedido | null>(null);
  readonly filtroCliente = signal('');
  readonly ordenarPor = signal<'data' | 'valor'>('data');
  readonly direcao = signal<'asc' | 'desc'>('desc');
  
  readonly opcoesStatus = [
    { valor: StatusPedido.Pendente, nome: 'Pendente' },
    { valor: StatusPedido.Pago, nome: 'Pago' },
    { valor: StatusPedido.Enviado, nome: 'Enviado' },
    { valor: StatusPedido.Entregue, nome: 'Entregue' },
    { valor: StatusPedido.Cancelado, nome: 'Cancelado' }
  ];
  
  aplicarFiltros(): void {
    this.paginaAtual.set(0);
    this.carregarPedidos();
  }
  
  limparFiltros(): void {
    this.filtroStatus.set(null);
    this.filtroCliente.set('');
    this.ordenarPor.set('data');
    this.direcao.set('desc');
  
    this.aplicarFiltros();
  }


  ngOnInit(): void {
    this.carregarPedidos();
  }

  
  carregarPedidos(): void {
    this.carregando.set(true);
    this.erro.set(false);
  
    const filtros: Partial<FiltroPedidos> = {
      cliente: this.filtroCliente(),
      ordenarPor: this.ordenarPor(),
      direcao: this.direcao()
    };
  
    if (this.filtroStatus() !== null) {
      filtros.status = this.filtroStatus()!;
    }
  
    this.pedidoService
      .listar(
        this.paginaAtual() + 1,
        this.tamanhoPagina(),
        filtros
      )
      .pipe(finalize(() => this.carregando.set(false)))
      .subscribe({
        next: response => {
          this.pedidos.set(response.dados);
          this.totalRegistros.set(response.totalRegistros);
        },
        error: () => {
          this.erro.set(true);
          this.pedidos.set([]);
        }
      });
  }


  alterarPagina(event: PageEvent): void {
    this.paginaAtual.set(event.pageIndex);
    this.tamanhoPagina.set(event.pageSize);
    this.carregarPedidos();
  }

  obterProximoStatus(status: StatusPedido): StatusPedido | null {
    switch (status) {
      case StatusPedido.Pendente:
        return StatusPedido.Pago;
      case StatusPedido.Pago:
        return StatusPedido.Enviado;
      case StatusPedido.Enviado:
        return StatusPedido.Entregue;
      default:
        return null;
    }
  }

  obterNomeStatus(status: StatusPedido): string {
    switch (status) {
      case StatusPedido.Pendente:
        return 'Pendente';
      case StatusPedido.Pago:
        return 'Pago';
      case StatusPedido.Enviado:
        return 'Enviado';
      case StatusPedido.Entregue:
        return 'Entregue';
      case StatusPedido.Cancelado:
        return 'Cancelado';
      default:
        return 'Desconhecido';
    }
  }

  obterClasseStatus(status: StatusPedido): string {
    switch (status) {
      case StatusPedido.Pendente:
        return 'status-pendente';
      case StatusPedido.Pago:
        return 'status-pago';
      case StatusPedido.Enviado:
        return 'status-enviado';
      case StatusPedido.Entregue:
        return 'status-entregue';
      case StatusPedido.Cancelado:
        return 'status-cancelado';
      default:
        return '';
    }
  }

  atualizarStatus(pedido: PedidoResponse): void {
    if (this.processandoPedidoId() !== null) {
      return;
    }

    const proximoStatus = this.obterProximoStatus(pedido.status);

    if (proximoStatus === null) {
      return;
    }

    this.processandoPedidoId.set(pedido.id);

    this.pedidoService.atualizarStatus(pedido.id, {
      status: proximoStatus
    })
      .pipe(finalize(() => this.processandoPedidoId.set(null)))
      .subscribe({
        next: () => {
          this.snackBar.open(
            `Pedido #${pedido.id} atualizado para ${this.obterNomeStatus(proximoStatus)}.`,
            'Fechar',
            { duration: 3000 }
          );

          this.carregarPedidos();
        },
        error: () => {
          // O interceptor HTTP apresenta o erro da API.
        }
      });
  }

  cancelarPedido(pedido: PedidoResponse): void {
    if (
      pedido.status !== StatusPedido.Pendente ||
      this.processandoPedidoId() !== null
    ) {
      return;
    }

    const dados: ConfirmacaoData = {
      titulo: 'Cancelar pedido',
      mensagem:
        `Deseja realmente cancelar o pedido #${pedido.id}? ` +
        'O estoque dos produtos será restaurado.',
      textoConfirmar: 'Cancelar pedido',
      textoCancelar: 'Voltar'
    };

    const dialogRef = this.dialog.open(Confirmacao, {
      width: '420px',
      maxWidth: '95vw',
      data: dados
    });

    dialogRef.afterClosed().subscribe(confirmado => {
      if (confirmado !== true || this.processandoPedidoId() !== null) {
        return;
      }

      this.processandoPedidoId.set(pedido.id);

      this.pedidoService.cancelar(pedido.id)
        .pipe(finalize(() => this.processandoPedidoId.set(null)))
        .subscribe({
          next: () => {
            this.snackBar.open(
              `Pedido #${pedido.id} cancelado com sucesso.`,
              'Fechar',
              { duration: 3000 }
            );

            this.carregarPedidos();
          },
          error: () => {
            // O interceptor HTTP apresenta o erro da API.
          }
        });
    });
  }
}

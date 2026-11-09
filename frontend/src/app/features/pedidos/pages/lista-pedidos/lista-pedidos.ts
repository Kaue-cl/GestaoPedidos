
import { Component, OnInit, inject, signal } from '@angular/core';
import { DatePipe, CurrencyPipe } from '@angular/common';
import { finalize } from 'rxjs';

import { MatTableModule } from '@angular/material/table';
import { MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';

import { PedidoService } from '../../services/pedido.service';
import { PedidoResponse } from '../../models/pedido.model';
import { StatusPedido } from '../../../../core/models/status-pedido.enum';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'app-lista-pedidos',
  standalone: true,
  imports: [
    DatePipe,
    CurrencyPipe,
    MatTableModule,
    MatPaginatorModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatProgressSpinnerModule,
    RouterLink
  ],
  templateUrl: './lista-pedidos.html',
  styleUrl: './lista-pedidos.scss'
})
export class ListaPedidos implements OnInit {
  private readonly pedidoService = inject(PedidoService);

  readonly pedidos = signal<PedidoResponse[]>([]);
  readonly carregando = signal(false);
  readonly erro = signal(false);

  readonly totalRegistros = signal(0);
  readonly paginaAtual = signal(0);
  readonly tamanhoPagina = signal(10);

  readonly colunasExibidas = [
    'id',
    'cliente',
    'dataPedido',
    'status',
    'valorTotal'
  ];

  ngOnInit(): void {
    this.carregarPedidos();
  }

  carregarPedidos(): void {
    this.carregando.set(true);
    this.erro.set(false);

    const pagina = this.paginaAtual() + 1;

    this.pedidoService
      .listar(pagina, this.tamanhoPagina())
      .pipe(
        finalize(() => this.carregando.set(false))
      )
      .subscribe({
        next: (response) => {
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
}

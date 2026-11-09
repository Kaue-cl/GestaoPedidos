
import {
  Component,
  OnInit,
  OnDestroy,
  inject,
  signal
} from '@angular/core';
import { CurrencyPipe, DecimalPipe, DatePipe } from '@angular/common';
import {
  EMPTY,
  Subject,
  catchError,
  exhaustMap,
  finalize,
  interval,
  takeUntil
} from 'rxjs';

import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatButtonModule } from '@angular/material/button';

import { DashboardService } from '../../services/dashboard.service';
import { DashboardResponse } from '../../models/dashboard.model';
import { StatusPedido } from '../../../../core/models/status-pedido.enum';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [
    CurrencyPipe,
    DecimalPipe,
    MatCardModule,
    MatIconModule,
    MatProgressSpinnerModule,
    MatButtonModule,
    DatePipe
  ],
  templateUrl: './dashboard.html',
  styleUrl: './dashboard.scss'
})
export class Dashboard implements OnInit, OnDestroy {
  private readonly dashboardService = inject(DashboardService);
  private readonly destruir$ = new Subject<void>();
  private readonly atualizar$ = new Subject<void>();

  readonly dados = signal<DashboardResponse | null>(null);
  readonly carregando = signal(false);
  readonly erro = signal(false);
  readonly ultimaAtualizacao = signal<Date | null>(null);

  readonly intervaloAtualizacaoSegundos = 30;

  ngOnInit(): void {
    interval(this.intervaloAtualizacaoSegundos * 1000)
      .pipe(
        takeUntil(this.destruir$)
      )
      .subscribe(() => this.atualizar$.next());

    this.atualizar$
      .pipe(
        exhaustMap(() => {
          this.carregando.set(this.dados() === null);
          this.erro.set(false);

          return this.dashboardService.obterResumo().pipe(
            catchError(() => {
              if (this.dados() === null) {
                this.erro.set(true);
              }

              return EMPTY;
            }),
            finalize(() => this.carregando.set(false))
          );
        }),
        takeUntil(this.destruir$)
      )
      .subscribe(response => {
        this.dados.set(response);
        this.ultimaAtualizacao.set(new Date());
      });

    this.carregarDashboard();
  }

  ngOnDestroy(): void {
    this.destruir$.next();
    this.destruir$.complete();
    this.atualizar$.complete();
  }

  carregarDashboard(): void {
    this.atualizar$.next();
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

  calcularPercentual(quantidade: number): number {
    const total = this.dados()?.totalPedidos ?? 0;

    if (total === 0) {
      return 0;
    }

    return (quantidade / total) * 100;
  }

  calcularPercentualProduto(quantidade: number): number {
    const produtos = this.dados()?.produtosMaisVendidos ?? [];
    const maiorQuantidade = Math.max(
      0,
      ...produtos.map(produto => produto.quantidadeVendida)
    );

    if (maiorQuantidade === 0) {
      return 0;
    }

    return (quantidade / maiorQuantidade) * 100;
  }
}

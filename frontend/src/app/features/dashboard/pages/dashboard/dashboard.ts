
import { Component, OnInit, inject, signal } from '@angular/core';
import { CurrencyPipe, DecimalPipe } from '@angular/common';
import { finalize } from 'rxjs';

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
    MatButtonModule
  ],
  templateUrl: './dashboard.html',
  styleUrl: './dashboard.scss'
})
export class Dashboard implements OnInit {
  private readonly dashboardService = inject(DashboardService);

  readonly dados = signal<DashboardResponse | null>(null);
  readonly carregando = signal(false);
  readonly erro = signal(false);

  ngOnInit(): void {
    this.carregarDashboard();
  }

  carregarDashboard(): void {
    this.carregando.set(true);
    this.erro.set(false);

    this.dashboardService.obterResumo()
      .pipe(finalize(() => this.carregando.set(false)))
      .subscribe({
        next: (response) => {
          this.dados.set(response);
        },
        error: () => {
          this.dados.set(null);
          this.erro.set(true);
        }
      });
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
}

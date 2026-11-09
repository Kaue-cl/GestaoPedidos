
import { TestBed, ComponentFixture } from '@angular/core/testing';
import { of, throwError } from 'rxjs';
import { vi } from 'vitest';

import { Dashboard } from './dashboard';
import { DashboardService } from '../../services/dashboard.service';
import { DashboardResponse } from '../../models/dashboard.model';
import { StatusPedido } from '../../../../core/models/status-pedido.enum';

describe('Dashboard', () => {
  let fixture: ComponentFixture<Dashboard>;
  let component: Dashboard;

  const dashboardMock: DashboardResponse = {
    totalPedidos: 10,
    faturamentoTotal: 1500,
    ticketMedio: 150,
    pedidosPorStatus: [
      {
        status: StatusPedido.Pendente,
        quantidade: 4
      },
      {
        status: StatusPedido.Pago,
        quantidade: 6
      }
    ],
    produtosMaisVendidos: [
      {
        produtoId: 1,
        nome: 'Mouse Gamer',
        quantidadeVendida: 8,
        valorTotalVendido: 800
      },
      {
        produtoId: 2,
        nome: 'Teclado Mecânico',
        quantidadeVendida: 4,
        valorTotalVendido: 700
      }
    ]
  };

  const obterResumoMock = vi.fn();

  beforeEach(async () => {
    obterResumoMock.mockReset();
    obterResumoMock.mockReturnValue(of(dashboardMock));

    await TestBed.configureTestingModule({
      imports: [Dashboard],
      providers: [
        {
          provide: DashboardService,
          useValue: {
            obterResumo: obterResumoMock
          }
        }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(Dashboard);
    component = fixture.componentInstance;
  });

  afterEach(() => {
    fixture.destroy();
    vi.useRealTimers();
  });

  it('deve criar o componente', () => {
    expect(component).toBeTruthy();
  });

  it('deve carregar os indicadores ao inicializar', () => {
    fixture.detectChanges();

    expect(obterResumoMock).toHaveBeenCalledTimes(1);
    expect(component.dados()).toEqual(dashboardMock);
    expect(component.carregando()).toBe(false);
    expect(component.erro()).toBe(false);
    expect(component.ultimaAtualizacao()).not.toBeNull();
  });

  it('deve exibir os indicadores e os produtos mais vendidos', () => {
    fixture.detectChanges();

    const elemento = fixture.nativeElement as HTMLElement;
    const texto = elemento.textContent ?? '';

    expect(texto).toContain('Total de pedidos');
    expect(texto).toContain('10');
    expect(texto).toContain('Faturamento total');
    expect(texto).toContain('R$');
    expect(texto).toContain('Mouse Gamer');
    expect(texto).toContain('Teclado Mecânico');
  });

  it('deve tratar erros no carregamento inicial', () => {
    obterResumoMock.mockReturnValue(
      throwError(() => new Error('API indisponível'))
    );

    fixture.detectChanges();

    expect(component.dados()).toBeNull();
    expect(component.erro()).toBe(true);
    expect(component.carregando()).toBe(false);

    const elemento = fixture.nativeElement as HTMLElement;

    expect(elemento.textContent).toContain(
      'Não foi possível carregar o Dashboard'
    );
  });

  it('deve calcular corretamente os percentuais por status', () => {
    component.dados.set(dashboardMock);

    expect(component.calcularPercentual(4)).toBe(40);
    expect(component.calcularPercentual(6)).toBe(60);
  });

  it('deve retornar zero quando não houver pedidos', () => {
    component.dados.set({
      ...dashboardMock,
      totalPedidos: 0
    });

    expect(component.calcularPercentual(4)).toBe(0);
  });

  it('deve calcular percentuais dos produtos mais vendidos', () => {
    component.dados.set(dashboardMock);

    expect(component.calcularPercentualProduto(8)).toBe(100);
    expect(component.calcularPercentualProduto(4)).toBe(50);
  });

  it('deve atualizar os dados ao solicitar atualização manual', () => {
    fixture.detectChanges();

    component.carregarDashboard();

    expect(obterResumoMock).toHaveBeenCalledTimes(2);
  });

  it('deve atualizar automaticamente após 30 segundos', async () => {
    vi.useFakeTimers();

    fixture.detectChanges();

    expect(obterResumoMock).toHaveBeenCalledTimes(1);

    await vi.advanceTimersByTimeAsync(30_000);

    expect(obterResumoMock).toHaveBeenCalledTimes(2);
  });
});

import { StatusPedido } from '../../../core/models/status-pedido.enum';

export interface PedidosPorStatusResponse {
  status: StatusPedido;
  quantidade: number;
}

export interface ProdutoMaisVendidoResponse {
  produtoId: number;
  nome: string;
  quantidadeVendida: number;
  valorTotalVendido: number;
}

export interface DashboardResponse {
  totalPedidos: number;
  faturamentoTotal: number;
  ticketMedio: number;
  pedidosPorStatus: PedidosPorStatusResponse[];
  produtosMaisVendidos: ProdutoMaisVendidoResponse[];
}
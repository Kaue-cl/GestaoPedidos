import { StatusPedido } from '../../../core/models/status-pedido.enum';

export interface CriarItemPedidoRequest {
  produtoId: number;
  quantidade: number;
}

export interface CriarPedidoRequest {
  clienteId: number;
  itens: CriarItemPedidoRequest[];
}

export interface AtualizarStatusPedidoRequest {
  status: StatusPedido;
}

export interface ItemPedidoResponse {
  id: number;
  produtoId: number;
  quantidade: number;
  precoUnitario: number;
  subtotal: number;
}

export interface PedidoResponse {
  id: number;
  clienteId: number;
  dataPedido: string;
  status: StatusPedido;
  valorTotal: number;
  itens: ItemPedidoResponse[];
}

export interface PaginatedResponse<T> {
  pagina: number;
  tamanhoPagina: number;
  totalRegistros: number;
  totalPaginas: number;
  dados: T[];
}

export interface FiltroPedidos {
  pagina: number;
  tamanhoPagina: number;
  status?: StatusPedido;
  cliente?: string;
  ordenarPor: 'data' | 'valor';
  direcao: 'asc' | 'desc';
}

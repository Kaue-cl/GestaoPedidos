export interface CriarProdutoRequest {
  nome: string;
  preco: number;
  estoqueAtual: number;
  ativo: boolean;
}

export interface AtualizarProdutoRequest {
  nome: string;
  preco: number;
  estoqueAtual: number;
  ativo: boolean;
}

export interface ProdutoResponse {
  id: number;
  nome: string;
  preco: number;
  estoqueAtual: number;
  ativo: boolean;
}
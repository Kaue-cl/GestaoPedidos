export interface CriarClienteRequest {
  nome: string;
  email: string;
  telefone: string;
}

export interface AtualizarClienteRequest {
  nome: string;
  email: string;
  telefone: string;
}

export interface ClienteResponse {
  id: number;
  nome: string;
  email: string;
  telefone: string;
  dataCriacao: string;
}
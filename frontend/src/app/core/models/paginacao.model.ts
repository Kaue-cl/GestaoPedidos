
export interface PaginacaoResponse<T> {
  pagina: number;
  tamanhoPagina: number;
  totalRegistros: number;
  totalPaginas: number;
  dados: T[];
}

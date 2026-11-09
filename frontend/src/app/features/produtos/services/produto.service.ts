import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

import { HttpParams } from '@angular/common/http';
import { PaginacaoResponse } from '../../../core/models/paginacao.model';

import { environment } from '../../../../environments/environment';

import {
  CriarProdutoRequest,
  AtualizarProdutoRequest,
  ProdutoResponse
} from '../models/produto.model';


export interface FiltroProdutos {
  pagina?: number;
  tamanhoPagina?: number;
  nome?: string;
  ativo?: boolean | null;
  ordenarPor?: 'nome' | 'preco' | 'estoque';
  direcao?: 'asc' | 'desc';
}


@Injectable({
  providedIn: 'root'
})
export class ProdutoService {
  private readonly http = inject(HttpClient);

  private readonly apiUrl = `${environment.apiUrl}/produtos`;

  listar(): Observable<ProdutoResponse[]> {
    return this.http.get<ProdutoResponse[]>(this.apiUrl);
  }

  buscarPorId(id: number): Observable<ProdutoResponse> {
    return this.http.get<ProdutoResponse>(`${this.apiUrl}/${id}`);
  }

  criar(request: CriarProdutoRequest): Observable<ProdutoResponse> {
    return this.http.post<ProdutoResponse>(this.apiUrl, request);
  }

  atualizar(
    id: number,
    request: AtualizarProdutoRequest
  ): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/${id}`, request);
  }

  excluir(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }

  listarPaginado(
    filtro: FiltroProdutos
  ): Observable<PaginacaoResponse<ProdutoResponse>> {
    let params = new HttpParams()
      .set('pagina', filtro.pagina ?? 1)
      .set('tamanhoPagina', filtro.tamanhoPagina ?? 10)
      .set('ordenarPor', filtro.ordenarPor ?? 'nome')
      .set('direcao', filtro.direcao ?? 'asc');

    if (filtro.nome?.trim()) {
      params = params.set('nome', filtro.nome.trim());
    }

    if (filtro.ativo !== null && filtro.ativo !== undefined) {
      params = params.set('ativo', filtro.ativo);
    }

    return this.http.get<PaginacaoResponse<ProdutoResponse>>(
      `${this.apiUrl}/paginado`,
      { params }
    );
  }
}
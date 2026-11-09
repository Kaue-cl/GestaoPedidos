import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

import { environment } from '../../../../environments/environment';

import {
  CriarProdutoRequest,
  AtualizarProdutoRequest,
  ProdutoResponse
} from '../models/produto.model';

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
}
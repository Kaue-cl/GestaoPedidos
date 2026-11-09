import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

import { HttpParams } from '@angular/common/http';
import { PaginacaoResponse } from '../../../core/models/paginacao.model';

import { environment } from '../../../../environments/environment';

import {
  CriarClienteRequest,
  AtualizarClienteRequest,
  ClienteResponse
} from '../models/cliente.model';


export interface FiltroClientes {
  pagina?: number;
  tamanhoPagina?: number;
  nome?: string;
  email?: string;
  ordenarPor?: 'nome' | 'data';
  direcao?: 'asc' | 'desc';
}


@Injectable({
  providedIn: 'root'
})
export class ClienteService {
  private readonly http = inject(HttpClient);

  private readonly apiUrl = `${environment.apiUrl}/clientes`;

  listar(): Observable<ClienteResponse[]> {
    return this.http.get<ClienteResponse[]>(this.apiUrl);
  }

  buscarPorId(id: number): Observable<ClienteResponse> {
    return this.http.get<ClienteResponse>(`${this.apiUrl}/${id}`);
  }

  criar(request: CriarClienteRequest): Observable<ClienteResponse> {
    return this.http.post<ClienteResponse>(this.apiUrl, request);
  }

  atualizar(
    id: number,
    request: AtualizarClienteRequest
  ): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/${id}`, request);
  }

  excluir(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }

  listarPaginado(
    filtro: FiltroClientes
  ): Observable<PaginacaoResponse<ClienteResponse>> {
    let params = new HttpParams()
      .set('pagina', filtro.pagina ?? 1)
      .set('tamanhoPagina', filtro.tamanhoPagina ?? 10)
      .set('ordenarPor', filtro.ordenarPor ?? 'nome')
      .set('direcao', filtro.direcao ?? 'asc');

    if (filtro.nome?.trim()) {
      params = params.set('nome', filtro.nome.trim());
    }

    if (filtro.email?.trim()) {
      params = params.set('email', filtro.email.trim());
    }

    return this.http.get<PaginacaoResponse<ClienteResponse>>(
      `${this.apiUrl}/paginado`,
      { params }
    );
  }
}
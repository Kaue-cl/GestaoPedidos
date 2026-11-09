import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';

import { environment } from '../../../../environments/environment';

import {
  CriarPedidoRequest,
  AtualizarStatusPedidoRequest,
  PedidoResponse,
  PaginatedResponse
} from '../models/pedido.model';

@Injectable({
  providedIn: 'root'
})
export class PedidoService {
  private readonly http = inject(HttpClient);

  private readonly apiUrl = `${environment.apiUrl}/pedidos`;

  criar(request: CriarPedidoRequest): Observable<PedidoResponse> {
    return this.http.post<PedidoResponse>(
      this.apiUrl,
      request
    );
  }

  listar(
    pagina: number = 1,
    tamanhoPagina: number = 10
  ): Observable<PaginatedResponse<PedidoResponse>> {
    const params = new HttpParams()
      .set('pagina', pagina.toString())
      .set('tamanhoPagina', tamanhoPagina.toString());

    return this.http.get<PaginatedResponse<PedidoResponse>>(
      this.apiUrl,
      { params }
    );
  }

  buscarPorId(id: number): Observable<PedidoResponse> {
    return this.http.get<PedidoResponse>(
      `${this.apiUrl}/${id}`
    );
  }

  atualizarStatus(
    id: number,
    request: AtualizarStatusPedidoRequest
  ): Observable<PedidoResponse> {
    return this.http.patch<PedidoResponse>(
      `${this.apiUrl}/${id}/status`,
      request
    );
  }

  cancelar(id: number): Observable<PedidoResponse> {
    return this.http.patch<PedidoResponse>(
      `${this.apiUrl}/${id}/cancelar`,
      {}
    );
  }
}
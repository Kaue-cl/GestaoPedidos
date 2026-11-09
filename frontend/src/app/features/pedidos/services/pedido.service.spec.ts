
import { TestBed } from '@angular/core/testing';
import {
  provideHttpClient
} from '@angular/common/http';
import {
  HttpTestingController,
  provideHttpClientTesting
} from '@angular/common/http/testing';

import { PedidoService } from './pedido.service';
import { environment } from '../../../../environments/environment';
import {
  CriarPedidoRequest,
  PedidoResponse,
  PaginatedResponse
} from '../models/pedido.model';
import { StatusPedido } from '../../../core/models/status-pedido.enum';

describe('PedidoService', () => {
  let service: PedidoService;
  let httpMock: HttpTestingController;

  const apiUrl = `${environment.apiUrl}/pedidos`;

  const pedidoMock: PedidoResponse = {
    id: 1,
    clienteId: 1,
    clienteNome: 'Maria Silva',
    dataPedido: '2026-11-09T10:00:00',
    status: StatusPedido.Pendente,
    valorTotal: 100,
    itens: [
      {
        id: 1,
        produtoId: 2,
        quantidade: 2,
        precoUnitario: 50,
        subtotal: 100
      }
    ]
  };

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        PedidoService,
        provideHttpClient(),
        provideHttpClientTesting()
      ]
    });

    service = TestBed.inject(PedidoService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('deve criar um pedido', () => {
    const request: CriarPedidoRequest = {
      clienteId: 1,
      itens: [
        {
          produtoId: 2,
          quantidade: 2
        }
      ]
    };

    service.criar(request).subscribe(response => {
      expect(response.id).toBe(1);
      expect(response.valorTotal).toBe(100);
    });

    const requisicao = httpMock.expectOne(apiUrl);

    expect(requisicao.request.method).toBe('POST');
    expect(requisicao.request.body).toEqual(request);

    requisicao.flush(pedidoMock);
  });

  it('deve listar pedidos com paginação padrão', () => {
    const resposta: PaginatedResponse<PedidoResponse> = {
      pagina: 1,
      tamanhoPagina: 10,
      totalRegistros: 1,
      totalPaginas: 1,
      dados: [pedidoMock]
    };

    service.listar().subscribe(response => {
      expect(response.totalRegistros).toBe(1);
      expect(response.dados.length).toBe(1);
    });

    const requisicao = httpMock.expectOne(
      req => req.url === apiUrl
    );

    expect(requisicao.request.method).toBe('GET');
    expect(requisicao.request.params.get('pagina')).toBe('1');
    expect(requisicao.request.params.get('tamanhoPagina')).toBe('10');
    expect(requisicao.request.params.get('ordenarPor')).toBe('data');
    expect(requisicao.request.params.get('direcao')).toBe('desc');

    requisicao.flush(resposta);
  });

  it('deve aplicar filtros na listagem', () => {
    service.listar(2, 5, {
      status: StatusPedido.Pago,
      cliente: '  Maria  ',
      ordenarPor: 'valor',
      direcao: 'asc'
    }).subscribe();

    const requisicao = httpMock.expectOne(
      req => req.url === apiUrl
    );

    expect(requisicao.request.method).toBe('GET');
    expect(requisicao.request.params.get('pagina')).toBe('2');
    expect(requisicao.request.params.get('tamanhoPagina')).toBe('5');
    expect(requisicao.request.params.get('status')).toBe('2');
    expect(requisicao.request.params.get('cliente')).toBe('Maria');
    expect(requisicao.request.params.get('ordenarPor')).toBe('valor');
    expect(requisicao.request.params.get('direcao')).toBe('asc');

    requisicao.flush({
      pagina: 2,
      tamanhoPagina: 5,
      totalRegistros: 0,
      totalPaginas: 0,
      dados: []
    });
  });

  it('deve buscar um pedido pelo ID', () => {
    service.buscarPorId(1).subscribe(response => {
      expect(response.id).toBe(1);
      expect(response.clienteNome).toBe('Maria Silva');
    });

    const requisicao = httpMock.expectOne(`${apiUrl}/1`);

    expect(requisicao.request.method).toBe('GET');

    requisicao.flush(pedidoMock);
  });

  it('deve atualizar o status de um pedido', () => {
    const request = {
      status: StatusPedido.Pago
    };

    service.atualizarStatus(1, request).subscribe(response => {
      expect(response.status).toBe(StatusPedido.Pago);
    });

    const requisicao = httpMock.expectOne(`${apiUrl}/1/status`);

    expect(requisicao.request.method).toBe('PATCH');
    expect(requisicao.request.body).toEqual(request);

    requisicao.flush({
      ...pedidoMock,
      status: StatusPedido.Pago
    });
  });

  it('deve cancelar um pedido', () => {
    service.cancelar(1).subscribe(response => {
      expect(response.status).toBe(StatusPedido.Cancelado);
    });

    const requisicao = httpMock.expectOne(`${apiUrl}/1/cancelar`);

    expect(requisicao.request.method).toBe('PATCH');
    expect(requisicao.request.body).toEqual({});

    requisicao.flush({
      ...pedidoMock,
      status: StatusPedido.Cancelado
    });
  });

  it('deve propagar erros HTTP da API', () => {
    service.buscarPorId(999).subscribe({
      next: () => {
        throw new Error('A requisição deveria falhar');
      },
      error: erro => {
        expect(erro.status).toBe(404);
      }
    });

    const requisicao = httpMock.expectOne(`${apiUrl}/999`);

    requisicao.flush(
      { mensagem: 'Pedido não encontrado' },
      {
        status: 404,
        statusText: 'Not Found'
      }
    );
  });
});

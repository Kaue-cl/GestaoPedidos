
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { of, throwError } from 'rxjs';
import { vi } from 'vitest';

import { MatDialog } from '@angular/material/dialog';
import { MatSnackBar } from '@angular/material/snack-bar';

import { ListaProdutos } from './lista-produtos';
import { ProdutoService } from '../../services/produto.service';
import { ProdutoResponse } from '../../models/produto.model';

import { LOCALE_ID } from '@angular/core';
import { registerLocaleData } from '@angular/common';
import localePt from '@angular/common/locales/pt';

registerLocaleData(localePt, 'pt-BR');

describe('ListaProdutos', () => {
  let fixture: ComponentFixture<ListaProdutos>;
  let component: ListaProdutos;

  const produtoMock: ProdutoResponse = {
    id: 1,
    nome: 'Mouse Gamer',
    preco: 150,
    estoqueAtual: 10,
    ativo: true
  };

  const produtoServiceMock = {
    listar: vi.fn(),
    listarPaginado: vi.fn(),
    criar: vi.fn(),
    atualizar: vi.fn(),
    excluir: vi.fn()
  };

  const snackBarMock = {
    open: vi.fn()
  };

  const dialogMock = {
    open: vi.fn(),
    afterAllClosed: of(undefined),
    afterOpened: of(undefined)
  };

  beforeEach(async () => {
    vi.resetAllMocks();

    produtoServiceMock.listarPaginado.mockReturnValue(
        of({
          pagina: 1,
          tamanhoPagina: 10,
          totalRegistros: 1,
          totalPaginas: 1,
          dados: [produtoMock]
        })
      );
    produtoServiceMock.criar.mockReturnValue(of(produtoMock));
    produtoServiceMock.atualizar.mockReturnValue(of(undefined));
    produtoServiceMock.excluir.mockReturnValue(of(undefined));

    await TestBed.configureTestingModule({
      imports: [ListaProdutos],
      providers: [
        {
          provide: ProdutoService,
          useValue: produtoServiceMock
        },
        {
          provide: MatSnackBar,
          useValue: snackBarMock
        },
        {
          provide: MatDialog,
          useValue: dialogMock
        },
        {
          provide: LOCALE_ID,
          useValue: 'pt-BR'
        }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(ListaProdutos);
    component = fixture.componentInstance;
    
    Object.defineProperty(component, 'dialog', {
      value: dialogMock,
      configurable: true
    });
    
    Object.defineProperty(component, 'snackBar', {
      value: snackBarMock,
      configurable: true
    });
  });

  afterEach(() => {
    fixture.destroy();
  });

  it('deve criar o componente', () => {
    expect(component).toBeTruthy();
  });

  it('deve carregar os produtos ao inicializar', () => {
    fixture.detectChanges();

    expect(produtoServiceMock.listarPaginado).toHaveBeenCalledTimes(1);
    expect(component.produtos()).toEqual([produtoMock]);
    expect(component.carregando()).toBe(false);
    expect(component.erro()).toBe(false);
  });

  it('deve exibir os produtos na tabela', () => {
    fixture.detectChanges();

    const elemento = fixture.nativeElement as HTMLElement;
    const texto = elemento.textContent ?? '';

    expect(texto).toContain('Mouse Gamer');
    expect(texto).toContain('150,00');
    expect(texto).toContain('10');
  });

  it('deve abrir o formulário para cadastrar um produto', () => {
    component.novoProduto();

    expect(component.formularioAberto()).toBe(true);
    expect(component.produtoEmEdicao()).toBeNull();
    expect(component.formulario.controls.nome.value).toBe('');
    expect(component.formulario.controls.ativo.value).toBe(true);
  });

  it('não deve salvar um formulário inválido', () => {
    component.novoProduto();

    component.salvarProduto();

    expect(component.formulario.invalid).toBe(true);
    expect(produtoServiceMock.criar).not.toHaveBeenCalled();
  });

  it('deve cadastrar um produto válido', () => {
    fixture.detectChanges();

    component.novoProduto();

    component.formulario.setValue({
      nome: '  Teclado Mecânico  ',
      preco: 250,
      estoqueAtual: 5,
      ativo: true
    });

    component.salvarProduto();

    expect(produtoServiceMock.criar).toHaveBeenCalledWith({
      nome: 'Teclado Mecânico',
      preco: 250,
      estoqueAtual: 5,
      ativo: true
    });

    expect(component.formularioAberto()).toBe(false);
    expect(produtoServiceMock.listarPaginado).toHaveBeenCalledTimes(2);
    expect(snackBarMock.open).toHaveBeenCalled();
  });

  it('deve preencher o formulário ao editar um produto', () => {
    component.editarProduto(produtoMock);

    expect(component.formularioAberto()).toBe(true);
    expect(component.produtoEmEdicao()).toBe(1);

    expect(component.formulario.getRawValue()).toEqual({
      nome: 'Mouse Gamer',
      preco: 150,
      estoqueAtual: 10,
      ativo: true
    });
  });

  it('deve atualizar um produto existente', () => {
    fixture.detectChanges();

    component.editarProduto(produtoMock);

    component.formulario.patchValue({
      nome: 'Mouse Gamer Atualizado',
      preco: 180
    });

    component.salvarProduto();

    expect(produtoServiceMock.atualizar).toHaveBeenCalledWith(
      1,
      {
        nome: 'Mouse Gamer Atualizado',
        preco: 180,
        estoqueAtual: 10,
        ativo: true
      }
    );

    expect(component.formularioAberto()).toBe(false);
    expect(produtoServiceMock.listarPaginado).toHaveBeenCalledTimes(2);
  });

  it('não deve excluir um produto sem confirmação', () => {
    dialogMock.open.mockReturnValue({
      afterClosed: () => of(false)
    });

    component.excluirProduto(produtoMock);

    expect(dialogMock.open).toHaveBeenCalled();
    expect(produtoServiceMock.excluir).not.toHaveBeenCalled();
  });

  it('deve excluir um produto após confirmação', () => {
    fixture.detectChanges();

    dialogMock.open.mockReturnValue({
      afterClosed: () => of(true)
    });

    component.excluirProduto(produtoMock);

    expect(produtoServiceMock.excluir).toHaveBeenCalledWith(1);
    expect(component.excluindoId()).toBeNull();
    expect(produtoServiceMock.listarPaginado).toHaveBeenCalledTimes(2);
    expect(snackBarMock.open).toHaveBeenCalled();
  });

  it('deve tratar erros ao carregar produtos', () => {
    produtoServiceMock.listarPaginado.mockReturnValue(
      throwError(() => new Error('Erro ao carregar produtos'))
    );

    component.carregarProdutos();
    fixture.detectChanges();

    expect(component.erro()).toBe(true);
    expect(component.produtos()).toEqual([]);
    expect(component.carregando()).toBe(false);
  });
  
  it('deve aplicar filtros e voltar para a primeira página', () => {
    component.paginaAtual.set(2);

    component.filtros.patchValue({
      nome: 'Mouse',
      ativo: 'ativos',
      ordenarPor: 'preco',
      direcao: 'desc'
    });

    component.aplicarFiltros();

    expect(component.paginaAtual()).toBe(0);

    expect(produtoServiceMock.listarPaginado).toHaveBeenLastCalledWith(
      expect.objectContaining({
        pagina: 1,
        nome: 'Mouse',
        ativo: true,
        ordenarPor: 'preco',
        direcao: 'desc'
      })
    );
  });
});

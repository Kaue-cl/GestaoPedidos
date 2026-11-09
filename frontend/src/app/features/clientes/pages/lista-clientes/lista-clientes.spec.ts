
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { of, throwError } from 'rxjs';
import { vi } from 'vitest';

import { MatDialog } from '@angular/material/dialog';
import { MatSnackBar } from '@angular/material/snack-bar';

import { ListaClientes } from './lista-clientes';
import { ClienteService } from '../../services/cliente.service';
import { ClienteResponse } from '../../models/cliente.model';

describe('ListaClientes', () => {
  let fixture: ComponentFixture<ListaClientes>;
  let component: ListaClientes;

  const clienteMock: ClienteResponse = {
    id: 1,
    nome: 'Maria Silva',
    email: 'maria@email.com',
    telefone: '11999999999',
    dataCriacao: '2026-11-09T10:00:00'
  };

  const clienteServiceMock = {
    listar: vi.fn(),
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

    clienteServiceMock.listar.mockReturnValue(of([clienteMock]));
    clienteServiceMock.criar.mockReturnValue(of(clienteMock));
    clienteServiceMock.atualizar.mockReturnValue(of(undefined));
    clienteServiceMock.excluir.mockReturnValue(of(undefined));

    await TestBed.configureTestingModule({
      imports: [ListaClientes],
      providers: [
        {
          provide: ClienteService,
          useValue: clienteServiceMock
        },
        {
          provide: MatSnackBar,
          useValue: snackBarMock
        },
        {
          provide: MatDialog,
          useValue: dialogMock
        }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(ListaClientes);
    component = fixture.componentInstance;

    // Garante que o componente utilize os mocks
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

  it('deve carregar os clientes ao inicializar', () => {
    fixture.detectChanges();

    expect(clienteServiceMock.listar).toHaveBeenCalledTimes(1);
    expect(component.clientes()).toEqual([clienteMock]);
    expect(component.carregando()).toBe(false);
    expect(component.erro()).toBe(false);
  });

  it('deve exibir os clientes na tabela', () => {
    fixture.detectChanges();

    const elemento = fixture.nativeElement as HTMLElement;
    const texto = elemento.textContent ?? '';

    expect(texto).toContain('Maria Silva');
    expect(texto).toContain('maria@email.com');
    expect(texto).toContain('11999999999');
  });

  it('deve abrir o formulário para cadastrar um cliente', () => {
    component.novoCliente();

    expect(component.formularioAberto()).toBe(true);
    expect(component.clienteEmEdicao()).toBeNull();

    expect(component.formulario.controls.nome.value).toBe('');
    expect(component.formulario.controls.email.value).toBe('');
    expect(component.formulario.controls.telefone.value).toBe('');
  });

  it('não deve salvar um formulário inválido', () => {
    component.novoCliente();
    component.salvarCliente();

    expect(component.formulario.invalid).toBe(true);
    expect(clienteServiceMock.criar).not.toHaveBeenCalled();
  });

  it('deve cadastrar um cliente válido', () => {
    fixture.detectChanges();

    component.novoCliente();

    component.formulario.setValue({
      nome: 'João Santos',
      email: 'joao@email.com',
      telefone: '11988888888'
    });

    component.salvarCliente();

    expect(clienteServiceMock.criar).toHaveBeenCalledWith({
      nome: 'João Santos',
      email: 'joao@email.com',
      telefone: '11988888888'
    });

    expect(component.formularioAberto()).toBe(false);
    expect(component.salvando()).toBe(false);
    expect(clienteServiceMock.listar).toHaveBeenCalledTimes(2);

    expect(snackBarMock.open).toHaveBeenCalledWith(
      'Cliente cadastrado com sucesso.',
      'Fechar',
      { duration: 3000 }
    );
  });

  it('deve preencher o formulário ao editar um cliente', () => {
    component.editarCliente(clienteMock);

    expect(component.formularioAberto()).toBe(true);
    expect(component.clienteEmEdicao()).toBe(1);

    expect(component.formulario.getRawValue()).toEqual({
      nome: 'Maria Silva',
      email: 'maria@email.com',
      telefone: '11999999999'
    });
  });

  it('deve atualizar um cliente existente', () => {
    fixture.detectChanges();

    component.editarCliente(clienteMock);

    component.formulario.patchValue({
      nome: 'Maria Oliveira',
      email: 'maria.oliveira@email.com'
    });

    component.salvarCliente();

    expect(clienteServiceMock.atualizar).toHaveBeenCalledWith(
      1,
      {
        nome: 'Maria Oliveira',
        email: 'maria.oliveira@email.com',
        telefone: '11999999999'
      }
    );

    expect(component.formularioAberto()).toBe(false);
    expect(component.salvando()).toBe(false);
    expect(clienteServiceMock.listar).toHaveBeenCalledTimes(2);

    expect(snackBarMock.open).toHaveBeenCalledWith(
      'Cliente atualizado com sucesso.',
      'Fechar',
      { duration: 3000 }
    );
  });

  it('não deve excluir um cliente sem confirmação', () => {
    dialogMock.open.mockReturnValue({
      afterClosed: () => of(false)
    });

    component.excluirCliente(clienteMock);

    expect(dialogMock.open).toHaveBeenCalled();
    expect(clienteServiceMock.excluir).not.toHaveBeenCalled();
  });

  it('deve excluir um cliente após confirmação', () => {
    fixture.detectChanges();

    dialogMock.open.mockReturnValue({
      afterClosed: () => of(true)
    });

    component.excluirCliente(clienteMock);

    expect(clienteServiceMock.excluir).toHaveBeenCalledWith(1);
    expect(component.excluindoId()).toBeNull();
    expect(clienteServiceMock.listar).toHaveBeenCalledTimes(2);

    expect(snackBarMock.open).toHaveBeenCalledWith(
      'Cliente excluído com sucesso.',
      'Fechar',
      { duration: 3000 }
    );
  });

  it('deve tratar erros ao carregar clientes', () => {
    clienteServiceMock.listar.mockReturnValue(
      throwError(() => new Error('API indisponível'))
    );

    fixture.detectChanges();

    expect(component.erro()).toBe(true);
    expect(component.clientes()).toEqual([]);
    expect(component.carregando()).toBe(false);
  });
});

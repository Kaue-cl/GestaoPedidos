# Gestão de Pedidos — Dashboard Operacional

Sistema Full Stack para gerenciamento de clientes, produtos e pedidos, com controle de estoque, acompanhamento de status e dashboard operacional.

Desenvolvido como parte de um teste técnico Full Stack, utilizando **ASP.NET Core 10**, **Angular 21**, **Entity Framework Core** e **SQLite**.

## 1. Tecnologias utilizadas

### Backend
- .NET 10 / ASP.NET Core Web API
- C#
- Entity Framework Core
- SQLite
- Swagger / OpenAPI
- Serilog
- Testes automatizados

### Frontend
- Angular 21
- TypeScript
- Angular Material
- RxJS
- Angular Signals
- Reactive Forms
- Vitest

## 2. Funcionalidades

### Clientes
- Cadastro de clientes
- Listagem de clientes
- Edição de informações
- Exclusão com confirmação
- Validação dos campos obrigatórios e do e-mail

### Produtos
- Cadastro e edição de produtos
- Consulta de produtos
- Controle de estoque
- Ativação e desativação de produtos
- Exclusão com confirmação

### Pedidos
- Criação de pedidos vinculados a clientes
- Inclusão de múltiplos produtos
- Validação da disponibilidade de estoque
- Cálculo automático do valor total
- Atualização de status
- Cancelamento de pedidos com confirmação
- Devolução ao estoque dos itens de pedidos cancelados
- Listagem paginada
- Filtro por status e nome do cliente
- Ordenação por data e valor

### Dashboard
- Total de pedidos
- Faturamento total
- Ticket médio
- Distribuição de pedidos por status
- Cinco produtos mais vendidos
- Atualização automática a cada 30 segundos
- Atualização manual dos indicadores

## 3. Arquitetura

O backend utiliza uma arquitetura em camadas:

**Controller → Service → Repository → Banco de dados**

- **Controllers:** recebem requisições HTTP e retornam as respostas da API.
- **Services:** implementam as regras de negócio.
- **Repositories:** realizam operações de persistência.
- **Entities:** representam as entidades do domínio.
- **DTOs:** definem os contratos de entrada e saída da API.
- **Middleware:** centraliza o tratamento de exceções.

O frontend utiliza componentes standalone, organizados por funcionalidade, com separação entre páginas, serviços e modelos.

## 4. Estrutura do repositório

```text
GestaoPedidos/
├── backend/
│   └── GestaoPedidos.API/
│       ├── Controllers/
│       ├── Services/
│       ├── Repositories/
│       ├── Data/
│       ├── Entities/
│       ├── DTOs/
│       ├── Middleware/
│       └── Program.cs
├── frontend/
│   └── src/
│       └── app/
│           ├── core/
│           ├── shared/
│           └── features/
│               ├── clientes/
│               ├── produtos/
│               ├── pedidos/
│               └── dashboard/
├── .gitignore
└── README.md
```

## 5. Pré-requisitos

Para executar o projeto localmente, é necessário ter:

- .NET SDK 10
- Node.js 22 ou versão compatível com Angular 21
- npm
- Angular CLI 21
- Git

O banco de dados utilizado é o SQLite, portanto não é necessário instalar um servidor de banco de dados.

## 6. Executando o backend

Abra um terminal na pasta do projeto da API:

```powershell
cd backend/GestaoPedidos.API
```

Restaure as dependências:

```powershell
dotnet restore
```

Aplique as migrations do Entity Framework Core:

```powershell
dotnet ef database update
```

Execute a aplicação:

```powershell
dotnet run
```

Na configuração de desenvolvimento utilizada neste projeto, a API é executada em:

`http://localhost:5245`

A documentação interativa dos endpoints pode ser consultada pelo Swagger, quando habilitado no ambiente de desenvolvimento.

O banco SQLite é utilizado para persistência de clientes, produtos e pedidos.

## 7. Executando o frontend

Em outro terminal, acesse a pasta do Angular:

```powershell
cd frontend
```

Instale as dependências:

```powershell
npm install
```

Inicie o servidor de desenvolvimento:

```powershell
npm start
```

Acesse:

`http://localhost:4200`

O frontend está configurado para consumir a API em:

`http://localhost:5245/api`

Essa URL é definida no arquivo `frontend/src/environments/environment.ts`.

**Importante:** mantenha o backend em execução enquanto utiliza o frontend.

## 8. Endpoints principais

### Clientes

| Método | Endpoint | Descrição |
|---|---|---|
| GET | `/api/clientes` | Listar clientes |
| GET | `/api/clientes/{id}` | Consultar cliente |
| POST | `/api/clientes` | Cadastrar cliente |
| PUT | `/api/clientes/{id}` | Atualizar cliente |
| DELETE | `/api/clientes/{id}` | Excluir cliente |

### Produtos

| Método | Endpoint | Descrição |
|---|---|---|
| GET | `/api/produtos` | Listar produtos |
| GET | `/api/produtos/{id}` | Consultar produto |
| POST | `/api/produtos` | Cadastrar produto |
| PUT | `/api/produtos/{id}` | Atualizar produto |
| DELETE | `/api/produtos/{id}` | Excluir produto |

### Pedidos

| Método | Endpoint | Descrição |
|---|---|---|
| GET | `/api/pedidos` | Listar e filtrar pedidos |
| GET | `/api/pedidos/{id}` | Consultar pedido |
| POST | `/api/pedidos` | Criar pedido |
| PATCH | `/api/pedidos/{id}/status` | Atualizar status |
| PATCH | `/api/pedidos/{id}/cancelar` | Cancelar pedido |

### Dashboard

| Método | Endpoint | Descrição |
|---|---|---|
| GET | `/api/dashboard` | Consultar indicadores operacionais |

## 9. Regras de negócio

### Fluxo de status

```text
Pendente → Pago → Enviado → Entregue
    |
    └────→ Cancelado
```

O cancelamento é permitido apenas enquanto o pedido estiver pendente.

### Controle de estoque

Ao criar um pedido, o sistema valida a disponibilidade dos produtos e desconta as quantidades solicitadas do estoque.

Quando um pedido pendente é cancelado, as quantidades reservadas são devolvidas ao estoque.

### Indicadores financeiros

O faturamento total considera os pedidos com status Pago, Enviado ou Entregue.

Pedidos pendentes e cancelados não entram no cálculo do faturamento.

## 10. Testes automatizados

### Backend

Execute os testes a partir da solução ou projeto de testes correspondente:

```powershell
dotnet test
```

Durante o desenvolvimento, foram validados 25 testes automatizados no backend.

### Frontend

Acesse a pasta `frontend` e execute:

```powershell
npm test -- --watch=false
```

A suíte frontend contempla 40 testes, incluindo:

- Inicialização da aplicação
- Serviço HTTP de pedidos
- Dashboard
- Gerenciamento de produtos
- Gerenciamento de clientes

Para validar a compilação de produção:

```powershell
npm run build
```

## 11. Tratamento de erros

O backend possui tratamento centralizado de exceções, com respostas HTTP adequadas para erros de validação, recursos inexistentes e conflitos de negócio.

O frontend utiliza um interceptor HTTP para apresentar mensagens de erro ao usuário por meio de notificações do Angular Material.

Os erros são tratados sem depender de dados simulados nas funcionalidades da aplicação.

## 12. Considerações técnicas

- O SQLite foi escolhido para simplificar a instalação e a execução local.
- A arquitetura em camadas facilita a manutenção e a evolução das regras de negócio.
- Os DTOs evitam expor diretamente as entidades de persistência.
- Os componentes standalone simplificam a organização do Angular.
- Os Signals gerenciam o estado local das telas.
- O RxJS é utilizado nas requisições HTTP e na atualização periódica do dashboard.
- O Angular Material fornece componentes de interface reutilizáveis.

## 13. Melhorias futuras

- Autenticação e autorização
- Testes end-to-end
- Paginação de clientes e produtos
- Filtros adicionais nos relatórios
- Atualizações em tempo real com SignalR
- Containerização com Docker
- Pipeline de integração contínua

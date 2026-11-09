import { Routes } from '@angular/router';

import { Layout } from './shared/components/layout/layout';

export const routes: Routes = [
  {
    path: '',
    component: Layout,
    children: [
      {
        path: '',
        redirectTo: 'dashboard',
        pathMatch: 'full'
      },
      {
        path: 'dashboard',
        loadComponent: () =>
          import('./features/dashboard/pages/dashboard/dashboard')
            .then(m => m.Dashboard)
      },
      {
        path: 'pedidos',
        loadComponent: () =>
          import('./features/pedidos/pages/lista-pedidos/lista-pedidos')
            .then(m => m.ListaPedidos)
      },
      {
        path: 'pedidos/novo',
        loadComponent: () =>
          import('./features/pedidos/pages/novo-pedido/novo-pedido')
            .then(m => m.NovoPedido)
      },
      {
        path: 'clientes',
        loadComponent: () =>
          import('./features/clientes/pages/lista-clientes/lista-clientes')
            .then(m => m.ListaClientes)
      },
      {
        path: 'produtos',
        loadComponent: () =>
          import('./features/produtos/pages/lista-produtos/lista-produtos')
            .then(m => m.ListaProdutos)
      },
      
    ]
  },
  {
    path: '**',
    redirectTo: 'dashboard'
  }
];
import { Routes } from '@angular/router';
import { authGuard } from './guards/auth.guard';

export const routes: Routes = [
  
  { 
    path: 'login', 
    loadComponent: () => import('./login/login').then(m => m.LoginComponent)
  },
  { 
    path: 'search', 
    loadComponent: () => import('./search/search').then(m => m.SearchComponent),
    canActivate: [authGuard]
  },
  { 
    path: 'import', 
    loadComponent: () => import('./import/import').then(m => m.ImportComponent),
    canActivate: [authGuard]
  },
  { 
    path: 'player/:id', 
    loadComponent: () => import('./player/player').then(m => m.PlayerComponent),
    canActivate: [authGuard]
  },
  { 
    path: 'edit/:id', 
    loadComponent: () => import('./media-editor/media-editor').then(m => m.MediaEditorComponent),
    canActivate: [authGuard]
  },
  { 
    path: 'edit', 
    loadComponent: () => import('./media-editor/media-editor').then(m => m.MediaEditorComponent),
    canActivate: [authGuard]
  },
  { 
    path: 'cast', 
    loadComponent: () => import('./cast/cast').then(m => m.CastComponent),
    canActivate: [authGuard]
  },
  { 
    path: '**', 
    redirectTo: '/login'
  }
];

import { Routes } from '@angular/router';
import { authGuard } from './guards/auth.guard';
import { SearchComponent } from './search/search';

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
    path: 'import/:fileName', 
    loadComponent: () => import('./media-editor/media-editor').then(m => m.MediaEditorComponent),
    canActivate: [authGuard]
  },
  { 
    path: 'edit', 
    loadComponent: () => import('./media-editor/media-editor').then(m => m.MediaEditorComponent),
    canActivate: [authGuard]
  },
  { 
    path: 'meta/:type', 
    loadComponent: () => import('./meta/meta').then(m => m.MetaComponent),
    canActivate: [authGuard]
  },
  { 
    path: 'user-management', 
    loadComponent: () => import('./user-management/user-management').then(m => m.UserManagementComponent),
    canActivate: [authGuard]
  },
  { 
    path: '**', 
    redirectTo: '/search?sort=' + SearchComponent.DEFAULT_SORT
  }
];

import { Routes } from '@angular/router';

export const routes: Routes = [
  { path: '', redirectTo: '/search', pathMatch: 'full' },
  { 
    path: 'search', 
    loadComponent: () => import('./search/search').then(m => m.SearchComponent)
  },
  { 
    path: 'import', 
    loadComponent: () => import('./import/import').then(m => m.ImportComponent)
  },
  { 
    path: 'player/:id', 
    loadComponent: () => import('./player/player').then(m => m.PlayerComponent)
  },
  { 
    path: 'edit/:id', 
    loadComponent: () => import('./media-editor/media-editor').then(m => m.MediaEditorComponent)
  },
  { 
    path: 'edit', 
    loadComponent: () => import('./media-editor/media-editor').then(m => m.MediaEditorComponent)
  },
  { 
    path: 'cast', 
    loadComponent: () => import('./cast/cast').then(m => m.CastComponent)
  }
];

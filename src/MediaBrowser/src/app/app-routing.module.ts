import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';

import { AddFileComponent } from './add-file/add-file.component';
import { AddRoleComponent } from './add-role/add-role.component';
import { AddUserComponent } from './add-user/add-user.component';
import { FileInfoComponent } from './file-info/file-info.component';
import { FileComponent } from './file/file.component';
import { FilesComponent } from './files/files.component';
import { RoleComponent } from './role/role.component';
import { RolesComponent } from './roles/roles.component';
import { UserComponent } from './user/user.component';
import { UsersComponent } from './users/users.component';

const routes: Routes = [
  { path: 'AddFile', component: AddFileComponent },
  { path: 'File/:id/View', component: FileComponent },
  { path: 'File/:id/Info', component: FileInfoComponent },
  { path: 'Files', component: FilesComponent },
  { path: 'Files/:pageSelection', component: FilesComponent },

  { path: 'AddRole', component: AddRoleComponent },
  { path: 'Roles', component: RolesComponent },
  { path: 'Roles/:pageSelection', component: RolesComponent },
  { path: 'Role/:id', component: RoleComponent },

  { path: 'AddUser', component: AddUserComponent },
  { path: 'Users', component: UsersComponent },
  { path: 'Users/:pageSelection', component: UsersComponent },
  { path: 'User/:id', component: UserComponent },
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }

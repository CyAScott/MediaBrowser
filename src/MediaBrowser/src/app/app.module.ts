import { AppComponent } from './app.component';
import { AppRoutingModule } from './app-routing.module';
import { BrowserModule } from '@angular/platform-browser';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { HttpClientModule } from '@angular/common/http';
import { NgModule } from '@angular/core';
import { RouterModule } from '@angular/router';

import { AddFileComponent } from './add-file/add-file.component';
import { AddRoleComponent } from './add-role/add-role.component';
import { AddUserComponent } from './add-user/add-user.component';
import { FileComponent } from './file/file.component';
import { FilesComponent } from './files/files.component';
import { RoleComponent } from './role/role.component';
import { RolesComponent } from './roles/roles.component';
import { UserComponent } from './user/user.component';
import { UsersComponent } from './users/users.component';
import { ModalsComponent } from './modals/modals.component';
import { PaginationComponent } from './pagination/pagination.component';
import { SearchComponent } from './search/search.component';
import { FileInfoComponent } from './file-info/file-info.component';
import { RoleBuilderComponent } from './role-builder/role-builder.component';
import { AddPlaylistComponent } from './add-playlist/add-playlist.component';
import { PlaylistComponent } from './playlist/playlist.component';
import { PlaylistsComponent } from './playlists/playlists.component';
import { ThumbnailBuilderComponent } from './thumbnail-builder/thumbnail-builder.component';
import { PlaylistBuilderComponent } from './playlist-builder/playlist-builder.component';
import { FormatMillisecondsPipe } from './format-milliseconds.pipe';

@NgModule({
  declarations: [
    AppComponent,
    AddFileComponent,
    AddRoleComponent,
    AddUserComponent,
    FileComponent,
    FilesComponent,
    RoleComponent,
    RolesComponent,
    UserComponent,
    UsersComponent,
    ModalsComponent,
    PaginationComponent,
    SearchComponent,
    FileComponent,
    FileInfoComponent,
    RoleBuilderComponent,
    AddPlaylistComponent,
    PlaylistComponent,
    PlaylistsComponent,
    ThumbnailBuilderComponent,
    PlaylistBuilderComponent,
    FormatMillisecondsPipe
  ],
  imports: [
    BrowserModule,
    CommonModule,
    AppRoutingModule,
    FormsModule,
    HttpClientModule,
    RouterModule
  ],
  providers: [],
  bootstrap: [AppComponent]
})
export class AppModule { }

import { Component } from '@angular/core';
import { CommonControlsService, Page } from '../common-controls.service';
import { LoggerService } from '../logger.service';
import { MsgBoxType } from '../modals/modals.component';
import { PlaylistsService } from '../playlists.service';
import { Thumbnail } from '../thumbnail-builder/thumbnail-builder.component';
import { UsersService } from '../users.service';

@Component({
  selector: 'app-add-playlist',
  templateUrl: './add-playlist.component.html',
  styleUrls: ['./add-playlist.component.less']
})
export class AddPlaylistComponent extends Page {

  constructor(
    controls : CommonControlsService,
    logger : LoggerService,
    private playlists : PlaylistsService,
    public users : UsersService) {
    super(controls, logger, 'AddPlaylist')
  }

  public add(): void {
    if (!this.name || !this.name.length) {
      this.controls.modals?.getMsgBoxResult(MsgBoxType.Ok, 'Please provide a playlist name.');
      return;
    }

    this.controls.modals?.toggleLoader();

    let thumbnailFiles : any[] = this.thumbnails.map(it => it.file);

    this.playlists.create({
      description: this.description,
      name: this.name,
      readRoles : this.readRoles,
      updateRoles : this.updateRoles
    }, thumbnailFiles)
    .subscribe(response => {
      this.controls.modals?.toggleLoader();
      this.controls.refresh([ 'Playlists' ]);
    },
    error => {
      this.controls.modals?.toggleLoader();
      this.controls.modals?.getMsgBoxResult(MsgBoxType.Ok, 'Add playlist error.');
    });
  }

  public description : string = '';

  public name : string = '';

  public readRoles : string[] = [];

  public thumbnails : Thumbnail[] = [];

  public updateRoles : string[] = [];
}

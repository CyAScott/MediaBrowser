import { Component } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { CommonControlsService, Page } from '../common-controls.service';
import { PlaylistReadModel, PlaylistsService } from '../playlists.service';
import { LoggerService } from '../logger.service';
import { MsgBoxType } from '../modals/modals.component';
import { UsersService } from '../users.service';
import { Thumbnail } from '../thumbnail-builder/thumbnail-builder.component';

@Component({
  selector: 'app-playlist',
  templateUrl: './playlist.component.html',
  styleUrls: ['./playlist.component.less']
})
export class PlaylistComponent extends Page {

  constructor(
    controls: CommonControlsService,
    logger : LoggerService,
    route : ActivatedRoute,
    private playlists : PlaylistsService,
    public users : UsersService) {
    super(controls, logger, 'Playlist')
    route.params.subscribe(params => this.id = params.id);
  }
  
  public playlist : PlaylistReadModel | undefined;

  public description : string = '';

  public id : string = '';

  public init() : void {
    this.playlists
      .get(this.id)
      .subscribe(playlist => {
        this.controls.modals?.toggleLoader();
        this.description = playlist.description;
        this.name = playlist.name;
        this.playlist = playlist;
        this.readRoles = playlist.readRoles;
        this.updateRoles = playlist.updateRoles;

        if (playlist.thumbnails) {
          this.thumbnails = playlist.thumbnails.map(it => {
            let thumbnail = new Thumbnail();

            thumbnail.file = undefined;
            thumbnail.id = it.md5;
            thumbnail.name = '';
            thumbnail.src = it.url;

            return thumbnail;
          });
        }
      });
  }

  public name : string = '';

  public readRoles : string[] = [];

  public thumbnails : Thumbnail[] = [];

  public thumbnailsToRemove : string[] = [];

  public updateRoles : string[] = [];

  public update(): void {
    this.controls.modals?.toggleLoader();

    let thumbnailFiles : any[] = this.thumbnails.map(it => it.file);

    this.playlists.update(this.id, {
      description: this.description,
      name: this.name,
      readRoles: this.readRoles,
      thumbnailsToRemove: this.thumbnailsToRemove,
      updateRoles: this.updateRoles
    }, thumbnailFiles)
    .subscribe(response => {
      this.controls.modals?.toggleLoader();
      this.controls.refresh([ 'Playlists' ]);
    },
    error => {
      this.controls.modals?.toggleLoader();
      this.controls.modals?.getMsgBoxResult(MsgBoxType.Ok, 'Update playlist error.');
    });
  }
}

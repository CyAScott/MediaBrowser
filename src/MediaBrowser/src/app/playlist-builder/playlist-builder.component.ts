import { Component, ElementRef, Input, ViewChild } from '@angular/core';
import { CommonControlsService } from '../common-controls.service';
import { MsgBoxResult, MsgBoxType } from '../modals/modals.component';
import { PlaylistFilterOptions, PlaylistReadModel, PlaylistSortOptions, PlaylistsService } from '../playlists.service';

@Component({
  selector: 'app-playlist-builder',
  templateUrl: './playlist-builder.component.html',
  styleUrls: ['./playlist-builder.component.less']
})
export class PlaylistBuilderComponent {

  constructor(
    private controls : CommonControlsService,
    private playlists : PlaylistsService) { }

  public addToPlaylist(playlistMatch : PlaylistReadModel) : void {
    this.playlistMatches = [];

    if (this.playlistInput?.nativeElement) {
      this.playlistInput.nativeElement.value = '';
    }

    if (this.playlistsForFile.every(it => it.id !== playlistMatch.id)) {
      
      this.controls.modals?.toggleLoader();

      this.playlists.setPlaylistReference(playlistMatch.id, this.fileid).subscribe(playlist => {

        this.controls.modals?.toggleLoader();

        if (this.playlistsForFile.every(it => it.id !== playlist.id)) {
          this.playlistsForFile.push(playlistMatch);
        }
      });
    }
  }

  @Input()
  public fileid : string = '';

  @Input()
  public playlistsForFile : PlaylistReadModel[] = [];

  @ViewChild('playlistInput')
  public playlistInput : ElementRef | undefined;

  public playlistMatches : PlaylistReadModel[] = [];

  public async removeFromPlaylist(playlistMatch : PlaylistReadModel) : Promise<void> {
    if (!this.controls.modals) {
      return;
    }

    let result = await this.controls.modals.getMsgBoxResult(MsgBoxType.YesNo, `Do you want to remove this file from ${playlistMatch.name}?`);
    if (result !== MsgBoxResult.Yes) {
      return;
    }

    this.controls.modals.toggleLoader();

    this.playlists.deletePlaylistReference(playlistMatch.id, this.fileid).subscribe(it => {
      this.controls.modals?.toggleLoader();

      const index = this.playlistsForFile.findIndex(it => it.id === playlistMatch.id);
      if (index > -1) {
        this.playlistsForFile.splice(index, 1);
      }
    });
  }

  public timerHandle : any;

  public updatePlaylistMatches(event: any) : void {
    if (event.code === 'Enter') {
      if (this.playlistMatches.length) {
        this.addToPlaylist(this.playlistMatches[0]);
      } else if (this.playlistInput?.nativeElement) {
        this.playlistInput.nativeElement.value = '';
      }
      return;
    }

    let input = this.playlistInput?.nativeElement.value as string;
    
    if (!input?.length) {
      this.playlistMatches = [];
      return;
    }

    if (this.timerHandle) {
      clearTimeout(this.timerHandle);
    }

    this.timerHandle = setTimeout(() => {
      this.timerHandle = undefined;

      if (this.waitingForResponse) {
        return;
      }

      this.waitingForResponse = true;

      this.playlists.search({
        keywords: input,
        skip: 0,
        take: 5,
        filter: PlaylistFilterOptions.noFilter,
        sort: PlaylistSortOptions.name,
        ascending: true
      }).subscribe(response => {
        this.waitingForResponse = false;
        this.playlistMatches = response.results;
      });
    }, 100);
  }

  public waitingForResponse : boolean = false;
}

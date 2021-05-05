import { Component } from '@angular/core';
import { CommonControlsService, PageSearchable } from '../common-controls.service';
import { PlaylistFilterOptions, PlaylistSortOptions, PlaylistsService, SearchPlaylistsRequest, SearchPlaylistsResponse } from '../playlists.service';
import { LoggerService } from '../logger.service';
import { SelectionOption } from '../modals/modals.component';

@Component({
  selector: 'app-playlists',
  templateUrl: './playlists.component.html',
  styleUrls: ['./playlists.component.less']
})
export class PlaylistsComponent extends PageSearchable {

  public static readonly filterOptions : SelectionOption[] = [
    new SelectionOption('No Filter', PlaylistFilterOptions.noFilter)
  ];
  public static readonly sortOptions : SelectionOption[] = [
    new SelectionOption('Created On', PlaylistSortOptions.createdOn),
    new SelectionOption('Name', PlaylistSortOptions.name)
  ];

  constructor(
    controls : CommonControlsService,
    logger : LoggerService,
    private playlists : PlaylistsService) {
    super(controls, logger, 'Playlists',
    PlaylistsComponent.filterOptions, PlaylistsComponent.filterOptions[0],
    PlaylistsComponent.sortOptions, PlaylistsComponent.sortOptions[0])
  }

  public add() : void {
    this.controls.refresh([ 'AddPlaylist' ]);
  }

  public addEnabled() : boolean {
    return true;
  }

  public init(): void {
    this.controls.autoPlay = false;
    this.playlists
      .search(new SearchPlaylistsRequest(this.getSearchRequest()))
      .subscribe(response => {
        this.controls.modals?.toggleLoader();
        if (this.controls.pagination) {
          this.controls.pagination.pageCount = Math.ceil(response.count / this.controls.pagination.countPerPage);
        }
        this.response = response;
      });
  }

  public response : SearchPlaylistsResponse | undefined;

}

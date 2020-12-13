import { Component } from '@angular/core';
import { CommonControlsService, PageSearchable } from '../common-controls.service';
import { LoggerService } from '../logger.service';
import { SelectionOption } from '../modals/modals.component';
import { SearchUsersRequest, SearchUsersResponse, UserFilterOptions, UserReadModel, UserSortOptions, UsersService } from '../users.service';

@Component({
  selector: 'app-users',
  templateUrl: './users.component.html',
  styleUrls: ['./users.component.less']
})
export class UsersComponent extends PageSearchable {

  public static readonly filterOptions : SelectionOption[] = [
    new SelectionOption('No Filter', UserFilterOptions.nofilter),
    new SelectionOption('Deleted', UserFilterOptions.deleted),
    new SelectionOption('Non Deleted', UserFilterOptions.nonDeleted)
  ];
  public static readonly sortOptions : SelectionOption[] = [
    new SelectionOption('User Name', UserSortOptions.userName),
    new SelectionOption('Deleted On', UserSortOptions.deletedOn),
    new SelectionOption('First Name', UserSortOptions.firstName),
    new SelectionOption('Last Name', UserSortOptions.lastName)
  ];

  constructor(
    controls : CommonControlsService,
    logger : LoggerService,
    private users : UsersService) {
    super(controls, logger, 'Users',
      UsersComponent.filterOptions, UsersComponent.filterOptions[0],
      UsersComponent.sortOptions, UsersComponent.sortOptions[0])
  }

  public add() : void {
    this.controls.refresh([ 'AddUser' ]);
  }
  
  public addEnabled() : boolean {
    return this.users.hasRole('Admin');
  }

  public init() : void {
    this.users
      .search(new SearchUsersRequest(this.getSearchRequest()))
      .subscribe(response => {
        this.controls.modals?.toggleLoader();
        if (this.controls.pagination) {
          this.controls.pagination.pageCount = Math.ceil(response.count / this.controls.pagination.countPerPage);
        }
        this.response = response;
      });
  }

  public view(user : UserReadModel) : void {
    if (this.controls.modals?.filterSelection) {
      this.controls.modals.filterSelection = undefined;
    }

    if (this.controls.modals?.sortSelection) {
      this.controls.modals.sortSelection = undefined;
    }
    
    this.controls.refresh([ 'User', user.id ]);
  }

  public response : SearchUsersResponse | undefined;
}

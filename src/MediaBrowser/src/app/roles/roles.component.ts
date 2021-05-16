import { Component } from '@angular/core';
import { CommonControlsService, PageSearchable } from '../common-controls.service';
import { LoggerService } from '../logger.service';
import { SelectionOption } from '../modals/modals.component';
import { RoleReadModel, RoleSortOptions, RolesService, SearchRolesRequest, SearchRolesResponse } from '../roles.service';
import { UsersService } from '../users.service';

@Component({
  selector: 'app-roles',
  templateUrl: './roles.component.html',
  styleUrls: ['./roles.component.less']
})
export class RolesComponent extends PageSearchable {

  public static readonly filterOptions : SelectionOption[] = [];
  public static readonly sortOptions : SelectionOption[] = [
    new SelectionOption('Description', RoleSortOptions.description),
    new SelectionOption('Name', RoleSortOptions.name)
  ];

  constructor(
    controls : CommonControlsService,
    logger : LoggerService,
    private roles : RolesService,
    public users : UsersService) {
    super(controls, logger, 'Roles',
      RolesComponent.filterOptions, undefined,
      RolesComponent.sortOptions, RolesComponent.sortOptions[1])
  }

  public add() : void {
    this.controls.refresh([ 'AddRole' ]);
  }

  public init() : void {
    this.roles
      .search(new SearchRolesRequest(this.getSearchRequest()))
      .subscribe(response => {
        this.controls.modals?.toggleLoader();
        if (this.controls.pagination) {
          this.controls.pagination.pageCount = Math.ceil(response.count / this.controls.pagination.countPerPage);
        }
        this.response = response;
      });
  }

  public view(role : RoleReadModel) : void {
    if (this.controls.modals?.filterSelection) {
      this.controls.modals.filterSelection = undefined;
    }

    if (this.controls.modals?.sortSelection) {
      this.controls.modals.sortSelection = undefined;
    }

    this.controls.refresh([ 'Role', role.id ]);
  }

  public response : SearchRolesResponse | undefined;
}

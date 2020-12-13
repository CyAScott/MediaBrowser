import { Component } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { CommonControlsService, Page } from '../common-controls.service';
import { LoggerService } from '../logger.service';
import { RoleReadModel, RolesService } from '../roles.service';

@Component({
  selector: 'app-role',
  templateUrl: './role.component.html',
  styleUrls: ['./role.component.less']
})
export class RoleComponent extends Page {

  constructor(
    controls: CommonControlsService,
    logger : LoggerService,
    route : ActivatedRoute,
    private roles : RolesService) {
    super(controls, logger, 'Role')
    route.params.subscribe(params => this.id = params.id);
  }

  public id : string = '';

  public init() : void {
    this.roles
      .get(this.id)
      .subscribe(role => {
        this.controls.modals?.toggleLoader();
        this.role = role;
      });
  }

  public role : RoleReadModel | undefined;
}

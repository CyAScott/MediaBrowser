import { Component } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { CommonControlsService, Page } from '../common-controls.service';
import { LoggerService } from '../logger.service';
import { MsgBoxType } from '../modals/modals.component';
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
        this.description = role.description;
      });
  }

  public description : string = '';

  public name : string = '';

  public update(): void {
    this.controls.modals?.toggleLoader();

    this.roles.update(this.id, {
      description: this.description
    })
    .subscribe(response => {
      this.controls.modals?.toggleLoader();
      this.controls.refresh([ 'Roles' ]);
    },
    error => {
      this.controls.modals?.toggleLoader();
      this.controls.modals?.getMsgBoxResult(MsgBoxType.Ok, 'Update role error.');
    });
  }
}

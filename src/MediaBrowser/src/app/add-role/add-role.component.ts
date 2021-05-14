import { Component } from '@angular/core';
import { CommonControlsService, Page } from '../common-controls.service';
import { LoggerService } from '../logger.service';
import { MsgBoxType } from '../modals/modals.component';
import { RolesService } from '../roles.service';

@Component({
  selector: 'app-add-role',
  templateUrl: './add-role.component.html',
  styleUrls: ['./add-role.component.less']
})
export class AddRoleComponent extends Page {

  constructor(
    controls: CommonControlsService,
    logger : LoggerService,
    private roles : RolesService) {
    super(controls, logger, 'AddRole');
  }

  public add(): void {
    if (!this.name || !this.name.length) {
      this.controls.modals?.getMsgBoxResult(MsgBoxType.Ok, 'Please provide a role name.');
      return;
    }

    this.controls.modals?.toggleLoader();

    this.roles.create({
      description: this.description,
      name: this.name.toUpperCase()
    })
    .subscribe(response => {
      this.controls.modals?.toggleLoader();
      this.controls.refresh([ 'Roles' ]);
    },
    error => {
      this.controls.modals?.toggleLoader();
      this.controls.modals?.getMsgBoxResult(MsgBoxType.Ok, 'Add role error.');
    });
  }

  public description : string = '';

  public name : string = '';
}

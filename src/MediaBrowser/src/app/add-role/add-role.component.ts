import { Component } from '@angular/core';
import { CommonControlsService, Page } from '../common-controls.service';
import { LoggerService } from '../logger.service';

@Component({
  selector: 'app-add-role',
  templateUrl: './add-role.component.html',
  styleUrls: ['./add-role.component.less']
})
export class AddRoleComponent extends Page {

  constructor(
    controls : CommonControlsService,
    logger : LoggerService) {
    super(controls, logger, 'AddRole');
  }
}

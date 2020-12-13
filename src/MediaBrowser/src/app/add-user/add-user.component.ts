import { Component } from '@angular/core';
import { CommonControlsService, Page } from '../common-controls.service';
import { LoggerService } from '../logger.service';

@Component({
  selector: 'app-add-user',
  templateUrl: './add-user.component.html',
  styleUrls: ['./add-user.component.less']
})
export class AddUserComponent extends Page {

  constructor(
    controls : CommonControlsService,
    logger : LoggerService) {
    super(controls, logger, 'AddUser');
  }
}

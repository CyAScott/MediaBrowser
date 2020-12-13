import { Component } from '@angular/core';
import { CommonControlsService, Page } from '../common-controls.service';
import { LoggerService } from '../logger.service';
import { MsgBoxType } from '../modals/modals.component';
import { UsersService } from '../users.service';

@Component({
  selector: 'app-add-user',
  templateUrl: './add-user.component.html',
  styleUrls: ['./add-user.component.less']
})
export class AddUserComponent extends Page {

  constructor(
    controls : CommonControlsService,
    logger : LoggerService,
    private users : UsersService) {
    super(controls, logger, 'AddUser');
  }

  public add(): void {
    try {
      if (!this.firstName || !this.firstName.length) {
        throw 'Please provide a first name.';
      }

      if (!this.lastName || !this.lastName.length) {
        throw 'Please provide a last name.';
      }

      if (!this.password || !this.password.length) {
        throw 'Please provide a password.';
      }

      if (this.password !== this.passwordConfirmed) {
        throw 'The passwords do not match.';
      }

      if (!this.userName || !this.userName.length) {
        throw 'Please provide a user name.';
      }
    } catch (error) {
      this.controls.modals?.getMsgBoxResult(MsgBoxType.Ok, error);
    }

    this.controls.modals?.toggleLoader();

    this.users.create({
      firstName: this.firstName,
      lastName: this.lastName,
      password: this.password,
      userName: this.userName,
      roles: this.roles
    })
    .subscribe(response => {
      this.controls.modals?.toggleLoader();
      this.controls.refresh([ 'Users' ]);
    },
    error => {
      this.controls.modals?.toggleLoader();
      this.controls.modals?.getMsgBoxResult(MsgBoxType.Ok, 'Add user error.');
    });
  }

  public firstName : string = '';

  public lastName : string = '';

  public password : string = '';

  public passwordConfirmed : string = '';

  public userName : string = '';

  public roles : string[] = [];
}

import { ChangeDetectorRef, Component, ElementRef, Input, ViewChild } from '@angular/core';
import { RolesService } from '../roles.service';

@Component({
  selector: 'app-role-builder',
  templateUrl: './role-builder.component.html',
  styleUrls: ['./role-builder.component.less']
})
export class RoleBuilderComponent {

  constructor(
    roles: RolesService,
    private ref: ChangeDetectorRef) {
      this.allRoles = roles.allRoles;
    }
    
  public allRoles : string[];

  @ViewChild('roleInput')
  public roleInput : ElementRef | undefined;

  public add(role : string) : void {
    if (this.roleInput?.nativeElement) {
      this.roleInput.nativeElement.value = '';
    }

    if (!role || !role.length) {
      return;
    }

    role = role.toUpperCase();
    
    this.options = [];

    role = this.allRoles.find(it => role == it.toUpperCase()) as string;

    if (role && !this.selection.some(it => it === role)) {
      this.selection.push(role);
    }

    this.ref.detectChanges();
  }

  public options : string[] = [];

  public remove(role : string) : void {
    
    const index = this.selection.indexOf(role);

    if (index >= 0) {
      this.selection.splice(index, 1);
      this.ref.detectChanges();
    }
  }

  @Input()
  public selection : string[] = [];

  public updateOptions(event: any) : void {
    if (event.code == 'Enter') {
      this.add(this.roleInput?.nativeElement.value);
      return;
    }

    let input = this.roleInput?.nativeElement.value as string;
    
    this.options = [];
    
    if (input) {
      input = input.replace(/\W/, '');
    }

    if (!input.length) {
      return;
    }

    let pattern = new RegExp(input, 'i');

    for (var index = 0; index < this.allRoles.length && this.options.length < 5; index++) {
      let role = this.allRoles[index];

      if (role.match(pattern)) {
        this.options.push(role);
      }
    }
  }
}

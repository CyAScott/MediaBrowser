import { ComponentFixture, TestBed } from '@angular/core/testing';

import { RoleBuilderComponent } from './role-builder.component';

describe('RoleBuilderComponent', () => {
  let component: RoleBuilderComponent;
  let fixture: ComponentFixture<RoleBuilderComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ RoleBuilderComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(RoleBuilderComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});

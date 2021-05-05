import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ThumbnailBuilderComponent } from './thumbnail-builder.component';

describe('ThumbnailBuilderComponent', () => {
  let component: ThumbnailBuilderComponent;
  let fixture: ComponentFixture<ThumbnailBuilderComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ ThumbnailBuilderComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(ThumbnailBuilderComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});

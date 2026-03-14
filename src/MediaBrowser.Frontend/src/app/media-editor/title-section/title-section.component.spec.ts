import { TestBed } from '@angular/core/testing';
import { beforeEach, describe, expect, it, vi } from 'vitest';
import { TitleSectionComponent } from './title-section.component';

describe('TitleSectionComponent', () => {
  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [TitleSectionComponent]
    }).compileComponents();
  });

  it('creates with default title data and emits on title change', () => {
    const fixture = TestBed.createComponent(TitleSectionComponent);
    const component = fixture.componentInstance;
    const emitSpy = vi.spyOn(component.titleDataChange, 'emit');

    expect(component.titleData).toEqual({
      title: '',
      originalTitle: '',
      description: ''
    });

    component.titleData = {
      title: 'Movie',
      originalTitle: 'Original',
      description: 'Desc'
    };

    component.onTitleChange();

    expect(emitSpy).toHaveBeenCalledWith(component.titleData);
    expect(emitSpy).toHaveBeenCalledTimes(1);
  });
});

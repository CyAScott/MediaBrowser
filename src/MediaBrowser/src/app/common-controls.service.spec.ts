import { TestBed } from '@angular/core/testing';

import { CommonControlsService } from './common-controls.service';

describe('CommonControlsService', () => {
  let service: CommonControlsService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(CommonControlsService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});

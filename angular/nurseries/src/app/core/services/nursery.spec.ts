import { TestBed } from '@angular/core/testing';

import { Nursery } from './nursery';

describe('Nursery', () => {
  let service: Nursery;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(Nursery);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});

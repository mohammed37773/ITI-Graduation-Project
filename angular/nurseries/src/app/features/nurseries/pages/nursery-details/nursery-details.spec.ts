import { ComponentFixture, TestBed } from '@angular/core/testing';

import { NurseryDetails } from './nursery-details';

describe('NurseryDetails', () => {
  let component: NurseryDetails;
  let fixture: ComponentFixture<NurseryDetails>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [NurseryDetails],
    }).compileComponents();

    fixture = TestBed.createComponent(NurseryDetails);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});

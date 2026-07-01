import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ManageBookings } from './manage-bookings';

describe('ManageBookings', () => {
  let component: ManageBookings;
  let fixture: ComponentFixture<ManageBookings>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ManageBookings],
    }).compileComponents();

    fixture = TestBed.createComponent(ManageBookings);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});

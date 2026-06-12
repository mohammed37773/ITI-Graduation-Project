import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ManageNurseries } from './manage-nurseries';

describe('ManageNurseries', () => {
  let component: ManageNurseries;
  let fixture: ComponentFixture<ManageNurseries>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ManageNurseries],
    }).compileComponents();

    fixture = TestBed.createComponent(ManageNurseries);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});

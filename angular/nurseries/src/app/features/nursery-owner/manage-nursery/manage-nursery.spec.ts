import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ManageNursery } from './manage-nursery';

describe('ManageNursery', () => {
  let component: ManageNursery;
  let fixture: ComponentFixture<ManageNursery>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ManageNursery],
    }).compileComponents();

    fixture = TestBed.createComponent(ManageNursery);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});

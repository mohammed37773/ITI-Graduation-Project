import { ComponentFixture, TestBed } from '@angular/core/testing';

import { NurseryOwner } from './nursery-owner';

describe('NurseryOwner', () => {
  let component: NurseryOwner;
  let fixture: ComponentFixture<NurseryOwner>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [NurseryOwner],
    }).compileComponents();

    fixture = TestBed.createComponent(NurseryOwner);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});

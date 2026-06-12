import { ComponentFixture, TestBed } from '@angular/core/testing';

import { NurseryCard } from './nursery-card';

describe('NurseryCard', () => {
  let component: NurseryCard;
  let fixture: ComponentFixture<NurseryCard>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [NurseryCard],
    }).compileComponents();

    fixture = TestBed.createComponent(NurseryCard);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});

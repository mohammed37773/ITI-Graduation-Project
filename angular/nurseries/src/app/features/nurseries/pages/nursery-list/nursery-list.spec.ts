import { ComponentFixture, TestBed } from '@angular/core/testing';

import { NurseryList } from './nursery-list';

describe('NurseryList', () => {
  let component: NurseryList;
  let fixture: ComponentFixture<NurseryList>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [NurseryList],
    }).compileComponents();

    fixture = TestBed.createComponent(NurseryList);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});

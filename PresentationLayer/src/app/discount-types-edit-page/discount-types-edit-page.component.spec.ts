import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { DiscountTypesEditPageComponent } from './discount-types-edit-page.component';

describe('DiscountTypesEditPageComponent', () => {
  let component: DiscountTypesEditPageComponent;
  let fixture: ComponentFixture<DiscountTypesEditPageComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ DiscountTypesEditPageComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(DiscountTypesEditPageComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});

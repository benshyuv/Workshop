import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { AddItemConditionalDiscountMinItemsToDiscountOnAllComponent } from './add-item-conditional-discount-min-items-to-discount-on-all.component';

describe('AddItemConditionalDiscountMinItemsToDiscountOnAllComponent', () => {
  let component: AddItemConditionalDiscountMinItemsToDiscountOnAllComponent;
  let fixture: ComponentFixture<AddItemConditionalDiscountMinItemsToDiscountOnAllComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ AddItemConditionalDiscountMinItemsToDiscountOnAllComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(AddItemConditionalDiscountMinItemsToDiscountOnAllComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});

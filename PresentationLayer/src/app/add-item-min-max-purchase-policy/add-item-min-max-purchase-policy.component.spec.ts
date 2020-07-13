import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { AddItemMinMaxPurchasePolicyComponent } from './add-item-min-max-purchase-policy.component';

describe('AddItemMinMaxPurchasePolicyComponent', () => {
  let component: AddItemMinMaxPurchasePolicyComponent;
  let fixture: ComponentFixture<AddItemMinMaxPurchasePolicyComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ AddItemMinMaxPurchasePolicyComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(AddItemMinMaxPurchasePolicyComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});

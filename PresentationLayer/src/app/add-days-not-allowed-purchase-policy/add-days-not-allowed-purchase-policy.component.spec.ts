import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { AddDaysNotAllowedPurchasePolicyComponent } from './add-days-not-allowed-purchase-policy.component';

describe('AddDaysNotAllowedPurchasePolicyComponent', () => {
  let component: AddDaysNotAllowedPurchasePolicyComponent;
  let fixture: ComponentFixture<AddDaysNotAllowedPurchasePolicyComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ AddDaysNotAllowedPurchasePolicyComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(AddDaysNotAllowedPurchasePolicyComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});

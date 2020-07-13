import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { ManagePurchasePoliciesComponent } from './manage-purchase-policies.component';

describe('ManagePurchasePoliciesComponent', () => {
  let component: ManagePurchasePoliciesComponent;
  let fixture: ComponentFixture<ManagePurchasePoliciesComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ ManagePurchasePoliciesComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(ManagePurchasePoliciesComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});

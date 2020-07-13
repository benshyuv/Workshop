import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { ApproveStoreOwnerComponent } from './approve-store-owner.component';

describe('ApproveStoreOwnerComponent', () => {
  let component: ApproveStoreOwnerComponent;
  let fixture: ComponentFixture<ApproveStoreOwnerComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ ApproveStoreOwnerComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(ApproveStoreOwnerComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});

import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { AddStoreOwnerComponent } from './add-store-owner.component';

describe('AddStoreOwnerComponent', () => {
  let component: AddStoreOwnerComponent;
  let fixture: ComponentFixture<AddStoreOwnerComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ AddStoreOwnerComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(AddStoreOwnerComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});

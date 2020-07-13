import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { RemoveStoreOwnerComponent } from './remove-store-owner.component';

describe('RemoveStoreOwnerComponent', () => {
  let component: RemoveStoreOwnerComponent;
  let fixture: ComponentFixture<RemoveStoreOwnerComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ RemoveStoreOwnerComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(RemoveStoreOwnerComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});

import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { RemoveStoreManagerComponent } from './remove-store-manager.component';

describe('RemoveStoreManagerComponent', () => {
  let component: RemoveStoreManagerComponent;
  let fixture: ComponentFixture<RemoveStoreManagerComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ RemoveStoreManagerComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(RemoveStoreManagerComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});

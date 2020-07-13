import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { AddStoreManagerComponent } from './add-store-manager.component';

describe('AddStoreManagerComponent', () => {
  let component: AddStoreManagerComponent;
  let fixture: ComponentFixture<AddStoreManagerComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ AddStoreManagerComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(AddStoreManagerComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});

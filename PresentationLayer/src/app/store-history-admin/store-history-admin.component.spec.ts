import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { StoreHistoryAdminComponent } from './store-history-admin.component';

describe('StoreHistoryAdminComponent', () => {
  let component: StoreHistoryAdminComponent;
  let fixture: ComponentFixture<StoreHistoryAdminComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ StoreHistoryAdminComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(StoreHistoryAdminComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});

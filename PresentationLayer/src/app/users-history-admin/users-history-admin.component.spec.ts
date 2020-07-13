import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { UsersHistoryAdminComponent } from './users-history-admin.component';

describe('UsersHistoryAdminComponent', () => {
  let component: UsersHistoryAdminComponent;
  let fixture: ComponentFixture<UsersHistoryAdminComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ UsersHistoryAdminComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(UsersHistoryAdminComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});

import { Component, OnInit, ChangeDetectorRef, Input, ViewChild, ElementRef,CUSTOM_ELEMENTS_SCHEMA  } from '@angular/core';
import { Router } from "@angular/router";
import { ExtendedSearchDetails } from './contracts.service';
import { HttpRequestService, JsonParsed } from './http-request.service';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css'],
  //schemas: [ CUSTOM_ELEMENTS_SCHEMA ]
})

export class AppComponent implements OnInit {
    @Input()
    User: string;
    isAdmin: boolean;

    @ViewChild('SearchItemName') SearchItemName: ElementRef;

    constructor(private router: Router, private _cdr: ChangeDetectorRef, private httpService: HttpRequestService,) {
        this.isAdmin = false;

    }

    ngOnInit(): void {
        this.User = localStorage.getItem('currentUser');
        this.httpService.get('api/user/IsAdmin').subscribe((data: JsonParsed) => {
            if (data.Success) {
                this.isAdmin = true;
            }
        });
    }

    changeStatus(): void {
        setTimeout(() => {
            this.User = undefined;
            this._cdr.detectChanges()
        }, 1000);
    }

    triggerSearchItems(searchItemName: string) {
        let details: ExtendedSearchDetails = {
            itemName: searchItemName
        };
        this.router.navigate(['/searchResults', details]);
        this.SearchItemName.nativeElement.value = '';
    }

    triggerExtendedSearchItems() {
        this.router.navigateByUrl('/extendedSearch');
        this.SearchItemName.nativeElement.value = '';
    }
}

import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from "@angular/router";
import { AddDaysNotAllowedPurchasePolicyBodyRequest } from '../contracts.service';
import { HttpRequestService, JsonParsed } from '../http-request.service';
import { FormGroup, FormControl } from '@angular/forms';

@Component({
  selector: 'app-add-days-not-allowed-purchase-policy',
  templateUrl: './add-days-not-allowed-purchase-policy.component.html',
  styleUrls: ['./add-days-not-allowed-purchase-policy.component.css']
})
export class AddDaysNotAllowedPurchasePolicyComponent implements OnInit {
    storeName: string;
    storeid: string;
    actionFailed: boolean;
    messageToUser: string;

    dropdownList = [];
    selectedItems = [];
    dropdownSettings: any = {};
    selectedDays: any[] = [];

    selectDaysNotAllowed = new FormGroup({
        Sunday: new FormControl(),
        Monday: new FormControl(),
        Tuesday: new FormControl(),
        Wednesday: new FormControl(),
        Thursday: new FormControl(),
        Friday: new FormControl(),
        Saturday: new FormControl()
    });

 
    constructor(private httpService: HttpRequestService, private router: ActivatedRoute, private routerDaysPolicy: Router)
    {
        this.storeName = "";
        this.storeid = "";
        this.actionFailed = false;
        this.messageToUser = "";
    }

    ngOnInit(): void {
        this.storeid = this.router.snapshot.paramMap.get('storeid');
        this.storeName = this.router.snapshot.paramMap.get('storeName');

        this.dropdownList = [
            { item_id: 1, item_text: 'Sunday' },
            { item_id: 2, item_text: 'Monday' },
            { item_id: 3, item_text: 'Tuesday' },
            { item_id: 4, item_text: 'Wednesday' },
            { item_id: 5, item_text: 'Thursday' },
            { item_id: 6, item_txt: 'Friday' },
            { item_id: 6, item_txt: 'Saturday' }
        ];
        this.selectedItems = [];

        this.dropdownSettings = {
            singleSelection: false,
            idField: 'item_id',
            textField: 'item_text',
            selectAllText: 'Select All',
            unSelectAllText: 'UnSelect All',
            allowSearchFilter: true
        };
    }

    onItemSelect(item: any) {
        if (!this.selectedDays.includes(item)) {

        }
        this.selectedDays.push(item);
        //console.log(item);
    }
    onSelectAll(items: any) {
        //console.log(items);
    }

    onSubmit() {
        let daysNotAllowed: number[] = [];

        if (this.selectDaysNotAllowed.get('Sunday').value) {
            daysNotAllowed.push(1);
        }

        if (this.selectDaysNotAllowed.get('Monday').value) {
            daysNotAllowed.push(2);
        }

        if (this.selectDaysNotAllowed.get('Tuesday').value) {
            daysNotAllowed.push(3);
        }

        if (this.selectDaysNotAllowed.get('Wednesday').value) {
            daysNotAllowed.push(4);
        }

        if (this.selectDaysNotAllowed.get('Thursday').value) {
            daysNotAllowed.push(5);
        }

        if (this.selectDaysNotAllowed.get('Friday').value) {
            daysNotAllowed.push(6);
        }

        if (this.selectDaysNotAllowed.get('Saturday').value) {
            daysNotAllowed.push(7);
        }

        let request: AddDaysNotAllowedPurchasePolicyBodyRequest = {
            StoreID: this.storeid,
            DaysNotAllowed: daysNotAllowed
        }

        this.httpService.post('api/store/AddDaysNotAllowedPurchasePolicy', request).subscribe((data: JsonParsed) => {
            if (data.Success) {
                window.alert('The purchase policy type has been added to store');
                this.routerDaysPolicy.navigateByUrl('/RefreshComponent', { skipLocationChange: true }).then(() => {
                    this.routerDaysPolicy.navigate(['/home/' + this.storeid + '/storeManagment/purchasesManagment', { 'storeName': this.storeName }]);
                });
            } else {
                this.actionFailed = true;
                let response = JSON.parse(data.AnswerOrExceptionJson);
                if (response) {
                    this.messageToUser = response.Message;
                }
            }
        });
    }
}

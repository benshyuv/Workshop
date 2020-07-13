import { Component, OnInit } from '@angular/core';
import { Router, ActivatedRoute } from "@angular/router";
import { HttpRequestService, JsonParsed } from '../http-request.service';
import { FormGroup, FormControl, Validators } from '@angular/forms';
import { MyStore, Item, StoreInventory, MakeDiscountNotAllowedBodyRequest, MakeDiscountAllowedBodyRequest } from '../contracts.service';
@Component({
  selector: 'app-discount-types-edit-page',
  templateUrl: './discount-types-edit-page.component.html',
  styleUrls: ['./discount-types-edit-page.component.css']
})
export class DiscountTypesEditPageComponent implements OnInit {
    discountTypesAllowed: string[];
    storeName: string;
    storeid: string;
    actionFailed: boolean;
    messageToUser: string;
    allTypesOfDiscount: string[] = [
        "OPENED", "ITEM_CONDITIONAL", "STORE_CONDITIONAL", "COMPOSITE"
    ];

    possibleTypesToAdd: string[];

    formDeleteType = new FormGroup({
        formDeleteType: new FormControl('', Validators.required),
    });

    formAddType = new FormGroup({
        formAddType: new FormControl('', Validators.required),
    });

    constructor(private httpService: HttpRequestService, private router: ActivatedRoute,
        private routerDiscountsTypes: Router) {
        this.discountTypesAllowed = [];
        this.storeName = "";
        this.storeid = "";
        this.actionFailed = false;
        this.messageToUser = "";
        this.possibleTypesToAdd = [];
    }

    async ngOnInit(): Promise<void> {
        this.possibleTypesToAdd = [];
        this.storeid = this.router.snapshot.paramMap.get('storeid');
        this.storeName = this.router.snapshot.paramMap.get('storeName');

        let params: Map<string, string> = new Map();
        params.set("storeid", this.storeid);
        let response: JsonParsed = await this.httpService.getWithParamsAsync('api/store/GetAllowedDiscounts', params);
        if (response.Success) {
            this.discountTypesAllowed = JSON.parse(response.AnswerOrExceptionJson);
            this.allTypesOfDiscount.forEach(type => {
                if (!this.discountTypesAllowed.includes(type)) {
                    this.possibleTypesToAdd.push(type);
                }
            });
        }
        else {
            let error = JSON.parse(response.AnswerOrExceptionJson);
            if (error) {
                this.actionFailed = true;
                this.messageToUser = error.Message;
                return;
            }
        }
    }

    deleteDiscountType() {
        let deleteTypeRequest: MakeDiscountNotAllowedBodyRequest = {
            StoreID: this.storeid,
            DiscountTypeString: this.formDeleteType.get('formDeleteType').value
        };

        this.httpService.post('api/store/MakeDiscountNotAllowed', deleteTypeRequest).subscribe((data: JsonParsed) => {
            if (data.Success) {
                window.alert('The discount type has been removed from store');
                this.routerDiscountsTypes.navigateByUrl('/RefreshComponent', { skipLocationChange: true }).then(() => {
                    this.routerDiscountsTypes.navigate(['/home/' + this.storeid + '/storeManagment/discountsManagement', { 'storeName': this.storeName }]);
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

    addDiscountType() {
        let addTypeRequest: MakeDiscountAllowedBodyRequest = {
            StoreID: this.storeid,
            DiscountTypeString: this.formAddType.get('formAddType').value
        };

        this.httpService.post('api/store/MakeDiscountAllowed', addTypeRequest).subscribe((data: JsonParsed) => {
            if (data.Success) {
                window.alert('The discount type has been added to store');
                this.routerDiscountsTypes.navigateByUrl('/RefreshComponent', { skipLocationChange: true }).then(() => {
                    this.routerDiscountsTypes.navigate(['/home/' + this.storeid + '/storeManagment/discountsManagement', { 'storeName': this.storeName }]);
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

    backToDiscountManagement() {
        this.routerDiscountsTypes.navigate(['/home/' + this.storeid + '/storeManagment/discountsManagement', { 'storeName': this.storeName }]);
    }
}

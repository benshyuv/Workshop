import { Component, OnInit } from '@angular/core';
import { Router, ActivatedRoute } from "@angular/router";
import { HttpRequestService, JsonParsed } from '../http-request.service';
import { FormGroup, FormControl, Validators } from '@angular/forms';
import { MakePurchasePolicyNotAllowedBodyRequest, MakePurchasePolicyAllowedBodyRequest } from '../contracts.service';

@Component({
  selector: 'app-manage-types-of-purchase-policy',
  templateUrl: './manage-types-of-purchase-policy.component.html',
  styleUrls: ['./manage-types-of-purchase-policy.component.css']
})
export class ManageTypesOfPurchasePolicyComponent implements OnInit {
    purchasePolicyTypesAllowed: string[];
    storeName: string;
    storeid: string;
    actionFailed: boolean;
    messageToUser: string;
    allTypesOfPurchasePolicy: string[] = [
        "ITEM", "STORE", "DAYS", "COMPOSITE"
    ];

    possibleTypesToAdd: string[];

    formDeleteType = new FormGroup({
        formDeleteType: new FormControl('', Validators.required),
    });

    formAddType = new FormGroup({
        formAddType: new FormControl('', Validators.required),
    });


    constructor(private httpService: HttpRequestService, private router: ActivatedRoute,
        private routerPurchasePolicyTypes: Router)
    {
        this.purchasePolicyTypesAllowed = [];
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
        let response: JsonParsed = await this.httpService.getWithParamsAsync('api/store/GetAllowedPurchasePolicys', params);
        if (response.Success) {
            this.purchasePolicyTypesAllowed = JSON.parse(response.AnswerOrExceptionJson);
            this.allTypesOfPurchasePolicy.forEach(type => {
                if (!this.purchasePolicyTypesAllowed.includes(type)) {
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

    deletePurchasePolicyType() {
        let deleteTypeRequest: MakePurchasePolicyNotAllowedBodyRequest = {
            StoreID: this.storeid,
            PurchasePolicy: this.formDeleteType.get('formDeleteType').value
        };

        this.httpService.post('api/store/MakePurcahsePolicyNotAllowed', deleteTypeRequest).subscribe((data: JsonParsed) => {
            if (data.Success) {
                window.alert('The purchase policy type has been removed from store');
                this.routerPurchasePolicyTypes.navigateByUrl('/RefreshComponent', { skipLocationChange: true }).then(() => {
                    this.routerPurchasePolicyTypes.navigate(['/home/' + this.storeid + '/storeManagment/purchasesManagment', { 'storeName': this.storeName }]);
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

    addPurchasePolicyType() {
        let addTypeRequest: MakePurchasePolicyAllowedBodyRequest = {
            StoreID: this.storeid,
            PurchasePolicy: this.formAddType.get('formAddType').value
        };

        this.httpService.post('api/store/MakePurcahsePolicyAllowed', addTypeRequest).subscribe((data: JsonParsed) => {
            if (data.Success) {
                window.alert('The purchase policy type has been added to store');
                this.routerPurchasePolicyTypes.navigateByUrl('/RefreshComponent', { skipLocationChange: true }).then(() => {
                    this.routerPurchasePolicyTypes.navigate(['/home/' + this.storeid + '/storeManagment/purchasesManagment', { 'storeName': this.storeName }]);
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

    backToManagePurchases() {
        this.routerPurchasePolicyTypes.navigate(['/home/' + this.storeid + '/storeManagment/purchasesManagment', { 'storeName': this.storeName }]);
    }

}

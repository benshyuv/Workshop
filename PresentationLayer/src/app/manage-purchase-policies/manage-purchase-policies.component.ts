import { Component, OnInit } from '@angular/core';
import { Router, ActivatedRoute } from "@angular/router";
import { PurchasePolicy, RemovePurchasePolicyBodyRequest, ComposeTwoPurchasePoliciesBodyRequest } from '../contracts.service';
import { HttpRequestService, JsonParsed } from '../http-request.service';
import { FormGroup, FormControl, Validators } from '@angular/forms';

@Component({
  selector: 'app-manage-purchase-policies',
  templateUrl: './manage-purchase-policies.component.html',
  styleUrls: ['./manage-purchase-policies.component.css']
})
export class ManagePurchasePoliciesComponent implements OnInit {
    storeName: string;
    storeid: string;
    purchasePolicies: PurchasePolicy[];
    actionFailed1: boolean;
    actionFailed2: boolean;
    actionFailed0: boolean;
    messageToUser: string;
    operatorsForComopsite = ["&", "|", "xor"];
    formDelete = new FormGroup({
        formDelete: new FormControl('', Validators.required)
    });

    formPurchasePolicyComposite = new FormGroup({
        formPurchasePolicyLeft: new FormControl('', Validators.required),
        formPurchasePolicyRight: new FormControl('', Validators.required),
        formOperator: new FormControl('', Validators.required),
    });

    constructor(private httpService: HttpRequestService, private router: ActivatedRoute, private routerPurchasePolicies: Router) {
        this.storeName = "";
        this.storeid = "";
        this.actionFailed0 = false;
        this.actionFailed1 = false;
        this.actionFailed2 = false;
        this.messageToUser = "";
        this.purchasePolicies = [];
    }

    async ngOnInit(): Promise<void> {
        this.storeid = this.router.snapshot.paramMap.get('storeid');
        this.storeName = this.router.snapshot.paramMap.get('storeName');
        let params: Map<string, string> = new Map();
        params.set("storeid", this.storeid);

        let dataPurchasePolicies: JsonParsed = await this.httpService.getWithParamsAsync('api/store/GetAllPurchasePolicys', params);
        if (dataPurchasePolicies.Success) {
            this.purchasePolicies = JSON.parse(dataPurchasePolicies.AnswerOrExceptionJson);

        }
        else {
            let response = JSON.parse(dataPurchasePolicies.AnswerOrExceptionJson);
            if (response) {
                this.actionFailed0 = true;
                this.messageToUser = response.Message;
            }
        }
    }

    AddItemMinMaxPurchasePolicy() {
        this.routerPurchasePolicies.navigate(['/home/' + this.storeid + '/storeManagment/AddItemMinMaxPurchasePolicy', { 'storeName': this.storeName }]);
    }

    AddStoreMinMaxPurchasePolicy() {
        this.routerPurchasePolicies.navigate(['/home/' + this.storeid + '/storeManagment/AddStoreMinMaxPurchasePolicy', { 'storeName': this.storeName }]);
    }

    AddDaysNotAllowedPurchasePolicy() {
        this.routerPurchasePolicies.navigate(['/home/' + this.storeid + '/storeManagment/AddDaysNotAllowedPurchasePolicy', { 'storeName': this.storeName }]);
    }

    ComposeTwoPurchasePolicies() {
        let compositeRequest: ComposeTwoPurchasePoliciesBodyRequest = {
            StoreID: this.storeid,
            PolicyLeftID: this.formPurchasePolicyComposite.get('formPurchasePolicyLeft').value.PolicyID,
            PolicyRightID: this.formPurchasePolicyComposite.get('formPurchasePolicyRight').value.PolicyID,
            BoolOperator: this.formPurchasePolicyComposite.get('formOperator').value
        };

        this.httpService.post('api/store/ComposeTwoPurchasePolicies', compositeRequest).subscribe((data: JsonParsed) => {
            if (data.Success) {
                window.alert('The purchase policies have been composed');
                this.routerPurchasePolicies.navigateByUrl('/RefreshComponent', { skipLocationChange: true }).then(() =>
                    this.routerPurchasePolicies.navigate(['/home/' + this.storeid + '/storeManagment/purchasesManagment', { 'storeName': this.storeName }]));
            } else {
                this.actionFailed1 = true;
                let response = JSON.parse(data.AnswerOrExceptionJson);
                if (response) {
                    this.messageToUser = response.Message;
                }
            }
        });
    }

    DeletePurchasePolicy() {
        let deleteRequest: RemovePurchasePolicyBodyRequest = {
            StoreID: this.storeid,
            PolicyID: this.formDelete.get('formDelete').value.PolicyID
        };

        this.httpService.post('api/store/RemovePurchasePolicy', deleteRequest).subscribe((data: JsonParsed) => {
            if (data.Success) {
                window.alert('The purchase policy has been removed from store');
                this.routerPurchasePolicies.navigateByUrl('/RefreshComponent', { skipLocationChange: true }).then(() =>
                    this.routerPurchasePolicies.navigate(['/home/' + this.storeid + '/storeManagment/purchasesManagment', { 'storeName': this.storeName }]));
            } else {
                this.actionFailed2 = true;
                let response = JSON.parse(data.AnswerOrExceptionJson);
                if (response) {
                    this.messageToUser = response.Message;
                }
            }
        });
    }

    backToManagePurchases() {
        this.routerPurchasePolicies.navigate(['/home/' + this.storeid + '/storeManagment/purchasesManagment', { 'storeName': this.storeName }]);
    }

}

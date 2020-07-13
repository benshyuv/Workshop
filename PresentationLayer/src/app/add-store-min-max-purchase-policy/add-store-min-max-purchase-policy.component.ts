import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from "@angular/router";
import { AddStoreMinMaxPurchasePolicyBodyRequest } from '../contracts.service';
import { HttpRequestService, JsonParsed } from '../http-request.service';

@Component({
  selector: 'app-add-store-min-max-purchase-policy',
  templateUrl: './add-store-min-max-purchase-policy.component.html',
  styleUrls: ['./add-store-min-max-purchase-policy.component.css']
})
export class AddStoreMinMaxPurchasePolicyComponent implements OnInit {
    storeName: string;
    storeid: string;
    actionFailed: boolean;
    messageToUser: string;

    constructor(private httpService: HttpRequestService, private router: ActivatedRoute, private routerAddStoreMinMaxPolicy: Router) {
        this.storeName = "";
        this.storeid = "";
        this.actionFailed = false;
        this.messageToUser = "";
    }

    ngOnInit(): void {
        this.storeid = this.router.snapshot.paramMap.get('storeid');
        this.storeName = this.router.snapshot.paramMap.get('storeName');
  }

    onSubmit(addStoreMinMaxPolicyDetails) {
        let request: AddStoreMinMaxPurchasePolicyBodyRequest = {
            StoreID: this.storeid,
            MinAmount: addStoreMinMaxPolicyDetails.minAmount != "" ? addStoreMinMaxPolicyDetails.minAmount : undefined,
            MaxAmount: addStoreMinMaxPolicyDetails.maxAmount != "" ? addStoreMinMaxPolicyDetails.maxAmount : undefined
        };

        this.httpService.post('api/store/AddStoreMinMaxPurchasePolicy', request).subscribe((data: JsonParsed) => {
            if (data.Success) {
                window.alert('The purchase policy has been added to the store');
                this.routerAddStoreMinMaxPolicy.navigateByUrl('/RefreshComponent', { skipLocationChange: true }).then(() =>
                    this.routerAddStoreMinMaxPolicy.navigate(['/home/' + this.storeid + '/storeManagment/purchasesManagment', { 'storeName': this.storeName }]));
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

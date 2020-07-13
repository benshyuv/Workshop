import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from "@angular/router";
import { MyStore, Item, StoreInventory, AddItemMinMaxPurchasePolicyBodyRequest } from '../contracts.service';
import { HttpRequestService, JsonParsed } from '../http-request.service';

@Component({
  selector: 'app-add-item-min-max-purchase-policy',
  templateUrl: './add-item-min-max-purchase-policy.component.html',
  styleUrls: ['./add-item-min-max-purchase-policy.component.css']
})
export class AddItemMinMaxPurchasePolicyComponent implements OnInit {
    storeName: string;
    storeid: string;
    store: MyStore;
    items: Item[];
    storeInventory: StoreInventory;
    selectedItemId: string;
    actionFailed: boolean;
    messageToUser: string;

    constructor(private httpService: HttpRequestService, private router: ActivatedRoute, private routerAddItemMinMaxPolicy: Router) {
        this.storeName = "";
        this.storeid = "";
        this.selectedItemId = "";
        this.actionFailed = false;
        this.messageToUser = "";
        this.items = [];
    }

    async ngOnInit(): Promise<void> {
        this.storeid = this.router.snapshot.paramMap.get('storeid');
        this.storeName = this.router.snapshot.paramMap.get('storeName');
        let params: Map<string, string> = new Map();
        params.set("storeid", this.storeid);

        let dataStore: JsonParsed = await this.httpService.getWithParamsAsync('api/store/GetStoreInformationByID', params);
        if (dataStore.Success) {
            this.store = JSON.parse(dataStore.AnswerOrExceptionJson);
            this.storeName = this.store.ContactDetails.Name;
            this.storeInventory = this.store.storeInventory;
            for (let key in this.storeInventory.Items) {
                this.items.push(this.storeInventory.Items[key]);
            }
        }
        else {
            let response = JSON.parse(dataStore.AnswerOrExceptionJson);
            if (response) {
                this.actionFailed = true;
                this.messageToUser = response.Message;
            }
        }
    }

    onSubmit(addItemMinMaxPolicyDetails) {
        let request: AddItemMinMaxPurchasePolicyBodyRequest = {
            StoreID: this.storeid,
            ItemID: this.selectedItemId,
            MinAmount: addItemMinMaxPolicyDetails.minAmount != "" ? addItemMinMaxPolicyDetails.minAmount : undefined,
            MaxAmount: addItemMinMaxPolicyDetails.maxAmount != "" ? addItemMinMaxPolicyDetails.maxAmount : undefined
        };

        this.httpService.post('api/store/AddItemMinMaxPurchasePolicy', request).subscribe((data: JsonParsed) => {
            if (data.Success) {
                window.alert('The purchase policy has been added to the store');
                this.routerAddItemMinMaxPolicy.navigateByUrl('/RefreshComponent', { skipLocationChange: true }).then(() =>
                    this.routerAddItemMinMaxPolicy.navigate(['/home/' + this.storeid + '/storeManagment/purchasesManagment', { 'storeName': this.storeName }]));
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

import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router} from "@angular/router";
import { MyStore, Item, StoreInventory, AddOpenDiscountBodyRequest } from '../contracts.service';
import { HttpRequestService, JsonParsed } from '../http-request.service';

@Component({
  selector: 'app-add-open-discount',
  templateUrl: './add-open-discount.component.html',
  styleUrls: ['./add-open-discount.component.css']
})
export class AddOpenDiscountComponent implements OnInit {
    storeName: string;
    storeid: string;
    store: MyStore;
    items: Item[];
    storeInventory: StoreInventory;
    selectedItemId: string;
    actionFailed: boolean;
    messageToUser: string;

    constructor(private httpService: HttpRequestService, private router: ActivatedRoute, private routerAddOpenDiscount: Router) {
        this.storeName = "";
        this.storeid = "";
        this.selectedItemId = "";
        this.actionFailed = false;
        this.messageToUser = "";
        this.items = [];
    }

    async ngOnInit(): Promise<void> {
        this.storeid = this.router.snapshot.paramMap.get('storeid');
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

    onSubmit(addOpenDiscountDetails) {
        let request: AddOpenDiscountBodyRequest = {
            StoreID: this.storeid,
            ItemID: this.selectedItemId,
            Discount: addOpenDiscountDetails.discountPrecent,
            DurationInDays: addOpenDiscountDetails.durationInDays
        }

        this.httpService.post('api/store/AddOpenDiscount', request).subscribe((data: JsonParsed) => {
            if (data.Success) {
                window.alert('The discount has been added to the store');
                this.routerAddOpenDiscount.navigateByUrl('/RefreshComponent', { skipLocationChange: true }).then(() =>
                    this.routerAddOpenDiscount.navigate(['/home/' + this.storeid + '/storeManagment/discountsManagement', { 'storeName': this.storeName }]));
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

import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from "@angular/router";
import { MyStore, Item, StoreInventory, AddStoreConditionalDiscountBodyRequest } from '../contracts.service';
import { HttpRequestService, JsonParsed } from '../http-request.service';

@Component({
  selector: 'app-add-store-conditional-discount',
  templateUrl: './add-store-conditional-discount.component.html',
  styleUrls: ['./add-store-conditional-discount.component.css']
})
export class AddStoreConditionalDiscountComponent implements OnInit {
    storeName: string;
    storeid: string;
    actionFailed: boolean;
    messageToUser: string;

    constructor(private httpService: HttpRequestService, private router: ActivatedRoute, private routerAddOpenDiscount: Router) {
        this.storeName = "";
        this.storeid = "";
        this.actionFailed = false;
        this.messageToUser = "";
    }

    async ngOnInit(): Promise<void> {
        this.storeid = this.router.snapshot.paramMap.get('storeid');
        this.storeName = this.router.snapshot.paramMap.get('storeName');
    }

    onSubmit(costumerData) {
        let request: AddStoreConditionalDiscountBodyRequest = {
            StoreID: this.storeid,
            DurationInDays: costumerData.durationInDays,
            MinPurchase: costumerData.minPurchase,
            Discount: costumerData.discount
        }

        this.httpService.post('api/store/AddStoreConditionalDiscount', request).subscribe((data: JsonParsed) => {
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

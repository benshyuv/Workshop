import { Component, OnInit } from '@angular/core';
import { HttpRequestService, JsonParsed } from '../http-request.service';
import { ActivatedRoute, Router } from "@angular/router";
import { MyPair, MyStore ,Item,AddToCartBodyRequest} from '../contracts.service';
import { Location } from '@angular/common';

@Component({
  selector: 'app-edit-item-amount-in-cart',
  templateUrl: './edit-item-amount-in-cart.component.html',
  styleUrls: ['./edit-item-amount-in-cart.component.css']
})
export class EditItemAmountInCartComponent implements OnInit {

    updateAmountFailed: boolean;
    MessageToUser: string;
    itemId: string;
    storeId: string;
    item: Item;

    constructor(private router: ActivatedRoute,
        private httpService: HttpRequestService,
        private _location: Location
    ) {
        this.updateAmountFailed = false;
        this.MessageToUser = '';
    }

    ngOnInit(): void {
        this.storeId = this.router.snapshot.paramMap.get('storeId');
        this.itemId = this.router.snapshot.paramMap.get('itemId');
        this.httpService.getWithParams('api/store/GetStoreInformationByID', this.storeId).subscribe((data: JsonParsed) => {
            if (data.Success) {
                let store: MyStore = JSON.parse(data.AnswerOrExceptionJson);
                let storeInventory = store.storeInventory;
                for (let key in storeInventory.Items) {
                    if (storeInventory.Items[key].Id == this.itemId) {
                        this.item = storeInventory.Items[key];
                    }
                }
            } else {
                this.updateAmountFailed = true;
                let response = JSON.parse(data.AnswerOrExceptionJson);
                if (response) {
                    this.MessageToUser = response.Message;
                }
            }
        });
    }

    UpdateAmount(Amount) {
        let dataToSend: AddToCartBodyRequest = {
           storeId: this.storeId,
           itemId: this.itemId,
           amount: Amount.amount
        }
        this.httpService.post('api/user/ChangeItemAmountInCart',dataToSend).subscribe((data: JsonParsed) => {
            if (data.Success) {
                window.alert('The item amount has been changed!');
                this._location.back();
            } else {
                this.updateAmountFailed = true;
                let response = JSON.parse(data.AnswerOrExceptionJson);
                if (response) {
                    this.MessageToUser = 'not enought storage of this item';
                }
            }
        });
}
}

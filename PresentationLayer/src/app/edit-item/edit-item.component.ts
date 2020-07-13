import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from "@angular/router";
import { HttpRequestService, JsonParsed } from '../http-request.service';
import { MyStore, EditItemBodyRequest, Item } from '../contracts.service';
import { Location } from '@angular/common';

@Component({
  selector: 'app-edit-item',
  templateUrl: './edit-item.component.html',
  styleUrls: ['./edit-item.component.css']
})
export class EditItemComponent implements OnInit {
    editItemFailed: boolean;
    MessageToUser: string;
    storeid: string;
    itemid: string;
    StoreName: string;
    item: Item;

    constructor(
        private router: ActivatedRoute,
        private httpService: HttpRequestService,
        private _location: Location
    ) {
        this.editItemFailed = false;
        this.MessageToUser = '';
    }

    ngOnInit(): void {
        this.storeid = this.router.snapshot.paramMap.get('storeid');
        this.itemid = this.router.snapshot.paramMap.get('itemid');
        this.httpService.getWithParams('api/store/GetStoreInformationByID', this.storeid).subscribe((data: JsonParsed) => {
            if (data.Success) {
                let store: MyStore = JSON.parse(data.AnswerOrExceptionJson);
                this.StoreName = store.ContactDetails.Name;
                let storeInventory = store.storeInventory;
                for (let key in storeInventory.Items) {
                    if (storeInventory.Items[key].Id == this.itemid) {
                        this.item = storeInventory.Items[key];
                    }
                }
            } else {
                this.editItemFailed = true;
                let response = JSON.parse(data.AnswerOrExceptionJson);
                if (response) {
                    this.MessageToUser = response.Message;
                }
            }
        });
    }

    onSubmit(itemData) {
        let editItemRequest: EditItemBodyRequest =
        {
            StoreID: this.storeid,
            ItemID: this.itemid,
            Amount: itemData.amount != "" ? itemData.amount : this.item.Amount,
            Rank: itemData.rank != "" ? itemData.rank : this.item.Rank,
            Price: itemData.price != "" ? itemData.price : this.item.Price,
            Name: itemData.itemname != "" ? itemData.itemname : undefined,
            Categories: itemData.categories != "" ? JSON.stringify(itemData.categories.split(',').map(function (category) {
                return category.trim();
                }))
                :undefined,
            Keywords: itemData.keywords != "" ? JSON.stringify(itemData.keywords.split(',').map(function (keyword) {
                return keyword.trim();
                }))
                :undefined
        }

        this.httpService.post('api/store/EditItem', editItemRequest).subscribe((data: JsonParsed) => {
            if (data.Success) {
                this._location.back();
            } else {
                this.editItemFailed = true;
                let response = JSON.parse(data.AnswerOrExceptionJson);
                if (response) {
                    this.MessageToUser = response.Message;
                }
            }
        });
    }

}

import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from "@angular/router";
import { StoreInventory, Item, MyStore, AddToCartBodyRequest, DeleteItemBodyRequest/*, RateItemBodyRequest*/ } from '../contracts.service';
import { HttpRequestService, JsonParsed } from '../http-request.service';
import { Location } from '@angular/common';
//import { FormGroup, FormControl, Validators } from '@angular/forms';

@Component({
  selector: 'app-item-page',
  templateUrl: './item-page.component.html',
  styleUrls: ['./item-page.component.css']
})
export class ItemPageComponent implements OnInit {
    /*form = new FormGroup({
        rating: new FormControl('', Validators.required),
    });*/
    store: MyStore;
    item: Item;
    storeInventory: StoreInventory;
    storeid: string;
    itemid: string;
    actionFailed: boolean;
    MessageToUser: string;
    inventoryPermission: boolean;

    constructor(
        private router: ActivatedRoute,
        private httpService: HttpRequestService,
        private _location: Location,
        private routerItem: Router,
    )
    {
        this.actionFailed = false;
        this.MessageToUser = '';
        this.inventoryPermission = false;
    }

    async ngOnInit() {
        this.storeid = this.router.snapshot.paramMap.get('storeid');
        this.itemid = this.router.snapshot.paramMap.get('itemid');
        let data: JsonParsed = await this.httpService.getAsync('api/store/GetStoresWithPermissions');
        if (data.Success) {
            var storesWithPermissionsTuples = JSON.parse(data.AnswerOrExceptionJson);
            storesWithPermissionsTuples.forEach((element, index, array) => {
                if (element.Item2.includes("INVENTORY")) {
                    if (this.storeid == element.Item1.Id) {
                        this.inventoryPermission = true;
                    }
                }
            });
        }

        let storeInfoByIdResponse: JsonParsed = await this.httpService.getWithParamstAsync('api/store/GetStoreInformationByID', this.storeid);
        if (storeInfoByIdResponse.Success) {
            this.store = JSON.parse(storeInfoByIdResponse.AnswerOrExceptionJson);
            this.storeInventory = this.store.storeInventory;
            for (let key in this.storeInventory.Items) {
                if (this.itemid == this.storeInventory.Items[key].Id) {
                    this.item = this.storeInventory.Items[key];
                }
            }
        } else {
            this.actionFailed = true;
            let response = JSON.parse(storeInfoByIdResponse.AnswerOrExceptionJson);
            if (response) {
                this.MessageToUser = response.Message;
            }
        }
    }

    addToCartMethod(customerData) {
        if (customerData.amount == undefined || customerData.amount === "" || customerData.amount === 0) {
            this.actionFailed = true;
            this.MessageToUser = "Invalid amount";
            return;
        }

        let dataToSend: AddToCartBodyRequest = {
            storeId: this.store.Id,
            itemId: this.item.Id,
            amount: customerData.amount
        };

        this.httpService.post('api/user/AddToCart', dataToSend).subscribe((data: JsonParsed) => {
            if (data.Success) {
                window.alert('The item has been added to cart!');
                this._location.back();
            } else {
                this.actionFailed = true;
                let response = JSON.parse(data.AnswerOrExceptionJson);
                if (response) {
                    this.MessageToUser = response.Message;
                }
            }
        });
    }

    /*rate() {
        let rateItemRequest: RateItemBodyRequest =
        {
            StoreId: this.storeid,
            ItemId: this.itemid,
            Rank: this.form.value["rating"],
        }

        this.httpService.post('api/store/Rankitem', rateItemRequest).subscribe((data: JsonParsed) => {
            if (data.Success) {
                window.location.reload();
            } else {
                this.actionFailed = true;
                let response = JSON.parse(data.AnswerOrExceptionJson);
                if (response) {
                    this.MessageToUser = response.Message;
                }
            }
        });
        this.form.reset();
    }*/

    deleteItem() {
        let dataToSend: DeleteItemBodyRequest = {
            storeId: this.store.Id,
            itemId: this.item.Id
        };
        this.httpService.post('api/store/DeleteItem', dataToSend).subscribe((data: JsonParsed) => {
            if (data.Success) {
                window.alert('The item has been deleted!');
                this._location.back();
            } else {
                this.actionFailed = true;
                let response = JSON.parse(data.AnswerOrExceptionJson);
                if (response) {
                    this.MessageToUser = response.Message;
                }
            }
        });
    }

    editItem() {
        this.routerItem.navigateByUrl('/home/' + this.store.Id + '/' + this.itemid + '/editItem');
    }

    backToStore() {
        this.routerItem.navigateByUrl('/home/' + this.storeid);
    }

}

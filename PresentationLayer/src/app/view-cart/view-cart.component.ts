import { Component, OnInit } from '@angular/core';
import { HttpRequestService, JsonParsed } from '../http-request.service';
import { MyPair, MyStore ,StoreItem} from '../contracts.service';
import { ActivatedRoute, Router } from "@angular/router";
import { Location } from '@angular/common';

@Component({
  selector: 'app-view-cart',
  templateUrl: './view-cart.component.html',
  styleUrls: ['./view-cart.component.css']
})
export class ViewCartComponent implements OnInit {
    itemsInCart;
    response: string;
    viewCartFaild: boolean;
    RemoveItemFromCartFailed: boolean;
    storeId: string;
    itemId: string;
    UpdateItemAmountInCartFailed: boolean;
    MessageToUser: string;
    itemsToPresent;
    Object = Object;
    total;

      constructor(private router: Router,
        private httpService: HttpRequestService,
        private _location: Location )  {
        this.viewCartFaild = false;
        this.RemoveItemFromCartFailed=false;
        this.MessageToUser = '';
        this.itemsToPresent = {};
        this.UpdateItemAmountInCartFailed=false;
        this.itemId='empty';
        this.storeId='empty';
        this.total = 0;
    }

    ngOnInit(): void {
        this.httpService.get('api/user/ViewCart').subscribe((data: JsonParsed) => {
            if (data.Success) {
                this.itemsInCart = JSON.parse(data.AnswerOrExceptionJson);
                for (var key in this.itemsInCart) {
                    this.httpService.getWithParams('api/store/GetStoreInformationByID', key).subscribe((data: JsonParsed) => {
                        if (data.Success) {
                            let store: MyStore = JSON.parse(data.AnswerOrExceptionJson);
                            let name = store.ContactDetails.Name;
                            this.itemsToPresent[name] = [];
                            this.itemsInCart[store.Id].forEach((element, index, array) => {
                                let pair: MyPair = {
                                    item: element.Item1,
                                    amount: element.Item2,
                                    storeId: store.Id
                                }
                                this.total += element.Item1.Price * element.Item2;
                                this.itemsToPresent[name].push(pair);
                            });
                            
                        } else {
                            this.viewCartFaild = true;
                            let response = JSON.parse(data.AnswerOrExceptionJson);
                            if (response) {
                                this.MessageToUser = response.Message;
                            }
                        }
                    });
                }
                
            } else {
                this.viewCartFaild = true;
                let response = JSON.parse(data.AnswerOrExceptionJson);
                if (response) {
                    this.MessageToUser = response.Message;
                }
            }
        });
     
    }
RemoveItemFromCart(storeID,itemId)
{

        let ItemToRemove: StoreItem = {
            StoreID:storeID,
            ItemID: itemId
        };
            this.httpService.post('api/user/RemoveFromCart', ItemToRemove).subscribe((data: JsonParsed) => {
            if (data.Success) {
               window.location.reload();
            } else {
                this.RemoveItemFromCartFailed = true;
                let response = JSON.parse(data.AnswerOrExceptionJson);
                if (response) {
                    this.MessageToUser = response.Message;
                }
            }
        });
}
 
 editItemAmount(storeId,itemId) {
        this.itemId = itemId;
        this.storeId = storeId;
        this.router.navigate(['/viewCart/' + this.itemId + '/EditItemAmountInCart', { 'storeId': this.storeId }]);
    }

    checkout() {
        this.router.navigateByUrl('/displayOrder');
    }
}

import { Component, OnInit } from '@angular/core';
import { Router } from "@angular/router";
import { HttpRequestService, JsonParsed } from '../http-request.service';
import { Order, OrderItem, MyStore, StoreAndItems } from '../contracts.service';

@Component({
  selector: 'app-display-order',
  templateUrl: './display-order.component.html',
  styleUrls: ['./display-order.component.css']
})
export class DisplayOrderComponent implements OnInit {
    checkoutFailed: boolean;
    MessageToUser: string;
    order: Order;
    storesAndItems: StoreAndItems[];
    Object = Object;

    constructor(private router: Router,
        private httpService: HttpRequestService, )
    {
        this.checkoutFailed = false;
        this.MessageToUser = '';
    }

    async ngOnInit(){
        this.storesAndItems = [];
        let data: JsonParsed = await this.httpService.getAsync('api/purchase/DisplayBeforeCheckout');
        if (data.Success) {
            this.order = JSON.parse(data.AnswerOrExceptionJson);
            for (let session of Object.keys(this.order.StoreOrders)) {
                let storeOrder = this.order.StoreOrders[session];
                let storeInfoByIdResponse: JsonParsed = await this.httpService.getWithParamstAsync('api/store/GetStoreInformationByID', storeOrder.StoreId);
                if (storeInfoByIdResponse.Success) {
                    let store: MyStore = JSON.parse(storeInfoByIdResponse.AnswerOrExceptionJson);
                    let storeName = store.ContactDetails.Name;
                    let storeItems: OrderItem[] = storeOrder.StoreOrderItems;
                    this.storesAndItems.push({ orderId: this.order.OrderId, storeName: storeName, items: storeItems });
                } else {
                    this.checkoutFailed = true;
                    let response = JSON.parse(storeInfoByIdResponse.AnswerOrExceptionJson);
                    if (response) {
                        this.MessageToUser = response.Message;
                    }
                }
            }
        } else {
            this.checkoutFailed = true;
            let response = JSON.parse(data.AnswerOrExceptionJson);
            if (response) {
                this.MessageToUser = response.Message;
            }
        }
    }

    onApprove() {
        this.router.navigate(['/checkout', { 'orderId': this.order.OrderId }]);
    }
}

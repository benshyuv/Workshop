import { Component, OnInit } from '@angular/core';
import { MyStore, Order, StoreAndItems, OrderItem } from '../contracts.service';
import { HttpRequestService, JsonParsed } from '../http-request.service';

@Component({
    selector: 'app-user-history',
    templateUrl: './user-history.component.html',
    styleUrls: ['./user-history.component.css']
})
export class UserHistoryComponent implements OnInit {
    actionFailed: boolean;
    MessageToUser: string;
    Orders: Order[];
    storesAndItems: StoreAndItems[];
    Object = Object;
    selectedOrderId: string;
    data: JsonParsed;
    order: Order;
    showTotal: boolean;

    constructor(
        private httpService: HttpRequestService, ) {
        this.Orders = [];
        this.data;
        this.actionFailed = false;
        this.MessageToUser = '';
        this.selectedOrderId = 'empty';
        this.storesAndItems = [];
        this.showTotal = false;
    }

    async ngOnInit() {
        this.Orders = [];
        this.data = await this.httpService.getAsync('api/user/GetMyOrderHistory');
        if (this.data.Success) {
            this.Orders = JSON.parse(this.data.AnswerOrExceptionJson);
        }
        else {
            this.actionFailed = true;
            let response = JSON.parse(this.data.AnswerOrExceptionJson);
            if (response)
                this.MessageToUser = response.Message;
        }
    }


    async onSubmit() {
        this.storesAndItems = [];
        if (this.selectedOrderId != 'empty') {
            this.showTotal = true; 
            for (let ord of Object.keys(this.Orders)) {
                let Order: Order = this.Orders[ord];
                if (Order.OrderId == this.selectedOrderId) {
                    this.order = Order;
                    for (let session of Object.keys(Order.StoreOrders)) {
                        let storeOrder = Order.StoreOrders[session];
                        let storeInfoByIdResponse: JsonParsed = await this.httpService.getWithParamstAsync('api/store/GetStoreInformationByID', storeOrder.StoreId);
                        if (storeInfoByIdResponse.Success) {
                            let store: MyStore = JSON.parse(storeInfoByIdResponse.AnswerOrExceptionJson);
                            let storeName = store.ContactDetails.Name;
                            let storeItems: OrderItem[] = storeOrder.StoreOrderItems;
                            this.storesAndItems.push({ orderId: Order.OrderId, storeName: storeName, items: storeItems });
                        }
                        else {
                            this.actionFailed = true;
                            let response = JSON.parse(this.data.AnswerOrExceptionJson);
                            if (response)
                                this.MessageToUser = response.Message;
                        }
                    }
                }
            }
        }
    }
}

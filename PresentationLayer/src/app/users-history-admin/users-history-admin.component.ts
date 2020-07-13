import { Component, OnInit } from '@angular/core';
import { HttpRequestService, JsonParsed } from '../http-request.service';
import { Order, MyStore, OrderItem, StoreAndItems } from '../contracts.service';

@Component({
  selector: 'app-users-history-admin',
  templateUrl: './users-history-admin.component.html',
  styleUrls: ['./users-history-admin.component.css']
})
export class UsersHistoryAdminComponent implements OnInit {
    userHistoryAdminFailed: boolean;
    MessageToUser: string;
    Orders: Order[];
    Object = Object;
    storesAndItems: StoreAndItems[];

    constructor(private httpService: HttpRequestService,
    ) {
        this.userHistoryAdminFailed = false;
        this.MessageToUser = '';
        this.Orders = [];
        this.storesAndItems = [];
    }

    ngOnInit(): void {
        this.httpService.get('api/user/IsAdmin').subscribe((data: JsonParsed) => {
            if (!data.Success) {
                this.userHistoryAdminFailed = true;
                let response = JSON.parse(data.AnswerOrExceptionJson);
                if (response) {
                    this.MessageToUser = response.Message;
                }
            }
        });
    }

    async onSubmit(customerData) {
        this.storesAndItems = [];
        let dataOfHistory: JsonParsed = await this.httpService.getWithParamstAsync('api/Purchase/GetUserOrderHistory', customerData.username);
        if (dataOfHistory.Success) {
            this.Orders = JSON.parse(dataOfHistory.AnswerOrExceptionJson);
            for (let ord of Object.keys(this.Orders)) {
                let Order: Order = this.Orders[ord];
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
                        this.userHistoryAdminFailed = true;
                        let response = JSON.parse(storeInfoByIdResponse.AnswerOrExceptionJson);
                        if (response)
                            this.MessageToUser = response.Message;
                    }
                }
            }
        } else {
            this.userHistoryAdminFailed = true;
            let response = JSON.parse(dataOfHistory.AnswerOrExceptionJson);
            if (response) {
                this.MessageToUser = response.Message;
            }
        }
    }
}

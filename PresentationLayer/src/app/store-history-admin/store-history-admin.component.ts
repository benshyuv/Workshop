import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from "@angular/router";
import { MyStore, Order, OrderItem, OrderAndItems, StoreOrder } from '../contracts.service';
import { HttpRequestService, JsonParsed } from '../http-request.service';

@Component({
  selector: 'app-store-history-admin',
  templateUrl: './store-history-admin.component.html',
  styleUrls: ['./store-history-admin.component.css']
})
export class StoreHistoryAdminComponent implements OnInit {
    storeid: string;
    storeName: string;
    storeHistoryFailed: boolean;
    MessageToUser: string;
    Orders: Order[];
    ordersAndItems: OrderAndItems[];
    store: MyStore;
    Object = Object;
    permission: boolean;

    constructor(
        private httpService: HttpRequestService,
        private router: ActivatedRoute,
        private routerHistory: Router,
    ) {
        this.Orders = [];
        this.ordersAndItems = [];
        this.storeHistoryFailed = false;
        this.MessageToUser = '';
        this.permission = true;
    }

    async ngOnInit() {
        this.storeid = this.router.snapshot.paramMap.get('storeid');
        let dataAdmin: JsonParsed = await this.httpService.getAsync('api/user/IsAdmin');
        if (dataAdmin.Success) {
            this.permission = true;
        }

        let dataOfHistory: JsonParsed = await this.httpService.getWithParamstAsync('api/store/GetStoreOrderHistoryAdmin', this.storeid);
        if (dataOfHistory.Success) {
            this.Orders = JSON.parse(dataOfHistory.AnswerOrExceptionJson);
            for (let session of Object.keys(this.Orders)) {
                let storeOrder: StoreOrder = this.Orders[session];
                let storeInfoByIdResponse: JsonParsed = await this.httpService.getWithParamstAsync('api/store/GetStoreInformationByID', storeOrder.StoreId);
                if (storeInfoByIdResponse.Success) {
                    this.store = JSON.parse(storeInfoByIdResponse.AnswerOrExceptionJson);
                    this.storeName = this.store.ContactDetails.Name;
                    let storeItems: OrderItem[] = storeOrder.StoreOrderItems;
                    this.ordersAndItems.push({ orderId: storeOrder.OrderId, items: storeItems });
                }
            }
        } else {
            this.storeHistoryFailed = true;
            let response = JSON.parse(dataOfHistory.AnswerOrExceptionJson);
            if (response) {
                this.MessageToUser = response.Message;
            }
        }
    }

    backToStore() {
        this.routerHistory.navigateByUrl('/home/' + this.storeid);
    }
}

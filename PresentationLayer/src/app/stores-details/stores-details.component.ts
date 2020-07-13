import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from "@angular/router";
import { StoreInventory, Item, MyStore } from '../contracts.service';
import { HttpRequestService, JsonParsed } from '../http-request.service';

@Component({
  selector: 'app-stores-details',
  templateUrl: './stores-details.component.html',
  styleUrls: ['./stores-details.component.css']
})
export class StoresDetailsComponent implements OnInit {
    store: MyStore;
    storeInventory: StoreInventory;
    storeid: string;
    items: Item[];
    actionFailed: boolean;
    MessageToUser: string;
    permission: boolean;
    inventoryPermission: boolean;

    constructor(
        private router: ActivatedRoute,
        private routerAdd: Router,
        private httpService: HttpRequestService,
    )
    {
        this.items = [];
        this.actionFailed = false;
        this.MessageToUser = '';
        this.permission = false;
    }
    
    async ngOnInit() {
        this.storeid = this.router.snapshot.paramMap.get('id');
        let data: JsonParsed = await this.httpService.getWithParamstAsync('api/store/GetMyPermissions', this.storeid);
        if (data.Success) {
            let permissions: string[] = JSON.parse(data.AnswerOrExceptionJson);
            if (permissions.includes("APPOINT_OWNER") || permissions.includes("REMOVE_OWNER") || permissions.includes("APPOINT_MANAGER") ||
                permissions.includes("EDIT_PERMISSIONS") || permissions.includes("REMOVE_MANAGER") || permissions.includes("INVENTORY") ||
                permissions.includes("POLICY") || permissions.includes("CLOSE_STORE") || permissions.includes("REQUESTS") || permissions.includes("HISTORY")) {
                this.permission = true;
            }
        }

        let dataAdmin: JsonParsed = await this.httpService.getAsync('api/user/IsAdmin');
        if (dataAdmin.Success) {
            this.permission = true;
        }

        let storeInfoByIdResponse: JsonParsed = await this.httpService.getWithParamstAsync('api/store/GetStoreInformationByID', this.storeid);
        if (storeInfoByIdResponse.Success) {
            this.store = JSON.parse(storeInfoByIdResponse.AnswerOrExceptionJson);
            this.storeInventory = this.store.storeInventory;
            for (let key in this.storeInventory.Items) {
                this.items.push(this.storeInventory.Items[key]);
            }
        } else {
            this.actionFailed = true;
            let response = JSON.parse(data.AnswerOrExceptionJson);
            if (response) {
                this.MessageToUser = response.Message;
            }
        }
    }

    storeManagment() {
        this.routerAdd.navigate(['/home/' + this.store.Id + '/storeManagment', { 'storeName': this.store.ContactDetails.Name }]);
    }
}

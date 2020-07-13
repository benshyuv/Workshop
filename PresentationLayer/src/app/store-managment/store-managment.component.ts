import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from "@angular/router";
import { HttpRequestService, JsonParsed } from '../http-request.service';

@Component({
  selector: 'app-store-managment',
  templateUrl: './store-managment.component.html',
  styleUrls: ['./store-managment.component.css']
})
export class StoreManagmentComponent implements OnInit {
    storeid: string;
    storeName: string;
    appointOwnerPermission: boolean;
    removeOwnerPermission: boolean;
    appointManagerPermission: boolean;
    editPermission: boolean;
    removeManagerPermission: boolean;
    inventoryPermission: boolean;
    historyPermission: boolean;
    policyPermission: boolean;
    historyAdminPermission: boolean;

    constructor(private router: ActivatedRoute,
        private routerManagement: Router,
        private httpService: HttpRequestService,
    ) {
        this.inventoryPermission = false;
        this.appointOwnerPermission = false;
        this.removeOwnerPermission = false;
        this.appointManagerPermission = false;
        this.editPermission = false;
        this.removeManagerPermission = false;
        this.historyPermission = false;
        this.policyPermission = false;
        this.historyAdminPermission = false;
    }

    async ngOnInit() {
        this.storeid = this.router.snapshot.paramMap.get('storeid');
        this.storeName = this.router.snapshot.paramMap.get('storeName');
        let data: JsonParsed = await this.httpService.getWithParamstAsync('api/store/GetMyPermissions', this.storeid);
        if (data.Success) {
            let permissions: string[] = JSON.parse(data.AnswerOrExceptionJson);
            if (permissions.includes("APPOINT_OWNER")) {
                this.appointOwnerPermission = true;
            }
            if (permissions.includes("REMOVE_OWNER")) {
                this.removeOwnerPermission = true;
            }
            if (permissions.includes("APPOINT_MANAGER")) {
                this.appointManagerPermission = true;
            }
            if (permissions.includes("EDIT_PERMISSIONS")) {
                this.editPermission = true;
            }
            if (permissions.includes("REMOVE_MANAGER")) {
                this.removeManagerPermission = true;
            }
            if (permissions.includes("INVENTORY")) {
                this.inventoryPermission = true;
            }
            if (permissions.includes("HISTORY")) {
                this.historyPermission = true;
            }

            if (permissions.includes("POLICY")) {
                this.policyPermission = true;
            }
        }

        let dataAdmin: JsonParsed = await this.httpService.getAsync('api/user/IsAdmin');
        if (dataAdmin.Success) {
            this.historyAdminPermission = true;
        }
    }

    addItem() {
        this.routerManagement.navigate(['/home/' + this.storeid + '/storeManagment/addItem', { 'storeName': this.storeName }]);
    }

    storeHistory() {
        this.routerManagement.navigateByUrl('/home/' + this.storeid + '/storeManagment/storeHistory');
    }

    storeHistoryAdmin() {
        this.routerManagement.navigateByUrl('/home/' + this.storeid + '/storeManagment/storeHistoryAdmin');
    }

    AppointOwner() {
        this.routerManagement.navigate(['/home/' + this.storeid + '/storeManagment/addStoreOwner', { 'storeName': this.storeName }]);
    }

    ApproveOwner() {
        this.routerManagement.navigate(['/home/' + this.storeid + '/storeManagment/approveStoreOwner', { 'storeName': this.storeName }]);
    }

    AppointManager() {
        this.routerManagement.navigate(['/home/' + this.storeid + '/storeManagment/addStoreManager', { 'storeName': this.storeName }]);
    }

    removeStoreManager() {
        this.routerManagement.navigate(['/home/' + this.storeid + '/storeManagment/removeStoreManager', { 'storeName': this.storeName }]);
    }

    managePermissions() {
        this.routerManagement.navigate(['/home/' + this.storeid + '/storeManagment/managePermissions', { 'storeName': this.storeName }]);
    }

    manageDiscounts() {
        this.routerManagement.navigate(['/home/' + this.storeid + '/storeManagment/discountsManagement', { 'storeName': this.storeName }]);
    }

    managePurchases() {
        this.routerManagement.navigate(['/home/' + this.storeid + '/storeManagment/purchasesManagment', { 'storeName': this.storeName }]);
    }
	
    removeStoreOwner() {
        this.routerManagement.navigate(['/home/' + this.storeid + '/storeManagment/removeStoreOwner', { 'storeName': this.storeName }]);
    }

    backToStore() {
        this.routerManagement.navigateByUrl('/home/' + this.storeid);
    }
}

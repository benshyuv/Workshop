import { Component, OnInit } from '@angular/core';
import { Router, ActivatedRoute } from "@angular/router";

@Component({
  selector: 'app-manage-purchases',
  templateUrl: './manage-purchases.component.html',
  styleUrls: ['./manage-purchases.component.css']
})
export class ManagePurchasesComponent implements OnInit {

    storeName: string;
    storeid: string;

    constructor(private router: ActivatedRoute, private routerDiscountsManager: Router) { }

    ngOnInit(): void {
        this.storeName = this.router.snapshot.paramMap.get('storeName');
        this.storeid = this.router.snapshot.paramMap.get('storeid');
    }

    manageTypesOfPurchasePolicy() {
        this.routerDiscountsManager.navigate(['/home/' + this.storeid + '/storeManagment/typesOfPurchasePolicy', { 'storeName': this.storeName }]);
    }

    managePurchasePolicies() {
        this.routerDiscountsManager.navigate(['/home/' + this.storeid + '/storeManagment/purchasePolicies', { 'storeName': this.storeName }]);
    }

    backToStore() {
        this.routerDiscountsManager.navigateByUrl('/home/' + this.storeid);
    }

}

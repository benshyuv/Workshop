import { Component, OnInit } from '@angular/core';
import { Router, ActivatedRoute } from "@angular/router";

@Component({
  selector: 'app-discounts-management',
  templateUrl: './discounts-management.component.html',
  styleUrls: ['./discounts-management.component.css']
})
export class DiscountsManagementComponent implements OnInit {
    storeName: string;
    storeid: string;

    constructor(private router: ActivatedRoute, private routerDiscountsManager: Router) { }

    ngOnInit(): void {
        this.storeName = this.router.snapshot.paramMap.get('storeName');
        this.storeid = this.router.snapshot.paramMap.get('storeid');
    }

    discountTypesEditPage() {
        this.routerDiscountsManager.navigate(['/home/' + this.storeid + '/storeManagment/discountTypesEditPage', { 'storeName': this.storeName }]);
    }

    discountsEditPage() {
        this.routerDiscountsManager.navigate(['/home/' + this.storeid + '/storeManagment/discountsEditPage', { 'storeName': this.storeName }]);
    }

    backToStore() {
        this.routerDiscountsManager.navigateByUrl('/home/' + this.storeid);
    }
}

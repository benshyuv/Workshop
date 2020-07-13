import { Component, OnInit } from '@angular/core';
import { Router, ActivatedRoute } from "@angular/router";
import { MyStore, Item, StoreInventory, RemoveDiscountBodyRequest, Discount, ComposeTwoDiscountsBodyRequest} from '../contracts.service';
import { HttpRequestService, JsonParsed } from '../http-request.service';
import { FormGroup, FormControl, Validators } from '@angular/forms';

@Component({
  selector: 'app-discounts-edit-page',
  templateUrl: './discounts-edit-page.component.html',
  styleUrls: ['./discounts-edit-page.component.css']
})
export class DiscountsEditPageComponent implements OnInit {
    storeName: string;
    storeid: string;
    store: MyStore;
    items: Item[];
    discounts: Discount[]; //TODO:FIX!
    storeInventory: StoreInventory;
    actionFailed1: boolean;
    actionFailed2: boolean;
    actionFailed0: boolean;
    messageToUser: string;
    operatorsForComopsite = [ "&", "|", "xor"];
    formDelete = new FormGroup({
        formDelete: new FormControl('', Validators.required)
    });

    formDiscountComposite = new FormGroup({
        formDiscountLeft: new FormControl('', Validators.required),
        formDiscountRight: new FormControl('', Validators.required),
        formOperator: new FormControl('', Validators.required),
    });

    constructor(private httpService: HttpRequestService, private router: ActivatedRoute, private routerDiscountsEditPage: Router) {
        this.storeName = "";
        this.storeid = "";
        this.actionFailed0 = false;
        this.actionFailed1 = false;
        this.actionFailed2 = false;
        this.messageToUser = "";
        this.discounts = [];
    }

    async ngOnInit(): Promise<void> {
        this.storeid = this.router.snapshot.paramMap.get('storeid');
        this.storeName = this.router.snapshot.paramMap.get('storeName');
        let params: Map<string, string> = new Map();
        params.set("storeid", this.storeid);
        params.set("itemid", undefined);

        let dataDiscounts: JsonParsed = await this.httpService.getWithParamsAsync('api/store/GetAllDiscounts', params);
        if (dataDiscounts.Success) {
            this.discounts = JSON.parse(dataDiscounts.AnswerOrExceptionJson);
            //this.store = JSON.parse(dataDiscounts.AnswerOrExceptionJson);
            //this.storeName = this.store.ContactDetails.Name;
            //this.storeInventory = this.store.storeInventory;
           // for (let key in this.storeInventory.Items) {
            //    this.items.push(this.storeInventory.Items[key]);
           // }

        }
        else {
            let response = JSON.parse(dataDiscounts.AnswerOrExceptionJson);
            if (response) {
                this.actionFailed0 = true;
                this.messageToUser = response.Message;
            }
        }
    }

    AddOpenDiscount() {
        this.routerDiscountsEditPage.navigateByUrl('/home/' + this.storeid + '/storeManagment/AddOpenDiscount');
    }

    AddItemConditionalDiscount_MinItems_ToDiscountOnAll() {
        this.routerDiscountsEditPage.navigateByUrl('/home/' + this.storeid + '/storeManagment/AddItemConditionalDiscount_MinItems_ToDiscountOnAll');
    }

    AddItemConditionalDiscount_MinItems_ToDiscountOnExtraItems() {
        this.routerDiscountsEditPage.navigateByUrl('/home/' + this.storeid + '/storeManagment/AddItemConditionalDiscount_MinItems_ToDiscountOnExtraItems');
    }

    AddStoreConditionalDiscount() {
        this.routerDiscountsEditPage.navigate(['/home/' + this.storeid + '/storeManagment/AddStoreConditionalDiscount', { 'storeName': this.storeName }]);
    }

    backToDiscountManagement() {
        this.routerDiscountsEditPage.navigate(['/home/' + this.storeid + '/storeManagment/discountsManagement', { 'storeName': this.storeName }]);
    }

    ComposeTwoDiscounts() {
        let compositeRequest: ComposeTwoDiscountsBodyRequest = {
            StoreID: this.storeid,
            DiscountLeftID: this.formDiscountComposite.get('formDiscountLeft').value.DiscountID,
            DiscountRightID: this.formDiscountComposite.get('formDiscountRight').value.DiscountID,
            BoolOperator: this.formDiscountComposite.get('formOperator').value
        };

        this.httpService.post('api/store/ComposeTwoDiscounts', compositeRequest).subscribe((data: JsonParsed) => {
            if (data.Success) {
                window.alert('The discounts have been composed');
                this.routerDiscountsEditPage.navigateByUrl('/RefreshComponent', { skipLocationChange: true }).then(() =>
                    this.routerDiscountsEditPage.navigate(['/home/' + this.storeid + '/storeManagment/discountsManagement', { 'storeName': this.storeName }]));
            } else {
                this.actionFailed1 = true;
                let response = JSON.parse(data.AnswerOrExceptionJson);
                if (response) {
                    this.messageToUser = response.Message;
                }
            }
        });
    }

    deleteDiscount(){
        let deleteRequest: RemoveDiscountBodyRequest = {
            StoreID: this.storeid,
            DiscountID: this.formDelete.get('formDelete').value.DiscountID
        };

        this.httpService.post('api/store/RemoveDiscount', deleteRequest).subscribe((data: JsonParsed) => {
            if (data.Success) {
                window.alert('The discount has been removed from store');
                this.routerDiscountsEditPage.navigateByUrl('/RefreshComponent', { skipLocationChange: true }).then(() =>
                    this.routerDiscountsEditPage.navigate(['/home/' + this.storeid + '/storeManagment/discountsManagement', { 'storeName': this.storeName }]));
            } else {
                this.actionFailed2 = true;
                let response = JSON.parse(data.AnswerOrExceptionJson);
                if (response) {
                    this.messageToUser = response.Message;
                }
            }
        });
    }
}

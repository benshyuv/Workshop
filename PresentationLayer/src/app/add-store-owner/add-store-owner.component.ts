import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from "@angular/router";
import { HttpRequestService, JsonParsed } from '../http-request.service';
import { AppointOrRemoveOwnerOrManagerBodyRequest, MyStore } from '../contracts.service';

@Component({
  selector: 'app-add-store-owner',
  templateUrl: './add-store-owner.component.html',
  styleUrls: ['./add-store-owner.component.css']
})
export class AddStoreOwnerComponent implements OnInit {
    appointStoreOwnerFailed: boolean;
    MessageToUser: string;
    storeId: string;
    StoreName: string;

    constructor(
        private router: ActivatedRoute,
        private routerAppoint: Router,
        private httpService: HttpRequestService,
    ) {
        this.appointStoreOwnerFailed = false;
        this.MessageToUser = '';
    }

    ngOnInit(): void {
        this.storeId = this.router.snapshot.paramMap.get('storeid');
        this.StoreName = this.router.snapshot.paramMap.get('storeName');
  }

    onSubmit(customerData) {
        let appointOwnerRequest: AppointOrRemoveOwnerOrManagerBodyRequest =
        {
            StoreId: this.storeId,
            Username: customerData.username
        }

        this.httpService.post('api/store/AppointOwner', appointOwnerRequest).subscribe((data: JsonParsed) => {
            if (data.Success) {
                this.routerAppoint.navigateByUrl('/home/' + this.storeId);
            } else {
                this.appointStoreOwnerFailed = true;
                let response = JSON.parse(data.AnswerOrExceptionJson);
                if (response) {
                    this.MessageToUser = response.Message;
                }
            }
        });
    }

}

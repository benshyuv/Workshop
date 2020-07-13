import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from "@angular/router";
import { HttpRequestService, JsonParsed } from '../http-request.service';
import { AppointOrRemoveOwnerOrManagerBodyRequest, MyStore } from '../contracts.service';

@Component({
  selector: 'app-add-store-manager',
  templateUrl: './add-store-manager.component.html',
  styleUrls: ['./add-store-manager.component.css']
})
export class AddStoreManagerComponent implements OnInit {
    appointStoreManagerFailed: boolean;
    MessageToUser: string;
    storeId: string;
    StoreName: string;

    constructor(
        private router: ActivatedRoute,
        private routerAppoint: Router,
        private httpService: HttpRequestService,
    ) {
        this.appointStoreManagerFailed = false;
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

        this.httpService.post('api/store/AppointManager', appointOwnerRequest).subscribe((data: JsonParsed) => {
            if (data.Success) {
                this.routerAppoint.navigateByUrl('/home/' + this.storeId);
            } else {
                this.appointStoreManagerFailed = true;
                let response = JSON.parse(data.AnswerOrExceptionJson);
                if (response) {
                    this.MessageToUser = response.Message;
                }
            }
        });
    }
}

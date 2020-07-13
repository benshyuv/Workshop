import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from "@angular/router";
import { HttpRequestService, JsonParsed } from '../http-request.service';
import { AppointOrRemoveOwnerOrManagerBodyRequest, MyStore } from '../contracts.service';

@Component({
  selector: 'app-remove-store-manager',
  templateUrl: './remove-store-manager.component.html',
  styleUrls: ['./remove-store-manager.component.css']
})
export class RemoveStoreManagerComponent implements OnInit {
    removeStoreManagerFailed: boolean;
    MessageToUser: string;
    storeId: string;
    StoreName: string;

    constructor(
        private router: ActivatedRoute,
        private routerAppoint: Router,
        private httpService: HttpRequestService,
    ) {
        this.removeStoreManagerFailed = false;
        this.MessageToUser = '';
    }

    ngOnInit(): void {
        this.storeId = this.router.snapshot.paramMap.get('storeid');
        this.StoreName = this.router.snapshot.paramMap.get('storeName');
    }

    onSubmit(customerData) {
        let appointOwnerRequst: AppointOrRemoveOwnerOrManagerBodyRequest =
        {
            StoreId: this.storeId,
            Username: customerData.username
        }

        this.httpService.post('api/store/RemoveManager', appointOwnerRequst).subscribe((data: JsonParsed) => {
            if (data.Success) {
                this.routerAppoint.navigateByUrl('/home/' + this.storeId);
            } else {
                this.removeStoreManagerFailed = true;
                let response = JSON.parse(data.AnswerOrExceptionJson);
                if (response) {
                    this.MessageToUser = response.Message;
                }
            }
        });
    }

}

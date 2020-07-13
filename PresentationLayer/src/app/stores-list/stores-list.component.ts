import { Component, OnInit } from '@angular/core';
import { HttpRequestService, JsonParsed } from '../http-request.service';
import { MyStore } from '../contracts.service';

@Component({
    selector: 'app-stores-list',
    templateUrl: './stores-list.component.html',
    styleUrls: ['./stores-list.component.css']
})
export class StoresListComponent implements OnInit {
    stores: MyStore[];
    getAllStoresFaild: boolean;
    MessageToUser: string;

    constructor(
        private httpService: HttpRequestService,
    ) {
        this.getAllStoresFaild = false;
        this.MessageToUser = '';
    }

    ngOnInit(): void {
        this.httpService.get('api/store/GetAllStoresInformation').subscribe((data: JsonParsed) => {
            if (data.Success) {
                this.stores = JSON.parse(data.AnswerOrExceptionJson);
            } else {
                this.getAllStoresFaild = true;
                let response = JSON.parse(data.AnswerOrExceptionJson);
                if (response) {
                    this.MessageToUser = response.Message;
                }
            }
        });
    }

}

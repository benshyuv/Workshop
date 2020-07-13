import { Component, OnInit } from '@angular/core';
import { Router } from "@angular/router";
import { HttpRequestService, JsonParsed } from '../http-request.service';

@Component({
    selector: 'app-open-store',
    templateUrl: './open-store.component.html',
    styleUrls: ['./open-store.component.css']
})
export class OpenStoreComponent implements OnInit {
    openStoreFailed: boolean;
    MessageToUser: string;

    constructor(
        private router: Router,
        private httpService: HttpRequestService, )
    {
        this.openStoreFailed = false;
        this.MessageToUser = '';
    }

    ngOnInit(): void {
    }

    onSubmit(storeData) {
        this.httpService.post('api/store/OpenStore', storeData).subscribe((data: JsonParsed) => {
            if (data.Success) {
                this.router.navigateByUrl('/home');
            } else {
                this.openStoreFailed = true;
                let response = JSON.parse(data.AnswerOrExceptionJson);
                if (response) {
                    this.MessageToUser = response.Message;
                }
            }
        });
    }
}

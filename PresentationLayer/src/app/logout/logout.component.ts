import { Component, OnInit } from '@angular/core';
import { Router } from "@angular/router";
import { HttpRequestService, JsonParsed } from '../http-request.service';

@Component({
    selector: 'app-logout',
    templateUrl: './logout.component.html',
    styleUrls: ['./logout.component.css']
})
export class LogoutComponent implements OnInit {
    logoutSucces: boolean;
    MessageToUser: string;

    constructor(
        private httpService: HttpRequestService,
        private router: Router,
    ) {
        this.logoutSucces = false;
    }

    ngOnInit(): void {
        this.httpService.get('api/user/logout').subscribe((data: JsonParsed) => {
            if (!data.Success) {
                let response = JSON.parse(data.AnswerOrExceptionJson);
                if (response) {
                    this.MessageToUser = response.Message;
                }
            }
            else {
                this.logoutSucces = true;
                localStorage.removeItem('currentUser');
                window.alert('Logout Success - Hope to see you soon');
                this.router.navigateByUrl('/home').then((_) => window.location.reload()); 
            }
        });
    }
}

import { Component, OnInit, ChangeDetectorRef  } from '@angular/core';
import { Router } from "@angular/router";
import { HttpRequestService, JsonParsed } from '../http-request.service';

@Component({
  selector: 'app-login',
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.css'],
})
export class LoginComponent implements OnInit {
    response: string;
    result: JsonParsed;
    loginFailed: boolean;
    MessageToUser: string;
    
  constructor(
      private _cdr: ChangeDetectorRef,
      private router: Router,
      private httpService: HttpRequestService,
  ) {
      this.loginFailed = false;
      this.MessageToUser = '';
  }

  ngOnInit(): void {
  }

    onSubmit(customerData) {
        this.httpService.post('api/user/Login', customerData).subscribe((data: JsonParsed) => {
            if (data.Success) {
                localStorage.setItem('currentUser', customerData.username);
                this.router.navigateByUrl('/home').then((_) => window.location.reload());
            } else {
                this.loginFailed = true;
                let response = JSON.parse(data.AnswerOrExceptionJson);
                if (response) {
                    this.MessageToUser = response.Message;
                }
            }
        });

  }

    changeStatus(): void {
        setTimeout(() => {
            this.loginFailed = false;
            this.MessageToUser = '';
            this._cdr.detectChanges()
        }, 1000);
    }
}


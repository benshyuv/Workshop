import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { Router } from "@angular/router";
import { HttpRequestService, JsonParsed } from '../http-request.service';

@Component({
  selector: 'app-register',
  templateUrl: './register.component.html',
  styleUrls: ['./register.component.css']
})
export class RegisterComponent implements OnInit {
    registerFailed: boolean;
    MessageToUser: string;

  constructor(
      private _cdr: ChangeDetectorRef,
      private router: Router,
      private httpService: HttpRequestService,
  ) {
      this.registerFailed = false;
      this.MessageToUser = '';
  }

  ngOnInit(): void {
  }

    onSubmit(customerData) {
        this.httpService.post('api/user/Register', customerData).subscribe((data: JsonParsed) => {
            if (data.Success) {
              this.router.navigateByUrl('/login');
          } else {
                this.registerFailed = true;
                let response = JSON.parse(data.AnswerOrExceptionJson);
                if (response) {
                    this.MessageToUser = response.Message;
                }
          }
      });
  }

    changeStatus(): void {
        setTimeout(() => {
            this.registerFailed = false;
            this.MessageToUser = '';
            this._cdr.detectChanges()
        }, 1000);
    }
}




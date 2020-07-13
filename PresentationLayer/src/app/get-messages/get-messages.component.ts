import { Component, OnInit } from '@angular/core';
import { HttpRequestService, JsonParsed } from '../http-request.service';

@Component({
  selector: 'app-get-messages',
  templateUrl: './get-messages.component.html',
  styleUrls: ['./get-messages.component.css']
})
export class GetMessagesComponent implements OnInit {
    showMessagesFailed: boolean;
    MessageToUser: string;
    Messages: string[];


 constructor(private httpService: HttpRequestService, ) {
        this.showMessagesFailed=false;
        this.MessageToUser = '';
        this.Messages=[];
    }

  ngOnInit(): void {
        this.httpService.get('api/user/GetMyMessages').subscribe((data: JsonParsed) => {
            if (data.Success) {
                this.Messages = JSON.parse(data.AnswerOrExceptionJson);
                if (this.Messages.length === 0) {
                    this.Messages.push("You have no new messages");
                }
                
            }
            else {
                this.showMessagesFailed = true;
                let response = JSON.parse(data.AnswerOrExceptionJson);
                if (response) {
                    this.MessageToUser = response.Message;
                }
            }
        });
  }
  }

import { Component, OnInit } from '@angular/core';
import { Router, ActivatedRoute } from "@angular/router";
import { HttpRequestService, JsonParsed } from '../http-request.service';
import { Order, DateForStatisticsReqBody } from '../contracts.service';
import { FormGroup, FormControl } from '@angular/forms';
import { MomentDateAdapter, MAT_MOMENT_DATE_ADAPTER_OPTIONS } from '@angular/material-moment-adapter';
import { DateAdapter, MAT_DATE_FORMATS, MAT_DATE_LOCALE } from '@angular/material/core';
import { MatDatepicker } from '@angular/material/datepicker';
import * as _moment from 'moment';
import { GoogleChartsModule } from 'angular-google-charts';
import { MatMomentDateModule } from '@angular/material-moment-adapter';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { default as _rollupMoment, Moment } from 'moment';
import {MatDatepickerInputEvent} from '@angular/material/datepicker';


const moment = _rollupMoment || _moment;

@Component({
  selector: 'app-show-statistics',
  templateUrl: './show-statistics.component.html',
  styleUrls: ['./show-statistics.component.css'],
  providers: [  
    MatDatepickerModule,  
  ],
})
export class ShowStatisticsComponent implements OnInit {
    socket;
myData=[['date',0,0,0,0,0]];
myType='ColumnChart';
columnNames;
myWidth='600';
myHeight='400';
  permission : boolean;
  actionFailed: boolean;
  messageToUser: string;
  statisticsByUsers: { [key: string]: number[] };
  datefrom = new FormControl(moment());
  dateto = null;
  mycolumnNames=['Date','Guests','Registered','Managers','Owners','Admins'];
  myTitle='Statistics for WebSite';

    constructor(private httpService: HttpRequestService,       private router: Router,) {
  this.permission = true;
  this.actionFailed = false;
  this.messageToUser='';

        console.log('Websocket for stats starting');
        var connectionUrl = "wss://localhost:5001/statisticsLive";
        var self = this;
        this.socket = new WebSocket(connectionUrl);
        this.socket.onopen = function (event) {
            console.log('Websocket stats opened');
        };
        this.socket.onclose = function (event) {
            console.log('Websocket stats closed');

        };
        this.socket.onerror = function (event) {
            console.log('Websocket stats error' + event.data);
        };
        this.socket.onmessage = function (event) {
            console.log('Websocket ans' + event.data);
            console.log(`event data: ${event.data}`)
            if (event.data.includes('stats')) {
                console.log('in if');
                console.log('before ' + self.myData);
                var statsData = JSON.parse(event.data.substring(6));
                var newData = [];
                var found = false;
                for (var i = 0; i < self.myData.length; i++) {
                    console.log(self.myData[i]);
                    let date = self.myData[i][0];
                    
                    for (let keyDate in statsData) {
                        var shortDate = keyDate.substring(0, 10);
                        console.log(date + ' ' + shortDate);
                        if (shortDate === date) {
                            console.log('found');
                            newData.push([shortDate, statsData[keyDate][0], statsData[keyDate][1], statsData[keyDate][2], statsData[keyDate][3], statsData[keyDate][4]]);
                            found = true;
                            break;
                        }
                    }
                    if (!found) {
                        newData.push(self.myData[i]);
                    }
                }
                self.myData = newData;
                console.log('after ' + self.myData);
                }
            
        };

  
    
        
}



  ChangeDateTo(event: any,datepicker: MatDatepicker<Moment>) {
    this.dateto = new FormControl(moment());
    const ctrlValueTo = this.dateto.value;
    ctrlValueTo.set('year',event.target.value.getFullYear());
    ctrlValueTo.set('month',event.target.value.getMonth());
    ctrlValueTo.set('date',event.target.value.getDate());
    this.dateto.setValue(ctrlValueTo);
    datepicker.close();
  }
    ChangeDateFrom(event: any,datepicker: MatDatepicker<Moment>) {
    const ctrlValueFrom = this.datefrom.value;
    ctrlValueFrom.set('year',event.target.value.getFullYear());
    ctrlValueFrom.set('month',event.target.value.getMonth());
    ctrlValueFrom.set('date',event.target.value.getDate());
    this.datefrom.setValue(ctrlValueFrom);
    datepicker.close();
  }
  /*
  */

    ngOnInit(): void {

       

   
}
  async onSubmit(): Promise<void> {

     
        let statisticsReq: DateForStatisticsReqBody={
        datefrom: this.datefrom.value.format('L'),
        dateto: this.dateto.value.format('L')
        }
        let params: Map<string, string> = new Map();
        params.set('datefrom',statisticsReq.datefrom);
        params.set('dateto',statisticsReq.dateto);
        let data: JsonParsed= await this.httpService.getWithParamsAsync('api/user/GetDailyStatistics', params);
            if (data.Success) {
            this.myData=[];
             this.statisticsByUsers = JSON.parse(data.AnswerOrExceptionJson);
                this.myType = 'ColumnChart';
                for (let key in this.statisticsByUsers) {
                    let v = this.statisticsByUsers[key][0];
                    this.myData.push([key.substring(0,10), this.statisticsByUsers[key][0], this.statisticsByUsers[key][1], this.statisticsByUsers[key][2], this.statisticsByUsers[key][3], this.statisticsByUsers[key][4]]);
                    }
             }
             else {
                this.actionFailed = true;
                let response = JSON.parse(data.AnswerOrExceptionJson);
                if (response) {
                    this.messageToUser = response.Message;
                }
            }
    }
}

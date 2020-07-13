import { Component, OnInit } from '@angular/core';
import { Router, ActivatedRoute } from "@angular/router";
import { HttpRequestService, JsonParsed } from '../http-request.service';
import { Order, CheckoutBodyRequest } from '../contracts.service';

import { FormControl } from '@angular/forms';
import { MomentDateAdapter, MAT_MOMENT_DATE_ADAPTER_OPTIONS } from '@angular/material-moment-adapter';
import { DateAdapter, MAT_DATE_FORMATS, MAT_DATE_LOCALE } from '@angular/material/core';
import { MatDatepicker } from '@angular/material/datepicker';
import * as _moment from 'moment';
import { default as _rollupMoment, Moment } from 'moment';

const moment = _rollupMoment || _moment;

export const MY_FORMATS = {
    parse: {
        dateInput: 'MM/YYYY',
    },
    display: {
        dateInput: 'MM/YYYY',
        monthYearLabel: 'MMM YYYY',
        dateA11yLabel: 'LL',
        monthYearA11yLabel: 'MMMM YYYY',
    },
};


@Component({
  selector: 'app-checkout',
  templateUrl: './checkout.component.html',
    styleUrls: ['./checkout.component.css'],
    providers: [
        {
            provide: DateAdapter,
            useClass: MomentDateAdapter,
            deps: [MAT_DATE_LOCALE, MAT_MOMENT_DATE_ADAPTER_OPTIONS]
        },

        { provide: MAT_DATE_FORMATS, useValue: MY_FORMATS },
    ],
})

export class CheckoutComponent implements OnInit {
    checkoutFailed: boolean;
    MessageToUser: string;
    orderId: string;
    date = new FormControl(moment());
    
    constructor(
        private router: Router,
        private httpService: HttpRequestService,
        private routerCheckout: ActivatedRoute)
    {
        this.checkoutFailed = false;
        this.MessageToUser = '';
        this.orderId = 'emptyId';
    }

  ngOnInit(): void {
  }

    async onSubmit(customerData) {
        this.orderId = this.routerCheckout.snapshot.paramMap.get('orderId');
        let ExpireYear = this.date.value.year();
        let ExpireMonth = this.date.value.month() + 1;
        let checkoutRequest: CheckoutBodyRequest =
        {
            OrderID: this.orderId,
            PayingCustomerName: customerData.payingCustomerName,
            Card: customerData.card,
            Expire: ExpireMonth + '/' + ExpireYear,
            CCV: customerData.ccv,
            ID: customerData.id,
            Address: customerData.address,
            City: customerData.city,
            Country: customerData.country,
            ZipCode: customerData.zipcode
        }

        let checkoutResponse = await this.httpService.postAsync('api/purchase/CheckOut', checkoutRequest);
        if (checkoutResponse.Success) {
            window.alert('purchase was successful!');
            this.router.navigateByUrl('/home');
        } else {
            this.checkoutFailed = true;
            let response = JSON.parse(checkoutResponse.AnswerOrExceptionJson);
            if (response) {
                this.MessageToUser = response.Message;
            }
        }
    }

    chosenYearHandler(normalizedYear: Moment) {
        const ctrlValue = this.date.value;
        ctrlValue.year(normalizedYear.year());
        this.date.setValue(ctrlValue);
    }

    chosenMonthHandler(normalizedMonth: Moment, datepicker: MatDatepicker<Moment>) {
        const ctrlValue = this.date.value;
        ctrlValue.month(normalizedMonth.month());
        this.date.setValue(ctrlValue);
        datepicker.close();
    }

}

import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from "@angular/router";
import { HttpRequestService, JsonParsed } from '../http-request.service';
import { MyStore, AddItemBodyRequest } from '../contracts.service';

@Component({
  selector: 'app-add-item',
  templateUrl: './add-item.component.html',
  styleUrls: ['./add-item.component.css']
})
export class AddItemComponent implements OnInit {
    addItemFailed: boolean;
    MessageToUser: string;
    id: string;
    StoreName: string;

    constructor(
        private router: ActivatedRoute,
        private routerAdd: Router,
        private httpService: HttpRequestService,
    ) {
        this.addItemFailed = false;
        this.MessageToUser = '';
    }

    ngOnInit(): void {
        this.id = this.router.snapshot.paramMap.get('storeid');
        this.StoreName = this.router.snapshot.paramMap.get('storeName');
    }

    onSubmit(itemData) {
        let addItemRequest: AddItemBodyRequest =
        {
            categories: JSON.stringify(itemData.categories.split(',').map(function (category) {
                return category.trim();
            })),
            amount: itemData.amount,
            keywords: JSON.stringify(itemData.keywords.split(',').map(function (keyword) {
                return keyword.trim();
            })),
            name: itemData.itemname,
            price: itemData.price,
            storeId: this.id
        }
        
        this.httpService.post('api/store/AddItem', addItemRequest).subscribe((data: JsonParsed) => {
            if (data.Success) {
                this.routerAdd.navigateByUrl('/home/' + this.id);
            } else {
                this.addItemFailed = true;
                let response = JSON.parse(data.AnswerOrExceptionJson);
                if (response) {
                    this.MessageToUser = response.Message;
                }
            }
        });
    }
}

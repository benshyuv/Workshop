import { Component, OnInit } from '@angular/core';
import { Router, ActivatedRoute } from "@angular/router";
import { MyStore} from '../contracts.service';
import { HttpRequestService, JsonParsed } from '../http-request.service';

@Component({
  selector: 'app-search-results',
  templateUrl: './search-results.component.html',
  styleUrls: ['./search-results.component.css']
})
export class SearchResultsComponent implements OnInit {
    itemName: string;
    filterItemRank: string;
    filterMinPrice: string;
    filterMaxPrice: string;
    filterStoreRank: string;
    category: string;
    keywords: string;
    itemsByStoreName;
    searchFailed: boolean;
    messageToUser: string;
    messageNoResults: string;
    Object = Object;

    constructor(private httpService: HttpRequestService, private router: ActivatedRoute) {
        this.itemName = null;
        this.filterItemRank = null;
        this.filterMinPrice = null;
        this.filterMaxPrice = null;
        this.filterStoreRank = null;
        this.category = null;
        this.keywords = null;
        this.searchFailed = false;
        this.messageToUser = '';
        this.itemsByStoreName = {};
    }

    async ngOnInit(): Promise<void> {
        this.itemName = this.router.snapshot.paramMap.get('itemName');
        this.filterItemRank = this.router.snapshot.paramMap.get('filterItemRank');
        this.filterMinPrice = this.router.snapshot.paramMap.get('filterMinPrice');
        this.filterMaxPrice = this.router.snapshot.paramMap.get('filterMaxPrice');
        this.filterStoreRank = this.router.snapshot.paramMap.get('filterStoreRank');
        this.category = this.router.snapshot.paramMap.get('category');
        let keywordsAsString = this.router.snapshot.paramMap.get('keywords');
        if (keywordsAsString) {
            this.keywords = JSON.stringify(keywordsAsString.split(','));
        }
        else {
            this.keywords = keywordsAsString;
        }

        let params: Map<string, string> = new Map();

        if (this.itemName && this.itemName.length > 0) {
            params.set("itemName", this.itemName);
        }

        if (this.filterItemRank && this.filterItemRank.length > 0) {
            params.set("filterItemRank", this.filterItemRank);
        }
        else {
            params.set("filterItemRank", null);
        }

        if (this.filterMinPrice && this.filterMinPrice.length > 0) {
            params.set("filterMinPrice", this.filterMinPrice);
        }
        else {
            params.set("filterMinPrice", null);
        }

        if (this.filterMaxPrice && this.filterMaxPrice.length > 0) {
            params.set("filterMaxPrice", this.filterMaxPrice);
        }
        else {
            params.set("filterMaxPrice", null);
        }

        if (this.filterStoreRank && this.filterStoreRank.length > 0) {
            params.set("filterStoreRank", this.filterStoreRank);
        }
        else {
            params.set("filterStoreRank", null);
        }

        if (this.category && this.category.length > 0) {
            params.set("category", this.category);
        }

        if (this.keywords && this.keywords.length > 0) {
            params.set("keywords", this.keywords);
        }

        params.set("filterItemRank", this.filterItemRank);
        params.set("filterMinPrice", this.filterMinPrice);
        params.set("filterMaxPrice", this.filterMaxPrice);
        params.set("filterStoreRank", this.filterStoreRank);

        let dataSearch: JsonParsed = await this.httpService.getWithParamsAsync('api/store/SearchItems', params);
        if (dataSearch.Success) {
            let results = JSON.parse(dataSearch.AnswerOrExceptionJson);
            for (var key in results) {
                let paramsForStoreDetails: Map<string, string> = new Map();
                paramsForStoreDetails.set('storeid', key);
                let dataStore: JsonParsed = await this.httpService.getWithParamsAsync('api/store/GetStoreInformationByID', paramsForStoreDetails);
                if (dataStore.Success) {
                        let store: MyStore = JSON.parse(dataStore.AnswerOrExceptionJson);
                        let name = store.ContactDetails.Name;
                        this.itemsByStoreName[name] = results[key];
                 }
                else {
                    let response = JSON.parse(dataStore.AnswerOrExceptionJson);
                    if (response) {
                        this.searchFailed = true;
                        this.messageToUser = response.Message;
                        return;
                    }
                }
            }

            if (Object.keys(results).length === 0 && results.constructor === Object) {
                this.searchFailed = true;
                this.messageToUser = "we are very sorry but we didn't find what you are looking for";
            }

        } else {
            this.searchFailed = true;
            let response = JSON.parse(dataSearch.AnswerOrExceptionJson);
            if (response) {
                this.messageToUser = response.Message;
            }
        }
  }

}

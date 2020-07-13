import { Component, OnInit } from '@angular/core';
import { Router } from "@angular/router";
import { ExtendedSearchDetails } from '../contracts.service';

@Component({
  selector: 'app-extended-search',
  templateUrl: './extended-search.component.html',
  styleUrls: ['./extended-search.component.css']
})
export class ExtendedSearchComponent implements OnInit {

    constructor(private router: Router) { }

  ngOnInit(): void {
  }

    onSubmitSearch(extenedSearchForm) {
        let details: ExtendedSearchDetails = {
            itemName: extenedSearchForm.itemName,
            filterItemRank: extenedSearchForm.filterItemRank,
            filterMinPrice: extenedSearchForm.filterMinPrice,
            filterMaxPrice: extenedSearchForm.filterMaxPrice,
            filterStoreRank: extenedSearchForm.filterStoreRank,
            category: extenedSearchForm.category,
            keywords: extenedSearchForm.keywords
        };

        this.router.navigate(['/searchResults', details]);
    }
}

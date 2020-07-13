import { BrowserModule } from '@angular/platform-browser';
import { NgModule } from '@angular/core';
import { RouterModule } from '@angular/router';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { HttpClientModule } from '@angular/common/http';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { Observable } from 'rxjs';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatIconModule } from '@angular/material/icon';
import { MatSidenavModule } from '@angular/material/sidenav';
import { MatListModule } from '@angular/material/list';
import { GoogleChartsModule } from 'angular-google-charts';
import {MatNativeDateModule} from '@angular/material/core';
import { MatButtonModule } from '@angular/material/button';
import { MatSelectModule } from '@angular/material/select';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatDatepickerModule } from '@angular/material/datepicker';
// import { NgbModule } from '@ng-bootstrap/ng-bootstrap'; //rate item
import { AppComponent } from './app.component';
import { LoginComponent } from './login/login.component';
import { RegisterComponent } from './register/register.component';
import { StoresListComponent } from './stores-list/stores-list.component';
import { StoresDetailsComponent } from './stores-details/stores-details.component';
import { LogoutComponent } from './logout/logout.component';
import { OpenStoreComponent } from './open-store/open-store.component';
import { AddItemComponent } from './add-item/add-item.component';
import { ViewCartComponent } from './view-cart/view-cart.component';
import { CheckoutComponent } from './checkout/checkout.component';
import { DisplayOrderComponent } from './display-order/display-order.component';
import { StoreHistoryComponent } from './store-history/store-history.component';
import { ItemPageComponent } from './item-page/item-page.component';
import { EditItemComponent } from './edit-item/edit-item.component';
import { SearchResultsComponent } from './search-results/search-results.component';
import { ExtendedSearchComponent } from './extended-search/extended-search.component';
import { StoreManagmentComponent } from './store-managment/store-managment.component';
import { AddStoreOwnerComponent } from './add-store-owner/add-store-owner.component';
import { AddStoreManagerComponent } from './add-store-manager/add-store-manager.component';
import { RemoveStoreManagerComponent } from './remove-store-manager/remove-store-manager.component';
import { DiscountsManagementComponent } from './discounts-management/discounts-management.component';
import { DiscountTypesEditPageComponent } from './discount-types-edit-page/discount-types-edit-page.component';
import { DiscountsEditPageComponent } from './discounts-edit-page/discounts-edit-page.component';
import { EditItemAmountInCartComponent } from './edit-item-amount-in-cart/edit-item-amount-in-cart.component';
import { AddOpenDiscountComponent } from './add-open-discount/add-open-discount.component';
import { RefreshComponentComponent } from './refresh-component/refresh-component.component';
import { ManagePermissionsComponent } from './manage-permissions/manage-permissions.component';
import { RemoveStoreOwnerComponent } from './remove-store-owner/remove-store-owner.component';
import { AddItemConditionalDiscountMinItemsToDiscountOnAllComponent } from './add-item-conditional-discount-min-items-to-discount-on-all/add-item-conditional-discount-min-items-to-discount-on-all.component';
import { AddItemConditionalDiscountMinItemsToDiscountOnExtraItemsComponent } from './add-item-conditional-discount-min-items-to-discount-on-extra-items/add-item-conditional-discount-min-items-to-discount-on-extra-items.component';
import { AddStoreConditionalDiscountComponent } from './add-store-conditional-discount/add-store-conditional-discount.component';
import { UsersHistoryAdminComponent } from './users-history-admin/users-history-admin.component';
import { StoreHistoryAdminComponent } from './store-history-admin/store-history-admin.component';
import { UserHistoryComponent } from './user-history/user-history.component';
import { GetMessagesComponent } from './get-messages/get-messages.component';
import { ApproveStoreOwnerComponent } from './approve-store-owner/approve-store-owner.component';
import { ManagePurchasesComponent } from './manage-purchases/manage-purchases.component';
import { ManageTypesOfPurchasePolicyComponent } from './manage-types-of-purchase-policy/manage-types-of-purchase-policy.component';
import { ManagePurchasePoliciesComponent } from './manage-purchase-policies/manage-purchase-policies.component';
import { AddItemMinMaxPurchasePolicyComponent } from './add-item-min-max-purchase-policy/add-item-min-max-purchase-policy.component';
import { AddStoreMinMaxPurchasePolicyComponent } from './add-store-min-max-purchase-policy/add-store-min-max-purchase-policy.component';
import { AddDaysNotAllowedPurchasePolicyComponent } from './add-days-not-allowed-purchase-policy/add-days-not-allowed-purchase-policy.component';
import { ShowStatisticsComponent } from './show-statistics/show-statistics.component';

@NgModule({
  imports: [
        BrowserModule,
        FormsModule,
        // NgbModule,
        ReactiveFormsModule,
        HttpClientModule,
        GoogleChartsModule,
        BrowserAnimationsModule,
        MatToolbarModule,
        MatSidenavModule,
        MatListModule,
        MatButtonModule,
        MatIconModule,
        MatSelectModule,
        MatNativeDateModule,
        MatFormFieldModule,
        MatInputModule,
        MatDatepickerModule,
        RouterModule.forRoot([
            { path: '', redirectTo: '/home', pathMatch: 'full' },
            { path: 'home', component: StoresListComponent },
            { path: 'login', component: LoginComponent },
            { path: 'register', component: RegisterComponent },
            { path: 'logout', component: LogoutComponent },
            { path: 'getMessages', component: GetMessagesComponent },
            { path: 'openStore', component: OpenStoreComponent },
            { path: 'usersHistoryAdmin', component: UsersHistoryAdminComponent },
            { path: 'home/:id', component: StoresDetailsComponent },
            { path: 'home/:storeid/storeManagment', component: StoreManagmentComponent },
            { path: 'home/:storeid/:itemid', component: ItemPageComponent },
            { path: 'viewCart', component: ViewCartComponent },
            { path: 'showStatistics', component: ShowStatisticsComponent },
            { path: 'viewCart/:itemId/EditItemAmountInCart', component: EditItemAmountInCartComponent},
            { path: 'checkout', component: CheckoutComponent },
            { path: 'displayOrder', component: DisplayOrderComponent },
            { path: 'home/:storeid/:itemid/editItem', component: EditItemComponent },
			{ path: 'searchResults', component: SearchResultsComponent },
            { path: 'extendedSearch', component: ExtendedSearchComponent },
            { path: 'UserHistory', component: UserHistoryComponent },
            { path: 'home/:storeid/storeManagment/addItem', component: AddItemComponent },
            { path: 'home/:storeid/storeManagment/storeHistory', component: StoreHistoryComponent },
            { path: 'home/:storeid/storeManagment/storeHistoryAdmin', component: StoreHistoryAdminComponent },
            { path: 'home/:storeid/storeManagment/addStoreOwner', component: AddStoreOwnerComponent },
            { path: 'home/:storeid/storeManagment/approveStoreOwner', component: ApproveStoreOwnerComponent },
            { path: 'home/:storeid/storeManagment/addStoreManager', component: AddStoreManagerComponent },
            { path: 'home/:storeid/storeManagment/removeStoreManager', component: RemoveStoreManagerComponent },
            { path: 'home/:storeid/storeManagment/managePermissions', component: ManagePermissionsComponent },
			{ path: 'home/:storeid/storeManagment/removeStoreOwner', component: RemoveStoreOwnerComponent },
            { path: 'home/:storeid/storeManagment/discountsManagement', component: DiscountsManagementComponent },
            { path: 'home/:storeid/storeManagment/discountTypesEditPage', component: DiscountTypesEditPageComponent },
            { path: 'home/:storeid/storeManagment/discountsEditPage', component: DiscountsEditPageComponent },
            { path: 'home/:storeid/storeManagment/AddOpenDiscount', component: AddOpenDiscountComponent },
            { path: 'RefreshComponent', component: RefreshComponentComponent },
            { path: 'home/:storeid/storeManagment/AddItemConditionalDiscount_MinItems_ToDiscountOnAll', component: AddItemConditionalDiscountMinItemsToDiscountOnAllComponent },
            { path: 'home/:storeid/storeManagment/AddItemConditionalDiscount_MinItems_ToDiscountOnExtraItems', component: AddItemConditionalDiscountMinItemsToDiscountOnExtraItemsComponent },
            { path: 'home/:storeid/storeManagment/AddStoreConditionalDiscount', component: AddStoreConditionalDiscountComponent },
            { path: 'home/:storeid/storeManagment/purchasesManagment', component: ManagePurchasesComponent },
            { path: 'home/:storeid/storeManagment/typesOfPurchasePolicy', component: ManageTypesOfPurchasePolicyComponent },
            { path: 'home/:storeid/storeManagment/purchasePolicies', component: ManagePurchasePoliciesComponent },
            { path: 'home/:storeid/storeManagment/AddItemMinMaxPurchasePolicy', component: AddItemMinMaxPurchasePolicyComponent },
            { path: 'home/:storeid/storeManagment/AddStoreMinMaxPurchasePolicy', component: AddStoreMinMaxPurchasePolicyComponent },
            { path: 'home/:storeid/storeManagment/AddDaysNotAllowedPurchasePolicy', component: AddDaysNotAllowedPurchasePolicyComponent },
        ])
    ],
    declarations: [
        AppComponent,
        LoginComponent,
        RegisterComponent,
        StoresListComponent,
        StoresDetailsComponent,
        LogoutComponent,
        OpenStoreComponent,
        AddItemComponent,
        ViewCartComponent,
        CheckoutComponent,
        DisplayOrderComponent,
        StoreHistoryComponent,
        ItemPageComponent,
        EditItemComponent,
		SearchResultsComponent,
        ExtendedSearchComponent,
        DiscountsManagementComponent,
        DiscountTypesEditPageComponent,
        DiscountsEditPageComponent,
        AddOpenDiscountComponent,
        StoreManagmentComponent,
        AddStoreOwnerComponent,
        AddStoreManagerComponent,
        RemoveStoreManagerComponent,
        ManagePermissionsComponent,
        RefreshComponentComponent,
        RemoveStoreOwnerComponent,
        AddItemConditionalDiscountMinItemsToDiscountOnAllComponent,
        AddItemConditionalDiscountMinItemsToDiscountOnExtraItemsComponent,
        AddStoreConditionalDiscountComponent,
        UsersHistoryAdminComponent,
        StoreHistoryAdminComponent,
        UserHistoryComponent,
        GetMessagesComponent,
        EditItemAmountInCartComponent,
        ApproveStoreOwnerComponent,
        ManagePurchasesComponent,
        ManageTypesOfPurchasePolicyComponent,
        ManagePurchasePoliciesComponent,
        AddItemMinMaxPurchasePolicyComponent,
        AddStoreMinMaxPurchasePolicyComponent,
        AddDaysNotAllowedPurchasePolicyComponent,
        ShowStatisticsComponent,
    ],
    providers: [],
    bootstrap: [AppComponent]
  })

export class AppModule { }


//@Injectable()
//export class ConnectInterceptor implements HttpInterceptor {// from https://stackoverflow.com/a/53512758/9553459
//    intercept(req: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
//        const authReq = req.clone({
//            //headers: req.headers.set("Authorization", "custom-token"),

//            withCredentials: true // Importand for consistent session id in ASP.NET
//        });
//        return next.handle(authReq);
//    }
//}

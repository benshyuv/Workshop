export interface MyStore {
    Id: string; 
    storeInventory: StoreInventory;
    ContactDetails: StoreContactDetails;
    Rank: number;
    PurchasePolicy: string;
    DiscountPolicy: string;
}

export interface StoreContactDetails {
    Name: string;
    Email: string;
    Address: string;
    Phone: string;
    BankAccountNumber: string;
    Bank: string;
    Description: string;
}

export interface StoreInventory {
    Items: { id: string, item: Item };
    StoreId: string;
}
export interface StoreItem
{
      StoreID: string;
      ItemID: string;
}
export interface Item {
    Id: string;
    storeID: string;
    Name: string;
    Amount: number;
    Categories: string;
    Rank: number;
    Price: number;
    Keywords: string;
}

export interface AddItemBodyRequest {
    storeId: string;
    name: string;
    amount: number;
    categories: string;
    price: number;
    keywords: string;
}

export interface AddToCartBodyRequest {
    storeId: string;
    itemId: string;
    amount: number;
}

export interface MyPair {
    item: Item;
    amount: number;
    storeId: string;
}

export interface CheckoutBodyRequest {
    OrderID: string;
    PayingCustomerName: string;
    Card: string;
    Expire: string;
    CCV: string;
    ID: string;
    Address: string;
    City: string;
    Country: string;
    ZipCode: string;
}

export interface Order {
    OrderId: string;
    UserId: string;
    StoreOrders: StoreOrder[];
    Type: string;
    OrderTime: Date;
    Total: number;
}

export interface StoreOrder {
    id: string;
    UserId: string;
    OrderId: string;
    StoreId: string;
    StoreOrderItems: OrderItem[];
    Type: string;
    OrderTime: Date;
    Total: number;
}

export interface OrderItem {
    ItemId: string;
    Amount: number;
    Name: string;
    categories: string[];
    rank: number;
    BasePricePerItem: number;
    DiscountedPricePerItem: number;
    keywords: string[];  
}

export interface StoreAndItems {
    orderId: string;
    storeName: string;
    items: OrderItem[];
}

export interface OrderAndItems {
    orderId: string;
    items: OrderItem[];
}

export interface SearchResultsInStore {
    storeId: string;
    items: Item[];
}

export interface ExtendedSearchDetails {
    itemName?: string;
    filterItemRank?: string;
    filterMinPrice?: string;
    filterMaxPrice?: string;
    filterStoreRank?: string;
    category?: string;
    keywords?: string;
}

export interface DeleteItemBodyRequest {
    storeId: string;
    itemId: string;
}

export interface EditItemBodyRequest {
    StoreID: string;
    ItemID: string;
    Amount: number;
    Rank: number;
    Price: number;
    Name: string;
    Categories: string;
    Keywords: string;
}

export interface RemoveDiscountBodyRequest{
    StoreID: string;
    DiscountID: string;
}

export interface MakeDiscountNotAllowedBodyRequest {
    StoreID: string;
    DiscountTypeString: string;
}

export interface MakeDiscountAllowedBodyRequest {
    StoreID: string;
    DiscountTypeString: string;
}

export interface AddOpenDiscountBodyRequest {
    StoreID: string;
    ItemID: string;
    Discount: number;
    DurationInDays: number;
}

export interface AddItemConditionalDiscount_MinItems_ToDiscountOnAllBodyRequest {
    StoreID: string;
    ItemID: string;
    DurationInDays: number;
    MinItems: number;
    Discount: number;
}

export interface AddItemConditionalDiscount_MinItems_ToDiscountOnExtraItemsBodyRequest {
    StoreID: string;
    ItemID: string;
    DurationInDays: number;
    MinItems: number;
    ExtraItems: number;
    DiscountForExtra: number;
}

export interface AddStoreConditionalDiscountBodyRequest {
    StoreID: string;
    DurationInDays: number;
    MinPurchase: number;
    Discount: number;
}

export interface ComposeTwoDiscountsBodyRequest {
    StoreID: string;
    DiscountLeftID: string;
    DiscountRightID: string;
    BoolOperator: string;
}

export interface Discount {
    DiscountID: string;
    ItemID: string;
    DateUntil:string;
    Op: Operator;
    LeftDiscount: Discount;
    RightDiscount: Discount;
    Precent: number;
    MinPurchase: number;
    MinItems: number;
    ExtraItems: number;
    Description: string;
    DiscountType: string;
}

enum Operator{
    XOR, OR, AND, IMPLIES
}

/*export interface RateItemBodyRequest {
    StoreId: string;
    ItemId: string;
    Rank: number;
}*/

export interface AppointOrRemoveOwnerOrManagerBodyRequest {
    StoreId: string;
    Username: string;
}

export interface AddOrRemovePermissionBodyRequest {
    StoreId: string;
    Username: string;
    Permission: string;
}

export interface MakePurchasePolicyNotAllowedBodyRequest {
    StoreID: string;
    PurchasePolicy: string;
}

export interface MakePurchasePolicyAllowedBodyRequest {
    StoreID: string;
    PurchasePolicy: string;
}

export interface PurchasePolicy {
    PolicyID: string;
    ItemID: string;
    Op: Operator;
    LeftPolicy: PurchasePolicy;
    RightPolicy: PurchasePolicy;
    MinAmount?: number;
    MaxAmount?: number;
    DaysNotAllowed: number[];
    Description: string;
    //must be from [item, store, days, composite]
    policyType: string;
}

export interface RemovePurchasePolicyBodyRequest {
    StoreID: string;
    PolicyID: string;
}

export interface ComposeTwoPurchasePoliciesBodyRequest {
    StoreID: string;
    PolicyLeftID: string;
    PolicyRightID: string;
    BoolOperator: string;
}

export interface AddItemMinMaxPurchasePolicyBodyRequest {
    StoreID: string;
    ItemID: string;
    MinAmount?: number;
    MaxAmount?: number;
}

export interface AddStoreMinMaxPurchasePolicyBodyRequest {
    StoreID: string;
    MinAmount?: number;
    MaxAmount?: number;
}
export interface DateForStatisticsReqBody
{
    datefrom:string;
    dateto:string;
}
export interface AddDaysNotAllowedPurchasePolicyBodyRequest {
    StoreID: string;
    DaysNotAllowed: number[];
}

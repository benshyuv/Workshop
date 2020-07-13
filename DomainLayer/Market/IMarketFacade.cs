using System;
using System.Collections.Generic;

namespace DomainLayer.Market
{
    public interface IMarketFacade
    {
        /// <summary>
        /// validates amount of item in store >= amount
        /// </summary>
        /// <param name="storeID"></param>
        /// <param name="itemID"></param>
        /// <param name="amountToCheck"></param>
        /// <returns></returns>
        public bool CheckSufficentAmountInInventory(Guid storeID, Guid itemID, int amountToCheck);

        public bool StoreExist(Guid storeID);

        public bool ItemExistInStore(Guid storeID, Guid itemID);

        /// <summary>
        ///
        /// </summary>
        /// <param name="sessionID"></param>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <returns>json type bool</returns>
        public string Register(Guid sessionID, string username, string password);/*UC 2.2*/

        public string Login(Guid sessionID, string username, string password);// UC 2.3
        public void ReturnProducts(Guid sessionID, Guid orderID);

        /// <summary>
        /// Returns all the stores with their items
        /// If no data returns empty dictionary.
        /// If store has no items - it's item's collection will have 0 items
        /// </summary>
        /// <returns>ReadOnlyDictionary<Store></returns>
        public string GetAllStoresInformation(Guid sessionID);// UC 2.4
        public bool IsloggedIn(Guid sessionID);

        /// <summary>
        /// /// Search for items in store by the "And" of following params: 
        /// name (if provided) &
        /// category (if provided) &
        /// keywords (if provided) &
        /// standing in filters (if provided)
        /// </summary>
        /// <param name="sessionID"></param>
        /// <param name="filterItemRank"></param>
        /// <param name="filterMinPrice"></param>
        /// <param name="filterMaxPrice"></param>
        /// <param name="filterStoreRank"></param>
        /// <param name="name"></param>
        /// <param name="category"></param>
        /// <param name="keywords">jsonType: list</param>
        /// <param name="filterCategory"></param>
        /// <returns>jsonType: dictionary of <storeID, collection of items> if there are, otherwise empty dictionary</returns>
        public string SearchItems(Guid sessionID, double? filterItemRank, double? filterMinPrice, double? filterMaxPrice, double? filterStoreRank, string name = null, string category = null, string keywords = null);
        public bool StoreExistByName(string name);

        public string Logout(Guid sessionID);// UC 3.1


        /// <summary>
        /// 
        /// </summary>
        /// <param name="sessionID"></param>
        /// <returns>JsonItem: Dictionary<Guid, List<Tuple<Item, int>>></returns>
        public string GetUserCart(Guid sessionID);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="userID">user to get history of</param>
        /// <returns>Json: List(Order) </returns>
        public string GetHistoryOfUser(Guid userID, DbAccess.MarketDbContext context);

        public bool IsPermitedOperation(Guid sessionID, Guid storeID, string permission);
        public string CancelPayment(Guid sessionID, Guid orderID, string cardNum);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="storeID"></param>
        /// <param name="grantorID">user to check if he is the grantor of permissions for userID</param>
        /// <param name="userID">user to check if granted permissions by grantorID</param>
        /// <returns></returns>
        public bool IsGrantorOf(Guid storeID, Guid sessionID, string username);

        public bool IsOwnerOfStore(Guid storeID, string username);
		string ValidatePurchase(Guid sessionID);

        public string AddToCart(Guid sessionID, Guid storeID, Guid itemID, int amount);// UC 2.6

        public string RemoveFromCart(Guid sessionID, Guid storeID, Guid itemID);// UC 2.7

        public string ChangeItemAmountInCart(Guid sessionID, Guid storeID, Guid itemID, int amount);//UC 2.7

        /// <summary>
        /// 
        /// </summary>
        /// <param name="storeID"></param>
        /// <returns>Json :  List<StoreOrder> </returns>
        public string GetStoreHistory(Guid storeID);

        public List<string> GetPermissionsInStore(Guid storeID, Guid userID);

        public string CheckOut(Guid sessionID, Guid orderID);

        public string DiscountAndReserve(Guid sessionID);

        public string CollectPayment(Guid sessionID, Guid orderID, string cardNum, string expire, string CCV, string cardOwnerName, string cardOwnerID);

        public string ScheduleDelivery(Guid sessionID, Guid orderID, string address, string city, string country, string zipCode, string name);

        /// <summary>
		/// open store
		/// </summary>
		/// <param name="sessionID"></param>
		/// <param name="name"></param>
		/// <param name="email"></param>
		/// <param name="address"></param>
		/// <param name="phone"></param>
		/// <param name="bankAccountNumber"></param>
		/// <param name="bank"></param>
		/// <param name="description"></param>
		/// <param name="purchasePolicy"></param>
		/// <param name="discountPolicy"></param>
		/// <returns>jsonType: Store</returns>
		public string OpenStore(Guid sessionID, string name, string email, string address, string phone, string bankAccountNumber, string bank, string description, string purchasePolicy, string discountPolicy);// UC 3.2


        public string GetMyOrderHistory(Guid sessionID);// UC 3.7
        string GetStoreInformationByID(Guid sessionID, Guid storeID);

        /// <summary>
        /// Add Item
        /// </summary>
        /// <param name="sessionID"></param>
        /// <param name="storeID"></param>
        /// <param name="name"></param>
        /// <param name="amount"></param>
        /// <param name="price"></param>
        /// <param name="categories">array of categories as json</param>
        /// <param name="keyWords">NOT mendatory: array of keyWords as json</param>
        /// <returns>jsonType: Item's fields</returns>
        public string AddItem(Guid sessionID, Guid storeID, string name, int amount, double price, string categories, string keyWords = null);// UC 4.1

        public string DeleteItem(Guid sessionID, Guid storeID, Guid itemId);// UC 4.1
        public string GetStoresWithPermissions(Guid sessionID);//UC 2.4 + service requirment 3

        /// <summary>
        /// Edit Item's details: not all details mandatory.
        /// Optional params should be  written null if decided not to be passed.
        /// Params that have default value null in signature will get null if not provided
        /// </summary>
        /// <param name="sessionID"></param>
        /// <param name="storeID"></param>
        /// <param name="itemId"></param>
        /// <param name="amount"></param>
        /// <param name="rank"></param>
        /// <param name="price"></param>
        /// <param name="name"></param>
        /// <param name="categories"></param>
        /// <param name="keyWords"></param>
        /// <returns>jsonType: true/false</returns>
        public string EditItem(
            Guid sessionID,
            Guid storeID,
            Guid itemId,
            int? amount,
            double? rank,
            double? price,
            string name = null,
            string categories = null,
            string keyWords = null);// UC 4.1

        public string AppointOwner(Guid sessionID, Guid storeID, string username);// UC 4.3

         public string GetMyMessages(Guid sessionID);

        /// <summary>
        /// RemoveOwner: For Next Version: UC 4.4
        /// </summary>
        /// <param name="sessionID"></param>
        /// <param name="storeID"></param>
        /// <param name="username"></param>
        /// <returns></returns>
        public string RemoveOwner(Guid sessionID, Guid storeID, string username);// UC 4.4

        public string AppointManager(Guid sessionID, Guid storeID, string username);// UC 4.5
        public string HasMessages(Guid sessionID);

        /// <summary>
        /// Add Permission
        /// Permissions options:
        /// INVENTORY, POLICY, APPOINT_OWNER, REMOVE_OWNER, APPOINT_MANAGER, EDIT_PERMISSIONS, REMOVE_MANAGER,CLOSE_STORE, REQUESTS, HISTORY
        /// </summary>
        /// <param name="sessionID"></param>
        /// <param name="storeID"></param>
        /// <param name="username"></param>
        /// <param name="permission"></param>
        /// <returns>jsonType: true/false</returns>
        public string AddPermission(Guid sessionID, Guid storeID, string username, string permission);// UC 4.6

        /// <summary>
        /// Remove Permission
        /// Permissions options:
        /// INVENTORY, POLICY, APPOINT_OWNER, REMOVE_OWNER, APPOINT_MANAGER, EDIT_PERMISSIONS, REMOVE_MANAGER,CLOSE_STORE, REQUESTS, HISTORY 
        /// </summary>
        /// <param name="sessionID"></param>
        /// <param name="storeID"></param>
        /// <param name="username"></param>
        /// <param name="permission"></param>
        /// <returns>jsonType: true/false</returns>
        public string RemovePermission(Guid sessionID, Guid storeID, string username, string permission);// UC 4.6

        public string RemoveManager(Guid sessionID, Guid storeID, string username);// UC 4.7

        /// <summary>
		/// Get permmision of a user
		/// </summary>
		/// <param name="sessionID"></param>
		/// <param name="storeID"></param>
		/// <returns>jsonType: list of permission</returns>
		public string GetMyPermissions(Guid sessionID, Guid storeID);
        public string GetStoreOrderHistory(Guid sessionID, Guid storeID);// UC 4.10
        public string GetStoreOrderHistoryAdmin(Guid sessionID, Guid storeID);// UC 6.4
        public string GetUserOrderHistory(Guid sessionID, string username);//UC 6.4
        public string AreExternalSystemsConnected();
        bool IsPermitedOperation(string username, Guid storeID, string permission);
        bool IsManagerOfStore(string username, Guid storeID);
        bool IsAdmin(Guid sessionID);
        string AddOpenDiscount(Guid sessionID, Guid storeID, Guid itemID, double discount, int durationInDays);
        string AddItemConditionalDiscount_MinItems_ToDiscountOnAll(Guid sessionID, Guid storeID, Guid itemID, int durationInDays, int minItems, double discount);
        string AddItemConditionalDiscount_MinItems_ToDiscountOnExtraItems(Guid sessionID, Guid storeID, Guid itemID, int durationInDays, int minItems, int extraItems, double discountForExtra);
        string AddStoreConditionalDiscount(Guid sessionID, Guid storeID, int durationInDays, double minPurchase, double discount);
        bool DiscountExistsInStore(Guid storeID, Guid discountID);
        string ComposeTwoDiscounts(Guid sessionID, Guid storeID, Guid discountLeftID, Guid discountRightID, string boolOperator);
        string GetAllDicsounts(Guid sessionID, Guid storeID, Guid? itemID);
        string RemoveDiscount(Guid sessionID, Guid storeID, Guid discountID);
        bool IsDiscountTypeAllowed(Guid storeID, string discountTypeString);
        string MakeDiscountNotAllowed(Guid sessionID, Guid storeID, string discountTypeString);
        string MakeDiscountAllowed(Guid sessionID, Guid storeID, string discountTypeString);
        string GetAllowedDiscounts(Guid sessionID, Guid storeID);
        bool IsValidToCreateStoreConditionalDiscount (Guid storeID);
        bool IsValidToCreateItemOpenedDiscount( Guid storeID, Guid itemID);
        bool IsValidToCreateItemConditionalDiscount( Guid storeID, Guid itemID);
        bool IsRegisteredUser(string username);
        string ApproveOwnerContract(Guid sessionID, Guid storeID, string username);
        bool IsApproverOfContract(Guid sessionID, Guid storeID, string username);
        bool IsAwaitingContractApproval(Guid storeID, string username);
        bool IsValidToCreateItemMinMaxPurchasePolicy(Guid storeID, Guid itemID);
        string AddItemMinMaxPurchasePolicy(Guid sessionID, Guid storeID, Guid itemID, int? minAmount, int? maxAmount);
        bool IsValidToCreateStoreMinMaxPurchasePolicy(Guid storeID);
        string AddStoreMinMaxPurchasePolicy(Guid sessionID, Guid storeID, int? minAmount, int? maxAmount);
        bool IsValidToCreateDaysNotAllowedPurchasePolicy(Guid storeID);
        string AddDaysNotAllowedPurchasePolicy(Guid sessionID, Guid storeID, int[] daysNotAllowed);
        string ComposeTwoPurchasePolicys(Guid sessionID, Guid storeID, Guid policyLeftID, Guid policyRightID, string boolOperator);
        bool IsPurchaseTypeAllowed(Guid storeID, string purchasePolicy);
        string MakePurcahsePolicyNotAllowed(Guid sessionID, Guid storeID, string purchasePolicy);
        string MakePurcahsePolicyAllowed(Guid sessionID, Guid storeID, string purchasePolicy);
        bool PurchasePolicyExistsInStore(Guid storeID, Guid policyID);
        string RemovePurchasePolicy(Guid sessionID, Guid storeID, Guid policyID);
        string GetAllPurchasePolicys(Guid sessionID, Guid storeID);
        string GetAllowedPurchasePolicys(Guid sessionID, Guid storeID);

        string GetDailyStatistics(Guid sessionID, DateTime from, DateTime? to = null);
    }


}

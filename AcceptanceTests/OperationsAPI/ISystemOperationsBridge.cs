using System;
using System.Collections.Generic;
using AcceptanceTests.DataObjects;

namespace AcceptanceTests.OperationsAPI
{
    public interface ISystemOperationsBridge
    {
        User getNewUserForTests();
        //creates store and returns it for current logged in user as owner
        Store getNewStoreForTestsWithOwner();
        bool loginSystemAdmin();


        //assigns store manager by logged in owner
        bool assignStoreManagerByOwner(Guid storeID, User manager);

        bool removeStoreManagerRights(Store store, User manager);

        bool removeStoreOwnerByOwner(Guid storeToRemoveFromID, User userToRemove);

        //must be logged in as owner/ manager with edit permissions right
        bool isManagerOfStore(Store store, User manager);

        bool isOwnerOfStore(Store store, User user);

        //storeOwner here is used to add the items in the first place
        //then buy from current logged in user
        PurchaseRecord addRandomPurchase(Store store, User storeOwner, bool ownerLogedin);

        bool startupWithValidation(string userName, string pw);

        bool startupWithInitFileAndValidation(string adminsUsername, string adminPassword, string path);

        List<PurchaseRecord> getStoreHistory(Store store );

        public List<PurchaseRecord> getStoreHistoryByAdmin(Store store);

        bool? changeItemPrice(Store store, Item item, double price);

        //function must validate system state after execution
        bool? registerNewUser(string userName, string pw);

        //add item to store (user must be logged in as store owner) and return it
        Item addItemToStore(Guid storeID, Item itemToAdd, int amount);

        //add item to store (user must be logged in as store owner) and return bool according to validation
        bool addItemToStoreWithValidation(Guid storeToAddToID, Item productToAdd);

        //delete item from store (user must be logged in as store owner) and return bool according to validation
        bool deleteItemFromStoreWithValidation(Guid storeToDeleteFromID, Item productToDelete);

        bool deleteItemFromStoreWithValidation(Guid storeToDeleteFromID, Guid productToDeleteID);

        //update item in store with same guid but change properties according to productToUpdate
        //(user must be logged in as store owner) and return bool according to validation
        bool updateItemFromStoreWithValidation(Guid storeToDeleteFromID, Item productToUpdate);

        //assigns userToApoint as owner of storeToAssignToID, with validation.
        bool assignStoreOwnerByOwnerAndValidate_NoNeedForApprovers_FirstAssignment(Guid storeToAssignToID, User userToApoint);

        // edit managerToUpdate permissions.
        public bool addManagerPermission(Guid storeToEditManagerID, User managerToUpdate, string newPermissionName);

        public bool removeManagerPermission(Guid storeToEditManagerID, User managerToUpdate, string newPermissionName);

        //same as getCurrentUserHistory but done by system admin UC 6.4 on a registered user.
        //should throw exception when no permissions.
        List<PurchaseRecord> getUserHistory(string username);



        // function login must validate system state after login
        //must keep user details in session for logout and then login in other functions.
        bool login(string userName, string pw);

        //state after these functions must be retained (i.e if logged in then logged in with same user after)
        //stores generated should be from difeerent user
        List<Store> generateRandomListOfStoresWithItems();

        //must be done in current user state
        List<Store> getAllStoresAndItems();

        //must validate current user cart has these items, check by comparing details and not GUID
        bool? validateCartHas(Item itemToBuywithoutGUID, int amountToBuy);
        

        //state after these functions must be retained (i.e if logged in then logged in with same user after)
        //stores generated should be from difeerent user
        List<Store> makeAndInsertItemsToRandomStores(List<Item> itemsToInsert);

        //must be done from current user state
        public List<Item> searchAllStores(SearchDetail searchDetail);


        //add item from store with the specified amount, method should return success status or throw exception
        //method should use current seesionID / userID without updating
        //method should validate item was added to cart, and only one cart for the store
        bool addItemToCartFromStoreWithAmount(Item itemToBuyWithGUID, Store storeWithGuid, int amountToBuy);

        //geneate the store with the deatail and items, amounts
        //method should make the owner as well as the store
        //user should be logged out at end of method
        Store generateStoreWithItemsAndAmountsWithoutLogin(Store storeToGenerate);

        Tuple<Store, ItemOpenedDiscount> GenerateStoreWithItemAndOpenedDiscountLogin(Store storeToGenerate);
        Dictionary<DateTime, int[]> GetStatisticsSuccess(DateTime from, DateTime? to);

        //use update store cart use case and validate item updated as required, return false if not and exception if exception
        //if update amount is 0 validate deletion from cart
        bool updateItemAmountInCartAndValidateUpdated(Guid storeID, Item itemToUpdate, int amountUpdate);
        void doActionFromGuest();


        //should succeed to buy all items, if not raise.
        //must first get the items GUID by seaching all stores for identicall items without guid.
        bool buyItemsFromDifferentUserAndThenChangeBackToCurrentUser(Item[] itemsToBeBoughtWithoutGUID, int[] amountToBeBought, string[] paymentDetails, string[] deliveryDetails);
        void doActionFromUser(User registeredUser);

        //validate receipt got, inventory updated in stores(get cart first to know what to validate),
        //users order updated if registered, or guest, stores history updated.
        //return true if all validations are true.
        bool buyCartWithValidation(string[] paymentDetails, string[] deliveryDetails);

        List<StoreOrder> BuyCartAsListOfStoreOrders(string[] paymentDetails, string[] deliveryDetails);

        //buy operation which expectes a failure on the payment/delivery details
        //return the error string
        public string buyCartShouldFailOnExternalSystem(string[] paymentDetails, string[] deliveryDetails);

        //check history includes purchase for these items (buy details not guid)
        //check history is persistent for these items i.e logout login check again.
        bool? validateCurrentUserPurchaseHistoryIncludesAndPersistent(Item[] itemsToValidateWithoutGUID, int[] amountToValidate);

        List<Guid> getCurrentUserHistoryOrdersIds();

        //true if success
        bool? logout();
        //validates store exist and user has owning rights on store.
        bool openStore(StoreContactDetails contactDetails);

        bool editManagerPermissions(Guid storeID, User storeManager, List<Tuple<string, bool>> permissions);

        //done as a user operation UC 3.7
        List<PurchaseRecord> getCurrentUserHistory();

        //first generate stores and items, then buys from current user
        //loggedInUser@PRE==loggedInUser@POST
        List<PurchaseRecord> buyRandomItemsFromCurrentUser(int numOfPurchases);

        //changes the items price based on guid.
        //loggedInUser@PRE==loggedInUser@POST
        bool changePricesOfItemsInPurchaseRecord(List<PurchaseRecord> purchasesMade);

        bool deleteItemInCartAndValidateDeleted(Guid storeID, Item itemToDeleteWithGUID);

        ItemOpenedDiscount AddOpenedDiscountSuccess(Guid storeID, Guid itemID, double discount, int durationInDays);

        ItemConditionalDiscountOnAll AddItemConditionalDiscount_MinItems_ToDiscountOnAllSuccess(Guid storeID,
        Guid itemID, int durationInDays, int minItems, double discount);
        string GetStatisticsError(DateTime from, DateTime? to);
        ItemConditionalDiscount_DiscountOnExtraItems AddItemConditionalDiscount_MinItems_ToDiscountOnExtraItemsSuccess(Guid storeID,
           Guid itemID, int durationInDays, int minItems, int extraItems, double discountForExtra);

        StoreConditionalDiscount AddStoreConditionalDiscountSuccess(Guid storeID, int durationInDays, double minPurchase, double discount);

        CompositeTwoDiscounts ComposeTwoDiscountsSuccess(Guid storeID, Guid discount1ID, Guid discount2ID, string boolOperator);

        bool RemoveDiscountSuccess(Guid storeID, Guid discountID);

        List<Discount> GetAllDiscountsSuccess(Guid storeID, Guid? itemID);

        //Same methods but expected failure string

        string AddOpenedDiscountError(Guid storeID, Guid itemID, double discount, int durationInDays);

        string AddItemConditionalDiscount_MinItems_ToDiscountOnAllError(Guid storeID,
        Guid itemID, int durationInDays, int minItems, double discount);

        string AddItemConditionalDiscount_MinItems_ToDiscountOnExtraItemsError(Guid storeID,
           Guid itemID, int durationInDays, int minItems, int extraItems, double discountForExtra);

        string AddStoreConditionalDiscountError(Guid storeID, int durationInDays, double minPurchase, double discount);

        string ComposeTwoDiscountsError(Guid storeID, Guid discount1ID, Guid discount2ID, string boolOperator);

        string RemoveDiscountError(Guid storeID, Guid discountID);

        string GetAllDiscountsError(Guid storeID, Guid? itemID);

        ItemOpenedDiscount[] GeneratedOpenedDiscountOnItemsInStoreWithoutLogin(Guid storeId, Item[] items, double[] percent, int[] durations);

        ItemConditionalDiscountOnAll[] GeneratedConditionAllDiscountOnItemsInStoreWithoutLogin(Guid storeId, Item[] items, double[] percent, int[] durations, int[] minItems);

        StoreConditionalDiscount GenerateStoreConditionalDiscountWithoutLogin(Guid storeId, double minPurchase, double percent, int duration);

        CompositeTwoDiscounts CompositeDiscountWithouLogin(Guid storeId, Guid leftID, Guid rightID, string op);

        bool MakeDiscountNotAllowedSuccess(Guid storeID, string discountTypeString);

        bool MakeDiscountAllowedSuccess(Guid storeID, string discountTypeString);

        List<string> GetAllowedDiscountsSuccess(Guid storeID);

        string MakeDiscountNotAllowedError(Guid storeID, string discountTypeString);

        string MakeDiscountAllowedError(Guid storeID, string discountTypeString);

        string GetAllowedDiscountsError(Guid storeID);

        //gets all stores and permission in the store for current user.
        List<Tuple<Store, List<string>>> GetStoresWithPermissionsSuccess();

        string GetStoresWithPermissionsError();

        List<string> getAllNotificationMessages();

        bool assignStoreOwnerNoValidation(Guid storeToAssignToID, User userToApoint);
        bool approveStoreOwnerAssginment(Guid storeToAssignToID, User userToApoint);
        bool isOwnerOfStore(Guid storeToAssignToID, User userToApoint);
        bool approveStoreOwnerAssginment_by_approver(Guid storeToAssignToID, User userToApprove, User approver_not_logged_in);
        bool userHasMessages(User userToRemove);
        bool MakePolicyNotAllowedSuccess(Guid storeID, string policyTypeString);
        string MakePolicyNotAllowedError(Guid storeID, string policyTypeString);
        bool MakePolicyAllowedSuccess(Guid storeID, string policyTypeString);
        string MakePolicyAllowedError(Guid storeID, string policyTypeString);
        List<string> GetAllowedPurchasePoliciesSuccess(Guid storeID);
        void GenerateItemMinMaxPolicyInStoreWithoutLogin(Guid storeID, Item[] items, int?[] min, int?[] max);
        bool checkoutAndReuturnIfSucceed();
        void GenerateStoreMinMaxPolicyInStoreWithoutLogin(Guid storeID, int? min, int? max);
        Guid CreateDaysNotAllowedPolicyWithoutLogin(Guid storeID, int[] daysNotAllowed);
        bool removePolicySuccessWithoutLogin(Guid storeID, Guid policyID);
        Guid CreateItemMinMaxPolicyWithoutLogin(Guid storeID, Item item, int? min, int? max);
        Guid CreateCompositePolicyWithoutLogin(Guid storeID, Guid policyLeft, Guid policyRight, string opString);
        Guid AddStoreMinMaxPolicyWithoutLogin(Guid storeID, int? min, int? max);
        List<string> getAllNotificationMessages_without_login(User user);
    }
}

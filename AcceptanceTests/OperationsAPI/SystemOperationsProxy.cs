using System;
using System.Collections.Generic;
using AcceptanceTests.DataObjects;
using NUnit.Framework;

namespace AcceptanceTests.OperationsAPI
{
    public class SystemOperationsProxy : ISystemOperationsBridge

    {
        private ISystemOperationsBridge realBridge;

        public SystemOperationsProxy()
        {
            realBridge = new RealSystemOperations();
        }

        public bool addItemToCartFromStoreWithAmount(Item itemToBuyWithGUID, Store storeWithGuid, int amountToBuy)
        {
            if (realBridge != null)
                return realBridge.addItemToCartFromStoreWithAmount(itemToBuyWithGUID, storeWithGuid, amountToBuy);
            return false;
        }

        public bool editManagerPermissions(Guid storeID, User storeManager, List<Tuple<string, bool>> permissions)
        {
            if (realBridge != null)
                return realBridge.editManagerPermissions(storeID, storeManager, permissions);
            return false;
        }



        public Item addItemToStore(Guid storeID, Item itemToAdd, int amount)
        {
            if (realBridge != null)
                return realBridge.addItemToStore(storeID, itemToAdd, amount);
            return null;
        }

        public bool addItemToStoreWithValidation(Guid storeToAddToID, Item productToAdd)
        {
            if (realBridge != null)
                return realBridge.addItemToStoreWithValidation(storeToAddToID, productToAdd);
            return false;
        }

        public PurchaseRecord addRandomPurchase(Store store, User storeOwner, bool ownerLoggedIn)
        {
            if (realBridge != null)
                return realBridge.addRandomPurchase(store, storeOwner, ownerLoggedIn);
            return null;
        }

        public bool assignStoreManagerByOwner(Guid storeToAssignToID, User userToApoint)
        {
            if (realBridge != null)
                return realBridge.assignStoreManagerByOwner(storeToAssignToID, userToApoint);
            return false;
        }

        public bool assignStoreOwnerByOwnerAndValidate_NoNeedForApprovers_FirstAssignment(Guid storeToAssignToID, User userToApoint)
        {
            if (realBridge != null)
                return realBridge.assignStoreOwnerByOwnerAndValidate_NoNeedForApprovers_FirstAssignment(storeToAssignToID, userToApoint);
            return false;
        }

        public bool buyCartWithValidation(string[] paymentDetails, string[] deliveryDetails)
        {
            if (realBridge != null)
                return realBridge.buyCartWithValidation(paymentDetails, deliveryDetails);
            return false;
        }

        public string buyCartShouldFailOnExternalSystem(string[] paymentDetails, string[] deliveryDetails)
        {
            if (realBridge != null)
            {
                return realBridge.buyCartShouldFailOnExternalSystem(paymentDetails, deliveryDetails);
            }
            return "";
        }

        public bool buyItemsFromDifferentUserAndThenChangeBackToCurrentUser(Item[] itemsToBeBoughtWithoutGUID, int[] amountToBeBought, string[] paymentDetails, string[] deliveryDetails)
        {
            if (realBridge != null)
                return realBridge.buyItemsFromDifferentUserAndThenChangeBackToCurrentUser(itemsToBeBoughtWithoutGUID, amountToBeBought, paymentDetails, deliveryDetails);
            return false;
        }

        public List<PurchaseRecord> buyRandomItemsFromCurrentUser(int numOfPurchases)
        {
            if (realBridge != null)
                return realBridge.buyRandomItemsFromCurrentUser(numOfPurchases);
            return null;
        }

        public bool changePricesOfItemsInPurchaseRecord(List<PurchaseRecord> purchasesMade)
        {
            if (realBridge != null)
                return realBridge.changePricesOfItemsInPurchaseRecord(purchasesMade);
            return false;
        }

        public bool deleteItemFromStoreWithValidation(Guid storeToDeleteFromID, Item productToDelete)
        {
            if (realBridge != null)
                return realBridge.deleteItemFromStoreWithValidation(storeToDeleteFromID, productToDelete);
            return false;
        }

        public bool deleteItemFromStoreWithValidation(Guid storeToDeleteFromID, Guid productToDeleteID)
        {
            if (realBridge != null)
                return realBridge.deleteItemFromStoreWithValidation(storeToDeleteFromID, productToDeleteID);
            return false;
        }



        public List<Store> generateRandomListOfStoresWithItems()
        {
            if (realBridge != null)
                return realBridge.generateRandomListOfStoresWithItems();
            return null;
        }

        public Store generateStoreWithItemsAndAmountsWithoutLogin(Store storeToGenerate)
        {
            if (realBridge != null)
                return realBridge.generateStoreWithItemsAndAmountsWithoutLogin(storeToGenerate);
            return null;
        }

        public List<Store> getAllStoresAndItems()
        {
            if (realBridge != null)
                return realBridge.getAllStoresAndItems();
            return null;
        }

        public List<PurchaseRecord> getCurrentUserHistory()
        {
            if (realBridge != null)
                return realBridge.getCurrentUserHistory();
            return null;
        }

        public Store getNewStoreForTestsWithOwner()
        {
            if (realBridge != null)
                return realBridge.getNewStoreForTestsWithOwner();
            return null;
        }

        public User getNewUserForTests()
        {
            if (realBridge != null)
                return realBridge.getNewUserForTests();
            return null;
        }


        public List<PurchaseRecord> getStoreHistory(Store store)
        {
            if (realBridge != null)
                return realBridge.getStoreHistory(store);
            return null;
        }

        public List<PurchaseRecord> getStoreHistoryByAdmin(Store store)
        {
            if (realBridge != null)
                return realBridge.getStoreHistoryByAdmin(store);
            return null;
        }


        public List<PurchaseRecord> getUserHistory(string username)
        {
            if (realBridge != null)
                return realBridge.getUserHistory(username);
            return null;
        }

        public bool isManagerOfStore(Store store, User manager)
        {
            if (realBridge != null)
                return realBridge.isManagerOfStore(store, manager);
            return false;
        }


        public bool isOwnerOfStore(Store store, User user)
        {
            if (realBridge != null)
                return realBridge.isOwnerOfStore(store, user);
            return false;
        }


        public bool login(string userName, string pw)
        {
            if (realBridge != null)
                return realBridge.login(userName, pw);
            return false;
        }

        public bool loginSystemAdmin()
        {
            if (realBridge != null)
                return realBridge.loginSystemAdmin();
            return false;
        }

        public bool? logout()
        {
            if (realBridge != null)
                return realBridge.logout();
            return false;
        }

        public List<Store> makeAndInsertItemsToRandomStores(List<Item> itemsToInsert)
        {
            if (realBridge != null)
                return realBridge.makeAndInsertItemsToRandomStores(itemsToInsert);
            return null;
        }

        public bool openStore(StoreContactDetails contactDetails)
        {
            if (realBridge != null)
                return realBridge.openStore(contactDetails);
            return false;
        }

        public bool? registerNewUser(string userName, string pw)
        {
            if (realBridge != null)
                return realBridge.registerNewUser(userName, pw);
            return false;
        }

        public bool removeStoreManagerRights(Store store, User manager)
        {
            if (realBridge != null)
                return realBridge.removeStoreManagerRights(store, manager);
            return false;
        }

        public bool removeStoreOwnerByOwner(Guid storeToRemoveFromID, User userToRemove)
        {
            if (realBridge != null)
                return realBridge.removeStoreOwnerByOwner(storeToRemoveFromID, userToRemove);
            return false;
        }

        public List<Item> searchAllStores(SearchDetail searchDetail)
        {
            if (realBridge != null)
                return realBridge.searchAllStores(searchDetail);
            return null;
        }

        public bool updateItemFromStoreWithValidation(Guid storeToDeleteFromID, Item productToUpdate)
        {
            if (realBridge != null)
                return realBridge.updateItemFromStoreWithValidation(storeToDeleteFromID, productToUpdate);
            return false;
        }

        public bool updateItemAmountInCartAndValidateUpdated(Guid storeID, Item itemToUpdate, int amountUpdate)
        {
            if (realBridge != null)
                return realBridge.updateItemAmountInCartAndValidateUpdated(storeID, itemToUpdate, amountUpdate);
            return false;
        }

        public bool? validateCartHas(Item itemToBuywithoutGUID, int amountToBuy)
        {
            if (realBridge != null)
                return realBridge.validateCartHas(itemToBuywithoutGUID, amountToBuy);
            return false;
        }

        public bool? validateCurrentUserPurchaseHistoryIncludesAndPersistent(Item[] itemsToValidateWithoutGUID, int[] amountToValidate)
        {
            if (realBridge != null)
                return realBridge.validateCurrentUserPurchaseHistoryIncludesAndPersistent(itemsToValidateWithoutGUID, amountToValidate);
            return false;
        }

        public List<Guid> getCurrentUserHistoryOrdersIds()
        {
            if (realBridge != null)
            {
                return realBridge.getCurrentUserHistoryOrdersIds();
            }

            return null;
        }

        public bool? changeItemPrice(Store store, Item item, double price)
        {
            if (realBridge != null)
                return realBridge.changeItemPrice(store, item, price);
            return false;
        }

        public bool startupWithValidation(string userName, string pw)
        {
            if (realBridge != null)
                return realBridge.startupWithValidation(userName, pw);
            return false;
        }

        public bool startupWithInitFileAndValidation(string adminsUsername, string adminPassword, string path)
        {
            if (realBridge != null)
                return realBridge.startupWithInitFileAndValidation(adminsUsername, adminPassword, path);
            return false;
        }

        public bool deleteItemInCartAndValidateDeleted(Guid storeID, Item itemToDeleteWithGUID)
        {
            if (realBridge != null)
                return realBridge.deleteItemInCartAndValidateDeleted(storeID, itemToDeleteWithGUID);
            return false;
        }

        public bool addManagerPermission(Guid storeToEditManagerID, User managerToUpdate, string newPermissionName)
        {
            if (realBridge != null)
                return realBridge.addManagerPermission(storeToEditManagerID, managerToUpdate, newPermissionName);
            return false;
        }

        public bool removeManagerPermission(Guid storeToEditManagerID, User managerToUpdate, string newPermissionName)
        {
            if (realBridge != null)
                return realBridge.removeManagerPermission(storeToEditManagerID, managerToUpdate, newPermissionName);
            return false;
        }

        public ItemOpenedDiscount AddOpenedDiscountSuccess(Guid storeID, Guid itemID, double discount, int durationInDays)
        {
            if (realBridge != null)
            {
                return realBridge.AddOpenedDiscountSuccess(storeID, itemID, discount, durationInDays);
            }
            return null;
        }

        public ItemConditionalDiscountOnAll AddItemConditionalDiscount_MinItems_ToDiscountOnAllSuccess(Guid storeID, Guid itemID, int durationInDays, int minItems, double discount)
        {
            if (realBridge != null)
            {
                return realBridge.AddItemConditionalDiscount_MinItems_ToDiscountOnAllSuccess(storeID, itemID, durationInDays, minItems, discount);
            }
            return null;
        }

        public ItemConditionalDiscount_DiscountOnExtraItems AddItemConditionalDiscount_MinItems_ToDiscountOnExtraItemsSuccess(Guid storeID, Guid itemID, int durationInDays, int minItems, int extraItems, double discountForExtra)
        {
            if (realBridge != null)
            {
                return realBridge.AddItemConditionalDiscount_MinItems_ToDiscountOnExtraItemsSuccess(storeID, itemID, durationInDays, minItems, extraItems, discountForExtra);
            }
            return null;
        }

        public StoreConditionalDiscount AddStoreConditionalDiscountSuccess(Guid storeID, int durationInDays, double minPurchase, double discount)
        {
            if (realBridge != null)
            {
                return realBridge.AddStoreConditionalDiscountSuccess(storeID, durationInDays, minPurchase, discount);
            }
            return null;
        }

        public CompositeTwoDiscounts ComposeTwoDiscountsSuccess(Guid storeID, Guid discount1ID, Guid discount2ID, string boolOperator)
        {
            if (realBridge != null)
            {
                return realBridge.ComposeTwoDiscountsSuccess(storeID, discount1ID, discount2ID, boolOperator);
            }
            return null;
        }

        public bool RemoveDiscountSuccess(Guid storeID, Guid discountID)
        {
            if (realBridge != null)
            {
                return realBridge.RemoveDiscountSuccess(storeID, discountID);
            }
            return false;
        }

        public List<Discount> GetAllDiscountsSuccess(Guid storeID, Guid? itemID)
        {
            if (realBridge != null)
            {
                return realBridge.GetAllDiscountsSuccess(storeID, itemID);
            }
            return null;
        }

        public string AddOpenedDiscountError(Guid storeID, Guid itemID, double discount, int durationInDays)
        {
            if (realBridge != null)
            {
                return realBridge.AddOpenedDiscountError(storeID, itemID, discount, durationInDays);
            }
            return "";
        }

        public string AddItemConditionalDiscount_MinItems_ToDiscountOnAllError(Guid storeID, Guid itemID, int durationInDays, int minItems, double discount)
        {
            if (realBridge != null)
            {
                return realBridge.AddItemConditionalDiscount_MinItems_ToDiscountOnAllError(storeID, itemID, durationInDays, minItems, discount);
            }
            return "";
        }

        public string AddItemConditionalDiscount_MinItems_ToDiscountOnExtraItemsError(Guid storeID, Guid itemID, int durationInDays, int minItems, int extraItems, double discountForExtra)
        {
            if (realBridge != null)
            {
                return realBridge.AddItemConditionalDiscount_MinItems_ToDiscountOnExtraItemsError(storeID, itemID, durationInDays, minItems, extraItems, discountForExtra);
            }
            return "";
        }

        public string AddStoreConditionalDiscountError(Guid storeID, int durationInDays, double minPurchase, double discount)
        {
            if (realBridge != null)
            {
                return realBridge.AddStoreConditionalDiscountError(storeID, durationInDays, minPurchase, discount);
            }
            return "";
        }

        public string ComposeTwoDiscountsError(Guid storeID, Guid discount1ID, Guid discount2ID, string boolOperator)
        {
            if (realBridge != null)
            {
                return realBridge.ComposeTwoDiscountsError(storeID, discount1ID, discount2ID, boolOperator);
            }
            return "";
        }

        public string RemoveDiscountError(Guid storeID, Guid discountID)
        {
            if (realBridge != null)
            {
                return realBridge.RemoveDiscountError(storeID, discountID);
            }
            return "";
        }

        public string GetAllDiscountsError(Guid storeID, Guid? itemID)
        {
            if (realBridge != null)
            {
                return realBridge.GetAllDiscountsError(storeID, itemID);
            }
            return "";
        }

        public Tuple<Store, ItemOpenedDiscount> GenerateStoreWithItemAndOpenedDiscountLogin(Store storeToGenerate)
        {
            if (realBridge != null)
            {
                return realBridge.GenerateStoreWithItemAndOpenedDiscountLogin(storeToGenerate);
            }

            return null;
        }

        public ItemOpenedDiscount[] GeneratedOpenedDiscountOnItemsInStoreWithoutLogin(Guid storeId, Item[] items, double[] percent, int[] durations)
        {
            if (realBridge != null)
            {
                return realBridge.GeneratedOpenedDiscountOnItemsInStoreWithoutLogin(storeId, items, percent, durations);
            }

            return null;
        }

        public ItemConditionalDiscountOnAll[] GeneratedConditionAllDiscountOnItemsInStoreWithoutLogin(Guid storeId, Item[] items, double[] percent, int[] durations, int[] minItems)
        {
            if (realBridge != null)
            {
                return realBridge.GeneratedConditionAllDiscountOnItemsInStoreWithoutLogin(storeId, items, percent, durations, minItems);
            }

            return null;
        }

        public CompositeTwoDiscounts CompositeDiscountWithouLogin(Guid storeId, Guid leftID, Guid rightID, string op)
        {
            if (realBridge != null)
            {
                return realBridge.CompositeDiscountWithouLogin(storeId, leftID, rightID, op);
            }

            return null;
        }

        public List<StoreOrder> BuyCartAsListOfStoreOrders(string[] paymentDetails, string[] deliveryDetails)
        {
            if (realBridge != null)
            {
                return realBridge.BuyCartAsListOfStoreOrders(paymentDetails, deliveryDetails);
            }

            return null;
        }

        public StoreConditionalDiscount GenerateStoreConditionalDiscountWithoutLogin(Guid storeId, double minPurchase, double percent, int duration)
        {
            if (realBridge != null)
            {
                return realBridge.GenerateStoreConditionalDiscountWithoutLogin(storeId, minPurchase, percent, duration);

            }
            return null;
        }

        public bool MakeDiscountNotAllowedSuccess(Guid storeID, string discountTypeString)
        {
            if (realBridge != null)
            {
                return realBridge.MakeDiscountNotAllowedSuccess(storeID, discountTypeString);

            }
            return false;
        }

        public bool MakeDiscountAllowedSuccess(Guid storeID, string discountTypeString)
        {
            if (realBridge != null)
            {
                return realBridge.MakeDiscountAllowedSuccess(storeID, discountTypeString);

            }
            return false;
        }

        public List<string> GetAllowedDiscountsSuccess(Guid storeID)
        {
            if (realBridge != null)
            {
                return realBridge.GetAllowedDiscountsSuccess(storeID);

            }
            return null;
        }

        public string MakeDiscountNotAllowedError(Guid storeID, string discountTypeString)
        {
            if (realBridge != null)
            {
                return realBridge.MakeDiscountNotAllowedError(storeID, discountTypeString);

            }
            return "";
        }

        public string MakeDiscountAllowedError(Guid storeID, string discountTypeString)
        {
            if (realBridge != null)
            {
                return realBridge.MakeDiscountAllowedError(storeID, discountTypeString);

            }
            return "";
        }

        public string GetAllowedDiscountsError(Guid storeID)
        {
            if (realBridge != null)
            {
                return realBridge.GetAllowedDiscountsError(storeID);

            }
            return "";
        }

        public List<Tuple<Store, List<string>>> GetStoresWithPermissionsSuccess()
        {
            if (realBridge != null)
            {
                return realBridge.GetStoresWithPermissionsSuccess();
            }
            return null;
        }

        public string GetStoresWithPermissionsError()
        {
            if (realBridge != null)
            {
                return realBridge.GetStoresWithPermissionsError();
            }

            return "";
        }


        public List<string> getAllNotificationMessages()
        {
            if (realBridge != null)
            {
                return realBridge.getAllNotificationMessages();
            }

            return new List<string>();
         }

        public bool assignStoreOwnerNoValidation(Guid storeToAssignToID, User userToApoint)
        {
            if (realBridge != null)
            {
                return realBridge.assignStoreOwnerNoValidation( storeToAssignToID,  userToApoint);
            }

            return false;
        }

        public bool approveStoreOwnerAssginment(Guid storeToAssignToID, User userToApoint)
        {
            if (realBridge != null)
            {
                return realBridge.approveStoreOwnerAssginment( storeToAssignToID,  userToApoint);
            }

            return false;
        }

        public bool isOwnerOfStore(Guid storeToAssignToID, User userToApoint)
        {
            if (realBridge != null)
            {
                return realBridge.isOwnerOfStore( storeToAssignToID,  userToApoint);
            }

            return false;
        }

        public bool approveStoreOwnerAssginment_by_approver(Guid storeToAssignToID, User userToApprove, User approver_not_logged_in)
        {
            if (realBridge != null)
            {
                return realBridge.approveStoreOwnerAssginment_by_approver( storeToAssignToID,  userToApprove,  approver_not_logged_in);
            }

            return false;
        }

        public bool userHasMessages(User userToRemove)
        {
            if (realBridge != null)
            {
                return realBridge.userHasMessages(userToRemove);
            }

            return false;
        }

        public bool MakePolicyNotAllowedSuccess(Guid storeID, string policyTypeString)
        {
            if (realBridge != null)
            {
                return realBridge.MakePolicyNotAllowedSuccess( storeID,  policyTypeString);
            }

            return false;
        }

        public string MakePolicyNotAllowedError(Guid storeID, string policyTypeString)
        {
            if (realBridge != null)
            {
                return realBridge.MakePolicyNotAllowedError(storeID, policyTypeString);
            }

            return "false";
        }

        public bool MakePolicyAllowedSuccess(Guid storeID, string policyTypeString)
        {
            if (realBridge != null)
            {
                return realBridge.MakePolicyAllowedSuccess(storeID, policyTypeString);
            }

            return false;
        }

        public string MakePolicyAllowedError(Guid storeID, string policyTypeString)
        {
            if (realBridge != null)
            {
                return realBridge.MakePolicyAllowedError(storeID, policyTypeString);
            }

            return "false";
        }

        public List<string> GetAllowedPurchasePoliciesSuccess(Guid storeID)
        {
            if (realBridge != null)
            {
                return realBridge.GetAllowedPurchasePoliciesSuccess(storeID);
            }

            return null;
        }

        public void GenerateItemMinMaxPolicyInStoreWithoutLogin(Guid storeID, Item[] items, int?[] min, int?[] max)
        {
            if (realBridge != null)
            {
                realBridge.GenerateItemMinMaxPolicyInStoreWithoutLogin( storeID,  items, min, max);
            }
        }

        public bool checkoutAndReuturnIfSucceed()
        {
            if (realBridge != null)
            {
                return realBridge.checkoutAndReuturnIfSucceed();
            }

            return false;
        }

        public void GenerateStoreMinMaxPolicyInStoreWithoutLogin(Guid storeID, int? min, int? max)
        {
            if (realBridge != null)
            {
                realBridge.GenerateStoreMinMaxPolicyInStoreWithoutLogin(storeID, min, max);
            }
        }

        public Guid CreateDaysNotAllowedPolicyWithoutLogin(Guid storeID, int[] daysNotAllowed)
        {
            if (realBridge != null)
            {
                return realBridge.CreateDaysNotAllowedPolicyWithoutLogin(storeID, daysNotAllowed);
            }
            return Guid.Empty;
        }

        public bool removePolicySuccessWithoutLogin(Guid storeID, Guid policyID)
        {
            if (realBridge != null)
            {
                return realBridge.removePolicySuccessWithoutLogin(storeID, policyID);
            }
            return false;
        }

        public Guid CreateItemMinMaxPolicyWithoutLogin(Guid storeID, Item item, int? min, int? max)
        {
            if (realBridge != null)
            {
                return realBridge.CreateItemMinMaxPolicyWithoutLogin(storeID, item, min, max);
            }
            return Guid.Empty;
        }

        public Guid CreateCompositePolicyWithoutLogin(Guid storeID, Guid policyLeft, Guid policyRight, string opString)
        {
            if (realBridge != null)
            {
                return realBridge.CreateCompositePolicyWithoutLogin(storeID, policyLeft, policyRight, opString);
            }
            return Guid.Empty;
        }

        public Guid AddStoreMinMaxPolicyWithoutLogin(Guid storeID, int? min, int? max)
        {
            if (realBridge != null)
            {
                return realBridge.AddStoreMinMaxPolicyWithoutLogin(storeID, min, max);
            }
            return Guid.Empty;
        }


        public List<string> getAllNotificationMessages_without_login(User user)
        {
            if (realBridge != null)
            {
                return realBridge.getAllNotificationMessages_without_login(user);
            }
            return null;
        }

        public Dictionary<DateTime, int[]> GetStatisticsSuccess(DateTime from, DateTime? to)
        {
            if (realBridge != null)
            {
                return realBridge.GetStatisticsSuccess(from, to);
            }
            return null;
        }

        public void doActionFromGuest()
        {
            if (realBridge != null)
            {
                realBridge.doActionFromGuest();
                return;
            }
            Assert.Fail();
        }

        public void doActionFromUser(User user)
        {
            if (realBridge != null)
            {
                realBridge.doActionFromUser(user);
                return;
            }
            Assert.Fail();
        }

        public string GetStatisticsError(DateTime from, DateTime? to)
        {
            if (realBridge != null)
            {
                return realBridge.GetStatisticsError(from, to);
            }
            return null;
        }
    }
}

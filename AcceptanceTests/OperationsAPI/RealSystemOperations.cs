using System;
using System.Collections.Generic;
using AcceptanceTests.DataObjects;
using ServiceLayer;
using Newtonsoft.Json;
using ServiceLayer.Services;
using NUnit.Framework;
using DomainLayer.NotificationsCenter;
using NotificationsManagment;

namespace AcceptanceTests.OperationsAPI
{
    public class RealSystemOperations : ISystemOperationsBridge

    {
        private Guid sessionId;
        private User ownerOfAllNonRelatedStores;
        private StoreActions storeActions;
        private PurchaseActions purchaseActions;
        private UserActions userActions;
        private NotificationManager communication;
        private User admin = new User("admin", "qwerty");

        public RealSystemOperations()
        {
            Assert.True(initializeParams(admin.Username, admin.Pw));
            sessionId = Guid.NewGuid();
            ownerOfAllNonRelatedStores = getNewUserForTests();

        }
        private bool? initializeParams(string username, string pw, string init_file_loc=null)
        {
            storeActions = null;
            purchaseActions = null;
            userActions = null;
            string init = SetUp.Init(username, pw, init_file_loc, out communication, out purchaseActions, out userActions, out storeActions, true);
            bool? ret = SystemObjJsonConveter.boolFromJson(init);
            return ret;
        }
        //-------systemOperations-----

        public bool startupWithValidation(string adminUserName, string adminPassword)
        {
            bool? ret = initializeParams(adminUserName, adminPassword);
            if (ret is bool r && r)
                return true;
            else
                return false;

        }

        public bool startupWithInitFileAndValidation(string adminUserName, string adminPassword, string path)
        {
            bool? ret = initializeParams(adminUserName, adminPassword, path);
            if (ret is bool r && r)
                return true;
            else
                return false;
        }

        //------system admin operations------

        public bool loginSystemAdmin()
        {
            return login(admin.Username, admin.Pw);
        }

        public List<PurchaseRecord> getUserHistory(string username)
        {
            string historyJson = purchaseActions.GetUserOrderHistory(sessionId, username);
            return SystemObjJsonConveter.purchaseRecordsFromUserHistory(historyJson);
        }

        public List<PurchaseRecord> getStoreHistoryByAdmin(Store store)
        {
            string historyJson = storeActions.GetStoreOrderHistoryAdmin(sessionId, store.Id);
            return SystemObjJsonConveter.purchaseRecordsFromStoreHistory(historyJson);
        }




        //----------store managment-----------
        public bool assignStoreManagerByOwner(Guid storeID, User manager)
        {
            bool? ans = SystemObjJsonConveter.boolFromJson(storeActions.AppointManager(sessionId, storeID, manager.Username));
            if (ans is bool a && a)
            {
                return isManagerOfStore(storeID, manager);
            }
            return false;
        }


        public bool removeStoreManagerRights(Store store, User manager)
        {
            bool? ans = SystemObjJsonConveter.boolFromJson(storeActions.RemoveManager(sessionId, store.Id, manager.Username));
            if (ans is bool a && a)
            {
                return !isManagerOfStore(store, manager);
            }
            return false;
        }

        public bool removeStoreOwnerByOwner(Guid storeID, User owner)
        {
            bool? ans = SystemObjJsonConveter.boolFromJson(storeActions.RemoveOwner(sessionId, storeID, owner.Username));
            if (ans is bool a && a)
            {
                return !isOwnerOfStore(storeID, owner);
            }
            return false;
        }

        public bool isManagerOfStore(Store store, User manager)
        {
            return isManagerOfStore(store.Id, manager);
        }
        public bool isOwnerOfStore(Store store, User user)
        {
            return isOwnerOfStore(store.Id, user);
        }

      

        public bool isManagerOfStore(Guid storeID, User manager)
        {
            Guid sessionBefore = sessionId;
            sessionId = Guid.NewGuid();
            this.loginInternal(manager.Username, manager.Pw);
            StorePermissions storePermissions = SystemObjJsonConveter.permissionsFromJson(storeActions.GetMyPermissions(sessionId, storeID));
            logout();
            sessionId = sessionBefore;
            return storePermissions.isManagingRights();
        }

        public bool isOwnerOfStore(Guid storeID, User owner)
        {
            Guid sessionBefore = sessionId;
            sessionId = Guid.NewGuid();
            bool? logged_in_success = this.loginInternal(owner.Username, owner.Pw);
            if (logged_in_success is bool t && !t)
                sessionId = sessionBefore;
            StorePermissions storePermissions = SystemObjJsonConveter.permissionsFromJson(storeActions.GetMyPermissions(sessionId, storeID));
            if (logged_in_success is bool t2 && t2)
                logout();
            sessionId = sessionBefore;
            return storePermissions.isOwnerRights();
        }


        public List<PurchaseRecord> getStoreHistory(Store store)
        {
            String retJson = storeActions.GetStoreOrderHistory(sessionId, store.Id);
            return SystemObjJsonConveter.purchaseRecordsFromStoreHistory(retJson);
        }




        public Item addItemToStore(Guid storeID, Item itemToAdd, int amount)
        {
            return SystemObjJsonConveter.itemFromJson(storeActions.AddItem(sessionId, storeID, itemToAdd.Name,
                amount, JsonConvert.SerializeObject(itemToAdd.Categorys.ToArray()),
                itemToAdd.Price, JsonConvert.SerializeObject(itemToAdd.Keywords.ToArray())));
        }


        public bool deleteItemFromStoreWithValidation(Guid storeToDeleteFromID, Item productToDelete)
        {
            return deleteItemFromStoreWithValidation(storeToDeleteFromID, productToDelete.Id);
        }

        public bool deleteItemFromStoreWithValidation(Guid storeToDeleteFromID, Guid productToDeleteID)
        {
            bool? ans = SystemObjJsonConveter.boolFromJson(storeActions.DeleteItem(
                sessionId, storeToDeleteFromID, productToDeleteID));
            if (ans is bool a && a)
            {
                return !ItemExistInStore(storeToDeleteFromID, productToDeleteID);
            }
            return false;
        }

        private bool ItemExistInStore(Guid storeID, Guid productID)
        {
            List<Store> allStoresWithItems = getAllStoresAndItems();
            foreach (Store store in allStoresWithItems)
            {
                if (store.Id.Equals(storeID))
                {
                    return store.HasItemByGuid(productID);
                }
            }
            return false;
        }

        public bool updateItemFromStoreWithValidation(Guid storeToUpdateFromID, Item productToUpdate)
        {
            Item item = SystemObjJsonConveter.itemFromJson(storeActions.EditItem(sessionId, storeToUpdateFromID, productToUpdate.Id,
                null, 0, productToUpdate.Price, productToUpdate.Name,
                JsonConvert.SerializeObject(productToUpdate.Categorys.ToArray()),
                JsonConvert.SerializeObject(productToUpdate.Keywords.ToArray())));

            if (item != null)
                return item.isEqualWithoutGUID(productToUpdate) && item.Rank == 0;
            return false;
        }

        /// <summary>
        /// assigns and validates.
        /// </summary>
        /// <param name="storeToAssignToID"></param>
        /// <param name="userToApoint"></param>
        /// <returns></returns>
        public bool assignStoreOwnerByOwnerAndValidate_NoNeedForApprovers_FirstAssignment(Guid storeToAssignToID, User userToApoint)
        {
            bool? ans = SystemObjJsonConveter.boolFromJson(storeActions.AppointOwner(
                sessionId, storeToAssignToID, userToApoint.Username));
            if (ans is bool a && a)
            {
                Guid sessionBefore = sessionId;
                sessionId = Guid.NewGuid();
                loginInternal(userToApoint.Username, userToApoint.Pw);
                StorePermissions permissions = SystemObjJsonConveter.permissionsFromJson(
                    storeActions.GetMyPermissions(sessionId, storeToAssignToID));
                bool ret = permissions.isOwnerRights();
                logout();
                sessionId = sessionBefore;
                return ret;
            }

            return false;
        }

        public bool assignStoreOwnerNoValidation(Guid storeToAssignToID, User userToApoint)
        {
            return SystemObjJsonConveter.ansFromJson(storeActions.AppointOwner(
                sessionId, storeToAssignToID, userToApoint.Username));
        }

        public bool approveStoreOwnerAssginment(Guid storeToAssignToID, User userToApprove)
        {
            return SystemObjJsonConveter.ansFromJson(storeActions.ApproveOwnerContract(
                sessionId, storeToAssignToID, userToApprove.Username));
        }
        public bool approveStoreOwnerAssginment_by_approver(Guid storeToAssignToID, User userToApprove, User approver_not_logged_in)
        {
            Guid sessionBefore = sessionId;
            sessionId = Guid.NewGuid();
            Assert.True(loginInternal(approver_not_logged_in.Username, approver_not_logged_in.Pw));
            bool ans =  SystemObjJsonConveter.ansFromJson(storeActions.ApproveOwnerContract(
                sessionId, storeToAssignToID, userToApprove.Username));
            logout();
            sessionId = sessionBefore;
            return ans;
        }


        public bool addItemToStoreWithValidation(Guid storeToAddToID, Item productToAdd)
        {

            Item item = addItemToStore(storeToAddToID, productToAdd, 5);
            if (item != null)
            {
                return ItemExistInStore(storeToAddToID, item.Id) &&
                    item.isEqualWithoutGUID(productToAdd);
            }
            return false;

        }

        public bool editManagerPermissions(Guid storeToEditManagerID, User managerToUpdate, List<Tuple<string, bool>> permissions)
        {
            foreach (Tuple<string, bool> perm in permissions)
            {
                if (perm.Item2)
                {
                    if (!addManagerPermission(storeToEditManagerID, managerToUpdate, perm.Item1))
                    {
                        return false;
                    }
                }

                if (!perm.Item2)
                {
                    if (!removeManagerPermission(storeToEditManagerID, managerToUpdate, perm.Item1))
                    {
                        return false;
                    }
                }
            }
            return true; ;
        }

        public bool addManagerPermission(Guid storeToEditManagerID, User managerToUpdate, string PermissionName)
        {
            bool? ans = SystemObjJsonConveter.boolFromJson(storeActions.AddPermission(
                sessionId, storeToEditManagerID, managerToUpdate.Username, PermissionName));
            return ans switch
            {
                bool a when a => managerHasPermission(storeToEditManagerID, managerToUpdate, PermissionName),
                _ => false
            };
        }

        private bool managerHasPermission(Guid storeID, User managerToUpdate, string permissionName)
        {
            Guid sessionBefore = sessionId;
            sessionId = Guid.NewGuid();
            loginInternal(managerToUpdate.Username, managerToUpdate.Pw);
            StorePermissions storePermissions = SystemObjJsonConveter.permissionsFromJson(
                storeActions.GetMyPermissions(sessionId, storeID));
            bool ret = storePermissions.hasPermission(permissionName);
            logout();
            sessionId = sessionBefore;
            return ret;
        }

        public bool removeManagerPermission(Guid storeToEditManagerID, User managerToUpdate, string PermissionName)
        {
            bool? ans = SystemObjJsonConveter.boolFromJson(storeActions.RemovePermission(
                sessionId, storeToEditManagerID, managerToUpdate.Username, PermissionName));
            if (ans is bool a && a)
                return !managerHasPermission(storeToEditManagerID, managerToUpdate, PermissionName);
            return false;
        }

        /// <returns>null if error</returns>
        public Store openStoreAndGet(StoreContactDetails contactDetails)
        {
            string storeJson = storeActions.OpenStore(sessionId, contactDetails.Name, contactDetails.Email, contactDetails.Address,
                contactDetails.Phone, contactDetails.BankAccountNumber, contactDetails.Bank, contactDetails.Description,
                null, null);
            return SystemObjJsonConveter.jsonToStore(storeJson);
        }

        /// <summary>
        /// validates store exists and user has owning rights in store
        /// </summary>
        /// <param name="contactDetails"></param>
        /// <returns></returns>
        public bool openStore(StoreContactDetails contactDetails)
        {

            Store store = openStoreAndGet(contactDetails);
            if (store == null)
                return false;
            StorePermissions permissions = SystemObjJsonConveter.permissionsFromJson(
                storeActions.GetMyPermissions(sessionId, store.Id));
            if (permissions == null)
                return false;
            return permissions.isOwnerRights();
        }




        //----------user functions-----------
        public bool? registerNewUser(string userName, string pw)
        {
            return SystemObjJsonConveter.boolFromJson(userActions.Register(sessionId, userName, pw));
        }

        public bool login(string userName, string pw)
        {
            bool? succes = SystemObjJsonConveter.boolFromJson(userActions.Login(sessionId, userName, pw));
            if (succes is bool su)
            {
                return su;
            }
            return false;
        }

        private bool? loginInternal(string userName, string pw)
        {
            return SystemObjJsonConveter.boolFromJson(userActions.Login(sessionId, userName, pw));
        }

        public bool? logout()
        {
            return SystemObjJsonConveter.boolFromJson(userActions.Logout(sessionId));
        }


        public List<Store> getAllStoresAndItems()
        {
            string json = storeActions.GetAllStoresInformation(sessionId);
            return SystemObjJsonConveter.storesWithItemsListFromJson(json);
        }

        public List<Item> searchAllStores(SearchDetail searchDetail)
        {
            string itemsJson = storeActions.SearchItems(sessionId, searchDetail.FilterItemRank, searchDetail.FilterMinPrice,
                searchDetail.FilterMaxPrice, searchDetail.FilterStoreRank, searchDetail.Name, searchDetail.Category,
                JsonConvert.SerializeObject(searchDetail.Keywords));

            return SystemObjJsonConveter.searchJsonToList(itemsJson);
        }

        public List<PurchaseRecord> getCurrentUserHistory()
        {
            string historyJson = userActions.GetMyOrderHistory(sessionId);
            return SystemObjJsonConveter.purchaseRecordsFromUserHistory(historyJson);
        }

        public List<Guid> getCurrentUserHistoryOrdersIds()
        {
            string historyJson = userActions.GetMyOrderHistory(sessionId);
            return SystemObjJsonConveter.ordersIdFromUserHistory(historyJson);
        }


        //--------cart--------

        public Dictionary<Item, int> getUserCart()
        {
            string cartJson = userActions.ViewCart(sessionId);
            return SystemObjJsonConveter.cartJsonToDict(cartJson);
        }

        // should be deleted if amount == 0
        public bool updateItemAmountInCartAndValidateUpdated(Guid storeID, Item itemToUpdate, int amountUpdate)
        {
            userActions.ChangeItemAmountInCart(sessionId, storeID, itemToUpdate.Id, amountUpdate);
            Dictionary<Item, int> cart = getUserCart();

            if (cart.ContainsKey(itemToUpdate))
                return cart[itemToUpdate] == amountUpdate;
            else
                return false;
        }

        public bool deleteItemInCartAndValidateDeleted(Guid storeID, Item itemToDelete)
        {
            bool? ans = SystemObjJsonConveter.boolFromJson(userActions.RemoveFromCart(sessionId, storeID, itemToDelete.Id));
            if (ans == null || ans == false)
            {
                return false;
            }
            Dictionary<Item, int> cart = getUserCart();
            return !cart.ContainsKey(itemToDelete);
        }

        public bool? validateCartHas(Item itemToBuywithoutGUID, int amountToBuy)
        {
            Dictionary<Item, int> cart = getUserCart();
            foreach (KeyValuePair<Item, int> keyValuePair in cart)
            {
                if (itemToBuywithoutGUID.isEqualWithoutGUID(keyValuePair.Key))
                {
                    return amountToBuy == keyValuePair.Value;
                }
            }
            return false;
        }

        public bool addItemToCartFromStoreWithAmount(Item itemToBuyWithGUID, Store storeWithGuid, int amountToBuy)
        {
            bool? res = SystemObjJsonConveter.boolFromJson(userActions.AddToCart(
                sessionId, storeWithGuid.Id, itemToBuyWithGUID.Id, amountToBuy));
            if (res != null && (bool)res)
            {
                bool? has = validateCartHas(itemToBuyWithGUID, amountToBuy);
                return has != null;
            }
            return false;
        }


        public List<PurchaseRecord> ShowCartSummeryBeforeBuy()
        {

            string boughtJson = purchaseActions.DisplayBeforeCheckout(sessionId);
            return SystemObjJsonConveter.boughtCartJsonToPuchaseRecordList(boughtJson);
        }

        public string BuyCart(Guid orderID,
                                string cardNum, string expire, string CCV, string cardOwnerName, string cardOwnerID,
                                string address, string city, string country, string zipCode)
        {
            string receipt = purchaseActions.CheckOut(sessionId, orderID, cardNum, expire, CCV,cardOwnerName, cardOwnerID, address, city, country, zipCode);//TODO: info
            return receipt; //TODO: get from json receipt
        }


        //------- tests utility functions--------

        public User getNewUserForTests()
        {
            User u = new User(Utilitys.Utils.RandomAlphaNumericString(8), "password");
            Guid sessionBefore = sessionId;
            sessionId = Guid.NewGuid();
            bool? res = registerNewUser(u.Username, u.Pw);
            if (res == null || !(bool)res)
            {
                u = null;
            }
            sessionId = sessionBefore;
            return u;
        }

        public bool userHasMessages(User user)
        {
            Guid sessionBefore = sessionId;
            sessionId = Guid.NewGuid();
            loginInternal(user.Username, user.Pw);
            bool ans = SystemObjJsonConveter.ansFromJson(userActions.HasMessages(sessionId));
            logout();
            sessionId = sessionBefore;
            return ans;

        }

        public Store getNewStoreForTestsWithOwner()
        {
            StoreContactDetails contactDetails = Utilitys.Utils.RandomContactDetails();
            return openStoreAndGet(contactDetails);
        }

        /// <summary>
        /// changes price to random price, amount changed to 5
        /// done with user that own on non relevant stores.
        /// </summary>
        /// <param name="purchasesMade"></param>
        /// <returns></returns>
        public bool changePricesOfItemsInPurchaseRecord(List<PurchaseRecord> purchasesMade)
        {
            Random rnd = new Random();
            Guid sessionBefore = sessionId;
            sessionId = Guid.NewGuid();


            loginInternal(ownerOfAllNonRelatedStores.Username, ownerOfAllNonRelatedStores.Pw);
            bool ans = true;
            foreach (PurchaseRecord purchase in purchasesMade)
            {
                bool? res = changeItemPrice(purchase.storeID, purchase.ItemObj, rnd.Next(20, 100));
                if (res == null || res == false)
                {
                    ans = false;
                    break;
                }
            }

            logout();
            sessionId = sessionBefore;
            return ans;
        }


        public PurchaseRecord addRandomPurchase(Store store, User storeOwner, bool ownerLogedIn)
        {
            Guid sessionBefore = sessionId;
            if (!ownerLogedIn)
            {
                sessionId = Guid.NewGuid();
                loginInternal(storeOwner.Username, storeOwner.Pw);
            }

            Item itemNoGuid = Utilitys.Utils.RandomItem();
            Item itemWithGuid = addItemToStore(store.Id, itemNoGuid, 5);

            if (!ownerLogedIn)
            {
                logout();
                sessionId = sessionBefore;
            }


            addItemToCartFromStoreWithAmount(itemWithGuid, store, 1);

            PurchaseRecord record = ShowCartSummeryBeforeBuy()[0];
            string[][] detailsForPaymentAndDelivery = Utilitys.Utils.PaymentAndDeliveryDetailsValidExample();
            string receipt = BuyCart(record.OrderId,
                detailsForPaymentAndDelivery[0][0],
                detailsForPaymentAndDelivery[0][1],
                detailsForPaymentAndDelivery[0][2],
                detailsForPaymentAndDelivery[0][3],
                detailsForPaymentAndDelivery[0][4],
                detailsForPaymentAndDelivery[1][0],
                detailsForPaymentAndDelivery[1][1],
                detailsForPaymentAndDelivery[1][2],
                detailsForPaymentAndDelivery[1][3]);
            return record;
        }



        public List<PurchaseRecord> buyRandomItemsFromCurrentUser(int numOfPurchases)
        {
            Guid sessionBefore = sessionId;
            sessionId = Guid.NewGuid();
            loginInternal(ownerOfAllNonRelatedStores.Username, ownerOfAllNonRelatedStores.Pw);

            List<Store> stores = new List<Store>();
            for (int i = 0; i < numOfPurchases; i++)
            {
                stores.Add(getNewStoreForTestsWithOwner());
            }
            logout();
            sessionId = sessionBefore;

            List<PurchaseRecord> purchaseRecords = new List<PurchaseRecord>();
            for (int i = 0; i < numOfPurchases; i++)
            {

                PurchaseRecord purchase = addRandomPurchase(stores[i], ownerOfAllNonRelatedStores, false);
                if (purchase != null)
                {
                    purchaseRecords.Add(purchase);
                }
                else
                {
                    throw new Exception("add random purchase should always succeed");
                }
            }
            return purchaseRecords;
        }

        /// <summary>
        /// state before and after must be the same,
        /// stores generated should be from different user
        /// </summary>
        /// <param name="itemsToInsert"></param>
        /// <returns>list of generated stores with the items in them</returns>
        public List<Store> makeAndInsertItemsToRandomStores(List<Item> itemsToInsert)
        {
            Random rnd = new Random();
            int amountStoresToGenerate = rnd.Next(3, 8);
            List<Store> toGenerate = new List<Store>();
            for (int i = 0; i < amountStoresToGenerate; i++)
            {
                toGenerate.Add(Utilitys.Utils.RandomStoreNoItems());
            }
            for (int i = 0; i < itemsToInsert.Count; i++)
            {
                int storeToInsertToIndex = rnd.Next(0, toGenerate.Count);
                toGenerate[storeToInsertToIndex].ItemsWithAmount[itemsToInsert[i]] = rnd.Next(5, 10);
            }
            List<Store> generated = new List<Store>();
            foreach (Store toGenerateStore in toGenerate)
            {
                generated.Add(generateStoreWithItemsAndAmountsWithoutLogin(toGenerateStore));
            }

            return generated;
        }

        /// <summary>
        /// state before and after must be the same,
        /// stores generated should be from different user
        /// </summary>
        /// <returns>list of generated stores with the items in them</returns>
        public List<Store> generateRandomListOfStoresWithItems()
        {
            Random rnd = new Random();
            int amountToGenerate = rnd.Next(3, 8);
            List<Store> generated = new List<Store>();
            for (int i = 0; i < amountToGenerate; i++)
            {
                generated.Add(generateStoreWithItemsAndAmountsWithoutLogin(Utilitys.Utils.RandomStoreWithItemsAndAmount()));
            }

            return generated;
        }
        /// <summary>
        /// state before and after must be the same,
        /// store generated should be from different user
        /// </summary>
        /// <param name="storeToGenerate"></param>
        /// <returns>generated store with items in it</returns>
        public Store generateStoreWithItemsAndAmountsWithoutLogin(Store storeToGenerate)
        {
            Guid sessionBefore = sessionId;
            sessionId = Guid.NewGuid();
            loginInternal(ownerOfAllNonRelatedStores.Username, ownerOfAllNonRelatedStores.Pw);

            StoreContactDetails contactDetails = Utilitys.Utils.RandomContactDetailsWithName(storeToGenerate.Name);
            Store generated = openStoreAndGet(contactDetails);
            foreach (KeyValuePair<Item, int> itemToAmount in storeToGenerate.ItemsWithAmount)
            {
                generated.ItemsWithAmount[addItemToStore(generated.Id, itemToAmount.Key, itemToAmount.Value)] = itemToAmount.Value;
            }

            logout();
            sessionId = sessionBefore;

            return generated;
        }

        public Tuple<Store, ItemOpenedDiscount> GenerateStoreWithItemAndOpenedDiscountLogin(Store storeToGenerate)
        {
            Store s = generateStoreWithItemsAndAmountsWithoutLogin(storeToGenerate);
            ItemOpenedDiscount d = null;

            Guid sessionBefore = sessionId;
            sessionId = Guid.NewGuid();
            loginInternal(ownerOfAllNonRelatedStores.Username, ownerOfAllNonRelatedStores.Pw);

            List<Store> stores = getAllStoresAndItems();
            s = findStoreInList(s.Id, stores);

            foreach (KeyValuePair<Item, int> itemToAmount in s.ItemsWithAmount)
            {
                d = AddOpenedDiscountSuccess(s.Id, itemToAmount.Key.Id, 0.3, 15);
                break;
            }

            logout();
            sessionId = sessionBefore;

            return new Tuple<Store, ItemOpenedDiscount>(s, d);
        }

        public ItemOpenedDiscount[] GeneratedOpenedDiscountOnItemsInStoreWithoutLogin(Guid storeId, Item[] items, double[] percent, int[] durations)
        {
            Guid sessionBefore = sessionId;
            sessionId = Guid.NewGuid();
            loginInternal(ownerOfAllNonRelatedStores.Username, ownerOfAllNonRelatedStores.Pw);

            ItemOpenedDiscount[] discounts = new ItemOpenedDiscount[items.Length];
            for (int i = 0; i < items.Length; i++)
            {
                discounts[i] = AddOpenedDiscountSuccess(storeId, items[i].Id, percent[i], durations[i]);
            }

            logout();
            sessionId = sessionBefore;

            return discounts;
        }
        public void GenerateItemMinMaxPolicyInStoreWithoutLogin(Guid storeID, Item[] items, int?[] min, int?[] max)
        {
            Guid sessionBefore = sessionId;
            sessionId = Guid.NewGuid();
            loginInternal(ownerOfAllNonRelatedStores.Username, ownerOfAllNonRelatedStores.Pw);

            for (int i = 0; i < items.Length; i++)
            {
                Assert.True(AddItemMinMaxSuccess(storeID, items[i].Id, min[i], max[i]));
            }

            logout();
            sessionId = sessionBefore;
        }

        public Guid CreateItemMinMaxPolicyWithoutLogin(Guid storeID, Item item, int? min, int? max)
        {
            Guid sessionBefore = sessionId;
            sessionId = Guid.NewGuid();
            loginInternal(ownerOfAllNonRelatedStores.Username, ownerOfAllNonRelatedStores.Pw);


            string json = storeActions.AddItemMinMaxPurchasePolicy(sessionId, storeID, item.Id, min, max);
            

            logout();
            sessionId = sessionBefore;

            return SystemObjJsonConveter.GuidFromPolicySuccess(json);
        }

        public Guid CreateCompositePolicyWithoutLogin(Guid storeID, Guid policyLeft, Guid policyRight, string opString)
        {
            Guid sessionBefore = sessionId;
            sessionId = Guid.NewGuid();
            loginInternal(ownerOfAllNonRelatedStores.Username, ownerOfAllNonRelatedStores.Pw);


            string json = storeActions.ComposeTwoPurchasePolicies(sessionId, storeID, policyLeft, policyRight, opString);


            logout();
            sessionId = sessionBefore;

            return SystemObjJsonConveter.GuidFromPolicySuccess(json);
        }

        private bool AddItemMinMaxSuccess(Guid storeID, Guid itemID, int? min, int? max)
        {
            string json = storeActions.AddItemMinMaxPurchasePolicy(sessionId, storeID, itemID, min, max);
            return SystemObjJsonConveter.operationSuccededOrNotFromJson(json);
        }

        public void GenerateStoreMinMaxPolicyInStoreWithoutLogin(Guid storeID, int? min, int? max)
        {
            Guid sessionBefore = sessionId;
            sessionId = Guid.NewGuid();
            loginInternal(ownerOfAllNonRelatedStores.Username, ownerOfAllNonRelatedStores.Pw);

           
            Assert.True(AddStoreMinMaxSuccess(storeID, min, max));

            logout();
            sessionId = sessionBefore;
        }

        public Guid AddStoreMinMaxPolicyWithoutLogin(Guid storeID, int? min, int? max)
        {
            Guid sessionBefore = sessionId;
            sessionId = Guid.NewGuid();
            loginInternal(ownerOfAllNonRelatedStores.Username, ownerOfAllNonRelatedStores.Pw);


            string json = storeActions.AddStoreMinMaxPurchasePolicy(sessionId, storeID, min, max);

            logout();
            sessionId = sessionBefore;

            return SystemObjJsonConveter.GuidFromPolicySuccess(json);
        }

        private bool? AddStoreMinMaxSuccess(Guid storeID, int? min, int? max)
        {
            string json = storeActions.AddStoreMinMaxPurchasePolicy(sessionId, storeID, min, max);
            return SystemObjJsonConveter.operationSuccededOrNotFromJson(json);
        }

        public bool checkoutAndReuturnIfSucceed()
        {
            string boughtJson = purchaseActions.DisplayBeforeCheckout(sessionId);
            return SystemObjJsonConveter.operationSuccededOrNotFromJson(boughtJson);
        }

        public Guid CreateDaysNotAllowedPolicyWithoutLogin(Guid storeID, int[] daysNotAllowed)
        {
            Guid sessionBefore = sessionId;
            sessionId = Guid.NewGuid();
            loginInternal(ownerOfAllNonRelatedStores.Username, ownerOfAllNonRelatedStores.Pw);


            string json = storeActions.AddDaysNotAllowedPurchasePolicy(sessionId, storeID, daysNotAllowed);

            logout();
            sessionId = sessionBefore;

            
            return SystemObjJsonConveter.GuidFromPolicySuccess(json);
        }

        public bool removePolicySuccessWithoutLogin(Guid storeID, Guid policyID)
        {
            Guid sessionBefore = sessionId;
            sessionId = Guid.NewGuid();
            loginInternal(ownerOfAllNonRelatedStores.Username, ownerOfAllNonRelatedStores.Pw);


            string json = storeActions.RemovePurchasePolicy(sessionId, storeID, policyID);

            logout();
            sessionId = sessionBefore;

            
            return SystemObjJsonConveter.ansFromJsonSuccess(json);
        }

        public ItemConditionalDiscountOnAll[] GeneratedConditionAllDiscountOnItemsInStoreWithoutLogin(Guid storeId, Item[] items, double[] percent, int[] durations, int[] minItems)
        {
            Guid sessionBefore = sessionId;
            sessionId = Guid.NewGuid();
            loginInternal(ownerOfAllNonRelatedStores.Username, ownerOfAllNonRelatedStores.Pw);

            ItemConditionalDiscountOnAll[] discounts = new ItemConditionalDiscountOnAll[items.Length];
            for (int i = 0; i < items.Length; i++)
            {
                discounts[i] = AddItemConditionalDiscount_MinItems_ToDiscountOnAllSuccess(storeId, items[i].Id, durations[i], minItems[i], percent[i]);
            }

            logout();
            sessionId = sessionBefore;

            return discounts;
        }

        public StoreConditionalDiscount GenerateStoreConditionalDiscountWithoutLogin(Guid storeId, double minPurchase, double percent, int duration)
        {
            Guid sessionBefore = sessionId;
            sessionId = Guid.NewGuid();
            loginInternal(ownerOfAllNonRelatedStores.Username, ownerOfAllNonRelatedStores.Pw);

            StoreConditionalDiscount storeDiscount = AddStoreConditionalDiscountSuccess(storeId, duration, minPurchase, percent);
            logout();
            sessionId = sessionBefore;

            return storeDiscount;
        }

        public CompositeTwoDiscounts CompositeDiscountWithouLogin(Guid storeId, Guid leftID, Guid rightID, string op)
        {
            Guid sessionBefore = sessionId;
            sessionId = Guid.NewGuid();
            loginInternal(ownerOfAllNonRelatedStores.Username, ownerOfAllNonRelatedStores.Pw);

            CompositeTwoDiscounts result = ComposeTwoDiscountsSuccess(storeId, leftID, rightID, op);

            logout();
            sessionId = sessionBefore;

            return result;
        }

        private Store findStoreInList(Guid id, List<Store> stores)
        {
            foreach (Store s in stores)
            {
                if (id.Equals(s.Id))
                {
                    return s;
                }
            }

            return null;
        }

        /// <summary>
        /// switchs to different user, buy all itemstoBeBought, then returns to current user
        /// should succeed to buy all items
        /// should first search for all items then buy them.
        /// assumes store with items exist, with enough amount
        /// if not should raise exception
        /// </summary>
        /// <param name="itemsToBeBoughtWithoutGUID"></param>
        /// <param name="amountToBeBought"></param>
        /// <returns></returns>
        public bool buyItemsFromDifferentUserAndThenChangeBackToCurrentUser(Item[] itemsToBeBoughtWithoutGUID, int[] amountToBeBought, string[] paymentDetails, string[] deliveryDetails)
        {
            if (itemsToBeBoughtWithoutGUID.Length == 0)
            {
                return true;
            }

            Guid sessionBefore = sessionId;
            sessionId = Guid.NewGuid();
            loginInternal(ownerOfAllNonRelatedStores.Username, ownerOfAllNonRelatedStores.Pw);

            List<Store> allStores = getAllStoresAndItems();

            //addTocart
            for (int i = 0; i < itemsToBeBoughtWithoutGUID.Length; i++)
            {
                foreach (Store store in allStores)
                {
                    if (store.HasItemNotByGuid(itemsToBeBoughtWithoutGUID[i]))
                    {
                        Item toBuy = store.GetItemThatMatchesItemNotByGuid(itemsToBeBoughtWithoutGUID[i]);
                        bool succes = addItemToCartFromStoreWithAmount(toBuy, store, amountToBeBought[i]);
                        if (!succes)
                        {
                            logout();
                            sessionId = sessionBefore;
                            throw new ArgumentException();
                        }
                    }

                }
            }
            List<PurchaseRecord> bought = ShowCartSummeryBeforeBuy();

            if (bought == null || bought.Count == 0)
            {
                logout();
                sessionId = sessionBefore;
                throw new ArgumentException();
            }

            string receipt = BuyCart(bought[0].OrderId,
                              paymentDetails[0], paymentDetails[1], paymentDetails[2],paymentDetails[3],paymentDetails[4],
                              deliveryDetails[0], deliveryDetails[1], deliveryDetails[2], deliveryDetails[3]);

            if (!SystemObjJsonConveter.operationSuccededOrNotFromJson(receipt))
            {
                logout();
                sessionId = sessionBefore;
                throw new ArgumentException();
            }

            logout();
            sessionId = sessionBefore;

            return true;
        }

        /// <summary>
        /// validates purchase history include all items with their respective amounts
        /// and that the price on purchase is consistent
        /// </summary>
        /// <param name="itemsToValidateWithoutGUID"></param>
        /// <param name="amountToValidate"></param>
        /// <returns></returns>
        public bool? validateCurrentUserPurchaseHistoryIncludesAndPersistent(Item[] itemsToValidateWithoutGUID, int[] amountToValidate)
        {
            List<PurchaseRecord> purchaseRecords = getCurrentUserHistory();
            for (int i = 0; i < itemsToValidateWithoutGUID.Length; i++)
            {
                bool validated = false;
                foreach (PurchaseRecord purchase in purchaseRecords)
                {
                    if (purchase.Name.Equals(itemsToValidateWithoutGUID[i].Name))
                    {
                        if (amountToValidate[i] == purchase.amountBought)
                        {
                            validated = true;//TODO: validation on all or only one? @TOMER
                        }
                    }
                }
                if (!validated)
                {
                    return false;
                }
            }
            return true;
        }


        /// <summary>
        /// changes price of item, changes amount to 5.
        /// </summary>
        /// <param name="store"></param>
        /// <param name="item"></param>
        /// <param name="price"></param>
        /// <returns></returns>
        public bool? changeItemPrice(Store store, Item item, double price)
        {
            bool? ans = changeItemPrice(store.Id, item, price);
            return ans;
        }

        public bool? changeItemPrice(Guid storeID, Item item, double price)
        {

            Item res = SystemObjJsonConveter.itemFromJson(storeActions.EditItem(
                sessionId, storeID, item.Id, null, item.Rank, item.Price, item.Name,
                JsonConvert.SerializeObject(item.Categorys.ToArray()),
                JsonConvert.SerializeObject(item.Keywords.ToArray())));
            if (res == null)
                return false;
            return res.Price == price;
        }



        /// <summary>
        /// get cart first to know what to validate
        /// get stores inventory in order to validate with
        /// validate stores inventory are updated
        /// loggedInUser@pre == loggedInUser@post
        /// </summary>
        /// <param name="legalPayDetails"></param>
        /// <returns>true if all is true</returns>
        public bool buyCartWithValidation(string[] paymentDetails, string[] deliveryDetails)
        {
            Dictionary<Item, int> userCart = getUserCart();
            List<Store> storesWithInventoryBefore = getAllStoresAndItems();

            List<PurchaseRecord> purchaseRecords = ShowCartSummeryBeforeBuy();


            //validate ret val = cart
            if (purchaseRecords is null || purchaseRecords.Count != userCart.Keys.Count)
                return false;
            foreach (KeyValuePair<Item, int> itemToAmountCart in userCart)
            {
                bool bought = false;
                foreach (PurchaseRecord purchase in purchaseRecords)
                {
                    if (purchase.ItemId.Equals(itemToAmountCart.Key.Id) && purchase.amountBought == itemToAmountCart.Value)
                        bought = true;
                }
                if (!bought)
                    return false;
            }

            string receipt = BuyCart(purchaseRecords[0].OrderId,
                  paymentDetails[0], paymentDetails[1], paymentDetails[2],paymentDetails[3],paymentDetails[4],
                  deliveryDetails[0], deliveryDetails[1], deliveryDetails[2], deliveryDetails[3]);

            if (!SystemObjJsonConveter.operationSuccededOrNotFromJson(receipt))
            {
                return false;
            }


            //validate store inventory
            List<Store> storesWithInventoryAfter = getAllStoresAndItems();
            foreach (PurchaseRecord purchaseRecord in purchaseRecords)
            {
                Store storeWithThisItemBeforePurchase = getStoreWithItemFromList(storesWithInventoryBefore, purchaseRecord.ItemId);
                Store storeWithThisItemAfterPurchase = getStoreWithIdFromList(storesWithInventoryAfter, storeWithThisItemBeforePurchase.Id);

                if (storeWithThisItemBeforePurchase.GetAmountOfItemByGuid(purchaseRecord.ItemId) == purchaseRecord.amountBought)
                    if (storeWithThisItemAfterPurchase.GetAmountOfItemByGuid(purchaseRecord.ItemId) != 0)
                        return false;
                    else
                        return storeWithThisItemBeforePurchase.GetAmountOfItemByGuid(purchaseRecord.ItemId) -
                            purchaseRecord.amountBought == storeWithThisItemAfterPurchase.GetAmountOfItemByGuid(purchaseRecord.ItemId);
            }

            //validate cart empty
            Dictionary<Item, int> userCartAfter = getUserCart();
            if (userCartAfter != null && userCartAfter.Count != 0)
                return false;

            return true;
        }

        public string buyCartShouldFailOnExternalSystem(string[] paymentDetails, string[] deliveryDetails)
        {
            Dictionary<Item, int> userCart = getUserCart();
            List<PurchaseRecord> purchaseRecords = ShowCartSummeryBeforeBuy();


            if (purchaseRecords is null || purchaseRecords.Count != userCart.Keys.Count)
            {
                throw new ArgumentException();
            }

            string receiptInJson = BuyCart(purchaseRecords[0].OrderId,
            paymentDetails[0], paymentDetails[1], paymentDetails[2],paymentDetails[3],paymentDetails[4],
            deliveryDetails[0], deliveryDetails[1], deliveryDetails[2], deliveryDetails[3]);

            string error = SystemObjJsonConveter.errorFromFailedOperationFromJson(receiptInJson);

            return error;
        }

        private Store getStoreWithIdFromList(List<Store> storesWithInventory, Guid storeID)
        {
            foreach (Store store in storesWithInventory)
            {
                if (store.Id.Equals(storeID))
                    return store;
            }
            return null;
        }

        private Store getStoreWithItemFromList(List<Store> storesWithInventoryBefore, Guid itemId)
        {
            foreach (Store store in storesWithInventoryBefore)
            {
                if (store.GetItemThatMatchesByGuid(itemId) != null)
                    return store;
            }
            return null;
        }

        public ItemOpenedDiscount AddOpenedDiscountSuccess(Guid storeID, Guid itemID, double discount, int durationInDays)
        {
            string json = storeActions.AddOpenDiscount(sessionId, storeID, itemID, discount, durationInDays);
            return SystemObjJsonConveter.OpenedDiscountFromJson(json);
        }

        public ItemConditionalDiscountOnAll AddItemConditionalDiscount_MinItems_ToDiscountOnAllSuccess(Guid storeID, Guid itemID, int durationInDays, int minItems, double discount)
        {
            string json = storeActions.AddItemConditionalDiscount_MinItems_ToDiscountOnAll(sessionId, storeID, itemID, durationInDays, minItems, discount);
            return SystemObjJsonConveter.ItemConditionalDiscountOnAllFromJson(json);
        }

        public ItemConditionalDiscount_DiscountOnExtraItems AddItemConditionalDiscount_MinItems_ToDiscountOnExtraItemsSuccess(Guid storeID, Guid itemID, int durationInDays, int minItems, int extraItems, double discountForExtra)
        {
            string json = storeActions.AddItemConditionalDiscount_MinItems_ToDiscountOnExtraItems(sessionId, storeID, itemID, durationInDays, minItems, extraItems, discountForExtra);
            return SystemObjJsonConveter.ItemConditionalDiscount_DiscountOnExtraItemsFromJson(json);
        }

        public StoreConditionalDiscount AddStoreConditionalDiscountSuccess(Guid storeID, int durationInDays, double minPurchase, double discount)
        {
            string json = storeActions.AddStoreConditionalDiscount(sessionId, storeID, durationInDays, minPurchase, discount);
            return SystemObjJsonConveter.StoreConditionalDiscountFromJson(json);
        }

        public CompositeTwoDiscounts ComposeTwoDiscountsSuccess(Guid storeID, Guid discount1ID, Guid discount2ID, string boolOperator)
        {
            string json = storeActions.ComposeTwoDiscounts(sessionId, storeID, discount1ID, discount2ID, boolOperator);
            return SystemObjJsonConveter.CompositeTwoDiscountsFromJson(json);
        }

        public bool RemoveDiscountSuccess(Guid storeID, Guid discountID)
        {
            string json = storeActions.RemoveDiscount(sessionId, storeID, discountID);
            bool? ans = SystemObjJsonConveter.boolFromJson(json);
            if (ans is bool a && a)
            {
                return ans.Value;
            }
            return false;
        }

        public List<Discount> GetAllDiscountsSuccess(Guid storeID, Guid? itemID)
        {
            string json = storeActions.GetAllDiscounts(sessionId, storeID, itemID);
            //TODO: return SystemObjJsonConveter
            return null;
        }

        //Error scenarions expected
        public string AddOpenedDiscountError(Guid storeID, Guid itemID, double discount, int durationInDays)
        {
            string json = storeActions.AddOpenDiscount(sessionId, storeID, itemID, discount, durationInDays);
            string error = SystemObjJsonConveter.errorFromFailedOperationFromJson(json);

            return error;
        }

        public string AddItemConditionalDiscount_MinItems_ToDiscountOnAllError(Guid storeID, Guid itemID, int durationInDays, int minItems, double discount)
        {
            string json = storeActions.AddItemConditionalDiscount_MinItems_ToDiscountOnAll(sessionId, storeID, itemID, durationInDays, minItems, discount);
            string error = SystemObjJsonConveter.errorFromFailedOperationFromJson(json);

            return error;
        }

        public string AddItemConditionalDiscount_MinItems_ToDiscountOnExtraItemsError(Guid storeID, Guid itemID, int durationInDays, int minItems, int extraItems, double discountForExtra)
        {
            string json = storeActions.AddItemConditionalDiscount_MinItems_ToDiscountOnExtraItems(sessionId, storeID, itemID, durationInDays, minItems, extraItems, discountForExtra);
            string error = SystemObjJsonConveter.errorFromFailedOperationFromJson(json);

            return error;
        }

        public string AddStoreConditionalDiscountError(Guid storeID, int durationInDays, double minPurchase, double discount)
        {
            string json = storeActions.AddStoreConditionalDiscount(sessionId, storeID, durationInDays, minPurchase, discount);
            string error = SystemObjJsonConveter.errorFromFailedOperationFromJson(json);

            return error;
        }

        public string ComposeTwoDiscountsError(Guid storeID, Guid discount1ID, Guid discount2ID, string boolOperator)
        {
            string json = storeActions.ComposeTwoDiscounts(sessionId, storeID, discount1ID, discount2ID, boolOperator);
            string error = SystemObjJsonConveter.errorFromFailedOperationFromJson(json);

            return error;
        }

        public string RemoveDiscountError(Guid storeID, Guid discountID)
        {
            string json = storeActions.RemoveDiscount(sessionId, storeID, discountID);
            string error = SystemObjJsonConveter.errorFromFailedOperationFromJson(json);

            return error;
        }

        public string GetAllDiscountsError(Guid storeID, Guid? itemID)
        {
            throw new NotImplementedException();
        }

        public List<StoreOrder> BuyCartAsListOfStoreOrders(string[] paymentDetails, string[] deliveryDetails)
        {
            List<StoreOrder> storeOrders = ShowCartSummeryBeforeBuyAsListOfStoreOrders(); //change method

            string receipt = BuyCart(storeOrders[0].OrderId,
                 paymentDetails[0], paymentDetails[1], paymentDetails[2],paymentDetails[3],paymentDetails[4],
                 deliveryDetails[0], deliveryDetails[1], deliveryDetails[2], deliveryDetails[3]);

            if (!SystemObjJsonConveter.operationSuccededOrNotFromJson(receipt))
            {
                throw new ArgumentException();
            }

            return storeOrders;
        }

        private List<StoreOrder> ShowCartSummeryBeforeBuyAsListOfStoreOrders()
        {
            string boughtJson = purchaseActions.DisplayBeforeCheckout(sessionId);
            return SystemObjJsonConveter.BoughtCartJsonToStoreOrderList(boughtJson);
        }

        public bool MakeDiscountNotAllowedSuccess(Guid storeID, string discountTypeString)
        {
            string answer = storeActions.MakeDiscountNotAllowed(sessionId, storeID, discountTypeString);

            bool? ans = SystemObjJsonConveter.boolFromJson(answer);
            if (ans is bool a && a)
            {
                return a;
            }
            return false;
        }

        public bool MakeDiscountAllowedSuccess(Guid storeID, string discountTypeString)
        {
            string answer = storeActions.MakeDiscountAllowed(sessionId, storeID, discountTypeString);

            bool? ans = SystemObjJsonConveter.boolFromJson(answer);
            if (ans is bool a && a)
            {
                return a;
            }
            return false;
        }

        public List<string> GetAllowedDiscountsSuccess(Guid storeID)
        {
            string answer = storeActions.GetAllowedDiscounts(sessionId, storeID);

            return SystemObjJsonConveter.stringsListFromJson(answer);
        }

        public string MakeDiscountNotAllowedError(Guid storeID, string discountTypeString)
        {
            string answer = storeActions.MakeDiscountNotAllowed(sessionId, storeID, discountTypeString);

            return SystemObjJsonConveter.errorFromFailedOperationFromJson(answer);
        }

        public string MakeDiscountAllowedError(Guid storeID, string discountTypeString)
        {
            string answer = storeActions.MakeDiscountAllowed(sessionId, storeID, discountTypeString);

            return SystemObjJsonConveter.errorFromFailedOperationFromJson(answer);
        }

        public string GetAllowedDiscountsError(Guid storeID)
        {
            string answer = storeActions.GetAllowedDiscounts(sessionId, storeID);

            return SystemObjJsonConveter.errorFromFailedOperationFromJson(answer);
        }

        public bool MakePolicyNotAllowedSuccess(Guid storeID, string policyTypeString)
        {
            string answer = storeActions.MakePurcahsePolicyNotAllowed(sessionId, storeID, policyTypeString);

            bool? ans = SystemObjJsonConveter.boolFromJson(answer);
            if (ans is bool a && a)
            {
                return a;
            }
            return false;
        }

        public string MakePolicyNotAllowedError(Guid storeID, string policyTypeString)
        {
            string answer = storeActions.MakePurcahsePolicyNotAllowed(sessionId, storeID, policyTypeString);

            return SystemObjJsonConveter.errorFromFailedOperationFromJson(answer);
        }

        public bool MakePolicyAllowedSuccess(Guid storeID, string policyTypeString)
        {
            string answer = storeActions.MakePurcahsePolicyAllowed(sessionId, storeID, policyTypeString);

            bool? ans = SystemObjJsonConveter.boolFromJson(answer);
            if (ans is bool a && a)
            {
                return a;
            }
            return false;
        }

        public string MakePolicyAllowedError(Guid storeID, string policyTypeString)
        {
            string answer = storeActions.MakePurcahsePolicyAllowed(sessionId, storeID, policyTypeString);

            return SystemObjJsonConveter.errorFromFailedOperationFromJson(answer);
        }

        public List<string> GetAllowedPurchasePoliciesSuccess(Guid storeID)
        {
            string json = storeActions.GetAllowedPurchasePolicys(sessionId, storeID);
            return SystemObjJsonConveter.stringsListFromJson(json);
        }

        public List<Tuple<Store, List<string>>> GetStoresWithPermissionsSuccess()
        {
            string answer = storeActions.GetStoresWithPermissions(sessionId);

            return SystemObjJsonConveter.StoresWithPermissionsSuccessFromJson(answer);
        }

        public string GetStoresWithPermissionsError()
        {
            string answer = storeActions.GetStoresWithPermissions(sessionId);

            return SystemObjJsonConveter.errorFromFailedOperationFromJson(answer);
        }

        public List<string> getAllNotificationMessages()
        {
            return SystemObjJsonConveter.stringsListFromJson(userActions.GetMyMessages(sessionId));
        }


        public List<string> getAllNotificationMessages_without_login(User user)
        {
            Guid sessionBefore = sessionId;
            sessionId = Guid.NewGuid();
            loginInternal(user.Username, user.Pw);

            var result = SystemObjJsonConveter.stringsListFromJson(userActions.GetMyMessages(sessionId));

            logout();
            sessionId = sessionBefore;

            return result;

            

        }
        public Dictionary<DateTime, int[]> GetStatisticsSuccess(DateTime from, DateTime? to)
        {
            string json = userActions.GetDailyStatistics(sessionId, from.Date.ToString("MM/dd/yyyy"), to.GetValueOrDefault(from).Date.ToString("MM/dd/yyyy"));
            return SystemObjJsonConveter.statsFromJsonSuccess(json);
        }

        public void doActionFromGuest()
        {
            Guid session = Guid.NewGuid();
            string json = storeActions.GetAllStoresInformation(session);
            Assert.True(SystemObjJsonConveter.operationSuccededOrNotFromJson(json));
        }

        public void doActionFromUser(User registeredUser)
        {
            Guid session = Guid.NewGuid();
            Assert.True(SystemObjJsonConveter.ansFromJsonSuccess( userActions.Login(session, registeredUser.Username, registeredUser.Pw)));
            Assert.True(SystemObjJsonConveter.operationSuccededOrNotFromJson(storeActions.GetAllStoresInformation(session)));
            Assert.True(SystemObjJsonConveter.ansFromJsonSuccess(userActions.Logout(session)));
        }

        public string GetStatisticsError(DateTime from, DateTime? to)
        {
            string json = userActions.GetDailyStatistics(sessionId, from.Date.ToString("MM/dd/yyyy"), to.GetValueOrDefault(from).Date.ToString("MM/dd/yyyy"));
            return SystemObjJsonConveter.errorFromFailedOperationFromJson(json);
        }
    }
}

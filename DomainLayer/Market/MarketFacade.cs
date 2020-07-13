using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Timers;
using DomainLayer.DbAccess;
using DomainLayer.Exceptions;
using DomainLayer.Market.SearchEngine;
using DomainLayer.NotificationsCenter;
using DomainLayer.Orders;
using DomainLayer.StoreManagement;
using DomainLayer.Stores;
using DomainLayer.Stores.Certifications;
using DomainLayer.Stores.Discounts;
using DomainLayer.Stores.Inventory;
using DomainLayer.Stores.PurchasePolicies;
using DomainLayer.Users;
using Newtonsoft.Json;
using Logger = CustomLogger.Logger;

namespace DomainLayer.Market
{
    public class MarketFacade : IMarketFacade
    {
        //For now we keep all fields shared between old market and new market for compatability
        private readonly Dictionary<Guid, User> UsersSessionbyId;
        private readonly Dictionary<Guid, DateTime> TimeStamps;
        private readonly UserManager userManager;
        private readonly SearchFacade searchFacade;
        private readonly OrderManager orderManager;
        private readonly StoreManagementFacade storeManagement;
        private readonly INotificationObserver notificationObserver;
        private readonly bool useInMemoryDB;
        private readonly Effort.Provider.EffortConnection inMemoryConnection;

        public Timer Timer { get; }

        public MarketFacade(string adminName, string adminPassword, ICommunicationNotificationAlerter communicationNotification, bool useInMemoryDB = false)
        {
            UsersSessionbyId = new Dictionary<Guid, User>();
            TimeStamps = new Dictionary<Guid, DateTime>();
            StoreHandler storeHandler = new StoreHandler();
            searchFacade = new SearchFacade(storeHandler);
            this.useInMemoryDB = useInMemoryDB;
            this.inMemoryConnection = useInMemoryDB ? Effort.DbConnectionFactory.CreateTransient() : null;

            userManager = new UserManager(adminName, adminPassword, getMarketDBContext());
            orderManager = new OrderManager(storeHandler, this.inMemoryConnection);
            storeManagement = new StoreManagementFacade(storeHandler, orderManager);
            notificationObserver = new NotificationObserver(userManager,communicationNotification);
            notificationObserver.RegisterSubject(storeHandler);
            notificationObserver.RegisterSubject(orderManager);
            Timer = new Timer
            {
                Interval = 1000 * 60 * 15
            };
            Timer.Enabled = true;
            Timer.Elapsed += new ElapsedEventHandler(ReviewSessions);
        }

        private void ReviewSessions(object sender, ElapsedEventArgs e)
        {
            foreach (KeyValuePair<Guid, DateTime> lastActive in TimeStamps)
            {
                if (lastActive.Value.AddMinutes(15).CompareTo(DateTime.Now) < 0)
                {
                    try
                    {
                        Logout(lastActive.Key);
                    }
                    catch { }// Guest
                    UsersSessionbyId.Remove(lastActive.Key);
                }
            }
        }

        private MarketDbContext getMarketDBContext()
        {
            if (useInMemoryDB)
            {
                return new MarketDbContext(this.inMemoryConnection);
            }
            else
            {
                return new MarketDbContext();
            }
        }
        private void ValidateStoreExistIfNotThrowArgumentException(Guid storeID, MarketDbContext context)
        {
            if (!StoreExist(storeID))
            {
                throw new ArgumentException(string.Format("Store ID : '{0}' not found", storeID));
            }
        }

        public string Register(Guid sessionID, string username, string password){ /*UC 2.2*/
            using var context = getMarketDBContext();
            Logger.writeEvent(string.Format("MarketFacade: Register new user \'{0}\'| started", username));
            try
            {
                User user = GetUserAndUpdateTimestamp(sessionID, context);
                userManager.Register(user, username, password, context);
                Logger.writeEvent("MarketFacade: Register| succeeded");
                return Create_json_response(true, true);
            }
            catch (Exception e)
            {
                Logger.writeError(e);
                Logger.writeEvent("MarketFacade: Register| failed");
                return Create_json_response(false, e);
            }
        }

        private void CheckItemAmount(Guid storeID, Guid itemId, int amount, MarketDbContext context)
        {
            searchFacade.CheckItemAmount(storeID, itemId, amount, context);
        }

        public string AddToCart(Guid sessionID, Guid storeID, Guid itemID, int amount)
        {
            using var context = getMarketDBContext();
            Logger.writeEvent("MarketFacade: AddToCart| started");
            try
            {
                User user = GetUserAndUpdateTimestamp(sessionID, context);
                CheckItemAmount(storeID, itemID, amount, context);
                user.AddToCart(storeID, itemID, amount, context);
                Logger.writeEvent("MarketFacade: AddToCart| succeeded");
                return Create_json_response(true, true);
            }
            catch (Exception e)
            {
                Logger.writeError(e);
                Logger.writeEvent("MarketFacade: AddToCart| failed");
                return Create_json_response(false, e);
            }
        }


        public string Login(Guid sessionID, string username, string password)//UC 2.3
        {
            using var context = getMarketDBContext();
            Logger.writeEvent("MarketFacade: Login| started");
            try
            {
                User user = GetUserAndUpdateTimestamp(sessionID, context);
                userManager.Login(sessionID, user, username, password, context);
                Logger.writeEvent("MarketFacade: Login| succeeded");
                Console.WriteLine(string.Format("logged in from {0}", sessionID));
                notificationObserver.userLoggedIn(user, sessionID, user.IsAdmin(), context);
                notificationObserver.RefreshStatistics(context);
                return Create_json_response(true, true);
            }
            catch (Exception e)
            {
                Logger.writeError(e);
                Logger.writeEvent("MarketFacade: Login| failed");
                return Create_json_response(false, e);
            }
        }

        public string Logout(Guid sessionID)
        {
            using var context = getMarketDBContext();
            Logger.writeEvent("MarketFacade: logout| started");
            try
            {
                User user = GetUserAndUpdateTimestamp(sessionID, context);
                Guid? userID = GetUserIDBySessionID_Null(sessionID, context);
                userManager.Logout(user, context);
                Logger.writeEvent("MarketFacade: logout| succeeded");
                if (userID is Guid ID)
                {
                    notificationObserver.userLoggedOut(ID, sessionID);
                }

                return Create_json_response(true, true);
            }
            catch (Exception e)
            {
                Logger.writeError(e);
                Logger.writeEvent("MarketFacade: logout| failed");
                return Create_json_response(false, e);

            }
        }

        public string GetAllStoresInformation(Guid sessionID)
        {
            using var context = getMarketDBContext();
            Logger.writeEvent("MarketFacade: GetAllStoresInformation| started");
            try
            {
                GetUserAndUpdateTimestamp(sessionID, context);
                ReadOnlyCollection<Store> allStores = searchFacade.GetAllStoresInformation(context);
                Logger.writeEvent("MarketFacade: GetAllStoresInformation| succeeded");
                return Create_json_response(true, allStores);
            }
            catch (Exception e)
            {
                Logger.writeError(e);
                Logger.writeEvent("MarketFacade: GetAllStoresInformation| failed");
                return Create_json_response(false, e);
            }
        }

        public string GetStoreInformationByID(Guid sessionID, Guid storeID)
        {
            using var context = getMarketDBContext();
            Logger.writeEvent("MarketFacade: GetStoreInformationByID| started");
            ValidateStoreExistIfNotThrowArgumentException(storeID, context);
            try
            {
                GetUserAndUpdateTimestamp(sessionID, context);
                Store store = searchFacade.GetStoreInformationByID(storeID, context);
                Logger.writeEvent("MarketFacade: GetStoreInformationByID| succeeded");
                return Create_json_response(true, store);
            }
            catch (Exception e)
            {
                Logger.writeError(e);
                Logger.writeEvent("MarketFacade: GetAllStoresInformation| failed");
                return Create_json_response(false, e);
            }
        }

        public string SearchItems(Guid sessionID, double? filterItemRank, double? filterMinPrice, double? filterMaxPrice, double? filterStoreRank, string name = null, string category = null, string keywords = null)
        {
            using var context = getMarketDBContext();
            Logger.writeEvent("MarketFacade: SearchItems| started");
            try
            {
                GetUserAndUpdateTimestamp(sessionID, context);
                Dictionary<Guid, ReadOnlyCollection<Item>> filterdItems;
                if (keywords != null)
                {
                    Logger.writeEvent("MarketFacade: SearchItems| with keywords");
                    filterdItems = searchFacade.SearchItems(context, filterItemRank, filterMinPrice, filterMaxPrice,
                                        filterStoreRank, name, category,
                                        JsonConvert.DeserializeObject<List<string>>(keywords));
                }
                else
                {
                    filterdItems = searchFacade.SearchItems(context, filterItemRank, filterMinPrice, filterMaxPrice,
                                              filterStoreRank, name, category, null);
                }
                Logger.writeEvent("MarketFacade: SearchItems| succeeded");
                return Create_json_response(true, filterdItems);
            }
            catch (Exception e)
            {
                Logger.writeError(e);
                Logger.writeEvent("MarketFacade: SearchItems| failed");
                return Create_json_response(false, e);

            }
        }

        public bool CheckSufficentAmountInInventory(Guid storeID, Guid itemID, int amountToCheck)
        {
            using var context = getMarketDBContext();
            ValidateStoreExistIfNotThrowArgumentException(storeID, context);
            try
            {
                return orderManager.CheckSufficentAmountInInventory(storeID, itemID, amountToCheck, context);
            }
            catch(ItemNotFoundException)
            {
                throw new ArgumentException(string.Format("Item ID : '{0}' not found", itemID));
            }
        }

        //returns empty if no purchases or user id not registered.
        //always succeeds
        public string GetHistoryOfUser(Guid userID, MarketDbContext context)
        {
            List<Order> orders = orderManager.GetUserOrdersHistory(userID, context);
            return Create_json_response(true, orders);         
        }

        //returns empty list if no permissions
        public List<string> GetPermissionsInStore(Guid storeID, Guid userID)
        {
            using var context = getMarketDBContext();
            ValidateStoreExistIfNotThrowArgumentException(storeID, context);

            try
            {
                List<Permission> permissions = storeManagement.GetPermissionsInStore(storeID, userID, context);
                return permissions.ConvertAll(p => p.ToString());
            }
            catch (CertificationException)
            {
                return new List<string>();
            }
        }

        //always succeed unless no such store. in that case its an argument exception.
        public string GetStoreHistory(Guid storeID)
        {
            using var context = getMarketDBContext();
            ValidateStoreExistIfNotThrowArgumentException(storeID, context);

            List<StoreOrder> orders = orderManager.GetStoreOrdersHistory(storeID, context);
            return Create_json_response(true, orders);
        }

        private Guid GetStoreIDByName(string name, MarketDbContext context)
        {

            Guid id = searchFacade.GetStoreIDByName(name, context);
            if (id.Equals(Guid.Empty))
            {
                throw new ArgumentException(string.Format("Store name : '{0}' not found", name));
            }
            return id;
                
           
        }

        private User GetUserAndUpdateTimestamp(Guid sessionID, MarketDbContext context)
        {
            TimeStamps[sessionID] = DateTime.Now;
            if (!UsersSessionbyId.TryGetValue(sessionID, out User user))
            {
                user = NewSessionUser(sessionID, context);
            }
            else
            {
                user.Reload(context);
            }
            return user;
        }


        //should always succeed (reutrn empty dict if no items.)
        public string GetUserCart(Guid sessionID)
        {
            using var context = getMarketDBContext();
            Logger.writeEvent("MarketFacade: ViewCart| started");
            Dictionary<Guid, List<Tuple<Item, int>>> storeIDToItemsAndAmount = new Dictionary<Guid, List<Tuple<Item, int>>>();
            try
            {
                User user = GetUserAndUpdateTimestamp(sessionID, context);
                foreach (KeyValuePair<Guid, StoreCart> StoreCart in user.Cart.StoreCarts)
                {
                    Guid storeID = StoreCart.Key;
                    List<Tuple<Item, int>> itemsAndAmountFromStore = new List<Tuple<Item, int>>();
                    foreach (KeyValuePair<Guid, int> itemIdToAmount in StoreCart.Value.Items)
                    {
                        Item item = searchFacade.GetItemByIdFromStore(storeID, itemIdToAmount.Key, context);
                        itemsAndAmountFromStore.Add(new Tuple<Item, int>(item, itemIdToAmount.Value));
                    }
                    storeIDToItemsAndAmount[storeID] = itemsAndAmountFromStore;
                }
                Logger.writeEvent("MarketFacade: ViewCart| succeeded");
                return Create_json_response(true, storeIDToItemsAndAmount);
            }
            catch (Exception e)
            {
                Logger.writeError(e);
                Logger.writeEvent("MarketFacade: ViewCart| failed");
                return Create_json_response(false, e);
            }
        }

        public string RemoveFromCart(Guid sessionID, Guid storeID, Guid itemID)
        {
            using var context = getMarketDBContext();
            Logger.writeEvent("MarketFacade: RemoveFromCart| started");
            try
            {
                User user = GetUserAndUpdateTimestamp(sessionID, context);
                user.RemoveFromCart(storeID, itemID, context);
                Logger.writeEvent("MarketFacade: RemoveFromCart| succeeded");
                return Create_json_response(true, true);
            }
            catch (Exception e)
            {
                Logger.writeError(e);
                Logger.writeEvent("MarketFacade: RemoveFromCart| failed");
                return Create_json_response(false, e);

            }
        }

        public string ChangeItemAmountInCart(Guid sessionID, Guid storeID, Guid itemID, int amount)
        {
            using var context = getMarketDBContext();
            Logger.writeEvent("MarketFacade: ChangeItemAmountInCart| started");
            try
            {
                if (amount >= 0)
                {
                    User user = GetUserAndUpdateTimestamp(sessionID, context);
                    CheckItemAmount(storeID, itemID, amount, context);
                    user.SetItemAmountInCart(storeID, itemID, amount, context);
                    Logger.writeEvent("MarketFacade: ChangeItemAmountInCart| succeeded");
                    return Create_json_response(true, true);
                }
                Logger.writeEvent("MarketFacade: ChangeItemAmountInCart| failed due to negative amount");
                return Create_json_response(true, "Item amount can't be negative");
            }
            catch (Exception e)
            {
                Logger.writeError(e);
                Logger.writeEvent("MarketFacade: SearchItems| failed");
                return Create_json_response(false, e);
            }
        }

        /// <summary>
        /// returns Guid.empty if no such user
        /// </summary>
        /// <param name="username"></param>
        /// <returns></returns>
        private Guid GetUserIDByName(string username, MarketDbContext context)
        {
            try
            {
                return userManager.GetUserIDByName(username, context);
            }
            catch(UserManagementException e)
            {
                if (e.Message.Contains(ExceptionStrings.NoUserWithName))
                    return Guid.Empty;
                throw e;
            }
        }

        private User NewSessionUser(Guid sessionID, MarketDbContext context)
        {
            Logger.writeEvent(string.Format("New session ID {0} - Creating new Guest User", sessionID));
            User user = userManager.NewSession(sessionID, context);
            UsersSessionbyId.Add(sessionID, user);
            notificationObserver.RefreshStatistics(context);

            return user;
        }

        private Guid GetUserIDBySessionID_Throws(Guid sessionID, MarketDbContext context)
        {
            if (!UsersSessionbyId.TryGetValue(sessionID, out User user))
            {
                //not recognized, add new user guest.
                user = NewSessionUser(sessionID, context);
            }
            return userManager.GetSessionUserID(user);
        }

        private Guid? GetUserIDBySessionID_Null(Guid sessionID, MarketDbContext context)
        {
            UsersSessionbyId.TryGetValue(sessionID, out User user);
            try
            {
                return GetUserIDBySessionID_Throws(sessionID, context);
            }
            catch (UserStateException)
            {
                return null;
            }
        }

        public bool IsGrantorOf(Guid storeID, Guid sessionID, string username)
        {
            using var context = getMarketDBContext();
            GetUserAndUpdateTimestamp(sessionID, context);
            Guid grantorID = GetUserIDBySessionID_Throws(sessionID, context);
            Guid userID = GetUserIDByName(username, context);
            ValidateStoreExistIfNotThrowArgumentException(storeID, context);

            try
            {
                return storeManagement.IsGrantorOf(storeID, grantorID, userID, context);
            }
            catch (CertificationException)
            {
                throw new ArgumentException(string.Format("User : '{0}' does not have ceritification", userID));
            }
        }

        public bool IsOwnerOfStore(Guid storeID, string username)
        {
            using var context = getMarketDBContext();
            Guid userID = GetUserIDByName(username, context);
            ValidateStoreExistIfNotThrowArgumentException(storeID, context);

            return storeManagement.IsOwnerOfStore(storeID, userID, context);
        }

        public bool IsPermitedOperation(Guid sessionID, Guid storeID, string permission)
        {
            using var context = getMarketDBContext();
            GetUserAndUpdateTimestamp(sessionID, context);
            ValidateStoreExistIfNotThrowArgumentException(storeID, context);
            if (!Enum.TryParse(permission, true, out Permission perm))
            {
                throw new ArgumentException(string.Format("'{0}' is not a legal permission string", permission));
            }

            Guid userID = GetUserIDBySessionID_Throws(sessionID, context);
            return storeManagement.IsPermittedOperation(storeID, userID, perm, context); 
        }

        public bool ItemExistInStore(Guid storeID, Guid itemID)
        {
            using var context = getMarketDBContext();
            ValidateStoreExistIfNotThrowArgumentException(storeID, context);

            return storeManagement.ItemExistInStore(storeID, itemID, context);
        }

        public bool StoreExist(Guid storeID)
        {
            using var context = getMarketDBContext();
            return searchFacade.StoreExistByID(storeID, context);
        }

        private static string Create_json_response<T>(bool success, T answerOrException)
        {
            string responseJson = JsonConvert.SerializeObject(answerOrException);
            ResponseClass response = new ResponseClass(success, responseJson);
            return JsonConvert.SerializeObject(response);
        }

        public string CheckOut(Guid sessionID, Guid orderID)
        {
            using var context = getMarketDBContext();
            User user = GetUserAndUpdateTimestamp(sessionID, context);
            Guid? userID = GetUserIDBySessionID_Null(sessionID, context);
            try
            {
                orderManager.LogOrder(userID, orderID, context);
                user.ClearCart(context);
                Logger.writeEvent("MarketFacade: CheckOut| succeeded");
                return Create_json_response(true, true);
            }
            catch (Exception e)
            {
                Logger.writeEvent("MarketFacade: CheckOut| Failed");
                return Create_json_response(false, e);
            }
        }

        public string DiscountAndReserve(Guid sessionID)
        {
            using var context = getMarketDBContext();
            User user = GetUserAndUpdateTimestamp(sessionID, context);
            try
            {
                Guid? userID = GetUserIDBySessionID_Null(sessionID, context);
                Order order = orderManager.DiscountAndReserve(userID, user.Cart.StoreCarts, context);
                Logger.writeEvent("MarketFacade: ValidateCartPricesAfterDiscount| succeeded");
                return Create_json_response(true, order);
            }
            catch (Exception e)
            {
                Logger.writeEvent("MarketFacade: ValidateCartPricesAfterDiscount| failed");
                Logger.writeError(e);
                return Create_json_response(false, e);
            }
        }

        public string CollectPayment(Guid sessionID, Guid orderID, string cardNum, string expire, string CCV, string cardOwnerName, string cardOwnerID)
        {
            Logger.writeEvent("MarketFacade: CollectPayment| started");
            using var context = getMarketDBContext();
            Guid? userID = GetUserIDBySessionID_Null(sessionID, context);
            GetUserAndUpdateTimestamp(sessionID, context);
            try
            {
                orderManager.CollectPayment(userID, orderID, cardNum, expire, CCV, cardOwnerName, cardOwnerID, context);
                Logger.writeEvent("MarketFacade: CollectPayment| succeeded");
            }
            catch (Exception e)
            {
                Logger.writeError(e);
                Logger.writeEvent("MarketFacade: CollectPayment| failed");
                return Create_json_response(false, e);
            }
            return Create_json_response(true, true);
        }

        public string ScheduleDelivery(Guid sessionID, Guid orderID, string address, string city, string country, string zipCode, string name)
        {
            Logger.writeEvent("MarketFacade: ScheduleDelivery| started");
            using var context = getMarketDBContext();
            Guid? userID = GetUserIDBySessionID_Null(sessionID, context);
            GetUserAndUpdateTimestamp(sessionID, context);
            try
            {
                orderManager.ScheduleDelivery(userID, orderID, address, city, country, zipCode, name, context);
                Logger.writeEvent("MarketFacade: ScheduleDelivery| succeeded");
                return Create_json_response(true, true);
            }
            catch (Exception e)
            {
                Logger.writeError(e);
                Logger.writeEvent("MarketFacade: ScheduleDelivery| failed");
                return Create_json_response(false, e);
            }
        }

        public string CancelPayment(Guid sessionID, Guid orderID, string cardNum)
        {
            Logger.writeEvent("MarketFacade: CancelPayment| started");
            using var context = getMarketDBContext();
            Guid? userID = GetUserIDBySessionID_Null(sessionID, context);
            GetUserAndUpdateTimestamp(sessionID, context);
            try
            {
                orderManager.CancelPayment(userID, orderID, cardNum, context);
                Logger.writeEvent("MarketFacade: CancelPayment| succeeded");
            }
            catch (Exception e)
            {
                Logger.writeError(e);
                Logger.writeEvent("MarketFacade: CancelPayment| failed");
                return Create_json_response(false, e);
            }
            return Create_json_response(true, true);
        }

        public void ReturnProducts(Guid sessionID, Guid orderID)
        {
            using var context = getMarketDBContext();
            Guid? userID = GetUserIDBySessionID_Null(sessionID, context);
            GetUserAndUpdateTimestamp(sessionID, context);
            try
            {
                orderManager.ReturnProducts(userID, orderID, context);
            }
            catch (Exception)
            {
            }
        }

        public string ValidatePurchase(Guid sessionID)
        {
            using var context = getMarketDBContext();
            User user = GetUserAndUpdateTimestamp(sessionID, context);
            try
            {
                orderManager.ValidatePurchase(user, context);
                Logger.writeEvent("MarketFacade: ValidatePurchase| succeeded");
                return Create_json_response(true, true);
            }
            catch (Exception e)
            {
                Logger.writeEvent("MarketFacade: ValidatePurchase| failed");
                Logger.writeError(e);
                return Create_json_response(false, e);
            }
        }

        public string OpenStore(Guid sessionID, string name, string email, string address, string phone, string bankAccountNumber, string bank, string description, string purchasePolicy, string discountPolicy)
        {
            using var context = getMarketDBContext();
            Logger.writeEvent("MarketFacade: OpenStore| started");
            StoreContactDetails details = new StoreContactDetails(name, email, address, phone, bankAccountNumber, bank, description);
            GetUserAndUpdateTimestamp(sessionID, context);
            try
            {
                Guid userID = GetUserIDBySessionID_Throws(sessionID, context);
                Store newStore = storeManagement.OpenStore(details, userID, context);
                Logger.writeEvent("MarketFacade: OpenStore| succeeded");
                return Create_json_response(true, newStore);
            }
            catch (Exception e)
            {
                Logger.writeError(e);
                Logger.writeEvent("MarketFacade: OpenStore| failed");
                return Create_json_response(false, e);

            }
        }

        public string AddItem(Guid sessionID, Guid storeID, string name, int amount, double price, string categories, string keyWords = null)// UC 4.1
        {
            using var context = getMarketDBContext();
            Logger.writeEvent("MarketFacade: AddItem| started");
            GetUserAndUpdateTimestamp(sessionID, context);
            try
            {
                Guid userID = GetUserIDBySessionID_Throws(sessionID, context);
                Item item;
                if (keyWords != null && categories != null)
                {
                    item = storeManagement.AddItem(storeID, userID, name, amount,
                                                    JsonConvert.DeserializeObject<string[]>(categories).ToHashSet(), price,
                                                    context, JsonConvert.DeserializeObject<string[]>(keyWords).ToHashSet());
                }
                else if (keyWords != null)
                {
                    item = storeManagement.AddItem(storeID, userID, name, amount, null, price, context,
                                                    JsonConvert.DeserializeObject<string[]>(keyWords).ToHashSet());
                }
                else if (categories != null)
                {
                    item = storeManagement.AddItem(storeID, userID, name, amount,
                                                    JsonConvert.DeserializeObject<string[]>(categories).ToHashSet(), 
                                                    price, context, null);
                }
                else
                {
                    item = storeManagement.AddItem(storeID, userID, name, amount, null, price, null);
                }
                Logger.writeEvent("MarketFacade: AddItem| succeeded");
                return Create_json_response(true, item);

            }
            catch (InvalidOperationOnItemException e)
            {
                Logger.writeEvent("MarketFacade: AddItem| failed");
                return Create_json_response(false, e);
            }
            catch (Exception e)
            {
                Logger.writeError(e);
                Logger.writeEvent("MarketFacade: AddItem| failed");
                return Create_json_response(false, e);
            }

        }

        public string DeleteItem(Guid sessionID, Guid storeID, Guid itemId)
        {
            using var context = getMarketDBContext();
            Logger.writeEvent("MarketFacade: DeleteItem| started");
            GetUserAndUpdateTimestamp(sessionID, context);
            try
            {
                Guid userID = GetUserIDBySessionID_Throws(sessionID, context);
                storeManagement.DeleteItem(storeID, userID, itemId, context);
                Logger.writeEvent("MarketFacade: DeleteItem| succeeded");
                return Create_json_response(true, true);
            }
            catch (Exception e)
            {
                Logger.writeError(e);
                Logger.writeEvent("MarketFacade: DeleteItem| failed");
                return Create_json_response(false, e);
            }
        }

        public string EditItem(Guid sessionID, Guid storeID, Guid itemId, int? amount, double? rank, double? price, string name = null, string categories = null, string keyWords = null)
        {
            using var context = getMarketDBContext();
            Logger.writeEvent("MarketFacade: EditItem| started");
            GetUserAndUpdateTimestamp(sessionID, context);
            try
            {
                Guid userID = GetUserIDBySessionID_Throws(sessionID, context);
                Dictionary<StoresUtils.ItemEditDetails, object> detailsToEdit = new Dictionary<StoresUtils.ItemEditDetails, object>();
                if (name != null)
                {
                    detailsToEdit.Add(StoresUtils.ItemEditDetails.name, name);
                }
                if (categories != null)
                {
                    detailsToEdit.Add(StoresUtils.ItemEditDetails.categories, JsonConvert.DeserializeObject<List<string>>(categories));
                }
                if (keyWords != null)
                {
                    detailsToEdit.Add(StoresUtils.ItemEditDetails.keyWords, JsonConvert.DeserializeObject<List<string>>(keyWords));
                }
                if (price is double priceVal)
                {
                    detailsToEdit.Add(StoresUtils.ItemEditDetails.price, priceVal);
                }
                if (rank is double rankVal)
                {
                    detailsToEdit.Add(StoresUtils.ItemEditDetails.rank, rankVal);
                }
                if (amount is int amountVal)
                {
                    detailsToEdit.Add(StoresUtils.ItemEditDetails.amount, amountVal);
                }
                Item item = storeManagement.EditItem(storeID, userID, itemId, detailsToEdit, context);
                Logger.writeEvent("MarketFacade: EditItem| succeeded");
                return Create_json_response(true, item);
            }
            catch (InvalidOperationOnItemException e)
            {
                Logger.writeEvent("MarketFacade: EditItem| failed");
                return Create_json_response(false, e);
            }
            catch (Exception e)
            {
                Logger.writeError(e);
                Logger.writeEvent("MarketFacade: EditItem| failed");
                return Create_json_response(false, e);
            }

        }

        public string AppointOwner(Guid sessionID, Guid storeID, string username)
        {
            using var context = getMarketDBContext();
            Logger.writeEvent("MarketFacade: AppointOwner| started");
            GetUserAndUpdateTimestamp(sessionID, context);
            try
            {
                Guid userID = GetUserIDBySessionID_Throws(sessionID, context);
                storeManagement.AppointOwner(storeID, userManager.GetUserIDByName(username, context), userID, username, context);
                Logger.writeEvent("MarketFacade: AppointOwner| succeeded");
                return Create_json_response(true, true);
            }
            catch (Exception e)
            {
                Logger.writeError(e);
                Logger.writeEvent("MarketFacade: AppointOwner| failed");
                return Create_json_response(false, e);
            }
        }

        public string ApproveOwnerContract(Guid sessionID, Guid storeID, string username)
        {
            using var context = getMarketDBContext();

            ValidateStoreExistIfNotThrowArgumentException(storeID, context);
            Logger.writeEvent("MarketFacade: ApproveOwnerContract| started");
            GetUserAndUpdateTimestamp(sessionID, context);
            try
            {
                Guid userID = GetUserIDBySessionID_Throws(sessionID, context);
                storeManagement.ApproveOwnerContract(storeID, userManager.GetUserIDByName(username, context), userID, context);
                Logger.writeEvent("MarketFacade: ApproveOwnerContract| succeeded");
                return Create_json_response(true, true);
            }
            catch (Exception e)
            {
                Logger.writeError(e);
                Logger.writeEvent("MarketFacade: AppointOwner| failed");
                return Create_json_response(false, e);
            }
        }

        public bool IsApproverOfContract(Guid sessionID, Guid storeID, string username)
        {
            using var context = getMarketDBContext();
            GetUserAndUpdateTimestamp(sessionID, context);

            Guid sessionUser = GetUserIDBySessionID_Throws(sessionID, context);
            Guid userID = userManager.GetUserIDByName(username, context);
            return storeManagement.IsApproverOfContract(sessionUser, storeID, userID, context);
        }

        public bool IsAwaitingContractApproval(Guid storeID, string username)
        {
            using var context = getMarketDBContext();
            return storeManagement.IsAwaitingContractApproval( storeID, userManager.GetUserIDByName(username, context), context);
        }

        public string AppointManager(Guid sessionID, Guid storeID, string username)
        {
            using var context = getMarketDBContext();
            Logger.writeEvent(string.Format("MarketFacade: AppointManager| appoint {0} to store {1} started", username, storeID));
            GetUserAndUpdateTimestamp(sessionID, context);
            try
            {
                Guid userID = GetUserIDBySessionID_Throws(sessionID, context);
                storeManagement.AppointManager(storeID, userManager.GetUserIDByName(username, context), userID, context);
                Logger.writeEvent(string.Format("MarketFacade: AppointManager| appoint {0} to store {1} succeeded", username, storeID));
                return Create_json_response(true, true);
            }
            catch (Exception e)
            {
                Logger.writeError(e);
                Logger.writeEvent(string.Format("MarketFacade: AppointManager| appoint {0} to store {1} failed", username, storeID));
                return Create_json_response(false, e);
            }
        }

        public string AddPermission(Guid sessionID, Guid storeID, string username, string permission)// UC 4.6
        {
            using var context = getMarketDBContext();
            Logger.writeEvent("MarketFacade: AddPermission| started");
            GetUserAndUpdateTimestamp(sessionID, context);
            try
            {
                Guid userID = GetUserIDBySessionID_Throws(sessionID, context);
                if (Enum.TryParse(permission, true, out Permission toAdd))
                {
                    storeManagement.AddPermission(storeID, userManager.GetUserIDByName(username, context), userID, toAdd, context);
                    Logger.writeEvent("MarketFacade: AddPermission| succeeded");
                    return Create_json_response(true, true);
                }
                Logger.writeEvent(string.Format("MarketFacade: AddPermission| unable to parse permission: \'{0}\'", permission));
                Logger.writeEvent("MarketFacade: AddPermission| failed");
                return Create_json_response(false, "Could not add Permission due to illegal permission");

            }
            catch (Exception e)
            {
                Logger.writeError(e);
                Logger.writeEvent("MarketFacade: AddPermission| failed");
                return Create_json_response(false, e);
            }
        }

        public string RemovePermission(Guid sessionID, Guid storeID, string username, string permission)
        {
            using var context = getMarketDBContext();
            Logger.writeEvent("MarketFacade: RemovePermission| started");
            GetUserAndUpdateTimestamp(sessionID, context);
            try
            {
                Guid userID = GetUserIDBySessionID_Throws(sessionID, context);
                if (Enum.TryParse<Permission>(permission, true, out Permission toRemove))
                {
                    storeManagement.RemovePermission(storeID, userManager.GetUserIDByName(username, context), userID, toRemove, context);
                    Logger.writeEvent("MarketFacade: RemovePermission| started");
                    return Create_json_response(true, true);
                }
                Logger.writeEvent(string.Format("MarketFacade: RemovePermission| unable to parse permission: \'{0}\'", permission));
                Logger.writeEvent("MarketFacade: RemovePermission| failed");
                return Create_json_response(false, "Could not remove Permission due to illegal permission");
            }
            catch (Exception e)
            {
                Logger.writeError(e);
                Logger.writeEvent("MarketFacade: RemovePermission| failed");
                return Create_json_response(false, e);
            }
        }

        public string RemoveOwner(Guid sessionID, Guid storeID, string username)
        {
            using var context = getMarketDBContext();
            Logger.writeEvent("MarketFacade: RemoveOwner| started");
            GetUserAndUpdateTimestamp(sessionID, context);
            try
            {
                Guid userID = GetUserIDBySessionID_Throws(sessionID, context);
                storeManagement.RemoveOwner(storeID, userManager.GetUserIDByName(username, context), userID, context);
                Logger.writeEvent("MarketFacade: RemoveOwner| succeeded");
                return Create_json_response(true, true);
            }
            catch (Exception e)
            {
                Logger.writeError(e);
                Logger.writeEvent("MarketFacade: RemoveOwner| failed");
                return Create_json_response(false, e);
            }
        }

        public string RemoveManager(Guid sessionID, Guid storeID, string username)
        {
            using var context = getMarketDBContext();
            Logger.writeEvent("MarketFacade: RemoveManager| started");
            GetUserAndUpdateTimestamp(sessionID, context);
            try
            {
                Guid userID = GetUserIDBySessionID_Throws(sessionID, context);
                storeManagement.RemoveManager(storeID, userManager.GetUserIDByName(username, context), userID, context);
                Logger.writeEvent("MarketFacade: RemoveManager| succeeded");
                return Create_json_response(true, true);
            }
            catch (Exception e)
            {
                Logger.writeError(e);
                Logger.writeEvent("MarketFacade: RemoveManager| failed");
                return Create_json_response(false, e);

            }
        }

        public bool IsloggedIn(Guid sessionID)
        {
            using var context = getMarketDBContext();
            User user = GetUserAndUpdateTimestamp(sessionID, context);
            return userManager.IsLoggedIn(user);
        }

        public bool StoreExistByName(string name)
        {
            using var context = getMarketDBContext();
            return searchFacade.StoreExistByName(name, context);
        }

        public bool IsPermitedOperation(string username, Guid storeID, string permission)
        {
            using var context = getMarketDBContext();
            ValidateStoreExistIfNotThrowArgumentException(storeID, context);
            if (!Enum.TryParse(permission, true, out Permission perm))
            {
                throw new ArgumentException(string.Format("'{0}' is not a legal permission string", permission));
            }
            Guid userID = GetUserIDByName(username, context);
            if (userID.Equals(Guid.Empty))
            {
                throw new ArgumentException(string.Format("'{0}' is not a registered username", username));
            }

            return storeManagement.IsPermittedOperation(storeID, userID, perm, context);
        }

        public bool IsManagerOfStore(string username, Guid storeID)
        {
            using var context = getMarketDBContext();
            Guid userID = GetUserIDByName(username, context);
            ValidateStoreExistIfNotThrowArgumentException(storeID, context);

            return storeManagement.IsManagerOfStore(storeID, userID, context);
        }

        public string GetMyPermissions(Guid sessionID, Guid storeID)
        {
            using var context = getMarketDBContext();
            Logger.writeEvent("MarketFacade: GetMyPermissions| started");
            GetUserAndUpdateTimestamp(sessionID, context);
            try
            {
                Guid userID = GetUserIDBySessionID_Throws(sessionID, context);
                List<Permission> permissions = storeManagement.GetPermissionsInStore(storeID, userID, context);
                List<string> permissionStrings = permissions.ConvertAll(p => p.ToString());
                Logger.writeEvent("MarketFacade: GetMyPermissions| succeeded");
                return Create_json_response(true, permissionStrings);
            }
            catch (CertificationException e)
            {
                Logger.writeError(e);
                Logger.writeEvent(string.Format("MarketFacade: GetMyPermissions| user has no permissions to store: {0}", storeID));
                return Create_json_response(true, new List<string>());
            }
            catch (Exception e)
            {
                Logger.writeError(e);
                Logger.writeEvent("MarketFacade: GetMyPermissions| failed");
                return Create_json_response(false, e);
            }
        }

        public string GetMyOrderHistory(Guid sessionID)
        {
            using var context = getMarketDBContext();
            GetUserAndUpdateTimestamp(sessionID, context);
            try
            {
                Guid userID = GetUserIDBySessionID_Throws(sessionID, context);
                List<Order> orders = orderManager.GetUserOrdersHistory(userID, context);
                return Create_json_response(true, orders);
            }
            catch (Exception e)
            {
                Logger.writeError(e);
                return Create_json_response(false, e);
            }
        }

        public string GetStoreOrderHistory(Guid sessionID, Guid storeID)
        {
            using var context = getMarketDBContext();
            GetUserAndUpdateTimestamp(sessionID, context);
            try
            {
                Guid userID = GetUserIDBySessionID_Throws(sessionID, context);
                Dictionary<Guid, List<OrderItem>> ordersOfItems = new Dictionary<Guid, List<OrderItem>>();
                List<StoreOrder> orders = storeManagement.GetStoreOrderHistory(storeID, userID, context);
                return Create_json_response(true, orders);
            }
            catch (Exception e)
            {
                Logger.writeError(e);
                return Create_json_response(false, e);
            }
        }

        public string GetUserOrderHistory(Guid sessionID, string username)
        {
            using var context = getMarketDBContext();
            Logger.writeEvent("MarketFacade: GetUserOrderHistory| started");
            try
            {
                User user = GetUserAndUpdateTimestamp(sessionID, context);
                if (userManager.IsAdmin(user))
                {
                    List<Order> orders = orderManager.GetUserOrdersHistory(userManager.GetUserIDByName(username, context), context);
                    Logger.writeEvent("MarketFacade: GetUserOrderHistory| succeeded");
                    return Create_json_response(true, orders);
                }
                Logger.writeEvent("MarketFacade: GetUserOrderHistory| failed, not admin");
                return Create_json_response(false, "Only an Admin has the authority to perform this action");
            }
            catch (Exception e)
            {
                Logger.writeError(e);
                Logger.writeEvent("MarketFacade: GetUserOrderHistory| failed");
                return Create_json_response(false, e);

            }
        }

        public string GetStoreOrderHistoryAdmin(Guid sessionID, Guid storeID)
        {
            using var context = getMarketDBContext();
            Logger.writeEvent("MarketFacade: GetStoreOrderHistoryAdmin| started");
            try
            {
                if (IsAdmin(sessionID))
                {
                    List<StoreOrder> orders = orderManager.GetStoreOrdersHistory(storeID, context);
                    Logger.writeEvent("MarketFacade: GetStoreOrderHistoryAdmin| succeeded");
                    return Create_json_response(true, orders);
                }
                Logger.writeEvent("MarketFacade: GetStoreOrderHistoryAdmin| failed, not admin");
                return Create_json_response(false, "Only Admin can perform this action");
            }
            catch (Exception e)
            {
                Logger.writeError(e);
                Logger.writeEvent("MarketFacade: GetStoreOrderHistoryAdmin| failed");
                return Create_json_response(false, e);
            }
        }

        public string AreExternalSystemsConnected()
        {
            try
            {
                if (orderManager.ExternalSystemsAreConnected())
                {
                    Logger.writeEvent("MarketFacade: AreExternalSystemsConnected| succeeded");
                    return Create_json_response(true, true);
                }
                return Create_json_response(true, false);
            }
            catch (ConnectToExternalSystemsException e)
            {
                Logger.writeError(e);
                Logger.writeEvent("MarketFacade: AreExternalSystemsConnected| failed");
                return Create_json_response(false, e);
            }
        }

        public bool IsAdmin(Guid sessionID)
        {
            using var context = getMarketDBContext();
            User user = GetUserAndUpdateTimestamp(sessionID, context);
            return user.IsAdmin();
        }

        #region discountHandling

        public string AddOpenDiscount(Guid sessionID, Guid storeID, Guid itemID, double discount, int durationInDays)
        {
            using var context = getMarketDBContext();
            Logger.writeEvent("MarketFacade: AddOpenDiscount| started");
            GetUserAndUpdateTimestamp(sessionID, context);
            try
            {
                Guid userID = GetUserIDBySessionID_Throws(sessionID, context);
                OpenDiscount openDiscount = storeManagement.AddOpenDiscount(storeID, userID, itemID, discount, 
                                                                            DateTime.Now.AddDays(durationInDays), context);
                Logger.writeEvent("MarketFacade: AddOpenDiscount| succeeded");
                return Create_json_response(true, openDiscount);
            }
            catch (Exception e)
            {
                Logger.writeError(e);
                Logger.writeEvent("MarketFacade: AddOpenDiscount| failed");
                return Create_json_response(false, e);
            }
        }

        public string AddItemConditionalDiscount_MinItems_ToDiscountOnAll(Guid sessionID, Guid storeID, Guid itemID, int durationInDays, int minItems, double discount)
        {
            using var context = getMarketDBContext();
            Logger.writeEvent("MarketFacade: AddItemConditionalDiscount_MinItems_ToDiscountOnAll| started");
            GetUserAndUpdateTimestamp(sessionID, context);
            try
            {
                Guid userID = GetUserIDBySessionID_Throws(sessionID, context);
                ItemConditionalDiscount_MinItems_ToDiscountOnAll conditionalDiscount = 
                    storeManagement.AddItemConditionalDiscount_MinItems_ToDiscountOnAll(storeID, userID, itemID, discount, 
                    minItems, DateTime.Now.AddDays(durationInDays), context);
                Logger.writeEvent("MarketFacade: AddItemConditionalDiscount_MinItems_ToDiscountOnAll| succeeded");
                return Create_json_response(true, conditionalDiscount);
            }
            catch (Exception e)
            {
                Logger.writeError(e);
                Logger.writeEvent("MarketFacade: AddItemConditionalDiscount_MinItems_ToDiscountOnAll| failed");
                return Create_json_response(false, e);
            }
        }

        public string AddItemConditionalDiscount_MinItems_ToDiscountOnExtraItems(Guid sessionID, Guid storeID, Guid itemID, int durationInDays, int minItems, int extraItems, double discountForExtra)
        {
            using var context = getMarketDBContext();
            Logger.writeEvent("MarketFacade: AddItemConditionalDiscount_MinItems_ToDiscountOnExtraItems| started");
            GetUserAndUpdateTimestamp(sessionID, context);
            try
            {
                Guid userID = GetUserIDBySessionID_Throws(sessionID, context);
                ItemConditionalDiscount_MinItems_ToDiscountOnExtraItems conditionalDiscount = 
                    storeManagement.AddItemConditionalDiscount_MinItems_ToDiscountOnExtraItems(storeID, userID, itemID, 
                    discountForExtra, minItems,extraItems, DateTime.Now.AddDays(durationInDays), context);
                Logger.writeEvent("MarketFacade: AddItemConditionalDiscount_MinItems_ToDiscountOnExtraItems| succeeded");
                return Create_json_response(true, conditionalDiscount);
            }
            catch (Exception e)
            {
                Logger.writeError(e);
                Logger.writeEvent("MarketFacade: AddItemConditionalDiscount_MinItems_ToDiscountOnExtraItems| failed");
                return Create_json_response(false, e);
            }
        }

        public string AddStoreConditionalDiscount(Guid sessionID, Guid storeID, int durationInDays, double minPurchase, double discount)
        {
            using var context = getMarketDBContext();
            Logger.writeEvent("MarketFacade: AddStoreConditionalDiscount| started");
            GetUserAndUpdateTimestamp(sessionID, context);
            try
            {
                Guid userID = GetUserIDBySessionID_Throws(sessionID, context);
                StoreConditionalDiscount storeConditional = storeManagement.AddStoreConditionalDiscount(
                    storeID, userID, minPurchase, discount, DateTime.Now.AddDays(durationInDays), context);
                Logger.writeEvent("MarketFacade: AddStoreConditionalDiscount| succeeded");
                return Create_json_response(true, storeConditional);
            }
            catch (Exception e)
            {
                Logger.writeError(e);
                Logger.writeEvent("MarketFacade: AddStoreConditionalDiscount| failed");
                return Create_json_response(false, e);
            }
        }

        public bool DiscountExistsInStore(Guid storeID, Guid discountID)
        {
            using var context = getMarketDBContext();
            ValidateStoreExistIfNotThrowArgumentException(storeID, context);

            bool ans = storeManagement.DiscountExistsInStore(storeID,discountID, context);
            return ans;  
        }

        public string ComposeTwoDiscounts(Guid sessionID, Guid storeID, Guid discountLeftID, Guid discountRightID, string boolOperator)
        {
            using var context = getMarketDBContext();
            Logger.writeEvent("MarketFacade: ComposeTwoDiscounts| started");
            GetUserAndUpdateTimestamp(sessionID, context);
            try
            {
                Guid userID = GetUserIDBySessionID_Throws(sessionID, context);
                CompositeDiscount compositeDiscount = storeManagement.AddCompositeDiscount(userID, storeID,
                    discountLeftID, discountRightID, boolOperator, context);
                Logger.writeEvent("MarketFacade: ComposeTwoDiscounts| succeeded");
                return Create_json_response(true, compositeDiscount);
            }
            catch (Exception e)
            {
                Logger.writeError(e);
                Logger.writeEvent("MarketFacade: ComposeTwoDiscounts| failed");
                return Create_json_response(false, e);
            }
        }

        public string RemoveDiscount(Guid sessionID, Guid storeID, Guid discountID)
        {
            using var context = getMarketDBContext();
            Logger.writeEvent("MarketFacade: RemoveDiscount| started");
            GetUserAndUpdateTimestamp(sessionID, context);
            try
            {
                Guid userID = GetUserIDBySessionID_Throws(sessionID, context);
                bool success = storeManagement.RemoveDiscount(userID, storeID,discountID, context);
                Logger.writeEvent("MarketFacade: RemoveDiscount| succeeded");
                return Create_json_response(true, success);
            }
            catch (Exception e)
            {
                Logger.writeError(e);
                Logger.writeEvent("MarketFacade: RemoveDiscount| failed");
                return Create_json_response(false, e);
            }
        }

        public bool IsDiscountTypeAllowed(Guid storeID, string discountTypeString)
        {
            using var context = getMarketDBContext();
            ValidateStoreExistIfNotThrowArgumentException(storeID, context);
            if (!Enum.TryParse(discountTypeString, true, out DiscountType discount))
            {
                throw new ArgumentException(string.Format("'{0}' is not a legal discountType string", discountTypeString));
            }

            return storeManagement.IsDiscountAllowed(storeID, discount, context);
        }

        public string MakeDiscountNotAllowed(Guid sessionID, Guid storeID, string discountTypeString)
        {
            using var context = getMarketDBContext();
            Logger.writeEvent("MarketFacade: MakeDiscountNotAllowed| started");
            if (!Enum.TryParse(discountTypeString, true, out DiscountType discountType))
            {
                throw new ArgumentException(string.Format("'{0}' is not a legal discountType string", discountTypeString));
            }
            GetUserAndUpdateTimestamp(sessionID, context);
            try
            {
                Guid userID = GetUserIDBySessionID_Throws(sessionID, context);
                bool success = storeManagement.MakeDiscountNotAllowed(userID, storeID, discountType, context);
                Logger.writeEvent("MarketFacade: MakeDiscountNotAllowed| succeeded");
                return Create_json_response(true, success);
            }
            catch (Exception e)
            {
                Logger.writeError(e);
                Logger.writeEvent("MarketFacade: MakeDiscountNotAllowed| failed");
                return Create_json_response(false, e);
            }
        }

        public string MakeDiscountAllowed(Guid sessionID, Guid storeID, string discountTypeString)
        {
            using var context = getMarketDBContext();
            Logger.writeEvent("MarketFacade: MakeDiscountAllowed| started");
            GetUserAndUpdateTimestamp(sessionID, context);
            try
            {
                Guid userID = GetUserIDBySessionID_Throws(sessionID, context);
                if (!Enum.TryParse(discountTypeString, true, out DiscountType discountType))
                {
                    throw new ArgumentException(string.Format("'{0}' is not a legal discountType string", discountTypeString));
                }

                bool success = storeManagement.MakeDiscountAllowed(userID, storeID, discountType, context);
                Logger.writeEvent("MarketFacade: MakeDiscountNotAllowed| succeeded");
                return Create_json_response(true, success);
            }
            catch (Exception e)
            {
                Logger.writeError(e);
                Logger.writeEvent("MarketFacade: MakeDiscountNotAllowed| failed");
                return Create_json_response(false, e);
            }
        }

        public string GetAllDicsounts(Guid sessionID, Guid storeID, Guid? itemID)
        {
            using var context = getMarketDBContext();
            ValidateStoreExistIfNotThrowArgumentException(storeID, context);

            Logger.writeEvent("MarketFacade: GetAllDicsounts| started");
            GetUserAndUpdateTimestamp(sessionID, context);
            try
            {
                Guid userID = GetUserIDBySessionID_Throws(sessionID, context);
               
                List<ADisountDataClassForSerialization> discounts = storeManagement.GetAllDiscounts(userID, storeID, itemID, context);
                Logger.writeEvent("MarketFacade: GetAllDicsounts| succeeded");
                return Create_json_response(true, discounts);
            }
            catch (Exception e)
            {
                Logger.writeError(e);
                Logger.writeEvent("MarketFacade: GetAllDicsounts| failed");
                return Create_json_response(false, e);
            }
        }

        public string GetAllowedDiscounts(Guid sessionID, Guid storeID)
        {
            using var context = getMarketDBContext();
            Logger.writeEvent("MarketFacade: GetAllowedDiscounts| started");
            GetUserAndUpdateTimestamp(sessionID, context);
            try
            {
                Guid userID = GetUserIDBySessionID_Throws(sessionID, context);
                List<DiscountType> discountTypes = storeManagement.GetAllowedDiscounts(storeID, userID, context);
                List<string> discountTypeStrings = discountTypes.ConvertAll(p => p.ToString());
                Logger.writeEvent("MarketFacade: GetAllowedDiscounts| succeeded");
                return Create_json_response(true, discountTypeStrings);
            }
            catch (CertificationException e)
            {
                Logger.writeError(e);
                Logger.writeEvent(string.Format("MarketFacade: GetAllowedDiscounts| user has no permissions to store: {0}", storeID));
                return Create_json_response(true, new List<string>());
            }
            catch (Exception e)
            {
                Logger.writeError(e);
                Logger.writeEvent("MarketFacade: GetAllowedDiscounts| failed");
                return Create_json_response(false, e);
            }
        }

        public string GetStoresWithPermissions(Guid sessionID)
        {
            using var context = getMarketDBContext();
            Logger.writeEvent("MarketFacade: GetStoresWithPermissions| started");
            GetUserAndUpdateTimestamp(sessionID, context);
            try
            {
                Guid userID = GetUserIDBySessionID_Throws(sessionID, context);
                
                List<Tuple<Store,List<Permission>>> storesWithPermissions = storeManagement.GetStoresWithPermissions(userID, context);
                List<Tuple<Store, List<string>>> result = new List<Tuple<Store, List<string>>>();
                foreach(var item in storesWithPermissions)
                {
                    result.Add(new Tuple<Store, List<string>>(item.Item1,
                        item.Item2.ConvertAll(p => p.ToString())));
                }
                Logger.writeEvent("MarketFacade: GetStoresWithPermissions| succeeded");
                return Create_json_response(true, result);
            }
            catch (Exception e)
            {
                Logger.writeError(e);
                Logger.writeEvent("MarketFacade: GetStoresWithPermissions| failed");
                return Create_json_response(false, e);
            }
        }

        public bool IsValidToCreateStoreConditionalDiscount( Guid storeID)
        {
            using var context = getMarketDBContext();
            return storeManagement.IsValidToCreateStoreConditionalDiscount(storeID, context);
        }

        public bool IsValidToCreateItemOpenedDiscount( Guid storeID, Guid itemID)
        {
            using var context = getMarketDBContext();
            return storeManagement.IsValidToCreateItemOpenedDiscount(storeID, itemID, context);
        }

        public bool IsValidToCreateItemConditionalDiscount(Guid storeID, Guid itemID)
        {
            using var context = getMarketDBContext();
            return storeManagement.IsValidToCreateItemConditionalDiscount(storeID, itemID, context);
        }

        public bool IsRegisteredUser(string username)
        {
            using var context = getMarketDBContext();
            return userManager.HasUser(username, context);
        }

        #endregion

        #region Messages
        public string GetMyMessages(Guid sessionID)
        {
            using var context = getMarketDBContext();
            try
            {
                User user = GetUserAndUpdateTimestamp(sessionID, context);
                List<string> messages = userManager.GetUserMessages(user, context);
                return Create_json_response(true, messages);
            }
            catch (Exception e)
            {
                Logger.writeError(e);
                return Create_json_response(false, e);
            }
        }

        public string HasMessages(Guid sessionID)
        {
            using var context = getMarketDBContext();
            try
            {
                User user = GetUserAndUpdateTimestamp(sessionID, context);
                bool ans = user.HasAwaitingMessages();
                return Create_json_response(true, ans);
            }
            catch (Exception e)
            {
                Logger.writeError(e);
                return Create_json_response(false, e);
            }
        }
        #endregion Messages

        #region purchasePolicies

        public bool IsValidToCreateItemMinMaxPurchasePolicy(Guid storeID, Guid itemID)
        {
            using var context = getMarketDBContext();
            return storeManagement.IsValidToCreateItemMinMaxPurchasePolicy(storeID,itemID, context);
        }

        public string AddItemMinMaxPurchasePolicy(Guid sessionID, Guid storeID, Guid itemID, int? minAmount, int? maxAmount)
        {
            using var context = getMarketDBContext();
            Logger.writeEvent("MarketFacade: AddItemMinMaxPurchasePolicy| started");
            GetUserAndUpdateTimestamp(sessionID, context);
            try
            {
                Guid userID = GetUserIDBySessionID_Throws(sessionID, context);
                ItemMinMaxPurchasePolicy policy = storeManagement.AddItemMinMaxPurchasePolicy(storeID, userID, itemID, minAmount,
                    maxAmount, context);
                Logger.writeEvent("MarketFacade: AddItemMinMaxPurchasePolicy| succeeded");
                return Create_json_response(true, policy);
            }
            catch (Exception e)
            {
                Logger.writeError(e);
                Logger.writeEvent("MarketFacade: AddItemMinMaxPurchasePolicy| failed");
                return Create_json_response(false, e);
            }
        }

        public bool IsValidToCreateStoreMinMaxPurchasePolicy(Guid storeID)
        {
            using var context = getMarketDBContext();
            return storeManagement.IsValidToCreateStoreMinMaxPurchasePolicy(storeID, context);
        }

        public string AddStoreMinMaxPurchasePolicy(Guid sessionID, Guid storeID, int? minAmount, int? maxAmount)
        {
            using var context = getMarketDBContext();
            Logger.writeEvent("MarketFacade: AddStoreMinMaxPurchasePolicy| started");
            GetUserAndUpdateTimestamp(sessionID, context);
            try
            {
                Guid userID = GetUserIDBySessionID_Throws(sessionID, context);
                StoreMinMaxPurchasePolicy policy = storeManagement.AddStoreMinMaxPurchasePolicy(storeID, userID, minAmount,
                    maxAmount, context);
                Logger.writeEvent("MarketFacade: AddStoreMinMaxPurchasePolicy| succeeded");
                return Create_json_response(true, policy);
            }
            catch (Exception e)
            {
                Logger.writeError(e);
                Logger.writeEvent("MarketFacade: AddStoreMinMaxPurchasePolicy| failed");
                return Create_json_response(false, e);
            }
        }

        public bool IsValidToCreateDaysNotAllowedPurchasePolicy(Guid storeID)
        {
            using var context = getMarketDBContext();
            return storeManagement.IsValidToCreateDaysNotAllowedPurchasePolicy(storeID, context);
        }

        public string AddDaysNotAllowedPurchasePolicy(Guid sessionID, Guid storeID, int[] daysNotAllowed)
        {
            using var context = getMarketDBContext();
            Logger.writeEvent("MarketFacade: AddDaysNotAllowedPurchasePolicy| started");
            GetUserAndUpdateTimestamp(sessionID, context);
            try
            {
                Guid userID = GetUserIDBySessionID_Throws(sessionID, context);
                DaysNotAllowedPurchasePolicy policy = storeManagement.AddDaysNotAllowedPurchasePolicy(storeID, userID, daysNotAllowed, context);
                Logger.writeEvent("MarketFacade: AddDaysNotAllowedPurchasePolicy| succeeded");
                return Create_json_response(true, policy);
            }
            catch (Exception e)
            {
                Logger.writeError(e);
                Logger.writeEvent("MarketFacade: AddDaysNotAllowedPurchasePolicy| failed");
                return Create_json_response(false, e);
            }
        }

        public bool PurchasePolicyExistsInStore(Guid storeID, Guid policyID)
        {
            using var context = getMarketDBContext();
            ValidateStoreExistIfNotThrowArgumentException(storeID, context);

            bool ans = storeManagement.PolicyExistsInStore(storeID, policyID, context);
            return ans;
        }

        public string ComposeTwoPurchasePolicys(Guid sessionID, Guid storeID, Guid policyLeftID, Guid policyRightID, string boolOperator)
        {
            using var context = getMarketDBContext();
            Logger.writeEvent("MarketFacade: ComposeTwoPurchasePolicys| started");
            GetUserAndUpdateTimestamp(sessionID, context);
            try
            {
                Guid userID = GetUserIDBySessionID_Throws(sessionID, context);
                CompositePurchasePolicy compositeDiscount = storeManagement.ComposeTwoPurchasePolicys(userID, storeID,
                    policyLeftID, policyRightID, boolOperator, context);
                Logger.writeEvent("MarketFacade: ComposeTwoPurchasePolicys| succeeded");
                return Create_json_response(true, compositeDiscount);
            }
            catch (Exception e)
            {
                Logger.writeError(e);
                Logger.writeEvent("MarketFacade: ComposeTwoPurchasePolicys| failed");
                return Create_json_response(false, e);
            }
        }

        public bool IsPurchaseTypeAllowed(Guid storeID, string purchasePolicy)
        {
            using var context = getMarketDBContext();
            ValidateStoreExistIfNotThrowArgumentException(storeID, context);
            if (!Enum.TryParse(purchasePolicy, true, out PurchasePolicyType policy))
            {
                throw new ArgumentException(string.Format("'{0}' is not a legal purchasePolicy string", purchasePolicy));
            }

            return storeManagement.IsPurchaseTypeAllowed(storeID, policy, context);
        }

        public string MakePurcahsePolicyNotAllowed(Guid sessionID, Guid storeID, string purchasePolicy)
        {
            using var context = getMarketDBContext();
            Logger.writeEvent("MarketFacade: MakePurcahsePolicyNotAllowed| started");
            if (!Enum.TryParse(purchasePolicy, true, out PurchasePolicyType policy))
            {
                throw new ArgumentException(string.Format("'{0}' is not a legal purchasePolicy string", purchasePolicy));
            }
            GetUserAndUpdateTimestamp(sessionID, context);
            try
            {
                Guid userID = GetUserIDBySessionID_Throws(sessionID, context);
                bool success = storeManagement.MakePurcahsePolicyNotAllowed(userID, storeID, policy, context);
                Logger.writeEvent("MarketFacade: MakePurcahsePolicyNotAllowed| succeeded");
                return Create_json_response(true, success);
            }
            catch (Exception e)
            {
                Logger.writeError(e);
                Logger.writeEvent("MarketFacade: MakePurcahsePolicyNotAllowed| failed");
                return Create_json_response(false, e);
            }
        }

        public string MakePurcahsePolicyAllowed(Guid sessionID, Guid storeID, string purchasePolicy)
        {
            using var context = getMarketDBContext();
            Logger.writeEvent("MarketFacade: MakePurcahsePolicyAllowed| started");
            GetUserAndUpdateTimestamp(sessionID, context);
            try
            {
                Guid userID = GetUserIDBySessionID_Throws(sessionID, context);
                if (!Enum.TryParse(purchasePolicy, true, out PurchasePolicyType policy))
                {
                    throw new ArgumentException(string.Format("'{0}' is not a legal purchasePolicy string", purchasePolicy));
                }

                bool success = storeManagement.MakePurcahsePolicyAllowed(userID, storeID, policy, context);
                Logger.writeEvent("MarketFacade: MakePurcahsePolicyAllowed| succeeded");
                return Create_json_response(true, success);
            }
            catch (Exception e)
            {
                Logger.writeError(e);
                Logger.writeEvent("MarketFacade: MakePurcahsePolicyAllowed| failed");
                return Create_json_response(false, e);
            }
        }

        public string RemovePurchasePolicy(Guid sessionID, Guid storeID, Guid policyID)
        {
            using var context = getMarketDBContext();
            Logger.writeEvent("MarketFacade: RemovePurchasePolicy| started");
            GetUserAndUpdateTimestamp(sessionID, context);
            try
            {
                Guid userID = GetUserIDBySessionID_Throws(sessionID, context);
                bool success = storeManagement.RemovePurchasePolicy(userID, storeID, policyID, context);
                Logger.writeEvent("MarketFacade: RemovePurchasePolicy| succeeded");
                return Create_json_response(true, success);
            }
            catch (Exception e)
            {
                Logger.writeError(e);
                Logger.writeEvent("MarketFacade: RemovePurchasePolicy| failed");
                return Create_json_response(false, e);
            }
        }

        public string GetAllPurchasePolicys(Guid sessionID, Guid storeID)
        {
            using var context = getMarketDBContext();
            ValidateStoreExistIfNotThrowArgumentException(storeID, context);

            Logger.writeEvent("MarketFacade: GetAllPurchasePolicys| started");
            GetUserAndUpdateTimestamp(sessionID, context);
            try
            {
                Guid userID = GetUserIDBySessionID_Throws(sessionID, context);

                List<APurchasePolicyDataClassForSerialization> policys = storeManagement.GetAllPurchasePolicys(userID, storeID, context);
                Logger.writeEvent("MarketFacade: GetAllPurchasePolicys| succeeded");
                return Create_json_response(true, policys);
            }
            catch (Exception e)
            {
                Logger.writeError(e);
                Logger.writeEvent("MarketFacade: GetAllPurchasePolicys| failed");
                return Create_json_response(false, e);
            }
        }

        public string GetAllowedPurchasePolicys(Guid sessionID, Guid storeID)
        {
            using var context = getMarketDBContext();
            Logger.writeEvent("MarketFacade: GetAllowedPurchasePolicys| started");
            GetUserAndUpdateTimestamp(sessionID, context);
            try
            {
                Guid userID = GetUserIDBySessionID_Throws(sessionID, context);
                List<PurchasePolicyType> policyTypes = storeManagement.GetAllowedPurchasePolicys(storeID, userID, context);
                List<string> policyTypesStrings = policyTypes.ConvertAll(p => p.ToString());
                Logger.writeEvent("MarketFacade: GetAllowedPurchasePolicys| succeeded");
                return Create_json_response(true, policyTypesStrings);
            }
            catch (CertificationException e)
            {
                Logger.writeError(e);
                Logger.writeEvent(string.Format("MarketFacade: GetAllowedPurchasePolicys| user has no permissions to store: {0}", storeID));
                return Create_json_response(false, false);
            }
            catch (Exception e)
            {
                Logger.writeError(e);
                Logger.writeEvent("MarketFacade: GetAllowedPurchasePolicys| failed");
                return Create_json_response(false, e);
            }
        }

        #endregion
        public string GetDailyStatistics(Guid sessionID, DateTime from, DateTime? to = null)
        {
            Logger.writeEvent("MarketFacade: GetDailyStatistics| started");
            try
            {
                if (IsAdmin(sessionID))
                {
                    Dictionary<DateTime, int[]> statistics = GetAdminStatistics(from, to);
                    Logger.writeEvent("MarketFacade: GetDailyStatistics| succeeded");
                    return Create_json_response(true, statistics);
                }
                Logger.writeEvent("MarketFacade: GetDailyStatistics| failed, not admin");
                return Create_json_response(false, "Only Admin can perform this action");
            }
            catch (Exception e)
            {
                Logger.writeError(e);
                Logger.writeEvent("MarketFacade: GetDailyStatistics| failed");
                return Create_json_response(false, e);
            }
        }

        internal Dictionary<DateTime, int[]> GetAdminStatistics(DateTime from, DateTime? to = null)
        {
            using var context = getMarketDBContext();
            return userManager.GetStatistics(context, from, to);
        }
    }

    internal class ResponseClass
    {
        public bool Success { get; set; }
        public string AnswerOrExceptionJson { get; set; }

        public ResponseClass(bool success, string answerOrExceptionJson)
        {
            this.Success = success;
            this.AnswerOrExceptionJson = answerOrExceptionJson;
        }

        public ResponseClass() { }

    }

}

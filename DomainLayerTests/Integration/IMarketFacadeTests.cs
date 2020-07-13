using DomainLayer.DbAccess;
using DomainLayer.Exceptions;
using DomainLayer.Market;
using DomainLayer.Orders;
using DomainLayer.Stores;
using DomainLayer.Stores.Discounts;
using DomainLayer.Stores.Inventory;
using DomainLayer.Stores.PurchasePolicies;
using DomainLayer.Users;
using DomainLayerTests.UnitTests.Data;
using Newtonsoft.Json;
using NotificationsManagment;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace DomainLayerTests.Integration
{
	class IMarketFacadeTests
	{
		internal MarketFacade marketFacade;
		internal Guid GUEST_SESSION_ID;
		internal Guid REGISTERED_SESSION_ID;
		internal Guid ADMIN_SESSION_ID;
		internal const string FIRST_OPENER_USERNAME = "Opener1";
		internal const string FIRST_OWNER_USERNAME = "Owner1";
		internal const string SECOND_OPENER_USERNAME = "Opener2";
		internal const string SECOND_OWNER_USERNAME = "Owner2";
		internal const string STORE_MANAGER_USERNAME = "Manager";
		internal const string BUYER_USERNAME = "Buyer";
		internal const string NEW_USERNAME = "New";
		internal const string PASSWORD = "1234";
		internal const string WRONG_PASSWORD = "666";
		internal const string FIRST_STORE_NAME = "store1";
		internal const string SECOND_STORE_NAME = "store2";
		internal const string ADMIN_USERNAME = "Admin";
		internal const string FIRST_ITEM_FIRST_STORE_NAME = "item1";
		internal const string SECOND_ITEM_FIRST_STORE_NAME = "item2";
		internal const string THIRD_ITEM_FIRST_STORE_NAME = "item3";
		internal const string FIRST_ITEM_SECOND_STORE_NAME = "item4";
		internal const string SECOND_ITEM_SECOND_STORE_NAME = "item5";
		internal const string THIRD_ITEM_SECOND_STORE_NAME = "item6";
		internal const int FIRST_ITEM_FIRST_STORE_AMOUNT = 30;
		internal const int SECOND_ITEM_FIRST_STORE_AMOUNT = 30;
		internal const int THIRD_ITEM_FIRST_STORE_AMOUNT = 200;
		internal const int FIRST_ITEM_SECOND_STORE_AMOUNT = 20;
		internal const int SECOND_ITEM_SECOND_STORE_AMOUNT = 25;
		internal const int THIRD_ITEM_SECOND_STORE_AMOUNT = 1;
		internal const double FIRST_ITEM_FIRST_STORE_PRICE = 20.5;
		internal const double SECOND_ITEM_FIRST_STORE_PRICE = 20;
		internal const double THIRD_ITEM_FIRST_STORE_PRICE = 50;
		internal const double FIRST_ITEM_SECOND_STORE_PRICE = 20.5;
		internal const double SECOND_ITEM_SECOND_STORE_PRICE = 23.5;
		internal const double THIRD_ITEM_SECOND_STORE_PRICE = 203.5;
		internal const int FIRST_ITEM_FIRST_STORE_PURCHASE_AMOUNT = 12;
		internal const int SECOND_ITEM_FIRST_STORE_PURCHASE_AMOUNT = 10;
		internal const int THIRD_ITEM_FIRST_STORE_PURCHASE_AMOUNT = 50;
		internal const int FIRST_ITEM_SECOND_STORE_PURCHASE_AMOUNT = 5;
		internal HashSet<string> hashCategories1 = new HashSet<string>() { "cat1" };
		internal string[] stringCategories1 = new string[] { "cat1" };
		internal string[] stringCategories2 = new string[] { "cat2" };
		internal string[] stringCategories3 = new string[] { "cat1", "cat2" };
		internal string[] stringKeywords1 = new string[] { "word1" };
		internal string[] stringKeywords2 = new string[] { "word2" };
		internal string[] stringKeywords3 = new string[] { "word3" };
		internal Store FIRST_STORE;
		internal Store SECOND_STORE;
		internal Guid FIRST_STORE_ID;
		internal Guid SECOND_STORE_ID;
		internal Guid FIRST_ITEM_FIRST_STORE_ID;
		internal Guid SECOND_ITEM_FIRST_STORE_ID;
		internal Guid THIRD_ITEM_FIRST_STORE_ID;
		internal Guid FIRST_ITEM_SECOND_STORE_ID;
		internal Guid SECOND_ITEM_SECOND_STORE_ID;
		internal Guid THIRD_ITEM_SECOND_STORE_ID;

		[SetUp]
		public void Setup()
		{
			//market
			using var context = new MarketDbContext(Effort.DbConnectionFactory.CreateTransient());
            context.Init();
			marketFacade = new MarketFacade(ADMIN_USERNAME, PASSWORD, new NotificationManager(), true);
			GUEST_SESSION_ID = Guid.NewGuid();
			REGISTERED_SESSION_ID = Guid.NewGuid();
			ADMIN_SESSION_ID = Guid.NewGuid();
		}

		internal void SetupUsers()
		{
			Guid TEMP_SESSION_ID = Guid.NewGuid();
			RegisterSessionSuccess(TEMP_SESSION_ID, FIRST_OPENER_USERNAME);
			RegisterSessionSuccess(TEMP_SESSION_ID, FIRST_OWNER_USERNAME);
			RegisterSessionSuccess(TEMP_SESSION_ID, SECOND_OPENER_USERNAME);
			RegisterSessionSuccess(TEMP_SESSION_ID, SECOND_OWNER_USERNAME);
			RegisterSessionSuccess(TEMP_SESSION_ID, STORE_MANAGER_USERNAME);
			RegisterSessionSuccess(TEMP_SESSION_ID, BUYER_USERNAME);
		}

		internal void SetupStores()
		{
			Guid TEMP_SESSION_ID = Guid.NewGuid();
			LoginSessionSuccess(TEMP_SESSION_ID, FIRST_OPENER_USERNAME);
			OpenStoreSuccess(TEMP_SESSION_ID, FIRST_STORE_NAME, out FIRST_STORE);
			FIRST_STORE_ID = FIRST_STORE.Id;
			LogoutSessionSuccess(TEMP_SESSION_ID);
			LoginSessionSuccess(TEMP_SESSION_ID, SECOND_OPENER_USERNAME);
			OpenStoreSuccess(TEMP_SESSION_ID, SECOND_STORE_NAME, out SECOND_STORE);
			SECOND_STORE_ID = SECOND_STORE.Id;
			LogoutSessionSuccess(TEMP_SESSION_ID);
		}

		internal void OpenStoreSuccess(Guid sessionID, string storeName, out Store store)
		{
			ResponseClass response = DeserializeOpenStore(sessionID, storeName);
			if (!response.Success)
			{
				throw new ActionOutcomeException();
			}
			store = JsonConvert.DeserializeObject<Store>(response.AnswerOrExceptionJson);
		}

		internal string OpenStoreError(Guid sessionID, string storeName)
		{
			ResponseClass response = DeserializeOpenStore(sessionID, storeName);
			return response.Success ? null : response.AnswerOrExceptionJson;
		}

		private ResponseClass DeserializeOpenStore(Guid sessionID, string storeName)
		{
			StoreContactDetails details = DataForTests.CreateTestContactDetails();
			string json = marketFacade.OpenStore(sessionID, storeName, details.Email, details.Address, details.Phone, details.BankAccountNumber,
													details.Bank, details.Description, null, null);
			return JsonConvert.DeserializeObject<ResponseClass>(json);
		}

		internal void RegisterSessionSuccess(Guid sessionID, string username)
		{
			ResponseClass response = DeserializeRegister(sessionID, username);
			if (!response.Success)
			{
				throw new ActionOutcomeException();
			}
		}

		internal string RegisterSessionError(Guid sessionID, string username)
		{
			ResponseClass response = DeserializeRegister(sessionID, username);
			return response.Success ? null : response.AnswerOrExceptionJson;
		}

		private ResponseClass DeserializeRegister(Guid sessionID, string username)
		{
			string json = marketFacade.Register(sessionID, username, PASSWORD);
			return JsonConvert.DeserializeObject<ResponseClass>(json);
		}

		internal void LoginSessionSuccess(Guid sessionID, string username = BUYER_USERNAME, string password = PASSWORD)
		{
			ResponseClass response = DeserializeLogin(sessionID, username, password);
			if (!response.Success)
			{
				throw new ActionOutcomeException();
			}
		}

		internal string LoginSessionError(Guid sessionID, string username = BUYER_USERNAME, string password = PASSWORD)
		{
			ResponseClass response = DeserializeLogin(sessionID, username, password);
			return response.Success ? null : response.AnswerOrExceptionJson;
		}

		private ResponseClass DeserializeLogin(Guid sessionID, string username, string password)
		{
			string json = marketFacade.Login(sessionID, username, password);
			return JsonConvert.DeserializeObject<ResponseClass>(json);
		}

		internal void LogoutSessionSuccess(Guid sessionID)
		{
			ResponseClass response = DeserializeLogout(sessionID);
			if (!response.Success)
			{
				throw new ActionOutcomeException();
			}
		}

		internal string LogoutSessionError(Guid sessionID)
		{
			ResponseClass response = DeserializeLogout(sessionID);
			return response.Success ? null : response.AnswerOrExceptionJson;
		}

		private ResponseClass DeserializeLogout(Guid sessionID)
		{
			string json = marketFacade.Logout(sessionID);
			return JsonConvert.DeserializeObject<ResponseClass>(json);
		}

		internal void GetAllStoresInformationSuccess(Guid sessionID, out ReadOnlyCollection<Store> allStoresInfo)
		{
			ResponseClass response = DeserializeGetAllStoresInformation(sessionID);

			if (!response.Success)
			{
				throw new ActionOutcomeException();
			}
			allStoresInfo = JsonConvert.DeserializeObject<ReadOnlyCollection<Store>>(response.AnswerOrExceptionJson);
		}

		internal Store GetStoreInformationByIDSuccess(Guid sessionID, Guid storeID)
		{
			ResponseClass response = DeserializeGetStoresInformation(sessionID, storeID);

			if (!response.Success)
			{
				throw new ActionOutcomeException();
			}
			return JsonConvert.DeserializeObject<Store>(response.AnswerOrExceptionJson);
		}

        internal ResponseClass DeserializeGetStoresInformation(Guid sessionID, Guid storeID)
        {
			string jsonAnswer = marketFacade.GetStoreInformationByID(sessionID, storeID);
			return JsonConvert.DeserializeObject<ResponseClass>(jsonAnswer);
		}

        internal string GetAllStoresInformationError(Guid sessionID)
		{
			ResponseClass response = DeserializeGetAllStoresInformation(sessionID);
			return response.Success ? null : response.AnswerOrExceptionJson;
		}

		private ResponseClass DeserializeGetAllStoresInformation(Guid sessionID)
		{
			string jsonAnswer = marketFacade.GetAllStoresInformation(sessionID);
			return JsonConvert.DeserializeObject<ResponseClass>(jsonAnswer);
		}

		internal void SearchItemsSuccess(
			Guid sessionID,
			out Dictionary<Guid, ReadOnlyCollection<Item>> filterdItems,
			double? filterItemRank,
			double? filterMinPrice,
			double? filterMaxPrice,
			double? filterStoreRank,
			string name = null,
			string category = null,
			string keywords = null)
		{
			ResponseClass response = DeserializeSearchItems(sessionID, filterItemRank, filterMinPrice, filterMaxPrice, filterStoreRank, name, category, keywords);

			if (!response.Success)
			{
				throw new ActionOutcomeException();
			}
			filterdItems = JsonConvert.DeserializeObject<Dictionary<Guid, ReadOnlyCollection<Item>>>(response.AnswerOrExceptionJson);
		}

		internal string SearchItemsError(
			Guid sessionID,
			double? filterItemRank,
			double? filterMinPrice,
			double? filterMaxPrice,
			double? filterStoreRank,
			string name = null,
			string category = null,
			string keywords = null)
		{
			ResponseClass response = DeserializeSearchItems(sessionID, filterItemRank, filterMinPrice, filterMaxPrice, filterStoreRank, 
																name, category, keywords);
			return !response.Success ? null : response.AnswerOrExceptionJson;
		}

		private ResponseClass DeserializeSearchItems(Guid sessionID, double? filterItemRank, double? filterMinPrice, double? filterMaxPrice, double? filterStoreRank, string name, string category, string keywords)
		{
			string jsonAnswer = marketFacade.SearchItems(sessionID, filterItemRank, filterMinPrice, filterMaxPrice, filterStoreRank, name, category, keywords);
			ResponseClass response = JsonConvert.DeserializeObject<ResponseClass>(jsonAnswer);
			return response;
		}

		internal void AddItemSuccess(Guid sessionID, out Item item, Guid storeID, string name, int amount, string categories, double price, string keyWords = null)
		{
			ResponseClass response = DeserializeAddItem(sessionID, storeID, name, amount, categories, price, keyWords);

			if (!response.Success)
			{
				throw new ActionOutcomeException();
			}

			item = JsonConvert.DeserializeObject<Item>(response.AnswerOrExceptionJson);
		}

		internal string AddItemError(Guid sessionID, Guid storeID, string name, int amount, string categories, double price, string keyWords = null)
		{
			ResponseClass response = DeserializeAddItem(sessionID, storeID, name, amount, categories, price, keyWords);

			return response.Success ? null : response.AnswerOrExceptionJson;
		}

		private ResponseClass DeserializeAddItem(Guid sessionID, Guid storeID, string name, int amount, string categories, double price, string keyWords)
		{
			string jsonAnswer = marketFacade.AddItem(sessionID, storeID, name, amount, price, categories, keyWords);
			ResponseClass response = JsonConvert.DeserializeObject<ResponseClass>(jsonAnswer);
			return response;
		}

		internal void DeleteItemSuccess(Guid sessionID, Guid storeID, Guid itemId)
		{
			ResponseClass response = DeserializeDeleteItem(sessionID, storeID, itemId);
			if (!response.Success)
			{
				throw new ActionOutcomeException();
			}
		}

		internal string DeleteItemError(Guid sessionID, Guid storeID, Guid itemId)
		{
			ResponseClass response = DeserializeDeleteItem(sessionID, storeID, itemId);
			return response.Success ? null : response.AnswerOrExceptionJson;
		}

		internal void EditItemSuccess(Guid sessionID, Guid storeID, Guid itemId, int? amount, double? rank, double? price, string name = null, string categories = null, string keyWords = null)
		{
			ResponseClass response = DeserializeEditItem(sessionID, storeID, itemId, amount, rank, price, name, categories, keyWords);
			if (!response.Success)
			{
				throw new ActionOutcomeException();
			}
		}

		internal string EditItemError(Guid sessionID, Guid storeID, Guid itemId, int? amount, double? rank, double? price, string name = null, string categories = null, string keyWords = null)
		{
			ResponseClass response = DeserializeEditItem(sessionID, storeID, itemId, amount, rank, price, name, categories, keyWords);
			return response.Success ? null : response.AnswerOrExceptionJson;
		}

		private ResponseClass DeserializeDeleteItem(Guid sessionID, Guid storeID, Guid itemId)
		{
			string json = marketFacade.DeleteItem(sessionID, storeID, itemId);
			ResponseClass response = JsonConvert.DeserializeObject<ResponseClass>(json);
			return response;
		}

		private ResponseClass DeserializeEditItem(Guid sessionID, Guid storeID, Guid itemId, int? amount, double? rank, double? price, string name = null, string categories = null, string keyWords = null)
		{
			string json = marketFacade.EditItem(sessionID, storeID, itemId, amount, rank, price, name, categories, keyWords);
			ResponseClass response = JsonConvert.DeserializeObject<ResponseClass>(json);
			return response;
		}

		private ResponseClass DeserializeAppointOwner(Guid sessionID, Guid storeID, string owner)
        {
            string json = marketFacade.AppointOwner(sessionID, storeID, owner );
            return JsonConvert.DeserializeObject<ResponseClass>(json);
        }

        internal void AppointOwnerSuccess(Guid sessionID, Guid storeID, string owner)
        {
            ResponseClass response = DeserializeAppointOwner(sessionID, storeID, owner);
            if (!response.Success)
            {
                throw new ActionOutcomeException();
            }
        }

		internal void ApproveOwnerSuccess(Guid sessionID, Guid storeID, string owner)
        {
			ResponseClass response = DeserializeApproveOwner(sessionID, storeID, owner);
			if (!response.Success)
			{
				throw new ActionOutcomeException();
			}
		}

        private ResponseClass DeserializeApproveOwner(Guid sessionID, Guid storeID, string owner)
        {
			string json = marketFacade.ApproveOwnerContract(sessionID, storeID, owner);
			return JsonConvert.DeserializeObject<ResponseClass>(json);
		}

        internal string AppointOwnerError(Guid sessionID, Guid storeID, string owner)
		{
			ResponseClass response = DeserializeAppointOwner(sessionID, storeID, owner);
			return response.Success ? null : response.AnswerOrExceptionJson;
		}

		private ResponseClass DeserializeAppointManager(Guid sessionID, Guid storeID, string owner)
		{
			string json = marketFacade.AppointManager(sessionID, storeID, owner);
			return JsonConvert.DeserializeObject<ResponseClass>(json);
		}

		internal void AppointManagerSuccess(Guid sessionID, Guid storeID, string owner)
		{
			ResponseClass response = DeserializeAppointManager(sessionID, storeID, owner);
			if (!response.Success)
			{
				throw new ActionOutcomeException();
			}
		}

		internal string AppointManagerError(Guid sessionID, Guid storeID, string owner)
		{
			ResponseClass response = DeserializeAppointManager(sessionID, storeID, owner);
			return response.Success ? null : response.AnswerOrExceptionJson;
		}

		private ResponseClass DeserializeAddPermission(Guid sessionID, Guid storeID, string owner, string permission)
		{
			string json = marketFacade.AddPermission(sessionID, storeID, owner, permission);
			return JsonConvert.DeserializeObject<ResponseClass>(json);
		}

		internal void AddPermissionSuccess(Guid sessionID, Guid storeID, string owner, string permission)
		{
			ResponseClass response = DeserializeAddPermission(sessionID, storeID, owner, permission);
			if (!response.Success)
			{
				throw new ActionOutcomeException();
			}
		}

		internal string AddPermissionError(Guid sessionID, Guid storeID, string owner, string permission)
		{
			ResponseClass response = DeserializeAddPermission(sessionID, storeID, owner, permission);
			return response.Success ? null : response.AnswerOrExceptionJson;
		}

		private ResponseClass DeserializeRemovePermission(Guid sessionID, Guid storeID, string owner, string permission)
		{
			string json = marketFacade.RemovePermission(sessionID, storeID, owner, permission);
			return JsonConvert.DeserializeObject<ResponseClass>(json);
		}

		internal void RemovePermissionSuccess(Guid sessionID, Guid storeID, string owner, string permission)
		{
			ResponseClass response = DeserializeRemovePermission(sessionID, storeID, owner, permission);
			if (!response.Success)
			{
				throw new ActionOutcomeException();
			}
		}

		internal string RemovePermissionError(Guid sessionID, Guid storeID, string owner, string permission)
		{
			ResponseClass response = DeserializeRemovePermission(sessionID, storeID, owner, permission);
			return response.Success ? null : response.AnswerOrExceptionJson;
		}

		private ResponseClass DeserializeRemoveOwner(Guid sessionID, Guid storeID, string owner)
		{
			string json = marketFacade.RemoveOwner(sessionID, storeID, owner);
			return JsonConvert.DeserializeObject<ResponseClass>(json);
		}

		internal void RemoveOwnerSuccess(Guid sessionID, Guid storeID, string owner)
		{
			ResponseClass response = DeserializeRemoveOwner(sessionID, storeID, owner);
			if (!response.Success)
			{
				throw new ActionOutcomeException();
			}
		}

		internal string RemoveOwnerError(Guid sessionID, Guid storeID, string owner)
		{
			ResponseClass response = DeserializeRemoveOwner(sessionID, storeID, owner);
			return response.Success ? null : response.AnswerOrExceptionJson;
		}

		private ResponseClass DeserializeRemoveManager(Guid sessionID, Guid storeID, string manager)
		{
			string json = marketFacade.RemoveManager(sessionID, storeID, manager);
			return JsonConvert.DeserializeObject<ResponseClass>(json);
		}

		internal void RemoveManagerSuccess(Guid sessionID, Guid storeID, string manager)
		{
			ResponseClass response = DeserializeRemoveManager(sessionID, storeID, manager);
			if (!response.Success)
			{
				throw new ActionOutcomeException();
			}
		}

		internal string RemoveManagerError(Guid sessionID, Guid storeID, string manager)
		{
			ResponseClass response = DeserializeRemoveManager(sessionID, storeID, manager);
			return response.Success ? null : response.AnswerOrExceptionJson;
		}

		private ResponseClass DeserializeGetMyPermissions(Guid sessionID, Guid storeID)
		{
			string json = marketFacade.GetMyPermissions(sessionID, storeID);
			return JsonConvert.DeserializeObject<ResponseClass>(json);
		}

		internal List<string> GetMyPermissionsSuccess(Guid sessionID, Guid storeID)
		{
			ResponseClass response = DeserializeGetMyPermissions(sessionID, storeID);
			if (!response.Success)
			{
				throw new ActionOutcomeException();
			}
			return JsonConvert.DeserializeObject<List<string>>(response.AnswerOrExceptionJson);
		}

		internal List<Tuple<Store, List<string>>> GetStoresWithPermissionsSuccess(Guid sessionID)
		{
			ResponseClass response = DeserializeGetStoresWithPermissions(sessionID);
			if (!response.Success)
			{
				throw new ActionOutcomeException();
			}
			return JsonConvert.DeserializeObject<List<Tuple<Store, List<string>>>>(response.AnswerOrExceptionJson);
		}

        internal ResponseClass DeserializeGetStoresWithPermissions(Guid sessionID)
        {
			string json = marketFacade.GetStoresWithPermissions(sessionID);
			return JsonConvert.DeserializeObject<ResponseClass>(json);
		}

		internal string GetMyPermissionsError(Guid sessionID, Guid storeID)
		{
			ResponseClass response = DeserializeGetMyPermissions(sessionID, storeID);
			return response.Success ? null : response.AnswerOrExceptionJson;
		}

		internal void AppointLoginManager(Guid sessionId, Guid storeID, string grantor, string manager = STORE_MANAGER_USERNAME)
		{
			LoginAppointManager(sessionId, storeID, grantor, manager);
			LogoutSessionSuccess(sessionId);
            int oldcount = GetTodayStats()[(int)Roles.MANAGER];
            LoginSessionSuccess(sessionId, manager);
            Assert.AreEqual(oldcount + 1, GetTodayStats()[(int)Roles.MANAGER]);
        }

		internal void LoginAppointManager(Guid sessionId, Guid storeID, string grantor, string manager)
		{
			LoginSessionSuccess(sessionId, grantor);
			AppointManagerSuccess(sessionId, storeID, manager);
		}

		public Order CheckoutSuccess(Guid sessionID)
        {
			ResponseClass response = DeserializeValidateAndDiscounts(sessionID);
			if (!response.Success)
			{
				throw new ActionOutcomeException();
			}
			return JsonConvert.DeserializeObject<Order>(response.AnswerOrExceptionJson);
		}

		public string CheckoutError(Guid sessionID)
        {
			ResponseClass response = DeserializeValidateAndDiscounts(sessionID);
			return response.Success ? null : response.AnswerOrExceptionJson;
		}

		private ResponseClass DeserializeValidateAndDiscounts(Guid sessionID)
        {
			string json = marketFacade.DiscountAndReserve(sessionID);
			return JsonConvert.DeserializeObject<ResponseClass>(json);
		}

		public void AddToCartSuccess(Guid sessionID, Guid storeID, Guid itemID, int amount)
		{
			ResponseClass response = DeserializeAddToCart(sessionID, storeID, itemID, amount);
			if (!response.Success)
			{
				throw new ActionOutcomeException();
			}
		}

		public string AddToCartError(Guid sessionID, Guid storeID, Guid itemID, int amount)
		{
			ResponseClass response = DeserializeAddToCart(sessionID, storeID, itemID, amount);
			return response.Success ? null : response.AnswerOrExceptionJson;
		}

		private ResponseClass DeserializeAddToCart(Guid sessionID, Guid storeID, Guid itemID, int amount)
		{
			string json = marketFacade.AddToCart(sessionID, storeID, itemID, amount);
			return JsonConvert.DeserializeObject<ResponseClass>(json);
		}

		public void GetMyHistorySuccess(Guid sessionID)
        {
			ResponseClass response = DeserializeGetMyHistory(sessionID);
			if (!response.Success)
			{
				throw new ActionOutcomeException();
			}
		}

		public string GetMyHistoryError(Guid sessionID)
        {
			ResponseClass response = DeserializeGetMyHistory(sessionID);
			return response.Success ? null : response.AnswerOrExceptionJson;
		}

		private ResponseClass DeserializeGetMyHistory(Guid sessionID)
        {
			string json = marketFacade.GetMyOrderHistory(sessionID);
			return JsonConvert.DeserializeObject<ResponseClass>(json);
		}

		public void UserHistorySuccess(Guid sessionID, string username)
        {
			ResponseClass response = DeserializeUserHistory(sessionID, username);
			if (!response.Success)
			{
				throw new ActionOutcomeException();
			}
		}

		public string UserHistoryError(Guid sessionID, string username)
        {
			ResponseClass response = DeserializeUserHistory(sessionID, username);
			return response.Success ? null : response.AnswerOrExceptionJson;
		}

		private ResponseClass DeserializeUserHistory(Guid sessionID, string username)
        {
			string json = marketFacade.GetUserOrderHistory(sessionID, username);
			return JsonConvert.DeserializeObject<ResponseClass>(json);
		}

		public void StoreHistorySuccess(Guid sessionID, Guid storeId)
        {
			ResponseClass response = DeserializeStoreHistory(sessionID, storeId);
			if (!response.Success)
			{
				throw new ActionOutcomeException();
			}
		}

		public string StoreHistoryError(Guid sessionID, Guid storeId)
        {
			ResponseClass response = DeserializeStoreHistory(sessionID, storeId);
			return response.Success ? null : response.AnswerOrExceptionJson;
		}

		private ResponseClass DeserializeStoreHistory(Guid sessionID, Guid storeId)
        {
			string json = marketFacade.GetStoreOrderHistory(sessionID, storeId);
			return JsonConvert.DeserializeObject<ResponseClass>(json);
		}

		public void StoreHistoryAdminSuccess(Guid sessionID, Guid storeId)
        {
			ResponseClass response = DeserializeStoreHistoryAdmin(sessionID, storeId);
			if (!response.Success)
			{
				throw new ActionOutcomeException();
			}
		}

		public string StoreHistoryAdminError(Guid sessionID, Guid storeId)
        {
			ResponseClass response = DeserializeStoreHistoryAdmin(sessionID, storeId);
			return response.Success ? null : response.AnswerOrExceptionJson;
		}

		private ResponseClass DeserializeStoreHistoryAdmin(Guid sessionID, Guid storeId)
        {
			string json = marketFacade.GetStoreOrderHistoryAdmin(sessionID, storeId);
			return JsonConvert.DeserializeObject<ResponseClass>(json);
		}

		public List<string> DeserializeGetMyMessagesSuccess(Guid sessionID)
        {
			ResponseClass response = DeserializeGetMyMessages(sessionID);
			Assert.True(response.Success);
			return JsonConvert.DeserializeObject<List<string>>(response.AnswerOrExceptionJson);
		}

        public ResponseClass DeserializeGetMyMessages(Guid sessionID)
        {
			string jsonAnswer = marketFacade.GetMyMessages(sessionID);
			ResponseClass response = JsonConvert.DeserializeObject<ResponseClass>(jsonAnswer);
			return response;
		}

        #region DiscountHandling

        public List<string> DeserializeGetAllowedDiscountsSuccess(Guid sessionID, Guid storeID)
		{
			ResponseClass response = DeserializeGetAllowedDiscounts(sessionID, storeID);
			Assert.True(response.Success);
			return JsonConvert.DeserializeObject<List<string>>(response.AnswerOrExceptionJson);
		}

		public ResponseClass DeserializeGetAllowedDiscounts(Guid sessionID, Guid storeID)
		{
			string jsonAnswer = marketFacade.GetAllowedDiscounts(sessionID, storeID);
			ResponseClass response = JsonConvert.DeserializeObject<ResponseClass>(jsonAnswer);
			return response;
		}

		public List<string> DeserializeGetAllowedPurchasePolicySuccess(Guid sessionID, Guid storeID)
		{
			ResponseClass response = DeserializeGetAllowedPurchasePolicy(sessionID, storeID);
			Assert.True(response.Success);
			return JsonConvert.DeserializeObject<List<string>>(response.AnswerOrExceptionJson);
		}

		public ResponseClass DeserializeGetAllowedPurchasePolicy(Guid sessionID, Guid storeID)
		{
			string jsonAnswer = marketFacade.GetAllowedPurchasePolicys(sessionID, storeID);
			ResponseClass response = JsonConvert.DeserializeObject<ResponseClass>(jsonAnswer);
			return response;
		}

		public bool DeserializeMakeDiscountNotAllowedSuccess(Guid sessionID, Guid storeID, string discountTypeString)
		{
			ResponseClass response = DeserializeMakeDiscountNotAllowed(sessionID, storeID, discountTypeString);
			Assert.True(response.Success);
			return JsonConvert.DeserializeObject<bool>(response.AnswerOrExceptionJson);
		}

		public ResponseClass DeserializeMakeDiscountNotAllowed(Guid sessionID, Guid storeID, string discountTypeString)
		{
			string jsonAnswer = marketFacade.MakeDiscountNotAllowed(sessionID, storeID, discountTypeString);
			ResponseClass response = JsonConvert.DeserializeObject<ResponseClass>(jsonAnswer);
			return response;
		}

		public bool DeserializeMakePurchasePolicyNotAllowedSuccess(Guid sessionID, Guid storeID, string policy)
		{

			ResponseClass response = DeserializeMakePurchasePolicyNotAllowed(sessionID, storeID, policy);
			Assert.True(response.Success);
			return JsonConvert.DeserializeObject<bool>(response.AnswerOrExceptionJson);
		}

		public ResponseClass DeserializeMakePurchasePolicyNotAllowed(Guid sessionID, Guid storeID, string discountTypeString)
		{
			string jsonAnswer = marketFacade.MakePurcahsePolicyNotAllowed(sessionID, storeID, discountTypeString);
			ResponseClass response = JsonConvert.DeserializeObject<ResponseClass>(jsonAnswer);
			return response;
		}

		public bool DeserializeMakeDiscountAllowedSuccess(Guid sessionID, Guid storeID, string discountTypeString)
		{
			ResponseClass response = DeserializeMakeDiscountAllowed(sessionID, storeID, discountTypeString);
			Assert.True(response.Success);
			return JsonConvert.DeserializeObject<bool>(response.AnswerOrExceptionJson);
		}

		public ResponseClass DeserializeMakeDiscountAllowed(Guid sessionID, Guid storeID, string discountTypeString)
		{
			string jsonAnswer = marketFacade.MakeDiscountAllowed(sessionID, storeID, discountTypeString);
			ResponseClass response = JsonConvert.DeserializeObject<ResponseClass>(jsonAnswer);
			return response;
		}

		public bool DeserializeMakePurchasePolicyAllowedSuccess(Guid sessionID, Guid storeID, string policy)
		{
			ResponseClass response = DeserializeMakePurchasePolicyAllowed(sessionID, storeID, policy);
			Assert.True(response.Success);
			return JsonConvert.DeserializeObject<bool>(response.AnswerOrExceptionJson);
		}

		public ResponseClass DeserializeMakePurchasePolicyAllowed(Guid sessionID, Guid storeID, string policy)
		{
			string jsonAnswer = marketFacade.MakePurcahsePolicyAllowed(sessionID, storeID, policy);
			ResponseClass response = JsonConvert.DeserializeObject<ResponseClass>(jsonAnswer);
			return response;
		}

		public bool DeserializeRemoveDiscountSuccess(Guid sessionID, Guid storeID, Guid discountID)
		{
			ResponseClass response = DeserializeRemoveDiscount(sessionID, storeID, discountID);
			Assert.True(response.Success);
			return JsonConvert.DeserializeObject<bool>(response.AnswerOrExceptionJson);
		}

		public ResponseClass DeserializeRemoveDiscount(Guid sessionID, Guid storeID, Guid discountID)
		{

			string jsonAnswer = marketFacade.RemoveDiscount(sessionID, storeID, discountID);
			ResponseClass response = JsonConvert.DeserializeObject<ResponseClass>(jsonAnswer);
			return response;
		}

		public bool DeserializeRemovePurchasePolicySuccess(Guid sessionID, Guid storeID, Guid policyID)
		{
			ResponseClass response = DeserializeRemovePurchasePolicy(sessionID, storeID, policyID);
			Assert.True(response.Success);
			return JsonConvert.DeserializeObject<bool>(response.AnswerOrExceptionJson);
		}

		public ResponseClass DeserializeRemovePurchasePolicy(Guid sessionID, Guid storeID, Guid policyID)
		{

			string jsonAnswer = marketFacade.RemovePurchasePolicy(sessionID, storeID, policyID);
			ResponseClass response = JsonConvert.DeserializeObject<ResponseClass>(jsonAnswer);
			return response;
		}

		public StoreConditionalDiscount DeserializeStoreConditionalDiscountSuccess(Guid sessionID, Guid storeID, int durationInDays, double minPurchase, double discount)
		{
			ResponseClass response = DeserializeStoreConditionalDiscount(sessionID, storeID, durationInDays, minPurchase, discount);
			Assert.True(response.Success);
			return JsonConvert.DeserializeObject<StoreConditionalDiscount>(response.AnswerOrExceptionJson);
		}

		public ResponseClass DeserializeStoreConditionalDiscount(Guid sessionID, Guid storeID, int durationInDays, double minPurchase, double discount)
		{
			string jsonAnswer = marketFacade.AddStoreConditionalDiscount(sessionID, storeID, durationInDays, minPurchase, discount);
			ResponseClass response = JsonConvert.DeserializeObject<ResponseClass>(jsonAnswer);
			return response;
		}

		public OpenDiscount DeserializeOpenDiscountSuccess(Guid sessionID, Guid StoreID, Guid itemID, double discount, int durationInDays)
		{
			ResponseClass response = DeserializeOpenDiscount(sessionID, StoreID, itemID, discount, durationInDays);
			Assert.True(response.Success);
			return JsonConvert.DeserializeObject<OpenDiscount>(response.AnswerOrExceptionJson);
		}

		public ResponseClass DeserializeOpenDiscount(Guid sessionID, Guid StoreID, Guid itemID, double discount, int durationInDays)
		{
			string jsonAnswer = marketFacade.AddOpenDiscount(sessionID, StoreID, itemID, discount, durationInDays);
			ResponseClass response = JsonConvert.DeserializeObject<ResponseClass>(jsonAnswer);
			return response;
		}

		public ItemMinMaxPurchasePolicy DeserialzeItemMinMaxSuccess(Guid sessionID, Guid StoreID, Guid itemID, int? minAmount, int? maxAmount)
		{
			ResponseClass response = DeserialzeItemMinMax(sessionID, StoreID, itemID, minAmount, maxAmount);
			Assert.True(response.Success);
			return JsonConvert.DeserializeObject<ItemMinMaxPurchasePolicy>(response.AnswerOrExceptionJson);
		}

		public ResponseClass DeserialzeItemMinMax(Guid sessionID, Guid StoreID, Guid itemID, int? minAmount, int? maxAmount)
		{
			string jsonAnswer = marketFacade.AddItemMinMaxPurchasePolicy(sessionID, StoreID, itemID, minAmount, maxAmount);
			ResponseClass response = JsonConvert.DeserializeObject<ResponseClass>(jsonAnswer);
			return response;
		}

        public DaysNotAllowedPurchasePolicy DeserialzeDaysNotAllowedSucces(Guid sessionID, Guid StoreID, int[] daysNotAllowed)
        {
            ResponseClass response = DeserialzeDaysNotAllowed(sessionID, StoreID, daysNotAllowed);
            Assert.True(response.Success);
            return JsonConvert.DeserializeObject<DaysNotAllowedPurchasePolicy>(response.AnswerOrExceptionJson);
        }

        public ResponseClass DeserialzeDaysNotAllowed(Guid sessionID, Guid StoreID, int[] daysNotAllowed)
        {
            string jsonAnswer = marketFacade.AddDaysNotAllowedPurchasePolicy(sessionID, StoreID, daysNotAllowed);
            ResponseClass response = JsonConvert.DeserializeObject<ResponseClass>(jsonAnswer);
            return response;
        }

		public CompositePurchasePolicy DeserializeCompositePurchasePolicySucces(Guid sessionID, Guid StoreID, Guid policyLeftID, Guid policyRightID, string boolOperator)
		{
			ResponseClass response = DeserializeCompositePurchasePolicy(sessionID, StoreID, policyLeftID, policyRightID, boolOperator);
			Assert.True(response.Success);
			return JsonConvert.DeserializeObject<CompositePurchasePolicy>(response.AnswerOrExceptionJson);
		}

		public ResponseClass DeserializeCompositePurchasePolicy(Guid sessionID, Guid StoreID, Guid policyLeftID, Guid policyRightID, string boolOperator)
		{
			string jsonAnswer = marketFacade.ComposeTwoPurchasePolicys(sessionID, StoreID, policyLeftID,policyRightID,boolOperator);
			ResponseClass response = JsonConvert.DeserializeObject<ResponseClass>(jsonAnswer);
			return response;
		}

		public StoreMinMaxPurchasePolicy DeserialzeStoreMinMaxSuccess(Guid sessionID, Guid StoreID, int? minAmount, int? maxAmount)
		{
			ResponseClass response = DeserialzeStoreMinMax(sessionID, StoreID, minAmount, maxAmount);
			Assert.True(response.Success);
			return JsonConvert.DeserializeObject<StoreMinMaxPurchasePolicy>(response.AnswerOrExceptionJson);
		}

		public ResponseClass DeserialzeStoreMinMax(Guid sessionID, Guid StoreID, int? minAmount, int? maxAmount)
		{
			string jsonAnswer = marketFacade.AddStoreMinMaxPurchasePolicy(sessionID, StoreID, minAmount, maxAmount);
			ResponseClass response = JsonConvert.DeserializeObject<ResponseClass>(jsonAnswer);
			return response;
		}

		public ItemConditionalDiscount_MinItems_ToDiscountOnExtraItems DeserializeItemConditionalDiscount_discountOnExtraSuccess(
			Guid sessionID, Guid storeID, Guid itemID, int durationInDays, int minItems, int extraItems, double discountForExtra)
		{
			ResponseClass response = DeserializeItemConditionalDiscount_discountOnExtra(
				 sessionID, storeID, itemID, durationInDays, minItems, extraItems, discountForExtra);
			Assert.True(response.Success);
			return JsonConvert.DeserializeObject<ItemConditionalDiscount_MinItems_ToDiscountOnExtraItems>(response.AnswerOrExceptionJson);
		}

		public ResponseClass DeserializeItemConditionalDiscount_discountOnExtra(Guid sessionID, Guid storeID, Guid itemID, int durationInDays, int minItems, int extraItems, double discountForExtra)
		{
			string jsonAnswer = marketFacade.AddItemConditionalDiscount_MinItems_ToDiscountOnExtraItems(
				sessionID, storeID, itemID, durationInDays, minItems, extraItems, discountForExtra);
			ResponseClass response = JsonConvert.DeserializeObject<ResponseClass>(jsonAnswer);
			return response;
		}

		public ItemConditionalDiscount_MinItems_ToDiscountOnAll DeserializeItemConditionalDiscount_discountOnAllSuccess(
            Guid sessionID, Guid storeID, Guid itemID, double discount, int durationInDays, int minItems)
        {
			ResponseClass response = DeserializeItemConditionalDiscount_discountOnAll(
				 sessionID, storeID, itemID, durationInDays, minItems, discount);
			Assert.True(response.Success);
			return JsonConvert.DeserializeObject<ItemConditionalDiscount_MinItems_ToDiscountOnAll>(response.AnswerOrExceptionJson);
		}

		public ResponseClass DeserializeItemConditionalDiscount_discountOnAll(Guid sessionID, Guid storeID, Guid itemID, int durationInDays, int minItems, double discount)
		{
			string jsonAnswer = marketFacade.AddItemConditionalDiscount_MinItems_ToDiscountOnAll(
				sessionID, storeID, itemID, durationInDays, minItems, discount);
			ResponseClass response = JsonConvert.DeserializeObject<ResponseClass>(jsonAnswer);
			return response;
		}

		public CompositeDiscount DeserializeCompositeDiscountSuccess(Guid sessionID, Guid storeID, Guid discountLeftID, Guid discountRightID, string boolOperator)
		{
			ResponseClass response = DeserializeCompositeDiscount(
				 sessionID, storeID, discountLeftID, discountRightID, boolOperator);
			Assert.True(response.Success);
			return JsonConvert.DeserializeObject<CompositeDiscount>(response.AnswerOrExceptionJson);

		}

		public bool MarketOperationSuccess(string market_ret_str) {
            ResponseClass response = JsonConvert.DeserializeObject<ResponseClass>(market_ret_str);
			return response.Success;
		}

		public ResponseClass DeserializeCompositeDiscount(Guid sessionID, Guid storeID, Guid discountLeftID, Guid discountRightID, string boolOperator)
		{
			string jsonAnswer = marketFacade.ComposeTwoDiscounts(
				sessionID, storeID, discountLeftID, discountRightID, boolOperator);
			ResponseClass response = JsonConvert.DeserializeObject<ResponseClass>(jsonAnswer);
			return response;
		}

		#endregion

		internal void CreateItemsAndAddToCart(Guid guestSessionID, bool logOut)
		{
			AddItemsToStores(guestSessionID);

			AddToCart(guestSessionID, logOut);
		}

		internal void AddItemsToStores(Guid guestSessionID)
		{
			Item item = null;

			//FIRST OPENER
			Assert.DoesNotThrow(() => LoginSessionSuccess(guestSessionID, FIRST_OPENER_USERNAME));

			Assert.DoesNotThrow(() => AddItemSuccess(guestSessionID, out item, FIRST_STORE_ID, FIRST_ITEM_FIRST_STORE_NAME,
														FIRST_ITEM_FIRST_STORE_AMOUNT, JsonConvert.SerializeObject(stringCategories1),
														FIRST_ITEM_FIRST_STORE_PRICE, JsonConvert.SerializeObject(stringKeywords1)));
			Assert.IsNotNull(item);
			FIRST_ITEM_FIRST_STORE_ID = item.Id;

			Assert.DoesNotThrow(() => AddItemSuccess(guestSessionID, out item, FIRST_STORE_ID, SECOND_ITEM_FIRST_STORE_NAME,
														SECOND_ITEM_FIRST_STORE_AMOUNT, JsonConvert.SerializeObject(stringCategories2),
														SECOND_ITEM_FIRST_STORE_PRICE, JsonConvert.SerializeObject(stringKeywords2)));
			Assert.IsNotNull(item);
			SECOND_ITEM_FIRST_STORE_ID = item.Id;

			Assert.DoesNotThrow(() => AddItemSuccess(guestSessionID, out item, FIRST_STORE_ID, THIRD_ITEM_FIRST_STORE_NAME,
														THIRD_ITEM_FIRST_STORE_AMOUNT, JsonConvert.SerializeObject(stringCategories3),
														THIRD_ITEM_FIRST_STORE_PRICE, JsonConvert.SerializeObject(stringKeywords3)));
			Assert.IsNotNull(item);
			THIRD_ITEM_FIRST_STORE_ID = item.Id;

			Assert.DoesNotThrow(() => LogoutSessionSuccess(guestSessionID));

			//SECOND OPENER
			Assert.DoesNotThrow(() => LoginSessionSuccess(guestSessionID, SECOND_OPENER_USERNAME));
			Assert.DoesNotThrow(() => AddItemSuccess(guestSessionID, out item, SECOND_STORE_ID, FIRST_ITEM_SECOND_STORE_NAME,
														FIRST_ITEM_SECOND_STORE_AMOUNT, JsonConvert.SerializeObject(stringCategories2),
														FIRST_ITEM_SECOND_STORE_PRICE, JsonConvert.SerializeObject(stringKeywords2)));
			Assert.IsNotNull(item);
			FIRST_ITEM_SECOND_STORE_ID = item.Id;

			Assert.DoesNotThrow(() => AddItemSuccess(guestSessionID, out item, SECOND_STORE_ID, SECOND_ITEM_SECOND_STORE_NAME,
														SECOND_ITEM_SECOND_STORE_AMOUNT, JsonConvert.SerializeObject(stringCategories2),
														SECOND_ITEM_SECOND_STORE_PRICE, JsonConvert.SerializeObject(stringKeywords2)));
			Assert.IsNotNull(item);
			SECOND_ITEM_SECOND_STORE_ID = item.Id;

			Assert.DoesNotThrow(() => AddItemSuccess(guestSessionID, out item, SECOND_STORE_ID, THIRD_ITEM_SECOND_STORE_NAME,
														THIRD_ITEM_SECOND_STORE_AMOUNT, JsonConvert.SerializeObject(stringCategories2),
														THIRD_ITEM_SECOND_STORE_PRICE, JsonConvert.SerializeObject(stringKeywords2)));
			Assert.IsNotNull(item);
			THIRD_ITEM_SECOND_STORE_ID = item.Id;
			Assert.DoesNotThrow(() => LogoutSessionSuccess(guestSessionID));
		}

		internal void AddToCart(Guid guestSessionID, bool logOut)
		{
			//BUYER
			Assert.DoesNotThrow(() => LoginSessionSuccess(guestSessionID));// logs in as buyer by default
			Assert.DoesNotThrow(() => AddToCartSuccess(guestSessionID, FIRST_STORE_ID, FIRST_ITEM_FIRST_STORE_ID,
														FIRST_ITEM_FIRST_STORE_PURCHASE_AMOUNT));
			Assert.DoesNotThrow(() => AddToCartSuccess(guestSessionID, FIRST_STORE_ID, SECOND_ITEM_FIRST_STORE_ID,
														SECOND_ITEM_FIRST_STORE_PURCHASE_AMOUNT));
			Assert.DoesNotThrow(() => AddToCartSuccess(guestSessionID, FIRST_STORE_ID, THIRD_ITEM_FIRST_STORE_ID,
														THIRD_ITEM_FIRST_STORE_PURCHASE_AMOUNT));
			Assert.DoesNotThrow(() => AddToCartSuccess(guestSessionID, SECOND_STORE_ID, FIRST_ITEM_SECOND_STORE_ID, 
														FIRST_ITEM_SECOND_STORE_PURCHASE_AMOUNT));
			if (logOut)
			{
				Assert.DoesNotThrow(() => LogoutSessionSuccess(guestSessionID));
			}
		}

        internal void LogoutUserLoginBuyer(Guid sessionID)
        {
            Assert.DoesNotThrow(() => LogoutSessionSuccess(sessionID));
            Assert.DoesNotThrow(() => LoginSessionSuccess(sessionID));
        }

		internal int[] GetTodayStats()
        {
			return marketFacade.GetAdminStatistics(from: DateTime.Today)[DateTime.Today];
        }
	}
}

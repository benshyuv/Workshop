using DomainLayer.Exceptions;
using DomainLayer.Stores;
using DomainLayer.Market;
using DomainLayer.Stores.Inventory;
using DomainLayer.Users;
using Newtonsoft.Json;
using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace DomainLayerTests.Integration
{
	class MarketOrdersTests : IMarketFacadeTests
	{
		[SetUp]
		public new void Setup()
		{
			SetupUsers();
			SetupStores();
		}

		private void AdminLogin()
		{
			Assert.DoesNotThrow(() => marketFacade.Login(ADMIN_SESSION_ID, ADMIN_USERNAME, PASSWORD));
		}

		[Test]
		public void Checkout_LegalUser_ShouldPass()
		{
			CreateItemsAndAddToCart(REGISTERED_SESSION_ID, false);
			Assert.DoesNotThrow(() => CheckoutSuccess(REGISTERED_SESSION_ID));
		}

		[Test]
		public void Checkout_ItemNotFoundInCart_ShouldFail()
		{
			Guid sessionId = Guid.NewGuid();
			string json = CheckoutError(sessionId);
			Assert.IsNotNull(json);
			OrderNotValidException e = JsonConvert.DeserializeObject<OrderNotValidException>(json);
			Assert.IsNotNull(e);
			Assert.IsTrue(e.Message.Contains("shopping cart is null or empty"));
		}

		[Test]
		public void UserHistory_LegalUser_ShouldPass()
		{
			Checkout_LegalUser_ShouldPass();
			Assert.DoesNotThrow(() => GetMyHistorySuccess(REGISTERED_SESSION_ID));
		}

		[Test]
		public void UserHistory_NoOrdersForUser_ShouldFail()
		{
			Checkout_LegalUser_ShouldPass();
			string json = GetMyHistoryError(GUEST_SESSION_ID);
			Assert.IsNotNull(json);
			UserStateException e = JsonConvert.DeserializeObject<UserStateException>(json);
			Assert.IsNotNull(e);
			Assert.IsTrue(e.Message.Contains("Illegal action: Get UserID for user state Guest"));
		}

		[Test]
		public void UserHistoryByAdmin_LegalAdmin_ShouldPass()
		{
			Checkout_LegalUser_ShouldPass();

			Assert.DoesNotThrow(() => LoginSessionSuccess(ADMIN_SESSION_ID, ADMIN_USERNAME));
			Assert.DoesNotThrow(() => UserHistorySuccess(ADMIN_SESSION_ID, BUYER_USERNAME));
		}

		[Test]
		public void UserHistoryByAdmin_NotAdmin_ShouldFail()
		{
			Checkout_LegalUser_ShouldPass();

			string json = UserHistoryError(REGISTERED_SESSION_ID, BUYER_USERNAME);
			Assert.IsNotNull(json);
			string e = JsonConvert.DeserializeObject<string>(json);
			Assert.IsNotNull(e);
			Assert.IsTrue(e.Contains("Only an Admin has the authority to perform this action"));
		}

		[Test]
		public void StoreHistory_LegalManager_ShouldPass()
		{
			Checkout_LegalUser_ShouldPass();

			Assert.DoesNotThrow(() => LogoutSessionSuccess(REGISTERED_SESSION_ID));
			Assert.DoesNotThrow(() => LoginSessionSuccess(REGISTERED_SESSION_ID, FIRST_OPENER_USERNAME));
			Assert.DoesNotThrow(() => StoreHistorySuccess(REGISTERED_SESSION_ID, FIRST_STORE_ID));
		}

		[Test]
		public void StoreHistory_NotLegalManager_ShouldFail()
		{
			Checkout_LegalUser_ShouldPass();

			string json = StoreHistoryError(REGISTERED_SESSION_ID, FIRST_STORE_ID);
			Assert.IsNotNull(json);
			UserStateException e = JsonConvert.DeserializeObject<UserStateException>(json);
			Assert.IsNotNull(e);
			Assert.IsTrue(e.Message.Contains("No certification exists for userID:"));
		}

		[Test]
		public void StoreHistory_LegalAdmin_ShouldPass()
		{
			Checkout_LegalUser_ShouldPass();

			AdminLogin();
			Assert.DoesNotThrow(() => StoreHistoryAdminSuccess(ADMIN_SESSION_ID, SECOND_STORE_ID));
		}

		[Test]
		public void StoreHistory_NotAdmin_ShouldFail()
		{
			Checkout_LegalUser_ShouldPass();

			string json = StoreHistoryAdminError(GUEST_SESSION_ID, SECOND_STORE_ID);
			Assert.IsNotNull(json);
			string e = JsonConvert.DeserializeObject<string>(json);
			Assert.IsNotNull(e);
			Assert.IsTrue(e.Contains("Only Admin can perform this action"));
		}
	}
}

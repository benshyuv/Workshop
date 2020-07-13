using DomainLayer.Exceptions;
using Newtonsoft.Json;
using NUnit.Framework;

namespace DomainLayerTests.Integration
{
	class MarketUserTests : IMarketFacadeTests
	{
		[SetUp]
		public new void Setup() => SetupUsers();

		[Test]
		public void Register_Guest_LegalUserNameAndPassword_ShouldPass()
		{
			Assert.DoesNotThrow(() =>RegisterSessionSuccess(GUEST_SESSION_ID, NEW_USERNAME));
		}

		[Test]
		public void Register_ExistingUserName_ShouldFail()
		{
			Register_Guest_LegalUserNameAndPassword_ShouldPass();
			string json = RegisterSessionError(GUEST_SESSION_ID, NEW_USERNAME);
			Assert.IsNotNull(json);
			UserManagementException e = JsonConvert.DeserializeObject<UserManagementException>(json);
			Assert.IsNotNull(e);
			Assert.IsTrue(e.Message.Contains("Username already exsits"));
		}

		[Test]
		public void Register_LoggedInUser_ShouldFail()
		{
			Login_LegalUserNameAndPassword_ShouldPass();
			string json = RegisterSessionError(REGISTERED_SESSION_ID, NEW_USERNAME);
			Assert.IsNotNull(json);
			UserStateException e = JsonConvert.DeserializeObject<UserStateException>(json);
			Assert.IsNotNull(e);
			Assert.IsTrue(e.Message.Contains("Register"));
			Assert.IsTrue(e.Message.Contains("Registered"));
		}

		[Test]
		public void Login_NonExsitingUserName_ShouldFail()
		{
			string json = LoginSessionError(GUEST_SESSION_ID, NEW_USERNAME);
			Assert.IsNotNull(json);
			UserManagementException e = JsonConvert.DeserializeObject<UserManagementException>(json);
			Assert.IsNotNull(e);
			Assert.IsTrue(e.Message.Contains("Username doesn't exsits"));
		}

		[Test]
		public void Login_IlegalPassword_ShouldFail()
		{
			string json = LoginSessionError(GUEST_SESSION_ID, BUYER_USERNAME, WRONG_PASSWORD);
			Assert.IsNotNull(json);
			UserManagementException e = JsonConvert.DeserializeObject<UserManagementException>(json);
			Assert.IsNotNull(e);
			Assert.IsTrue(e.Message.Contains("Password doesn't match"));
		}

		[Test]
		public void Login_LegalUserNameAndPassword_ShouldPass()
		{
			Assert.DoesNotThrow(() => LoginSessionSuccess(REGISTERED_SESSION_ID));
		}

		[Test]
		public void Login_RegisteredUser_ShouldFail()
		{
			Assert.DoesNotThrow(() => LoginSessionSuccess(REGISTERED_SESSION_ID));
			string json = LoginSessionError(REGISTERED_SESSION_ID, BUYER_USERNAME, WRONG_PASSWORD);
			Assert.IsNotNull(json);
			UserStateException e = JsonConvert.DeserializeObject<UserStateException>(json);
			Assert.IsNotNull(e);
			Assert.IsTrue(e.Message.Contains("Login"));
			Assert.IsTrue(e.Message.Contains("Registered"));
		}

		[Test]
		public void Logout_GuestUser_ShouldFail()
		{
			string json = LogoutSessionError(GUEST_SESSION_ID);
			Assert.IsNotNull(json);
			UserStateException e = JsonConvert.DeserializeObject<UserStateException>(json);
			Assert.IsNotNull(e);
			Assert.IsTrue(e.Message.Contains("Logout"));
			Assert.IsTrue(e.Message.Contains("Guest"));
		}

		[Test]
		public void Logout_RegisteredUser_ShouldPass()
		{
			Assert.DoesNotThrow(() => LoginSessionSuccess(REGISTERED_SESSION_ID));
			Assert.DoesNotThrow(() => LogoutSessionSuccess(REGISTERED_SESSION_ID));
		}

		[Test]
		public void LogoutThenLogin_RegisteredUser_ShouldPass()
		{
			Assert.DoesNotThrow(() => LoginSessionSuccess(REGISTERED_SESSION_ID));
			Assert.DoesNotThrow(() => LogoutSessionSuccess(REGISTERED_SESSION_ID));
			Assert.DoesNotThrow(() => LoginSessionSuccess(REGISTERED_SESSION_ID));
		}

		[Test]
		public void OpenStore_ShouldPass()
		{
			Assert.DoesNotThrow(() => LoginSessionSuccess(GUEST_SESSION_ID, FIRST_OPENER_USERNAME));
			Assert.DoesNotThrow(() => OpenStoreSuccess(GUEST_SESSION_ID, FIRST_STORE_NAME, out FIRST_STORE));

			Assert.AreEqual(FIRST_STORE_NAME, FIRST_STORE.ContactDetails.Name);
			Assert.AreEqual("store@gmail.com", FIRST_STORE.ContactDetails.Email);
			Assert.AreEqual("Address", FIRST_STORE.ContactDetails.Address);
			Assert.AreEqual("0544444444", FIRST_STORE.ContactDetails.Phone);
			Assert.AreEqual("888-444444/34", FIRST_STORE.ContactDetails.BankAccountNumber);
			Assert.AreEqual("Leumi", FIRST_STORE.ContactDetails.Bank);
			Assert.AreEqual("Store description", FIRST_STORE.ContactDetails.Description);
		}

		[Test]
		public void OpenStore_AndTryAnotherSameOne_ShouldFail()
		{
			Assert.DoesNotThrow(() => LoginSessionSuccess(GUEST_SESSION_ID, FIRST_OPENER_USERNAME));
			Assert.DoesNotThrow(() => OpenStoreSuccess(GUEST_SESSION_ID, FIRST_STORE_NAME, out FIRST_STORE));
			string json = OpenStoreError(GUEST_SESSION_ID, FIRST_STORE_NAME);
			Assert.IsNotNull(json);
			StoreAlreadyExistException e = JsonConvert.DeserializeObject<StoreAlreadyExistException>(json);
			Assert.IsNotNull(e);
			Assert.IsTrue(e.Message.Contains(string.Format("Store with name {0} already exists", FIRST_STORE_NAME)));
		}
	}
}

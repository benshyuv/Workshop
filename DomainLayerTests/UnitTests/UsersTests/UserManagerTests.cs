using NUnit.Framework;
using DomainLayer.Users;
using System;
using DomainLayer.Exceptions;
using DomainLayer.DbAccess;
using Effort.Provider;
using Effort;

namespace DomainLayerTests.UnitTests.UsersTests
{
	public class UserManagerTests
	{
		private const string TEST_USERNAME = "TEST_1";
		private const string TEST_NEW_USERNAME = "TEST_0";
		private const string TEST_PASSWORD = "1234";
		private Guid SESSIONID;
		private UserManager manager;
		private User guestUser;
		private User registeredUser;
		private EffortConnection inMemoryConnection;

		[SetUp]
		public void Setup()
		{
			inMemoryConnection = DbConnectionFactory.CreateTransient();
			using var context = new MarketDbContext(inMemoryConnection);
			context.Init();
			manager = new UserManager("Admin", "Admin", context);
			guestUser = manager.NewSession(Guid.NewGuid(), context);
			manager.Register(guestUser, TEST_USERNAME, TEST_PASSWORD, context);
			manager.Register(guestUser, "TEST_2", "1111", context);
			registeredUser = manager.NewSession(Guid.NewGuid(), context);
			SESSIONID = Guid.NewGuid();
		}

		public void LoginRegisteredSession()
		{
			using var context = new MarketDbContext(inMemoryConnection);
			DateTime today = DateTime.Today;
			int oldCount = manager.GetStatistics(context, from: today)[today][(int)Roles.REGISTERED];
			context.Dispose();
			using var context2 = new MarketDbContext(inMemoryConnection);
			Assert.DoesNotThrow(()=>manager.Login(SESSIONID, registeredUser, TEST_USERNAME, TEST_PASSWORD, context2));
			context2.Dispose();
			using var context3 = new MarketDbContext(inMemoryConnection);
			Assert.AreEqual(oldCount+1, manager.GetStatistics(context3, from: today)[today][(int)Roles.REGISTERED]);
		}

		private void LoginGuestSession()
		{
			using var context = new MarketDbContext(inMemoryConnection);
			DateTime today = DateTime.Today;
			int oldCount = manager.GetStatistics(context, from: today)[today][(int)Roles.REGISTERED];
			context.Dispose();
			using var context2 = new MarketDbContext(inMemoryConnection);
			Assert.DoesNotThrow(() => manager.Login(SESSIONID, guestUser, TEST_USERNAME, TEST_PASSWORD, context2));
			context2.Dispose();
			using var context3 = new MarketDbContext(inMemoryConnection);
			Assert.AreEqual(oldCount+1, manager.GetStatistics(context3, from: today)[today][(int)Roles.REGISTERED]);
		}

        private void LoginGuestAsAdmin(MarketDbContext context)
        {
			DateTime today = DateTime.Today;
			int oldCount = manager.GetStatistics(context, from: today)[today][(int)Roles.ADMIN];
			context.Dispose();
			using var context2 = new MarketDbContext(inMemoryConnection);
			manager.Login(SESSIONID, guestUser, "Admin", "Admin", context2);
			context2.Dispose();
			using var context3 = new MarketDbContext(inMemoryConnection);
			Assert.AreEqual(oldCount+1, manager.GetStatistics(context3, from: today)[today][(int)Roles.ADMIN]);
		}

		[TearDown]
		public void Teardown()
		{
			manager = null;
			guestUser = null;
		}

		[Test]
		public void Register_LegalUserNameAndPassword_ShouldPass()
		{
			using var context = new MarketDbContext(inMemoryConnection);
			Assert.DoesNotThrow(() => manager.Register(guestUser, TEST_NEW_USERNAME, TEST_PASSWORD, context));
		}

		[Test]
		public void Register_ExistingUserName_ShouldFail()
		{
			using var context = new MarketDbContext(inMemoryConnection);
			Assert.Throws<UserManagementException>(() => manager.Register(guestUser, TEST_USERNAME, TEST_PASSWORD, context));
		}

		[Test]
		public void Register_LoggedInUser_ShouldFail()
		{
			LoginRegisteredSession();
			using var context = new MarketDbContext(inMemoryConnection);
			Assert.Throws<UserStateException>(() => manager.Register(registeredUser, TEST_NEW_USERNAME, TEST_PASSWORD, context));
		}

		[Test]
		public void Login_NonExsitingUserName_ShouldFail()
		{
			using var context = new MarketDbContext(inMemoryConnection);
			DateTime today = DateTime.Today;
			int oldCount = manager.GetStatistics(context, from: today)[today][(int)Roles.REGISTERED];
			Assert.Throws<UserManagementException>(()=>manager.Login(SESSIONID, guestUser, TEST_NEW_USERNAME, TEST_PASSWORD, context));
			Assert.AreEqual(oldCount, manager.GetStatistics(context, from: today)[today][(int)Roles.REGISTERED]);
		}

		[Test]
		public void Login_IlegalPassword_ShouldFail()
		{
			using var context = new MarketDbContext(inMemoryConnection);
			DateTime today = DateTime.Today;
			int oldCount = manager.GetStatistics(context, from: today)[today][(int)Roles.REGISTERED];
			Assert.Throws<UserManagementException>(() => manager.Login(SESSIONID, guestUser, TEST_USERNAME, "0000", context));
			Assert.AreEqual(oldCount, manager.GetStatistics(context, from: today)[today][(int)Roles.REGISTERED]);
		}

		[Test]
		public void Login_LegalUserNameAndPassword_ShouldPass()
		{
			Assert.DoesNotThrow(() => LoginGuestSession());
		}

		[Test]
		public void Login_AlreadyRegistered_ShouldFail()
		{
			LoginRegisteredSession();
			using var context = new MarketDbContext(inMemoryConnection);
			DateTime today = DateTime.Today;
			int oldCount = manager.GetStatistics(context, from: today)[today][(int)Roles.REGISTERED];
            Assert.Throws<UserStateException>(() => manager.Login(SESSIONID, registeredUser, TEST_NEW_USERNAME, TEST_PASSWORD, context));
            Assert.AreEqual(oldCount, manager.GetStatistics(context, from: today)[today][(int)Roles.REGISTERED]);
		}

		[Test]
		public void NewSession_ShouldIncGuestCount()
		{
			using var context = new MarketDbContext(inMemoryConnection);
			DateTime today = DateTime.Today;
			int oldCount = manager.GetStatistics(context, from: today)[today][(int)Roles.GUEST];
			context.Dispose();
			using var context2 = new MarketDbContext(inMemoryConnection);
			Assert.DoesNotThrow(() => manager.NewSession(SESSIONID, context2));
			context2.Dispose();
			using var context3 = new MarketDbContext(inMemoryConnection);
			Assert.AreEqual(oldCount+1, manager.GetStatistics(context3, from: today)[today][(int)Roles.GUEST]);
		}

		[Test]
		public void NewSession_Login_ShouldIncRegisteredCount()
		{
			using var context = new MarketDbContext(inMemoryConnection);
			DateTime today = DateTime.Today;
			int[] oldCount = manager.GetStatistics(context, from: today)[today];
			context.Dispose();
			using var context2 = new MarketDbContext(inMemoryConnection);
			User user = null;
			Assert.DoesNotThrow(() => user = manager.NewSession(SESSIONID, context2));
			Assert.IsNotNull(user);
			context2.Dispose();
			using var context3 = new MarketDbContext(inMemoryConnection);
			Assert.AreEqual(oldCount[(int)Roles.GUEST] + 1, manager.GetStatistics(context3, from: today)[today][(int)Roles.GUEST]);
			context3.Dispose();
			using var context4 = new MarketDbContext(inMemoryConnection);
			Assert.DoesNotThrow(() => manager.Login(SESSIONID, user, TEST_USERNAME, TEST_PASSWORD, context4));
			context4.Dispose();
			using var context5 = new MarketDbContext(inMemoryConnection);
            int[] newCount = manager.GetStatistics(context5, from: today)[today];
            Assert.AreEqual(oldCount[(int)Roles.GUEST], newCount[(int)Roles.GUEST]);
            Assert.AreEqual(oldCount[(int)Roles.REGISTERED] + 1, newCount[(int)Roles.REGISTERED]);
		}

		[Test]
		public void Register_RegisteredUserTwice_ShouldFail()
		{
			LoginRegisteredSession();
			using var context = new MarketDbContext(inMemoryConnection);
			Assert.Throws<UserStateException>(() => manager.Login(SESSIONID, registeredUser, TEST_USERNAME, TEST_PASSWORD, context));
		}

		[Test]
		public void Register_SameUsernameTwice_ShouldFail()
		{
			LoginRegisteredSession();
			using var context = new MarketDbContext(inMemoryConnection);
			Assert.Throws<UserManagementException>(() => manager.Login(SESSIONID, guestUser, TEST_USERNAME, TEST_PASSWORD, context));
		}

		[Test]
		public void Logout_GuestUser_ShouldFail()
		{
			using var context = new MarketDbContext(inMemoryConnection);
			Assert.Throws<UserStateException>(() => manager.Logout(guestUser, context));
		}

		[Test]
		public void Logout_RegisteredUser_ShouldPass()
		{
			LoginRegisteredSession();
			using var context = new MarketDbContext(inMemoryConnection);
			Assert.DoesNotThrow(() => manager.Logout(registeredUser, context));
		}

		[Test]
		public void LogoutThenLogin_RegisteredUser_ShouldPass()
		{
			LoginRegisteredSession();
			using var context = new MarketDbContext(inMemoryConnection);
			Assert.DoesNotThrow(() => manager.Logout(registeredUser, context));
			Assert.DoesNotThrow(() => manager.Login(SESSIONID, registeredUser, TEST_USERNAME, TEST_PASSWORD, context));
		}

		[Test]
		public void IsAdmin_RegisteredUser_ShouldFail()
		{
			LoginRegisteredSession();
			Assert.False(manager.IsAdmin(registeredUser));

		}

		[Test]
		public void IsAdmin_GuestUser_ShouldFail()
		{
			Assert.False(manager.IsAdmin(guestUser));
		}

		[Test]
		public void IsAdmin_Admin_ShouldPass()
		{
			using var context = new MarketDbContext(inMemoryConnection);
			LoginGuestAsAdmin(context);
			Assert.True(manager.IsAdmin(guestUser));
		}

		[Test]
		public void GetUsername_RegisterdUser_ShouldPass()
		{
			LoginRegisteredSession();
			Assert.AreEqual(TEST_USERNAME, manager.GetSessionUsername(registeredUser));
		}

		[Test]
		public void GetUsername_GuestUser_ShouldPass()
		{
			Assert.Throws<UserStateException>(() => manager.GetSessionUsername(guestUser));
		}

		[Test]
		public void GetUserID_RegisterdUser_ShouldPass()
		{
			LoginRegisteredSession();
			Assert.DoesNotThrow(() => manager.GetSessionUserID(registeredUser));
		}

		[Test]
		public void GetUserID_GuestUser_ShouldFail()
		{
			Assert.Throws<UserStateException>(() => manager.GetSessionUserID(guestUser));
		}

		[Test]
		public void GetUserIDByName_RegisterdUser_ShouldPass()
		{
			Guid userID = Guid.NewGuid();
			LoginRegisteredSession();
			using var context = new MarketDbContext(inMemoryConnection);
			Assert.DoesNotThrow(() => userID = manager.GetUserIDByName(TEST_USERNAME, context));
			Assert.AreEqual(registeredUser.GetUserID(), userID);
		}

		[Test]
		public void GetUserIDByName_GuestUser_ShouldFail()
		{
			using var context = new MarketDbContext(inMemoryConnection);
			Assert.Throws<UserManagementException>(() => manager.GetUserIDByName(TEST_NEW_USERNAME, context));
		}
	}
}
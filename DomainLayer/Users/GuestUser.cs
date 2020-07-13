using CustomLogger;
using DomainLayer.DbAccess;
using DomainLayer.Exceptions;
using System;
using System.Collections.Generic;

namespace DomainLayer.Users
{
    public class GuestUser : IUserState
	{
		public List<string> GetMessages(MarketDbContext context) => throw new UserStateException("Get Messages", "Guest");
		public bool HasAwaitingMessages() => false;
		public Guid GetUserID() => throw new UserStateException("Get UserID", "Guest");
		public string GetUsername() => throw new UserStateException("Get Username", "Guest");
		public bool IsAdmin() => false;
		public bool IsLoggedIn() => false;

		public void Login(UserManager userManager, User user, Guid sessionID, string username, string password, MarketDbContext context)
		{
			Logger.writeEvent("GuestState: Login| user is guest, continue to login");
			userManager.LoginGuest(user, sessionID, username, password, context);
		}

		public void Logout(UserManager userManager, User user, MarketDbContext context)
		{
			Logger.writeEvent("GuestState: Logout| user is not logged in");
			throw new UserStateException("Logout", "Guest");
		}

        public void LogoutClear(MarketDbContext context)
        {
			Logger.writeEvent("GuestState: LogoutClear| user is not logged in");
			throw new UserStateException("LogoutClear", "Guest");
		}

        public void Register(UserManager userManager, string username, string password, MarketDbContext context)
		{
			Logger.writeEvent("GuestState: Register| user is guest, continue to Register");
			userManager.RegisterGuest(username, password, context);
		}

        public bool IsRegisteredState()
        {
			return false;
        }

        public void AddToCart(User user, Guid storeID, Guid itemID, int amount, MarketDbContext context)
        {
			user.AddToCart(storeID, itemID, amount);
        }

        public void SetItemAmount(User user, Guid storeID, Guid itemID, int amount, MarketDbContext context)
        {
			user.SetItemAmountInCart(storeID, itemID, amount);
        }

        public void RemoveFromCart(User user, Guid storeID, Guid itemID, MarketDbContext context)
        {
			user.RemoveFromCart(storeID, itemID);
        }

		public void ClearCart(User user, MarketDbContext context) => user.NewCart(Guid.Empty);
		public void Reload(User user, MarketDbContext context) 
		{ 
		}
    }
}

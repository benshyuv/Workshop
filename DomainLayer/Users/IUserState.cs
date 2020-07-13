using DomainLayer.DbAccess;
using System;
using System.Collections.Generic;

namespace DomainLayer.Users
{
	interface IUserState
	{
		public bool IsAdmin();
		public bool IsLoggedIn();

		void Logout(UserManager userManager, User user, MarketDbContext context);

		void Login(UserManager userManager, User user, Guid sessionID, string username, string password, MarketDbContext context);

		void Register(UserManager userManager, string username, string password, MarketDbContext context);

		string GetUsername();

		Guid GetUserID();

        List<string> GetMessages(MarketDbContext context);
        void LogoutClear(MarketDbContext context);
        bool HasAwaitingMessages();
        bool IsRegisteredState();
        void AddToCart(User user, Guid storeID, Guid itemID, int amount, MarketDbContext context);
        void SetItemAmount(User user, Guid storeID, Guid itemID, int amount, MarketDbContext context);
        void RemoveFromCart(User user, Guid storeID, Guid itemID, MarketDbContext context);
        void ClearCart(User user, MarketDbContext context);
        void Reload(User user, MarketDbContext context);
    }
}

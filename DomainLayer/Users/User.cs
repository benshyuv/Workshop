using DomainLayer.DbAccess;
using System;
using System.Collections.Generic;

namespace DomainLayer.Users
{
    public class User
    {
        internal ShoppingCart Cart { get; set; }
        internal IUserState State { get; set; }

		internal User()
		{
            State = new GuestUser();
            Cart = new ShoppingCart(Guid.Empty);
		}

		internal bool IsAdmin() => State.IsAdmin();

        internal bool IsLoggedIn() => State.IsLoggedIn();

        internal void Login(UserManager userManager, Guid sessionID, string username, string password, MarketDbContext context)
        {
            State.Login(userManager, this, sessionID, username, password, context);
        }

        internal void AddToCart(Guid storeID, Guid itemID, int amount, MarketDbContext context) => State.AddToCart(this, storeID, itemID, amount, context);
        internal void PersistAndAddToCart(Guid storeID, Guid itemID, int amount, MarketDbContext context)
        {
            //context.Carts.Attach(Cart);
            AddToCart(storeID, itemID, amount);
            context.SaveChanges();
        }

        internal void AddToCart(Guid storeID, Guid itemID, int amount) => Cart.AddToCart(storeID, itemID, amount);

        internal void ReplaceState(IUserState newState, ShoppingCart oldCart)// when logging-in
        {
            State = newState;
            Cart = oldCart;// new additions to cart are discarded
        }

		internal string GetUsername() => State.GetUsername();
		internal Guid GetUserID() => State.GetUserID();
		internal void Logout(UserManager userManager, MarketDbContext context) => State.Logout(userManager, this, context);

        internal void Register(UserManager userManager, string username, string password, MarketDbContext context) => State.Register(userManager, username, password, context);
        internal void SetItemAmountInCart(Guid storeID, Guid itemID, int amount, MarketDbContext context)
        {
            State.SetItemAmount(this, storeID, itemID, amount, context);
        }

        internal void PersistAndSetItemAmountInCart(Guid storeID, Guid itemID, int amount, MarketDbContext context)
        {
            //context.Carts.Attach(Cart);
            SetItemAmountInCart(storeID, itemID, amount);
            context.SaveChanges();
        }

        internal void SetItemAmountInCart(Guid storeID, Guid itemID, int amount) => Cart.SetItemAmount(storeID, itemID, amount);
        internal void RemoveFromCart(Guid storeID, Guid itemID, MarketDbContext context) => State.RemoveFromCart(this, storeID, itemID, context);
        internal void PersistAndRemoveFromCart(Guid storeID, Guid itemID, MarketDbContext context)
        {
            //context.Carts.Attach(Cart);
            RemoveFromCart(storeID, itemID);
            context.SaveChanges();
        }

        internal void RemoveFromCart(Guid storeID, Guid itemID) => Cart.RemoveFromCart(storeID, itemID);
        internal void ClearCart(MarketDbContext context) => State.ClearCart(this, context);
        internal void PersistNewCart(Guid userID, MarketDbContext context)
        {
            //context.Carts.Attach(Cart);
            context.Carts.Remove(Cart);
            NewCart(userID);
            context.Carts.Add(Cart);
            context.SaveChanges();
        }

        internal void NewCart(Guid Id) => Cart = new ShoppingCart(Id);
        internal List<string> GetMessages(MarketDbContext context) => State.GetMessages(context);
        internal void LogoutClear(MarketDbContext context)
        {
            State.LogoutClear(context);
            ReplaceState(new GuestUser(), new ShoppingCart(Guid.Empty));
        }

        internal bool HasAwaitingMessages() => State.HasAwaitingMessages();

        internal bool IsRegisteredState() => State.IsRegisteredState();
        internal void Reload(MarketDbContext context)
        {
            State.Reload(this, context);
        }

        internal void ReloadRegistered(MarketDbContext context)
        {
            context.Carts.Attach(Cart);
            context.Entry(Cart).Reload();
            State = context.Users.Find(State.GetUserID());
        }
    }
}

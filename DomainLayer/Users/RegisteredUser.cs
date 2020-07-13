using CustomLogger;
using DomainLayer.Exceptions;
using System;
using System.Collections.Generic;

using System.Linq;
using DomainLayer.Stores.Certifications;
using DomainLayer.Orders;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using DomainLayer.DbAccess;
using Newtonsoft.Json;

namespace DomainLayer.Users
{
    public class RegisteredUser : IUserState
	{
		[Key]
		public Guid ID { get; set; }

        [Index(IsUnique = true)]
		[MaxLength(128)]
        public string Username { get; set; }
		public byte[] Password { get; set; }

		public bool LoggedIn { get; set; }

		public virtual ICollection<UserMessage> Messages { get; set; }

		public List<string> awaitingMessages
        {
			get => Messages.ToList().ConvertAll(m => m.Message);
        }
		[JsonIgnore]
		public virtual ICollection<Certification> Certifications { get; set; }

        [JsonIgnore]
		public Dictionary<Guid, ISet<Permission>> StorePermissions
        {
			get => Certifications.ToDictionary(c => c.StoreID, c => c.Permissions);
        }
		//public virtual ICollection<Order> Orders { get; set; }

		[InverseProperty("Grantor")]
        [JsonIgnore]
		public virtual ICollection<StoreOwnerApointmentContract> GrantorContracts { get; set; }

		[InverseProperty("Grantee")]
		[JsonIgnore]
		public virtual ICollection<StoreOwnerApointmentContract> PendingContracts { get; set; }
		[JsonIgnore]
		public virtual ICollection<Approval> ContractsToApprove { get; set; }

		public RegisteredUser(string username, byte[] password)
		{
			ID = Guid.NewGuid();
			Username = username;
			Password = password;
			LoggedIn = false;
			Messages = new List<UserMessage>();
			Certifications = new HashSet<Certification>();
			//Orders = new HashSet<Order>();
			GrantorContracts = new HashSet<StoreOwnerApointmentContract>();
			PendingContracts = new HashSet<StoreOwnerApointmentContract>();
			ContractsToApprove = new HashSet<Approval>();
		}

        public RegisteredUser()
        {
        }

        public virtual bool IsAdmin() => false;
		public bool IsLoggedIn() => LoggedIn;

		public void Logout(UserManager userManager, User user, MarketDbContext context)
		{
			Logger.writeEvent("RegisteredState: Logout| user is logged in, continue to log out");
			userManager.LogoutRegistered(user, Username, context);
		}

		internal bool ConfirmPassword(byte[] hashed) => Enumerable.SequenceEqual(hashed, Password);

		public void Login(UserManager userManager, User user, Guid sessionID, string username, string password, MarketDbContext context)
		{
			Logger.writeEvent("RegisteredState: Login| user is already logged in");
			throw new UserStateException("Login", "Registered");
		}

		public void Register(UserManager userManager, string username, string password, MarketDbContext context)
		{
			Logger.writeEvent("RegisterdState: Register| user is already logged in");
			throw new UserStateException("Register", "Registered");
		}

        internal void AddNotificationMessage(string  message)
        {
			Messages.Add(new UserMessage(message, ID));
        }

        public List<string> GetAllPendingMessagesAndRemoveFromQueue(MarketDbContext context)
        {
			context.Entry(this).Collection(u => u.Messages).Load();
			List<string> messages = awaitingMessages;
			context.Messages.RemoveRange(Messages);
			context.SaveChanges();
			return messages;
		}

        public string GetUsername() => Username;

		public Guid GetUserID() => ID;

        public List<string> GetMessages(MarketDbContext context)
        {
			context.Users.Attach(this);
            return GetAllPendingMessagesAndRemoveFromQueue(context);
        }

        public void LogoutClear(MarketDbContext context)
        {
			context.Users.Attach(this);
			LoggedIn = false;
			context.SaveChanges();
        }

		public bool HasAwaitingMessages() => awaitingMessages.Count != 0;
        internal virtual void LogEntry(Guid sessionID, DailyStatistics stats)
        {
			// simple Registerd
			if (!Certifications.Any())
            {
				stats.LogRegistered(sessionID);
            }
			// Owner
			else if (Certifications.Any(c => c.IsOwner()))
            {
				stats.LogOwner(sessionID);
			}
			// Manager
			else
            {
				stats.LogManager(sessionID);
            }
		}

		public bool IsRegisteredState()
        {
			return true;
        }

		public virtual bool ShouldSerializeOrders()
		{
			return false;
		}

        public void AddToCart(User user, Guid storeID, Guid itemID, int amount, MarketDbContext context)
        {
			user.PersistAndAddToCart(storeID, itemID, amount, context);
		}

        public void SetItemAmount(User user, Guid storeID, Guid itemID, int amount, MarketDbContext context)
        {
			user.PersistAndSetItemAmountInCart(storeID, itemID, amount, context);
        }

        public void RemoveFromCart(User user, Guid storeID, Guid itemID, MarketDbContext context)
        {
			user.PersistAndRemoveFromCart(storeID, itemID, context);
        }

		public void ClearCart(User user, MarketDbContext context) => user.PersistNewCart(ID, context);
        public void Reload(User user, MarketDbContext context)
        {
			user.ReloadRegistered(context);
        }
    }
}

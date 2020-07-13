using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using CustomLogger;
using DomainLayer.DbAccess;
using DomainLayer.Exceptions;
using DomainLayer.NotificationsCenter;
using Security;

namespace DomainLayer.Users
{
	public class UserManager
	{
		//internal Dictionary<string, RegisteredUser> UsersByName { get; set; }
		//internal Dictionary<Guid, RegisteredUser> UsersByID { get; set; }
		//internal Dictionary<string, bool> LoggedIn { get; set; }
		//internal Dictionary<string, ShoppingCart> CartHistory { get; set; }

		ISecurityManager SecurityManager { get; set; }
		//private Dictionary<DateTime, DailyStatistics> Statistics { get; }
		private DailyStatistics Stats { get; set; }
		private DailyStatistics TodayStats(MarketDbContext context) 
		{ 
            DateTime today = DateTime.Today;
            if (Stats.Date.CompareTo(today) != 0)
            {
				Stats = new DailyStatistics(today);
				context.Stats.Add(Stats);
            }
			else
            {
				context.Stats.Attach(Stats);
            }
			return Stats;
		}

		public UserManager(string admin, string password, MarketDbContext context) 
		{
			SecurityManager = SecurityFactory.GetSecurity;
            DateTime today = DateTime.Today;
            Stats = context.Stats.Find(today);
			if (Stats is null)
            {
				Stats = new DailyStatistics(today);
				context.Stats.Add(Stats);
				context.SaveChanges();
            }
			RegisterAdmin(admin, password, context);
		}

        public void Login(Guid sessionID, User user, string username, string password, MarketDbContext context)
        {
            user.Login(this, sessionID, username, password, context);
        }

        internal void AddMessageToRecipients(INotificationEvent notification, MarketDbContext context)
        {
            foreach(Guid userID in notification.getRecipientsIDs())
            {
                if (TryGetRegistered(userID, out RegisteredUser registered, context))
                {
                    registered.AddNotificationMessage(notification.GetMessage());
                }
                else
                {
                    throw new UserManagementException(string.Format("Add Notification Message Failed: user \'{0}\' not found", userID));
                }

				context.SaveChanges();
            }
        }

        private bool TryGetRegistered(Guid userID, out RegisteredUser user, MarketDbContext context)
        {
			user = context.Users.Find(userID);
			return !(user is null);
        }

        public User NewSession(Guid sessionID, MarketDbContext context)
        {
			TodayStats(context).LogSession(sessionID);
			context.SaveChanges();
			return new User();
        }

		private int[] GetOrNewStats(DateTime date, MarketDbContext context)
        {
			DailyStatistics stats = context.Stats.Find(date);
			return (stats is null) ? new int[] { 0, 0, 0, 0, 0 } : stats.Statistics();
		}

		public string GetSessionUsername(User sessionUser) => sessionUser.GetUsername();

		public Guid GetSessionUserID(User sessionUser) => sessionUser.GetUserID();

		public Guid GetUserIDByName(string username, MarketDbContext context)
		{
			if (TryGetByName(username, out RegisteredUser user, context))
			{
				return user.GetUserID();
			}
			throw new UserManagementException(string.Format("{0} \'{1}\' registerd",ExceptionStrings.NoUserWithName, username));
		}

        private bool TryGetByName(string username, out RegisteredUser user, MarketDbContext context)
        {
			user = context.Users.Where(u => u.Username == username)
				.Include(u => u.Messages)
				.Include(u => u.Certifications)
				.SingleOrDefault();
			return !(user is null);
		}

		//private RegisteredUser GetUserByID(Guid userID) => UsersByID[userID];

		internal void LoginGuest(User user, Guid sessionID, string username, string password, MarketDbContext context)
		{
			if (!TryGetByName(username, out RegisteredUser Registered, context))
			{
				Logger.writeEvent(string.Format("UserManager: GuestLogin| username \'{0}\' in not registered", username));
				throw new UserManagementException("Login Failed: Username doesn't exsits in system");
			}
			if (ConfirmPassword(password, Registered))
			{
				if (Registered.IsLoggedIn())
				{
					Logger.writeEvent(string.Format("UserManager: GuestLogin| user \'{0}\' already logged in", username));
					throw new UserManagementException(string.Format("Login Failed: user \'{0}\' already logged in", username));
				}
				ShoppingCart oldCart = TryGetCartOrDefault(Registered.ID, user.Cart, context);//if no past cart exists, use current
				user.ReplaceState(Registered, oldCart);
				Registered.LoggedIn = true;
				Registered.LogEntry(sessionID, TodayStats(context));
				context.SaveChanges();
				Logger.writeEvent(string.Format("UserManager: GuestLogin| User: \'{0}\' by user Id: {1} has logged in successfully",
																			user.GetUsername(), user.GetUserID()));
			}
			else
			{
				Logger.writeEvent("UserManager: GuestLogin| Incorrect password");
				throw new UserManagementException("Login Failed: Password doesn't match");
			}
		}

        private ShoppingCart TryGetCartOrDefault(Guid userID, ShoppingCart cart, MarketDbContext context)
        {
			ShoppingCart found = context.Carts.Where(c => c.UserID == userID).SingleOrDefault();
			if (found is null)
			{
				cart.UserID = userID;
				context.Carts.Add(cart);
				return cart;
			}
			else
			{
				return found;
			}
        }

        public void Logout(User user, MarketDbContext context) => user.Logout(this, context);

        internal void LogoutRegistered(User user, string username, MarketDbContext context)
		{
			//CartHistory[username] = user.Cart;
			
			user.LogoutClear(context);
			Logger.writeEvent(string.Format("UserManager: LogoutRegisterd| User: \'{0}\' has logged out Successfully", username));
		}

		public void Register(User user, string username, string password, MarketDbContext context) => user.Register(this, username, password, context);

		internal void RegisterGuest(string username, string password, MarketDbContext context)
        {
			if (HasUser(username, context))
			{
				Logger.writeEvent(string.Format("UserManager: RegisterGuest| username \'{0}\' is taken", username));
				throw new UserManagementException("Registration Failed: Username already exsits in system");
			}
			byte[] hashed = SecurityManager.encrypt(password);
			RegisteredUser newUser = new RegisteredUser(username, hashed);
			context.Users.Add(newUser);
			context.SaveChanges();
			//UsersByName.Add(username, newUser);
			//UsersByID.Add(newUser.GetUserID(), newUser);
			//LoggedIn.Add(username, false);
			Logger.writeEvent(string.Format("Registration Succeeded: User: \'{0}\' has been Registered Successfully and got User Id: {1}",
															username, newUser.GetUserID()));
		}

		private void RegisterAdmin(string username, string password, MarketDbContext context)
		{
			byte[] hashed = SecurityManager.encrypt(password);
			AdminUser newAdmin = new AdminUser(username, hashed);
			if (context.Users.Any())
				return;
			context.Users.Add(newAdmin);
			context.SaveChanges();
			//UsersByName.Add(username, newAdmin);
			//UsersByID.Add(newAdmin.GetUserID(), newAdmin);
			//LoggedIn[username] = false;
			Logger.writeEvent(string.Format("RegisterAdmin Succeeded: \'{0}\' has been Registered Successfully and got Admin id: {1}", 
															username, newAdmin.GetUserID()));
		}

		private bool ConfirmPassword(string password, RegisteredUser registered)
		{
			byte[] hashed = SecurityManager.encrypt(password);
			return registered.ConfirmPassword(hashed);
		}

		public bool IsAdmin(User sessionUser) => sessionUser.IsAdmin();

        internal bool IsLoggedIn(User user) => user.IsLoggedIn();

        internal bool HasUser(string username, MarketDbContext context) => TryGetByName(username, out _, context);

		internal List<string> GetUserMessages(User user, MarketDbContext context) => user.GetMessages(context);

		internal Dictionary<DateTime, int[]> GetStatistics(MarketDbContext context, DateTime from, DateTime? to = null)
        {
			DateTime dateTo = to.GetValueOrDefault(from);
			Logger.writeEvent(string.Format("UserManager: GetStatistics| fetching Statistics from \"{0}\" to \"{1}\"", 
											from.ToShortDateString(), dateTo.ToShortDateString()));
			Dictionary<DateTime, int[]> stats = new Dictionary<DateTime, int[]>();
			DateTime date = from.Date;
			while (date.CompareTo(dateTo) <= 0)
            {

				stats.Add(date, GetOrNewStats(date, context));
				date = date.AddDays(1);
            }
			return stats;
        }
	}
}

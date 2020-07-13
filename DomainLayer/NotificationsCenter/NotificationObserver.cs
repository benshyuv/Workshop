using System;
using System.Collections.Concurrent;
using DomainLayer.DbAccess;
using DomainLayer.Users;

namespace DomainLayer.NotificationsCenter
{
    public class NotificationObserver : INotificationObserver
    {
        private ConcurrentDictionary<Guid, INotificationSubject> subjects;
        private UserManager userManager;
        private ICommunicationNotificationAlerter communicationNotificationAlerter;

        public NotificationObserver(UserManager userManager, ICommunicationNotificationAlerter communicationNotificationAlerter = null)
        {
            this.userManager = userManager;
            subjects = new ConcurrentDictionary<Guid, INotificationSubject>();
            this.communicationNotificationAlerter = communicationNotificationAlerter;
        }

        public void NotifyEvent(INotificationEvent notification, MarketDbContext context)
        {
            userManager.AddMessageToRecipients(notification, context);
            if (communicationNotificationAlerter != null)
            {
                foreach (Guid userID in notification.getRecipientsIDs())
                {
                     this.communicationNotificationAlerter.AlertUser(userID);
                }
            }
        }

        public bool RegisterSubject(INotificationSubject registerTo)
        {
            if (registerTo.RegisterObserver(this))
            {
                if (subjects.TryAdd(registerTo.GetGuid(), registerTo))
                {
                    return true;
                } 
            }
            return false;
        }

        public bool UnRegisterSubject(INotificationSubject unRegisterFrom)
        {
            if (unRegisterFrom.UnregisterObserver(this))
            {
                if (subjects.TryRemove(unRegisterFrom.GetGuid(),out INotificationSubject removed))
                {
                    return removed.GetGuid().Equals(unRegisterFrom.GetGuid());
                }
            }
            return false;
        }

        public void userLoggedIn(User user, Guid sessionID, bool admin, MarketDbContext context)
        {
            this.communicationNotificationAlerter.AssociateUserToSession(user.GetUserID(), sessionID, admin);
            if (user.HasAwaitingMessages())
            {
                this.communicationNotificationAlerter.AlertUser(user.GetUserID());
            }
            RefreshStatistics(context);
        }

        public void RefreshStatistics(MarketDbContext context)
        {
            communicationNotificationAlerter.UpdateAdminStats(userManager.GetStatistics(context, from: DateTime.Today));
        }

        public void userLoggedOut(Guid userID, Guid sessionID)
        {
            this.communicationNotificationAlerter.DisassociateUser(userID);
        }
    }
}

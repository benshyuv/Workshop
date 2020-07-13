using DomainLayer.DbAccess;
using System;
using DomainLayer.Users;

namespace DomainLayer.NotificationsCenter
{
    public interface INotificationObserver
    {
        public bool RegisterSubject(INotificationSubject registerTo);
        public void NotifyEvent(INotificationEvent notification, MarketDbContext context);
        public bool UnRegisterSubject(INotificationSubject unRegisterFrom);
        void userLoggedOut(Guid userID, Guid sessionID);
        void userLoggedIn(User user, Guid sessionID, bool admin, MarketDbContext context);
        void RefreshStatistics(MarketDbContext context);
    }
}

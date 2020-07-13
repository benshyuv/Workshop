using System;
using DomainLayer.NotificationsCenter.NotificationEvents;

namespace DomainLayer.NotificationsCenter
{
    public interface INotificationSubject
    {
        bool RegisterObserver(NotificationObserver notificationObserver);
        Guid GetGuid();
        bool UnregisterObserver(NotificationObserver notificationObserver);
        void notifyEvent(INotificationEvent notificationEvent, DbAccess.MarketDbContext context);
    }
}
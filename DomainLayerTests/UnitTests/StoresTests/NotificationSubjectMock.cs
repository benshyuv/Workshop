using System;
using DomainLayer.NotificationsCenter;

namespace DomainLayerTests.UnitTests.StoresTests
{
    internal class NotificationSubjectMock : INotificationSubject
    {
        public Guid GetGuid()
        {
            return Guid.Empty;
        }

        public void notifyEvent(INotificationEvent notificationEvent, DomainLayer.DbAccess.MarketDbContext context)
        {
        }

        public bool RegisterObserver(NotificationObserver notificationObserver)
        {
            return true;
        }

        public bool UnregisterObserver(NotificationObserver notificationObserver)
        {
            return true;
        }
    }
}
using System;
using System.Collections.Generic;

namespace DomainLayer.NotificationsCenter
{
    public interface INotificationEvent
    {
        IEnumerable<Guid> getRecipientsIDs();
        string GetMessage();
    }
}
using System;
using System.Collections.Generic;

namespace DomainLayer.NotificationsCenter.NotificationEvents
{
    public class OwnerRemovedEvent : INotificationEvent
    {
        private Guid userID;
        private string message;


        public OwnerRemovedEvent(Guid userID, string storeName)
        {
            this.userID = userID;
            this.message = String.Format("You have been removed from owning store: {0}", storeName);
    
        }

        public string GetMessage()
        {
            return message;
        }

        public IEnumerable<Guid> getRecipientsIDs()
        {
            return new List<Guid> { userID };
        }

    }
}

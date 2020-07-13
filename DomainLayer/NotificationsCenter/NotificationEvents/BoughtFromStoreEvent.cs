using System;
using System.Collections.Generic;

namespace DomainLayer.NotificationsCenter.NotificationEvents
{
    public class BoughtFromStoreEvent : INotificationEvent
    {
        private List<Guid> ownerGuids;
        private string message;


        public BoughtFromStoreEvent(string storeName, List<Guid> ownerGuids)
        {
            this.message = String.Format("A purchase has been made in store {0} owned by you", storeName);
            this.ownerGuids = ownerGuids;
        }

        public string GetMessage()
        {
            return message;
        }

        public IEnumerable<Guid> getRecipientsIDs()
        {
            return ownerGuids;
        }

    }
}

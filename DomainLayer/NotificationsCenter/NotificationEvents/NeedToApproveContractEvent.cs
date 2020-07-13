using System;
using System.Collections.Generic;

namespace DomainLayer.NotificationsCenter.NotificationEvents
{
    public class NeedToApproveContractEvent : INotificationEvent
    {
        private Guid userID;
        private string message;


        public NeedToApproveContractEvent(Guid userID, string storeName, string userName)
        {
            this.userID = userID;
            this.message = String.Format("user: {0} is awaiting your approval on ownership contract in store {1}", userName, storeName);

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

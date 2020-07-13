using System;
using System.Threading.Tasks;

namespace DomainLayer.NotificationsCenter
{
    public interface ICommunicationNotificationAlerter
    {
        public Task AlertUser(Guid userID);
        public Task<bool> AssociateUserToSession(Guid userID, Guid sessionID, bool admin);
        public bool DisassociateUser(Guid userID);
        public Task UpdateAdminStats(System.Collections.Generic.Dictionary<DateTime, int[]> dictionaries);
    }
}

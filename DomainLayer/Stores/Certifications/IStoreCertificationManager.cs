using DomainLayer.DbAccess;
using System;
using System.Collections.Generic;
using System.Text;
using DomainLayer.NotificationsCenter;

namespace DomainLayer.Stores.Certifications
{
    public interface IStoreCertificationManager
    {
        void EnsurePermission(Guid userID, Permission request, MarketDbContext context);

        void AddPermission(Guid userID, Guid grantor, Permission toAdd, MarketDbContext context);

        void RemovePermission(Guid userID, Guid grantor, Permission toRemove, MarketDbContext context);

        void AddOwner(Guid userID, Guid grantor, string userName, INotificationSubject notificationSubject, MarketDbContext context);

        void AddManager(Guid userID, Guid grantor, MarketDbContext context);

        void RemoveOwner(Guid userID, Guid grantor, INotificationSubject notificationSubject, MarketDbContext context);

        void RemoveManager(Guid userID, Guid grantor, INotificationSubject notificationSubject, MarketDbContext context);

        List<Permission> GetPermissions(Guid userID, Guid grantor, MarketDbContext context);

        List<Permission> GetPermissions(Guid sessionUser, MarketDbContext context);

        bool IsGrantorOf(Guid grantorID, Guid userID, MarketDbContext context);
        bool IsOwnerOfStore(Guid userID, MarketDbContext context);
        bool IsPermittedOperation(Guid userID, Permission permission, MarketDbContext context);
        bool IsManagerOfStore(Guid userID, MarketDbContext context);
        void ApproveContract(Guid userToApprove, Guid approve, MarketDbContext context);
        bool isApproverOf(Guid approver, Guid userToApprove, MarketDbContext context);
        bool HasAwaitingContract(Guid userID);
    }
}

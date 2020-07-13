using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using DomainLayer.Exceptions;
using DomainLayer.Users;
using Newtonsoft.Json;

namespace DomainLayer.Stores.Certifications
{
    public class StoreOwnerApointmentContract
    {
        [Key, ForeignKey("Grantee")]
        public Guid GranteeID { get; set; }
        public virtual RegisteredUser Grantee { get; set; }

        [Key, ForeignKey("Store")]
        public Guid StoreID { get; set; }
        [JsonIgnore]
        public virtual Store Store { get; set; }

        [ForeignKey("Grantor")]
        public Guid GrantorID { get; set; }
        public virtual RegisteredUser Grantor { get; set; }
        public virtual ICollection<Approval> AwaitingAprovalsFrom { get; set; }

        public StoreOwnerApointmentContract(Guid granteeID, Guid storeID, Guid grantorID, List<Guid> needToApprove)
        {
            this.GrantorID = grantorID;
            this.StoreID = storeID;
            this.GranteeID = granteeID;
            this.AwaitingAprovalsFrom = needToApprove.ConvertAll(ID => new Approval(ID, this.GranteeID, this.StoreID));
        }

        public StoreOwnerApointmentContract()
        {
        }

        public void approve(Guid approver)
        {
            if (TryGetApprover(approver, out Approval approval))
            {
                AwaitingAprovalsFrom.Remove(approval);
            }
            else
                throw new NotAnApproverException();
        }

        private bool TryGetApprover(Guid approver, out Approval approval)
        {
            approval = AwaitingAprovalsFrom.Where(a => a.UserID == approver).SingleOrDefault();
            return !(approval is null);
        }

        public bool isApproved()
        {
            return !AwaitingAprovalsFrom.Any();
        }

        public bool isAwaitingApprovalFrom(Guid approver)
        {
            return TryGetApprover(approver, out _);
        }
    }

    public class Approval
    {
        public Approval()
        {
        }

        public Approval(Guid userID, Guid granteeID, Guid storeID)
        {
            UserID = userID;
            GranteeID = granteeID;
            StoreID = storeID;
        }

        public Guid UserID { get; set; }
        [ForeignKey("UserID")]
        public virtual RegisteredUser User { get; set; }
        public Guid GranteeID { get; set; }
        public Guid StoreID { get; set; }
        [ForeignKey("GranteeID,StoreID")] 
        public virtual StoreOwnerApointmentContract Contract { get; set; }
    }
}

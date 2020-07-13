using System;
using System.ComponentModel.DataAnnotations;
using DomainLayer.DbAccess;
using DomainLayer.Users;

namespace DomainLayer.Stores.PurchasePolicies
{
    public abstract class APurchasePolicy
    {
        [Key]
        public Guid policyID { get; set; }

        public bool Active { get; set; }

        public APurchasePolicy()
        {
            policyID = Guid.NewGuid();
            Active = true;
        }

        public abstract bool deleteChildren(PurchasePolicy purchasePolicy, MarketDbContext context);

        internal abstract APurchasePolicyDataClassForSerialization ToDataClass();

        internal abstract bool IsValidPurchase(StoreCart cart);

        public override bool Equals(object obj)
        {
            return Equals(obj as APurchasePolicy);
        }

        public bool Equals(APurchasePolicy other)
        {
            return other != null &&
                   policyID.Equals(other.policyID);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(policyID);
        }

        public abstract override string ToString();
    }
}

using DomainLayer.DbAccess;
using DomainLayer.Exceptions;
using DomainLayer.Orders;
using DomainLayer.Stores.Discounts;
using DomainLayer.Stores.PurchasePolicies;
using DomainLayer.Users;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;

namespace DomainLayer.Stores
{

   
    public enum PurchasePolicyType
    {
        ITEM = 1, STORE = 2, DAYS = 4, COMPOSITE = 8
    }
    public class PurchasePolicy
    {
        [Key, ForeignKey("Store")]
        public Guid StoreId { get; set; }
        public virtual Store Store { get; set; }
        //types of purchases
        //IMMEDIATE = 0
        //AUCTION = 1
        //LOTTERY = 2

        private Dictionary<Guid, APurchasePolicy> allPoliciesForRetrevalByGuidOnly
        {
            get => AllPolicies.ToDictionary(p => p.policyID);
        }

        public virtual ICollection<APurchasePolicy> AllPolicies { get; set; }

        //default for active policies is an OR between all of them as discussed with yair
        internal List<APurchasePolicy> ActivePolicies
        {
            get => AllPolicies.Where(p => p.Active).ToList();
        }

        public PurchasePolicyType PolicyFlags { get; set; }

        internal ISet<PurchasePolicyType> AllowedPolicies
        {
            get
            {
                HashSet<PurchasePolicyType> result = new HashSet<PurchasePolicyType>();
                foreach (PurchasePolicyType type in Enum.GetValues(typeof(PurchasePolicyType)))
                {
                    if (PolicyFlags.HasFlag(type))
                    {
                        result.Add(type);
                    }
                }
                return result;
            }
            private set
            {
                PolicyFlags = 0;
                if (!(value is null))
                {
                    foreach (PurchasePolicyType type in value)
                    {
                        PolicyFlags |= type;
                    }
                }
            }
        }

        //Next version: rules of purchases
        //Next version: who can buy in store

        public PurchasePolicy(Guid id, HashSet<PurchasePolicyType> allowedPolicies)
        {
            StoreId = id;
            AllPolicies = new HashSet<APurchasePolicy>();
            PolicyFlags = 0;

            foreach (PurchasePolicyType policyType in allowedPolicies)
            {
                PolicyFlags |= policyType;
            }

        }

        public PurchasePolicy(Guid id)
        {
            StoreId = id;
            foreach (PurchasePolicyType type in Enum.GetValues(typeof(PurchasePolicyType)))
            {
                PolicyFlags |= type;
            }
            AllPolicies = new HashSet<APurchasePolicy>();
        }

        public PurchasePolicy()
        { }

        internal bool ValidatePurchase(StoreCart cart, User user)
        {
            if (ActivePolicies.Count == 0)
                return true;
            //AND is default between active policies (all must be true).
            StringBuilder sb = new StringBuilder();
            sb.Append("Purchase is not valid, violates:\n");
            foreach (APurchasePolicy policy in ActivePolicies)
            {
                if (!policy.IsValidPurchase(cart))
                    throw new PolicyException(sb.Append(policy.ToString()).ToString());
                
            }
            return true;
           
        }

        internal bool DeletePolicyRecursiveFromNotActivePolicies(Guid policyID, MarketDbContext context)
        {
            if (!PolicyExistsInStore(policyID))
            {
                throw new ArgumentException("Policy id doesnt exist");
            }
            APurchasePolicy policy = GetPolicyByID(policyID);
            AllPolicies.Remove(policy);
            context.Policies.Remove(policy);
            context.SaveChanges();
            return policy.deleteChildren(this, context);
        }

        public virtual bool ShouldSerializeStore()
        {
            return false;
        }

        internal bool IsValidToCreateItemMinMaxPurchasePolicy(Guid itemID)
        {
            foreach (APurchasePolicy aPolicy in AllPolicies)
            {
                if (aPolicy is ItemMinMaxPurchasePolicy purchasePolicy && purchasePolicy.ItemID.Equals(itemID))   
                {
                    return false;
                }
            }
            return true;
        }

        internal bool IsValidToCreateDaysNotAllowedPurchasePolicy()
        {
            foreach (APurchasePolicy aPolicy in AllPolicies)
            {
                if (aPolicy is DaysNotAllowedPurchasePolicy)
                {
                    return false;
                }
            }
            return true;
        }

        internal bool IsValidToCreateStoreMinMaxPurchasePolicy()
        {
            foreach (APurchasePolicy aPolicy in AllPolicies)
            {
                if (aPolicy is StoreMinMaxPurchasePolicy)
                {
                    return false;
                }
            }
            return true;
        }

        internal ItemMinMaxPurchasePolicy AddItemMinMaxPurchasePolicy(Guid itemID, int? minAmount, int? maxAmount, string itemName, MarketDbContext context)
        {
            if (!IsPurchaseTypeAllowed(PurchasePolicyType.ITEM))
            {
                throw new PurchasePolicyTypeNotAllowedException(PurchasePolicyType.ITEM);
            }

            ItemMinMaxPurchasePolicy policy = new ItemMinMaxPurchasePolicy(itemID, minAmount, maxAmount, itemName);
            AllPolicies.Add(policy);
            context.Policies.Add(policy);
            context.SaveChanges();
            return policy;
        }

        internal StoreMinMaxPurchasePolicy AddStoreMinMaxPurchasePolicy(int? minAmount, int? maxAmount, string storeName, MarketDbContext context)
        {
            if (!IsPurchaseTypeAllowed(PurchasePolicyType.STORE))
            {
                throw new PurchasePolicyTypeNotAllowedException(PurchasePolicyType.STORE);
            }

            StoreMinMaxPurchasePolicy policy = new StoreMinMaxPurchasePolicy(minAmount, maxAmount, storeName);
            AllPolicies.Add(policy);
            context.Policies.Add(policy);
            context.SaveChanges();
            return policy;
        }

        internal DaysNotAllowedPurchasePolicy AddDaysNotAllowedPurchasePolicy(int[] daysNotAllowed, string storeName, MarketDbContext context)
        {
            if (!IsPurchaseTypeAllowed(PurchasePolicyType.DAYS))
            {
                throw new PurchasePolicyTypeNotAllowedException(PurchasePolicyType.DAYS);
            }

            DaysNotAllowedPurchasePolicy policy = new DaysNotAllowedPurchasePolicy(daysNotAllowed, storeName);
            AllPolicies.Add(policy);
            context.Policies.Add(policy);
            context.SaveChanges();
            return policy;
        }

        internal bool PolicyExistsInStore(Guid policyID)
        {
            return AllPolicies.Any(p => p.policyID == policyID);
        }

        internal CompositePurchasePolicy ComposeTwoPurchasePolicys(Guid policyLeftID, Guid policyRightID, Operator boolOperator, MarketDbContext context)
        {
            if (!IsPurchaseTypeAllowed(PurchasePolicyType.COMPOSITE))
            {
                throw new PurchasePolicyTypeNotAllowedException(PurchasePolicyType.COMPOSITE);
            }

            if (!PolicyExistsInStore(policyLeftID) || !PolicyExistsInStore(policyRightID))
            {
                throw new ArgumentException("One or both of the Policy ids dont exist");
            }
            APurchasePolicy left = GetPolicyByID(policyLeftID);
            APurchasePolicy right = GetPolicyByID(policyRightID);

            CompositePurchasePolicy policy = new CompositePurchasePolicy(left, right, boolOperator);
            AllPolicies.Add(policy);
            left.Active = false;
            right.Active = false;
            context.Policies.Add(policy);
            context.SaveChanges();
            return policy;
        }

        private APurchasePolicy GetPolicyByID(Guid policyID)
        {
            return AllPolicies.Where(p => p.policyID == policyID).Single();
        }

        internal bool IsPurchaseTypeAllowed(PurchasePolicyType policy)
        {
            return PolicyFlags.HasFlag(policy);
        }

        internal bool MakePurcahsePolicyNotAllowed(PurchasePolicyType policy, MarketDbContext context)
        {
            if (!IsPurchaseTypeAllowed(policy))
            {
                throw new PurchasePolicyTypeAlreadyAllowedException(policy);
            }
            PolicyFlags ^= policy;
            context.SaveChanges();
            return true;
        }

        internal bool MakePurcahsePolicyAllowed(PurchasePolicyType policy, MarketDbContext context)
        {
            if (IsPurchaseTypeAllowed(policy))
            {
                throw new PurchasePolicyTypeAlreadyAllowedException(policy);
            }
            PolicyFlags |= policy;
            context.SaveChanges();
            return true;
        }

        internal bool DeletePolicy(Guid policyID, MarketDbContext context)
        {
            if (!PolicyExistsInStore(policyID))
            {
                throw new ArgumentException("policy id doesnt exist");
            }
            APurchasePolicy policy = GetPolicyByID(policyID);
            if (!ActivePolicies.Contains(policy))
            {
                throw new ArgumentException("Can only remove active policies, cannot remove basic policy that there exist" +
                    " a composite policy witch is composed of it.");
            }
            AllPolicies.Remove(policy);
            context.Policies.Remove(policy);
            context.SaveChanges();
            return policy.deleteChildren(this, context);
        }

        internal List<APurchasePolicyDataClassForSerialization> GetAllPurchasePolicys()
        {
            List<APurchasePolicyDataClassForSerialization> ret = new List<APurchasePolicyDataClassForSerialization>();

            
            foreach (APurchasePolicy policy in ActivePolicies)
            {
                ret.Add(policy.ToDataClass());
            }
            
            return ret;
        }

        internal List<PurchasePolicyType> GetAllowedPurchasePolicys()
        {
            return new List<PurchasePolicyType>(AllowedPolicies);
        }
    }
}

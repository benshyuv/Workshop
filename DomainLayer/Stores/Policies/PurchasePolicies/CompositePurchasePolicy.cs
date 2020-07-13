using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using DomainLayer.DbAccess;
using DomainLayer.Stores.Discounts;
using DomainLayer.Users;
using Newtonsoft.Json;

namespace DomainLayer.Stores.PurchasePolicies
{
    public class CompositePurchasePolicy : APurchasePolicy
    {
        public Operator Op { get; set; }

        public Guid PolicyLeftID { get; set; }

        public Guid PolicyRightID { get; set; }

        [JsonIgnore]
        public List<APurchasePolicy> ChildrenPolicies { get; set; }

        [JsonIgnore]
        [NotMapped]
        public APurchasePolicy PolicyLeft
        {
            get
            {
                if (ChildrenPolicies != null)
                {
                    return ChildrenPolicies[0];
                }
                else //because of desirialization
                {
                    return null;
                }
            }
        }

        [JsonIgnore]
        [NotMapped]
        public APurchasePolicy PolicyRight
        {
            get
            {
                if (ChildrenPolicies != null)
                {
                    return ChildrenPolicies[1];
                }
                else //because of desirialization
                {
                    return null;
                }
            }
        }
        public CompositePurchasePolicy()
        {
        }

        public CompositePurchasePolicy(APurchasePolicy leftPolicy, APurchasePolicy rightPolicy, Operator boolOperator) : base()
        {
            this.Op = boolOperator;

            this.ChildrenPolicies = new List<APurchasePolicy>();
            this.ChildrenPolicies.Add(leftPolicy);
            this.ChildrenPolicies.Add(rightPolicy);

            this.PolicyLeftID = PolicyLeft.policyID;
            this.PolicyRightID = PolicyRight.policyID;
        }




        internal override bool IsValidPurchase(StoreCart cart)
        {
            switch (Op)
            {
                case Operator.AND:
                    return PolicyLeft.IsValidPurchase(cart) && PolicyRight.IsValidPurchase(cart);
                case Operator.OR:
                    return PolicyLeft.IsValidPurchase(cart) || PolicyRight.IsValidPurchase(cart);
                case Operator.XOR:
                    if (PolicyLeft.IsValidPurchase(cart))
                    {
                        return !PolicyRight.IsValidPurchase(cart);
                    }
                    else
                    {
                        return PolicyRight.IsValidPurchase(cart);
                    }
                default:
                    throw new NotSupportedException();
            }
        }

        internal override APurchasePolicyDataClassForSerialization ToDataClass()
        {
            return new APurchasePolicyDataClassForSerialization(this);
        }

        public override bool deleteChildren(PurchasePolicy purchasePolicy, MarketDbContext context)
        {
            bool leftremoved = purchasePolicy.DeletePolicyRecursiveFromNotActivePolicies(PolicyLeftID, context);
            bool rightremoved = purchasePolicy.DeletePolicyRecursiveFromNotActivePolicies(PolicyRightID, context);
            bool resultDelete = leftremoved && rightremoved;
            if (resultDelete)
            {
                this.ChildrenPolicies = new List<APurchasePolicy>();
            }
            return leftremoved && rightremoved;
        }

        public override string ToString()
        {
            return String.Format("{0} Composite policy\nLeft: {1}  \nRight: {2}", this.Op.ToString(), this.PolicyLeft, this.PolicyRight);
        }
    }
}

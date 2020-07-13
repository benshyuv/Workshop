using System;
using System.Collections.Generic;
using DomainLayer.Stores.Discounts;

namespace DomainLayer.Stores.PurchasePolicies
{
    public class APurchasePolicyDataClassForSerialization

    {
        public Guid PolicyID;
        public Guid ItemID;
        public Operator Op;
        public APurchasePolicyDataClassForSerialization LeftPolicy;
        public APurchasePolicyDataClassForSerialization RightPolicy;
        public int? MinAmount;
        public int? MaxAmount;
        public List<int> DaysNotAllowed;
        public string Description;
        //must be from [item, store, days, composite]
        public string policyType;


        public APurchasePolicyDataClassForSerialization()
        {
        }

        public APurchasePolicyDataClassForSerialization(ItemMinMaxPurchasePolicy itemMinMaxPurchasePolicy)
        {
            this.PolicyID = itemMinMaxPurchasePolicy.policyID;
            this.ItemID = itemMinMaxPurchasePolicy.ItemID;
            this.MinAmount = itemMinMaxPurchasePolicy.MinAmount;
            this.MaxAmount = itemMinMaxPurchasePolicy.MaxAmount;
            this.Description = itemMinMaxPurchasePolicy.ToString();
            this.policyType = "item";
        }

        public APurchasePolicyDataClassForSerialization(StoreMinMaxPurchasePolicy storeMinMaxPurchasePolicy)
        {
            this.PolicyID = storeMinMaxPurchasePolicy.policyID;
            this.MinAmount = storeMinMaxPurchasePolicy.MinAmount;
            this.MaxAmount = storeMinMaxPurchasePolicy.MaxAmount;
            this.Description = storeMinMaxPurchasePolicy.ToString();
            this.policyType = "store";
        }

        public APurchasePolicyDataClassForSerialization(DaysNotAllowedPurchasePolicy daysNotAllowedPurchasePolicy)
        {
            this.PolicyID = daysNotAllowedPurchasePolicy.policyID;
            this.DaysNotAllowed = getIntArrayFromDaysOfWeek(daysNotAllowedPurchasePolicy.DaysNotAllowed);
            this.Description = daysNotAllowedPurchasePolicy.ToString();
            this.policyType = "days";
        }

        private List<int> getIntArrayFromDaysOfWeek(ICollection<DayOfWeek> daysNotAllowed)
        {
            List<int> ret = new List<int>();
            foreach(DayOfWeek day in daysNotAllowed)
            {
                switch (day)
                {
                    case (DayOfWeek.Sunday):
                        ret.Add(1);
                        break;
                    case (DayOfWeek.Monday):
                        ret.Add(2);
                        break;
                    case (DayOfWeek.Tuesday):
                        ret.Add(3);
                        break;
                    case (DayOfWeek.Wednesday):
                        ret.Add(4);
                        break;
                    case (DayOfWeek.Thursday):
                        ret.Add(5);
                        break;
                    case (DayOfWeek.Friday):
                        ret.Add(6);
                        break;
                    case (DayOfWeek.Saturday):
                        ret.Add(7);
                        break;
                }
            }
            return ret;
        }

        public APurchasePolicyDataClassForSerialization(CompositePurchasePolicy compositePurchasePolicy)
        {
            this.PolicyID = compositePurchasePolicy.policyID;
            this.Op = compositePurchasePolicy.Op;
            this.LeftPolicy = compositePurchasePolicy.PolicyLeft.ToDataClass();
            this.RightPolicy = compositePurchasePolicy.PolicyRight.ToDataClass();
            this.Description = compositePurchasePolicy.ToString();
            this.policyType = "composite";
        }
    }
}

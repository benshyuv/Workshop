using System;
using System.Collections.Generic;
using DomainLayer.Users;

namespace DomainLayer.Stores.PurchasePolicies
{
    public class ItemMinMaxPurchasePolicy: ABasicPurchasePolicy
    {

        public Guid ItemID { get; set; }

        public int? MinAmount { get; set; }
        public int? MaxAmount { get; set; }
        public string ItemName { get; set; }

        public ItemMinMaxPurchasePolicy()
        {
        }

        public ItemMinMaxPurchasePolicy(Guid itemID, int? minAmount, int? maxAmount, string itemName):base()
        {
            if (minAmount is int min && maxAmount is int max)
            {
                if (min > max)
                {
                    throw new ArgumentException("min higher than max");
                }
            }
            if(minAmount is null && maxAmount is null)
            {
                throw new ArgumentException("at least one of min or max must not be null");
            }
            ItemID = itemID;
            MinAmount = minAmount;
            MaxAmount = maxAmount;
            ItemName = itemName;
        }

        internal override APurchasePolicyDataClassForSerialization ToDataClass()
        {
            return new APurchasePolicyDataClassForSerialization(this);
        }

        internal override bool IsValidPurchase(StoreCart cart)
        {
            foreach (KeyValuePair<Guid,int> itemAmount in cart.Items)
            {
                if (ItemID.Equals(itemAmount.Key))
                {
                    return isBetweenMinMax(itemAmount.Value);
                }
            }
            return true;
        }

        private bool isBetweenMinMax(int value)
        {
            if(MinAmount is int min && MaxAmount is int max)
            {
                return min <= value && value <= max;
            }
            else if(MinAmount is int min1 && MaxAmount is null)
            {
                return min1 <= value;
            }
            else if (MaxAmount is int max1 && MinAmount is null)
            {
                return value <= max1;
            }
            else
            {
                throw new NullReferenceException();
            }
        }
        public override string ToString()
        {
            if (MinAmount is null)
                return String.Format("Must buy at most {0} of {1}", MaxAmount, ItemName);
            else if (MaxAmount is null)
                return String.Format("Must buy at least {0} of {1}", MinAmount, ItemName);
            else
                return String.Format("Must buy at most {0} and at least {1} of {2}", MaxAmount, MinAmount, ItemName);
        }
    }
}

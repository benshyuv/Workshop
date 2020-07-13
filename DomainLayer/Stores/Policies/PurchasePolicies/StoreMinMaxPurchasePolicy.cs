using System;
using System.Collections.Generic;
using DomainLayer.Users;

namespace DomainLayer.Stores.PurchasePolicies
{
    public class StoreMinMaxPurchasePolicy : ABasicPurchasePolicy
    {
        public int? MinAmount { get; set; }
        public int? MaxAmount { get; set; }
        public string StoreName { get; set; }

        public StoreMinMaxPurchasePolicy()
        {
        }

        public StoreMinMaxPurchasePolicy(int? minAmount, int? maxAmount, string storeName):base()
        {
            if (minAmount is int min && maxAmount is int max)
            {
                if (min > max)
                {
                    throw new ArgumentException("min higher than max");
                }
            }
            if (minAmount is null && maxAmount is null)
            {
                throw new ArgumentException("at least one of min or max must not be null");
            }
            MinAmount = minAmount;
            MaxAmount = maxAmount;
            StoreName = storeName;
        }

        internal override bool IsValidPurchase(StoreCart cart)
        {
            int totalItems = 0;
            foreach (KeyValuePair<Guid, int> itemAmount in cart.Items)
            {
                totalItems = totalItems + itemAmount.Value;
            }
            return isBetweenMinMax(totalItems);
        }

        private bool isBetweenMinMax(int value)
        {
            if (MinAmount is int min && MaxAmount is int max)
            {
                return min <= value && value <= max;
            }
            else if (MinAmount is int min1 && MaxAmount is null)
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

        internal override APurchasePolicyDataClassForSerialization ToDataClass()
        {
            return new APurchasePolicyDataClassForSerialization(this);
        }

        public override string ToString()
        {
            if (MinAmount is null)
                return String.Format("Must buy at most {0} items from {1}", MaxAmount, StoreName);
            else if(MaxAmount is null)
                return String.Format("Must buy at least {0} items from {1}", MinAmount, StoreName);
            else
                return String.Format("Must buy at most {0} and at least {1} items from {2}",MaxAmount, MinAmount, StoreName);
        }
    }
}

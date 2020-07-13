using System;
using System.Collections.Generic;
using DomainLayer.Stores.Inventory;

namespace DomainLayer.Stores.Discounts
{
    public class StoreConditionalDiscount : ABasicDiscount
    {

        public double Precent { get; set; }
        public double MinPurchase { get; set; }

        public StoreConditionalDiscount(DateTime dateUntil, double minPurchase, double discount):base(dateUntil)
        {
            this.Precent = discount;
            this.MinPurchase = minPurchase;
        }

        public StoreConditionalDiscount()
        {
        }

        public override bool IsConditionSatisfied(Dictionary<Guid, Tuple<int, double>> cart)
        {
            double totalFromStore = 0;
            foreach (KeyValuePair< Guid, Tuple<int, double>> keyValuePair in cart)
            {
                totalFromStore += keyValuePair.Value.Item2;
            }
            return totalFromStore >= MinPurchase;
        }

        protected override Dictionary<Guid, Tuple<int, double>> UpdatedPriceList(Dictionary<Guid, Tuple<int, double>> cartBefore)
        {
            Dictionary<Guid, Tuple<int, double>> cartAfter = deepCopyCart(cartBefore); ;
            foreach (KeyValuePair<Guid, Tuple<int, double>> keyValuePair in cartBefore)
            {
                cartAfter[keyValuePair.Key] = new Tuple<int, double>(keyValuePair.Value.Item1, keyValuePair.Value.Item2 * (1-Precent));
            }
            return cartAfter;
        }

        internal override ADisountDataClassForSerialization ToDataClass()
        {
            return new ADisountDataClassForSerialization(this);
        }

        public override string ToString()
        {
            return String.Format("{0} precent discount on all items from store, when total is at least {1} \nfinish date: {2}", Precent, MinPurchase, this.DateUntil.Date);
        }
        internal override bool isForItem(Guid itemID)
        {
            return false;
        }
    }
}

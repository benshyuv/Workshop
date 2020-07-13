using System;
using System.Collections.Generic;
using DomainLayer.Stores.Inventory;

namespace DomainLayer.Stores.Discounts
{
    public class ItemConditionalDiscount_MinItems_ToDiscountOnExtraItems: ABasicDiscount
    {
        public double DiscountForExtra { get; set; }
        public Guid ItemID { get; set; }
        public int MinItems { get; set; }
        public int ExtraItems { get; set; }
        public string ItemName { get; set; }

        public ItemConditionalDiscount_MinItems_ToDiscountOnExtraItems(Guid itemID, DateTime dateUntil, int minItems,
            int extraItems, double discountForExtra, string itemName):base(dateUntil)
        {
            this.ItemID = itemID;
            this.MinItems = minItems;
            this.ExtraItems = extraItems;
            this.ItemName = itemName;
            this.DiscountForExtra = discountForExtra;
        }

        public ItemConditionalDiscount_MinItems_ToDiscountOnExtraItems()
        {
        }

        public override bool IsConditionSatisfied(Dictionary<Guid, Tuple<int, double>> cart)
        {
            if (cart.ContainsKey(ItemID) && cart[ItemID].Item1 >= MinItems)
            {
                return true;
            }
            return false;
        }

        protected override Dictionary<Guid, Tuple<int, double>> UpdatedPriceList(Dictionary<Guid, Tuple<int, double>> cartBefore)
        {

            Dictionary<Guid, Tuple<int, double>> cartAfter = deepCopyCart(cartBefore);
            if (cartAfter.ContainsKey(ItemID) && cartAfter[ItemID].Item1 >= MinItems)
            {
                double priceBefore = cartAfter[ItemID].Item2;
                int amountTotal = cartAfter[ItemID].Item1;
                double pricePerItem = priceBefore / amountTotal;
                int extraItemsBought = amountTotal - MinItems;
                int extraItemsDiscounted = extraItemsBought > ExtraItems ? ExtraItems : extraItemsBought;
                int extraItemsNoDiscount = extraItemsBought > ExtraItems ? (extraItemsBought - ExtraItems) : 0;
                double pricePerDiscountedExtraItem = pricePerItem * (1-DiscountForExtra);

                double priceTotal = (pricePerItem * (MinItems + extraItemsNoDiscount)) + (pricePerDiscountedExtraItem * extraItemsDiscounted);
                Tuple<int, double> updatedAmountPrice = new Tuple<int, double>(amountTotal,
                    priceTotal);
                cartAfter[ItemID] = updatedAmountPrice;
            }
            return cartAfter;
            
        }

        public override string ToString()
        {
            return String.Format("{0} precent discount on {3} {1}'s, when buying at least {2} \nfinish date: {4}",
                DiscountForExtra, ItemName, this.MinItems, this.ExtraItems, this.DateUntil.Date);

        }

        internal override ADisountDataClassForSerialization ToDataClass()
        {
            return new ADisountDataClassForSerialization(this);
        }

        internal override bool isForItem(Guid itemID)
        {
            return this.ItemID.Equals(itemID);
        }
    }
}

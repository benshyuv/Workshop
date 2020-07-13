using System;
using System.Collections.Generic;
using DomainLayer.Stores.Inventory;

namespace DomainLayer.Stores.Discounts
{

    public class ItemConditionalDiscount_MinItems_ToDiscountOnAll : ABasicDiscount
    {
        public double Precent { get; set; }
        public Guid ItemID { get; set; }
        public string ItemName { get; set; }
        public int MinItems { get; set; }
        public ItemConditionalDiscount_MinItems_ToDiscountOnAll(Guid itemID, DateTime dateUntill,
            int minItems, double discount, string itemName) : base(dateUntill)
        {
            this.Precent = discount;
            this.MinItems = minItems;
            this.ItemID = itemID;
            this.ItemName = itemName;
        }

        public ItemConditionalDiscount_MinItems_ToDiscountOnAll() : base()
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
                Tuple<int, double> updatedAmountPrice = new Tuple<int, double>(cartAfter[ItemID].Item1,
                    cartAfter[ItemID].Item2 * (1-Precent));
                cartAfter[ItemID] = updatedAmountPrice;
            }
            return cartAfter;
            
        }

        internal override ADisountDataClassForSerialization ToDataClass()
        {
            return new ADisountDataClassForSerialization(this);
        }

        public override string ToString()
        {
            return String.Format("{0} precent discount on {1} when buying at least {2} \nfinish date: {3}", Precent, ItemName, this.MinItems, this.DateUntil.Date);
        }

        internal override bool isForItem(Guid itemID)
        {
            return this.ItemID.Equals(itemID);
        }
    }
}

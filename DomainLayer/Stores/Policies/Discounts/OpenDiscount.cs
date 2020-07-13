using System;
using System.Collections.Generic;
using System.Text;
using DomainLayer.Stores.Inventory;
using DomainLayer.Users;

namespace DomainLayer.Stores.Discounts
{
    public class OpenDiscount : ABasicDiscount
    {
        public double Precent { get; set; }
        public Guid ItemID { get; set; }
        public string ItemName { get; set; }
        

        public OpenDiscount(Guid itemID, double discount, DateTime dateUntil, string itemName):base(dateUntil)
        {
            this.Precent = discount;
            this.ItemID = itemID;
            this.ItemName = itemName;
        }

        public OpenDiscount()
        {
        }

        public override bool IsConditionSatisfied(Dictionary<Guid, Tuple<int, double>> cart)
        {
            return true;
        }

        protected override Dictionary<Guid, Tuple<int, double>> UpdatedPriceList(Dictionary<Guid, Tuple<int, double>> cartBefore)
        {
            Dictionary<Guid, Tuple<int, double>> cartAfter = deepCopyCart(cartBefore);
            if (cartAfter.ContainsKey(ItemID))
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
            return String.Format("{0} precent discount on {1} \nfinish date: {2}", Precent, ItemName, this.DateUntil.Date);
        }
        internal override bool isForItem(Guid itemID)
        {
            return this.ItemID.Equals(itemID);
        }
    }
}

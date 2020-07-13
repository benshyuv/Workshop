using AcceptanceTests.OperationsAPI;
using System;
using System.Collections.Generic;
using System.Text;

namespace AcceptanceTests.DataObjects
{
    public class ItemConditionalDiscount_DiscountOnExtraItems :Discount
    {
        public Guid ItemID { get; set; }
        public int NumOfItems { get; set; }
        public int ExtraItems { get; set; }
        public double DiscountForExtra { get; set; }

        public ItemConditionalDiscount_DiscountOnExtraItems(Guid itemID, int numOfItems, int extraItems, double discountForExtra, Guid discountID, DateTime dateUntil) : base(discountID, dateUntil)
        {
            ItemID = itemID;
            NumOfItems = numOfItems;
            ExtraItems = extraItems;
            DiscountForExtra = discountForExtra;
        }

       public ItemConditionalDiscount_DiscountOnExtraItems(SystemObjJsonConveter.JsonItemConditionalDiscount_DiscountOnExtraItems jDiscount) : base(jDiscount.discountID, jDiscount.DateUntil)
        {
            ItemID = jDiscount.ItemID;
            NumOfItems = jDiscount.MinItems;
            ExtraItems = jDiscount.ExtraItems;
            DiscountForExtra = jDiscount.DiscountForExtra;
        }
    }
}

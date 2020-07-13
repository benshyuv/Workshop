using AcceptanceTests.OperationsAPI;
using System;
using System.Collections.Generic;
using System.Text;

namespace AcceptanceTests.DataObjects
{
    public class ItemConditionalDiscountOnAll : Discount
    {
        public ItemConditionalDiscountOnAll(Guid itemID, int numOfItem, double discount, Guid discountID, DateTime dateUntil) : base(discountID, dateUntil)
        {
            ItemID = itemID;
            NumOfItem = numOfItem;
            Discount = discount;
        }

        public ItemConditionalDiscountOnAll(SystemObjJsonConveter.JsonItemConditionalDiscountOnAll jDiscount) : base(jDiscount.discountID, jDiscount.DateUntil)
        {
            ItemID = jDiscount.ItemID;
            NumOfItem = jDiscount.MinItems;
            Discount = jDiscount.Precent;
        }

        public Guid ItemID { get; set; }
        public int NumOfItem { get; set; }
        public double Discount { get; set; }

    }
}

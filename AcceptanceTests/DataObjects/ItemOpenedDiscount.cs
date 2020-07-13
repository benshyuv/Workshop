using AcceptanceTests.OperationsAPI;
using System;
using System.Collections.Generic;
using System.Text;

namespace AcceptanceTests.DataObjects
{
    public class ItemOpenedDiscount: Discount
    {
        public ItemOpenedDiscount(Guid itemID, double discount, Guid discountID, DateTime dateUntil) : base(discountID, dateUntil)
        {
            ItemID = itemID;
            Discount = discount;
        }

        public ItemOpenedDiscount(SystemObjJsonConveter.JsonItemOpenedDiscount jDiscount) : base(jDiscount.discountID, jDiscount.DateUntil)
        {
            ItemID = jDiscount.ItemID;
            Discount = jDiscount.Precent;
        }

        public Guid ItemID { get; set; }
        public double Discount { get; set; }
    }
}

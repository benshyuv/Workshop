using AcceptanceTests.OperationsAPI;
using System;
using System.Collections.Generic;
using System.Text;

namespace AcceptanceTests.DataObjects
{
    public class StoreConditionalDiscount : Discount
    {
        public double MinPurchaseSum { get; set; }
        public double Discount { get; set; }

        public StoreConditionalDiscount(double minPurchaseSum, double discount, Guid discountID, DateTime dateUntil) : base(discountID, dateUntil)
        {
            MinPurchaseSum = minPurchaseSum;
            Discount = discount;
        }

        public StoreConditionalDiscount(SystemObjJsonConveter.JsonStoreConditionalDiscount jDiscount) : base(jDiscount.discountID, jDiscount.DateUntil)
        {
            MinPurchaseSum = jDiscount.MinPurchase;
            Discount = jDiscount.Precent;
        }
    }
}

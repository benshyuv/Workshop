using AcceptanceTests.OperationsAPI;
using System;
using System.Collections.Generic;
using System.Text;

namespace AcceptanceTests.DataObjects
{
    public class CompositeTwoDiscounts : Discount
    {
        public Guid DiscountLeftID { get; set; }
        public Guid DiscountRightID { get; set; }
        public Utilitys.Utils.Operator Operator { get; set; }
       
        public CompositeTwoDiscounts(Guid discountLeftID, Guid discountRightID, Utilitys.Utils.Operator op, Guid discountID, DateTime dateUntil) : base(discountID, dateUntil)
        {
            DiscountLeftID = discountLeftID;
            DiscountRightID = discountRightID;
            Operator = op;
        }

        public CompositeTwoDiscounts(SystemObjJsonConveter.JsonCompositeTwoDiscounts jDiscount) : base(jDiscount.discountID, jDiscount.DateUntil)
        {
            DiscountLeftID = jDiscount.DiscountLeftID;
            DiscountRightID = jDiscount.DiscountRightID;
            Operator = jDiscount.Op;
        }
    }
}

using System;
using DomainLayer.Stores.Discounts;

namespace DomainLayer.Exceptions
{
    public class DiscountTypeAlreadyAllowedException : Exception
    {
        public DiscountTypeAlreadyAllowedException(DiscountType type) :
            base(String.Format("Discount type {0} is already allowed", type.ToString()))
        {

        }
    }
}

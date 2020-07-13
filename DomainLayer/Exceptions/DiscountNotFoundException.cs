using System;
namespace DomainLayer.Exceptions
{
    public class DiscountNotFoundException : Exception
    {
        public DiscountNotFoundException(Guid DiscountID)
            : base(string.Format("Invalid Discount id: {0}", DiscountID))
        {

        }
    }
}

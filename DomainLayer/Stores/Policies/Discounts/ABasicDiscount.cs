using System;
using System.Collections.Generic;
using DomainLayer.DbAccess;
using DomainLayer.Stores.Inventory;
using DomainLayer.Users;

namespace DomainLayer.Stores.Discounts
{
    public abstract class ABasicDiscount : ADiscount
    {

        public ABasicDiscount(DateTime dateUntil) : base(dateUntil)
        {
        }

        public ABasicDiscount()
        {
        }

        protected abstract Dictionary<Guid, Tuple<int, double>> UpdatedPriceList(Dictionary<Guid, Tuple<int, double>> cartBefore);

        public override Dictionary<Guid, Tuple<int, double>> GetUpdatedPricesFromCart(Dictionary<Guid, Tuple<int, double>> cartBefore)
        {
            if (DateTime.Now.Date <= DateUntil.Date && IsConditionSatisfied(cartBefore))
            {
                return UpdatedPriceList(cartBefore);
            }
            else
                return cartBefore;
        }

        public override bool deleteChildren(DiscountPolicy discountPolicy, MarketDbContext context)
        {
            return true;
        }
    }
}

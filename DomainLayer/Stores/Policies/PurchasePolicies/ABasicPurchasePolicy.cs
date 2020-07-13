using System;
using DomainLayer.DbAccess;

namespace DomainLayer.Stores.PurchasePolicies
{
    public abstract class ABasicPurchasePolicy:APurchasePolicy
    {
        public override bool deleteChildren(PurchasePolicy discountPolicy, MarketDbContext context)
        {
            return true;
        }
    }
}

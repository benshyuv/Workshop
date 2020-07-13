using DomainLayer.DbAccess;
using DomainLayer.Stores.PurchasePolicies;
using System;
using System.Collections.Generic;

namespace DomainLayer.Stores.Discounts
{
    public interface IStorePolicyManager
    {
        OpenDiscount AddOpenDiscount(Guid itemID, double discount, DateTime dateUntil, MarketDbContext context);
        ItemConditionalDiscount_MinItems_ToDiscountOnAll AddItemConditionalDiscount_MinItems_ToDiscountOnAll(Guid itemID, double discount, int minItems, DateTime dateUntil, MarketDbContext context);
        ItemConditionalDiscount_MinItems_ToDiscountOnExtraItems AddItemConditionalDiscount_MinItems_ToDiscountOnExtraItems(Guid itemID, double discountForExtra, int minItems, int extraItems, DateTime dateUntil, MarketDbContext context);
        StoreConditionalDiscount AddStoreConditionalDiscount(double minPurchase, double discount, DateTime dateUntil, MarketDbContext context);
        bool DiscountExistsInStore(Guid discountID, MarketDbContext context);
        CompositeDiscount AddCompositeDiscount(Guid discountLeftID, Guid discountRightID, string boolOperator, MarketDbContext context);
        bool RemoveDiscount(Guid discountID, MarketDbContext context);

        bool IsDiscountAllowed(DiscountType discountType, MarketDbContext context);
        bool MakeDiscountNotAllowed(DiscountType discountType, MarketDbContext context);
        bool MakeDiscountAllowed(DiscountType discountType, MarketDbContext context);
        List<DiscountType> GetAllowedDiscounts(MarketDbContext context);

        bool IsValidToCreateStoreConditionalDiscount(MarketDbContext context);
        bool IsValidToCreateConditionalItemDiscount(Guid itemID, MarketDbContext context);
        bool IsValidToCreateOpenItemDiscount(Guid itemID, MarketDbContext context);

        List<ADisountDataClassForSerialization> GetAllDiscounts(Guid? itemID);
        bool IsValidToCreateItemMinMaxPurchasePolicy(Guid itemID, MarketDbContext context);
        bool IsValidToCreateStoreMinMaxPurchasePolicy(MarketDbContext context);
        bool IsValidToCreateDaysNotAllowedPurchasePolicy(MarketDbContext context);
        ItemMinMaxPurchasePolicy AddItemMinMaxPurchasePolicy(Guid itemID, int? minAmount, int? maxAmount, MarketDbContext context);
        StoreMinMaxPurchasePolicy AddStoreMinMaxPurchasePolicy(int? minAmount, int? maxAmount, MarketDbContext context);
        DaysNotAllowedPurchasePolicy AddDaysNotAllowedPurchasePolicy(int[] daysNotAllowed, MarketDbContext context);
        bool PolicyExistsInStore(Guid policyID, MarketDbContext context);
        CompositePurchasePolicy ComposeTwoPurchasePolicys(Guid policyLeftID, Guid policyRightID, string boolOperator, MarketDbContext context);
        bool IsPurchaseTypeAllowed(PurchasePolicyType policy, MarketDbContext context);
        bool MakePurcahsePolicyNotAllowed(PurchasePolicyType policy, MarketDbContext context);
        bool MakePurcahsePolicyAllowed(PurchasePolicyType policy, MarketDbContext context);
        bool RemovePurchasePolicy(Guid policyID, MarketDbContext context);
        List<APurchasePolicyDataClassForSerialization> GetAllPurchasePolicys();
        List<PurchasePolicyType> GetAllowedPurchasePolicys(MarketDbContext context);
    }
}

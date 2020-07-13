using System;
namespace DomainLayer.Stores.Discounts
{
    public class ADisountDataClassForSerialization
    {

        public Guid DiscountID;
        public Guid ItemID;
        public DateTime DateUntill;
        public Operator Op;
        public ADisountDataClassForSerialization LeftDiscount;
        public ADisountDataClassForSerialization RightDiscount;
        public double Precent;
        public double MinPurchase;
        public int MinItems;
        public int ExtraItems;
        public string Description;

        //must be from [opened, itemCondOnALl, itemCondOnExtra, storeCond, composite]
        public string discountType;

        public ADisountDataClassForSerialization(OpenDiscount discount)
        {
            this.DiscountID = discount.discountID;
            this.ItemID = discount.ItemID;
            this.DateUntill = discount.DateUntil;
            this.Precent = discount.Precent;
            this.Description = discount.ToString();
            this.discountType = "opened";
        }

        public ADisountDataClassForSerialization(ItemConditionalDiscount_MinItems_ToDiscountOnAll discount)
        {
            this.DiscountID = discount.discountID;
            this.ItemID = discount.ItemID;
            this.DateUntill = discount.DateUntil;
            this.Description = discount.ToString();
            this.Precent = discount.Precent;
            this.MinItems = discount.MinItems;
            this.discountType = "itemCondOnALl";
        }

        public ADisountDataClassForSerialization(ItemConditionalDiscount_MinItems_ToDiscountOnExtraItems discount)
        {
            this.DiscountID = discount.discountID;
            this.ItemID = discount.ItemID;
            this.DateUntill = discount.DateUntil;
            this.Description = discount.ToString();
            this.Precent = discount.DiscountForExtra;
            this.MinItems = discount.MinItems;
            this.ExtraItems = discount.ExtraItems;
            this.discountType = "itemCondOnExtra";
        }

        public ADisountDataClassForSerialization(StoreConditionalDiscount discount)
        {
            this.DiscountID = discount.discountID;
            this.DateUntill = discount.DateUntil;
            this.Precent = discount.Precent;
            this.MinPurchase = discount.MinPurchase;
            this.Description = discount.ToString();
            this.discountType = "storeCond";
        }

        public ADisountDataClassForSerialization(CompositeDiscount discount)
        {
            this.DiscountID = discount.discountID;
            this.DateUntill = discount.DateUntil;
            this.Op = discount.Op;
            this.LeftDiscount = discount.DiscountLeft.ToDataClass();
            this.RightDiscount = discount.DiscountRight.ToDataClass();
            this.Description = discount.ToString();
            this.discountType = "composite";
        }
    }
}

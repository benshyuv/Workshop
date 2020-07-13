using DomainLayer.DbAccess;
using DomainLayer.Exceptions;
using DomainLayer.Orders;
using DomainLayer.Stores.Inventory;
using DomainLayer.Users;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;

namespace DomainLayer.Stores.Discounts
{
    public enum DiscountType
    {
        OPENED = 1, ITEM_CONDITIONAL = 2, STORE_CONDITIONAL = 4, COMPOSITE = 8
    }
    public class DiscountPolicy
    {
        [Key, ForeignKey("Store")]
        public Guid StoreId { get; set; }
        public virtual Store Store { get; set; }

        private Dictionary<Guid,ADiscount> allDiscountsForRetrevalByGuidOnly
        {
            get => AllDiscounts.ToDictionary(d => d.discountID);
        }

        //default for active discounts is an OR between all of them as discussed with yair
        internal List<ADiscount> ActiveDiscounts 
        {
            get => AllDiscounts.Where(d => d.Active).ToList();
        }
        public virtual ICollection<ADiscount> AllDiscounts { get; set; }

        public DiscountType DiscountFlags { get; set; }
        internal ISet<DiscountType> AllowedDiscounts
        {
            get
            {
                HashSet<DiscountType> result = new HashSet<DiscountType>();
                foreach (DiscountType type in Enum.GetValues(typeof(DiscountType)))
                {
                    if (DiscountFlags.HasFlag(type))
                    {
                        result.Add(type);
                    }
                }
                return result;
            }
            private set
            {
                DiscountFlags = 0;
                if (!(value is null))
                {
                    foreach (DiscountType type in value)
                    {
                        DiscountFlags |= type;
                    }
                }
            }
        }

        public DiscountPolicy(Guid id, HashSet<DiscountType> allowedDiscounts)
        {
            StoreId = id;
            AllDiscounts = new HashSet<ADiscount>();

            DiscountFlags = 0;
            foreach (DiscountType discountType in allowedDiscounts)
            {
                DiscountFlags |= discountType;
            }
        }

        public DiscountPolicy(Guid id)
        {
            StoreId = id;
            foreach (DiscountType type in Enum.GetValues(typeof(DiscountType)))
            {
                DiscountFlags |= type;
            }
            AllDiscounts = new HashSet<ADiscount>();
        }

        public DiscountPolicy()
        {
        }

        public bool MakeDiscountNotAllowed(DiscountType discountType, MarketDbContext context)
        {
            if (!IsDiscountTypeAllowed(discountType))
            {
                throw new DiscountTypeNotAllowedException(discountType);
            }
            DiscountFlags ^= discountType;
            context.SaveChanges();
            return true;
        }

        public bool MakeDiscountAllowed(DiscountType discountType, MarketDbContext context)
        {
            if (IsDiscountTypeAllowed(discountType))
            {
                throw new DiscountTypeAlreadyAllowedException(discountType);
            }
            DiscountFlags |= discountType;
            context.SaveChanges();
            return true;
        }

        public bool IsDiscountTypeAllowed(DiscountType discountType)
        {
            return DiscountFlags.HasFlag(discountType);
        }

        public List<DiscountType> GetAllowedDiscountTypes()
        {
            return new List<DiscountType>(AllowedDiscounts);
        }

        internal Dictionary<Guid, Tuple<int, double>> GetPricesAfterDiscount(Dictionary<Guid, Tuple<int, double>> cart) {
            //OR is default between active discounts (if one fails then the reutrn value is equal to the given value).
            foreach(ADiscount discount in ActiveDiscounts)
            {
                cart = discount.GetUpdatedPricesFromCart(cart);
            }
            return cart;
        }

        public bool IsValidToCreateOpenItemDiscount(Guid itemID)
        {
            foreach(ADiscount aDiscount in ActiveDiscounts)
            {
                if (aDiscount is OpenDiscount openDiscount && openDiscount.ItemID.Equals(itemID)
                    && openDiscount.DateUntil.Date > DateTime.Now.Date)
                {
                    return false;
                }
            }
            return true;
        }

        public bool IsValidToCreateConditionalItemDiscount(Guid itemID)
        {
            foreach (ADiscount aDiscount in ActiveDiscounts)
            {
                if ((aDiscount is ItemConditionalDiscount_MinItems_ToDiscountOnAll conditionalDiscountToAll
                    && conditionalDiscountToAll.ItemID.Equals(itemID)
                    && conditionalDiscountToAll.DateUntil.Date > DateTime.Now.Date) ||
                    (aDiscount is ItemConditionalDiscount_MinItems_ToDiscountOnExtraItems conditionalDiscountExtra
                    && conditionalDiscountExtra.ItemID.Equals(itemID)
                    && conditionalDiscountExtra.DateUntil.Date > DateTime.Now.Date))

                {
                    return false;
                }
            }
            return true;
        }

        public bool IsValidToCreateStoreConditionalDiscount()
        {
            foreach (ADiscount aDiscount in ActiveDiscounts)
            {
                if (aDiscount is StoreConditionalDiscount storeConditional
                    && storeConditional.DateUntil.Date > DateTime.Now.Date)

                {
                    return false;
                }
            }
            return true;
        }

        //Assumes its a valid discount <==> IsValidOpenItemDiscount(itemID, datUntill) == True
        public OpenDiscount AddOpenDiscount(Guid itemID, double discount, DateTime dateUntil,string itemName, MarketDbContext context)
        {

            if (!IsDiscountTypeAllowed(DiscountType.OPENED))
            {
                throw new DiscountTypeNotAllowedException(DiscountType.OPENED);
            }

            OpenDiscount openDiscount = new OpenDiscount(itemID, discount, dateUntil, itemName);
            AllDiscounts.Add(openDiscount);
            context.Discounts.Add(openDiscount);
            context.SaveChanges();
            return openDiscount;
        }
        //Assumes its a valid discount <==> IsValidOpenItemDiscount(datUntill) == True
        public StoreConditionalDiscount AddStoreConditionalDiscount(DateTime dateUntil, double minPurchase, double discount, MarketDbContext context)
        {
            if (!IsDiscountTypeAllowed(DiscountType.STORE_CONDITIONAL))
            {
                throw new DiscountTypeNotAllowedException(DiscountType.STORE_CONDITIONAL);
            }

            StoreConditionalDiscount storeConditional = new StoreConditionalDiscount(dateUntil, minPurchase, discount);
            AllDiscounts.Add(storeConditional);
            context.Discounts.Add(storeConditional);
            context.SaveChanges();
            return storeConditional;
        }
        //Assumes its a valid discount <==> IsValidConditionalItemDiscount(itemID, datUntill) == True
        public ItemConditionalDiscount_MinItems_ToDiscountOnAll AddItemConditionalDiscount_MinItems_ToDiscountOnAll(
            Guid itemID, DateTime dateUntill,int minItems, double discount, string itemName, MarketDbContext context)
        {
            if (!IsDiscountTypeAllowed(DiscountType.ITEM_CONDITIONAL))
            {
                throw new DiscountTypeNotAllowedException(DiscountType.ITEM_CONDITIONAL);
            }

            ItemConditionalDiscount_MinItems_ToDiscountOnAll itemConditional = new ItemConditionalDiscount_MinItems_ToDiscountOnAll
                (itemID, dateUntill, minItems, discount, itemName);
            AllDiscounts.Add(itemConditional);
            context.Discounts.Add(itemConditional);
            context.SaveChanges();
            return itemConditional;
        }
        //Assumes its a valid discount <==> IsValidConditionalItemDiscount(itemID, datUntill) == True
        public ItemConditionalDiscount_MinItems_ToDiscountOnExtraItems AddItemConditionalDiscount_MinItems_ToDiscountOnExtraItems(
            Guid itemID, DateTime dateUntil, int minItems,int extraItems, double discountForExtra, string itemName, MarketDbContext context)
        {
            if (!IsDiscountTypeAllowed(DiscountType.ITEM_CONDITIONAL))
            {
                throw new DiscountTypeNotAllowedException(DiscountType.ITEM_CONDITIONAL);
            }

            ItemConditionalDiscount_MinItems_ToDiscountOnExtraItems itemConditional = new ItemConditionalDiscount_MinItems_ToDiscountOnExtraItems
                ( itemID,  dateUntil,  minItems,  extraItems, discountForExtra, itemName);
            AllDiscounts.Add(itemConditional);
            context.Discounts.Add(itemConditional);
            context.SaveChanges();
            return itemConditional;
        }

        public CompositeDiscount AddCompositeDiscount(Guid discountLeftID, Guid discountRightID, Operator boolOperator, MarketDbContext context)
        {
            if (!IsDiscountTypeAllowed(DiscountType.COMPOSITE))
            {
                throw new DiscountTypeNotAllowedException(DiscountType.COMPOSITE);
            }

            if (!DiscountExist(discountLeftID) || !DiscountExist(discountRightID))
            {
                throw new ArgumentException("One or both of the Discount ids dont exist");
            }
            ADiscount left = GetDiscountById(discountLeftID);
            ADiscount right = GetDiscountById(discountRightID);

            CompositeDiscount compositeDiscount = new CompositeDiscount(left, right, boolOperator);
            AllDiscounts.Add(compositeDiscount);
            left.Active = false;
            right.Active = false;
            //ActiveDiscounts.Remove(left);
            //ActiveDiscounts.Remove(right);
            context.Discounts.Add(compositeDiscount);
            context.SaveChanges();
            return compositeDiscount;
        }

        private ADiscount GetDiscountById(Guid discountID)
        {
            return AllDiscounts.Where(d => d.discountID == discountID).Single();
        }

        public bool DeleteDiscount(Guid discountID, MarketDbContext context)
        {
            if (!DiscountExist(discountID))
            {
                throw new ArgumentException("Discount id doesnt exist");
            }
            ADiscount discount = GetDiscountById(discountID);
            if (!ActiveDiscounts.Contains(discount))
            {
                throw new ArgumentException("Can only remove active discount, cannot remove basic discount that there exist" +
                    " a composite discount witch is composed of it.");
            }
            AllDiscounts.Remove(discount);
            context.Discounts.Remove(discount);
            context.SaveChanges();
            return discount.deleteChildren(this, context);
        }

        internal bool DiscountExist(Guid discountID)
        {
            return AllDiscounts.Any(d => d.discountID == discountID);
        }

        internal bool DeleteDiscountRecursiveFromNotActiveDiscounts(Guid discountID, MarketDbContext context)
        {
            if (!DiscountExist(discountID))
            {
                throw new ArgumentException("Discount id doesnt exist");
            }
            ADiscount discount = GetDiscountById(discountID);
            AllDiscounts.Remove(discount);
            context.Discounts.Remove(discount);
            context.SaveChanges();
            return discount.deleteChildren(this, context);
        }

        public virtual bool ShouldSerializeStore()
        {
            return false;
        }

        public virtual bool ShouldSerializeActiveDiscounts()
        {
            return false;
        }

        internal List<ADisountDataClassForSerialization> GetAllDiscounts(Guid? itemID)
        {
            List<ADisountDataClassForSerialization> ret = new List<ADisountDataClassForSerialization>();

            if (itemID is Guid iid)
            {
                foreach (ADiscount discount in ActiveDiscounts)
                {
                    if (discount.isForItem(iid))
                    {
                        ret.Add(discount.ToDataClass());
                    }
                }
            }
            else
            {
                foreach (ADiscount discount in ActiveDiscounts)
                {
                    ret.Add(discount.ToDataClass());
                }
            }
            return ret;
        }
    }
}

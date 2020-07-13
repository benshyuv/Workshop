using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using DomainLayer.DbAccess;
using DomainLayer.Stores.Inventory;
using Newtonsoft.Json;

namespace DomainLayer.Stores.Discounts
{
    public enum Operator
    {
        XOR, OR, AND, IMPLIES
    }

    public class CompositeDiscount : ADiscount
    {
        public Operator Op { get;  set; }
        
        public Guid DiscountLeftID { get; set; }
        
        public Guid DiscountRightID { get; set; }

        [JsonIgnore]
        public List<ADiscount> ChildrenDiscounts { get; set; }

        [JsonIgnore]
        [NotMapped]
        public ADiscount DiscountLeft
        {
            get {
                    if (ChildrenDiscounts != null)
                    {
                        return ChildrenDiscounts[0];
                    }
                    else //because of desirialization
                    {
                        return null;
                    }
               }
            //set { DiscountLeft = value; }
        }

        [JsonIgnore]
        [NotMapped]
        public ADiscount DiscountRight
        {
            get {
                    if (ChildrenDiscounts != null)
                    {
                        return ChildrenDiscounts[1];
                    }
                    else //because of desirialization
                    {
                        return null;
                    }
                }
            //set { DiscountRight = value; }
        }



        //CTOR for serialization only.
        public CompositeDiscount() { }

        public CompositeDiscount(ADiscount discountLeft, ADiscount discountRight, Operator boolOperator):base(new DateTime())
        {
            this.DateUntil = discountLeft.DateUntil.Date < discountRight.DateUntil.Date ? discountLeft.DateUntil.Date : discountRight.DateUntil.Date;
            this.Op = boolOperator;

            this.ChildrenDiscounts = new List<ADiscount>();
            this.ChildrenDiscounts.Add(discountLeft);
            this.ChildrenDiscounts.Add(discountRight);

            //this.DiscountLeft = discountLeft;
            //this.DiscountRight = discountRight;
            this.DiscountLeftID = discountLeft.discountID;
            this.DiscountRightID = discountRight.discountID;
        }

        public override Dictionary<Guid, Tuple<int, double>> GetUpdatedPricesFromCart(Dictionary<Guid, Tuple<int, double>> cartBefore)
        {
            if (IsConditionSatisfied(cartBefore))
            {
                return CombineBothDiscounts(cartBefore);
            }
            else
                return cartBefore;
        }

        private Dictionary<Guid, Tuple<int, double>> CombineBothDiscounts(Dictionary<Guid, Tuple<int, double>> cartBefore)
        {
            Dictionary<Guid, Tuple<int, double>> leftCart = DiscountLeft.GetUpdatedPricesFromCart(cartBefore);
            Dictionary<Guid, Tuple<int, double>> cartAfter = DiscountRight.GetUpdatedPricesFromCart(leftCart);
            
            return cartAfter;
        }

        public override bool IsConditionSatisfied(Dictionary<Guid, Tuple<int, double>> cart)
        {
            if (DateTime.Now.Date <= DateUntil.Date)
            {
                switch (Op)
                {
                    case Operator.AND:
                        return DiscountLeft.IsConditionSatisfied(cart) && DiscountRight.IsConditionSatisfied(cart);
                    case Operator.OR:
                        return DiscountLeft.IsConditionSatisfied(cart) || DiscountRight.IsConditionSatisfied(cart);
                    case Operator.XOR:
                        if (DiscountLeft.IsConditionSatisfied(cart))
                        {
                            return !DiscountRight.IsConditionSatisfied(cart);
                        }
                        else
                        {
                            return DiscountRight.IsConditionSatisfied(cart);
                        }
                    case Operator.IMPLIES:
                        if (DiscountLeft.IsConditionSatisfied(cart))
                        {
                            return DiscountRight.IsConditionSatisfied(cart);
                        }
                        else
                            return true;
                    default:
                        throw new NotSupportedException();
                }
            }
            else
            {
                return false;
            }
        }

        public override bool deleteChildren(DiscountPolicy discountPolicy, MarketDbContext context)
        {
            bool leftremoved = discountPolicy.DeleteDiscountRecursiveFromNotActiveDiscounts(DiscountLeftID, context);
            bool rightremoved = discountPolicy.DeleteDiscountRecursiveFromNotActiveDiscounts(DiscountRightID, context);
            bool resultDelete = leftremoved && rightremoved;
            if (resultDelete)
            {
                this.ChildrenDiscounts = new List<ADiscount>();
            }
            return leftremoved && rightremoved;
        }

        internal override ADisountDataClassForSerialization ToDataClass()
        {
            return new ADisountDataClassForSerialization(this);
        }

        public override string ToString()
        {
            return String.Format("{0} Composite discount\nLeft: {1}  \nRight: {2} \nfinish date: {3}", this.Op.ToString(), this.DiscountLeft, this.DiscountRight, this.DateUntil.Date);
        }

        internal override bool isForItem(Guid itemID)
        {
            return DiscountLeft.isForItem(itemID) || DiscountRight.isForItem(itemID);
        }
    }

    
}

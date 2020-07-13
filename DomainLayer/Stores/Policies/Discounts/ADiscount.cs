using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using DomainLayer.DbAccess;
using DomainLayer.Stores.Inventory;
using Newtonsoft.Json;

namespace DomainLayer.Stores.Discounts
{
    public abstract class ADiscount : IDiscount, IEquatable<ADiscount>
    {
        [Key]
        public Guid discountID { get; set; }
        public DateTime DateUntil { get; set; }

        public bool Active { get; set; }

        public ADiscount()
        {
        }

        public ADiscount(DateTime dateUntil)
        {
            discountID = Guid.NewGuid();
            DateUntil = dateUntil;
            Active = true;
        }

        public abstract bool IsConditionSatisfied(Dictionary<Guid, Tuple<int, double>> cartBefore);

        public abstract Dictionary<Guid, Tuple<int, double>> GetUpdatedPricesFromCart(Dictionary<Guid, Tuple<int, double>> cartBefore);

        protected Dictionary<Guid, Tuple<int, double>> deepCopyCart(Dictionary<Guid, Tuple<int, double>> cartBefore)
        {
            Dictionary<Guid, Tuple<int, double>> retVal = new Dictionary<Guid, Tuple<int, double>>();
            foreach(KeyValuePair< Guid, Tuple<int, double>> keyValuePair in cartBefore)
            {
                retVal[keyValuePair.Key] = new Tuple<int, double>(keyValuePair.Value.Item1, keyValuePair.Value.Item2);
            }
            return retVal;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as ADiscount);
        }

        public bool Equals(ADiscount other)
        {
            return other != null &&
                   discountID.Equals(other.discountID);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(discountID);
        }

        public abstract bool deleteChildren(DiscountPolicy discountPolicy, MarketDbContext context);

        internal abstract ADisountDataClassForSerialization ToDataClass();

        public abstract override string ToString();

        internal abstract bool isForItem(Guid itemID);
        
    }
}

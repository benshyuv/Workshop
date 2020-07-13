using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace AcceptanceTests.DataObjects
{
    public class Discount : IEquatable<Discount>
    {
        public Guid DiscountID { get; set; }
        public DateTime DateUntil { get; set; }

        public Discount( Guid discountID, DateTime dateUntil)
        {
            DiscountID = discountID;
            DateUntil = dateUntil;
        }

        public bool Equals([AllowNull] Discount other)
        {
            return other != null &&
            DiscountID.Equals(other.DiscountID);
        }

        public override bool Equals(object obj)
        {
            return Equals(obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(DiscountID);
        }

    }
}

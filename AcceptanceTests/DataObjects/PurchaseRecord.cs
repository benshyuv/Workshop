using System;

namespace AcceptanceTests.DataObjects
{
    public class PurchaseRecord : IEquatable<PurchaseRecord>
    {
        public Guid OrderId { get; set; }
        public string Name { get; set; }
        public Guid ItemId { get; set; }
        public double BasePriceOnPurchase { get; set; }
        public double DiscountedPriceOnPurchase { get; set; }
        public int amountBought { get; set; }
        public Item ItemObj { get; set; }
        public Guid storeID { get; set; }
        public PurchaseRecord(Guid orderId, string name, Guid itemId, double priceOnPurchase, int amountBought)
        {
            OrderId = orderId;
            Name = name;
            ItemId = itemId;
            BasePriceOnPurchase = priceOnPurchase;
            this.amountBought = amountBought;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as PurchaseRecord);
        }

        public bool Equals(PurchaseRecord other)
        {
            return other != null &&
                   Name.ToLower().Trim() == other.Name.ToLower().Trim() &&
                   ItemId.Equals(other.ItemId) &&
                   BasePriceOnPurchase == other.BasePriceOnPurchase &&
                   amountBought == other.amountBought;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Name, ItemId, BasePriceOnPurchase, amountBought);
        }

    }
}

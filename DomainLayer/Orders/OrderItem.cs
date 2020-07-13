using DomainLayer.Stores.Inventory;
using System;
using System.Collections.Generic;
using CustomLogger;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DomainLayer.Orders
{
    [Serializable]
    public class OrderItem
    {
        [Key]
        public Guid ItemId { get; set; }

        [Key, ForeignKey("StoreOrder")]
        public Guid StoreOrderId { get; set; }
        [JsonIgnore]
        public virtual StoreOrder StoreOrder { get; set; }
        public int Amount { get; set; }
        public string Name { get; set; }
        public virtual ICollection<Category> Categories { get; set; }
        public double Rank { get; set; }
        public double BasePricePerItem { get; set; }
        public double DiscountedPricePerItem { get; set; }
        public virtual ICollection<Keyword> KeyWords { get; set; }

        //public OrderItem(Guid item, int amount, Guid storeOrderId)
        //{
        //    ItemId = item;
        //    Amount = amount;
        //    StoreOrderId = storeOrderId;
        //}

        [JsonConstructor]
        private OrderItem(Guid itemID, int amount, string name, HashSet<Category> categories, HashSet<Keyword> keyWords, 
                            double rank, double basePricePerItem, double discountedPricePerItem)
        {
            ItemId = itemID;
            Name = name;

            Categories = categories;

            KeyWords = keyWords;

            Rank = rank;
            BasePricePerItem = basePricePerItem;
            DiscountedPricePerItem = discountedPricePerItem;
            Amount = amount;
        }

        public OrderItem(Guid storeOrderID, Item i, int amount, double discountedPricePerItem)
		{
            StoreOrderId = storeOrderID;
            ItemId = i.Id;
            Name = i.Name;

            Categories = new HashSet<Category>(i.Categories);

            KeyWords = new HashSet<Keyword>(i.Keywords);
            
            Rank = i.Rank;
            BasePricePerItem = i.Price;
            DiscountedPricePerItem = discountedPricePerItem;
            Amount = amount;
            
            Logger.writeEvent("OrderManager: OrderItem| the order item's details were saved");
        }

        public OrderItem()
        {
        }

        internal double GetTotal() => DiscountedPricePerItem * Amount;//make sure DPPI is final price, and not limited to discounts

        public void UpdateData(Item i, double? discountedPricePerItem)
        {
            Name = i.Name;
            Rank = i.Rank;
            BasePricePerItem = i.Price;

            Categories = new HashSet<Category>(i.Categories);

            KeyWords = new HashSet<Keyword>(i.Keywords);


            if (discountedPricePerItem is double dppi)
            {
                DiscountedPricePerItem = dppi;
            }

            Logger.writeEvent("OrderManager: UpdateData| the order item's details were saved");

        }
    }
}

using DomainLayer.Stores;
using DomainLayer.Users;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;

namespace DomainLayer.Orders
{
    public class StoreOrder
    {
        public Guid Id { get; set; }
        [ForeignKey("User")]
        public Guid? userKey { get; set; }
        public Guid UserId
        {
            get => (userKey is Guid id) ? id : Guid.Empty;
            set => userKey = value;
        }

        public virtual RegisteredUser User { get; set; }

        [ForeignKey("Order")]
        public Guid OrderId { get; set; }

        [JsonIgnore]
        public virtual Order Order { get; set; }

        [ForeignKey("Store")]
        public Guid StoreId { get; set; }
        [JsonIgnore]
        public virtual Store Store { get; set; }

        public virtual ICollection<OrderItem> StoreOrderItems { get; set; }
        public Dictionary<Guid, OrderItem> OrderItemsDict 
        {
            get => StoreOrderItems.ToDictionary(oi => oi.ItemId);
            set => StoreOrderItems = value.Values;
        }

        //public int type { get; set; }
        //public PurchaseType Type 
        //{ 
        //    get => (PurchaseType)type; 
        //    set => type = (int)value; 
        //}

        public PurchaseType Type { get; set; }

        //public int status { get; set; }
        //public OrderStatus Status
        //{
        //    get => (OrderStatus)status;
        //    set => status = (int)value;
        //}

        public OrderStatus Status { get; set; }
        public DateTime OrderTime { get; set; }

        public double Total { get; set; }

        public StoreOrder(Guid id, Guid orderId, Guid? userID, Guid storeID, List<OrderItem> storeOrderItems, PurchaseType type, DateTime orderTime)
        {
            Id = id;
            this.userKey = userID;
            OrderId = orderId;
            StoreId = storeID;
            this.StoreOrderItems = storeOrderItems;
            Total = GetTotal();
            Type = type;
            OrderTime = orderTime;
            Status = OrderStatus.PENDING;
        }

        //[JsonConstructor]
        //private StoreOrder(Guid id, Guid orderId, Guid userID, Guid storeID, Dictionary<Guid, OrderItem> storeOrderItems, double total, PurchaseType type, DateTime orderTime)
        //{
        //    Id = id;
        //    UserId = userID;
        //    OrderId = orderId;
        //    StoreId = storeID;
        //    OrderItemsDict = storeOrderItems;
        //    Total = total;
        //    Type = type;
        //    OrderTime = orderTime;
        //}

        public StoreOrder()
        {
        }

        private double GetTotal()
        {
            double total = 0;
            foreach (OrderItem item in StoreOrderItems)
            {
                total += item.GetTotal();
            }
            return total;
        }

        public virtual bool ShouldSerializeUser()
        {
            return false;
        }
    }
}

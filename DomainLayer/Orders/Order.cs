using DomainLayer.Users;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;

namespace DomainLayer.Orders
{
    public enum OrderStatus { PENDING, PAYED_FOR, DELIVERED, COMPLETED }
    public class Order
    {
        public Guid OrderId { get; set; }

        //[ForeignKey("User")]
        public Guid? userKey { get; set; }
        public Guid UserId 
        {
            get => (userKey is Guid id) ? id : Guid.Empty;
            set 
            {
                userKey = value == Guid.Empty ? null : (Guid?)value;
            }
        }
        //[ForeignKey("UserId")]
        //public virtual RegisteredUser User { get; set; }

        public virtual ICollection<StoreOrder> StoreOrders { get; set; } //stores orders as a whole order
        public Dictionary<Guid, StoreOrder> StoreOrdersDict 
        {
            get => StoreOrders.ToDictionary(so => so.StoreId);
            set => StoreOrders = value.Values;
        } //stores orders as a whole order

        public OrderStatus Status { get; set; }
        public PurchaseType Type { get; set; }
        public DateTime OrderTime { get; set; }
        public double Total { get; set; }
        public int PaymentTransactionId { get; set; }
        public int DeliveryTransactionId { get; set; }

        public Order(Guid orderId, Guid? userID, List<StoreOrder> storeOrders, PurchaseType type, DateTime orderTime)
        {
            OrderId = orderId;
            this.userKey = userID;
            this.StoreOrders = storeOrders;
            Total = GetTotal();
            Type = type;
            OrderTime = orderTime;
            Status = OrderStatus.PENDING;
        }

        public Order()
        {
        }

        private double GetTotal()
        {
            double total = 0;
            foreach (StoreOrder storeOrder in StoreOrdersDict.Values)
            {
                total += storeOrder.Total;
            }
            return total;
        }

        public virtual bool ShouldSerializeUser()
        {
            return false;
        }
    }
}

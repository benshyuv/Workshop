using System;
using System.Linq;
using System.Collections.Concurrent;
using System.Collections.Generic;
using DomainLayer.Users;
using DomainLayer.Stores;
using DomainLayer.Stores.Inventory;
using DomainLayer.Exceptions;
using CustomLogger;
using System.Timers;
using DomainLayer.DbAccess;
using System.Runtime.CompilerServices;
using DomainLayer.NotificationsCenter;
using DomainLayer.NotificationsCenter.NotificationEvents;

[assembly: InternalsVisibleTo("DomainLayerTests")]
namespace DomainLayer.Orders
{
    public class OrderManager : INotificationSubject
    {
        private readonly StoreHandler StoreHandler;
        public PaymentProxy Payment { get;  set; }
        public DeliveryProxy Delivery { get;  set; }
        internal readonly Timer Timer;
        private bool useInMemoryDB;
        private Effort.Provider.EffortConnection inMemoryConnection;
        private HashSet<NotificationObserver> observers;
        private Guid IDForNotifactionSubject;

        public OrderManager(StoreHandler storeHandler, Effort.Provider.EffortConnection inMemoryConnection)
        {
            //Orders = new BlockingCollection<Order>();
            //usersOrders = new Dictionary<Guid, List<Order>>();
            //storesOrders = new Dictionary<Guid, List<StoreOrder>>();
            //pendingOrders = new Dictionary<Guid, Order>();
            StoreHandler = storeHandler;
            observers = new HashSet<NotificationObserver>();
            this.IDForNotifactionSubject = Guid.NewGuid();
            Payment = new PaymentProxy();
            Delivery = new DeliveryProxy();
            if (inMemoryConnection != null)
            {
                this.useInMemoryDB = true;
                this.inMemoryConnection = inMemoryConnection;
            }
            else
            {
                this.useInMemoryDB = false;
            }
            Timer = new Timer
            {
                Interval = 1000 * 60 * 2
            };
            Timer.Enabled = true;
            Timer.Elapsed += new ElapsedEventHandler(ReviewPendings);
        }

        public bool ExternalSystemsAreConnected()
        {
            Logger.writeEvent("OrderManager: checking external systems connections");
            if (Payment.IsConnected().Result && Delivery.IsConnected().Result)
            {
                Logger.writeEvent("OrderManager: external systems connected");
                return true;
            }
            else
            {
                Logger.writeEvent("OrderManager: checking external systems connections failed");
                throw new ConnectToExternalSystemsException("Error when connecting to external systems");
            }
        }

        private void CheckInventory(StoreCart storeCart, MarketDbContext context)
        {
            Guid storeID = storeCart.StoreID;
            IStoreInventoryManager store = StoreHandler.GetStoreInventoryManager(storeID, context);
            store.IsOrderItemsAmountAvailable(storeCart.Items, context);
        }

        private OrderItem FindOrderItem(List<OrderItem> items, Guid itemId)
        {
            foreach(OrderItem orderItem in items)
            {
                if (orderItem.ItemId.Equals(itemId))
                {
                    return orderItem;
                }
            }
            Logger.writeEvent(string.Format("OrderManager: FindOrderItem| item: {0} not found", itemId));
            return null;
        }

        internal void LogOrder(Guid? userID, Guid orderID, MarketDbContext context)
        {
            if (!TryGetOrder(orderID, out Order order, context))
            {
                throw new OrderNotValidException(string.Format("No order Exists with this ID:{0}", orderID));
            }
            if (userID != order.userKey)
            {
                throw new OrderNotValidException(string.Format("order:{0} was not made by user:{1}", orderID, userID));
            }
            if (order.Status != OrderStatus.DELIVERED)
            {
                throw new OrderNotValidException(string.Format("order:{0} was not delivered yet", orderID));
            }
            foreach (StoreOrder storeOrder in order.StoreOrders)
            {
                storeOrder.Status = OrderStatus.COMPLETED;
            }
            order.Status = OrderStatus.COMPLETED;
            context.SaveChanges();
            NotifyStoreOwners(order, context);
        }

        private void NotifyStoreOwners(Order order, MarketDbContext context)
        {
            foreach(StoreOrder storeOrder in order.StoreOrders)
            {
                notifyEvent(new BoughtFromStoreEvent(storeOrder.Store.ContactDetails.Name,
                    storeOrder.Store.GetAllOwnerGuids()), context);
            }
        }

        private bool TryGetOrder(Guid orderID, out Order order, MarketDbContext context)
        {
            order = context.Orders.Find(orderID);
            return !(order is null);
        }

        public List<Order> GetUserOrdersHistory(Guid userID, MarketDbContext context)
        {
            List<Order> userOrders = context.Orders.Where(o => o.UserId == userID && o.Status == OrderStatus.COMPLETED).ToList();
            if (userOrders.Count == 0)
            {
                Logger.writeEvent(string.Format("OrderManager: GetUserOrdersHistory| no orders exist for user: {0}", userID));
            }
            else
            {
                Logger.writeEvent(string.Format("OrderManager: GetUserOrdersHistory| found order history for user: {0}", userID));
            }
            return userOrders;
        }

        internal void ValidatePurchase(User user, MarketDbContext context)
        {
            foreach (KeyValuePair<Guid, StoreCart> storeCart in user.Cart.StoreCarts)
            {
                IStoreInventoryManager store = StoreHandler.GetStoreInventoryManager(storeCart.Key, context);
                try
                {
                    store.ValidatePurchase(storeCart.Value, user, context);
                    CheckInventory(storeCart.Value, context);
                }
                catch (PolicyException e)
                {
                    throw new PolicyException(store.GetName(), e.Message);
                }
                catch (ItemAmountException e)
                {
                    throw new ItemAmountException(storeCart.Key, e.Message);
                }
            }
        }

        internal void CollectPayment(Guid? userID, Guid orderID, string cardNum, string expire, string CCV, string cardOwnerName, string cardOwnerID, MarketDbContext context)
        {
            if (!TryGetOrder(orderID, out Order order, context))
            {
                throw new OrderNotValidException(string.Format("No order Exists with this ID:", orderID));
            }
            if (userID != order.userKey)
            {
                throw new OrderNotValidException(string.Format("order:{0} was not made by user:{1}", orderID, userID));
            }
            if (order.Status != OrderStatus.PENDING)
            {
                throw new OrderNotValidException(string.Format("order:{0} was not pending", orderID));
            }
            try
            {
                _ = Payment.Pay(userID, order, cardNum, expire, CCV, cardOwnerName, cardOwnerID).Result;
				
                foreach (StoreOrder storeOrder in order.StoreOrders)
                {
                    storeOrder.Status = OrderStatus.PAYED_FOR;
                }
                order.Status = OrderStatus.PAYED_FOR;
                context.SaveChanges();
            }
            catch (Exception e)
            {
                throw new PaymentFailedException(e.InnerException.Message);
            }
        }

        internal Order DiscountAndReserve(Guid? userID, Dictionary<Guid, StoreCart> storeCarts, DbAccess.MarketDbContext context)
        {
            if (storeCarts == null || !storeCarts.Any())
            {
                Logger.writeEvent("OrderManager: ReserveItems| shopping cart is empty");
                throw new OrderNotValidException($"{nameof(DiscountAndReserve)}: shopping cart is null or empty");
            }
            Guid orderID = Guid.NewGuid();
            List<StoreOrder> storeOrders = new List<StoreOrder>();
            foreach (KeyValuePair<Guid, StoreCart> storeCart in storeCarts)
            {
                try 
                {
                    Guid storeOrderID = Guid.NewGuid();
                    IStoreInventoryManager store = StoreHandler.GetStoreInventoryManager(storeCart.Key, context);
                    StoreHandler.AquireLockOnStore(storeCart.Key);
                    List<Tuple<Item, int, double>> itemsWithPrices;
                    try
                    {
                        store.IsOrderItemsAmountAvailable(storeCart.Value, context);
                        itemsWithPrices = store.GetDiscountedPriceAndReserve(storeCart.Value, context);
                    }
                    finally
                    {
                        StoreHandler.ReleaseLockOnStore(storeCart.Key);    
                    }

                    List<OrderItem> itemList = new List<OrderItem>();
                    foreach (Tuple<Item, int, double> itemAmountPrice in itemsWithPrices)
                    {
                        OrderItem orderItem = new OrderItem(storeOrderID, itemAmountPrice.Item1, itemAmountPrice.Item2, itemAmountPrice.Item3);
                        itemList.Add(orderItem);
                    }
                    Logger.writeEvent(string.Format("OrderManager: ReserveItems| new Store Order was created for store: {0}", storeCart.Key));
                    StoreOrder storeOrder = new StoreOrder(storeOrderID, orderID, userID, storeCart.Key,
                                                            itemList, PurchaseType.IMMEDIATE, DateTime.Now);
                    storeOrders.Add(storeOrder);
                }
                catch (Exception e)
                {
                    foreach (StoreOrder storeOrder in storeOrders)// revert order
                    {
                        RevertStoreOrder(storeOrder, context);
                    }
                    throw e;
                }
            }
            Order order = new Order(orderID, userID, storeOrders, PurchaseType.IMMEDIATE, DateTime.Now);
            context.Orders.Add(order);
            context.SaveChanges();
            return order;
        }

        internal void ScheduleDelivery(Guid? userID, Guid orderID, string address, string city, string country, string zipCode,string name, MarketDbContext context)
        {
            if (!TryGetOrder(orderID, out Order order, context))
            {
                throw new OrderNotValidException(string.Format("No order Exists with this ID:", orderID));
            }
            if (userID != order.userKey)
            {
                throw new OrderNotValidException(string.Format("order:{0} was not made by user:{1}", orderID, userID));
            }
            if (order.Status != OrderStatus.PAYED_FOR)
            {
                throw new OrderNotValidException(string.Format("order:{0} was not pending", orderID));
            }
            try
            {
                _ = Delivery.Deliver(userID, order, address, city, country, zipCode, name).Result;

                foreach (StoreOrder storeOrder in order.StoreOrders)
                {
                    storeOrder.Status = OrderStatus.DELIVERED;
                }
                order.Status = OrderStatus.DELIVERED;
                context.SaveChanges();
            }
            catch (Exception e)
            {
                throw new DeliveryFailedException(e.InnerException.Message);
            }
        }

        public List<StoreOrder> GetStoreOrdersHistory(Guid storeID, MarketDbContext context)
        {
            List<StoreOrder> storeOrders = context.StoreOrders.Where(so => so.StoreId == storeID && so.Status == OrderStatus.COMPLETED).ToList();
            if (storeOrders.Count == 0)
            {
                Logger.writeEvent(string.Format("OrderManager: GetStoreOrdersHistory| no orders exist for store: {0}", storeID));
            }
            else
            {
                Logger.writeEvent(string.Format("OrderManager: GetStoreOrdersHistory| found order history for store: {0}", storeID));
            }
            return storeOrders;
        }

        internal bool CheckSufficentAmountInInventory(Guid storeID, Guid itemID, int amountToCheck, MarketDbContext context)
        {
            IStoreInventoryManager storeInventory = StoreHandler.GetStoreInventoryManager(storeID, context);
            return storeInventory.GetItemById(itemID).Amount >= amountToCheck;

        }
        private MarketDbContext getMarketDBContext()
        {
            if (useInMemoryDB)
            {
                return new MarketDbContext(this.inMemoryConnection);
            }
            else
            {
                return new MarketDbContext();
            }
        }

        private void ReviewPendings(object sender, EventArgs e)
        {
            using var context = getMarketDBContext();
            RemovePendings(context, DateTime.Now);
        }

        internal void RemovePendings(MarketDbContext context, DateTime now)
        {
            List<Order> pending = context.Orders.Where(o => o.Status == OrderStatus.PENDING).ToList();

            foreach (Order order in pending)
            {
                if (order.OrderTime.AddMinutes(2).CompareTo(now) < 0)
                {
                    RevertOrder(order, context);
                }
            }
        }

        private void RevertOrder(Order order, MarketDbContext context)
        {
            foreach (StoreOrder storeOrder in order.StoreOrders)// revert order
            {
                RevertStoreOrder(storeOrder, context);
            }
            context.Orders.Remove(order);
            context.SaveChanges();
        }

        private void RevertStoreOrder(StoreOrder storeOrder, MarketDbContext context)
        {
            IStoreInventoryManager store = StoreHandler.GetStoreInventoryManager(storeOrder.StoreId, context);
            StoreHandler.AquireLockOnStore(storeOrder.StoreId);
            try
            {
                store.ReturnItemsFromOrder(storeOrder.StoreOrderItems.ToList(), context);
            }
            finally
            {
                StoreHandler.ReleaseLockOnStore(storeOrder.StoreId);
            }
        }

        internal void CancelPayment(Guid? userID, Guid orderID, string cardNum, MarketDbContext context)
        {
            if (!TryGetOrder(orderID, out Order order, context))
            {
                throw new OrderNotValidException(string.Format("No order Exists with this ID:", orderID));
            }
            if (userID != order.userKey)
            {
                throw new OrderNotValidException(string.Format("order:{0} was not made by user:{1}", orderID, userID));
            }
            if (order.Status != OrderStatus.PAYED_FOR)
            {
                switch (order.Status)
                {
                    case OrderStatus.PENDING:
                        throw new OrderNotValidException(string.Format("order:{0} was not payed for", orderID));
                    case OrderStatus.DELIVERED:
                        throw new OrderNotValidException(string.Format("order:{0} was already delivered", orderID));
                    case OrderStatus.COMPLETED:
                        throw new OrderNotValidException(string.Format("order:{0} was completed", orderID));
                }
            }
            try
            {
                _ = Payment.Cancel(userID, order, cardNum).Result;
                context.Orders.Remove(order);
            }
            catch (Exception e)
            {
                throw new PaymentFailedException(e.Message);
            }
        }

        internal void ReturnProducts(Guid? userID, Guid orderID, MarketDbContext context)
        {
            if (!TryGetOrder(orderID, out Order order, context))
            {
                throw new OrderNotValidException(string.Format("No order Exists with this ID:", orderID));
            }
            if (userID != order.UserId)
            {
                throw new OrderNotValidException(string.Format("order:{0} was not made by user:{1}", orderID, userID));
            }
            RevertOrder(order, context);
        }

        public bool RegisterObserver(NotificationObserver notificationObserver)
        {
            return this.observers.Add(notificationObserver);
        }

        public Guid GetGuid()
        {
            return this.IDForNotifactionSubject;
        }

        public bool UnregisterObserver(NotificationObserver notificationObserver)
        {
            return this.observers.Remove(notificationObserver);
        }

        public void notifyEvent(INotificationEvent notificationEvent, MarketDbContext context)
        {
            foreach (NotificationObserver observer in observers)
            {
                observer.NotifyEvent(notificationEvent, context);
            }
        }
    }
}

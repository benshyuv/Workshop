using AcceptanceTests.OperationsAPI;
using System;
using System.Collections.Generic;
using System.Text;

namespace AcceptanceTests.DataObjects
{
    public class StoreOrder
    {
        public StoreOrder(Guid orderId, Guid sotreId, double sum, List<PurchaseRecord> records)
        {
            this.OrderId = orderId;
            StoreId = sotreId;
            Sum = sum;
            Records = records;
        }

        public StoreOrder(SystemObjJsonConveter.JsonStoreOrder jsonStoreOrder)
        {
            this.OrderId = jsonStoreOrder.OrderId;
            StoreId = jsonStoreOrder.StoreId;
            Sum = jsonStoreOrder.Total;
            Records = new List<PurchaseRecord>();
            SystemObjJsonConveter.ParseJsonStoreOrderItemsToPurchaseItemsList(Records, jsonStoreOrder);
        }

        public Guid OrderId { get; set; }
        public Guid StoreId { get; set; }
        public double Sum { get; set; }
        public List<PurchaseRecord> Records { get; set; }


    }
}

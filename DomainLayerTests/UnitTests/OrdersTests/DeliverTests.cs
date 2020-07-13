using DomainLayer.Orders;
using DomainLayer.Stores;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Net.Http;

namespace DomainLayerTests.UnitTests.OrdersTests
{
    public class DeliverTests
    {
        private IDelivery deliveryReal;
        private DeliveryProxy deliveryProxy;

        [SetUp]
        public void Setup()
        {
            deliveryReal = new DeliveryReal(new HttpClient());
            deliveryProxy = new DeliveryProxy();
        }

        [Test]
        public void IDelivey_RealDelivery_ShouldPassl()
        {
            Order order = new Order(Guid.NewGuid(), Guid.NewGuid(), new List<StoreOrder>(), PurchaseType.IMMEDIATE, DateTime.Now);
            string answer = deliveryProxy.Deliver(Guid.NewGuid(), order, "Ben Baruh 19", "Tel Aviv", "Israel", "9993456", "name").Result;
            Assert.IsTrue(answer == "");
        }

        /*[Test]
        public void IDelivey_RealDelivery_ShouldFail()
        {
            Order order = new Order(Guid.NewGuid(), Guid.NewGuid(), new List<StoreOrder>(), PurchaseType.IMMEDIATE, DateTime.Now);
            Assert.Throws<NotImplementedException>(() => deliveryReal.Deliver(Guid.NewGuid(), order, "", "", "","", ""));
        }
        */
        [Test]
        public void IDelivey_SetRealDelivery_ShouldPass()
        {
            deliveryProxy.Real = deliveryReal;
            Assert.NotNull(deliveryProxy.Real);
        }

        /* [Test]
         public void IDelivey_DeliveryToReal_ShouldFail()
         {
             deliveryProxy.Real = deliveryReal;
             Order order = new Order(Guid.NewGuid(), Guid.NewGuid(), new List<StoreOrder>(), PurchaseType.IMMEDIATE, DateTime.Now);
             Assert.Throws<NotImplementedException>(() => deliveryReal.Deliver(Guid.NewGuid(), order, "", "", "", "", ""));
         }*/

        [Test]
        public void IDelivey_DeliveryIsConnected_ShouldPass()
        {
            deliveryProxy.Real = deliveryReal;

            Assert.IsTrue(deliveryReal.IsConnected().Result);
        }


        /*[Test]
        public void IDelivey_DeliveryIsConnected_ShouldFail()
        {
            deliveryProxy.Real = deliveryReal;
            Assert.Throws<NotImplementedException>(() => deliveryReal.IsConnected());
        }*/
    }
}

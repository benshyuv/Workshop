using DomainLayer.Orders;
using DomainLayer.Stores;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;

namespace DomainLayerTests.UnitTests.OrdersTests
{
    public class PaymentTests
    {
        private IPayment paymentReal;
        private PaymentProxy paymentProxy;

        [SetUp]
        public void Setup()
        {
            paymentReal = new PaymentReal(new HttpClient());
            paymentProxy = new PaymentProxy();
        }

      /*  [Test]
        public void IPayment_RealPaymentShouldFail()
        {
            Order order = new Order(Guid.NewGuid(), Guid.NewGuid(), new List<StoreOrder>(), PurchaseType.IMMEDIATE, DateTime.Now);
            Assert.Throws<NotImplementedException>(() => paymentReal.Pay(Guid.NewGuid(), order, "", "", "","",""));
        }
      */
        [Test]
        public void IPayment_RealPaymentShouldsuccess()
        {
            Order order = new Order(Guid.NewGuid(), Guid.NewGuid(), new List<StoreOrder>(), PurchaseType.IMMEDIATE, DateTime.Now);
            _ = paymentProxy.Pay(Guid.NewGuid(), order, "1111222233334444", "4/2020", "444", "name", "333333333").Result;
            Assert.IsTrue(order.PaymentTransactionId != -1);
        }

        [Test]
        public void IPayment_SetRealPayment_ShouldPass()
        {
            //paymentProxy.Real = paymentReal;
            Assert.NotNull(paymentProxy.Real);
        }

      /*  [Test]
        public void IPayment_PaymentToReal_ShouldFail()
        {
            paymentProxy.Real = paymentReal;
            Order order = new Order(Guid.NewGuid(), Guid.NewGuid(), new List<StoreOrder>(), PurchaseType.IMMEDIATE, DateTime.Now);
<<<<<<< HEAD
            Assert.Throws<NotImplementedException>(() => paymentReal.Pay(Guid.NewGuid(), order, "", "", "","",""));
        }*/

        [Test]
        public void IPayment_PaymentIsConnected_ShouldPass()
        {
            //paymentProxy.Real = paymentReal;
            Assert.IsTrue(paymentReal.IsConnected().Result);
        }

      /*  [Test]
        public void IPayment_PaymentIsConnected_ShouldFail()
        {
            //paymentProxy.Real = paymentReal;
            Assert.IsTrue(paymentReal.IsConnected().Result);
        }*/

        [Test]
        public void IPayment_RealPaymentCancel()
        {
            Order order = new Order(Guid.NewGuid(), Guid.NewGuid(), new List<StoreOrder>(), PurchaseType.IMMEDIATE, DateTime.Now);
            _ = paymentProxy.Pay(Guid.NewGuid(), order, "1111222233334444", "4/2020", "444", "name", "333333333").Result;
            Assert.IsTrue(order.PaymentTransactionId != -1);

            Assert.IsTrue(paymentProxy.Cancel(Guid.NewGuid(), order, "1111222233334444").Result);
        }
    }
}

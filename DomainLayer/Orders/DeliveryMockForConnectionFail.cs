using DomainLayer.Exceptions;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace DomainLayer.Orders
{
    public class DeliveryMockForConnectionFail : IDelivery
    {

        private bool firstConnection;
        public DeliveryMockForConnectionFail()
        {
            this.firstConnection = true;
        }
        public async Task<bool> IsConnected()
        {
            if (firstConnection)
            {
                firstConnection = false;
                return true;
            }
            else
                return false;
        }

        public async Task<string> Deliver(Guid? userID, Order order, string address, string city, string country, string zipCode, string name)
        {
            return "";
        }

    }
}

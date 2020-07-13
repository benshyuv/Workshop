using DomainLayer.Exceptions;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace DomainLayer.Orders
{
    public class DeliveryMockForFail: IDelivery
    {
        public async Task<string> Deliver(Guid? userID, Order order, string address, string city, string country, string zipCode, string name)
        {
            throw new DeliveryFailedException("Failed: address/order problem");
        }

        public async Task<bool> IsConnected()
        {
            return true;
        }
    }
}

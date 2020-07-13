using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace DomainLayer.Orders
{
    public interface IDelivery
    {
        public Task<string> Deliver(Guid? userID, Order order, string address, string city, string country, string zipCode, string name);
        public Task<bool> IsConnected();
    }
}

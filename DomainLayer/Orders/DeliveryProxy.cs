using CustomLogger;
using DomainLayer.Exceptions;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace DomainLayer.Orders
{
    public class DeliveryProxy : IDelivery
    {
        public IDelivery Real { get; set; }

        public DeliveryProxy()
        {
            //Real = new DeliveryMockForFail();
            //Real = new DeliveryMockForConnectionFail();
            //Real = null;

            Real = new DeliveryReal(new HttpClient());
        }

        public async Task<string> Deliver(Guid? userID, Order order, string address, string city, string country, string zipCode, string name)
        {
            if (Real != null)
            {
                var connection = await Real.IsConnected().ConfigureAwait(false);
                if (connection)
                {
                    var deliver = await Real.Deliver(userID, order, address, city, country, zipCode, name);
                    return deliver;
                }
                else
                {
                    ConnectToExternalSystemsException exc = new ConnectToExternalSystemsException("No Connection To external payment system");
                    Logger.writeError(exc);
                    throw exc;
                }
            }
            return "";
        }

        public async Task<bool> IsConnected()
        {
            Logger.writeEvent("Delivery: Checking connection");
            return Real != null ? await Real.IsConnected().ConfigureAwait(false) : true;
        }
    }
}

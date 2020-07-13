using CustomLogger;
using DomainLayer.Exceptions;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace DomainLayer.Orders
{
    public class DeliveryReal : IDelivery
    {
        HttpClient DeliveryClient;

        public DeliveryReal(HttpClient deliveryClient)
        {
            DeliveryClient = deliveryClient;
        }
		
        public async Task<string> Deliver(Guid? userID, Order order, string address, string city, string country, string zipCode, string name)
        {
            var postContent = new Dictionary<string, string>
            {
                {"action_type", "supply" },
                {"name", name},
                {"address", address},
                {"city", city},
                {"country", country},
                {"zip", zipCode},
            };


            var url = "https://cs-bgu-wsep.herokuapp.com/";

            HttpResponseMessage result = null;

            try
            {
                result = await DeliveryClient.PostAsync(url, new FormUrlEncodedContent(postContent)).ConfigureAwait(false);
            }
            catch(Exception e)
            {
                throw new DeliveryFailedException("Failed: HTTP request failed " + e.Message);
            }

            if (result == null)
            {
                throw new DeliveryFailedException("Failed: HTTPResponseMessage in null");
            }

            if (!result.IsSuccessStatusCode)
            {

                throw new DeliveryFailedException("Failed: deliver request to external system faild with statusCode: " + result.StatusCode);
            }

            string returnValue = await result.Content.ReadAsStringAsync().ConfigureAwait(false);

            if (int.TryParse(returnValue, out int transactionId))
            {
                if (transactionId >= 10000 && transactionId <= 100000)
                {
                    order.DeliveryTransactionId = transactionId;
                    Logger.writeEvent("DeliveryReal: Deliver| succeeded for orderId: " + order.OrderId);
                    return "";
                }
                else
                {
                    if (transactionId == -1)
                    {
                        Logger.writeEvent("DeliveryReal: Deliver| failed for orderId: " + order.OrderId);
                        throw new DeliveryFailedException("Failed: delivery request to external system faild with transaction id -1");
                    }
                }
            }
            Logger.writeEvent("DeliveryReal: Deliver| failed for orderId: " + order.OrderId);
            throw new DeliveryFailedException("Failed: delivery request to external system faild with unknown reason. The return value: " + returnValue);
        }

        public async Task<bool> IsConnected()
        {
            var postContent = new Dictionary<string, string>
            {
                {"action_type", "handshake" },
            };

            var url = "https://cs-bgu-wsep.herokuapp.com/";

            HttpResponseMessage result = null;

            try
            {
                result = await DeliveryClient.PostAsync(url, new FormUrlEncodedContent(postContent)).ConfigureAwait(false);
            }
            catch(Exception e)
            {
                throw new DeliveryFailedException("Failed: HTTP request failed " + e.Message);
            }

            if (result == null)
            {
                throw new DeliveryFailedException("Failed: HTTPResponseMessage in null");
            }

            if (!result.IsSuccessStatusCode)
            {
                throw new DeliveryFailedException("Failed: handshake request to external system faild with statusCode: " + result.StatusCode);
            }

            string returnValue = await result.Content.ReadAsStringAsync().ConfigureAwait(false);

            if (returnValue.Equals("OK"))
            {
                Logger.writeEvent("DeliveryReal: IsConnected| succeeded");
                return true;
            }
            else
            {
                Logger.writeEvent("DeliveryReal: IsConnected| failed");
                return false;
            }
        }
    }
}

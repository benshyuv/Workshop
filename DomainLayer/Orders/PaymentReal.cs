using CustomLogger;
using DomainLayer.Exceptions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace DomainLayer.Orders
{
    public class PaymentReal : IPayment
    {
        HttpClient PaymentClient;

        public PaymentReal(HttpClient paymentClient)
        {
            PaymentClient = paymentClient;
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
                result = await PaymentClient.PostAsync(url, new FormUrlEncodedContent(postContent)).ConfigureAwait(false);
            }
            catch(Exception e)
            {
                throw new PaymentFailedException("Failed: HTTP request failed "+ e.Message);
            }

            if(result == null)
            {
                throw new PaymentFailedException("Failed: HTTPResponseMessage is null");
            }

            if (!result.IsSuccessStatusCode)
            {
                throw new PaymentFailedException("Failed: handshake request to external system faild with statusCode: " + result.StatusCode);
            }

            string returnValue = await result.Content.ReadAsStringAsync().ConfigureAwait(false);

            if (returnValue.Equals("OK"))
            {
                Logger.writeEvent("PaymentReal: IsConnected| succeeded");
                return true;
            }
            else
            {
                Logger.writeEvent("PaymentReal: IsConnected| failed");
                return false;
            }
        }

        public async Task<string> Pay(Guid? userID, Order order, string cardNum, string expire, string CCV, string cardOwnerName, string cardOwnerID)
        {
            string month = "";
            string year = "";

            if(expire.Length == 6)
            {
                month = expire.Substring(0, 1);
                year = expire.Substring(2);
            }

            else
            {
                if(expire.Length == 7)
                {
                    month = expire.Substring(0, 2);
                    year = expire.Substring(3);
                }
            }

            var postContent = new Dictionary<string, string>
            {
                {"action_type", "pay" },
                {"card_number", cardNum},
                {"month", month},
                {"year", year},
                {"holder", cardOwnerName},
                {"ccv", CCV},
                {"id", cardOwnerID}
            };

            var url = "https://cs-bgu-wsep.herokuapp.com/";

            HttpResponseMessage result = null;

            try
            {
                result = await PaymentClient.PostAsync(url, new FormUrlEncodedContent(postContent)).ConfigureAwait(false);
            }
            catch(Exception e)
            {
                throw new PaymentFailedException("Failed: HTTP request failed " + e.Message);
            }

            if (result == null)
            {
                throw new PaymentFailedException("Failed: HTTPResponseMessage is null");
            }

            if (!result.IsSuccessStatusCode)
            {

                throw new PaymentFailedException("Failed: payment request to external system faild with statusCode: " + result.StatusCode);
            }

            string returnValue = await result.Content.ReadAsStringAsync().ConfigureAwait(false);

            if(int.TryParse(returnValue, out int transactionId)){
                if(transactionId >= 10000 && transactionId <= 100000)
                {
                    order.PaymentTransactionId = transactionId;
                    Logger.writeEvent("PaymentReal: Pay| succeeded for orderId: "+ order.OrderId);
                    return "";
                }
                else
                {
                    if(transactionId == -1)
                    {
                        Logger.writeEvent("PaymentReal: Pay| failed for orderId: " + order.OrderId);
                        throw new PaymentFailedException("Failed: payment request to external system faild with transaction id -1");
                    }
                }
            }
            Logger.writeEvent("PaymentReal: Pay| failed for orderId: " + order.OrderId);
            throw new PaymentFailedException("Failed: payment request to external system faild with unknown reason. The return value: " + returnValue);

        }


        public async Task<bool> Cancel(Guid? userID, Order order, string card)
        {
            var postContent = new Dictionary<string, string>
            {
                {"action_type", "cancel_pay" },
                {"transaction_id", order.PaymentTransactionId.ToString()},
            };

            var url = "https://cs-bgu-wsep.herokuapp.com/";

            HttpResponseMessage result = null;

            try
            {
                result = await PaymentClient.PostAsync(url, new FormUrlEncodedContent(postContent)).ConfigureAwait(false);
            }
            catch(Exception e)
            {
                throw new PaymentFailedException("Failed: HTTP request failed " + e.Message);
            }

            if (result == null)
            {
                throw new PaymentFailedException("Failed: HTTPResponseMessage is null");
            }

            if (!result.IsSuccessStatusCode)
            {
                Logger.writeEvent("PaymentReal: Cancel| failed for orderId: " + order.OrderId);
                throw new PaymentFailedException("Failed: payment cancellation request to external system faild with statusCode: " + result.StatusCode);
            }

            string returnValue = await result.Content.ReadAsStringAsync().ConfigureAwait(false);

            if (returnValue.Equals("1"))
            {
                Logger.writeEvent("PaymentReal: Cancel| succeeded for orderId: " + order.OrderId);
                return true;
            }
            else
            {
                if (returnValue.Equals("-1"))
                {
                    Logger.writeEvent("PaymentReal: Cancel| failed for orderId: " + order.OrderId);
                    throw new PaymentFailedException("Failed: payment cancellation request to external system faild with returned value -1");

                }
            }
            Logger.writeEvent("PaymentReal: Cancel| failed for orderId: " + order.OrderId);
            throw new PaymentFailedException("Failed: payment cancellation request to external system faild with unknown reason. The return value: " + returnValue);
        }
    }
}

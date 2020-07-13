using CustomLogger;
using DomainLayer.Exceptions;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace DomainLayer.Orders
{
    public class PaymentProxy : IPayment
    {
        public IPayment Real{ get; set; }
        private HttpClient PaymentClient;
        public PaymentProxy()
        {
            //Real = new PaymentMockForFail();
            //Real = new PaymentMockForConnectionFailed();

            PaymentClient = new HttpClient();
            Real = new PaymentReal(PaymentClient);
        }

        public async Task<bool> IsConnected()
        {
            Logger.writeEvent("Payment: checking connection");

            return Real != null ? await Real.IsConnected().ConfigureAwait(false) : true;
        }

        public async Task<string> Pay(Guid? userID, Order order, string cardNum, string expire, string CCV, string cardOwnerName, string cardOwnerID)
        {
            if (Real != null)
            {
                var connection = await Real.IsConnected().ConfigureAwait(false);
                if (connection)
                {
                    var payment = await Real.Pay(userID, order, cardNum, expire, CCV, cardOwnerName, cardOwnerID).ConfigureAwait(false);
                    return payment;
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

        public async Task<bool> Cancel(Guid? userID, Order order, string card)
        {
            if (Real != null)
            {
                var connection = await Real.IsConnected().ConfigureAwait(false);
                if (connection)
                {
                    var cancellation = await Real.Cancel(userID, order, card).ConfigureAwait(false);
                    return cancellation;
                }
                else
                {
                    ConnectToExternalSystemsException exc = new ConnectToExternalSystemsException("No Connection To external payment system");
                    Logger.writeError(exc);
                    throw exc;
                }
            }

            return true;
        }
    }
}

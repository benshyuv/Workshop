using DomainLayer.Exceptions;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace DomainLayer.Orders
{
    class PaymentMockForConnectionFailed : IPayment
    {
        private bool firstConnection;
        public PaymentMockForConnectionFailed()
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

        public async Task<string> Pay(Guid? userID, Order order, string cardNum, string expire, string CCV, string cardOwnerName, string cardOwnerID) => "Success";

        public Task<bool> Cancel(Guid? userID, Order order, string card) => throw new NotImplementedException();
    }
}

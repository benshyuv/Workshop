using DomainLayer.Exceptions;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace DomainLayer.Orders
{
    class PaymentMockForFail : IPayment
    {

        public async Task<bool> IsConnected()
        {
            return true;
        }

        public async Task<string> Pay(Guid? userID, Order order, string cardNum, string expire, string CCV, string cardOwnerName, string cardOwnerID) => throw new PaymentFailedException("Failed: credit card");

        public async Task<bool> Cancel(Guid? userID, Order order, string card) => throw new NotImplementedException();
    }
}

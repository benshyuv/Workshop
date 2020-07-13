using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace DomainLayer.Orders
{
    public interface IPayment
    {
        public Task<string> Pay(Guid? userID, Order order, string cardNum, string expire, string CCV, string cardOwnerName,string cardOwnerID);

        public Task<bool> Cancel(Guid? userID, Order order, string card);

        public Task<bool> IsConnected();
    }
}

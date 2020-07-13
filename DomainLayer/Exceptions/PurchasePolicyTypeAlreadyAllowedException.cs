
using System;
using DomainLayer.Stores;

namespace DomainLayer.Exceptions
{
    public class PurchasePolicyTypeAlreadyAllowedException : Exception
    {
        public PurchasePolicyTypeAlreadyAllowedException(PurchasePolicyType type) :
            base(String.Format("Policy type {0} is already allowed", type.ToString()))
        {

        }
    }
}


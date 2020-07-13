using System;
using System.Collections.Generic;
using System.Text;

namespace DomainLayer.Exceptions
{
    class ConnectToExternalSystemsException : Exception
    {
        public ConnectToExternalSystemsException()
            : base()
        {
        }

        public ConnectToExternalSystemsException(string message)
            : base(message)
        {
        }
    }
}

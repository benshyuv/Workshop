using System;
using System.Runtime.Serialization;

namespace DomainLayer.Exceptions
{
    [Serializable]
    public class UserStateException : Exception
    {
		public UserStateException(string action, string context) : base(String.Format("Illegal action: {0} for user state {1}", action, context))
        {
        }

        protected UserStateException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}

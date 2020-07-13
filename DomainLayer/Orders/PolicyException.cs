using System;
using System.Runtime.Serialization;

namespace DomainLayer.Exceptions
{
	[Serializable]
	internal class PolicyException : Exception
	{
		public PolicyException(): base ("Purchase is not allowed according to store policy")
		{
		}

		public PolicyException(string message) : base(message)
		{
		}

		public PolicyException(string message, Exception innerException) : base(message, innerException)
		{
		}

		public PolicyException(string storeName, string message) : base(string.Format("{0} in store: {1}", message, storeName))
		{
		}

		protected PolicyException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}
	}
}
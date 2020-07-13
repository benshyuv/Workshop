using System;
using System.Runtime.Serialization;

namespace DomainLayer.Exceptions
{
	[Serializable]
	public class CartException : Exception
	{

		public CartException(string message) : base(message)
		{
		}

		protected CartException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}
	}
}
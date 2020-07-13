using System;
using System.Runtime.Serialization;

namespace DomainLayer.Exceptions
{
	[Serializable]
	internal class NonGrantorException : Exception
	{
		public NonGrantorException()
		{
		}

		public NonGrantorException(string message) : base(message)
		{
		}

		protected NonGrantorException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}
	}
}
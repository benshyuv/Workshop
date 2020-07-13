using System;
using System.Runtime.Serialization;

namespace DomainLayer.Exceptions
{
	[Serializable]
	internal class NotAnApproverException : Exception
	{
		public NotAnApproverException()
		{
		}

		public NotAnApproverException(string message) : base(message)
		{
		}

		protected NotAnApproverException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}
	}
}



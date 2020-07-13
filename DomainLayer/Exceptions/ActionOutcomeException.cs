using System;
using System.Runtime.Serialization;

namespace DomainLayer.Exceptions
{
	[Serializable]
	internal class ActionOutcomeException : Exception
	{
		public ActionOutcomeException()
		{
		}

		protected ActionOutcomeException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}
	}
}
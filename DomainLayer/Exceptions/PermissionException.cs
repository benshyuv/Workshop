using System;
using System.Runtime.Serialization;

namespace DomainLayer.Exceptions
{
	[Serializable]
	internal class PermissionException : Exception
	{
		public PermissionException(string message) : base(message)
		{
		}

		protected PermissionException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}
	}
}
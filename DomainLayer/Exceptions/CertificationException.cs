using System;
using System.Runtime.Serialization;

namespace DomainLayer.Exceptions
{
	[Serializable]
	internal class CertificationException : Exception
	{
		public CertificationException(string message) : base(message)
		{
		}

		protected CertificationException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}
	}
}
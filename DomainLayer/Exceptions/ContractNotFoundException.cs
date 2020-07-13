using System;
using System.Runtime.Serialization;
namespace DomainLayer.Exceptions
{
	[Serializable]
	internal class ContractNotFoundException : Exception
	{
		public ContractNotFoundException()
		{
		}

		public ContractNotFoundException(string message) : base(message)
		{
		}

		protected ContractNotFoundException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}
	}
}




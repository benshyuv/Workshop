using System;
using System.Runtime.Serialization;

namespace DomainLayer.Exceptions
{
	[Serializable]
	internal class ItemAmountException : Exception
	{
		public ItemAmountException(string message) : base(message)
		{
		}

		public ItemAmountException(Guid storeID, string message) : base(string.Format("{0} in store: {1}", message, storeID))
		{
		}

		public ItemAmountException(Guid itemId, int amountWanted, int amountExists) 
			: base(string.Format("Item: {0} has {1} in inventory while needed {2}",itemId, amountExists, amountWanted))
		{
		}

		protected ItemAmountException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}
	}
}
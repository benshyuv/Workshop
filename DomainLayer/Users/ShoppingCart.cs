using CustomLogger;
using DomainLayer.Exceptions;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace DomainLayer.Users
{
    public class ShoppingCart
	{
		[Key]
		public Guid UserID { get; set; }

        public ShoppingCart(Guid userID)
        {
            UserID = userID;
			storeCarts = new List<StoreCart>();
		}

        public ShoppingCart()
        {
        }

		internal Dictionary<Guid, StoreCart> StoreCarts 
		{ 
			get => storeCarts.ToDictionary(sc => sc.StoreID); 
		}

		public virtual ICollection<StoreCart> storeCarts { get; set; }

		public void AddToCart(Guid storeID, Guid itemID, int amount)
		{
			Logger.writeEvent(string.Format("ShoppingCart: AddToCart| trying to add {0} of item: {1} from store:{2}",
															amount, itemID, storeID));
			if (!StoreCarts.ContainsKey(storeID))
			{
				Logger.writeEvent(string.Format("ShoppingCart: AddToCart| creating new store cart for store: {0}", storeID)); 
				storeCarts.Add(new StoreCart(storeID));
			}
			StoreCarts[storeID].AddToStoreCart(itemID, amount);
		}

		public int ItemAmount(Guid storeID, Guid itemID)
		{
			if (!StoreCarts.ContainsKey(storeID))
			{
				throw new CartException(string.Format("Store Cart for store: {0} doesn't exist", storeID));
			}
			else
			{
				return StoreCarts[storeID].GetItemAmount(itemID);
			}
		}

		internal void SetItemAmount(Guid storeID, Guid itemID, int amount)
		{
			Logger.writeEvent(string.Format("ShoppingCart: SetItemAmount| trying to set amount of item: {0} from store: {1} to {2}",
															itemID, storeID, amount));
			if (!StoreCarts.ContainsKey(storeID))
			{
				Logger.writeEvent(string.Format("ShoppingCart: SetItemAmount| no cart for store {0} exists", storeID));
				throw new CartException(string.Format("Store Cart for store: {0} doesn't exist", storeID));
			}
			else
			{
				StoreCarts[storeID].SetItemAmount(itemID, amount);
			}
		}

		internal void RemoveFromCart(Guid storeID, Guid itemID)
		{
			Logger.writeEvent(string.Format("ShoppingCart: RemoveFromCart| trying to remove item: {0} from store:{1}",
															itemID, storeID));
			if (!StoreCarts.ContainsKey(storeID))
			{
				Logger.writeEvent(string.Format("ShoppingCart: RemoveFromCart| no cart for store {0} exists", storeID));
				throw new CartException(string.Format("Store Cart for store: {0} doesn't exist", storeID));
			}
			else
			{
				StoreCarts[storeID].RemoveFromCart(itemID);
                if (StoreCarts[storeID].HasNoItems())
                {
					storeCarts.Remove(StoreCarts[storeID]);
                }
			}
		}
	}

	public class StoreCart
	{
		public Guid UserID { get; set; }
		public Guid StoreID { get; set; }
		public Dictionary<Guid, int> Items 
		{ 
			get => itemAmounts.ToDictionary(ia => ia.ItemID, ia => ia.Amount); 
		}

		public virtual ICollection<ItemAmount> itemAmounts { get; set; }

		public StoreCart(Guid storeID)
		{
			StoreID = storeID;
			itemAmounts = new List<ItemAmount>();
		}

        public StoreCart()
        {
        }


        // when an existing item is attempted to be added, amount is updated
        public void AddToStoreCart(Guid itemID, int amount)
		{
			if (TryGetItemAmount(itemID, out _))
			{
				Logger.writeEvent(string.Format("StoreCart: AddToStoreCart| item: {0} of store: {1} already in StoreCart",
																itemID, StoreID));
				throw new CartException(string.Format("ItemID: {0} already added to cart", itemID));
			}
			itemAmounts.Add(new ItemAmount(itemID, StoreID, UserID, amount));
			Logger.writeEvent(string.Format("StoreCart: AddToStoreCart| added {0} of item: {1} to store cart of store: {2}",
																amount, itemID, StoreID));
		}

		public int GetItemAmount(Guid itemID)
		{
			if (!TryGetItemAmount(itemID, out ItemAmount itemAmount))
			{
				Logger.writeEvent(string.Format("StoreCart: GetItemAmount| item {0} doesn't exist in cart", itemID));
				throw new CartException(string.Format("No match for ItemID: {0}", itemID));
			}
			else
			{
				return itemAmount.Amount;
			}
		}

        private bool TryGetItemAmount(Guid itemID, out ItemAmount itemAmount)
        {
			itemAmount = itemAmounts.Where(ia => ia.ItemID == itemID).SingleOrDefault();
			return !(itemAmount is null);
		}

        public void SetItemAmount(Guid itemID, int amount)
		{
			if (!TryGetItemAmount(itemID, out ItemAmount itemAmount))
			{
				Logger.writeEvent(string.Format("StoreCart: SetItemAmount| item {0} doesn't exist in cart", itemID));
				throw new CartException(string.Format("No match for ItemID: {0}", itemID));
			}
			else
			{
				itemAmount.Amount = amount;
				Logger.writeEvent(string.Format("StoreCart: SetItemAmount| set amount of item: {0} from store: {1} to {2}",
																itemID, StoreID, amount));
			}
		}

		internal void RemoveFromCart(Guid itemID)
		{
			if (!TryGetItemAmount(itemID, out ItemAmount itemAmount))
			{
				Logger.writeEvent(string.Format("StoreCart: RemoveFromCart| item: {0} of store: {1} was not in cart",
																itemID, StoreID));
				throw new CartException(string.Format("No match for ItemID: {0} in cart", itemID));
			}
			itemAmounts.Remove(itemAmount);
			Logger.writeEvent(string.Format("StoreCart: RemoveFromCart| item: {0} of store: {1} has been removed from cart",
															itemID, StoreID));
		}

        internal bool HasNoItems()
        {
			return itemAmounts.Count == 0;
        }
    }

    public class ItemAmount
    {
        public ItemAmount()
        {
        }

        public ItemAmount(Guid itemID, Guid storeID, Guid userID, int amount)
        {
            ItemID = itemID;
            StoreID = storeID;
            UserID = userID;
            Amount = amount;
        }
		[Key]
        public Guid ItemID { get; set; }
		[Key]
		public Guid StoreID { get; set; }
		[Key]
		public Guid UserID { get; set; }
		public int Amount { get; set; }
    }
}

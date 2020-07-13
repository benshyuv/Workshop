using System;
using System.Collections.Generic;
using System.Linq;
using AcceptanceTests.OperationsAPI;
using AcceptanceTests.Utilitys;

namespace AcceptanceTests.DataObjects
{

       public class Store : IEquatable<Store>
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public StoreContactDetails storeContactDetails;
        public Dictionary<Item,int> ItemsWithAmount { get; set; }

        //generates store with 1 of each item
        public Store(Guid id, string name, List<Item> items)
        {
            Id = id;
            Name = name;
            ItemsWithAmount = new Dictionary<Item, int>();
            foreach (Item item in items)
            {
                ItemsWithAmount[item] = 1;
            }
              
        }
        // generates store with random id with 1 of each item
        public Store(string name, List<Item> items) : this(Guid.Empty,name,items)
        {

        }

        public Store(Guid id, string name, List<Tuple<Item, int>> itemsWithAmount)
        {
            Id = id;
            Name = name;
            foreach(Tuple<Item, int>  itemAmountTuple in itemsWithAmount)
            {
                ItemsWithAmount[itemAmountTuple.Item1] = itemAmountTuple.Item2;
            }
        }

        public Store( string name, List<Tuple<Item, int>> itemsWithAmount): this(Guid.Empty, name, itemsWithAmount) { }

        //ctor without details except item array and amounts, items lenght must match amount lenght
        public Store(Item[] items, int[] amounts):this(Guid.Empty, Utils.RandomAlphaNumericString(8),new List<Item>())
        { 
            for(int i = 0; i< items.Length; i++)
            {
                ItemsWithAmount[items[i]] = amounts[i];
            }
        }

        public Store(SystemObjJsonConveter.JsonStore jsonStoreNoItems)
        {
            this.Name = jsonStoreNoItems.ContactDetails.Name;
            this.Id = jsonStoreNoItems.Id;
            this.storeContactDetails = jsonStoreNoItems.ContactDetails;
            this.ItemsWithAmount = new Dictionary<Item, int>();
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as Store);
        }

        public bool Equals(Store other)
        {
            return other != null &&
                   Id.Equals(other.Id) &&
                   Name.ToLower().Trim() == other.Name.ToLower().Trim() &&
                   ItemsWithAmount.Intersect(other.ItemsWithAmount).Count().Equals(
                       ItemsWithAmount.Union(other.ItemsWithAmount).Count());
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Id, Name, ItemsWithAmount);
        }

        internal bool HasItemNotByGuid(Item itemWithoutGuid)
        {
            foreach(KeyValuePair<Item,int> itemWithAmount in ItemsWithAmount)
            {
                if (itemWithAmount.Key.isEqualWithoutGUID(itemWithoutGuid))
                {
                    return true;
                }
            }
            return false;
        }

        internal Item GetItemThatMatchesItemNotByGuid(Item itemWithoutGuid)
        {
            foreach (KeyValuePair<Item, int> itemWithAmount in ItemsWithAmount)
            {
                if (itemWithAmount.Key.isEqualWithoutGUID(itemWithoutGuid))
                {
                    return itemWithAmount.Key;
                }
            }
            return null;
        }

        internal Item GetItemThatMatchesByGuid(Guid itemGuid)
        {
            foreach (KeyValuePair<Item, int> itemWithAmount in ItemsWithAmount)
            {
                if (itemWithAmount.Key.Id.Equals(itemGuid))
                {
                    return itemWithAmount.Key;
                }
            }
            return null;
        }

        internal int GetAmountOfItemByGuid(Guid itemGuid)
        {
            foreach (KeyValuePair<Item, int> itemWithAmount in ItemsWithAmount)
            {
                if (itemWithAmount.Key.Id.Equals(itemGuid))
                {
                    return ItemsWithAmount[itemWithAmount.Key];
                }
            }
            return 0;
        }

        internal bool HasItemByGuid(Guid itemID)
        {
            foreach (KeyValuePair<Item, int> itemWithAmount in ItemsWithAmount)
            {
                if (itemWithAmount.Key.Id.Equals(itemID))
                {
                    return true;
                }
            }
            return false;
        }

    }
}

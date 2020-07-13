using CustomLogger;
using DomainLayer.DbAccess;
using DomainLayer.Exceptions;
using DomainLayer.Market;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text.Json.Serialization;

namespace DomainLayer.Stores.Inventory
{
    public class StoreInventory
    {
        [Key, ForeignKey("Store")]
        public Guid StoreId { get; set; }
        public virtual Store Store { get; set; }// not serialized on purpose
        public virtual ICollection<Item> StoreItems { get; set; }// not serialized on purpose

        public Dictionary<Guid, Item> Items 
        {
            get => StoreItems.ToDictionary(i => i.Id);
            set => StoreItems = value.Values;
        }

        public StoreInventory(Guid storeID)
        {
            StoreItems = new HashSet<Item>();
            StoreId = storeID;
        }

        public StoreInventory()
        {
            StoreItems = new HashSet<Item>();
        }

        public Item AddItem(
                    string name,
                    int amount,
                    HashSet<string> categoryNames,
                    double price,
                    MarketDbContext context,
                    HashSet<string> keywordNames = null)
        {
            Logger.writeEvent(string.Format("StoreInventory: AddItem| trying to add item with name \'{0}\'", name));
            if (ItemExistByName(name))
            {
                Logger.writeEvent(string.Format("StoreInventory: AddItem| item with name \'{0}\' already exists", name));
                throw new ItemAlreadyExistsException(name);
            }
            if (amount<0)
            {
                Logger.writeEvent(string.Format("StoreInventory: AddItem| \'{0}\' has illegal negative amount {1}", name, amount));
                throw new InvalidOperationOnItemException("AddItem: item must have an amount>=0");
            }
            if (categoryNames == null || categoryNames.Count < 1)
            {
                Logger.writeEvent(string.Format("StoreInventory: AddItem| can't add \'{0}\' without categories", name));
                throw new InvalidOperationOnItemException("AddItem: item must have at least one category");
            }

            Guid itemId = Guid.NewGuid();
            HashSet<Category> categories = ToCategories(categoryNames, context);
            HashSet<Keyword> keywords = ToKeywords(keywordNames, context);
            Item item = new Item(itemId, StoreId, name, amount, categories, price, keywords);
            StoreItems.Add(item);
            context.Items.Add(item);
            context.SaveChanges();
            Logger.writeEvent(string.Format("StoreInventory: AddItem| \'{0}\' added to inventory", name));
            return item;
        }

        private HashSet<Keyword> ToKeywords(ICollection<string> keywordNames, MarketDbContext context)
        {
            HashSet<Keyword> keywords = new HashSet<Keyword>();
            if (keywordNames is null)
            {
                return keywords;
            }
            foreach (string name in keywordNames)
            {
                Keyword keyword = ToKeyword(name, context);
                keywords.Add(keyword);
            }
            return keywords;
        }

        private static Keyword ToKeyword(string name, MarketDbContext context)
        {
            Keyword keyword = context.Keywords.Find(name);
            if (keyword is null)
            {
                keyword = new Keyword(name);
                context.Keywords.Add(keyword);
            }

            return keyword;
        }

        private HashSet<Category> ToCategories(ICollection<string> categoryNames, MarketDbContext context)
        {
            HashSet<Category> categories = new HashSet<Category>();
            foreach (string name in categoryNames)
            {
                Category category = ToCategory(name, context);
                categories.Add(category);
            }
            return categories;
        }

        private static Category ToCategory(string name, MarketDbContext context)
        {
            Category category = context.Categories.Find(name);
            if (category is null)
            {
                category = new Category(name);
                context.Categories.Add(category);
                context.SaveChanges();
            }

            return category;
        }

        public void DeleteItem(Guid itemId, MarketDbContext context)
        {
            IEnumerable<Item> toDelete = StoreItems.Where(i => i.Id == itemId);
            if (toDelete.Count() != 1)
            {
                Logger.writeEvent(string.Format("StoreInventory: DeleteItem| item {0} doesn't exist in inventory", itemId));
                throw new ItemNotFoundException(itemId);
            }
            Item delete = toDelete.Single();
            context.Items.Attach(delete);
            if (StoreItems.Remove(delete))
            {
                context.Items.Remove(delete);
                context.SaveChanges();
                Logger.writeEvent(string.Format("StoreInventory: DeleteItem| item: {0} was deleted from inventory", itemId));

            }
            else
            {
                Logger.writeEvent(string.Format("StoreInventory: DeleteItem| item: {0} could not be deleted", itemId));
            }
        }

        //Method assumes details are from correct types
        public Item EditItem(Guid itemId, Dictionary<StoresUtils.ItemEditDetails, object> detailsToEdit, MarketDbContext context)
        {
            Logger.writeEvent(string.Format("StoreInventory: EditItem| trying to add item: {0}", itemId));
            Item item;
            try
            {
                item = StoreItems.Where(i => i.Id == itemId).Single();
            }
            catch
            {
                Logger.writeEvent(string.Format("StoreInventory: EditItem| item: {0} doesn't exist in inventory", itemId));
                throw new ItemNotFoundException(itemId);
            }
            context.Items.Attach(item);
            context.Entry(item).Collection("Keywords").Load();
            context.Entry(item).Collection("Categories").Load();
            Item updated = new Item(item);// clone
            context.Entry(item).State = System.Data.Entity.EntityState.Detached;
            context.Items.Attach(updated);
            object value;
            foreach(KeyValuePair<StoresUtils.ItemEditDetails, object> entry in detailsToEdit)
            {
                value = entry.Value;
                if(value == null)
                {
                    continue;
                }

                switch (entry.Key)
                {
                    case StoresUtils.ItemEditDetails.name:
                        if(!updated.Name.Equals(value))
                        {
                            if (ItemExistByName((string)value))
                            {
                                Logger.writeEvent(string.Format("StoreInventory: EditItem| item with name \'{0}\' already exists",
                                                                                (string)value));
                                context.Entry(updated).State = System.Data.Entity.EntityState.Detached;
                                context.Items.Attach(item);
                                throw new ItemAlreadyExistsException((string)value);
                            }
                            updated.Name = (string)value;
                            Logger.writeEvent(string.Format("StoreInventory: EditItem| updated item: {0} name to \'{1}\'", 
                                                                            itemId, (string)value));
                        }
                        break;
                    case StoresUtils.ItemEditDetails.amount:
                        if ((int)value < 0)
                        {
                            Logger.writeEvent(string.Format("StoreInventory: EditItem| item: {0} amount can't be negative: {1}",
                                                                            itemId, (int)value));
                            context.Entry(updated).State = System.Data.Entity.EntityState.Detached;
                            context.Items.Attach(item); 
                            throw new InvalidOperationOnItemException("EditItem: item must have an amount >= 0");
                        }
                        updated.Amount = (int)value;
                        Logger.writeEvent(string.Format("StoreInventory: EditItem| updated item: {0} amount to {1}", 
                                                                        itemId, (int)value));
                        break;
                    case StoresUtils.ItemEditDetails.categories:
                        if (((List<string>)value).Count < 1)
                        {
                            Logger.writeEvent(string.Format("StoreInventory: EditItem| item: {0} must have at least one category",
                                                                            itemId));
                            context.Entry(updated).State = System.Data.Entity.EntityState.Detached;
                            context.Items.Attach(item);
                            throw new InvalidOperationOnItemException("EditItem: item must have at least one category");
                        }
                        updated.Categories = ToCategories((List<string>)value, context);
                        Logger.writeEvent(string.Format("StoreInventory: EditItem| updated item: {0} categories", itemId));
                        break;
                    case StoresUtils.ItemEditDetails.rank:
                        if ((double)value < 0 || (double)value > 10)
                        {
                            Logger.writeEvent(string.Format("StoreInventory: EditItem| item: {0} rank must be in range 0-10: {1}",
                                                                            itemId, (double)value));
                            context.Entry(updated).State = System.Data.Entity.EntityState.Detached;
                            context.Items.Attach(item);
                            throw new InvalidOperationOnItemException("EditItem: item must have a rank between 0-10");
                        }
                        updated.Rank = (double)value;
                        Logger.writeEvent(string.Format("StoreInventory: EditItem| updated item: {0} rank to {1}", 
                                                                        itemId, (double)value));
                        break;
                    case StoresUtils.ItemEditDetails.price:
                        if ((double)value <= 0)
                        {
                            Logger.writeEvent(string.Format("StoreInventory: EditItem| item: {0} amount can't be negative: {1}",
                                                                            itemId, (double)value));
                            context.Entry(updated).State = System.Data.Entity.EntityState.Detached;
                            context.Items.Attach(item);
                            throw new InvalidOperationOnItemException("EditItem: item must have a price >= 0");
                        }
                        updated.Price = (double)value;
                        Logger.writeEvent(string.Format("StoreInventory: EditItem| updated item: {0} price to {1}", 
                                                                        itemId, (double)value));
                        break;
                    case StoresUtils.ItemEditDetails.keyWords:
                        updated.Keywords = ToKeywords((List<string>)value, context);
                        Logger.writeEvent(string.Format("StoreInventory: EditItem| updated item: {0} keywords", itemId));
                        break;
                    default:
                        break;
                }
            }
            Logger.writeEvent(string.Format("StoreInventory: EditItem| updated item: {0}", itemId));

            StoreItems.Remove(item);// TODO: attach in db?
            StoreItems.Add(updated);
            context.SaveChanges();
            //return Items.Where(i => i.Id == itemId).Single();
            return updated;
        }

        public bool ReduceItemAmount(Guid itemId, int amountToReduce, MarketDbContext context)
        {
            Logger.writeEvent(string.Format("StoreInventory: ReduceItemAmount| trying to add item: {0}", itemId));
            Item item;
            try
            {
                item = StoreItems.Where(i => i.Id == itemId).Single();
            }
            catch
            {
                Logger.writeEvent(string.Format("StoreInventory: ReduceItemAmount| item: {0} doesn't exist in inventory", itemId));
                throw new ItemNotFoundException(itemId);
            }

            if (amountToReduce < 0)
            {
                Logger.writeEvent(string.Format("StoreInventory: ReduceItemAmount| amount of item {0} to reduce can't be negative: {1}",
                                                                itemId, amountToReduce));
                throw new InvalidOperationOnItemException(string.Format("ReduceItemAmount: amount to reduce is not positive value", itemId));
            }

            int updatedAmount = item.Amount - amountToReduce;
            if (updatedAmount < 0)
            {
                Logger.writeEvent(string.Format("StoreInventory: ReduceItemAmount| remaining amount for item: {0} can't be negative: {1}",
                                                                itemId, updatedAmount));
                throw new InvalidOperationOnItemException(string.Format("ReduceItemAmount: Item {0} cannot have non positive amount", itemId));
            }
            else
            {
                item.Amount = updatedAmount;
                context.SaveChanges();
                Logger.writeEvent(string.Format("StoreInventory: ReduceItemAmount| updated item: {0} amount to {1}",
                                                                itemId, updatedAmount));
                return true;
            }
        }

        public bool IncreaseItemAmount(Guid itemId, int amountToIncrease)
        {
            Logger.writeEvent(string.Format("StoreInventory: IncreaseItemAmount| trying to add item: {0}", itemId));
            Item item;
            try
            {
                item = StoreItems.Where(i => i.Id == itemId).Single();
            }
            catch
            {
                Logger.writeEvent(string.Format("StoreInventory: IncreaseItemAmount| item: {0} doesn't exist in inventory", itemId));
                throw new ItemNotFoundException(itemId);
            }
            
            if (amountToIncrease < 0)
            {
                Logger.writeEvent(string.Format("StoreInventory: IncreaseItemAmount| amount of item {0} to reduce can't be negative: {1}",
                                                                itemId, amountToIncrease));
                throw new InvalidOperationOnItemException(string.Format("IncreaseItemAmount: amount to reduce is not positive value", itemId));
            }

            item.Amount += amountToIncrease;
            Logger.writeEvent(string.Format("StoreInventory: ReduceItemAmount| updated item: {0} amount to {1}",
                                                            itemId, item.Amount));
            return true;
        }

        public bool AddCategoryItem(Guid itemId, string categoryName, MarketDbContext context)
        {
            Item item;
            try
            {
                item = StoreItems.Where(i => i.Id == itemId).Single();
            }
            catch
            {
                Logger.writeEvent(string.Format("StoreInventory: AddCategoryItem| item: {0} doesn't exist in inventory", itemId));
                throw new ItemNotFoundException(itemId);
            }
            context.Entry(item).Collection("Categories").Load();
            Category category = ToCategory(categoryName, context);
            if (!item.Categories.Contains(category))
            {
                item.Categories.Add(category);
                context.SaveChanges();
            }
            return true;
        }

        public bool UpdateCategoryItem(Guid itemId, string originalCategoryName, string updatedCategoryName, 
                                        MarketDbContext context)
        {
            Item item;
            try
            {
                item = StoreItems.Where(i => i.Id == itemId).Single();
            }
            catch
            {
                Logger.writeEvent(string.Format("StoreInventory: UpdateCategoryItem| item: {0} doesn't exist in inventory", itemId));
                throw new ItemNotFoundException(itemId);
            }
            context.Entry(item).Collection("Categories").Load();
            Category originalCategory = ToCategory(originalCategoryName, context);
            if (item.Categories.Contains(originalCategory))
            {
                item.Categories.Remove(originalCategory);
                Category updatedCategory = ToCategory(updatedCategoryName, context);
                item.Categories.Add(updatedCategory); //hashet will prevent duplicates

                context.SaveChanges();
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool DeleteCategoryItem(Guid itemId, string categoryName, MarketDbContext context)
        {
            Item item;
            try
            {
                item = StoreItems.Where(i => i.Id == itemId).Single();
            }
            catch
            {
                Logger.writeEvent(string.Format("StoreInventory: DeleteCategoryItem| item: {0} doesn't exist in inventory", itemId));
                throw new ItemNotFoundException(itemId);
            }
            context.Entry(item).Collection("Categories").Load();
            Category category = ToCategory(categoryName, context); 
            bool categoryExist = item.Categories.Contains(category);
            if (categoryExist && item.Categories.Count > 1)
            {
                item.Categories.Remove(category);
                context.SaveChanges();
                return true;
            }
            else
            {
                if (!categoryExist)
                {
                    return true; //category doesn't exist, nothing to delete
                }

                throw new InvalidOperationOnItemException("Cannot delete the only category of item with id:" + itemId);
                //TODO: add log
            }
        }

        public bool AddKeyWordItem(Guid itemId, string word, MarketDbContext context)
        {
            Item item;
            try
            {
                item = StoreItems.Where(i => i.Id == itemId).Single();
            }
            catch
            {
                Logger.writeEvent(string.Format("StoreInventory: AddKeyWordItem| item: {0} doesn't exist in inventory", itemId));
                throw new ItemNotFoundException(itemId);
            }
            context.Entry(item).Collection("Keywords").Load();
            Keyword keyword = ToKeyword(word, context);
            if (!item.Keywords.Contains(keyword))
            {
                item.Keywords.Add(keyword);
                context.SaveChanges();
            }

            return true;
        }

        public bool UpdateKeyWordItem(Guid itemId, string originalWord, string updatedWord, MarketDbContext context)
        {
            Item item;
            try
            {
                item = StoreItems.Where(i => i.Id == itemId).Single();
            }
            catch
            {
                Logger.writeEvent(string.Format("StoreInventory: UpdateKeyWordItem| item: {0} doesn't exist in inventory", itemId));
                throw new ItemNotFoundException(itemId);
            }
            context.Entry(item).Collection("Keywords").Load();
            Keyword originalKeyword = ToKeyword(originalWord, context);
            if (item.Keywords.Contains(originalKeyword))
            {
                item.Keywords.Remove(originalKeyword);
                Keyword updatedKeyword = ToKeyword(updatedWord, context);
                item.Keywords.Add(updatedKeyword);

                context.SaveChanges();
                return true;
            }
            else
            {
                return false;
            }

        }

        public bool DeleteKeyWordItem(Guid itemId, string word, MarketDbContext context)
        {
            Item item;
            try
            {
                item = StoreItems.Where(i => i.Id == itemId).Single();
            }
            catch
            {
                Logger.writeEvent(string.Format("StoreInventory: DeleteKeyWordItem| item: {0} doesn't exist in inventory", itemId));
                throw new ItemNotFoundException(itemId);
            }
            context.Entry(item).Collection("Keywords").Load();
            Keyword keyword = ToKeyword(word, context);
            if (item.Keywords.Contains(keyword))
            {
                item.Keywords.Remove(keyword);
                context.SaveChanges();
            }
            return true;
        }

        public ReadOnlyCollection<Item> SearchItems(MarketDbContext context, string name = null, string category = null,
                                                    List<string> keywords = null, List<SearchFilter> itemFilters = null)
        {

            if (StoreItems.Count == 0)
            {
                Logger.writeEvent("StoreInventory: searching empty store");
                return StoreItems.ToList().AsReadOnly();
            }
            List<Item> searchItems = StoreItems.ToList();
            if (searchItems.Count != 0 && name != null)
            {
                Logger.writeEvent(string.Format("StoreInventory: searching by item name- {0}", name));
                searchItems = SearchByName(name, searchItems);
            }

            if(searchItems.Count != 0 && category != null)
            {
                Logger.writeEvent(string.Format("StoreInventory: searching by category- {0}", category));
                searchItems = SearchByCategory(category, searchItems, context);
            }

            if(searchItems.Count != 0 && keywords != null && keywords.Count != 0)
            {
                Logger.writeEvent("StoreInventory: searching by keywords");
                searchItems = SearchByKeyWords(keywords, searchItems, context);
            }

            if(searchItems.Count != 0 && itemFilters != null)
            {
                List<Item> filteredItems = new List<Item>() { };
                foreach (Item item in searchItems)
                {
                    Logger.writeEvent("StoreInventory: filtering results");
                    if (CheckItemForFilters(item, itemFilters))
                    {
                        filteredItems.Add(item);
                    }
                }
                Logger.writeEvent(string.Format("StoreInventory: found {0} matches", filteredItems.Count));
                return filteredItems.AsReadOnly();
            }
            else
            {
                Logger.writeEvent(string.Format("StoreInventory: found {0} matches", searchItems.Count));
                return searchItems.AsReadOnly();
            }

            
        }

        //Search functionality
        private List<Item> SearchByName(string name, List<Item> items)
        {
            List<Item> searchItems = new List<Item>() { };

            foreach (Item item in items)
            {
                if (item.Name.Equals(name))
                {
                    searchItems.Add(item);
                }
            }

            return searchItems;
        }

        private List<Item> SearchByCategory(string categoryName, List<Item> items, MarketDbContext context)
        {
            List<Item> searchItems = new List<Item>() { };
            Category category = ToCategory(categoryName, context);
            foreach (Item item in items)
            {
                if (item.Categories.Contains(category))
                {
                    searchItems.Add(item);
                }
            }

            return searchItems;
        }

        private List<Item> SearchByKeyWords(List<string> keywords, List<Item> items, MarketDbContext context)
        {
            List<Item> searchItems = new List<Item>() { };

            foreach (Item item in items)
            {
                if (ItemIncludesKeyWords(item, keywords, context))
                {
                    searchItems.Add(item);
                }
            }

            return searchItems;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="itemId"></param>
        /// <returns>true if exists, otherwise false</returns>
        public bool ItemExistById(Guid itemId)
        {
            foreach(Item item in StoreItems)
            {
                if (item.Id == itemId)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <returns>true if exists, otherwise false</returns>
        private bool ItemExistByName(string name)
        {
            foreach (Item item in StoreItems)
            {
                if (item.Name.ToLower().Equals(name.ToLower()))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="itemId"></param>
        /// <returns>Item if found, otherwise throws ItemNotFoundException</returns>
        public Item GetItemById(Guid itemId)
        {
            Logger.writeEvent(string.Format("StoreInventory: fetching item: {0}", itemId));
            try
            {
                return StoreItems.Where(i => i.Id == itemId).Single();
            }
            catch
            {
                Logger.writeEvent(string.Format("StoreInventory: GetItemById| item: {0} doesn't exist in inventory", itemId));
                throw new ItemNotFoundException(itemId);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <returns>Item if found, otherwise throws ItemNotFoundException</returns>
        public Item GetItemByName(string name)
        {
            Logger.writeEvent(string.Format("StoreInventory: fetching item: {0}", name));
            try
            {
                return StoreItems.Where(i => i.Name == name).Single();
            }
            catch
            {
                Logger.writeEvent(string.Format("StoreInventory: GetItemByName| item \'{0}\' doesn't exist in inventory", name));
                throw new ItemNotFoundException(name);
            }
        }

        /// <summary>
        /// Checks if it is available to reduce "amount" from item.Amount
        /// Leading rule : in each moment must be a non zero quantity of item
        /// If item not found - ItemNotFoundException is thrown
        /// </summary>
        /// <param name="itemId"></param>
        /// <param name="amount"></param>
        /// <returns>true if available, otherwise false</returns>
        public void IsItemAmountAvailableForPurchase(Guid itemId, int amount)
        {

            Item item;
            try
            {
                item = StoreItems.Where(i => i.Id == itemId).Single();
            }
            catch
            {
                Logger.writeEvent(string.Format("StoreInventory: IsItemAmountAvailableForPurchase| item: {0} doesn't exist in inventory", itemId));
                throw new ItemNotFoundException(itemId);
            }

            if (amount < 0)//TODO: dead code?
            {
                throw new ArgumentOutOfRangeException("amount is lower than 0");
            }

            if(item.Amount < amount)
            {
                throw new ItemAmountException(itemId, amount, item.Amount);
            }
        }

        //Getters

        public ReadOnlyCollection<Item> GetStoreItems()
        {
            return StoreItems.ToList().AsReadOnly();
        }

        private bool CheckItemForFilters(Item item, List<SearchFilter> itemFilters)
        {
            foreach (SearchFilter sf in itemFilters)
            {
                if (!sf.DoesItemStandInFilter(item))
                {
                    return false;
                }
            }

            return true;
        }

/*        private bool ItemInAtLeastOneCategory(Item item, List<string> categories)
        {
            IEnumerable<string> intersect = item.Categories.Intersect(categories, StringComparer.Ordinal);
            return (intersect.Count() > 0) ? true : false;
        }*/

        private bool ItemIncludesKeyWords(Item item, List<string> keywords, MarketDbContext context)
        {
            foreach (string word in keywords)
            {
                Keyword keyword = ToKeyword(word,context);
                if (!item.Keywords.Contains(keyword))
                {
                    return false;
                }
            }

            return true;
        }

        //public virtual bool ShouldSerializeStoreItems()
        //{
        //    return false;
        //}

        public virtual bool ShouldSerializeStore()
        {
            return false;
        }
    }
}

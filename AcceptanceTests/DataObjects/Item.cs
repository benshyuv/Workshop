using System;
using System.Collections.Generic;
using System.Linq;
using AcceptanceTests.OperationsAPI;

namespace AcceptanceTests.DataObjects
{
    public class Item : IEquatable<Item>
    {

        private Guid id;
        public List<string> Keywords { get; set; }
        public List<string> Categorys { get; set; }
        public Guid Id { get => id; set => id = value; }
        public string Name { get; set; }
        public double Price { get; set; }
        public double Rank { get; set; }
        public Guid storeID { get; set; }

        public Item(Guid id, string name, List<string> keywords, List<string> categorys, double price)
        {

            this.id = id;
            Name = name;
            Keywords = keywords;
            Categorys = categorys;
            Rank = 0;
            this.Price = price;
        }

        public Item(Guid id, string name, List<string> keywords, List<string> categorys) :
            this(id, name, keywords, categorys, 5)
        {

        
        }
        public Item( string name, List<string> keywords, List<string> categorys, double price) :
            this(Guid.NewGuid(), name, keywords, categorys, price)
        {
        }

        public Item(string json)
        {
            throw new NotImplementedException();
        }

        //adds random guid just for equality purpose
        //should not be checked for equality with real market items.
        public Item(string name, List<string> keywords, List<string> categorys): this(Guid.NewGuid(),name,keywords,categorys)
        {
        }

        public Item(SystemObjJsonConveter.JsonItem jsonItem)
        {
            this.id = jsonItem.Id;
            Name = jsonItem.Name;
            Keywords = new List<string>(jsonItem.KeyWords);
            Categorys = new List<string>(jsonItem.Categories);
            Rank = jsonItem.Rank;
            this.Price = jsonItem.Price;
            this.storeID = jsonItem.StoreId;
        }

        public Item(SystemObjJsonConveter.JsonStoreOrderItem jsonItem)
        {
            this.id = jsonItem.ItemId;
            Name = jsonItem.Name;
            Keywords = new List<string>(jsonItem.KeyWords);
            Categorys = new List<string>(jsonItem.Categories);
            Rank = jsonItem.Rank;
            this.Price = jsonItem.BasePricePerItem;
        }
        private List<string> listFromHashset(HashSet<string> set)
        {
            List<string> res = new List<string>();
            foreach(String s in set)
            {
                res.Add(s);
            }
            return res;
        }

        public bool isEqualWithoutGUID(Item other)
        {
            return (Keywords.Count == other.Keywords.Count) &&
                   !Keywords.ConvertAll(s => s.ToLower().Trim()).
                   Except(other.Keywords.ConvertAll(s => s.ToLower().Trim())).Any() &&
                   (Categorys.Count == other.Categorys.Count) &&
                   !Categorys.ConvertAll(s => s.ToLower().Trim()).
                   Except(other.Categorys.ConvertAll(s => s.ToLower().Trim())).Any() &&
                   Price.Equals(other.Price) &&
                   Name.ToLower().Trim().Equals(other.Name.ToLower().Trim());
        }
        public override bool Equals(object obj)
        {

            return Equals(obj as Item);
            
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(id);
        }

        public bool Equals( Item item)
        {
            return item != null &&
                   id.Equals(item.id);
        }

    }
}

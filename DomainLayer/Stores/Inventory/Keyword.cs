using DomainLayer.DbAccess;
using DomainLayer.Market;
using DomainLayer.Orders;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DomainLayer.Stores.Inventory
{
    [JsonConverter(typeof(JsonKeywordCustomConverter))]
    public class Keyword
    {
        public Keyword(string name)
        {
            Name = name;
            Items = new HashSet<Item>();
            OrderItems = new HashSet<OrderItem>();
        }

        public Keyword() { }

        [Key]
        public string Name { get; set; }

        public virtual ICollection<Item> Items { get; set; }
        public virtual ICollection<OrderItem> OrderItems { get; set; }

        public override bool Equals(object obj) => obj is Keyword keyword && Name == keyword.Name;
        public override int GetHashCode() => HashCode.Combine(Name);
    }
}

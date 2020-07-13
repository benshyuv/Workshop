using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DomainLayer.Stores.Inventory
{
    public class Item
    {
        public Guid Id { get; set; }

        [ForeignKey("StoreInventory")]
        public Guid StoreId { get; set; }

        [JsonIgnore]
        public virtual StoreInventory StoreInventory { get; set; }

        [Required]
        //[Index(IsUnique = true)]
        [StringLength(128)]
        public string Name { get; set; }
        public int Amount { get; set; }
        public virtual ICollection<Category> Categories { get; set; }
        public double Rank { get; set; }
        public double Price { get; set; }
        public virtual ICollection<Keyword> Keywords { get; set; }

        [JsonConstructor]
        public Item(Guid id,
                    Guid storeID,
                    string name,
                    int amount,
                    HashSet<Category> categories,
                    double price,
                    HashSet<Keyword> keyWords = null)
        {
            Id = id;
            StoreId = storeID;
            Name = name;
            Amount = amount;
            Categories = categories;
            Rank = 0;
            Price = price;
            if(keyWords == null)
            {
                Keywords = new HashSet<Keyword>();
            }
            else
            {
                Keywords = keyWords;
            }
        }

		public Item(Item original)
		{
            Id = original.Id;
            StoreId = original.StoreId;
            Name = original.Name;
            Amount = original.Amount;
            Categories = new HashSet<Category>(original.Categories);
            Rank = original.Rank;
            Price = original.Price;
            Keywords = new HashSet<Keyword>(original.Keywords);
        }

        public Item()
        {
        }

        public override bool Equals(object obj)
        {
            return obj == null ? false : obj is Item objAsItem ? Equals(objAsItem) : false;
        }

        /*  public override int GetHashCode()
          {
              return id;
          }*/

        public bool Equals(Item item)
        {
            return Id.Equals(item.Id) && StoreId.Equals(item.StoreId);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Id, StoreId);
        }
    }
}

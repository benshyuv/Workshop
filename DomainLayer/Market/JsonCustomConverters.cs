using DomainLayer.Stores.Inventory;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DomainLayer.Market
{
    internal class JsonCategoryCustomConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType) => objectType == typeof(Category);

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            string name = Convert.ToString(reader.Value);
            return new Category(name);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            Category category = (Category)value;
            writer.WriteValue(category.Name);
        }
    }

    internal class JsonKeywordCustomConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType) => objectType == typeof(Keyword);

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            string name = Convert.ToString(reader.Value);
            return new Keyword(name);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            Keyword keyword = (Keyword)value;
            writer.WriteValue(keyword.Name);
        }
    }

    //internal class JsonStoreInventoryCustomConverter : JsonConverter
    //{
    //    public override bool CanConvert(Type objectType) => objectType == typeof(StoreInventory);

    //    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    //    {
    //        JObject oValue = (JObject)reader.Value;
    //        JArray jItems = (JArray)oValue["Items"];
    //        ICollection<Item> items = jItems.ToObject<List<Item>>();
    //        Dictionary<Guid, Item> itemDict = items.ToDictionary(i => i.Id, i => i);
    //        oValue.Remove("Items");
    //        oValue.Add("Items", JToken.FromObject(itemDict));
    //        string name = Convert.ToString(reader.Value);
    //        return new Keyword(name);
    //    }

    //    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    //    {
    //        ICollection<Item> items = ((StoreInventory)value).Items;
    //        JObject oValue = JObject.FromObject(value);
    //        oValue.Remove("Items");
    //        oValue.Add("Items", new JArray(items));
    //        oValue.WriteTo(writer);
    //    }
    //}
}
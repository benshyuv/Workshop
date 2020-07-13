using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using AcceptanceTests.DataObjects;
using Newtonsoft.Json;

namespace AcceptanceTests.OperationsAPI
{
    public class SystemObjJsonConveter
    {
        public static List<PurchaseRecord> purchaseRecordsFromStoreHistory(string storeHistJson)
        {
            ResponseClass response = JsonConvert.DeserializeObject<ResponseClass>(storeHistJson);
            if (response.success)
            {
                List<JsonStoreOrder> storeOrders = JsonConvert.DeserializeObject<List<JsonStoreOrder>>(response.answerOrExceptionJson);
                List<PurchaseRecord> retList = new List<PurchaseRecord>();
                foreach (JsonStoreOrder jsonStoreOrder in storeOrders)
                {
                    ParseJsonStoreOrderItemsToPurchaseItemsList(retList, jsonStoreOrder);
                }
                return retList;
            }
            return null;

        }

        public static void ParseJsonStoreOrderItemsToPurchaseItemsList(List<PurchaseRecord> retList, JsonStoreOrder jsonStoreOrder)
        {
            foreach (JsonStoreOrderItem jsonItem in jsonStoreOrder.StoreOrderItems)
            {
                Item item = new Item(jsonItem);
                PurchaseRecord purchase = new PurchaseRecord(jsonStoreOrder.OrderId, item.Name, item.Id, item.Price, jsonItem.Amount);
                purchase.DiscountedPriceOnPurchase = jsonItem.DiscountedPricePerItem;
                item.storeID = jsonStoreOrder.StoreId;
                purchase.ItemObj = item;
                purchase.storeID = jsonStoreOrder.StoreId;
                retList.Add(purchase);
            }
        }

        internal static List<PurchaseRecord> purchaseRecordsFromUserHistory(string historyJson)
        {

            ResponseClass response = JsonConvert.DeserializeObject<ResponseClass>(historyJson);
            if (response.success)
            {
                List<JsonOrder> jsonOrders = JsonConvert.DeserializeObject<List<JsonOrder>>(response.answerOrExceptionJson);
                List<PurchaseRecord> retList = new List<PurchaseRecord>();
                foreach (JsonOrder jsonOrder in jsonOrders)
                {
                    foreach (JsonStoreOrder jsonStoreOrder in jsonOrder.StoreOrders)
                    {
                        foreach (JsonStoreOrderItem jsonItem in jsonStoreOrder.StoreOrderItems)
                        {
                            Item item = new Item(jsonItem);
                            PurchaseRecord purchase = new PurchaseRecord(jsonOrder.OrderId, item.Name, item.Id, item.Price, jsonItem.Amount);
                            purchase.DiscountedPriceOnPurchase = jsonItem.DiscountedPricePerItem;
                            item.storeID = jsonStoreOrder.StoreId;
                            purchase.ItemObj = item;
                            purchase.storeID = jsonStoreOrder.StoreId;
                            retList.Add(purchase);
                        }
                    }
                }
                return retList;
            }
            return null;
        }

        internal static List<Guid> ordersIdFromUserHistory(string historyJson)
        {

            ResponseClass response = JsonConvert.DeserializeObject<ResponseClass>(historyJson);
            if (response.success)
            {
                List<JsonOrder> jsonOrders = JsonConvert.DeserializeObject<List<JsonOrder>>(response.answerOrExceptionJson);
                List<Guid> ordersId = new List<Guid>() { };
                foreach (JsonOrder jsonOrder in jsonOrders)
                {
                    ordersId.Add(jsonOrder.OrderId);
                }

                return ordersId;
            }

            return null;
        }

       public static List<Store> storesWithItemsListFromJson(string json)
        {
            List<Store> stores = new List<Store>();
            ResponseClass response = JsonConvert.DeserializeObject<ResponseClass>(json);
            if (response.success)
            {
                List<JsonStore> storesFromService = JsonConvert.DeserializeObject<List<JsonStore>>(response.answerOrExceptionJson);
                foreach(JsonStore jsonStore in storesFromService)
                {
                    Store store = new Store(jsonStore);
                    foreach( JsonItem jsonItem in jsonStore.storeInventory.items.Values)
                    {
                        Item i = new Item(jsonItem);
                        i.storeID = store.Id;
                        store.ItemsWithAmount[i] = jsonItem.Amount;
                    }
                    stores.Add(store);
                }
                return stores;
            }
            return null;
        }

        public static List<Item> searchJsonToList(string itemsJson)
        {
            ResponseClass response = JsonConvert.DeserializeObject<ResponseClass>(itemsJson);
            if (response.success)
            {
                List<Item> ret = new List<Item>();
                Dictionary<Guid, List<JsonItem>> storesIdToItems = (JsonConvert.DeserializeObject<Dictionary<Guid,List<JsonItem>>>(response.answerOrExceptionJson));
                foreach(KeyValuePair< Guid, List<JsonItem>> storeIdToItems in storesIdToItems)
                {
                    foreach (JsonItem jsonItem in storeIdToItems.Value)
                    {
                        Item i = new Item(jsonItem);
                        i.storeID = storeIdToItems.Key;
                        ret.Add(i);
                    }
                }
                return ret;
            }
            return null;
        }



        public static Store jsonToStore(string storeJson)
        {
            ResponseClass response = JsonConvert.DeserializeObject<ResponseClass>(storeJson);
            if (response.success)
            {
                return new Store(JsonConvert.DeserializeObject<JsonStore>(response.answerOrExceptionJson));
            }
            return null;
        }

        public static Dictionary<Item, int> cartJsonToDict(string cartJson)
        {
            ResponseClass response = JsonConvert.DeserializeObject<ResponseClass>(cartJson);
            Dictionary<Item, int> retDict = new Dictionary<Item, int>();
            if (response.success)
            {
                Dictionary<Guid, List<Tuple<JsonItem, int>>> storesIDToItemsAndAmount =
                    JsonConvert.DeserializeObject<Dictionary<Guid, List<Tuple<JsonItem, int>>>>(
                        response.answerOrExceptionJson);
                foreach (KeyValuePair<Guid, List<Tuple<JsonItem, int>>> storeIDToItemsAndAmount in storesIDToItemsAndAmount)
                {
                    foreach (Tuple<JsonItem, int> itemAmount in storeIDToItemsAndAmount.Value)
                    {
                        Item i = new Item(itemAmount.Item1);
                        i.storeID = storeIDToItemsAndAmount.Key;
                        retDict[i] = itemAmount.Item2;
                    }
                }
                return retDict;
            }
            return null;
        }
        public static List<PurchaseRecord> boughtCartJsonToPuchaseRecordList(string boughtJson)
        {
            ResponseClass response = JsonConvert.DeserializeObject<ResponseClass>(boughtJson);
            if (response.success)
            {
                JsonOrder jsonOrder =  JsonConvert.DeserializeObject<JsonOrder>(response.answerOrExceptionJson);
                List<PurchaseRecord> retList = new List<PurchaseRecord>();
                foreach(JsonStoreOrder jsonStoreOrder in jsonOrder.StoreOrders)
                {
                    foreach(JsonStoreOrderItem jsonItem in jsonStoreOrder.StoreOrderItems)
                    {
                        Item item = new Item(jsonItem);
                        PurchaseRecord purchase = new PurchaseRecord(jsonOrder.OrderId, item.Name, item.Id, item.Price, jsonItem.Amount);
                        purchase.DiscountedPriceOnPurchase = jsonItem.DiscountedPricePerItem;
                        item.storeID = jsonStoreOrder.StoreId;
                        purchase.ItemObj = item;
                        purchase.storeID = jsonStoreOrder.StoreId;
                        retList.Add(purchase);
                    }
                }
                return retList;
            }
            return null;
        }

        public static List<StoreOrder> BoughtCartJsonToStoreOrderList(string boughtJson)
        {
            ResponseClass response = JsonConvert.DeserializeObject<ResponseClass>(boughtJson);
            if (response.success)
            {
                JsonOrder jsonOrder = JsonConvert.DeserializeObject<JsonOrder>(response.answerOrExceptionJson);

                List<StoreOrder> storeOrders = new List<StoreOrder>();
                foreach (JsonStoreOrder jsonStoreOrder in jsonOrder.StoreOrders)
                {
                    storeOrders.Add(new StoreOrder(jsonStoreOrder));

                }
                return storeOrders;
            }
            return null;
        }

        public static bool? boolFromJson(string json)
        {
            ResponseClass response = JsonConvert.DeserializeObject<ResponseClass>(json);
            if (response.success)
            {
                return JsonConvert.DeserializeObject<bool>(response.answerOrExceptionJson);
            }
            return false;
        }

        public static bool ansFromJson(string json)
        {
            ResponseClass response = JsonConvert.DeserializeObject<ResponseClass>(json);
            if (response.success)
            {
                return JsonConvert.DeserializeObject<bool>(response.answerOrExceptionJson);
            }
            return false;
        }

        internal static bool ansFromJsonSuccess(string json)
        {
            ResponseClass response = JsonConvert.DeserializeObject<ResponseClass>(json);
            if (response.success)
            {
                return JsonConvert.DeserializeObject<bool>(response.answerOrExceptionJson);
            }
            else
                throw new ArgumentException();
        }

        public static List<string> stringsListFromJson(string json)
        {

            ResponseClass response = JsonConvert.DeserializeObject<ResponseClass>(json);
            if (response.success)
            {
                return JsonConvert.DeserializeObject < List<string> > (response.answerOrExceptionJson);
            }
            else
            {
                throw new ArgumentException();
            }
        }

        public static Item itemFromJson(string itemJson)
        {
            ResponseClass response = JsonConvert.DeserializeObject<ResponseClass>(itemJson);
            if (response.success)
            {
                return new Item(JsonConvert.DeserializeObject<JsonItem>(response.answerOrExceptionJson));
            }
            return null;
        }

        public static StorePermissions permissionsFromJson(string json)
        {
            ResponseClass response = JsonConvert.DeserializeObject<ResponseClass>(json);
            if (response.success)
            {
                return new StorePermissions(JsonConvert.DeserializeObject<string[]>(response.answerOrExceptionJson));
            }
            return null;
        }

        public static ItemOpenedDiscount OpenedDiscountFromJson(string json)
        {
            ResponseClass response = JsonConvert.DeserializeObject<ResponseClass>(json);
            if (response.success)
            {
                return new ItemOpenedDiscount(JsonConvert.DeserializeObject<JsonItemOpenedDiscount>(response.answerOrExceptionJson));
            }
            return null;
        }

        public static ItemConditionalDiscountOnAll ItemConditionalDiscountOnAllFromJson(string json)
        {
            ResponseClass response = JsonConvert.DeserializeObject<ResponseClass>(json);
            if (response.success)
            {
                return new ItemConditionalDiscountOnAll(JsonConvert.DeserializeObject<JsonItemConditionalDiscountOnAll>(response.answerOrExceptionJson));
            }
            return null;
        }

        public static ItemConditionalDiscount_DiscountOnExtraItems ItemConditionalDiscount_DiscountOnExtraItemsFromJson(string json)
        {
            ResponseClass response = JsonConvert.DeserializeObject<ResponseClass>(json);
            if (response.success)
            {
                return new ItemConditionalDiscount_DiscountOnExtraItems(JsonConvert.DeserializeObject<JsonItemConditionalDiscount_DiscountOnExtraItems>(response.answerOrExceptionJson));
            }
            return null;
        }

        public static StoreConditionalDiscount StoreConditionalDiscountFromJson(string json)
        {
            ResponseClass response = JsonConvert.DeserializeObject<ResponseClass>(json);
            if (response.success)
            {
                return new StoreConditionalDiscount(JsonConvert.DeserializeObject<JsonStoreConditionalDiscount>(response.answerOrExceptionJson));
            }
            return null;
        }

        public static CompositeTwoDiscounts CompositeTwoDiscountsFromJson(string json)
        {
            ResponseClass response = JsonConvert.DeserializeObject<ResponseClass>(json);
            if (response.success)
            {
                return new CompositeTwoDiscounts(JsonConvert.DeserializeObject<JsonCompositeTwoDiscounts>(response.answerOrExceptionJson));
            }
            return null;
        }

        public static List<Tuple<Store, List<string>>> StoresWithPermissionsSuccessFromJson(string json)
        {
            ResponseClass response = JsonConvert.DeserializeObject<ResponseClass>(json);
            if (response.success)
            {
                List<Tuple<JsonStore, List<string>>> storesInJson =  JsonConvert.DeserializeObject<List<Tuple<JsonStore, List<string>>>>(response.answerOrExceptionJson);
                List<Tuple<Store, List<string>>> storesWithPermissions = new List<Tuple<Store, List<string>>>();
                foreach (Tuple<JsonStore, List<string>> store_permmision in storesInJson)
                {
                    storesWithPermissions.Add(new Tuple<Store, List<string>>(new Store(store_permmision.Item1), store_permmision.Item2));
                }

                return storesWithPermissions;
            }
            return null;
        }

        public static bool operationSuccededOrNotFromJson(string json)
        {
            ResponseClass response = JsonConvert.DeserializeObject<ResponseClass>(json);
            return response.success;
        }

        public static string errorFromFailedOperationFromJson(string json)
        {

            ResponseClass response = JsonConvert.DeserializeObject<ResponseClass>(json);
            if (!response.success)
            {
                Exception e =  JsonConvert.DeserializeObject<Exception>(response.answerOrExceptionJson);
                return e.Message;
            }
            else
            {
                throw new ArgumentException();
            }
        }


        internal static Guid GuidFromPolicySuccess(string json)
        {
            ResponseClass response = JsonConvert.DeserializeObject<ResponseClass>(json);
            if (response.success)
            {
                JsonPolicyForGuid  policy = JsonConvert.DeserializeObject<JsonPolicyForGuid>(response.answerOrExceptionJson);
                return policy.policyID;
            }
            else
            {
                throw new ArgumentException();
            }
        }

        internal static Dictionary<DateTime, int[]> statsFromJsonSuccess(string json)
        {
            ResponseClass response = JsonConvert.DeserializeObject<ResponseClass>(json);
            if (response.success)
            {
                Dictionary<DateTime, int[]> stats = JsonConvert.DeserializeObject<Dictionary<DateTime, int[]>>(response.answerOrExceptionJson);
                return stats;
            }
            else
            {
                throw new ArgumentException();
            }
        }

        internal class JsonPolicyForGuid
        {
            public Guid policyID { get; set; }
        }

        internal class ResponseClass
        {
            public bool success { get; set; }
            public string answerOrExceptionJson { get; set; }
            public ResponseClass()
            {      
            }
        }

        public class JsonItem
        {
            public Guid Id { get; set; }
            public Guid StoreId { get; set; }
            public string Name { get; set; }
            public int Amount { get; set; }
            public HashSet<string> Categories { get; set; }
            public double Rank { get; set; }
            public double Price { get; set; }
            public HashSet<string> KeyWords { get; set; }

            public JsonItem()
            {

            }
        }

        public class JsonStore
        {
            public Guid Id { get; set; }
            public StoreContactDetails ContactDetails { get; set; }
            public double Rank { get; set; }
            public JsonInventory storeInventory { get; set; }
        }

        public class JsonInventory
        {
            public Dictionary<Guid, JsonItem> items { get; set; }
        }

        public class JsonOrder
        {

            public Guid OrderId { get;  set; }
            public Guid UserId { get; set; }
            public List<JsonStoreOrder> StoreOrders { get; set; } //stores orders as a whole order
            public DateTime OrderTime { get; set; }

            public double Total { get; set; }

            //[JsonConstructor]
            //public JsonOrder (Dictionary<Guid, JsonStoreOrder> storeOrders)
            //{
            //    StoreOrders = storeOrders.Values.ToList();
            //}
        }

        public class JsonStoreOrder
        {
            public Guid Id { get;  set; }
            public Guid UserId { get; set; }
            public Guid OrderId { get;  set; }
            public Guid StoreId { get;  set; }
            //JsonItem and Domain.OrderItem are the same execpt JsonItem has storeID,
            //and the othre doesnt
            public List<JsonStoreOrderItem> StoreOrderItems { get; set; }
            public DateTime OrderTime { get; set; }

            public double Total { get; set; }

            //[JsonConstructor]
            //public JsonStoreOrder(Dictionary<Guid, JsonStoreOrderItem> storeOrderItems)
            //{
            //    StoreOrderItems = storeOrderItems.Values.ToList();
            //}
        }

        public class JsonStoreOrderItem
        {
            public Guid ItemId { get;  set; }
            public int Amount { get;  set; }
            public string Name { get;  set; }
            public HashSet<string> Categories { get; set; }
            public double Rank { get;  set; }
            public double BasePricePerItem { get;  set; }
            public double DiscountedPricePerItem { get;  set; }
            public HashSet<string> KeyWords { get; set; }

            public JsonStoreOrderItem()
            {

            }
        }

        public class JsonItemOpenedDiscount
        {

            public Guid discountID { get; set; }
            public DateTime DateUntil { get; set; }
            public double Precent { get; set; }
            public Guid ItemID { get; set; }

            public JsonItemOpenedDiscount()
            {

            }
        }

        public class JsonItemConditionalDiscountOnAll
        {
            public Guid discountID { get; set; }
            public DateTime DateUntil { get; set; }
            public double Precent { get; set; }
            public Guid ItemID { get; set; }
            public int MinItems { get; set; }

            public JsonItemConditionalDiscountOnAll()
            {

            }
        }

        public class JsonItemConditionalDiscount_DiscountOnExtraItems
        {
            public Guid discountID { get; set; }
            public DateTime DateUntil { get; set; }
            public double DiscountForExtra { get; set; }
            public Guid ItemID { get; set; }
            public int MinItems { get; set; }
            public int ExtraItems { get; set; }

            public JsonItemConditionalDiscount_DiscountOnExtraItems()
            {

            }
        }

        public class JsonStoreConditionalDiscount
        {
            public Guid discountID { get; set; }
            public DateTime DateUntil { get; set; }
            public double Precent { get; set; }
            public double MinPurchase { get; set; }

            public JsonStoreConditionalDiscount()
            {

            }
        }

        public class JsonCompositeTwoDiscounts
        {
            public Guid discountID { get; set; }
            public DateTime DateUntil { get; set; }
            public Utilitys.Utils.Operator Op { get; set; }
            public Guid DiscountLeftID { get; set; }
            public Guid DiscountRightID { get; set; }

            public JsonCompositeTwoDiscounts()
            {

            }
        }

       
    }
}

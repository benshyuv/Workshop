using DomainLayer.Orders;
using DomainLayer.Stores;
using DomainLayer.Stores.Discounts;
using DomainLayer.Stores.Inventory;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace ServiceLayer
{
    public class JsonResponse
    {
        public string Create_json_response(bool success, object answerOrexception)
        {
            string responseJson = JsonConvert.SerializeObject(answerOrexception);
            ResponseClass response = new ResponseClass(success, responseJson);
            return JsonConvert.SerializeObject(response);
        }
        public T deserializeResponse<T>(string respose)
        {
            ResponseClass res = JsonConvert.DeserializeObject<ResponseClass>(respose);
            T ans = JsonConvert.DeserializeObject<T>(res.AnswerOrExceptionJson);
            return ans;
        }

        public bool deserializeSuccess(string respose)
        {
            ResponseClass res = JsonConvert.DeserializeObject<ResponseClass>(respose);
            return res.Success;
        }

        public Guid storeID_from_store_json(string repsonse)
        {
            var store = deserializeResponse<Store>(repsonse);
            return store.Id;
        }

        public Guid itemID_from_item_json(string repsonse)
        {
            var item = deserializeResponse<Item>(repsonse);
            return item.Id;
        }
        public Guid orderID_from_order_json(string repsonse)
        {
            var order = deserializeResponse<Order>(repsonse);
            return order.OrderId;
        }
        private Guid discountID_from_discount_json<T >(string repsonse) where T : ADiscount
        {
            var discount = deserializeResponse<T>(repsonse);
            return discount.discountID;
        }

        public Guid discountID_from_open_discount_json(string response) => discountID_from_discount_json<OpenDiscount>(response);
        public Guid discountID_from_store_discount_json(string response) => discountID_from_discount_json<StoreConditionalDiscount>(response);
        public Guid discountID_from_itemOnAll_discount_json(string response) => discountID_from_discount_json<ItemConditionalDiscount_MinItems_ToDiscountOnAll>(response);
        public Guid discountID_from_itemOnExtra_discount_json(string response) => discountID_from_discount_json<ItemConditionalDiscount_MinItems_ToDiscountOnExtraItems>(response);
        public Guid discountID_from_composite_discount_json(string response) => discountID_from_discount_json<CompositeDiscount>(response);





    }
}

internal class ResponseClass
{
    public bool Success { get; set; }
    public string AnswerOrExceptionJson { get; set; }

    public ResponseClass(bool success, string answerOrExceptionJson)
    {
        this.Success = success;
        this.AnswerOrExceptionJson = answerOrExceptionJson;
    }

    public ResponseClass() { }

}

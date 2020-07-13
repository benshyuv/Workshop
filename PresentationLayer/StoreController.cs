using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.Serialization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using ServiceLayer.Services;

namespace PresentationLayer
{
    [Route("api/[controller]")]
    public class StoreController : Controller
    {
        private static StoreActions GetStoreActions() => Actions.GetActions().Store;

        [HttpGet]
        [Route("GetAllStoresInformation")]
        public string GetAllStoresInformation()
        {
            HttpContext.Session.Set("active", new byte[] { 1 });
            return GetStoreActions()
                .GetAllStoresInformation(Guid.Parse(HttpContext.Session.Id));
        }

        [HttpGet]
        [Route("GetStoresWithPermissions")]
        public string GetStoresWithPermissions()
        {
            HttpContext.Session.Set("active", new byte[] { 1 });
            return GetStoreActions()
                .GetStoresWithPermissions(Guid.Parse(HttpContext.Session.Id));
        }

        [HttpGet]
        [Route("GetStoreInformationByID")]
        public string GetStoreInformationByID([FromQuery(Name = "storeid")] Guid storeID)
        {
            HttpContext.Session.Set("active", new byte[] { 1 });
            return GetStoreActions()
                .GetStoreInformationByID(Guid.Parse(HttpContext.Session.Id), storeID);
        }

        [HttpGet]
        [Route("GetMyPermissions")]
        public string GetMyPermissions([FromQuery(Name = "storeid")] Guid storeID)
        {
            HttpContext.Session.Set("active", new byte[] { 1 });
            return GetStoreActions()
                .GetMyPermissions(Guid.Parse(HttpContext.Session.Id), storeID);
        }

        [HttpPost]
        [Route("OpenStore")]
        public string OpenStore([FromBody] OpenStoreDetails details)
        {
            HttpContext.Session.Set("active", new byte[] { 1 });
            return GetStoreActions()
                .OpenStore(Guid.Parse(HttpContext.Session.Id), details.Name, details.Email, details.Address, details.Phone,
                            details.BankAccountNumber, details.Bank, details.Description, details.PurchasePolicy, details.DiscountPolicy);
        }

        [HttpPost]
        [Route("AddItem")]
        public string AddItem([FromBody] AddItemDetails details)
        {
            HttpContext.Session.Set("active", new byte[] { 1 });
            return GetStoreActions()
                .AddItem(Guid.Parse(HttpContext.Session.Id), details.StoreID, details.Name,
                            details.Amount, details.Categories, details.Price, details.Keywords);// TODO: check keywords not mandatory
        }

        [HttpPost]
        [Route("DeleteItem")]
        public string DeleteItem([FromBody] DeleteItemDetails details)
        {
            HttpContext.Session.Set("active", new byte[] { 1 });
            return GetStoreActions()
                .DeleteItem(Guid.Parse(HttpContext.Session.Id), details.StoreID, details.ItemID);
        }

        [HttpPost]
        [Route("EditItem")]
        public string EditItem([FromBody] EditItemDetails details)
        {
            HttpContext.Session.Set("active", new byte[] { 1 });
            return GetStoreActions()
                .EditItem(Guid.Parse(HttpContext.Session.Id), details.StoreID, details.ItemID, details.Amount,
                            details.Rank, details.Price, details.Name, details.Categories, details.Keywords);
        }

        /*[HttpPost]
        [Route("Rankitem")]
        public string RankItem([FromBody] RateItemDetails details)
        {
            HttpContext.Session.Set("active", new byte[] { 1 });
            return GetStoreActions()
                .EditItem(Guid.Parse(HttpContext.Session.Id), details.StoreID, details.ItemID, amount: null, details.Rank, price: null);
        }*/

        [HttpGet]
        [Route("GetStoreOrderHistory")]
        public string GetStoreOrderHistory([FromQuery(Name = "storeid")] Guid storeID)
        {
            HttpContext.Session.Set("active", new byte[] { 1 });
            return GetStoreActions()
                .GetStoreOrderHistory(Guid.Parse(HttpContext.Session.Id), storeID);
        }

        [HttpGet]
        [Route("GetStoreOrderHistoryAdmin")]
        public string GetStoreOrderHistoryAdmin([FromQuery(Name = "storeid")] Guid storeID)
        {
            HttpContext.Session.Set("active", new byte[] { 1 });
            return GetStoreActions()
                .GetStoreOrderHistoryAdmin(Guid.Parse(HttpContext.Session.Id), storeID);
        }

        [HttpPost]
        [Route("AppointOwner")]
        public string AppointOwner([FromBody] AppointOrRemoveOrApproveOwnerOrManagerDetails details)
        {
            HttpContext.Session.Set("active", new byte[] { 1 });
            return GetStoreActions()
                .AppointOwner(Guid.Parse(HttpContext.Session.Id), details.StoreID, details.Username);
        }

        [HttpPost]
        [Route("ApproveOwnerContract")]
        public string ApproveOwnerContract([FromBody] AppointOrRemoveOrApproveOwnerOrManagerDetails details)
        {
            HttpContext.Session.Set("active", new byte[] { 1 });
            return GetStoreActions()
                .ApproveOwnerContract(Guid.Parse(HttpContext.Session.Id), details.StoreID, details.Username);
        }

        [HttpPost]
        [Route("RemoveOwner")]
        public string RemoveOwner([FromBody] AppointOrRemoveOrApproveOwnerOrManagerDetails details)
        {
            HttpContext.Session.Set("active", new byte[] { 1 });
            return GetStoreActions()
                .RemoveOwner(Guid.Parse(HttpContext.Session.Id), details.StoreID, details.Username);
        }

        [HttpPost]
        [Route("AppointManager")]
        public string AppointManager([FromBody] AppointOrRemoveOrApproveOwnerOrManagerDetails details)
        {
            HttpContext.Session.Set("active", new byte[] { 1 });
            return GetStoreActions()
                .AppointManager(Guid.Parse(HttpContext.Session.Id), details.StoreID, details.Username);
        }

        [HttpPost]
        [Route("RemoveManager")]
        public string RemoveManager([FromBody] AppointOrRemoveOrApproveOwnerOrManagerDetails details)
        {
            HttpContext.Session.Set("active", new byte[] { 1 });
            return GetStoreActions()
                .RemoveManager(Guid.Parse(HttpContext.Session.Id), details.StoreID, details.Username);
        }

        [HttpPost]
        [Route("AddPermission")]
        public string AddPermission([FromBody] AddOrRemovePermissionDetails details)
        {
            HttpContext.Session.Set("active", new byte[] { 1 });
            return GetStoreActions()
                .AddPermission(Guid.Parse(HttpContext.Session.Id), details.StoreID, details.Username, details.Permission);
        }

        [HttpPost]
        [Route("RemovePermission")]
        public string RemovePermission([FromBody] AddOrRemovePermissionDetails details)
        {
            HttpContext.Session.Set("active", new byte[] { 1 });
            return GetStoreActions()
                .RemovePermission(Guid.Parse(HttpContext.Session.Id), details.StoreID, details.Username, details.Permission);
        }

        [HttpGet]
        [Route("SearchItems")]
        public string SearchItems([FromQuery(Name = "filterItemRank")] double? filterItemRank, [FromQuery(Name = "filterMinPrice")] double? filterMinPrice, [FromQuery(Name = "filterMaxPrice")] double? filterMaxPrice, [FromQuery(Name = "filterStoreRank")] double? filterStoreRank, [FromQuery(Name = "itemName")] string itemName = null, [FromQuery(Name = "category")] string category = null, [FromQuery(Name = "keywords")] string keywords = null, [FromQuery(Name = "filterCategory")] string filterCategory = null)
        {
            //TODO: remove filter categpry
            HttpContext.Session.Set("active", new byte[] { 1 });
            return GetStoreActions()
                .SearchItems(Guid.Parse(HttpContext.Session.Id), filterItemRank, filterMinPrice, filterMaxPrice, filterStoreRank, itemName, category, keywords);
        }

        [HttpGet]
        [Route("GetAllowedDiscounts")]
        public string GetAllowedDiscounts([FromQuery(Name = "storeid")] Guid storeID)
        {
            HttpContext.Session.Set("active", new byte[] { 1 });
            return GetStoreActions()
                .GetAllowedDiscounts(Guid.Parse(HttpContext.Session.Id), storeID);
        }

        [HttpPost]
        [Route("RemoveDiscount")]
        public string RemoveDiscount([FromBody] RemoveDiscountDetails details)
        {
            HttpContext.Session.Set("active", new byte[] { 1 });
            return GetStoreActions()
                .RemoveDiscount(Guid.Parse(HttpContext.Session.Id), details.StoreID, details.DiscountID);
        }

        [HttpPost]
        [Route("MakeDiscountNotAllowed")]
        public string MakeDiscountNotAllowed([FromBody] MakeDiscountNotAllowedDetails details)
        {
            HttpContext.Session.Set("active", new byte[] { 1 });
            return GetStoreActions()
                .MakeDiscountNotAllowed(Guid.Parse(HttpContext.Session.Id), details.StoreID, details.DiscountTypeString);
        }

        [HttpPost]
        [Route("MakeDiscountAllowed")]
        public string MakeDiscountAllowed([FromBody] MakeDiscountAllowedDetails details)
        {
            HttpContext.Session.Set("active", new byte[] { 1 });
            return GetStoreActions()
                .MakeDiscountAllowed(Guid.Parse(HttpContext.Session.Id), details.StoreID, details.DiscountTypeString);
        }
        [HttpPost]
        [Route("AddOpenDiscount")]
        public string AddOpenDiscount([FromBody] AddOpenDiscountDetails details)
        {
            HttpContext.Session.Set("active", new byte[] { 1 });
            return GetStoreActions()
                .AddOpenDiscount(Guid.Parse(HttpContext.Session.Id), details.StoreID, details.ItemID, details.Discount, details.DurationInDays);
        }

        [HttpPost]
        [Route("AddItemConditionalDiscount_MinItems_ToDiscountOnAll")]
        public string AddItemConditionalDiscount_MinItems_ToDiscountOnAll([FromBody] AddItemConditionalDiscount_MinItems_ToDiscountOnAllDetails details)
        {
            HttpContext.Session.Set("active", new byte[] { 1 });
            return GetStoreActions()
                .AddItemConditionalDiscount_MinItems_ToDiscountOnAll(Guid.Parse(HttpContext.Session.Id), details.StoreID, details.ItemID, details.DurationInDays, details.MinItems, details.Discount);
        }

        [HttpPost]
        [Route("AddItemConditionalDiscount_MinItems_ToDiscountOnExtraItems")]
        public string AddItemConditionalDiscount_MinItems_ToDiscountOnExtraItems([FromBody] AddItemConditionalDiscount_MinItems_ToDiscountOnExtraItemsDetails details)
        {
            HttpContext.Session.Set("active", new byte[] { 1 });
            return GetStoreActions()
                .AddItemConditionalDiscount_MinItems_ToDiscountOnExtraItems(Guid.Parse(HttpContext.Session.Id), details.StoreID, details.ItemID, details.DurationInDays, details.MinItems, details.ExtraItems, details.DiscountForExtra);
        }

        [HttpPost]
        [Route("AddStoreConditionalDiscount")]
        public string AddStoreConditionalDiscount([FromBody] AddStoreConditionalDiscountDetails details)
        {
            HttpContext.Session.Set("active", new byte[] { 1 });
            return GetStoreActions()
                .AddStoreConditionalDiscount(Guid.Parse(HttpContext.Session.Id), details.StoreID, details.DurationInDays, details.MinPurchase, details.Discount);
        }

        [HttpPost]
        [Route("ComposeTwoDiscounts")]
        public string ComposeTwoDiscounts([FromBody] ComposeTwoDiscountsDetails details)
        {
            HttpContext.Session.Set("active", new byte[] { 1 });
            return GetStoreActions()
                .ComposeTwoDiscounts(Guid.Parse(HttpContext.Session.Id), details.StoreID, details.DiscountLeftID, details.DiscountRightID, details.BoolOperator);
        }

        [HttpGet]
        [Route("GetAllDiscounts")]
        public string GetAllDiscounts([FromQuery(Name = "storeid")] Guid storeID, [FromQuery(Name = "itemid")] Guid? itemid)
        {
            HttpContext.Session.Set("active", new byte[] { 1 });
            return GetStoreActions()
                .GetAllDiscounts(Guid.Parse(HttpContext.Session.Id), storeID, itemid);
        }

        [HttpPost]
        [Route("AddItemMinMaxPurchasePolicy")]
        public string AddItemMinMaxPurchasePolicy([FromBody] AddItemMinMaxPurchasePolicyDetails details)
        {
            HttpContext.Session.Set("active", new byte[] { 1 });
            return GetStoreActions()
                .AddItemMinMaxPurchasePolicy(Guid.Parse(HttpContext.Session.Id), details.StoreID, details.ItemID, details.MinAmount, details.MaxAmount);
        }

        [HttpPost]
        [Route("AddStoreMinMaxPurchasePolicy")]
        public string AddStoreMinMaxPurchasePolicy([FromBody] AddStoreMinMaxPurchasePolicyDetails details)
        {
            HttpContext.Session.Set("active", new byte[] { 1 });
            return GetStoreActions()
                .AddStoreMinMaxPurchasePolicy(Guid.Parse(HttpContext.Session.Id), details.StoreID, details.MinAmount, details.MaxAmount);
        }

        [HttpPost]
        [Route("AddDaysNotAllowedPurchasePolicy")]
        public string AddDaysNotAllowedPurchasePolicy([FromBody] AddDaysNotAllowedPurchasePolicyDetails details)
        {
            HttpContext.Session.Set("active", new byte[] { 1 });
            return GetStoreActions()
                .AddDaysNotAllowedPurchasePolicy(Guid.Parse(HttpContext.Session.Id), details.StoreID, details.DaysNotAllowed);
        }

        [HttpPost]
        [Route("ComposeTwoPurchasePolicies")]
        public string ComposeTwoPurchasePolicies([FromBody] ComposeTwoPurchasePoliciesDetails details)
        {
            HttpContext.Session.Set("active", new byte[] { 1 });
            return GetStoreActions()
                .ComposeTwoPurchasePolicies(Guid.Parse(HttpContext.Session.Id), details.StoreID, details.PolicyLeftID, details.PolicyRightID, details.BoolOperator);
        }

        [HttpPost]
        [Route("MakePurcahsePolicyAllowed")]
        public string MakePurcahsePolicyAllowed([FromBody] MakePurcahsePolicyAllowedDetails details)
        {
            HttpContext.Session.Set("active", new byte[] { 1 });
            return GetStoreActions()
                .MakePurcahsePolicyAllowed(Guid.Parse(HttpContext.Session.Id), details.StoreID, details.PurchasePolicy);
        }

        [HttpPost]
        [Route("MakePurcahsePolicyNotAllowed")]
        public string MakePurcahsePolicyNotAllowed([FromBody] MakePurcahsePolicyNotAllowedDetails details)
        {
            HttpContext.Session.Set("active", new byte[] { 1 });
            return GetStoreActions()
                .MakePurcahsePolicyNotAllowed(Guid.Parse(HttpContext.Session.Id), details.StoreID, details.PurchasePolicy);
        }

        [HttpPost]
        [Route("RemovePurchasePolicy")]
        public string RemovePurchasePolicy([FromBody] RemovePurchasePolicyDetails details)
        {
            HttpContext.Session.Set("active", new byte[] { 1 });
            return GetStoreActions()
                .RemovePurchasePolicy(Guid.Parse(HttpContext.Session.Id), details.StoreID, details.PolicyID);
        }

        [HttpGet]
        [Route("GetAllowedPurchasePolicys")]
        public string GetAllowedPurchasePolicys([FromQuery(Name = "storeid")] Guid storeID)
        {
            HttpContext.Session.Set("active", new byte[] { 1 });
            return GetStoreActions()
                .GetAllowedPurchasePolicys(Guid.Parse(HttpContext.Session.Id), storeID);
        }

        [HttpGet]
        [Route("GetAllPurchasePolicys")]
        public string GetAllPurchasePolicys([FromQuery(Name = "storeid")] Guid storeID)
        {
            HttpContext.Session.Set("active", new byte[] { 1 });
            return GetStoreActions()
                .GetAllPurchasePolicys(Guid.Parse(HttpContext.Session.Id), storeID);
        }

    }
    public class AddItemDetails
    {
        public Guid StoreID { get; set; }

        public string Name { get; set; }

        public int Amount { get; set; }

        public string Categories { get; set; }

        public double Price { get; set; }

        public string Keywords { get; set; }
    }

    public class DeleteItemDetails
    {
        public Guid StoreID { get; set; }

        public Guid ItemID { get; set; }
    }

    /*public class RateItemDetails
    {
        public Guid StoreID { get; set; }

        public Guid ItemID { get; set; }

        public int Rank { get; set; }
    }*/

    public class EditItemDetails
    {
        public Guid StoreID { get; set; }

        public Guid ItemID { get; set; }

        public int Amount { get; set; }

        public int Rank { get; set; }

        public double Price { get; set; }

        public string Name { get; set; }

        public string Categories { get; set; }

        public string Keywords { get; set; }
    }

    public class OpenStoreDetails
    {
        public string Name { get; set; }

        public string Email { get; set; }

        public string Address { get; set; }

        public string Phone { get; set; }

        public string BankAccountNumber { get; set; }

        public string Bank { get; set; }

        public string Description { get; set; }

        public string PurchasePolicy { get; set; }

        public string DiscountPolicy { get; set; }
    }

    public class AppointOrRemoveOrApproveOwnerOrManagerDetails
    {
        public Guid StoreID { get; set; }

        public string Username { get; set; }
    }

    public class AddOrRemovePermissionDetails
    {
        public Guid StoreID { get; set; }

        public string Username { get; set; }

        public string Permission { get; set; }
    }

    public class RemoveDiscountDetails
    {
        public Guid StoreID { get; set; }
        public Guid DiscountID { get; set; }
    }

    public class MakeDiscountNotAllowedDetails
    {
        public Guid StoreID { get; set; }
        public string DiscountTypeString { get; set; }
    }

    public class MakeDiscountAllowedDetails
    {
        public Guid StoreID { get; set; }
        public string DiscountTypeString { get; set; }
    }

    public class AddOpenDiscountDetails
    {
        public Guid StoreID { get; set; }
        public Guid ItemID { get; set; }
        public double Discount { get; set; }
        public int DurationInDays { get; set; }
    }

    public class AddItemConditionalDiscount_MinItems_ToDiscountOnAllDetails
    {
        public Guid StoreID { get; set; }
        public Guid ItemID { get; set; }
        public int DurationInDays { get; set; }
        public int MinItems { get; set; }
        public double Discount { get; set; }
    }

    public class AddItemConditionalDiscount_MinItems_ToDiscountOnExtraItemsDetails
    {
        public Guid StoreID { get; set; }
        public Guid ItemID { get; set; }
        public int DurationInDays { get; set; }
        public int MinItems { get; set; }
        public int ExtraItems { get; set; }
        public double DiscountForExtra { get; set; }
    }

    public class AddStoreConditionalDiscountDetails
    {
        public Guid StoreID { get; set; }
        public int DurationInDays { get; set; }
        public double MinPurchase { get; set; }
        public double Discount { get; set; }
    }

    public class ComposeTwoDiscountsDetails
    {
        public Guid StoreID { get; set; }
        public Guid DiscountLeftID { get; set; }
        public Guid DiscountRightID { get; set; }
        public string BoolOperator { get; set; }
    }

    public class AddItemMinMaxPurchasePolicyDetails
    {
        public Guid StoreID { get; set; }
        public Guid ItemID { get; set; }
        public int? MinAmount { get; set; }
        public int? MaxAmount { get; set; }
    }

    public class AddStoreMinMaxPurchasePolicyDetails
    {
        public Guid StoreID { get; set; }
        public int? MinAmount { get; set; }
        public int? MaxAmount { get; set; }
    }

    public class AddDaysNotAllowedPurchasePolicyDetails
    {
        public Guid StoreID { get; set; }
        public int[] DaysNotAllowed { get; set; }
    }

    public class ComposeTwoPurchasePoliciesDetails
    {
        public Guid StoreID { get; set; }
        public Guid PolicyLeftID { get; set; }
        public Guid PolicyRightID { get; set; }
        public string BoolOperator { get; set; }
    }

    public class MakePurcahsePolicyAllowedDetails
    {
        public Guid StoreID { get; set; }
        public string PurchasePolicy { get; set; }
    }

    public class MakePurcahsePolicyNotAllowedDetails
    {
        public Guid StoreID { get; set; }
        public string PurchasePolicy { get; set; }
    }

    public class RemovePurchasePolicyDetails
    {
        public Guid StoreID { get; set; }
        public Guid PolicyID { get; set; }
    }
}

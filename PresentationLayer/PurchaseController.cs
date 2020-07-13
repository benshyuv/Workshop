using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using ServiceLayer.Services;

namespace PresentationLayer
{
    [Route("api/[controller]")]
    public class PurchaseController : Controller
    {
        private static PurchaseActions GetPurchaseActions() => Actions.GetActions().Purchase;

        [HttpGet]
        [Route("DisplayBeforeCheckout")]
        public string DisplayBeforeCheckout()
        {
            HttpContext.Session.Set("active", new byte[] { 1 });
            return GetPurchaseActions()
                .DisplayBeforeCheckout(Guid.Parse(HttpContext.Session.Id));
        }

        [HttpPost]
        [Route("CheckOut")]
        public string CheckOut([FromBody] CheckOutDetails details)
        {
            HttpContext.Session.Set("active", new byte[] { 1 });
            //return GetPurchaseActions()
            //    .CheckOut(Guid.Parse(HttpContext.Session.Id), details.OrderID, details.Card, details.Expire, details.CCV,
            //                details.Address, details.City, details.Country, details.ZipCode);
            return GetPurchaseActions()
                .CheckOut(Guid.Parse(HttpContext.Session.Id), details.OrderID,  details.Card, details.Expire, details.CCV,
                              details.PayingCustomerName, details.ID, details.Address, details.City, details.Country, details.ZipCode);
        }

        [HttpGet]
        [Route("GetUserOrderHistory")]
        public string GetUserOrderHistory([FromQuery(Name = "storeid")] string username)
        {
            HttpContext.Session.Set("active", new byte[] { 1 });
            return GetPurchaseActions()
                .GetUserOrderHistory(Guid.Parse(HttpContext.Session.Id), username);
        }
    }

    public class CheckOutDetails
    {
        public Guid OrderID { get; set; }

        public string PayingCustomerName { get; set; }

        public string Card { get; set; }

        public string Expire { get; set; }

        public string CCV { get; set; }

        public string ID { get; set; }

        public string Address { get; set; }

        public string City { get; set; }

        public string Country { get; set; }

        public string ZipCode { get; set; }

    }
}

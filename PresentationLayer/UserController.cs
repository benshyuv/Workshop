using System;
using System.Collections.Generic;
using System.Runtime.InteropServices.ComTypes;
using System.Runtime.Serialization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using ServiceLayer.Services;

namespace PresentationLayer
{
    [Route("api/[controller]")]
    public class UserController : Controller
    {
        private static UserActions GetUserActions() => Actions.GetActions().User;

        [HttpPost]
        [Route("Login")]
        public string Login([FromBody] UsernamePassword request)
        {
            HttpContext.Session.Set("active", new byte[] { 1 });
            return GetUserActions()
                .Login(Guid.Parse(HttpContext.Session.Id), request.Username, request.Password);
        }

        [HttpPost]
        [Route("Register")]
        public string Register([FromBody] UsernamePassword request)
        {
            HttpContext.Session.Set("active", new byte[] { 1 });
            return GetUserActions()
                .Register(Guid.Parse(HttpContext.Session.Id), request.Username, request.Password);
        }

        [HttpGet]
        [Route("GetDailyStatistics")]
        public string GetDailyStatistics([FromQuery(Name = "datefrom")] string datefrom,[FromQuery(Name = "dateto")] string dateto) 
        {
            HttpContext.Session.Set("active", new byte[] { 1 });
            return GetUserActions()
                .GetDailyStatistics(Guid.Parse(HttpContext.Session.Id),datefrom,dateto);
        }
        [HttpGet]
        [Route("Logout")]
        public string Logout()
        {
            HttpContext.Session.Set("active", new byte[] { 1 });
            return GetUserActions()
                .Logout(Guid.Parse(HttpContext.Session.Id));
        }

        [HttpGet]
        [Route("GetMyOrderHistory")]
        public string History()
        {
            HttpContext.Session.Set("active", new byte[] { 1 });
            return GetUserActions()
                .GetMyOrderHistory(Guid.Parse(HttpContext.Session.Id));
        }
        [HttpGet]
        [Route("GetMyMessages")]
        public string GetMyMessages()
        {
            HttpContext.Session.Set("active", new byte[] { 1 });
            return GetUserActions()
                .GetMyMessages(Guid.Parse(HttpContext.Session.Id));
        }
        [HttpGet]
        [Route("ViewCart")]
        public string ViewCart()
        {
            HttpContext.Session.Set("active", new byte[] { 1 });
            return GetUserActions()
                .ViewCart(Guid.Parse(HttpContext.Session.Id));
        }

        [HttpPost]
        [Route("AddToCart")]
        public string AddToCart( [FromBody] StoreItemAmount details)
        {
            HttpContext.Session.Set("active", new byte[] { 1 });
            return GetUserActions()
                .AddToCart(Guid.Parse(HttpContext.Session.Id), details.StoreID, details.ItemID, details.Amount);
        }

        [HttpPost]
        [Route("ChangeItemAmountInCart")]
        public string ChangeItemAmountInCart([FromBody] StoreItemAmount details)
        {
            HttpContext.Session.Set("active", new byte[] { 1 });
            return GetUserActions()
                .ChangeItemAmountInCart(Guid.Parse(HttpContext.Session.Id), details.StoreID,
                                            details.ItemID, details.Amount);
        }

        [HttpPost]
        [Route("RemoveFromCart")]
        public string RemoveFromCart([FromBody] StoreItem details)
        {
            HttpContext.Session.Set("active", new byte[] { 1 });
            return GetUserActions()
                .RemoveFromCart(Guid.Parse(HttpContext.Session.Id), details.StoreID, details.ItemID);
        }

        [HttpGet]
        [Route("IsAdmin")]
        public string IsAdmin()
        {
            HttpContext.Session.Set("active", new byte[] { 1 });
            return GetUserActions()
                .IsAdmin(Guid.Parse(HttpContext.Session.Id));
        }

        [DataContract]
        public class UsernamePassword
        {
            public string Username { get; set; }

            public string Password { get; set; }
        }

        [DataContract]
        public class StoreItem
        {
            public Guid StoreID { get; set; }

            public Guid ItemID { get; set; }
        }

        [DataContract]
        public class StoreItemAmount
        {
            public Guid StoreID { get; set; }

            public Guid ItemID { get; set; }

            public int Amount { get; set; }
        }
    }
}

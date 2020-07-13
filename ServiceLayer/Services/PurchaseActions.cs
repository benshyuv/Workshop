using DomainLayer.Market;
using System;
using CustomLogger;
using ServiceLayer.Exceptions;

namespace ServiceLayer.Services
{
    public class PurchaseActions
    {
        private readonly IMarketFacade newMarketFacade;
        private readonly JsonResponse Json;
        public PurchaseActions( IMarketFacade newMarketFacade)
        {
            this.newMarketFacade = newMarketFacade;
            Json = new JsonResponse();
        }

        public string CheckOut(Guid sessionID, Guid orderID, 
                                string cardNum, string expire, string CCV,
                                string name, string cardOwnerID,
                                string address, string city, string country, string zipCode)// UC 2.8
        {
            string paymentResponse = newMarketFacade.CollectPayment(sessionID, orderID, cardNum, expire, CCV, name, cardOwnerID);
            if (!Json.deserializeSuccess(paymentResponse))
            {
                newMarketFacade.ReturnProducts(sessionID, orderID);
                return paymentResponse;
            }
            string deliveryResponse = newMarketFacade.ScheduleDelivery(sessionID, orderID, address, city, country, zipCode, name);
            if (!Json.deserializeSuccess(deliveryResponse))
            {
                newMarketFacade.ReturnProducts(sessionID, orderID);
                newMarketFacade.CancelPayment(sessionID, orderID, cardNum);
                return deliveryResponse;
            }
            return newMarketFacade.CheckOut(sessionID, orderID);
        }

        public string DisplayBeforeCheckout(Guid sessionID)
        {
            string validateResponse = newMarketFacade.ValidatePurchase(sessionID);
            if (!Json.deserializeSuccess(validateResponse))
            {
                return validateResponse;
            }
            return newMarketFacade.DiscountAndReserve(sessionID);
        }

        public string GetUserOrderHistory(Guid sessionID, string username)// UC 6.4
        {
            if (string.IsNullOrWhiteSpace(username) || sessionID == null || sessionID.Equals(Guid.Empty))
            {
                Logger.writeEvent("Invalid input while getting the user " + username + " history");
                return Json.Create_json_response(false, new InvalidInputException());
            }
            if (!newMarketFacade.IsloggedIn(sessionID))
            {
                Logger.writeEvent("GetUserOrderHistory failed | not logged in");
                return Json.Create_json_response(false, new LoggedOutException());
            }
            if (!newMarketFacade.IsRegisteredUser(username))
            {
                Logger.writeEvent("GetUserOrderHistory failed | not a registered user");
                return Json.Create_json_response(false, new NotRegisterdException());
            }

            if (!newMarketFacade.IsAdmin(sessionID))
            {
                Logger.writeEvent("GetUserOrderHistory failed | Not admin");
                return Json.Create_json_response(false, new NotAdminException());
            }
            return newMarketFacade.GetUserOrderHistory(sessionID, username);
        }
    }
}

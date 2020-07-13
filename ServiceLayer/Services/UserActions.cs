using DomainLayer.Market;
using System;
using CustomLogger;
using ServiceLayer.Exceptions;
using System.Globalization;

namespace ServiceLayer.Services
{
    public class UserActions
    {
        private readonly IMarketFacade newMarketFacade;
        private readonly JsonResponse Json;

        public UserActions( IMarketFacade newMarketFacade)
        {
            this.newMarketFacade = newMarketFacade;
            Json = new JsonResponse();
        }

        public string Register(Guid sessionID, string username, string password)// UC 2.2
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password) ||
                sessionID == null || sessionID.Equals(Guid.Empty))
            {
                Logger.writeEvent("Invalid input in register");
                return Json.Create_json_response(false, new InvalidInputException());
            }
            return newMarketFacade.Register(sessionID, username, password);
        }

        public string Login(Guid sessionID, string username, string password)// UC 2.3
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password) ||
                sessionID == null || sessionID.Equals(Guid.Empty))
            {
                Logger.writeEvent("Invalid input in login");
                return Json.Create_json_response(false, new InvalidInputException());
            }
            return newMarketFacade.Login(sessionID, username, password);
        }

        public string Logout(Guid sessionID)// UC 3.1
        {
            if(sessionID == null || sessionID.Equals(Guid.Empty))
            {
                Logger.writeEvent("Invalid input in logout");
                return Json.Create_json_response(false, new InvalidInputException());
            }
            return newMarketFacade.Logout(sessionID);
        }

        public string GetMyOrderHistory(Guid sessionID)// UC 3.7
        {
            if (sessionID == null || sessionID.Equals(Guid.Empty))
            {
                Logger.writeEvent("Invalid input while getting the user history");
                return Json.Create_json_response(false, new InvalidInputException());
            }
            if (!newMarketFacade.IsloggedIn(sessionID))
            {
                Logger.writeEvent("GetMyOrderHistory failed | not logged in");
                return Json.Create_json_response(false, new LoggedOutException());
            }
            return newMarketFacade.GetMyOrderHistory(sessionID);
        }

        public string AddToCart(Guid sessionID, Guid storeID, Guid itemID, int amount)// UC 2.6
        {
            if (amount <= 0 || sessionID == null || sessionID.Equals(Guid.Empty))
            {
                Logger.writeEvent("Invalid input while adding item to cart");
                return Json.Create_json_response(false, new InvalidInputException());
            }
            if (!newMarketFacade.StoreExist(storeID))
            {
                Logger.writeEvent("Add item to cart | invalid storeID");
                return Json.Create_json_response(false, new StoreIDDoesntExistException());
            }
            if(!newMarketFacade.ItemExistInStore(storeID, itemID))
            {
                Logger.writeEvent("Add item to cart | invalid itemID");
                return Json.Create_json_response(false, new ItemDoesntExistException());
            }
            if (!newMarketFacade.CheckSufficentAmountInInventory(storeID, itemID, amount))
            {
                Logger.writeEvent("Add item to cart | not enough amount of items in inventory");
                return Json.Create_json_response(false, new InsufficientItemAmountException());
            }
            return newMarketFacade.AddToCart(sessionID, storeID, itemID, amount);
        }

        public string ViewCart(Guid sessionID)// UC 2.7
        {
            if (sessionID == null || sessionID.Equals(Guid.Empty))
            {
                Logger.writeEvent("Invalid input while viewing items in cart");
                return Json.Create_json_response(false, new InvalidInputException());
            }
            return newMarketFacade.GetUserCart(sessionID);
        }

        public string RemoveFromCart(Guid sessionID, Guid storeID, Guid itemID)// UC 2.7
        {
            if (sessionID == null || sessionID.Equals(Guid.Empty))
            {
                Logger.writeEvent("Invalid input while removing item from cart");
                return Json.Create_json_response(false, new InvalidInputException());
            }
            if (!newMarketFacade.StoreExist(storeID))
            {
                Logger.writeEvent("Remove item to cart | invalid storeID");
                return Json.Create_json_response(false, new StoreIDDoesntExistException());
            }
            if (!newMarketFacade.ItemExistInStore(storeID, itemID))
            {
                Logger.writeEvent("Remove item to cart | invalid itemID");
                return Json.Create_json_response(false, new ItemDoesntExistException());
            }
            return newMarketFacade.RemoveFromCart(sessionID, storeID, itemID);
        }

        public string ChangeItemAmountInCart(Guid sessionID, Guid storeID, Guid itemID, int amount)//UC 2.7
        {
            if (amount <= 0 || sessionID == null || sessionID.Equals(Guid.Empty))
            {
                Logger.writeEvent("Invalid input while changing item's amount in cart");
                return Json.Create_json_response(false, new InvalidInputException());
            }
            if (!newMarketFacade.StoreExist(storeID))
            {
                Logger.writeEvent("Change item amount in cart | invalid storeID");
                return Json.Create_json_response(false, new StoreDoesntExistException());
            }
            if (!newMarketFacade.ItemExistInStore(storeID, itemID))
            {
                Logger.writeEvent("Change item amount in cart | invalid itemID");
                return Json.Create_json_response(false, new ItemDoesntExistException());
            }
            if (!newMarketFacade.CheckSufficentAmountInInventory(storeID, itemID, amount))
            {
                Logger.writeEvent("Change item amount in cart | not enough amount of items in inventory");
                return Json.Create_json_response(false, new InsufficientItemAmountException());
            }
            return newMarketFacade.ChangeItemAmountInCart(sessionID, storeID, itemID, amount);
        }

        public string GetMyMessages(Guid sessionID)
        {
            if (sessionID == null || sessionID.Equals(Guid.Empty))
            {
                Logger.writeEvent("Invalid input while getting the user messages");
                return Json.Create_json_response(false, new InvalidInputException());
            }
            if (!newMarketFacade.IsloggedIn(sessionID))
            {
                Logger.writeEvent("GetMyMessages failed | not logged in");
                return Json.Create_json_response(false, new LoggedOutException());
            }
            return newMarketFacade.GetMyMessages(sessionID);
        }

        public string HasMessages(Guid sessionID)
        {
            if (sessionID == null || sessionID.Equals(Guid.Empty))
            {
                Logger.writeEvent("Invalid input while checking if the user has messages");
                return Json.Create_json_response(false, new InvalidInputException());
            }
            if (!newMarketFacade.IsloggedIn(sessionID))
            {
                Logger.writeEvent("HasMessages failed | not logged in");
                return Json.Create_json_response(false, new LoggedOutException());
            }
            return newMarketFacade.HasMessages(sessionID);
        }

        public string IsAdmin(Guid sessionID)
        {
            if (sessionID == null || sessionID.Equals(Guid.Empty))
            {
                Logger.writeEvent("Invalid input while checking if the user is admin");
                return Json.Create_json_response(false, new InvalidInputException());
            }
            if (!newMarketFacade.IsAdmin(sessionID))
            {
                Logger.writeEvent("IsAdmin failed | not admin");
                return Json.Create_json_response(false, new NotAdminException());
            }
            return Json.Create_json_response(true, true);
        }

        public string GetDailyStatistics(Guid sessionID, string from, string? to)// UC 6.4
        {
            if (sessionID == null || sessionID.Equals(Guid.Empty) || string.IsNullOrWhiteSpace(from) || 
                !DateTime.TryParseExact(from, "MM/dd/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime dateFrom) || 
                dateFrom.CompareTo(DateTime.Now) > 0)
            {
                Logger.writeEvent("Invalid input while getting daily usage statistics");
                return Json.Create_json_response(false, new InvalidInputException());
            }

            DateTime dateTo;
            if (!string.IsNullOrWhiteSpace(to))
            {
                if (!DateTime.TryParseExact(to, "MM/dd/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out dateTo) ||
                    dateTo.CompareTo(DateTime.Now) > 0 || 
                    dateTo.CompareTo(dateFrom) < 0)
                {
                    Logger.writeEvent("Invalid input while getting daily usage statistics");
                    return Json.Create_json_response(false, new InvalidInputException());
                }
            }
            else
            {
                dateTo = dateFrom;
            }

            if (!newMarketFacade.IsloggedIn(sessionID))
            {
                Logger.writeEvent("GetDailyStatistics failed | not logged in");
                return Json.Create_json_response(false, new LoggedOutException());
            }
            if (!newMarketFacade.IsAdmin(sessionID))
            {
                Logger.writeEvent("GetDailyStatistics failed | Not admin");
                return Json.Create_json_response(false, new NotAdminException());
            }
            return newMarketFacade.GetDailyStatistics(sessionID, dateFrom, dateTo);
        }
    }
}

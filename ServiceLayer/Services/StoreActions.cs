using CustomLogger;
using DomainLayer.Market;
using ServiceLayer.Exceptions;
using System;
using System.Collections.Generic;

namespace ServiceLayer.Services
{
    public class StoreActions
    {
        //PERMISSION_CONSTANTS
        private readonly string INVENTORY_PERMISSION = "inventory";
        private readonly string APPOINT_OWNER = "appoint_owner";
        private readonly string APPOINT_MANAGER = "appoint_manager";
        private readonly string EDIT_PERMISSIONS = "edit_permissions";
        private readonly string REMOVE_MANAGER = "remove_manager";
        private readonly string STORE_HISTORY = "history";
        private readonly string REMOVE_OWNER = "remove_owner";
        private readonly string POLICY_PERMISSION = "policy";

        private readonly List<string> allowed_ops = new List<string> { "&", "|", "xor", ">" };
        private readonly List<string> allowed_DiscountType_strings =
            new List<string> { "opened", "item_conditional", "store_conditional", "composite" };
        private readonly List<string> allowed_PurchaePolicyType_strings =
            new List<string> { "item", "store", "days", "composite" };
        private readonly List<string> allowedPermissionStrings = new List<string>
            {"inventory","appoint_owner", "appoint_manager","edit_permissions","remove_manager","history","remove_owner","policy","close_store","requests"};

        private readonly IMarketFacade newMarketFacade;
        private readonly JsonResponse Json;

        public StoreActions( IMarketFacade newMarketFacade)
        {
            this.newMarketFacade = newMarketFacade;
            Json = new JsonResponse();
        }

        // UC 3.2
        /// <summary>
        /// open store
        /// </summary>
        /// <param name="sessionID"></param>
        /// <param name="name"></param>
        /// <param name="email"></param>
        /// <param name="address"></param>
        /// <param name="phone"></param>
        /// <param name="bankAccountNumber"></param>
        /// <param name="bank"></param>
        /// <param name="description"></param>
        /// <param name="purchasePolicy"></param>
        /// <param name="discountPolicy"></param>
        /// <returns>jsonType: Store</returns>
        public string OpenStore(Guid sessionID,
            string name,
            string email,
            string address,
            string phone,
            string bankAccountNumber,
            string bank,
            string description,
            string purchasePolicy,
            string discountPolicy)
        {
            if (sessionID == null || sessionID.Equals(Guid.Empty) ||
                string.IsNullOrWhiteSpace(name) ||
                string.IsNullOrWhiteSpace(email) ||
                string.IsNullOrWhiteSpace(address) ||
                string.IsNullOrWhiteSpace(phone) ||
                string.IsNullOrWhiteSpace(bankAccountNumber) ||
                string.IsNullOrWhiteSpace(bank) ||
                string.IsNullOrWhiteSpace(description )
                /*add check to policies in future*/)
            {
                Logger.writeEvent("Invalid input while opening a store");
                return Json.Create_json_response(false, new InvalidInputException());
            }
            if (!newMarketFacade.IsloggedIn(sessionID))
            {
                Logger.writeEvent("Open store failed | not logged in");
                return Json.Create_json_response(false, new LoggedOutException());
            }
            if (newMarketFacade.StoreExistByName(name))
            {
                Logger.writeEvent("Open store failed | store name already exist");
                return Json.Create_json_response(false, new StoreNameTakenException());
            }
            return newMarketFacade.OpenStore(
                                 sessionID, 
                                 name,
                                 email,
                                 address,
                                 phone,
                                 bankAccountNumber,
                                 bank,
                                 description,
                                 purchasePolicy,
                                 discountPolicy);
        }

        // UC 2.4
        /// <summary>
        /// Returns all the stores with their items
        /// </summary>
        /// <param name="sessionID"></param>
        /// <returns>JsonType: which includes store + items list</returns>
        public string GetAllStoresInformation(Guid sessionID)
        {
            if (sessionID == null || sessionID.Equals(Guid.Empty))
            {
                Logger.writeEvent("Invalid input while getting all the stores information");
                return Json.Create_json_response(false, new InvalidInputException());
            }

            return newMarketFacade.GetAllStoresInformation(sessionID);
        }

        // UC 2.4
        /// <summary>
        /// Returns single store with items by id
        /// </summary>
        /// <param name="sessionID"></param>
        /// <returns>JsonType: which includes store + items list</returns>
        public string GetStoreInformationByID(Guid sessionID, Guid storeID)
        {
            if (sessionID == null || sessionID.Equals(Guid.Empty) ||
                storeID == null || storeID.Equals(Guid.Empty))
            {
                Logger.writeEvent("Invalid input while getting store information by id");
                return Json.Create_json_response(false, new InvalidInputException());
            }

            if (!newMarketFacade.StoreExist(storeID))
            {
                Logger.writeEvent("GetStoreInformationByID failed | Store doesnt exist");
                return Json.Create_json_response(false, new StoreDoesntExistException());
            }

            return newMarketFacade.GetStoreInformationByID(sessionID, storeID);
        }

        // UC 2.4
        /// <summary>
        /// Returns All stores that user has managing rights over, with the list of permissions.
        /// </summary>
        /// <param name="sessionID"></param>
        /// <returns>JsonType: List<Tuple<Store,List<string>(perms)>/returns>
        public string GetStoresWithPermissions(Guid sessionID)
        {
            if (sessionID == null || sessionID.Equals(Guid.Empty) )
            {
                Logger.writeEvent("Invalid input while GetStoresWithPermission");
                return Json.Create_json_response(false, new InvalidInputException());
            }
            if (!newMarketFacade.IsloggedIn(sessionID))
            {
                Logger.writeEvent("GetStoresWithPermission failed | not logged in");
                return Json.Create_json_response(false, new LoggedOutException());
            }


            return newMarketFacade.GetStoresWithPermissions(sessionID);
        }

        // UC 2.5
        /// <summary>
        /// /// Search for items in store by the "And" of following params: 
        /// name (if provided) &
        /// category (if provided) &
        /// keywords (if provided) &
        /// standing in filters (if provided)
        /// </summary>
        /// <param name="sessionID"></param>
        /// <param name="filterItemRank"></param>
        /// <param name="filterMinPrice"></param>
        /// <param name="filterMaxPrice"></param>
        /// <param name="filterStoreRank"></param>
        /// <param name="name"></param>
        /// <param name="category"></param>
        /// <param name="keywords">jsonType: list</param>
        /// <param name="filterCategory"></param>
        /// <returns>jsonType: dictionary of <storeID, collection of items> if there are, otherwise empty dictionary</returns>
        public string SearchItems(Guid sessionID, double? filterItemRank, double? filterMinPrice, double? filterMaxPrice, double? filterStoreRank, string name = null, string category = null, string keywords = null)
        {
            if (sessionID == null || sessionID.Equals(Guid.Empty) ||
                (filterItemRank is double itemRank && ( itemRank < 0 || itemRank > 10)) ||
                (filterMinPrice is double minPrice && (minPrice < 0)) ||
                (filterMaxPrice is double maxPrice && (maxPrice < 0)) ||
                (filterStoreRank is double storeRank && (storeRank < 0 || storeRank > 10)) ||
                (name != null && name.Length == 0) ||
                (category != null && category.Length == 0) ||
                (keywords != null && keywords.Length == 0))
            {
                Logger.writeEvent("Invalid input while searching items");
                return Json.Create_json_response(false, new InvalidInputException());
            }

            return newMarketFacade.SearchItems(sessionID, filterItemRank, filterMinPrice, filterMaxPrice, filterStoreRank, name, category, keywords);
        }

        // UC 4.1
        /// <summary>
        /// Add Item
        /// </summary>
        /// <param name="sessionID"></param>
        /// <param name="storeID"></param>
        /// <param name="name"></param>
        /// <param name="amount"></param>
        /// <param name="categories">array of categories as json</param>
        /// <param name="price"></param>
        /// <param name="keyWords">NOT mendatory: array of keyWords as json</param>
        /// <returns>jsonType: Item's fields</returns>
        public string AddItem(Guid sessionID, Guid storeID, string name, int amount, string categories, double price, string keyWords = null)
        {
            if(sessionID == null || sessionID.Equals(Guid.Empty) ||
                storeID == null || storeID.Equals(Guid.Empty) ||
                string.IsNullOrWhiteSpace(name) ||
                (amount < 0) ||
                string.IsNullOrWhiteSpace(categories) ||
                (price < 0) ||
                (keyWords != null && keyWords.Length == 0))
            {
                Logger.writeEvent("Invalid input while adding an item");
                return Json.Create_json_response(false, new InvalidInputException());
            }
            if (!newMarketFacade.IsloggedIn(sessionID))
            {
                Logger.writeEvent("Add item failed | not logged in");
                return Json.Create_json_response(false, new LoggedOutException());
            }
            if (!newMarketFacade.StoreExist(storeID))
            {
                Logger.writeEvent("Add item failed | Store doesnt exist");
                return Json.Create_json_response(false, new StoreDoesntExistException());
            }
            if(!newMarketFacade.IsPermitedOperation(sessionID, storeID, INVENTORY_PERMISSION))
            {
                Logger.writeEvent("Add item failed | no permission");
                return Json.Create_json_response(false, new PermissionException());
            }

            return newMarketFacade.AddItem(sessionID, storeID, name, amount, price, categories, keyWords);
        }
        
        // UC 4.1
        public string DeleteItem(Guid sessionID, Guid storeID, Guid itemId)
        {
            if (sessionID == null || sessionID.Equals(Guid.Empty) ||
                storeID == null || storeID.Equals(Guid.Empty) ||
                itemId == null || itemId.Equals(Guid.Empty))
            {
                Logger.writeEvent("Invalid input while deleting an item");
                return Json.Create_json_response(false, new InvalidInputException());
            }
            if (!newMarketFacade.IsloggedIn(sessionID))
            {
                Logger.writeEvent("Delete item failed | not logged in");
                return Json.Create_json_response(false, new LoggedOutException());
            }
            if (!newMarketFacade.StoreExist(storeID))
            {
                Logger.writeEvent("Delete item failed | Store doesnt exist");
                return Json.Create_json_response(false, new StoreDoesntExistException());
            }
            if (!newMarketFacade.ItemExistInStore(storeID, itemId))
            {
                Logger.writeEvent("Delete item failed | item doesnt exist");
                return Json.Create_json_response(false, new ItemDoesntExistException());
            }
            if (!newMarketFacade.IsPermitedOperation(sessionID, storeID, INVENTORY_PERMISSION))
            {
                Logger.writeEvent("Delete item failed | no permission");
                return Json.Create_json_response(false, new PermissionException());
            }

            return newMarketFacade.DeleteItem(sessionID, storeID, itemId);
        }

        // UC 4.1
        /// <summary>
        /// Edit Item's details: not all details mandatory.
        /// Optional params should be  written null if decided not to be passed.
        /// Params that have default value null in signature will get null if not provided
        /// </summary>
        /// <param name="sessionID"></param>
        /// <param name="storeID"></param>
        /// <param name="itemId"></param>
        /// <param name="amount"></param>
        /// <param name="rank"></param>
        /// <param name="price"></param>
        /// <param name="name"></param>
        /// <param name="categories"></param>
        /// <param name="keyWords"></param>
        /// <returns>jsonType: true/false</returns>
        public string EditItem(
            Guid sessionID,
            Guid storeID,
            Guid itemId,
            int? amount,
            double? rank,
            double? price,
            string name = null,
            string categories = null,
            string keyWords = null)
        {
            if (sessionID == null || sessionID.Equals(Guid.Empty) ||
                storeID == null || storeID.Equals(Guid.Empty) ||
                itemId == null || itemId.Equals(Guid.Empty) ||
                (amount is int am && (am < 0)) ||
                (rank is double ra && (ra < 0)) ||
                (price is double pr && (pr < 0)) ||
                (name != null && (name.Length == 0)) ||
                (categories != null && (categories.Length == 0)) ||
                (keyWords != null && (keyWords.Length == 0)))
            {
                Logger.writeEvent("Invalid input while editing an item");
                return Json.Create_json_response(false, new InvalidInputException());
            }

            if (!newMarketFacade.IsloggedIn(sessionID))
            {
                Logger.writeEvent("Edit item failed | not logged in");
                return Json.Create_json_response(false, new LoggedOutException());
            }
            if (!newMarketFacade.StoreExist(storeID))
            {
                Logger.writeEvent("Edit item failed | Store doesnt exist");
                return Json.Create_json_response(false, new StoreDoesntExistException());
            }
            if (!newMarketFacade.ItemExistInStore(storeID, itemId))
            {
                Logger.writeEvent("Edit item failed | item doesnt exist");
                return Json.Create_json_response(false, new ItemDoesntExistException());
            }
            if (!newMarketFacade.IsPermitedOperation(sessionID, storeID, INVENTORY_PERMISSION))
            {
                Logger.writeEvent("Edit item failed | no permission");
                return Json.Create_json_response(false, new PermissionException());
            }


            return newMarketFacade.EditItem(sessionID, storeID, itemId, amount, rank, price, name, categories, keyWords);
        }

        // UC 4.3
        public string AppointOwner(Guid sessionID, Guid storeID, string username)
        {
            if (sessionID == null || sessionID.Equals(Guid.Empty) ||
                storeID == null || storeID.Equals(Guid.Empty) ||
                string.IsNullOrWhiteSpace(username))
            {
                Logger.writeEvent("Invalid input while appointing an owner to a store");
                return Json.Create_json_response(false, new InvalidInputException());
            }
            if (!newMarketFacade.IsloggedIn(sessionID))
            {
                Logger.writeEvent("AppointOwner failed | not logged in");
                return Json.Create_json_response(false, new LoggedOutException());
            }
            if (!newMarketFacade.StoreExist(storeID))
            {
                Logger.writeEvent("AppointOwner failed | Store doesnt exist");
                return Json.Create_json_response(false, new StoreDoesntExistException());
            }
            
            if (!newMarketFacade.IsPermitedOperation(sessionID, storeID, APPOINT_OWNER))
            {
                Logger.writeEvent("AppointOwner failed | no permission");
                return Json.Create_json_response(false, new PermissionException());
            }
            if (!newMarketFacade.IsRegisteredUser(username))
            {
                Logger.writeEvent("AppointOwner failed | not a registered user");
                return Json.Create_json_response(false, new NotRegisterdException());
            }
            if (newMarketFacade.IsOwnerOfStore(storeID, username))
            {
                Logger.writeEvent("AppointOwner failed | already an owner");
                return Json.Create_json_response(false, new AlreadyOwnerException());
            }

            return newMarketFacade.AppointOwner(sessionID, storeID, username);
        }

        // UC 4.3
        public string ApproveOwnerContract(Guid sessionID, Guid storeID, string username)
        {
            if (sessionID == null || sessionID.Equals(Guid.Empty) ||
                storeID == null || storeID.Equals(Guid.Empty) ||
                string.IsNullOrWhiteSpace(username))
            {
                Logger.writeEvent("Invalid input while approving owner contract");
                return Json.Create_json_response(false, new InvalidInputException());
            }
            if (!newMarketFacade.IsloggedIn(sessionID))
            {
                Logger.writeEvent("ApproveOwnerContract failed | not logged in");
                return Json.Create_json_response(false, new LoggedOutException());
            }
            if (!newMarketFacade.StoreExist(storeID))
            {
                Logger.writeEvent("ApproveOwnerContract failed | Store doesnt exist");
                return Json.Create_json_response(false, new StoreDoesntExistException());
            }

            if (!newMarketFacade.IsPermitedOperation(sessionID, storeID, APPOINT_OWNER))
            {
                Logger.writeEvent("ApproveOwnerContract failed | no permission");
                return Json.Create_json_response(false, new PermissionException());
            }
            if (!newMarketFacade.IsRegisteredUser(username))
            {
                Logger.writeEvent("ApproveOwnerContract failed | not a registered user");
                return Json.Create_json_response(false, new NotRegisterdException());
            }
            if (!newMarketFacade.IsAwaitingContractApproval(storeID, username))
            {
                Logger.writeEvent("ApproveOwnerContract failed | not a awaitingContractApproval");
                return Json.Create_json_response(false, new NotAwaitingContractException());
            }
            if (!newMarketFacade.IsApproverOfContract(sessionID ,storeID, username))
            {
                Logger.writeEvent("ApproveOwnerContract failed | not a Approver of contract");
                return Json.Create_json_response(false, new NotApproverException());
            }


            return newMarketFacade.ApproveOwnerContract(sessionID, storeID, username);
        }

        //private bool IsRegisteredUser(string username)
        //{
        //    Guid id = newMarketFacade.GetUserIDByName(username);
        //    return !id.Equals(Guid.Empty);
        //}

        //UC 4.4

        /// <summary>
        /// RemoveOwner: For Next Version
        /// </summary>
        /// <param name="sessionID"></param>
        /// <param name="storeID"></param>
        /// <param name="username"></param>
        /// <returns></returns>
        public string RemoveOwner(Guid sessionID, Guid storeID, string username) 
        {
            if (sessionID == null || sessionID.Equals(Guid.Empty) ||
                storeID == null || storeID.Equals(Guid.Empty) ||
                string.IsNullOrWhiteSpace(username))
            {
                Logger.writeEvent("Invalid input while removing an owner from a store");
                return Json.Create_json_response(false, new InvalidInputException());
            }
            if (!newMarketFacade.IsloggedIn(sessionID))
            {
                Logger.writeEvent("RemoveOwner failed | not logged in");
                return Json.Create_json_response(false, new LoggedOutException());
            }
            if (!newMarketFacade.StoreExist(storeID))
            {
                Logger.writeEvent("RemoveOwner failed | Store doesnt exist");
                return Json.Create_json_response(false, new StoreDoesntExistException());
            }

            if (!newMarketFacade.IsPermitedOperation(sessionID, storeID, REMOVE_OWNER))
            {
                Logger.writeEvent("RemoveOwner failed | no permission");
                return Json.Create_json_response(false, new PermissionException());
            }
            if (!newMarketFacade.IsRegisteredUser(username))
            {
                Logger.writeEvent("RemoveOwner failed | not a registered user");
                return Json.Create_json_response(false, new NotRegisterdException());
            }
            if (!newMarketFacade.IsOwnerOfStore(storeID, username))
            {
                Logger.writeEvent("RemoveOwner failed | not an owner");
                return Json.Create_json_response(false, new NotOwnerException());
            }

            return newMarketFacade.RemoveOwner(sessionID, storeID, username);
        }

        // UC 4.5
        public string AppointManager(Guid sessionID, Guid storeID, string username)
        {
            if (sessionID == null || sessionID.Equals(Guid.Empty) ||
               storeID == null || storeID.Equals(Guid.Empty) ||
               string.IsNullOrWhiteSpace(username))
            {
                Logger.writeEvent("Invalid input while appointing a manager to a store");
                return Json.Create_json_response(false, new InvalidInputException());
            }
            if (!newMarketFacade.IsloggedIn(sessionID))
            {
                Logger.writeEvent("AppointManager failed | not logged in");
                return Json.Create_json_response(false, new LoggedOutException());
            }
            if (!newMarketFacade.StoreExist(storeID))
            {
                Logger.writeEvent("AppointManager failed | Store doesnt exist");
                return Json.Create_json_response(false, new StoreDoesntExistException());
            }

            if (!newMarketFacade.IsPermitedOperation(sessionID, storeID, APPOINT_MANAGER))
            {
                Logger.writeEvent("AppointManager failed | no permission");
                return Json.Create_json_response(false, new PermissionException());
            }
            if (!newMarketFacade.IsRegisteredUser(username))
            {
                Logger.writeEvent("AppointManager failed | not a registered user");
                return Json.Create_json_response(false, new NotRegisterdException());
            }
            if (newMarketFacade.IsPermitedOperation(username, storeID, APPOINT_MANAGER))
            {
                Logger.writeEvent("AppointManager failed | already a manager");
                return Json.Create_json_response(false, new AlreadyManagerException());
            }

            return newMarketFacade.AppointManager(sessionID, storeID, username);
        }

        //UC 4.6
        /// <summary>
        /// Add Permission
        /// Permissions options:
        /// INVENTORY, POLICY, APPOINT_OWNER, REMOVE_OWNER, APPOINT_MANAGER, EDIT_PERMISSIONS, REMOVE_MANAGER,CLOSE_STORE, REQUESTS, HISTORY
        /// </summary>
        /// <param name="sessionID"></param>
        /// <param name="storeID"></param>
        /// <param name="username"></param>
        /// <param name="permission"></param>
        /// <returns>jsonType: true/false</returns>
        public string AddPermission(Guid sessionID, Guid storeID, string username, string permission)
        {
            if (sessionID == null || sessionID.Equals(Guid.Empty) ||
                storeID == null || storeID.Equals(Guid.Empty) ||
                string.IsNullOrWhiteSpace(username) ||
                string.IsNullOrWhiteSpace(permission))
            {
                Logger.writeEvent("Invalid input while adding permission");
                return Json.Create_json_response(false, new InvalidInputException());
            }
            if (!allowedPermissionStrings.Contains(permission.ToLower().Trim()))
            {
                Logger.writeEvent("Invalid input while adding permission");
                return Json.Create_json_response(false, new InvalidInputException());
            }
            if (!newMarketFacade.IsloggedIn(sessionID))
            {
                Logger.writeEvent("AddPermission failed | not logged in");
                return Json.Create_json_response(false, new LoggedOutException());
            }
            if (!newMarketFacade.StoreExist(storeID))
            {
                Logger.writeEvent("AddPermission failed | Store doesnt exist");
                return Json.Create_json_response(false, new StoreDoesntExistException());
            }

            if (!newMarketFacade.IsPermitedOperation(sessionID, storeID, EDIT_PERMISSIONS))
            {
                Logger.writeEvent("AddPermission failed | no permission");
                return Json.Create_json_response(false, new PermissionException());
            }
            if (!newMarketFacade.IsRegisteredUser(username))
            {
                Logger.writeEvent("AddPermission failed | not a registered user");
                return Json.Create_json_response(false, new NotRegisterdException());
            }
            if (newMarketFacade.IsPermitedOperation(username, storeID, permission))
            {
                Logger.writeEvent("AddPermission failed | already has this permission");
                return Json.Create_json_response(false, new ExistingPermissionException());
            }

            return newMarketFacade.AddPermission(sessionID, storeID, username, permission);
        }

        // UC 4.6
        /// <summary>
        /// Remove Permission
        /// Permissions options:
        /// INVENTORY, POLICY, APPOINT_OWNER, REMOVE_OWNER, APPOINT_MANAGER, EDIT_PERMISSIONS, REMOVE_MANAGER,CLOSE_STORE, REQUESTS, HISTORY 
        /// </summary>
        /// <param name="sessionID"></param>
        /// <param name="storeID"></param>
        /// <param name="username"></param>
        /// <param name="permission"></param>
        /// <returns>jsonType: true/false</returns>
        public string RemovePermission(Guid sessionID, Guid storeID, string username, string permission)
        {
            if (sessionID == null || sessionID.Equals(Guid.Empty) ||
                storeID == null || storeID.Equals(Guid.Empty) ||
                string.IsNullOrWhiteSpace(username) ||
                string.IsNullOrWhiteSpace(permission))
            {
                Logger.writeEvent("Invalid input while removing permission");
                return Json.Create_json_response(false, new InvalidInputException());
            }
            if (!allowedPermissionStrings.Contains(permission.ToLower().Trim()))
            {
                Logger.writeEvent("Invalid input while removing permission");
                return Json.Create_json_response(false, new InvalidInputException());
            }
            if (!newMarketFacade.IsloggedIn(sessionID))
            {
                Logger.writeEvent("RemovePermission failed | not logged in");
                return Json.Create_json_response(false, new LoggedOutException());
            }
            if (!newMarketFacade.StoreExist(storeID))
            {
                Logger.writeEvent("RemovePermission failed | Store doesnt exist");
                return Json.Create_json_response(false, new StoreDoesntExistException());
            }

            if (!newMarketFacade.IsPermitedOperation(sessionID, storeID, EDIT_PERMISSIONS))
            {
                Logger.writeEvent("RemovePermission failed | no permission");
                return Json.Create_json_response(false, new PermissionException());
            }
            if (!newMarketFacade.IsRegisteredUser(username))
            {
                Logger.writeEvent("RemovePermission failed | not a registered user");
                return Json.Create_json_response(false, new NotRegisterdException());
            }
            if (!newMarketFacade.IsPermitedOperation(username, storeID, permission))
            {
                Logger.writeEvent("RemovePermission failed | doesnt have this permission");
                return Json.Create_json_response(false, new MissingPermissionException());
            }

            return newMarketFacade.RemovePermission(sessionID, storeID, username, permission);
        }

        /// <summary>
        /// Get permmision of a user
        /// </summary>
        /// <param name="sessionID"></param>
        /// <param name="storeID"></param>
        /// <returns>jsonType: list of permission</returns>
        public string GetMyPermissions(Guid sessionID, Guid storeID)
        {
            if (sessionID == null || sessionID.Equals(Guid.Empty) ||
                  storeID == null || storeID.Equals(Guid.Empty))
            {
                Logger.writeEvent("Invalid input while getting the user permission");
                return Json.Create_json_response(false, new InvalidInputException());
            }
            if (!newMarketFacade.IsloggedIn(sessionID))
            {
                Logger.writeEvent("GetMyPermissions failed | not logged in");
                return Json.Create_json_response(false, new LoggedOutException());
            }
            if (!newMarketFacade.StoreExist(storeID))
            {
                Logger.writeEvent("GetMyPermissions failed | Store doesnt exist");
                return Json.Create_json_response(false, new StoreDoesntExistException());
            }

            return newMarketFacade.GetMyPermissions(sessionID, storeID);
        }

        // UC 4.7
        public string RemoveManager(Guid sessionID, Guid storeID, string username)
        {
            if (sessionID == null || sessionID.Equals(Guid.Empty) ||
                storeID == null || storeID.Equals(Guid.Empty) ||
                string.IsNullOrWhiteSpace(username))
            {
                Logger.writeEvent("Invalid input while removing a manager from a store");
                return Json.Create_json_response(false, new InvalidInputException());
            }
            if (!newMarketFacade.IsloggedIn(sessionID))
            {
                Logger.writeEvent("RemoveManager failed | not logged in");
                return Json.Create_json_response(false, new LoggedOutException());
            }
            if (!newMarketFacade.StoreExist(storeID))
            {
                Logger.writeEvent("RemoveManager failed | Store doesnt exist");
                return Json.Create_json_response(false, new StoreDoesntExistException());
            }

            if (!newMarketFacade.IsPermitedOperation(sessionID, storeID, REMOVE_MANAGER))
            {
                Logger.writeEvent("RemoveManager failed | no permission");
                return Json.Create_json_response(false, new PermissionException());
            }
            if (!newMarketFacade.IsRegisteredUser(username))
            {
                Logger.writeEvent("RemoveManager failed | not a registered user");
                return Json.Create_json_response(false, new NotRegisterdException());
            }
            if (!newMarketFacade.IsManagerOfStore(username, storeID))
            {
                Logger.writeEvent("RemoveManager failed | not a manager");
                return Json.Create_json_response(false, new NotManagerException());
            }

            if (!newMarketFacade.IsGrantorOf(storeID, sessionID, username))
            {
                Logger.writeEvent("RemoveManager failed | not grantor");
                return Json.Create_json_response(false, new NotGrantorException());
            }

            return newMarketFacade.RemoveManager(sessionID, storeID, username);
        }

        public string GetStoreOrderHistory(Guid sessionID, Guid storeID)// UC 4.10
        {
            if (sessionID == null || sessionID.Equals(Guid.Empty) ||
                storeID == null || storeID.Equals(Guid.Empty))
            {
                Logger.writeEvent("Invalid input while getting the store\'s order history");
                return Json.Create_json_response(false, new InvalidInputException());
            }
            if (!newMarketFacade.IsloggedIn(sessionID))
            {
                Logger.writeEvent("GetStoreOrderHistory failed | not logged in");
                return Json.Create_json_response(false, new LoggedOutException());
            }
            if (!newMarketFacade.StoreExist(storeID))
            {
                Logger.writeEvent("GetStoreOrderHistory failed | Store doesnt exist");
                return Json.Create_json_response(false, new StoreDoesntExistException());
            }

            if (!newMarketFacade.IsPermitedOperation(sessionID, storeID, STORE_HISTORY))
            {
                Logger.writeEvent("GetStoreOrderHistory failed | no permission");
                return Json.Create_json_response(false, new PermissionException());
            }

            return newMarketFacade.GetStoreOrderHistory(sessionID, storeID);
        }

        public string GetStoreOrderHistoryAdmin(Guid sessionID, Guid storeID)// UC 6.4
        {
            if (sessionID == null || sessionID.Equals(Guid.Empty) ||
                storeID == null || storeID.Equals(Guid.Empty))
            {
                Logger.writeEvent("Invalid input while getting the store\'s order history by the system admin");
                return Json.Create_json_response(false, new InvalidInputException());
            }
            if (!newMarketFacade.IsloggedIn(sessionID))
            {
                Logger.writeEvent("GetStoreOrderHistoryAdmin failed | not logged in");
                return Json.Create_json_response(false, new LoggedOutException());
            }
            if (!newMarketFacade.StoreExist(storeID))
            {
                Logger.writeEvent("GetStoreOrderHistoryAdmin failed | Store doesnt exist");
                return Json.Create_json_response(false, new StoreDoesntExistException());
            }

            if (!newMarketFacade.IsAdmin(sessionID))
            {
                Logger.writeEvent("GetStoreOrderHistoryAdmin failed | Not admin");
                return Json.Create_json_response(false, new NotAdminException());
            }

            return newMarketFacade.GetStoreOrderHistoryAdmin(sessionID, storeID);
        }

        /// <summary>
        /// only works if right permissions
        /// </summary>
        /// <param name="sessionID"></param>
        /// <param name="storeID"></param>
        /// <param name="itemID"></param>
        /// <param name="discount"> the percentage of the discount if 30 then price will be 30% under base.</param>
        /// <param name="durationInDays"> duration of the discount from creation date </param>
        /// <returns>json: openDiscount item (fields include: Guid discountID, Guid storeID, Guid itemID, Date dateUntil,
        /// double discount)</returns>
        public string AddOpenDiscount(Guid sessionID, Guid storeID, Guid itemID, double discount, int durationInDays)
        {
            if (sessionID == null || sessionID.Equals(Guid.Empty) ||
                storeID == null || storeID.Equals(Guid.Empty) ||
                itemID == null || itemID.Equals(Guid.Empty) ||
                discount>1 || discount<0 || durationInDays<0)
            {
                Logger.writeEvent("Invalid input while AddOpenDiscount");
                return Json.Create_json_response(false, new InvalidInputException());
            }
            if (!newMarketFacade.IsloggedIn(sessionID))
            {
                Logger.writeEvent("AddOpenDiscount failed | not logged in");
                return Json.Create_json_response(false, new LoggedOutException());
            }
            if (!newMarketFacade.StoreExist(storeID))
            {
                Logger.writeEvent("AddOpenDiscount failed | Store doesnt exist");
                return Json.Create_json_response(false, new StoreDoesntExistException());
            }
            if (!newMarketFacade.ItemExistInStore(storeID, itemID))
            {
                Logger.writeEvent("AddOpenDiscount failed | item doesnt exist");
                return Json.Create_json_response(false, new ItemDoesntExistException());
            }
            if (!newMarketFacade.IsPermitedOperation(sessionID, storeID, POLICY_PERMISSION))
            {
                Logger.writeEvent("AddOpenDiscount failed | no permission");
                return Json.Create_json_response(false, new PermissionException());
            }
            if (!newMarketFacade.IsValidToCreateItemOpenedDiscount( storeID, itemID))
            {
                Logger.writeEvent("AddOpenDiscount failed | Discount not valid - overlapping with exsisting");
                return Json.Create_json_response(false, new OverlappingDiscountException());
            }

            return newMarketFacade.AddOpenDiscount(sessionID, storeID, itemID, discount, durationInDays);

        }

        /// <summary>
        /// for example: 5% discount on 3 bisli (itemId= bisli.ID, minItems = 3, discount = 0.05)
        /// </summary>
        /// <param name="sessionID"></param>
        /// <param name="storeID"></param>
        /// <param name="itemID"></param>
        /// <param name="durationInDays"> duration of the discount from creation date </param>
        /// <param name="minItems">The minimum to buy for the discount </param>
        /// <param name="discount">discount on the minimum items.</param>
        /// <returns>Json of this Discount Object including Guid discountID, and all the args as feilds.</returns>
        public string AddItemConditionalDiscount_MinItems_ToDiscountOnAll(Guid sessionID, Guid storeID,
            Guid itemID, int durationInDays, int minItems, double discount)
        {
            if (sessionID == null || sessionID.Equals(Guid.Empty) ||
                storeID == null || storeID.Equals(Guid.Empty) ||
                itemID == null || itemID.Equals(Guid.Empty) ||
                discount > 1 || discount < 0 || durationInDays < 0 || minItems <0)
            {
                Logger.writeEvent("Invalid input while AddItemConditionalDiscount_MinItems_ToDiscountOnAll");
                return Json.Create_json_response(false, new InvalidInputException());
            }
            if (!newMarketFacade.IsloggedIn(sessionID))
            {
                Logger.writeEvent("AddItemConditionalDiscount_MinItems_ToDiscountOnAll failed | not logged in");
                return Json.Create_json_response(false, new LoggedOutException());
            }
            if (!newMarketFacade.StoreExist(storeID))
            {
                Logger.writeEvent("AddItemConditionalDiscount_MinItems_ToDiscountOnAll failed | Store doesnt exist");
                return Json.Create_json_response(false, new StoreDoesntExistException());
            }
            if (!newMarketFacade.ItemExistInStore(storeID, itemID))
            {
                Logger.writeEvent("AddItemConditionalDiscount_MinItems_ToDiscountOnAll failed | item doesnt exist");
                return Json.Create_json_response(false, new ItemDoesntExistException());
            }
            if (!newMarketFacade.IsPermitedOperation(sessionID, storeID, POLICY_PERMISSION))
            {
                Logger.writeEvent("AddItemConditionalDiscount_MinItems_ToDiscountOnAll failed | no permission");
                return Json.Create_json_response(false, new PermissionException());
            }
            if (!newMarketFacade.IsValidToCreateItemConditionalDiscount(storeID, itemID))
            {
                Logger.writeEvent("AddItemConditionalDiscount_MinItems_ToDiscountOnAll failed | Discount not valid - overlapping with exsisting");
                return Json.Create_json_response(false, new OverlappingDiscountException());
            }

            return newMarketFacade.AddItemConditionalDiscount_MinItems_ToDiscountOnAll(sessionID, storeID,
             itemID, durationInDays, minItems, discount);

        }

        /// <summary>
        /// for example: buy 2 bisli get the third free (itemId= bisli.ID, minItems = 2, discount = null, extraItems = 1, discountForExtra = 1)
        /// </summary>
        /// <param name="sessionID"></param>
        /// <param name="storeID"></param>
        /// <param name="itemID"></param>
        /// <param name="durationInDays"> duration of the discount from creation date </param>
        /// <param name="minItems">The minimum to buy for the discount </param>
        /// <param name="extraItems">number of extra items you get discountForExtra discount if bought</param>
        /// <param name="discountForExtra">the discount on the extraItems. evaluated as percentage i.e
        ///  price = base * discount</param>
        /// <returns>Json of this Discount Object including Guid discountID, and all the args as feilds.</returns>
        public string AddItemConditionalDiscount_MinItems_ToDiscountOnExtraItems(Guid sessionID, Guid storeID,
            Guid itemID, int durationInDays, int minItems, int extraItems, double discountForExtra)
        {
            if (sessionID == null || sessionID.Equals(Guid.Empty) ||
                storeID == null || storeID.Equals(Guid.Empty) ||
                itemID == null || itemID.Equals(Guid.Empty) ||
                discountForExtra > 1 || discountForExtra < 0 ||
                durationInDays < 0 || minItems < 0 || extraItems <0)
            {
                Logger.writeEvent("Invalid input while AddItemConditionalDiscount_MinItems_ToDiscountOnExtraItems");
                return Json.Create_json_response(false, new InvalidInputException());
            }
            if (!newMarketFacade.IsloggedIn(sessionID))
            {
                Logger.writeEvent("AddItemConditionalDiscount_MinItems_ToDiscountOnExtraItems failed | not logged in");
                return Json.Create_json_response(false, new LoggedOutException());
            }
            if (!newMarketFacade.StoreExist(storeID))
            {
                Logger.writeEvent("AddItemConditionalDiscount_MinItems_ToDiscountOnExtraItems failed | Store doesnt exist");
                return Json.Create_json_response(false, new StoreDoesntExistException());
            }
            if (!newMarketFacade.ItemExistInStore(storeID, itemID))
            {
                Logger.writeEvent("AddItemConditionalDiscount_MinItems_ToDiscountOnExtraItems failed | item doesnt exist");
                return Json.Create_json_response(false, new ItemDoesntExistException());
            }
            if (!newMarketFacade.IsPermitedOperation(sessionID, storeID, POLICY_PERMISSION))
            {
                Logger.writeEvent("AddItemConditionalDiscount_MinItems_ToDiscountOnExtraItems failed | no permission");
                return Json.Create_json_response(false, new PermissionException());
            }
            if (!newMarketFacade.IsValidToCreateItemConditionalDiscount( storeID, itemID))
            {
                Logger.writeEvent("AddItemConditionalDiscount_MinItems_ToDiscountOnExtraItems failed | Discount not valid - overlapping with exsisting");
                return Json.Create_json_response(false, new OverlappingDiscountException());
            }

            return newMarketFacade.AddItemConditionalDiscount_MinItems_ToDiscountOnExtraItems(sessionID, storeID,
             itemID, durationInDays, minItems, extraItems, discountForExtra);
        }

        /// <summary>
        /// if bought with  minPurchase price from store, get discount on purchase.
        /// for eaxample, 5% discount on purchases of over 200 shekels.
        /// </summary>
        /// <param name="sessionID"></param>
        /// <param name="StoreID"></param>
        /// <param name="durationInDays"> duration of the discount from creation date </param>
        /// <param name="minPurchase">i</param>
        /// <param name="discount"></param>
        /// <returns> Json of this Discount Object including Guid discountID, and all the args as feilds  </returns>

        public string AddStoreConditionalDiscount(Guid sessionID, Guid storeID, int durationInDays, double minPurchase, double discount)
        {
            if (sessionID == null || sessionID.Equals(Guid.Empty) ||
                storeID == null || storeID.Equals(Guid.Empty) ||
                discount > 1 || discount < 0 ||
                durationInDays < 0 || minPurchase < 0)
            {
                Logger.writeEvent("Invalid input while AddStoreConditionalDiscount");
                return Json.Create_json_response(false, new InvalidInputException());
            }
            if (!newMarketFacade.IsloggedIn(sessionID))
            {
                Logger.writeEvent("AddStoreConditionalDiscount failed | not logged in");
                return Json.Create_json_response(false, new LoggedOutException());
            }
            if (!newMarketFacade.StoreExist(storeID))
            {
                Logger.writeEvent("AddStoreConditionalDiscount failed | Store doesnt exist");
                return Json.Create_json_response(false, new StoreDoesntExistException());
            }
            if (!newMarketFacade.IsPermitedOperation(sessionID, storeID, POLICY_PERMISSION))
            {
                Logger.writeEvent("AddStoreConditionalDiscount failed | no permission");
                return Json.Create_json_response(false, new PermissionException());
            }
            if (!newMarketFacade.IsValidToCreateStoreConditionalDiscount( storeID))
            {
                Logger.writeEvent("AddStoreConditionalDiscount failed | Discount not valid - overlapping with exsisting");
                return Json.Create_json_response(false, new OverlappingDiscountException());
            }

            return newMarketFacade.AddStoreConditionalDiscount(sessionID, storeID, durationInDays, minPurchase, discount);
        }

        /// <summary>
        /// for example discount1 'gorer' discount2 (use ">")
        /// for 'or' use | (this or is if any of the terms suffice it will be discounted)
        /// for xor  use 'xor - only one discount will be calculated if any (the larger one).
        /// and - both discounts must suffice for the discount to work.
        /// this function nullifies its sons( meaning discount1, and discount2 wont be valid on there own)
        /// </summary>
        /// <param name="sessionID"></param>
        /// <param name="StoreID"></param>
        /// <param name="discount1ID"></param>
        /// <param name="discount2ID"></param>
        /// <param name="boolOperator">["&","|","xor"]</param>
        /// <returns>json: Adiscount (fields are discountID, left and right ID's, and operator</returns>
        public string ComposeTwoDiscounts(Guid sessionID, Guid storeID, Guid discountLeftID, Guid discountRightID, string boolOperator)
        {
            
            if (sessionID == null || sessionID.Equals(Guid.Empty) ||
                storeID == null || storeID.Equals(Guid.Empty) ||
                discountLeftID == null || discountLeftID.Equals(Guid.Empty) ||
                discountRightID == null || discountRightID.Equals(Guid.Empty) ||
                !allowed_ops.Contains(boolOperator.ToLower().Trim()))
            {
                Logger.writeEvent("Invalid input while ComposeTwoDiscounts");
                return Json.Create_json_response(false, new InvalidInputException());
            }
            if (!newMarketFacade.IsloggedIn(sessionID))
            {
                Logger.writeEvent("ComposeTwoDiscounts failed | not logged in");
                return Json.Create_json_response(false, new LoggedOutException());
            }
            if (!newMarketFacade.StoreExist(storeID))
            {
                Logger.writeEvent("ComposeTwoDiscounts failed | Store doesnt exist");
                return Json.Create_json_response(false, new StoreDoesntExistException());
            }
            if (!newMarketFacade.IsPermitedOperation(sessionID, storeID, POLICY_PERMISSION))
            {
                Logger.writeEvent("ComposeTwoDiscounts failed | no permission");
                return Json.Create_json_response(false, new PermissionException());
            }
            if(!newMarketFacade.DiscountExistsInStore(storeID, discountLeftID) ||
               !newMarketFacade.DiscountExistsInStore(storeID, discountRightID))
            {
                Logger.writeEvent("ComposeTwoDiscounts failed | DiscountID doesnt exist");
                return Json.Create_json_response(false, new DiscountDoesntExistException());
            }

            if (discountLeftID.Equals(discountRightID))
            {
                Logger.writeEvent("ComposeTwoDiscounts failed | Composed discount to itself");
                return Json.Create_json_response(false, new ComposeDiscountWithItselfException());
            }
            return newMarketFacade.ComposeTwoDiscounts(sessionID, storeID, discountLeftID, discountRightID, boolOperator);
        }

        /// <summary>
        /// if its a complex discount, removes all sons aswell.
        /// </summary>
        /// <param name="sessionID"></param>
        /// <param name="storeID"></param>
        /// <param name="discountID"></param>
        /// <returns>json bool</returns>
        public string RemoveDiscount(Guid sessionID, Guid storeID, Guid discountID)
        {
            if (sessionID == null || sessionID.Equals(Guid.Empty) ||
                storeID == null || storeID.Equals(Guid.Empty) ||
                discountID == null || discountID.Equals(Guid.Empty) )
            {
                Logger.writeEvent("Invalid input while RemoveDiscount");
                return Json.Create_json_response(false, new InvalidInputException());
            }
            if (!newMarketFacade.IsloggedIn(sessionID))
            {
                Logger.writeEvent("RemoveDiscount failed | not logged in");
                return Json.Create_json_response(false, new LoggedOutException());
            }
            if (!newMarketFacade.StoreExist(storeID))
            {
                Logger.writeEvent("RemoveDiscount failed | Store doesnt exist");
                return Json.Create_json_response(false, new StoreDoesntExistException());
            }
            if (!newMarketFacade.IsPermitedOperation(sessionID, storeID, POLICY_PERMISSION))
            {
                Logger.writeEvent("RemoveDiscount failed | no permission");
                return Json.Create_json_response(false, new PermissionException());
            }
            if (!newMarketFacade.DiscountExistsInStore(storeID, discountID))
            {
                Logger.writeEvent("RemoveDiscount failed | DiscountID doesnt exist");
                return Json.Create_json_response(false, new DiscountDoesntExistException());
            }

            return newMarketFacade.RemoveDiscount( sessionID,  storeID,  discountID);
        }

        /// <summary>
        /// if itemID!=null then only discounts (complex or simple) with the itemID somewhere will be returned
        /// </summary>
        /// <param name="sessionID"></param>
        /// <param name="storeID"></param>
        /// <param name="itemID"></param>
        /// <returns>List of Discounts (fields not yet determined)</returns>
        public string GetAllDiscounts(Guid sessionID, Guid storeID, Guid? itemID)
        {
            if (sessionID == null || sessionID.Equals(Guid.Empty) ||
                itemID.Equals(Guid.Empty) || Guid.Empty.Equals(itemID) )
            {
                Logger.writeEvent("Invalid input while GetAllDiscounts");
                return Json.Create_json_response(false, new InvalidInputException());
            }
            if (!newMarketFacade.IsloggedIn(sessionID))
            {
                Logger.writeEvent("GetAllDiscounts failed | not logged in");
                return Json.Create_json_response(false, new LoggedOutException());
            }
            if (!newMarketFacade.StoreExist(storeID))
            {
                Logger.writeEvent("GetAllDiscounts failed | Store doesnt exist");
                return Json.Create_json_response(false, new StoreDoesntExistException());
            }
            if (!newMarketFacade.IsPermitedOperation(sessionID, storeID, POLICY_PERMISSION))
            {
                Logger.writeEvent("GetAllDiscounts failed | no permission");
                return Json.Create_json_response(false, new PermissionException());
            }
            if(itemID is Guid iid && !newMarketFacade.ItemExistInStore(storeID, iid))
            {
                Logger.writeEvent("GetAllDiscounts failed | no such item");
                return Json.Create_json_response(false, new ItemDoesntExistException());
            }
            return newMarketFacade.GetAllDicsounts(sessionID, storeID, itemID);

        }
        /// <summary>
        /// discount type string must match a discountType defined in Stores.Discounts.DiscountPolicy
        /// </summary>
        /// <param name="sessionID"></param>
        /// <param name="storeID"></param>
        /// <param name="discountTypeString">legal values are:["opened", "item_conditional", "store_conditional", "composite"]</param>
        /// <returns>json bool</returns>
        public string MakeDiscountNotAllowed(Guid sessionID, Guid storeID, string discountTypeString)
        {
            if (sessionID == null || sessionID.Equals(Guid.Empty) ||
                storeID == null || storeID.Equals(Guid.Empty) ||
                !allowed_DiscountType_strings.Contains(discountTypeString.ToLower().Trim()))
            {
                Logger.writeEvent("Invalid input while MakeDiscountNotAllowed");
                return Json.Create_json_response(false, new InvalidInputException());
            }
            if (!newMarketFacade.IsloggedIn(sessionID))
            {
                Logger.writeEvent("MakeDiscountNotAllowed failed | not logged in");
                return Json.Create_json_response(false, new LoggedOutException());
            }
            if (!newMarketFacade.StoreExist(storeID))
            {
                Logger.writeEvent("MakeDiscountNotAllowed failed | Store doesnt exist");
                return Json.Create_json_response(false, new StoreDoesntExistException());
            }
            if (!newMarketFacade.IsPermitedOperation(sessionID, storeID, POLICY_PERMISSION))
            {
                Logger.writeEvent("MakeDiscountNotAllowed failed | no permission");
                return Json.Create_json_response(false, new PermissionException());
            }
            if (!newMarketFacade.IsDiscountTypeAllowed(storeID, discountTypeString))
            {
                Logger.writeEvent("MakeDiscountNotAllowed failed | DiscountType Already isnt allowed");
                return Json.Create_json_response(false, new DiscountTypeNotAllowedException());
            }

            return newMarketFacade.MakeDiscountNotAllowed(sessionID, storeID, discountTypeString);
        }
        /// <summary>
        /// discount type string must match a discountType defined in Stores.Discounts.DiscountPolicy
        /// </summary>
        /// <param name="sessionID"></param>
        /// <param name="storeID"></param>
        /// <param name="discountTypeString">legal values are:["opened", "item_conditional", "store_conditional", "composite"]</param>
        /// <returns>json bool</returns>
        public string MakeDiscountAllowed(Guid sessionID, Guid storeID, string discountTypeString)
        {
            if (sessionID == null || sessionID.Equals(Guid.Empty) ||
                storeID == null || storeID.Equals(Guid.Empty) ||
                !allowed_DiscountType_strings.Contains(discountTypeString.ToLower().Trim()))
            {
                Logger.writeEvent("Invalid input while MakeDiscountAllowed");
                return Json.Create_json_response(false, new InvalidInputException());
            }
            if (!newMarketFacade.IsloggedIn(sessionID))
            {
                Logger.writeEvent("MakeDiscountAllowed failed | not logged in");
                return Json.Create_json_response(false, new LoggedOutException());
            }
            if (!newMarketFacade.StoreExist(storeID))
            {
                Logger.writeEvent("MakeDiscountAllowed failed | Store doesnt exist");
                return Json.Create_json_response(false, new StoreDoesntExistException());
            }
            if (!newMarketFacade.IsPermitedOperation(sessionID, storeID, POLICY_PERMISSION))
            {
                Logger.writeEvent("MakeDiscountAllowed failed | no permission");
                return Json.Create_json_response(false, new PermissionException());
            }
            if (newMarketFacade.IsDiscountTypeAllowed(storeID, discountTypeString))
            {
                Logger.writeEvent("MakeDiscountAllowed failed | DiscountType Already allowed");
                return Json.Create_json_response(false, new DiscountTypeAllowedException());
            }

            return newMarketFacade.MakeDiscountAllowed(sessionID, storeID, discountTypeString);
        }

        /// <summary>
        /// discount type string must match a discountType defined in Stores.Discounts.DiscountPolicy
        /// </summary>
        /// <param name="sessionID"></param>
        /// <param name="storeID"></param>
        /// <param name="discountTypeString"></param>
        /// <returns>List<string> representing the discount types that are allowed</returns>
        public string GetAllowedDiscounts(Guid sessionID, Guid storeID)
        {
            if (sessionID == null || sessionID.Equals(Guid.Empty) ||
                storeID == null || storeID.Equals(Guid.Empty)
                )
            {
                Logger.writeEvent("Invalid input while GetAllowedDiscounts");
                return Json.Create_json_response(false, new InvalidInputException());
            }
            if (!newMarketFacade.IsloggedIn(sessionID))
            {
                Logger.writeEvent("GetAllowedDiscounts failed | not logged in");
                return Json.Create_json_response(false, new LoggedOutException());
            }
            if (!newMarketFacade.StoreExist(storeID))
            {
                Logger.writeEvent("GetAllowedDiscounts failed | Store doesnt exist");
                return Json.Create_json_response(false, new StoreDoesntExistException());
            }
            if (!newMarketFacade.IsPermitedOperation(sessionID, storeID, POLICY_PERMISSION))
            {
                Logger.writeEvent("GetAllowedDiscounts failed | no permission");
                return Json.Create_json_response(false, new PermissionException());
            }

            return newMarketFacade.GetAllowedDiscounts(sessionID, storeID);
        }

        #region purchase_policy


        /// <summary>
        /// at least one of minAmount or maxAmount are neccesary
        /// </summary>
        /// <param name="sessionID"></param>
        /// <param name="storeID"></param>
        /// <param name="itemID"></param>
        /// <param name="minAmount"></param>
        /// <param name="maxAmount"></param>
        /// <returns> object with  itemID, minAmount, maxAmount, itemName</returns>
        public string AddItemMinMaxPurchasePolicy(Guid sessionID, Guid storeID, Guid itemID, int? minAmount, int? maxAmount)
        {
            if (sessionID == null || sessionID.Equals(Guid.Empty) ||
                storeID == null || storeID.Equals(Guid.Empty) ||
                itemID == null || itemID.Equals(Guid.Empty) ||
                (minAmount == null && maxAmount ==null))
            {
                Logger.writeEvent("Invalid input while AddItemMinMaxPurchasePolicy");
                return Json.Create_json_response(false, new InvalidInputException());
            }
            if (minAmount is int min && maxAmount is int max)
            {
                if (min > max)
                {
                    Logger.writeEvent("AddStoreMinMaxPurchasePolicy failed | min > max");
                    return Json.Create_json_response(false, new MinHigherThanMaxException());
                }
            }
            if (!newMarketFacade.IsloggedIn(sessionID))
            {
                Logger.writeEvent("AddItemMinMaxPurchasePolicy failed | not logged in");
                return Json.Create_json_response(false, new LoggedOutException());
            }
            if (!newMarketFacade.StoreExist(storeID))
            {
                Logger.writeEvent("AddItemMinMaxPurchasePolicy failed | Store doesnt exist");
                return Json.Create_json_response(false, new StoreDoesntExistException());
            }
            if (!newMarketFacade.ItemExistInStore(storeID, itemID))
            {
                Logger.writeEvent("AddItemMinMaxPurchasePolicy failed | item doesnt exist");
                return Json.Create_json_response(false, new ItemDoesntExistException());
            }
            if (!newMarketFacade.IsPermitedOperation(sessionID, storeID, POLICY_PERMISSION))
            {
                Logger.writeEvent("AddItemMinMaxPurchasePolicy failed | no permission");
                return Json.Create_json_response(false, new PermissionException());
            }
            if (!newMarketFacade.IsValidToCreateItemMinMaxPurchasePolicy(storeID, itemID))
            {
                Logger.writeEvent("AddItemMinMaxPurchasePolicy failed | policy not valid - overlapping with exsisting");
                return Json.Create_json_response(false, new OverlappingPolicyException());
            }

            return newMarketFacade.AddItemMinMaxPurchasePolicy(sessionID, storeID, itemID, minAmount, maxAmount);
        }

        /// <summary>
        /// at least one of minAmount or maxAmount are neccesary
        /// </summary>
        /// <param name="sessionID"></param>
        /// <param name="storeID"></param>
        /// <param name="minAmount"></param>
        /// <param name="maxAmount"></param>
        /// <returns> object with storeID, minAmount, maxAmount, storeName</returns>
        public string AddStoreMinMaxPurchasePolicy(Guid sessionID, Guid storeID, int? minAmount, int? maxAmount)
        {
            if (sessionID == null || sessionID.Equals(Guid.Empty) ||
                storeID == null || storeID.Equals(Guid.Empty) ||
                (minAmount == null && maxAmount == null))
            {
                Logger.writeEvent("Invalid input while AddStoreMinMaxPurchasePolicy");
                return Json.Create_json_response(false, new InvalidInputException());
            }
            if (minAmount is int min && maxAmount is int max)
            {
                if (min > max)
                {
                    Logger.writeEvent("AddStoreMinMaxPurchasePolicy failed | min > max");
                    return Json.Create_json_response(false, new MinHigherThanMaxException());
                }
            }
            if (!newMarketFacade.IsloggedIn(sessionID))
            {
                Logger.writeEvent("AddStoreMinMaxPurchasePolicy failed | not logged in");
                return Json.Create_json_response(false, new LoggedOutException());
            }
            if (!newMarketFacade.StoreExist(storeID))
            {
                Logger.writeEvent("AddStoreMinMaxPurchasePolicy failed | Store doesnt exist");
                return Json.Create_json_response(false, new StoreDoesntExistException());
            }
           
            if (!newMarketFacade.IsPermitedOperation(sessionID, storeID, POLICY_PERMISSION))
            {
                Logger.writeEvent("AddStoreMinMaxPurchasePolicy failed | no permission");
                return Json.Create_json_response(false, new PermissionException());
            }
            if (!newMarketFacade.IsValidToCreateStoreMinMaxPurchasePolicy(storeID))
            {
                Logger.writeEvent("AddStoreMinMaxPurchasePolicy failed | Purchase policy not valid - overlapping with exsisting");
                return Json.Create_json_response(false, new OverlappingPolicyException());
            }

            return newMarketFacade.AddStoreMinMaxPurchasePolicy(sessionID, storeID, minAmount, maxAmount);

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sessionID"></param>
        /// <param name="storeID"></param>
        /// <param name="daysNotAllowed"> array with numbers between 1-7, 1 is sunday 7 saturday</param>
        /// <returns> object with  days not allowed</returns>
        public string AddDaysNotAllowedPurchasePolicy(Guid sessionID, Guid storeID, int[] daysNotAllowed)
        {
            if (sessionID == null || sessionID.Equals(Guid.Empty) ||
                storeID == null || storeID.Equals(Guid.Empty) ||
                daysNotAllowed==null || daysNotAllowed.Length == 0 || daysNotAllowed.Length == 7)
            {
                Logger.writeEvent("Invalid input while AddDaysNotAllowedPurchasePolicy");
                return Json.Create_json_response(false, new InvalidInputException());
            }
            foreach(int i in daysNotAllowed)
            {
                if (i <1 || i > 7)
                {
                    Logger.writeEvent("Invalid input while AddDaysNotAllowedPurchasePolicy");
                    return Json.Create_json_response(false, new InvalidInputException());
                }
            }
            if (!newMarketFacade.IsloggedIn(sessionID))
            {
                Logger.writeEvent("AddDaysNotAllowedPurchasePolicy failed | not logged in");
                return Json.Create_json_response(false, new LoggedOutException());
            }
            if (!newMarketFacade.StoreExist(storeID))
            {
                Logger.writeEvent("AddDaysNotAllowedPurchasePolicy failed | Store doesnt exist");
                return Json.Create_json_response(false, new StoreDoesntExistException());
            }

            if (!newMarketFacade.IsPermitedOperation(sessionID, storeID, POLICY_PERMISSION))
            {
                Logger.writeEvent("AddDaysNotAllowedPurchasePolicy failed | no permission");
                return Json.Create_json_response(false, new PermissionException());
            }
            if (!newMarketFacade.IsValidToCreateDaysNotAllowedPurchasePolicy(storeID))
            {
                Logger.writeEvent("AddDaysNotAllowedPurchasePolicy failed | Purchase policy not valid - overlapping with exsisting");
                return Json.Create_json_response(false, new OverlappingPolicyException());
            }

            return newMarketFacade.AddDaysNotAllowedPurchasePolicy(sessionID, storeID, daysNotAllowed);

        }


        /// <summary>
        /// same as discounts.
        /// </summary>
        /// <param name="sessionID"></param>
        /// <param name="storeID"></param>
        /// <param name="policyLeftID"></param>
        /// <param name="policyRightID"></param>
        /// <param name="boolOperator"></param>
        /// <returns></returns>
        public string ComposeTwoPurchasePolicies(Guid sessionID, Guid storeID, Guid policyLeftID, Guid policyRightID, string boolOperator)
        {
            if (sessionID == null || sessionID.Equals(Guid.Empty) ||
                storeID == null || storeID.Equals(Guid.Empty) ||
                policyLeftID == null || policyLeftID.Equals(Guid.Empty) ||
                policyRightID == null || policyRightID.Equals(Guid.Empty) ||
                !allowed_ops.Contains(boolOperator.ToLower().Trim()))
            {
                Logger.writeEvent("Invalid input while ComposeTwoPurchasePolicies");
                return Json.Create_json_response(false, new InvalidInputException());
            }
            if (!newMarketFacade.IsloggedIn(sessionID))
            {
                Logger.writeEvent("ComposeTwoPurchasePolicies failed | not logged in");
                return Json.Create_json_response(false, new LoggedOutException());
            }
            if (!newMarketFacade.StoreExist(storeID))
            {
                Logger.writeEvent("ComposeTwoPurchasePolicies failed | Store doesnt exist");
                return Json.Create_json_response(false, new StoreDoesntExistException());
            }
            if (!newMarketFacade.IsPermitedOperation(sessionID, storeID, POLICY_PERMISSION))
            {
                Logger.writeEvent("ComposeTwoPurchasePolicies failed | no permission");
                return Json.Create_json_response(false, new PermissionException());
            }
            if (!newMarketFacade.PurchasePolicyExistsInStore(storeID, policyLeftID) ||
               !newMarketFacade.PurchasePolicyExistsInStore(storeID, policyRightID))
            {
                Logger.writeEvent("ComposeTwoPurchasePolicies failed | PolicyID doesnt exist");
                return Json.Create_json_response(false, new DiscountDoesntExistException());
            }

            if (policyLeftID.Equals(policyRightID))
            {
                Logger.writeEvent("ComposeTwoPurchasePolicies failed | Composed policy to itself");
                return Json.Create_json_response(false, new ComposePolicyWithItselfException());
            }
            return newMarketFacade.ComposeTwoPurchasePolicys(sessionID, storeID, policyLeftID, policyRightID, boolOperator);

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sessionID"></param>
        /// <param name="storeID"></param>
        /// <param name="purchasePolicy">[ "item","store","days","composite"</param>
        /// <returns></returns>
        public string MakePurcahsePolicyAllowed(Guid sessionID, Guid storeID, string purchasePolicy)
        {
            if (sessionID == null || sessionID.Equals(Guid.Empty) ||
                storeID == null || storeID.Equals(Guid.Empty) ||
                !allowed_PurchaePolicyType_strings.Contains(purchasePolicy.ToLower().Trim()))
            {
                Logger.writeEvent("Invalid input while MakePurcahsePolicyAllowed");
                return Json.Create_json_response(false, new InvalidInputException());
            }
            if (!newMarketFacade.IsloggedIn(sessionID))
            {
                Logger.writeEvent("MakePurcahsePolicyAllowed failed | not logged in");
                return Json.Create_json_response(false, new LoggedOutException());
            }
            if (!newMarketFacade.StoreExist(storeID))
            {
                Logger.writeEvent("MakePurcahsePolicyAllowed failed | Store doesnt exist");
                return Json.Create_json_response(false, new StoreDoesntExistException());
            }
            if (!newMarketFacade.IsPermitedOperation(sessionID, storeID, POLICY_PERMISSION))
            {
                Logger.writeEvent("MakePurcahsePolicyAllowed failed | no permission");
                return Json.Create_json_response(false, new PermissionException());
            }
            if (newMarketFacade.IsPurchaseTypeAllowed(storeID, purchasePolicy))
            {
                Logger.writeEvent("MakePurcahsePolicyAllowed failed | purchasePolicy Already allowed");
                return Json.Create_json_response(false, new PurchasePolicyTypeAllowedException());
            }

            return newMarketFacade.MakePurcahsePolicyAllowed(sessionID, storeID, purchasePolicy);

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sessionID"></param>
        /// <param name="storeID"></param>
        /// <param name="purchasePolicy">[ "item","store","days","composite"</param>
        /// <returns></returns>
        public string MakePurcahsePolicyNotAllowed(Guid sessionID, Guid storeID, string purchasePolicy)
        {
            if (sessionID == null || sessionID.Equals(Guid.Empty) ||
                storeID == null || storeID.Equals(Guid.Empty) ||
                !allowed_PurchaePolicyType_strings.Contains(purchasePolicy.ToLower().Trim()))
            {
                Logger.writeEvent("Invalid input while MakePurcahsePolicyNotAllowed");
                return Json.Create_json_response(false, new InvalidInputException());
            }
            if (!newMarketFacade.IsloggedIn(sessionID))
            {
                Logger.writeEvent("MakePurcahsePolicyNotAllowed failed | not logged in");
                return Json.Create_json_response(false, new LoggedOutException());
            }
            if (!newMarketFacade.StoreExist(storeID))
            {
                Logger.writeEvent("MakePurcahsePolicyNotAllowed failed | Store doesnt exist");
                return Json.Create_json_response(false, new StoreDoesntExistException());
            }
            if (!newMarketFacade.IsPermitedOperation(sessionID, storeID, POLICY_PERMISSION))
            {
                Logger.writeEvent("MakePurcahsePolicyNotAllowed failed | no permission");
                return Json.Create_json_response(false, new PermissionException());
            }
            if (!newMarketFacade.IsPurchaseTypeAllowed(storeID, purchasePolicy))
            {
                Logger.writeEvent("MakePurcahsePolicyNotAllowed failed | purchasePolicy Already isnt allowed");
                return Json.Create_json_response(false, new PurchasePolicyTypeNotAllowedException());
            }

            return newMarketFacade.MakePurcahsePolicyNotAllowed(sessionID, storeID, purchasePolicy);

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sessionID"></param>
        /// <param name="storeID"></param>
        /// <param name="policyID"></param>
        /// <returns></returns>
        public string RemovePurchasePolicy(Guid sessionID, Guid storeID, Guid policyID)
        {
            if (sessionID == null || sessionID.Equals(Guid.Empty) ||
                storeID == null || storeID.Equals(Guid.Empty) ||
                policyID == null || policyID.Equals(Guid.Empty))
            {
                Logger.writeEvent("Invalid input while RemovePurchasePolicy");
                return Json.Create_json_response(false, new InvalidInputException());
            }
            if (!newMarketFacade.IsloggedIn(sessionID))
            {
                Logger.writeEvent("RemovePurchasePolicy failed | not logged in");
                return Json.Create_json_response(false, new LoggedOutException());
            }
            if (!newMarketFacade.StoreExist(storeID))
            {
                Logger.writeEvent("RemovePurchasePolicy failed | Store doesnt exist");
                return Json.Create_json_response(false, new StoreDoesntExistException());
            }
            if (!newMarketFacade.IsPermitedOperation(sessionID, storeID, POLICY_PERMISSION))
            {
                Logger.writeEvent("RemovePurchasePolicy failed | no permission");
                return Json.Create_json_response(false, new PermissionException());
            }
            if (!newMarketFacade.PurchasePolicyExistsInStore(storeID, policyID))
            {
                Logger.writeEvent("RemovePurchasePolicy failed | policyID doesnt exist");
                return Json.Create_json_response(false, new PurchasePolicyDoesntExistException());
            }

            return newMarketFacade.RemovePurchasePolicy(sessionID, storeID, policyID);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sessionID"></param>
        /// <param name="storeID"></param>
        /// <returns>string[]</returns>
        public string GetAllowedPurchasePolicys(Guid sessionID, Guid storeID)
        {
            if (sessionID == null || sessionID.Equals(Guid.Empty) ||
                storeID == null || storeID.Equals(Guid.Empty)
                )
            {
                Logger.writeEvent("Invalid input while GetAllowedPurchasePolicys");
                return Json.Create_json_response(false, new InvalidInputException());
            }
            if (!newMarketFacade.IsloggedIn(sessionID))
            {
                Logger.writeEvent("GetAllowedPurchasePolicys failed | not logged in");
                return Json.Create_json_response(false, new LoggedOutException());
            }
            if (!newMarketFacade.StoreExist(storeID))
            {
                Logger.writeEvent("GetAllowedPurchasePolicys failed | Store doesnt exist");
                return Json.Create_json_response(false, new StoreDoesntExistException());
            }
            if (!newMarketFacade.IsPermitedOperation(sessionID, storeID, POLICY_PERMISSION))
            {
                Logger.writeEvent("GetAllowedPurchasePolicys failed | no permission");
                return Json.Create_json_response(false, new PermissionException());
            }

            return newMarketFacade.GetAllowedPurchasePolicys(sessionID, storeID);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sessionID"></param>
        /// <param name="storeID"></param>
        /// <returns>same as discounts.</returns>
        public string GetAllPurchasePolicys(Guid sessionID, Guid storeID)
        {
            if (sessionID == null || sessionID.Equals(Guid.Empty) ||
                storeID.Equals(Guid.Empty) || Guid.Empty.Equals(storeID))
            {
                Logger.writeEvent("Invalid input while GetAllPurchasePolicys");
                return Json.Create_json_response(false, new InvalidInputException());
            }
            if (!newMarketFacade.IsloggedIn(sessionID))
            {
                Logger.writeEvent("GetAllPurchasePolicys failed | not logged in");
                return Json.Create_json_response(false, new LoggedOutException());
            }
            if (!newMarketFacade.StoreExist(storeID))
            {
                Logger.writeEvent("GetAllPurchasePolicys failed | Store doesnt exist");
                return Json.Create_json_response(false, new StoreDoesntExistException());
            }
            if (!newMarketFacade.IsPermitedOperation(sessionID, storeID, POLICY_PERMISSION))
            {
                Logger.writeEvent("GetAllPurchasePolicys failed | no permission");
                return Json.Create_json_response(false, new PermissionException());
            }
           
            return newMarketFacade.GetAllPurchasePolicys(sessionID, storeID);
        }

        #endregion
    }
}
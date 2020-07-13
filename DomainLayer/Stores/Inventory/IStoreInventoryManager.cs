using DomainLayer.DbAccess;
using DomainLayer.Orders;
using DomainLayer.Users;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace DomainLayer.Stores.Inventory
{
    public interface IStoreInventoryManager
    {
        /// <summary>
        /// Adds item with the foloowing properties.
        /// If item with requested name already exists - ItemAlreadyExistsException is thrown
        /// </summary>
        /// <param name="name"></param>
        /// <param name="amount"></param>
        /// <param name="categories"></param>
        /// <param name="price"></param>
        /// <param name="keyWords"></param>
        /// <returns>The item</returns>
        Item AddItem(
                    string name,
            int amount,
            HashSet<string> categories,
            double price,
            MarketDbContext context,
            HashSet<string> keyWords = null);

        /// <summary>
        /// Deletes item with requested id
        /// If there isn't item with this id - ItemNotFoundException is thrown
        /// </summary>
        /// <param name="itemId"></param>
        /// <returns></returns>
        void DeleteItem(Guid itemId, MarketDbContext context);

        /// <summary>
        /// Edits item with requested details to add.
        /// IMPORTANT assumption: details are from currect types!
        /// If there isn't item with this id - ItemNotFoundException is thrown
        /// </summary>
        /// <param name="itemId"></param>
        /// <param name="detailsToEdit"></param>
        /// <returns>The Item</returns>
        Item EditItem(Guid itemId, Dictionary<StoresUtils.ItemEditDetails, object> detailsToEdit, MarketDbContext context);

        /// <summary>
        /// Checks if the items are available in inventory with at least of the requested amount. 
        /// If an item is not found - ItemNotFoundException is thrown
        /// </summary>
        /// <param name="orderItems"></param>
        /// <returns>True if check passed successfully, otherwise false/returns>
        bool IsOrderItemsAmountAvailable(StoreCart cart, MarketDbContext context);

        /// <summary>
        /// Update the inventory as result of order.
        /// If item not found - ItemNotFoundException is thrown,
        /// If reducing amount from item is invalid - InvalidOperationOnItemException is thrown
        /// </summary>
        /// <param name="cart"> holds the dictionary ItemID:Amount</param>
        /// <returns>True is update succeeded, otherwise false</returns>
        bool ReduceStoreInventoryDueToOrder(StoreCart cart, MarketDbContext context);

        /*       bool UpdateNameItem(int id, string name);
       bool UpdateAmountItem(int id, int amount);*/
        bool AddCategoryItem(Guid itemId, string category, MarketDbContext context);
        bool UpdateCategoryItem(Guid itemId, string originalCategory, string updatedCategory, MarketDbContext context);
        bool DeleteCategoryItem(Guid itemId, string category, MarketDbContext context);
        bool AddKeyWordItem(Guid itemId, string keyWord, MarketDbContext context);
        bool UpdateKeyWordItem(Guid itemId, string originalKeyWord, string updatedKeyWord, MarketDbContext context);
        bool DeleteKeyWordItem(Guid itemId, string keyWord, MarketDbContext context);

        /// <summary>
        /// Return all requested items.
        /// If item was not found ItemNotFoundException is thrown
        /// </summary>
        /// <param name="itemIds"></param>
        /// <returns> list of the requested items</returns>
        List<Item> GetItemsById(List<Guid> itemIds, MarketDbContext context);

        /// <summary>
        /// Return requested item.
        /// If item was not found ItemNotFoundException is thrown
        /// </summary>
        /// <param name="itemId"></param>
        /// <returns> The requested item</returns>
        Item GetItemById(Guid itemId);

		bool ValidatePurchase(StoreCart value, User user, MarketDbContext context);
        /// <summary>
        /// return List<Tuple<Item itemOfCart, int amountInCart, double PricePerItemAfterDiscount>>
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        List<Tuple<Item, int, double>> GetDiscountedPriceAndReserve(StoreCart value, MarketDbContext context);
		void IsOrderItemsAmountAvailable(Dictionary<Guid, int> items, MarketDbContext context);
        void ReturnItemsFromOrder(List<OrderItem> items, MarketDbContext context);
        string GetName();

        /*        bool UpdateRankItem(int id, int rank);
        bool UpdatePrice(int id, double newPrice);
        bool UpdateUrlImage(int id, string urlImage);*/
    }
}

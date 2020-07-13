using CustomLogger;
using DomainLayer.DbAccess;
using DomainLayer.Exceptions;
using DomainLayer.Orders;
using DomainLayer.Stores;
using DomainLayer.Stores.Certifications;
using DomainLayer.Stores.Discounts;
using DomainLayer.Stores.Inventory;
using DomainLayer.Stores.PurchasePolicies;
using System;
using System.Collections.Generic;
using System.Text;

namespace DomainLayer.StoreManagement
{
    public class StoreManagementFacade
    {
        readonly StoreHandler Stores;
        readonly OrderManager Orders;

        public StoreManagementFacade(StoreHandler storeHandler, OrderManager orderManager)
        {
            Stores = storeHandler;
            Orders = orderManager;
        }

        private IStoreInventoryManager GetAsInventoryManager(Guid storeID, MarketDbContext context) => Stores.GetStoreInventoryManager(storeID, context);

        private IStoreCertificationManager GetAsCertificationManager(Guid storeID, MarketDbContext context) => Stores.GetStoreCertificationManager(storeID, context);

        private void EnsurePermissionForAction(Guid sessionUser, IStoreCertificationManager storeCertificationManager, Permission request, MarketDbContext context)
        {
            storeCertificationManager.EnsurePermission(sessionUser, request, context);
        }

        private IStoreCertificationManager GetCertificationManagerAndCheckPermissions(Guid storeID, Guid sessionUser, Permission request, MarketDbContext context)
        {
            IStoreCertificationManager storeCertificationManager = GetAsCertificationManager(storeID, context);// throws exception if not exsits
            EnsurePermissionForAction(sessionUser, storeCertificationManager, request, context);// throws exception if fails
            Logger.writeEvent(string.Format("StoreManagementFacade: User {0} permitted to {1} in store {2}",
                                                                                    sessionUser, request.ToString(), storeID));
            return storeCertificationManager;
        }

        private IStorePolicyManager GetAsDiscountManagerAndCheckPermissions(Guid storeID, Guid sessionUser, MarketDbContext context)
        {
            EnsurePermissionForAction(sessionUser, GetAsCertificationManager(storeID, context), Permission.POLICY, context);// throws exception if fails
            Logger.writeEvent(string.Format("StoreManagementFacade: User {0} permitted to modify discounts in store {1}",
                                            sessionUser, storeID));
            return GetAsDiscountManager(storeID, context);
        }

        private IStorePolicyManager GetAsDiscountManager(Guid storeID, MarketDbContext context) => Stores.GetStoreDiscountManager(storeID, context);

        private IStoreInventoryManager GetInventoryManagerAndCheckPermissions(Guid storeID, Guid sessionUser, MarketDbContext context)
        {
            EnsurePermissionForAction(sessionUser, GetAsCertificationManager(storeID, context), Permission.INVENTORY, context);// throws exception if fails
            Logger.writeEvent(string.Format("StoreManagementFacade: User {0} permitted to modify inventory in store {1}",
                                                                        sessionUser, storeID));
            return GetAsInventoryManager(storeID, context);
        }

        public Store OpenStore(StoreContactDetails contactDetails, Guid sessionUser, MarketDbContext context)
            => Stores.OpenStore(contactDetails,  sessionUser, context);

        public void AddPermission(Guid storeID, Guid userID, Guid sessionUser, Permission toAdd, MarketDbContext context)
        {
            GetCertificationManagerAndCheckPermissions(storeID, sessionUser, Permission.EDIT_PERMISSIONS, context)
                .AddPermission(userID, sessionUser, toAdd, context);
        }

        public void RemovePermission(Guid storeID, Guid userID, Guid sessionUser, Permission toRemove, MarketDbContext context)
        {
            GetCertificationManagerAndCheckPermissions(storeID, sessionUser, Permission.EDIT_PERMISSIONS, context)
                .RemovePermission(userID, sessionUser, toRemove, context);
        }

        public void AppointOwner(Guid storeID, Guid userID, Guid sessionUser, string userName, MarketDbContext context)
        {
            GetCertificationManagerAndCheckPermissions(storeID, sessionUser, Permission.APPOINT_OWNER, context)
                .AddOwner(userID, sessionUser, userName, Stores, context);
        }

        public void AppointManager(Guid storeID, Guid userID, Guid sessionUser, MarketDbContext context)
        {
            GetCertificationManagerAndCheckPermissions(storeID, sessionUser, Permission.APPOINT_MANAGER, context)
                .AddManager(userID, sessionUser, context);
        }

        public void RemoveOwner(Guid storeID, Guid userID, Guid sessionUser, MarketDbContext context)
        {
            GetCertificationManagerAndCheckPermissions(storeID, sessionUser, Permission.REMOVE_OWNER, context)
                .RemoveOwner(userID, sessionUser, Stores, context);
        }

        public void RemoveManager(Guid storeID, Guid userID, Guid sessionUser, MarketDbContext context)
        {
            GetCertificationManagerAndCheckPermissions(storeID, sessionUser, Permission.REMOVE_MANAGER, context)
                .RemoveManager(userID, sessionUser, Stores, context);
        }

        internal bool IsGrantorOf(Guid storeID, Guid grantorID, Guid userID, MarketDbContext context)
        {
            return GetAsCertificationManager(storeID, context).IsGrantorOf(grantorID, userID, context);

        }

        internal bool IsOwnerOfStore(Guid storeID, Guid userID, MarketDbContext context)
        {
            return GetAsCertificationManager(storeID, context).IsOwnerOfStore(userID, context);
        }

        internal bool IsPermittedOperation(Guid storeID, Guid userID, Permission permission, MarketDbContext context)
        {
            IStoreCertificationManager storeCertificationManager = GetAsCertificationManager(storeID, context);
            return storeCertificationManager.IsPermittedOperation(userID, permission, context);
        }

        public Item AddItem(Guid storeID, Guid sessionUser, string name, int amount, HashSet<string> categories, double price, MarketDbContext context, HashSet<string> keyWords = null)
        {
            return GetInventoryManagerAndCheckPermissions(storeID, sessionUser, context)
                .AddItem(name, amount, categories, price, context, keyWords);
        }

        public void DeleteItem(Guid storeID, Guid sessionUser, Guid itemId, MarketDbContext context)
        {
            GetInventoryManagerAndCheckPermissions(storeID, sessionUser, context)
                .DeleteItem(itemId, context);
        }

        public Item EditItem(Guid storeID, Guid sessionUser, Guid itemId, Dictionary<StoresUtils.ItemEditDetails, object> detailsToEdit, MarketDbContext context)
        {
            return GetInventoryManagerAndCheckPermissions(storeID, sessionUser, context)
                .EditItem(itemId, detailsToEdit, context);
        }

        public List<StoreOrder> GetStoreOrderHistory(Guid storeID, Guid sessionUser, MarketDbContext context)
        {
            GetCertificationManagerAndCheckPermissions(storeID, sessionUser, Permission.HISTORY, context);
            return Orders.GetStoreOrdersHistory(storeID, context);
        }

        internal bool ItemExistInStore(Guid storeID, Guid itemID, MarketDbContext context)
        {
            IStoreInventoryManager storeInventory = GetAsInventoryManager(storeID, context);
            try
            {
                Item i = storeInventory.GetItemById(itemID);
                return i != null;
            }
            catch (ItemNotFoundException)
            {
                return false;
            }

        }

        public List<Permission> GetUserPermissions(Guid storeID, Guid userID, Guid sessionUser, MarketDbContext context)
            => GetAsCertificationManager(storeID, context).GetPermissions(userID, sessionUser, context);

        public List<Permission> GetPermissionsInStore(Guid storeID, Guid sessionUser, MarketDbContext context)
            => GetAsCertificationManager(storeID, context).GetPermissions(sessionUser, context);

        internal bool IsManagerOfStore(Guid storeID, Guid userID, MarketDbContext context)
        {
            IStoreCertificationManager certificationManager = GetAsCertificationManager(storeID, context);
            return certificationManager.IsManagerOfStore(userID, context);
        }

        internal List<Tuple<Store, List<Permission>>> GetStoresWithPermissions(Guid sessionUser, MarketDbContext context)
        {

            List<Tuple<Store, List<Permission>>> result = new List<Tuple<Store, List<Permission>>>();
            foreach (Guid storeID in Stores.AllStoresIDs(context))
            {
                IStoreCertificationManager certificationManager = GetAsCertificationManager(storeID, context);
                try {
                    List<Permission> perms = certificationManager.GetPermissions(sessionUser, context);
                    Store s = Stores.GetStoreById(storeID, context);
                    result.Add(new Tuple<Store, List<Permission>>(s, perms));
                }
                catch (CertificationException)
                {
                    continue;
                }

            }
            return result;
        }

        #region DiscountManagment

        internal OpenDiscount AddOpenDiscount(Guid storeID, Guid sessionUser, Guid itemID, double discount, DateTime dateUntil, MarketDbContext context)
        {
            IStorePolicyManager discountManager = GetAsDiscountManagerAndCheckPermissions(storeID, sessionUser, context);
            return discountManager.AddOpenDiscount(itemID, discount, dateUntil, context);

        }

        internal ItemConditionalDiscount_MinItems_ToDiscountOnAll AddItemConditionalDiscount_MinItems_ToDiscountOnAll(
            Guid storeID, Guid sessionUser, Guid itemID, double discount, int minItems, DateTime dateUntil, MarketDbContext context)
        {
            IStorePolicyManager discountManager = GetAsDiscountManagerAndCheckPermissions(storeID, sessionUser, context);
            return discountManager.AddItemConditionalDiscount_MinItems_ToDiscountOnAll(itemID, discount, minItems, dateUntil, context);
        }

        internal ItemConditionalDiscount_MinItems_ToDiscountOnExtraItems AddItemConditionalDiscount_MinItems_ToDiscountOnExtraItems(
            Guid storeID, Guid sessionUser, Guid itemID, double discountForExtra, int minItems, int extraItems, DateTime dateUntil, MarketDbContext context)
        {
            IStorePolicyManager discountManager = GetAsDiscountManagerAndCheckPermissions(storeID, sessionUser, context);
            return discountManager.AddItemConditionalDiscount_MinItems_ToDiscountOnExtraItems(itemID, discountForExtra, minItems, extraItems, dateUntil, context);
        }

        internal StoreConditionalDiscount AddStoreConditionalDiscount(Guid storeID, Guid sessionUser, double minPurchase, double discount, DateTime dateUntil, MarketDbContext context)
        {
            IStorePolicyManager discountManager = GetAsDiscountManagerAndCheckPermissions(storeID, sessionUser, context);
            return discountManager.AddStoreConditionalDiscount(minPurchase, discount, dateUntil, context);
        }

        internal bool DiscountExistsInStore(Guid storeID, Guid discountID, MarketDbContext context)
        {
            IStorePolicyManager discountManager = GetAsDiscountManager(storeID, context);
            return discountManager.DiscountExistsInStore(discountID, context);
        }

        internal CompositeDiscount AddCompositeDiscount(Guid sessionUser, Guid storeID, Guid discountLeftID, Guid discountRightID, string boolOperator, MarketDbContext context)
        {
            return GetAsDiscountManagerAndCheckPermissions(storeID, sessionUser, context)
                .AddCompositeDiscount(discountLeftID, discountRightID, boolOperator, context);
        }

        internal bool RemoveDiscount(Guid sessionUser, Guid storeID, Guid discountID, MarketDbContext context)
        {
            return GetAsDiscountManagerAndCheckPermissions(storeID, sessionUser, context)
                    .RemoveDiscount(discountID, context);
        }


        internal bool IsDiscountAllowed(Guid storeID, DiscountType discountType, MarketDbContext context)
        {
            return GetAsDiscountManager(storeID, context).IsDiscountAllowed(discountType, context);
        }

        internal bool MakeDiscountNotAllowed(Guid sessionUser, Guid storeID, DiscountType discountType, MarketDbContext context)
        {
            IStorePolicyManager discountManager = GetAsDiscountManagerAndCheckPermissions(storeID, sessionUser, context);
            return discountManager.MakeDiscountNotAllowed(discountType, context);
        }

        internal bool MakeDiscountAllowed(Guid sessionUser, Guid storeID, DiscountType discountType, MarketDbContext context)
        {
            IStorePolicyManager discountManager = GetAsDiscountManagerAndCheckPermissions(storeID, sessionUser, context);
            return discountManager.MakeDiscountAllowed(discountType, context);
        }

        internal List<DiscountType> GetAllowedDiscounts(Guid storeID, Guid sessionUser, MarketDbContext context)
        {
            IStorePolicyManager discountManager = GetAsDiscountManagerAndCheckPermissions(storeID, sessionUser, context);
            return discountManager.GetAllowedDiscounts(context);
        }

        internal List<ADisountDataClassForSerialization> GetAllDiscounts(Guid sessionUser, Guid storeID, Guid? itemID, MarketDbContext context)
        {
            IStorePolicyManager discountManager = GetAsDiscountManagerAndCheckPermissions(storeID, sessionUser, context);
            return discountManager.GetAllDiscounts(itemID);
        }

        internal bool IsValidToCreateStoreConditionalDiscount(Guid storeID, MarketDbContext context)
        {
            IStorePolicyManager discountManager = GetAsDiscountManager(storeID, context);
            return discountManager.IsValidToCreateStoreConditionalDiscount(context);
        }

        internal bool IsValidToCreateItemOpenedDiscount(Guid storeID, Guid itemID, MarketDbContext context)
        {
            IStorePolicyManager discountManager = GetAsDiscountManager(storeID, context);
            return discountManager.IsValidToCreateOpenItemDiscount(itemID, context);
            }

        internal bool IsValidToCreateItemConditionalDiscount(Guid storeID, Guid itemID, MarketDbContext context)
        {
            IStorePolicyManager discountManager = GetAsDiscountManager(storeID, context);
            return discountManager.IsValidToCreateConditionalItemDiscount(itemID, context);

        }

        internal void ApproveOwnerContract(Guid storeID, Guid userID, Guid sessionUser, MarketDbContext context)
        {
            GetCertificationManagerAndCheckPermissions(storeID, sessionUser, Permission.APPOINT_OWNER, context)
                .ApproveContract( userID,  sessionUser, context);
        }

        internal bool IsApproverOfContract(Guid sessionUser, Guid storeID, Guid userID, MarketDbContext context)
        {
            return GetAsCertificationManager(storeID, context).isApproverOf(sessionUser, userID, context);
        }

        internal bool IsAwaitingContractApproval(Guid storeID, Guid userID, MarketDbContext context)
        {
            return GetAsCertificationManager(storeID, context).HasAwaitingContract(userID);
        }

        internal bool IsValidToCreateItemMinMaxPurchasePolicy(Guid storeID, Guid itemID, MarketDbContext context)
        {
            IStorePolicyManager policyManager = GetAsDiscountManager(storeID, context);
            return policyManager.IsValidToCreateItemMinMaxPurchasePolicy(itemID, context);
        }

        internal bool IsValidToCreateStoreMinMaxPurchasePolicy(Guid storeID, MarketDbContext context)
        {
            IStorePolicyManager policyManager = GetAsDiscountManager(storeID, context);
            return policyManager.IsValidToCreateStoreMinMaxPurchasePolicy(context);
        }

        internal bool IsValidToCreateDaysNotAllowedPurchasePolicy(Guid storeID, MarketDbContext context)
        {
            IStorePolicyManager policyManager = GetAsDiscountManager(storeID, context);
            return policyManager.IsValidToCreateDaysNotAllowedPurchasePolicy(context);
        }

        internal ItemMinMaxPurchasePolicy AddItemMinMaxPurchasePolicy(Guid storeID, Guid userID, Guid itemID, int? minAmount, int? maxAmount, MarketDbContext context)
        {
            IStorePolicyManager policyManager = GetAsDiscountManagerAndCheckPermissions(storeID, userID, context);
            return policyManager.AddItemMinMaxPurchasePolicy(itemID, minAmount, maxAmount, context);
        }

        internal StoreMinMaxPurchasePolicy AddStoreMinMaxPurchasePolicy(Guid storeID, Guid userID, int? minAmount, int? maxAmount, MarketDbContext context)
        {
            IStorePolicyManager policyManager = GetAsDiscountManagerAndCheckPermissions(storeID, userID, context);
            return policyManager.AddStoreMinMaxPurchasePolicy(minAmount, maxAmount, context);
        }

        internal DaysNotAllowedPurchasePolicy AddDaysNotAllowedPurchasePolicy(Guid storeID, Guid userID, int[] daysNotAllowed, MarketDbContext context)
        {
            IStorePolicyManager policyManager = GetAsDiscountManagerAndCheckPermissions(storeID, userID, context);
            return policyManager.AddDaysNotAllowedPurchasePolicy(daysNotAllowed, context);
        }

        internal bool PolicyExistsInStore(Guid storeID, Guid policyID, MarketDbContext context)
        {
            IStorePolicyManager policyManager = GetAsDiscountManager(storeID, context);
            return policyManager.PolicyExistsInStore(policyID, context);
        }

        internal CompositePurchasePolicy ComposeTwoPurchasePolicys(Guid userID, Guid storeID, Guid policyLeftID, Guid policyRightID, string boolOperator, MarketDbContext context)
        {
            return GetAsDiscountManagerAndCheckPermissions(storeID, userID, context)
                .ComposeTwoPurchasePolicys(policyLeftID, policyRightID, boolOperator, context);
        }

        internal bool IsPurchaseTypeAllowed(Guid storeID, PurchasePolicyType policy, MarketDbContext context)
        {
            return GetAsDiscountManager(storeID, context).IsPurchaseTypeAllowed(policy, context);
        }

        internal bool MakePurcahsePolicyNotAllowed(Guid userID, Guid storeID, PurchasePolicyType policy, MarketDbContext context)
        {
            IStorePolicyManager policyManager = GetAsDiscountManagerAndCheckPermissions(storeID, userID, context);
            return policyManager.MakePurcahsePolicyNotAllowed(policy, context);
        }

        internal bool MakePurcahsePolicyAllowed(Guid userID, Guid storeID, PurchasePolicyType policy, MarketDbContext context)
        {
            IStorePolicyManager policyManager = GetAsDiscountManagerAndCheckPermissions(storeID, userID, context);
            return policyManager.MakePurcahsePolicyAllowed(policy, context);
        }

        internal bool RemovePurchasePolicy(Guid userID, Guid storeID, Guid policyID, MarketDbContext context)
        {
            return GetAsDiscountManagerAndCheckPermissions(storeID, userID, context)
                    .RemovePurchasePolicy(policyID, context);
        }

        internal List<APurchasePolicyDataClassForSerialization> GetAllPurchasePolicys(Guid userID, Guid storeID, MarketDbContext context)
        {
            IStorePolicyManager policyManager = GetAsDiscountManagerAndCheckPermissions(storeID, userID, context);
            return policyManager.GetAllPurchasePolicys();
        }

        internal List<PurchasePolicyType> GetAllowedPurchasePolicys(Guid storeID, Guid userID, MarketDbContext context)
        {
            IStorePolicyManager policyManager = GetAsDiscountManagerAndCheckPermissions(storeID, userID, context);
            return policyManager.GetAllowedPurchasePolicys(context);
        }



        #endregion
    }
}

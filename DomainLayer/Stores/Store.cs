using CustomLogger;
using DomainLayer.Exceptions;
using DomainLayer.Orders;
using DomainLayer.Stores.Certifications;
using DomainLayer.Stores.Inventory;
using DomainLayer.Stores.Discounts;
using DomainLayer.Users;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using DomainLayer.NotificationsCenter;
using DomainLayer.NotificationsCenter.NotificationEvents;
using System.ComponentModel.DataAnnotations;
using DomainLayer.DbAccess;
using System.Linq;
using Newtonsoft.Json;
using DomainLayer.Stores.PurchasePolicies;

namespace DomainLayer.Stores
{
    public class Store : IStoreInventoryManager, IStoreSearch, IStoreCertificationManager, IStorePolicyManager

    {
        public Guid Id { get; set; }
        //private bool open;
        public virtual StoreInventory storeInventory { get; set; }
        public virtual StoreContactDetails ContactDetails { get; set; }
        public double Rank { get; set; }
        [Required]
        public virtual PurchasePolicy PurchasePolicy { get; set; }
        [Required]
        public virtual DiscountPolicy DiscountPolicy { get; set; }

        //TODO: Not for this version: public TraceabilityPolicy TraceabilityPolicy { get; set; }

        public virtual ICollection<StoreOrder> StoreOrders { get; set; }
        public virtual ICollection<Certification> Certifications { get; set; }
        public Dictionary<Guid, Certification> certifications 
        {
            get => Certifications.ToDictionary(c => c.UserID);
            set => Certifications = value.Values; 
        }
        [JsonIgnore]
        public virtual ICollection<StoreOwnerApointmentContract> Contracts { get; set; }
        internal Dictionary<Guid, StoreOwnerApointmentContract> awaitingOwnerContracts
        {
            get => Contracts.ToDictionary(c => c.GranteeID);
            set => Contracts = value.Values;
        }

        public Store(Guid id, StoreContactDetails contactDetails, PurchasePolicy purchasePolicy, DiscountPolicy discountPolicy, Guid owner, MarketDbContext context)
        {
            if (discountPolicy == null)
                throw new ArgumentNullException("discount policy cannot be null");
            if (purchasePolicy == null)
                throw new ArgumentNullException("purchase policy cannot be null");

            Id = id;
            ContactDetails = contactDetails;

            //Until implementing UC 4.2:
            PurchasePolicy = purchasePolicy;
            DiscountPolicy = discountPolicy;

            Rank = 0;
            storeInventory = new StoreInventory(Id);
            StoreOrders = new HashSet<StoreOrder>();
            Certifications = new HashSet<Certification>();
            Contracts = new HashSet<StoreOwnerApointmentContract>();
            AddFirstOwner(owner, context);
        }

        public Store()
        {
        }

        //Attention!!! currently non of the operations on store is checking who is doing it - stores manager is forced to check
        public Item AddItem(string name, int amount, HashSet<string> categories, double price,
            MarketDbContext context, HashSet<string> keyWords = null)
        {
            Item result = storeInventory.AddItem(name, amount, categories, price, context, keyWords);
            context.SaveChanges();
            return result;
        }

        //private void LoadInventory(MarketDbContext context)
        //{
        //    context.Entry(this).Reference("storeInventory").Load();
        //    context.Entry(storeInventory).Collection("Items").Load();
        //}

        public void DeleteItem(Guid itemId, MarketDbContext context)
        {
            storeInventory.DeleteItem(itemId, context);
        }

        public Item EditItem(Guid itemId, Dictionary<StoresUtils.ItemEditDetails, object> detailsToEdit, MarketDbContext context)
        {
            return storeInventory.EditItem(itemId, detailsToEdit, context);
        }

        public bool AddCategoryItem(Guid itemId, string category, MarketDbContext context)
        {
            return storeInventory.AddCategoryItem(itemId, category, context);
        }

        public bool UpdateCategoryItem(Guid itemId, string originalCategory, string updatedCategory, MarketDbContext context)
        {
            return storeInventory.UpdateCategoryItem(itemId, originalCategory, updatedCategory, context);
        }

        public bool DeleteCategoryItem(Guid itemId, string category, MarketDbContext context)
        {
            return storeInventory.DeleteCategoryItem(itemId, category, context);
        }

        public bool AddKeyWordItem(Guid itemId, string keyWord, MarketDbContext context)
        {
            return storeInventory.AddKeyWordItem(itemId, keyWord, context);
        }

        public bool UpdateKeyWordItem(Guid itemId, string originalKeyWord, string updatedKeyWord, MarketDbContext context)
        {
            return storeInventory.UpdateKeyWordItem(itemId, originalKeyWord, updatedKeyWord, context);
        }

        internal List<Guid> GetAllOwnerGuids()
        {
            return Certifications.Where(c => c.IsOwner()).ToList().ConvertAll(c => c.UserID);
        }

        public bool DeleteKeyWordItem(Guid itemId, string keyWord, MarketDbContext context)
        {
            return storeInventory.DeleteKeyWordItem(itemId, keyWord, context);
        }

        public ReadOnlyCollection<Item> SearchItems(MarketDbContext context, string name = null, string category = null,
            List<string> keywords = null, List<SearchFilter> filters = null)
        {
            Logger.writeEvent(string.Format("Store {0}: Searching", ContactDetails.Name));
            if (!CheckStoreForFilters(filters))
            {
                return new ReadOnlyCollection<Item>(new List<Item>() { });
            }
            return storeInventory.SearchItems(context, name, category, keywords, filters);
        }


        //Method assumes details are from correct types
        public bool EditStoreContactDetails(Dictionary<StoresUtils.StoreEditContactDetails, object> detailsToEdit, 
                                            MarketDbContext context)
        {
            object value;

            foreach (KeyValuePair<StoresUtils.StoreEditContactDetails, object> entry in detailsToEdit)
            {
                value = entry.Value;
                if (value == null)
                {
                    continue;
                }

                switch (entry.Key)
                {
                    case StoresUtils.StoreEditContactDetails.name:
                        ContactDetails.Name = (string)value;
                        break;
                    case StoresUtils.StoreEditContactDetails.email:
                        ContactDetails.Email = (string)value;
                        break;
                    case StoresUtils.StoreEditContactDetails.address:
                        ContactDetails.Address = (string)value;
                        break;
                    case StoresUtils.StoreEditContactDetails.phone:
                        ContactDetails.Phone = (string)value;
                        break;
                    case StoresUtils.StoreEditContactDetails.bankAccountNumber:
                        ContactDetails.BankAccountNumber = (string)value;
                        break;
                    case StoresUtils.StoreEditContactDetails.bank:
                        ContactDetails.Bank = (string)value;
                        break;
                    case StoresUtils.StoreEditContactDetails.description:
                        ContactDetails.Description = (string)value;
                        break;
                    default:
                        break;
                }
            }
            context.SaveChanges();
            return true;
        }

        public bool IsOrderItemsAmountAvailable(StoreCart cart, MarketDbContext context)
        {
            foreach (KeyValuePair<Guid, int> itemAmount in cart.Items)
            {
                storeInventory.IsItemAmountAvailableForPurchase(itemAmount.Key, itemAmount.Value);
            }
            return true;
        }

        public bool ReduceStoreInventoryDueToOrder(StoreCart cart, MarketDbContext context)
        {
            foreach (KeyValuePair<Guid, int> itemAmount in cart.Items)
            {
                storeInventory.ReduceItemAmount(itemAmount.Key, itemAmount.Value, context);
            }
            return true;
        }

        public List<Item> GetItemsById(List<Guid> itemIds, MarketDbContext context)
        {
            Logger.writeEvent("Store: GetItemsById| getting list of items by IDs");
            List<Item> items = new List<Item>();
            //LoadInventory(context);
            foreach (Guid id in itemIds)
            {
                items.Add(storeInventory.GetItemById(id));
            }
            Logger.writeEvent("Store: GetItemsById| found all items");
            return items;
        }

        public Item GetItemById(Guid itemId)
        {
            return storeInventory.GetItemById(itemId);
        }

        public ReadOnlyCollection<Item> GetStoreItems()
        {
            return storeInventory.GetStoreItems();
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }

            return !(obj is Store objAsStore) ? false : Equals(objAsStore);
        }

        public bool Equals(Store store) => Id.Equals(store.Id);

        public override int GetHashCode() => HashCode.Combine(Id);

        private bool CheckStoreForFilters(List<SearchFilter> filters)
        {
            if (filters == null) // no filters
            {
                return true;
            }
            Logger.writeEvent(string.Format("Store {0}: verifying store filters", ContactDetails.Name));
            foreach (SearchFilter sf in filters)
            {
                if (!sf.DoesStoreStandInFilter(this))
                {
                    Logger.writeEvent(string.Format("Store {0}: filter failed", ContactDetails.Name));
                    return false;
                }
            }
            Logger.writeEvent(string.Format("Store {0}: Passed all filters", ContactDetails.Name));
            return true;
        }

        public void EnsurePermission(Guid userID, Permission request, MarketDbContext context)
        {
            if (!TryGetCertification(userID, out Certification cert))
            {
                Logger.writeEvent(string.Format("Store: EnsurePermission| User: {0} has no certification in store: {1}",
                                                                userID, Id));
                throw new CertificationException(string.Format("No certification exists for userID: {0}", userID));
            }
            if (!cert.IsPermitted(request))
            {
                Logger.writeEvent(string.Format("Store: EnsurePermission| User: {0} lacks permission for action \"{1}\" in store: {2}",
                                                                userID, request, Id));
                throw new PermissionException(string.Format("User is not permitted to perform action \"{0}\"", request));
            }
        }

        private bool TryGetCertification(Guid userID, out Certification cert)
        {
            cert = Certifications.Where(c => c.UserID == userID).SingleOrDefault();
            return !(cert is null);
        }

        public void AddPermission(Guid userID, Guid grantor, Permission toAdd, MarketDbContext context)
        {
            if (!TryGetCertification(userID, out Certification cert))
            {
                Logger.writeEvent(string.Format("Store: AddPermission| User: {0} has no certification in store: {1}",
                                                                userID, Id));
                throw new CertificationException(string.Format("No certification exists for userID: {0}", userID));
            }
            if (!cert.IsGrantor(grantor))
            {
                Logger.writeEvent(string.Format("Store: AddPermission| User: {0} is not the grantor of user: {1} in store: {2}",
                                                                grantor, userID, Id));
                throw new NonGrantorException(string.Format("User {0} is not grantor of user {1}", grantor, userID));
            }
            cert.AddPermission(toAdd);
            context.SaveChanges();
        }

        public void RemovePermission(Guid userID, Guid grantor, Permission toRemove, MarketDbContext context)
        {
            if (!TryGetCertification(userID, out Certification cert))
            {
                Logger.writeEvent(string.Format("Store: RemovePermission| User: {0} has no certification in store: {1}",
                                                                userID, Id));
                throw new CertificationException(string.Format("No certification exists for userID: {0}", userID));
            }
            if (!cert.IsGrantor(grantor))
            {
                Logger.writeEvent(string.Format("Store: RemovePermission| User: {0} is not the grantor of user: {1} in store: {2}",
                                                                grantor, userID, Id));
                throw new NonGrantorException(string.Format("User {0} is not grantor of user {1}", grantor, userID));
            }
            cert.RemovePermission(toRemove);
            context.SaveChanges();
        }

        private void AddCertification(Guid userID, bool owner, Guid grantor, ISet<Permission> permissions, MarketDbContext context)
        {
            if (TryGetCertification(userID, out _))
            {
                Logger.writeEvent(string.Format("Store: AddCertfication| User: {0} already has a certification in store: {1}",
                    userID, Id));
                throw new CertificationException(string.Format("A certification already exists for userID: {0}", userID));
            }
            Certification newCert = new Certification(userID, Id, grantor, owner, permissions);
            if (!grantor.Equals(Guid.Empty))
            {
                if (!TryGetCertification(grantor, out Certification cert))
                {
                    Logger.writeEvent(string.Format("Store: AddCertfication| Grantor: {0} doesn't have a certification in store: {1}",
                                                                        grantor, Id));
                    throw new CertificationException(string.Format("No certification exists for userID: {0}", grantor));
                }
                cert.AddGrantedByMe(newCert);
            }
            RegisteredUser user = context.Users.Find(userID);
            user.Certifications.Add(newCert);
            Certifications.Add(newCert);
            context.Certifications.Add(newCert);
            context.SaveChanges();
            Logger.writeEvent(string.Format("Store: AddCertification| Added new Certification for user {0}", userID));
        }

        public void RemoveOwner(Guid userID, Guid grantor, INotificationSubject notificationSubject, MarketDbContext context)
        {
            if (!TryGetCertification(userID, out Certification cert))
            {
                Logger.writeEvent(string.Format("Store: RemoveOwner| User: {0} doesn't have a certification in store: {1}",
                                                                    userID, Id));
                throw new CertificationException(string.Format("No certification exists for userID: {0}", userID));
            }
            if (!cert.IsOwner())
            {
                Logger.writeEvent(string.Format("Store: RemoveOwner| User: {0} is a manager and not an owner in store: {1}",
                                                                    userID, Id));
                throw new CertificationException(string.Format("User {0} is a manager and not a store owner", userID));
            }
            if (!cert.IsGrantor(grantor))
            {
                Logger.writeEvent(string.Format("Store: RemoveOwner| User: {0} is not the grantor of user: {1} in store: {2}",
                                                                    grantor, userID, Id));
                throw new NonGrantorException(string.Format("User {0} is not grantor of user {1}", grantor, userID));
            }
            removeGrantedByUser(userID, cert, notificationSubject, context);
            RemoveCertification(cert, grantor, context);
            notificationSubject.notifyEvent(new OwnerRemovedEvent(userID, ContactDetails.Name), context);
        }

        public void RemoveManager(Guid userID, Guid grantor, INotificationSubject notificationSubject, MarketDbContext context)
        {
            if (!TryGetCertification(userID, out Certification cert))
            {
                Logger.writeEvent(string.Format("Store: RemoveManager| User: {0} doesn't have a certification in store: {1}",
                                                                    userID, Id));
                throw new CertificationException(string.Format("No certification exists for userID: {0}", userID));
            }
            if (cert.IsOwner())
            {
                Logger.writeEvent(string.Format("Store: RemoveManager| User: {0} is an owner and not a manager in store: {1}",
                                                                    userID, Id));
                throw new CertificationException(string.Format("User {0} is a store owner and not a manager", userID));
            }
            if (!cert.IsGrantor(grantor))
            {
                Logger.writeEvent(string.Format("Store: RemoveManager| User: {0} is not the grantor of user: {1} in store: {2}",
                                                                    grantor, userID, Id));
                throw new NonGrantorException(string.Format("User {0} is not grantor of user {1}", grantor, userID));
            }
            removeGrantedByUser(userID, cert, notificationSubject, context);
            RemoveCertification(cert, grantor, context);
        }

        private void removeGrantedByUser(Guid userID, Certification userCert, INotificationSubject notificationSubject, MarketDbContext context)
        {
            foreach (Guid grantedByUser in userCert.GetUsersIDsGrantedByMe())
            {
                try
                {
                    RemoveOwner(grantedByUser, userID, notificationSubject, context);
                }
                catch (CertificationException ce)
                {
                    if (ce.Message.Contains("is a manager and not a store owner"))
                        RemoveManager(grantedByUser, userID, notificationSubject, context);
                    else
                        throw ce;
                }
            }
        }

        private void RemoveCertification(Certification cert, Guid grantorID, MarketDbContext context)
        {
            List<StoreOwnerApointmentContract> contractsToRemove = new List<StoreOwnerApointmentContract>();
            List<StoreOwnerApointmentContract> contractsToApprove = new List<StoreOwnerApointmentContract>();
            if (!TryGetCertification(grantorID, out Certification grantorCert))
            {
                throw new CertificationException(string.Format("No certification exists for userID: {0}", grantorID));
            }
            foreach (StoreOwnerApointmentContract contract in Contracts)
            {
                //remove all contracts granted by this user
                if (contract.GrantorID == cert.UserID)
                {
                    contractsToRemove.Add(contract);
                }
                //approve all contracts waiting this user - this is equivlent to makring them as not needed approval from this user.
                else if (contract.isAwaitingApprovalFrom(cert.UserID))
                {
                    contractsToApprove.Add(contract);
                }
            }
            contractsToApprove.ForEach((StoreOwnerApointmentContract c) => ApproveContractAndAddCertIfApproved(c.GranteeID, cert.UserID, context));
            contractsToRemove.ForEach((StoreOwnerApointmentContract c) => Contracts.Remove(c));

            grantorCert.RemoveGrantedByMe(cert);
            Certifications.Remove(cert);
            context.Certifications.Remove(cert);
            context.SaveChanges();
            Logger.writeEvent(string.Format("Store: RemoveCertification| Removed Certification for user: {0} in store: {1}",
                                                                cert.UserID, Id));

        }

        private void AddFirstOwner(Guid owner, MarketDbContext context)
        {
            Logger.writeEvent(string.Format("Store: AddFirstOwner| Appointing First Owner: {0}", owner));
            Certification newCert = new Certification(owner, Id, null, true, null);
            Certifications.Add(newCert);
            context.Certifications.Add(newCert);
            RegisteredUser user = context.Users.Find(owner);
            user.Certifications.Add(newCert);
            Logger.writeEvent(string.Format("Store: AddCertification| Added new Certification for user {0}", owner));
        }

        public void AddOwner(Guid userID, Guid grantor, string userName, INotificationSubject notificationSubject, MarketDbContext context)
        {
            Logger.writeEvent(string.Format("Store: AddOwner| Appointing Owner: {0} by grantor: {1} to store: {2}",
                                                            userID, grantor, Id));
            if (!TryGetCertification(grantor, out Certification grantorCert))
            {
                Logger.writeEvent(string.Format("Store: AddAppointmentContract| User: {0} doesn't have a certification in store: {1}",
                                                                    grantor, Id));
                throw new CertificationException(string.Format("No certification exists for userID: {0}", grantor));
            }
            AddAppointmentContract(userID, grantorCert.UserID, userName, notificationSubject, context);
            ApproveContractAndAddCertIfApproved(userID, grantorCert.UserID, context);
        }

        public void ApproveContract(Guid userToApprove, Guid approver, MarketDbContext context)
        {
            if (!TryGetCertification(approver, out _))
            {
                Logger.writeEvent(string.Format("Store: AddAppointmentContract| User: {0} doesn't have a certification in store: {1}",
                                                                    approver, Id));
                throw new CertificationException(string.Format("No certification exists for userID: {0}", approver));
            }
            ApproveContractAndAddCertIfApproved(userToApprove, approver, context);
        }

        public bool HasAwaitingContract(Guid userID)
        {
            return TryGetContract(userID, out _);
        }

        public bool isApproverOf(Guid approver, Guid userToApprove, MarketDbContext context)
        {
            if (!TryGetCertification(approver, out Certification approverCert))
            {
                throw new CertificationException(string.Format("No certification exists for userID: {0}", approver));
            }

            if (!TryGetContract(userToApprove, out StoreOwnerApointmentContract contract))
            {
                Logger.writeEvent(string.Format("Store: isApproverOf| User: {0} does not have a contract in store: {1}",
                    userToApprove, Id));
                throw new ContractNotFoundException();
            }

            return contract.isAwaitingApprovalFrom(approverCert.UserID);
        }

        private bool TryGetContract(Guid granteeID, out StoreOwnerApointmentContract contract)
        {
            contract = Contracts.Where(c => c.GranteeID == granteeID).SingleOrDefault();
            return !(contract is null);
        }

        private void ApproveContractAndAddCertIfApproved(Guid userID, Guid approver, MarketDbContext context)
        {
            if (!TryGetContract(userID, out StoreOwnerApointmentContract contract))
            {
                Logger.writeEvent(string.Format("Store: ApproveContractAndAddCertIfApproved| User: {0} does not have a contract in store: {1}",
                    userID, Id));
                throw new ContractNotFoundException();
            }

            if (!contract.isAwaitingApprovalFrom(approver))
            {
                Logger.writeEvent(string.Format("Store: ApproveContractAndAddCertIfApproved| User: {0} is not an approver for {1}",
                    approver, userID));
                return;
            }
            contract.approve(approver);
            if(contract.isApproved())
            {
                AddCertification(userID, true, contract.GrantorID, null, context);
                Contracts.Remove(contract);
            }    
            context.SaveChanges();
        }

        private void AddAppointmentContract(Guid userID, Guid grantor, string userName, INotificationSubject notificationSubject, MarketDbContext context)
        {
            if (TryGetCertification(userID, out _))
            {
                Logger.writeEvent(string.Format("Store: AddAppointmentContract| User: {0} already has a certification in store: {1}",
                    userID, Id));
                throw new CertificationException(string.Format("A certification already exists for userID: {0}", userID));
            }
            if (awaitingOwnerContracts.ContainsKey(userID))
            {
                Logger.writeEvent(string.Format("Store: AddAppointmentContract| User: {0} already has a contract waiting in store: {1}",
                    userID, Id));
                throw new ContractFoundException(string.Format("A contract already exists for this user"));
            }

            List<Guid> needToApprove = Certifications.Where(c => c.IsOwner()).ToList().ConvertAll(c => c.UserID);

            StoreOwnerApointmentContract contract = new StoreOwnerApointmentContract(userID, Id, grantor, needToApprove);
            Contracts.Add(contract);
            needToApprove.ForEach((Guid approver) => {
                if (!approver.Equals(grantor)) {
                    notificationSubject.notifyEvent(new NeedToApproveContractEvent(approver, ContactDetails.Name, userName), context);
                }
            });
            context.SaveChanges();
            Logger.writeEvent(string.Format("Store: AddAppointmentContract| Added new Contract for user {0}", userID));

        }

        public void AddManager(Guid userID, Guid grantor, MarketDbContext context)//appoint manager with basic permissions
        {
            Logger.writeEvent(string.Format("Store: AddManager| Appointing Manager: {0} by grantor: {1} to store: {2}",
                                                            userID, grantor, Id));
            ISet<Permission> permissions = new HashSet<Permission>
            {
                Permission.HISTORY,
                Permission.REQUESTS
            };
            Logger.writeEvent("Store: generated basic permissions list for manager");
            AddCertification(userID, false, grantor, permissions, context);
        }

        public List<Permission> GetPermissions(Guid userID, Guid grantor, MarketDbContext context)
        {
            if (!TryGetCertification(userID, out Certification cert))
            {
                Logger.writeEvent(string.Format("Store: GetPermissions| User: {0} has no certification in store: {1}",
                                                                userID, Id));
                throw new CertificationException(string.Format("No certification exists for userID: {0}", userID));
            }
            if (!cert.IsGrantor(grantor))
            {
                Logger.writeEvent(string.Format("Store: GetPermissions| User: {0} is not the grantor of user: {1} in store: {2}",
                                                                grantor, userID, Id));
                throw new NonGrantorException(string.Format("User {0} is not grantor of user {1}", grantor, userID));
            }
            Logger.writeEvent(string.Format("Store: GetPermissions| Retrieving permissions of User: {0} in store: {1}", userID, Id));
            return new List<Permission>(cert.Permissions);
        }

        public List<Permission> GetPermissions(Guid userID, MarketDbContext context)
        {
            if (!TryGetCertification(userID, out Certification cert))
            {
                Logger.writeEvent(string.Format("Store: GetPermissions| User: {0} has no certification in store: {1}",
                                                                userID, Id));
                throw new CertificationException(string.Format("No certification exists for userID: {0}", userID));
            }
            Logger.writeEvent(string.Format("Store: GetPermissions| Retrieving permissions of User: {0} in store: {1}", userID, Id));
            return new List<Permission>(cert.Permissions);
        }

        public bool IsGrantorOf(Guid grantorID, Guid userID, MarketDbContext context)
        {
            if (!TryGetCertification(userID, out Certification cert))
            {
                throw new CertificationException(string.Format("No certification exists for userID: {0}", userID));
            }
            return cert.IsGrantor(grantorID);
        }

        public bool IsOwnerOfStore(Guid userID, MarketDbContext context)
        {
            if (!TryGetCertification(userID, out Certification cert))
            {
                return false;
            }
            return cert.IsOwner();
        }

        public bool IsManagerOfStore(Guid userID, MarketDbContext context)
        {
            if (!TryGetCertification(userID, out Certification cert))
            {
                return false;
            }
            return !cert.IsOwner();
        }

        public bool IsPermittedOperation(Guid userID, Permission permission, MarketDbContext context)
        {
            if (!TryGetCertification(userID, out Certification cert))
            {
                return false;
            }
            return cert.IsPermitted(permission);
        }

        public bool ValidatePurchase(StoreCart value, User user, MarketDbContext context) => PurchasePolicy.ValidatePurchase(value, user);//todo: context


        public List<Tuple<Item, int, double>> GetDiscountedPriceAndReserve(StoreCart cart, MarketDbContext context)//todo: context
        {
            ReduceStoreInventoryDueToOrder(cart, context);
            Dictionary<Guid, Tuple<int, double>> cartAsDict = convertCartToDiscountDict(cart, context);
            Dictionary<Guid, Tuple<int, double>> discountedCartAsDict = DiscountPolicy.GetPricesAfterDiscount(cartAsDict);
            return convertDiscountDictToItemTupleList(discountedCartAsDict, context);
        }

        private List<Tuple<Item, int, double>> convertDiscountDictToItemTupleList(Dictionary<Guid, Tuple<int, double>> cartAsDict, MarketDbContext context)
        {
            List<Tuple<Item, int, double>> retVal = new List<Tuple<Item, int, double>>();
            foreach (KeyValuePair<Guid, Tuple<int, double>> itemToAmountPrice in cartAsDict)
            {
                Item item = GetItemById(itemToAmountPrice.Key);
                int amount = itemToAmountPrice.Value.Item1;
                double totalPrice = itemToAmountPrice.Value.Item2;
                retVal.Add(new Tuple<Item, int, double>(item, amount, totalPrice/ amount));
            }
            return retVal;
        }

        private Dictionary<Guid, Tuple<int, double>> convertCartToDiscountDict(StoreCart cart, MarketDbContext context)
        {
            Dictionary<Guid, Tuple<int, double>> retVal = new Dictionary<Guid, Tuple<int, double>>();
            foreach (KeyValuePair<Guid, int> itemToAmount in cart.Items)
            {
                Item item = GetItemById(itemToAmount.Key);//throws if not found.
                double basePrice = item.Price;
                int amount = itemToAmount.Value;
                double total = basePrice * amount;
                retVal[itemToAmount.Key] = new Tuple<int, double>(amount, total);
            }
            return retVal;
        }

        public void IsOrderItemsAmountAvailable(Dictionary<Guid, int> items, MarketDbContext context)
        {
            foreach (KeyValuePair<Guid, int> orderItem in items)
            {
                storeInventory.IsItemAmountAvailableForPurchase(orderItem.Key, orderItem.Value);
            }
        }

        #region IStoreDiscountManager

        public OpenDiscount AddOpenDiscount(Guid itemID, double discount, DateTime dateUntil, MarketDbContext context)
        {
            Item i = GetItemById(itemID);//throws if item doesnt exist
            return DiscountPolicy.AddOpenDiscount(itemID, discount, dateUntil,i.Name, context);
        }

        public ItemConditionalDiscount_MinItems_ToDiscountOnAll AddItemConditionalDiscount_MinItems_ToDiscountOnAll(Guid itemID, double discount, int minItems, DateTime dateUntil, MarketDbContext context)
        {
            Item i = GetItemById(itemID);//throws if item doesnt exist
            return DiscountPolicy.AddItemConditionalDiscount_MinItems_ToDiscountOnAll(itemID, dateUntil, minItems, discount, i.Name, context);
        }

        public ItemConditionalDiscount_MinItems_ToDiscountOnExtraItems AddItemConditionalDiscount_MinItems_ToDiscountOnExtraItems(Guid itemID, double discountForExtra, int minItems, int extraItems, DateTime dateUntil, MarketDbContext context)
        {
            Item i = GetItemById(itemID);//throws if item doesnt exist
            return DiscountPolicy.AddItemConditionalDiscount_MinItems_ToDiscountOnExtraItems(itemID, dateUntil, minItems, extraItems, discountForExtra, i.Name, context);
        }

        public StoreConditionalDiscount AddStoreConditionalDiscount(double minPurchase, double discount, DateTime dateUntil, MarketDbContext context)
        {
            return DiscountPolicy.AddStoreConditionalDiscount(dateUntil, minPurchase, discount, context);
        }

        public bool DiscountExistsInStore(Guid discountID, MarketDbContext context)
        {
            return DiscountPolicy.DiscountExist(discountID);
        }

        public CompositeDiscount AddCompositeDiscount(Guid discountLeftID, Guid discountRightID, string boolOperator, MarketDbContext context)
        {
            if (!DiscountExistsInStore(discountLeftID, context))
            {
                throw new DiscountNotFoundException(discountLeftID);
            }
            if (!DiscountExistsInStore(discountRightID, context))
            {
                throw new DiscountNotFoundException(discountRightID);
            }
            Operator @operator = parseOperatorFromStringOrThrow(boolOperator, context);
            return DiscountPolicy.AddCompositeDiscount(discountLeftID, discountRightID, @operator, context);
        }

        private Operator parseOperatorFromStringOrThrow(string boolOperator, MarketDbContext context)
        {
            switch (boolOperator.ToLower().Trim())
            {
                case "&":
                case "and":
                    return Operator.AND;
                case "|":
                case "or":
                    return Operator.OR;
                case "xor":
                    return Operator.XOR;
                case ">":
                case "implies":
                    throw new NotSupportedException("> not supported in this version");
                default:
                    throw new ArgumentException("illegal argument string");

            }
        }

        public bool RemoveDiscount(Guid discountID, MarketDbContext context)
        {
            if (!DiscountExistsInStore(discountID, context))
            {
                throw new DiscountNotFoundException(discountID);
            }
            return DiscountPolicy.DeleteDiscount(discountID, context);
        }
        
        public bool IsDiscountAllowed(DiscountType discountType, MarketDbContext context)
        {
            return DiscountPolicy.IsDiscountTypeAllowed(discountType);
        }

        public bool MakeDiscountNotAllowed(DiscountType discountType, MarketDbContext context)
        {
            return DiscountPolicy.MakeDiscountNotAllowed(discountType, context);
        }

        public bool MakeDiscountAllowed(DiscountType discountType, MarketDbContext context)
        {
            return DiscountPolicy.MakeDiscountAllowed(discountType, context);
        }

        public List<DiscountType> GetAllowedDiscounts(MarketDbContext context)
        {
            return DiscountPolicy.GetAllowedDiscountTypes();
        }

        public List<ADisountDataClassForSerialization> GetAllDiscounts(Guid? itemID)
        {
            return DiscountPolicy.GetAllDiscounts(itemID);
        }

        public bool IsValidToCreateStoreConditionalDiscount(MarketDbContext context)
        {
            return DiscountPolicy.IsValidToCreateStoreConditionalDiscount();
        }

        public bool IsValidToCreateConditionalItemDiscount(Guid itemID, MarketDbContext context)
        {
            return DiscountPolicy.IsValidToCreateConditionalItemDiscount(itemID);
        }

        public bool IsValidToCreateOpenItemDiscount(Guid itemID, MarketDbContext context)
        {
            return DiscountPolicy.IsValidToCreateOpenItemDiscount(itemID);

        }

        public void ReturnItemsFromOrder(List<OrderItem> items, MarketDbContext context)
        {
            foreach (OrderItem item in items)
            {
                storeInventory.IncreaseItemAmount(item.ItemId, item.Amount);
            }
        }

        #endregion

        #region INotificationSubject


        public Guid GetGuid()
        {
            return Id;
        }

        #endregion

        public virtual bool ShouldSerializeStoreOrders()
        {
            return false;
        }

        #region PurchasePolicy

        public bool IsValidToCreateItemMinMaxPurchasePolicy(Guid itemID, MarketDbContext context)
        {
            return PurchasePolicy.IsValidToCreateItemMinMaxPurchasePolicy(itemID);
        }

        public bool IsValidToCreateStoreMinMaxPurchasePolicy(MarketDbContext context)
        {
            return PurchasePolicy.IsValidToCreateStoreMinMaxPurchasePolicy();
        }

        public bool IsValidToCreateDaysNotAllowedPurchasePolicy(MarketDbContext context)
        {
            return PurchasePolicy.IsValidToCreateDaysNotAllowedPurchasePolicy();
        }

        public ItemMinMaxPurchasePolicy AddItemMinMaxPurchasePolicy(Guid itemID, int? minAmount, int? maxAmount, MarketDbContext context)
        {
            Item i = GetItemById(itemID);//throws if item doesnt exist
            return PurchasePolicy.AddItemMinMaxPurchasePolicy(itemID, minAmount, maxAmount, i.Name, context);
        }

        public StoreMinMaxPurchasePolicy AddStoreMinMaxPurchasePolicy(int? minAmount, int? maxAmount, MarketDbContext context)
        {
            return PurchasePolicy.AddStoreMinMaxPurchasePolicy(minAmount, maxAmount, ContactDetails.Name, context);
        }

        public DaysNotAllowedPurchasePolicy AddDaysNotAllowedPurchasePolicy(int[] daysNotAllowed, MarketDbContext context)
        {
            return PurchasePolicy.AddDaysNotAllowedPurchasePolicy(daysNotAllowed, ContactDetails.Name, context);
        }

        public bool PolicyExistsInStore(Guid policyID, MarketDbContext context)
        {
            return PurchasePolicy.PolicyExistsInStore(policyID);
        }

        public CompositePurchasePolicy ComposeTwoPurchasePolicys(Guid policyLeftID, Guid policyRightID, string boolOperator, MarketDbContext context)
        {
            if (!PolicyExistsInStore(policyLeftID, context))
            {
                throw new PolicyNotFoundException(policyLeftID);
            }
            if (!PolicyExistsInStore(policyRightID, context))
            {
                throw new PolicyNotFoundException(policyRightID);
            }
            Operator @operator = parseOperatorFromStringOrThrow(boolOperator, context);
            return PurchasePolicy.ComposeTwoPurchasePolicys(policyLeftID, policyRightID, @operator, context);
        }

        public bool IsPurchaseTypeAllowed(PurchasePolicyType policy, MarketDbContext context)
        {
            return PurchasePolicy.IsPurchaseTypeAllowed(policy);
        }

        public bool MakePurcahsePolicyNotAllowed(PurchasePolicyType policy, MarketDbContext context)
        {
            return PurchasePolicy.MakePurcahsePolicyNotAllowed(policy, context);
        }

        public bool MakePurcahsePolicyAllowed(PurchasePolicyType policy, MarketDbContext context)
        {
            return PurchasePolicy.MakePurcahsePolicyAllowed(policy, context);
        }

        public bool RemovePurchasePolicy(Guid policyID, MarketDbContext context)
        {
            if (!PolicyExistsInStore(policyID, context))
            {
                throw new PolicyNotFoundException(policyID);
            }
            return PurchasePolicy.DeletePolicy(policyID, context);
        }

        public List<APurchasePolicyDataClassForSerialization> GetAllPurchasePolicys()
        {
            return PurchasePolicy.GetAllPurchasePolicys();
        }

        public List<PurchasePolicyType> GetAllowedPurchasePolicys(MarketDbContext context)
        {
            return PurchasePolicy.GetAllowedPurchasePolicys();
        }

        public string GetName()
        {
            return ContactDetails.Name;
        }

        #endregion
    }
}

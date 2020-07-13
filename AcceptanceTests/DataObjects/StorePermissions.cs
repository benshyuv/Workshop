using System;
using System.Collections.Generic;

namespace AcceptanceTests.DataObjects
{
    public class StorePermissions
    {



        public bool? ManageStorageInventory { get; set; }
        public bool? AppointOwner { get; set; }
        public bool? AppointManager { get; set; }
        public bool? EditManagingPermissions { get; set; }
        public bool? RemoveManager { get; set; }
        public bool? RemoveOwner { get; set; }

        //default is true
        public bool? ViewPurchaseHistory { get; set; }

        public bool? ManagePolicy { get; set; }

        //not in this iteration


        public StorePermissions()
        {
            ManageStorageInventory = false;
            AppointOwner = false;
            AppointManager = false;
            EditManagingPermissions = false;
            RemoveManager = false;
            ViewPurchaseHistory = true;
            ManagePolicy = false;
            RemoveOwner = false;
        }

        //ctor from json
        public StorePermissions(string[] json)
        {
            foreach(string permission in json)
            {
                setPermissionByString(permission);
            }
        }

        private void setPermissionByString(string permission)
        {
            string toCompare = permission.Trim().ToLower();
            switch (toCompare)
            {
                case "inventory":
                    ManageStorageInventory = true;
                    break;
                case "appoint_owner":
                    AppointOwner = true;
                    break;
                case "appoint_manager":
                    AppointManager = true;
                    break;
                case "edit_permissions":
                    EditManagingPermissions = true;
                    break;
                case "remove_manager":
                    RemoveManager = true;
                    break;
                case "history":
                    ViewPurchaseHistory = true;
                    break;
                case "policy":
                    ManagePolicy = true;
                    break;
                case "remove_owner":
                    RemoveOwner = true;
                    break;
                //not implemented in v1, but domain layer is allowed to return these strings.
                case "close_store":
                case "requests":
                    break;
                default:
                    throw new ArgumentException(permission + "is not legal permission string");
            }
        }

        public StorePermissions fullPermissions()
        {
            ManageStorageInventory = true;
            AppointOwner = true;
            AppointManager = true;
            EditManagingPermissions = true;
            RemoveManager = true;
            ViewPurchaseHistory = true;
            ManagePolicy = true;
            RemoveOwner = true;
            return this;
        }


        public StorePermissions noPermissions()
        {
            ManageStorageInventory = false;
            AppointOwner = false;
            AppointManager = false;
            EditManagingPermissions = false;
            RemoveManager = false;
            ViewPurchaseHistory = false;
            ManagePolicy = false;
            RemoveOwner = false;
            return this;
        }

        internal bool isManagingRights()
        {
            return (ManageStorageInventory is bool msi && msi) ||
                   (AppointManager is bool am && am )||
                   (AppointOwner is bool ao && ao) ||
                   (EditManagingPermissions is bool emp && emp) ||
                   (RemoveManager is bool rm && rm) ||
                   (ViewPurchaseHistory is bool vph && vph)||
                   (ManagePolicy is bool mp && mp) ||
                   (RemoveOwner is bool ro && ro);
        }

        internal bool isOwnerRights()
        {
            return (ManageStorageInventory is bool msi && msi) &&
                   (AppointManager is bool am && am) &&
                   (AppointOwner is bool ao && ao) &&
                   (EditManagingPermissions is bool emp && emp) &&
                   (RemoveManager is bool rm && rm) &&
                   (RemoveOwner is bool ro && ro) &&
                   (ViewPurchaseHistory is bool vph && vph) ||
                   (ManagePolicy is bool mp && mp);
        }

        internal IEnumerable<Tuple<string, bool?>> getPermsAsTupleList()
        {
            List<Tuple<string, bool?>> ans = new List<Tuple<string, bool?>>();
            ans.Add(new Tuple<string, bool?>("inventory", ManageStorageInventory));
            ans.Add(new Tuple<string, bool?>("appoint_owner", AppointOwner));
            ans.Add(new Tuple<string, bool?>("appoint_manager", AppointManager));
            ans.Add(new Tuple<string, bool?>("edit_permissions", EditManagingPermissions));
            ans.Add(new Tuple<string, bool?>("remove_manager", RemoveManager));
            ans.Add(new Tuple<string, bool?>("history", ViewPurchaseHistory));
            ans.Add(new Tuple<string, bool?>("policy", ManagePolicy));
            ans.Add(new Tuple<string, bool?>("remove_owner", RemoveOwner));
            return ans;
        }

        internal bool hasPermission(string permissionName)
        {
            string toCompare = permissionName.Trim().ToLower();
            switch (toCompare)
            {
                case "inventory":
                    return (ManageStorageInventory is bool msi && msi);
                case "appoint_manager":
                    return (AppointManager is bool am && am);
                case "appoint_owner":
                    return (AppointOwner is bool ao && ao);
                case "edit_permissions":
                    return (EditManagingPermissions is bool emp && emp);
                case "remove_manager":
                    return (RemoveManager is bool rm && rm);
                case "history":
                    return (ViewPurchaseHistory is bool vph && vph);
                case "policy":
                    return (ManagePolicy is bool mnp && mnp);
                case "remove_owner":
                    return (RemoveOwner is bool ro && ro);
                //not implemented in v1, but domain layer is allowed to return these strings.
                case "close_store":
                case "requests":
                    return false;
                default:
                    throw new ArgumentException(permissionName + "is not legal permission string");
            }


        }

    }
}


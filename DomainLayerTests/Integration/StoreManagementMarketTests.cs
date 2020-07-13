using DomainLayer.Exceptions;
using DomainLayer.Stores;
using DomainLayer.Stores.Certifications;
using DomainLayer.Users;
using Newtonsoft.Json;
using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace DomainLayerTests.Integration
{
    class StoreManagementMarketTests : IMarketFacadeTests
	{
        private const string PERM_INV = "inventory";
        private const string PERM_HIS = "HisTory";
        private const string PERM_BAD = "nothing";

        [SetUp]
		public new void Setup()
		{
			SetupUsers();
			SetupStores();
        }


        [Test]
        public void AppointOwner_Owner_ShouldPass()
        {
            int oldcount = GetTodayStats()[(int)Roles.OWNER];
            Assert.DoesNotThrow(() => LoginSessionSuccess(REGISTERED_SESSION_ID, FIRST_OPENER_USERNAME));
            Assert.AreEqual(oldcount + 1, GetTodayStats()[(int)Roles.OWNER]);
            Assert.DoesNotThrow(() => AppointOwnerSuccess(REGISTERED_SESSION_ID, FIRST_STORE_ID, FIRST_OWNER_USERNAME));
            Assert.True(marketFacade.IsOwnerOfStore(FIRST_STORE_ID, FIRST_OWNER_USERNAME));

        }

        [Test]
        public void AppointOwner_Owner_NeedsApporval_ShouldPass()
        {
            int oldcount = GetTodayStats()[(int)Roles.OWNER];
            Assert.DoesNotThrow(() => LoginSessionSuccess(REGISTERED_SESSION_ID, FIRST_OPENER_USERNAME));
            Assert.AreEqual(oldcount + 1, GetTodayStats()[(int)Roles.OWNER]); Assert.DoesNotThrow(() => AppointOwnerSuccess(REGISTERED_SESSION_ID, FIRST_STORE_ID, FIRST_OWNER_USERNAME));
            Assert.True(marketFacade.IsOwnerOfStore(FIRST_STORE_ID, FIRST_OWNER_USERNAME));
            Assert.DoesNotThrow(() => AppointOwnerSuccess(REGISTERED_SESSION_ID, FIRST_STORE_ID, SECOND_OWNER_USERNAME));
            Assert.False(marketFacade.IsOwnerOfStore(FIRST_STORE_ID, SECOND_OWNER_USERNAME));
            Assert.True(marketFacade.IsOwnerOfStore(FIRST_STORE_ID, FIRST_OWNER_USERNAME));
            LogoutSessionSuccess(REGISTERED_SESSION_ID);
            LoginSessionSuccess(REGISTERED_SESSION_ID, FIRST_OWNER_USERNAME);
            Assert.True(marketFacade.IsAwaitingContractApproval(FIRST_STORE_ID, SECOND_OWNER_USERNAME));
            Assert.True(marketFacade.IsApproverOfContract(REGISTERED_SESSION_ID, FIRST_STORE_ID, SECOND_OWNER_USERNAME));
            Assert.DoesNotThrow(() => ApproveOwnerSuccess(REGISTERED_SESSION_ID, FIRST_STORE_ID, SECOND_OWNER_USERNAME));
            Assert.False(marketFacade.IsAwaitingContractApproval(FIRST_STORE_ID, SECOND_OWNER_USERNAME));
            Assert.True(marketFacade.IsOwnerOfStore(FIRST_STORE_ID, SECOND_OWNER_USERNAME));
            LogoutSessionSuccess(REGISTERED_SESSION_ID);
        }

        [Test]
        public void AppointOwnerNotifyHasContract_shouldPass() {
            AppointOwner_Owner_NeedsApporval_ShouldPass();
            LoginSessionSuccess(REGISTERED_SESSION_ID, FIRST_OWNER_USERNAME);
            List<string> messages = DeserializeGetMyMessagesSuccess(REGISTERED_SESSION_ID);
            Assert.True(messages.Count >= 1);
            bool messageAdded = false;
            foreach (string message in messages)
            {
                string trimedLower = message.ToLower().Trim();
                if (trimedLower.Contains(String.Format("user: {0} is awaiting your approval on ownership contract in store {1}", SECOND_OWNER_USERNAME, FIRST_STORE_NAME).ToLower().Trim()))
                {
                    messageAdded = true;
                }
            }
            Assert.True(messageAdded);

        }

        [Test]
        public void AppointOwner_GuestUser_ShouldFail()
        {
            string json = AppointOwnerError(GUEST_SESSION_ID, FIRST_STORE_ID, FIRST_OWNER_USERNAME);
            Assert.IsNotNull(json);
            UserStateException e = JsonConvert.DeserializeObject<UserStateException>(json);
            Assert.IsNotNull(e);
            Assert.IsTrue(e.Message.Contains("Get UserID"));
            Assert.IsTrue(e.Message.Contains("Guest"));
        }

        [Test]
        public void AppointOwner_NoCertification_ShouldFail()
        {
            LoginSessionSuccess(REGISTERED_SESSION_ID, BUYER_USERNAME);
            string json = AppointOwnerError(REGISTERED_SESSION_ID, FIRST_STORE_ID, FIRST_OWNER_USERNAME);
            Assert.IsNotNull(json);
            CertificationException e = JsonConvert.DeserializeObject<CertificationException>(json);
            Assert.IsNotNull(e);
            Assert.IsTrue(e.Message.ToLower().Contains("no certification"));
        }

        [Test]
        public void AppointOwner_NoPermission_ShouldFail()
        {
            AppointLoginManager(REGISTERED_SESSION_ID,FIRST_STORE_ID,FIRST_OPENER_USERNAME);
            string json = AppointOwnerError(REGISTERED_SESSION_ID, FIRST_STORE_ID, BUYER_USERNAME);
            Assert.IsNotNull(json);
            PermissionException e = JsonConvert.DeserializeObject<PermissionException>(json);
            Assert.IsNotNull(e);
            Assert.IsTrue(e.Message.ToLower().Contains("not permitted"));
        }

        [Test]
        public void AppointManager_Owner_ShouldPass()
        {
            Assert.DoesNotThrow(() => LoginAppointManager(REGISTERED_SESSION_ID, FIRST_STORE_ID, FIRST_OPENER_USERNAME, STORE_MANAGER_USERNAME));
        }

        [Test]
        public void AppointManager_GuestUser_ShouldFail()
        {
            string json = AppointManagerError(GUEST_SESSION_ID, FIRST_STORE_ID, STORE_MANAGER_USERNAME);
            Assert.IsNotNull(json);
            UserStateException e = JsonConvert.DeserializeObject<UserStateException>(json);
            Assert.IsNotNull(e);
            Assert.IsTrue(e.Message.Contains("Get UserID"));
            Assert.IsTrue(e.Message.Contains("Guest"));
        }

        [Test]
        public void AppointManager_NoCertification_ShouldFail()
        {
            LoginSessionSuccess(REGISTERED_SESSION_ID, BUYER_USERNAME);
            string json = AppointManagerError(REGISTERED_SESSION_ID, FIRST_STORE_ID, STORE_MANAGER_USERNAME);
            Assert.IsNotNull(json);
            CertificationException e = JsonConvert.DeserializeObject<CertificationException>(json);
            Assert.IsNotNull(e);
            Assert.IsTrue(e.Message.ToLower().Contains("no certification"));
        }

        [Test]
        public void AppointManager_NoPermission_ShouldFail()
        {
            AppointLoginManager(REGISTERED_SESSION_ID, FIRST_STORE_ID, FIRST_OPENER_USERNAME);
            string json = AppointManagerError(REGISTERED_SESSION_ID, FIRST_STORE_ID, BUYER_USERNAME);
            Assert.IsNotNull(json);
            PermissionException e = JsonConvert.DeserializeObject<PermissionException>(json);
            Assert.IsNotNull(e);
            Assert.IsTrue(e.Message.ToLower().Contains("not permitted"));
        }

        [Test]
        public void AddPermission_Owner_ShouldPass()
        {
            Assert.DoesNotThrow(() => LoginAppointManager(REGISTERED_SESSION_ID, FIRST_STORE_ID, FIRST_OPENER_USERNAME, STORE_MANAGER_USERNAME));
            Assert.DoesNotThrow(() => AddPermissionSuccess(REGISTERED_SESSION_ID, FIRST_STORE_ID, STORE_MANAGER_USERNAME, PERM_INV));
        }

        [Test]
        public void AddPermission_BadParse_ShouldFail()
        {
            Assert.DoesNotThrow(() => LoginAppointManager(REGISTERED_SESSION_ID, FIRST_STORE_ID, FIRST_OPENER_USERNAME, STORE_MANAGER_USERNAME));
            string json = AddPermissionError(REGISTERED_SESSION_ID, FIRST_STORE_ID, STORE_MANAGER_USERNAME, PERM_BAD);
            Assert.IsNotNull(json);
            Assert.IsTrue(json.Contains("illegal permission"));
        }

        [Test]
        public void AddPermission_Existing_ShouldFail()
        {
            Assert.DoesNotThrow(() => LoginAppointManager(REGISTERED_SESSION_ID, FIRST_STORE_ID, FIRST_OPENER_USERNAME, STORE_MANAGER_USERNAME));
            string json = AddPermissionError(REGISTERED_SESSION_ID, FIRST_STORE_ID, STORE_MANAGER_USERNAME, PERM_HIS);
            Assert.IsNotNull(json);
            UserStateException e = JsonConvert.DeserializeObject<UserStateException>(json);
            Assert.IsNotNull(e);
            Assert.IsTrue(e.Message.Contains("Can't add exsiting permission"));
        }

        [Test]
        public void AddPermission_GuestUser_ShouldFail()
        {
            string json = AddPermissionError(GUEST_SESSION_ID, FIRST_STORE_ID, STORE_MANAGER_USERNAME, PERM_INV);
            Assert.IsNotNull(json);
            UserStateException e = JsonConvert.DeserializeObject<UserStateException>(json);
            Assert.IsNotNull(e);
            Assert.IsTrue(e.Message.Contains("Get UserID"));
            Assert.IsTrue(e.Message.Contains("Guest"));
        }

        [Test]
        public void AddPermission_NoCertification_ShouldFail()
        {
            Assert.DoesNotThrow(() => LoginAppointManager(REGISTERED_SESSION_ID, FIRST_STORE_ID, FIRST_OPENER_USERNAME, STORE_MANAGER_USERNAME));
            Assert.DoesNotThrow(() => LogoutSessionSuccess(REGISTERED_SESSION_ID));
            LoginSessionSuccess(REGISTERED_SESSION_ID, BUYER_USERNAME);
            string json = AddPermissionError(REGISTERED_SESSION_ID, FIRST_STORE_ID, STORE_MANAGER_USERNAME, PERM_INV);
            Assert.IsNotNull(json);
            CertificationException e = JsonConvert.DeserializeObject<CertificationException>(json);
            Assert.IsNotNull(e);
            Assert.IsTrue(e.Message.ToLower().Contains("no certification"));
        }

        [Test]
        public void AddPermission_NotGrantor_ShouldFail()
        {
            Assert.DoesNotThrow(() => LoginAppointManager(REGISTERED_SESSION_ID, FIRST_STORE_ID, FIRST_OPENER_USERNAME, STORE_MANAGER_USERNAME));
            Assert.DoesNotThrow(() => AppointOwnerSuccess(REGISTERED_SESSION_ID, FIRST_STORE_ID, FIRST_OWNER_USERNAME));
            Assert.DoesNotThrow(() => LogoutSessionSuccess(REGISTERED_SESSION_ID));
            Assert.DoesNotThrow(() => LoginSessionSuccess(REGISTERED_SESSION_ID, FIRST_OWNER_USERNAME));
            string json = AddPermissionError(REGISTERED_SESSION_ID, FIRST_STORE_ID, STORE_MANAGER_USERNAME, PERM_INV);
            Assert.IsNotNull(json);
            NonGrantorException e = JsonConvert.DeserializeObject<NonGrantorException>(json);
            Assert.IsNotNull(e);
            Assert.IsTrue(e.Message.Contains("not grantor"));
        }

        [Test]
        public void AddPermission_NoPermission_ShouldFail()
        {
            Assert.DoesNotThrow(() => LoginAppointManager(REGISTERED_SESSION_ID, FIRST_STORE_ID, FIRST_OPENER_USERNAME, BUYER_USERNAME));
            Assert.DoesNotThrow(() => AppointManagerSuccess(REGISTERED_SESSION_ID, FIRST_STORE_ID, STORE_MANAGER_USERNAME));
            Assert.DoesNotThrow(() => LogoutSessionSuccess(REGISTERED_SESSION_ID));
            Assert.DoesNotThrow(() => LoginSessionSuccess(REGISTERED_SESSION_ID, STORE_MANAGER_USERNAME));
            string json = AddPermissionError(REGISTERED_SESSION_ID, FIRST_STORE_ID, BUYER_USERNAME, PERM_INV);
            Assert.IsNotNull(json);
            PermissionException e = JsonConvert.DeserializeObject<PermissionException>(json);
            Assert.IsNotNull(e);
            Assert.IsTrue(e.Message.ToLower().Contains("not permitted"));
        }

        [Test]
        public void RemovePermission_Owner_ShouldPass()
        {
            AddPermission_Owner_ShouldPass();
            Assert.DoesNotThrow(() => RemovePermissionSuccess(REGISTERED_SESSION_ID, FIRST_STORE_ID, STORE_MANAGER_USERNAME, PERM_INV));
        }

        [Test]
        public void RemovePermission_NonExisting_ShouldFail()
        {
            Assert.DoesNotThrow(() => LoginAppointManager(REGISTERED_SESSION_ID, FIRST_STORE_ID, FIRST_OPENER_USERNAME, STORE_MANAGER_USERNAME));
            string json = RemovePermissionError(REGISTERED_SESSION_ID, FIRST_STORE_ID, STORE_MANAGER_USERNAME, PERM_INV);
            Assert.IsNotNull(json);
            UserStateException e = JsonConvert.DeserializeObject<UserStateException>(json);
            Assert.IsNotNull(e);
            Assert.IsTrue(e.Message.Contains("Can't remove non exsiting permission"));
        }

        [Test]
        public void RemovePermission_BadParse_ShouldFail()
        {
            AddPermission_Owner_ShouldPass();
            string json = RemovePermissionError(REGISTERED_SESSION_ID, FIRST_STORE_ID, STORE_MANAGER_USERNAME, PERM_BAD);
            Assert.IsNotNull(json);
            Assert.IsTrue(json.Contains("illegal permission"));
        }

        [Test]
        public void RemovePermission_GuestUser_ShouldFail()
        {
            string json = RemovePermissionError(GUEST_SESSION_ID, FIRST_STORE_ID, STORE_MANAGER_USERNAME, PERM_INV);
            Assert.IsNotNull(json);
            UserStateException e = JsonConvert.DeserializeObject<UserStateException>(json);
            Assert.IsNotNull(e);
            Assert.IsTrue(e.Message.Contains("Get UserID"));
            Assert.IsTrue(e.Message.Contains("Guest"));
        }

        [Test]
        public void RemovePermission_NoCertification_ShouldFail()
        {
            AddPermission_Owner_ShouldPass();
            Assert.DoesNotThrow(() => LogoutSessionSuccess(REGISTERED_SESSION_ID));
            LoginSessionSuccess(REGISTERED_SESSION_ID, BUYER_USERNAME);
            string json = RemovePermissionError(REGISTERED_SESSION_ID, FIRST_STORE_ID, STORE_MANAGER_USERNAME, PERM_INV);
            Assert.IsNotNull(json);
            CertificationException e = JsonConvert.DeserializeObject<CertificationException>(json);
            Assert.IsNotNull(e);
            Assert.IsTrue(e.Message.ToLower().Contains("no certification"));
        }

        [Test]
        public void RemovePermission_NotGrantor_ShouldFail()
        {
            Assert.DoesNotThrow(() => LoginAppointManager(REGISTERED_SESSION_ID, FIRST_STORE_ID, FIRST_OPENER_USERNAME, STORE_MANAGER_USERNAME));
            Assert.DoesNotThrow(() => AppointOwnerSuccess(REGISTERED_SESSION_ID, FIRST_STORE_ID, FIRST_OWNER_USERNAME));
            Assert.DoesNotThrow(() => AddPermissionSuccess(REGISTERED_SESSION_ID, FIRST_STORE_ID, STORE_MANAGER_USERNAME, PERM_INV));
            Assert.DoesNotThrow(() => LogoutSessionSuccess(REGISTERED_SESSION_ID));
            Assert.DoesNotThrow(() => LoginSessionSuccess(REGISTERED_SESSION_ID, FIRST_OWNER_USERNAME));
            string json = RemovePermissionError(REGISTERED_SESSION_ID, FIRST_STORE_ID, STORE_MANAGER_USERNAME, PERM_INV);
            Assert.IsNotNull(json);
            NonGrantorException e = JsonConvert.DeserializeObject<NonGrantorException>(json);
            Assert.IsNotNull(e);
            Assert.IsTrue(e.Message.Contains("not grantor"));
        }

        [Test]
        public void RemovePermission_NoPermission_ShouldFail()
        {
            Assert.DoesNotThrow(() => LoginAppointManager(REGISTERED_SESSION_ID, FIRST_STORE_ID, FIRST_OPENER_USERNAME, BUYER_USERNAME));
            Assert.DoesNotThrow(() => AppointManagerSuccess(REGISTERED_SESSION_ID, FIRST_STORE_ID, STORE_MANAGER_USERNAME));
            Assert.DoesNotThrow(() => AddPermissionSuccess(REGISTERED_SESSION_ID, FIRST_STORE_ID, STORE_MANAGER_USERNAME, PERM_INV));
            Assert.DoesNotThrow(() => LogoutSessionSuccess(REGISTERED_SESSION_ID));
            Assert.DoesNotThrow(() => LoginSessionSuccess(REGISTERED_SESSION_ID, STORE_MANAGER_USERNAME));
            string json = RemovePermissionError(REGISTERED_SESSION_ID, FIRST_STORE_ID, BUYER_USERNAME, PERM_INV);
            Assert.IsNotNull(json);
            PermissionException e = JsonConvert.DeserializeObject<PermissionException>(json);
            Assert.IsNotNull(e);
            Assert.IsTrue(e.Message.ToLower().Contains("not permitted"));

        }

        [Test]
        public void RemoveOwner_Grantor_ShouldPass()
        {
            AppointOwner_Owner_ShouldPass();
            Assert.DoesNotThrow(() => RemoveOwnerSuccess(REGISTERED_SESSION_ID, FIRST_STORE_ID, FIRST_OWNER_USERNAME));
        }

        [Test]
        public void OwnerRemovedNotificationEvent_basic_ShouldPass()
        {
            AppointOwner_Owner_ShouldPass();
            RemoveOwnerSuccess(REGISTERED_SESSION_ID, FIRST_STORE_ID, FIRST_OWNER_USERNAME);
            Guid sessionID = Guid.NewGuid();
            LoginSessionSuccess(sessionID, FIRST_OWNER_USERNAME, PASSWORD);
            List<string> messages = DeserializeGetMyMessagesSuccess(sessionID);
            Assert.True(messages.Count >= 1);
            bool messageAdded = false;
            foreach(string message in messages)
            {
                string trimedLower = message.ToLower().Trim();
                if(trimedLower.Contains(FIRST_STORE_NAME) && trimedLower.Contains("you have been removed from owning"))
                {
                    messageAdded = true;
                }
            }
            Assert.True(messageAdded);
        }

       

        [Test]
        public void RemoveOwner_GuestUser_ShouldFail()
        {
            string json = RemoveOwnerError(GUEST_SESSION_ID, FIRST_STORE_ID, FIRST_OPENER_USERNAME);
            Assert.IsNotNull(json);
            UserStateException e = JsonConvert.DeserializeObject<UserStateException>(json);
            Assert.IsNotNull(e);
            Assert.IsTrue(e.Message.Contains("Get UserID"));
            Assert.IsTrue(e.Message.Contains("Guest"));
        }

        [Test]
        public void RemoveOwner_NoCertification_ShouldFail()
        {
            LoginSessionSuccess(REGISTERED_SESSION_ID, BUYER_USERNAME);
            string json = RemoveOwnerError(REGISTERED_SESSION_ID, FIRST_STORE_ID, FIRST_OPENER_USERNAME);
            Assert.IsNotNull(json);
            CertificationException e = JsonConvert.DeserializeObject<CertificationException>(json);
            Assert.IsNotNull(e);
            Assert.IsTrue(e.Message.ToLower().Contains("no certification"));
        }

        [Test]
        public void RemoveOwner_NotGrantor_ShouldFail()
        {
            AppointOwner_Owner_ShouldPass();
            Assert.DoesNotThrow(() => LogoutSessionSuccess(REGISTERED_SESSION_ID));
            Assert.DoesNotThrow(() => LoginSessionSuccess(REGISTERED_SESSION_ID, FIRST_OWNER_USERNAME));
            string json = RemoveOwnerError(REGISTERED_SESSION_ID, FIRST_STORE_ID, FIRST_OPENER_USERNAME);
            Assert.IsNotNull(json);
            NonGrantorException e = JsonConvert.DeserializeObject<NonGrantorException>(json);
            Assert.IsNotNull(e);
            Assert.IsTrue(e.Message.ToLower().Contains("not grantor"));
        }

        [Test]
        public void RemoveOwner_NoPermission_ShouldFail()
        {
            AppointLoginManager(REGISTERED_SESSION_ID, FIRST_STORE_ID, FIRST_OPENER_USERNAME);
            string json = RemoveOwnerError(REGISTERED_SESSION_ID, FIRST_STORE_ID, FIRST_OPENER_USERNAME);
            Assert.IsNotNull(json);
            PermissionException e = JsonConvert.DeserializeObject<PermissionException>(json);
            Assert.IsNotNull(e);
            Assert.IsTrue(e.Message.ToLower().Contains("not permitted"));
        }

        [Test]
        public void RemoveManager_Grantor_ShouldPass()
        {
            AppointManager_Owner_ShouldPass();
            Assert.DoesNotThrow(() => RemoveManagerSuccess(REGISTERED_SESSION_ID, FIRST_STORE_ID, STORE_MANAGER_USERNAME));
        }

        [Test]
        public void RemoveManager_GuestUser_ShouldFail()
        {
            AppointManager_Owner_ShouldPass();
            string json = RemoveManagerError(GUEST_SESSION_ID, FIRST_STORE_ID, STORE_MANAGER_USERNAME);
            Assert.IsNotNull(json);
            UserStateException e = JsonConvert.DeserializeObject<UserStateException>(json);
            Assert.IsNotNull(e);
            Assert.IsTrue(e.Message.Contains("Get UserID"));
            Assert.IsTrue(e.Message.Contains("Guest"));
        }

        [Test]
        public void RemoveManager_NoCertification_ShouldFail()
        {
            Assert.DoesNotThrow(() => LoginSessionSuccess(REGISTERED_SESSION_ID, BUYER_USERNAME));
            string json = RemoveManagerError(REGISTERED_SESSION_ID, FIRST_STORE_ID, FIRST_OPENER_USERNAME);
            Assert.IsNotNull(json);
            CertificationException e = JsonConvert.DeserializeObject<CertificationException>(json);
            Assert.IsNotNull(e);
            Assert.IsTrue(e.Message.ToLower().Contains("no certification"));
        }

        [Test]
        public void RemoveManager_NotGrantor_ShouldFail()
        {
            AppointManager_Owner_ShouldPass();
            Assert.DoesNotThrow(() => AppointOwnerSuccess(REGISTERED_SESSION_ID, FIRST_STORE_ID, FIRST_OWNER_USERNAME));
            Assert.DoesNotThrow(() => LogoutSessionSuccess(REGISTERED_SESSION_ID));
            Assert.DoesNotThrow(() => LoginSessionSuccess(REGISTERED_SESSION_ID, FIRST_OWNER_USERNAME));
            string json = RemoveManagerError(REGISTERED_SESSION_ID, FIRST_STORE_ID, STORE_MANAGER_USERNAME);
            Assert.IsNotNull(json);
            NonGrantorException e = JsonConvert.DeserializeObject<NonGrantorException>(json);
            Assert.IsNotNull(e);
            Assert.IsTrue(e.Message.ToLower().Contains("not grantor"));
        }

        [Test]
        public void RemoveManager_NoPermission_ShouldFail()
        {
            Assert.DoesNotThrow(() => LoginAppointManager(REGISTERED_SESSION_ID, FIRST_STORE_ID, FIRST_OPENER_USERNAME, STORE_MANAGER_USERNAME));
            Assert.DoesNotThrow(() => AppointManagerSuccess(REGISTERED_SESSION_ID, FIRST_STORE_ID, BUYER_USERNAME));
            Assert.DoesNotThrow(() => LogoutSessionSuccess(REGISTERED_SESSION_ID));
            Assert.DoesNotThrow(() => LoginSessionSuccess(REGISTERED_SESSION_ID, STORE_MANAGER_USERNAME));
            string json = RemoveManagerError(REGISTERED_SESSION_ID, FIRST_STORE_ID, BUYER_USERNAME);
            Assert.IsNotNull(json);
            PermissionException e = JsonConvert.DeserializeObject<PermissionException>(json);
            Assert.IsNotNull(e);
            Assert.IsTrue(e.Message.ToLower().Contains("not permitted"));
        }

        [Test]
        public void GetMyPermissions_Owner_ShouldPass()
        {
            int oldcount = GetTodayStats()[(int)Roles.OWNER];
            Assert.DoesNotThrow(() => LoginSessionSuccess(REGISTERED_SESSION_ID, FIRST_OPENER_USERNAME));
            Assert.AreEqual(oldcount + 1, GetTodayStats()[(int)Roles.OWNER]); 
            List<string> permissions = null;
            Assert.DoesNotThrow(() => permissions = GetMyPermissionsSuccess(REGISTERED_SESSION_ID, FIRST_STORE_ID));
            Assert.IsNotNull(permissions);
            Permission[] allPermissions = (Permission[])Enum.GetValues(typeof(Permission));
            Assert.AreEqual(allPermissions.Length, permissions.Count);
            foreach (Permission p in allPermissions)
            {
                Assert.IsTrue(permissions.Contains(p.ToString()));
            }
        }

        [Test]
        public void GetStoresWithPermissions_Owner_ShouldPass()
        {
            int oldcount = GetTodayStats()[(int)Roles.OWNER];
            Assert.DoesNotThrow(() => LoginSessionSuccess(REGISTERED_SESSION_ID, SECOND_OPENER_USERNAME));
            Assert.AreEqual(oldcount + 1, GetTodayStats()[(int)Roles.OWNER]);
            AppointManagerSuccess(REGISTERED_SESSION_ID, SECOND_STORE_ID, FIRST_OPENER_USERNAME);
            LogoutSessionSuccess(REGISTERED_SESSION_ID);
            Assert.DoesNotThrow(() => LoginSessionSuccess(REGISTERED_SESSION_ID, FIRST_OPENER_USERNAME));
            Assert.AreEqual(oldcount + 2, GetTodayStats()[(int)Roles.OWNER]);

            List<Tuple<Store, List<string>>> storesWithPermissions = GetStoresWithPermissionsSuccess(REGISTERED_SESSION_ID);
            Permission[] allPermissions = (Permission[])Enum.GetValues(typeof(Permission));
            Assert.AreEqual(2, storesWithPermissions.Count);
            foreach(var tuple in storesWithPermissions)
            {
                if (tuple.Item1.Id.Equals(FIRST_STORE_ID))
                {
                    Assert.AreEqual(allPermissions.Length, tuple.Item2.Count);
                    foreach (Permission p in allPermissions)
                    {
                        Assert.IsTrue(tuple.Item2.Contains(p.ToString()));
                    }
                }
                //second store
                else
                {
                    Assert.AreEqual(SECOND_STORE_ID, tuple.Item1.Id);
                    Assert.AreEqual(2, tuple.Item2.Count);
                    Permission p = Permission.HISTORY;
                    Assert.IsTrue(tuple.Item2.Contains(p.ToString()));
                    p = Permission.REQUESTS;
                    Assert.IsTrue(tuple.Item2.Contains(p.ToString()));
                }
            }
            
        }
        [Test]
        public void GetStoresWithPermissions_noperms_ShouldPass()
        {
            LoginSessionSuccess(REGISTERED_SESSION_ID, BUYER_USERNAME);   

            List<Tuple<Store, List<string>>> storesWithPermissions = GetStoresWithPermissionsSuccess(REGISTERED_SESSION_ID);
            Assert.AreEqual(0, storesWithPermissions.Count);
        }


        [Test]
        public void GetMyPermissions_Manager_ShouldPass()
        {
            AppointLoginManager(REGISTERED_SESSION_ID, FIRST_STORE_ID, FIRST_OPENER_USERNAME);
            List<string> permissions = null;
            Assert.DoesNotThrow(() => permissions = GetMyPermissionsSuccess(REGISTERED_SESSION_ID, FIRST_STORE_ID));
            Assert.IsNotNull(permissions);
            List<Permission> basicPermissions = new List<Permission>(){
                Permission.HISTORY,
                Permission.REQUESTS
            };
            Assert.AreEqual(basicPermissions.Count, permissions.Count);
            foreach (Permission p in basicPermissions)
            {
                Assert.IsTrue(permissions.Contains(p.ToString()));
            }
        }

        [Test]
        public void GetMyPermissions_NoCertification_ShouldPass()
        {
            Assert.DoesNotThrow(() => LoginSessionSuccess(REGISTERED_SESSION_ID, BUYER_USERNAME));
            List<string> permissions = null;
            Assert.DoesNotThrow(() => permissions = GetMyPermissionsSuccess(REGISTERED_SESSION_ID, FIRST_STORE_ID));
            Assert.IsNotNull(permissions);
            Assert.AreEqual(0, permissions.Count);
        }

        [Test]
        public void GetMyPermissions_GuestUser_ShouldFail()
        {
            string json = GetMyPermissionsError(GUEST_SESSION_ID, FIRST_STORE_ID);
            Assert.IsNotNull(json);
            UserStateException e = JsonConvert.DeserializeObject<UserStateException>(json);
            Assert.IsNotNull(e);
            Assert.IsTrue(e.Message.Contains("Get UserID"));
            Assert.IsTrue(e.Message.Contains("Guest"));
        }
    }
}

using System;
using System.Collections.Generic;
using AcceptanceTests.DataObjects;
using NUnit.Framework;

namespace AcceptanceTests.tests
{
    public class StoreManagerTests : StoreOwnerTests
    {
        

        [SetUp]
        public override void Setup()
        {
            base.Setup();
            loggedInStore1Manager = systemOperations.getNewUserForTests();
            systemOperations.assignStoreManagerByOwner(store1.Id, loggedInStore1Manager);
            systemOperations.logout();
            systemOperations.login(loggedInStore1Manager.Username, loggedInStore1Manager.Pw);
            store1MangerGrantedByMe = systemOperations.getNewUserForTests();
            givePermissions();
            systemOperations.assignStoreManagerByOwner(store1.Id, store1MangerGrantedByMe);
        }

        [Test, TestCaseSource("addItemToStoreCases")]
        public override void UC_4_1_addItemToStore_successsOrFailScenario(Func<StoreOwnerTests, (Guid, Item)> thisToStoreToAddIDAndProductToAdd, bool success)
        {
            givePermissions();
            base.UC_4_1_addItemToStore_successsOrFailScenario(thisToStoreToAddIDAndProductToAdd, success);
            Setup();
            removePermissions();
            base.UC_4_1_addItemToStore_successsOrFailScenario(thisToStoreToAddIDAndProductToAdd, false);

        }

        [Test, TestCaseSource("deleteAndUpdateItemInStoreCases")]
        public override void UC_4_1_deletItemInStore_successsOrFailScenario(Func<StoreOwnerTests, (Guid, Item)> thisToStoreToDeleteIDAndProductToDelete, bool success)
        {
            givePermissions();
            base.UC_4_1_deletItemInStore_successsOrFailScenario(thisToStoreToDeleteIDAndProductToDelete, success);
            Setup();
            removePermissions();
            base.UC_4_1_deletItemInStore_successsOrFailScenario(thisToStoreToDeleteIDAndProductToDelete, false);
        }

        [Test, TestCaseSource("deleteAndUpdateItemInStoreCases")]
        public override void UC_4_1_updateItemInStore_successsOrFailScenario(Func<StoreOwnerTests, (Guid, Item)> thisToStoreToUpdateIDAndProductToUpdate, bool success)
        {
            givePermissions();
            base.UC_4_1_updateItemInStore_successsOrFailScenario(thisToStoreToUpdateIDAndProductToUpdate, success);
            Setup();
            removePermissions();
            base.UC_4_1_updateItemInStore_successsOrFailScenario(thisToStoreToUpdateIDAndProductToUpdate, false);
        }

       [Test]
       public void UC_4_2AddOpenedDiscount_ManagerHasNoPermission_FailScenario()
        {
            removePermissions();
            string answer = systemOperations.AddOpenedDiscountError(store1.Id, itemInStore1.Id, 0.02, 30);
            Assert.AreEqual("no permission", answer);
        }

        [Test]
        public void UC_4_2_AddItemConditionalDiscountOnAll_ManagerHasNoPermission_FailScenario()
        {
            removePermissions();
            string answer = systemOperations.AddItemConditionalDiscount_MinItems_ToDiscountOnAllError(store1.Id, itemInStore1.Id, 30, 5, 0.6);
            Assert.AreEqual("no permission", answer);
        }

        [Test]
        public void UC_4_2_AddItemConditional_DiscountOnExtraItems_ManagerHasNoPermission_FailScenario()
        {
            removePermissions();
            string answer = systemOperations.AddItemConditionalDiscount_MinItems_ToDiscountOnExtraItemsError(store1.Id, itemInStore1.Id, 30, 3, 1, 0.5);
            Assert.AreEqual("no permission", answer);
        }

        [Test]
        public void UC_4_2_AddStoreConditionalDiscount_ManagerHasNoPermission_FailScenario()
        {
            removePermissions();
            string answer = systemOperations.AddStoreConditionalDiscountError(store1.Id, 60, 270.50, 0.10);
            Assert.AreEqual("no permission", answer);
        }

        [Test]
        public virtual void UC_4_2_RemoveItemConditionalDiscountOnAll_ManagerHasNoPermission_FailScenario()
        {
            ItemConditionalDiscountOnAll c_d_all = systemOperations.AddItemConditionalDiscount_MinItems_ToDiscountOnAllSuccess(store1.Id, itemInStore1.Id, 20, 6, 0.2);
            removePermissions();
            Assert.AreEqual("no permission", systemOperations.RemoveDiscountError(store1.Id, c_d_all.DiscountID));
        }

        [Test]
        public virtual void UC_4_2_MakeDiscountAllowed_ManagerHasNoPermission_FailureScenario()
        {
            RemoveAllDiscountTypesFromStorePolicy();

            removePermissions();
            Assert.AreEqual("no permission", systemOperations.MakeDiscountAllowedError(store1.Id, "opened"));
        }

        [Test]
        public virtual void UC_4_2_MakeDiscountNotAllowed_ManagerHasNoPermission_FailureScenario()
        {
            RemoveAllDiscountTypesFromStorePolicy();

            removePermissions();
            Assert.AreEqual("no permission", systemOperations.MakeDiscountNotAllowedError(store1.Id, "opened"));
        }

        [Test]
        public virtual void UC_4_2_GetAllowedDiscounts_ManagerHasNoPermission_SuccessScenario()
        {
            removePermissions();
            Assert.AreEqual("no permission", systemOperations.GetAllowedDiscountsError(store1.Id));
        }

        
        public override void UC_4_3_AppoiningStoreOwner_successOrFailScenarios(Func<StoreOwnerTests, (Guid, User)> thisToStoreToAssingIDAnduser, bool success, bool calledFromManager)
        {
            //pass
        }

        [Test, TestCaseSource("apointstoreOwnerCases")]
        public void UC_4_3_AppoiningStoreOwner_successOrFailScenarios_manager(Func<StoreOwnerTests, (Guid, User)> thisToStoreToAssingIDAnduser, bool success, bool calledFromManager)
        {
            givePermissions();
            base.UC_4_3_AppoiningStoreOwner_successOrFailScenarios(thisToStoreToAssingIDAnduser, success, true);
            Setup();
            removePermissions();
            base.UC_4_3_AppoiningStoreOwner_successOrFailScenarios(thisToStoreToAssingIDAnduser, false, true);
        }

        [Test]
        public override void UC_4_3_getAllStoresBeforeApproveal_shouldPass()
        {
            //pass - not relevant
            Assert.Pass();
        }

        [Test, TestCaseSource("apointstoreOwnerOrManagerCases")]
        public override void UC_4_5_AppointingStoreManager_successOrFailScenarios(Func<StoreOwnerTests, (Guid, User)> thisToStoreToAssingIDAnduser, bool success)
        {
            givePermissions();
            base.UC_4_5_AppointingStoreManager_successOrFailScenarios(thisToStoreToAssingIDAnduser, success);
            Setup();
            removePermissions();
            base.UC_4_5_AppointingStoreManager_successOrFailScenarios(thisToStoreToAssingIDAnduser, false);
        }

        public static IEnumerable<TestCaseData> editManagerPermissionsCasesByManager()
        {
            //success
            yield return new TestCaseData(new Func<StoreOwnerTests, (Guid, User)>
                (testsObject => (testsObject.store1.Id, testsObject.store1MangerGrantedByMe)), permissions, true, null);
            //not manager
            yield return new TestCaseData(new Func<StoreOwnerTests, (Guid, User)>
                (testsObject => (testsObject.store1.Id, testsObject.registeredUser)), permissions, false, null);
            //not registered
            yield return new TestCaseData(new Func<StoreOwnerTests, (Guid, User)>
                (testsObject => (testsObject.store1.Id, new User("notRegistered", "password"))), permissions, false, null);
            //not valid  store id
            yield return new TestCaseData(new Func<StoreOwnerTests, (Guid, User)>
                (testsObject => (Guid.Empty, testsObject.manager1)), permissions, false, null);
            //notOwnerOfStore
            yield return new TestCaseData(new Func<StoreOwnerTests, (Guid, User)>
                (testsObject => (testsObject.store2.Id, testsObject.manager2)), permissions, false, null);

        }

        [Test, TestCaseSource("editManagerPermissionsCasesByManager")]
        public override void UC_4_6_editStoreManagerPermissions_successOrFailScenarios(Func<StoreOwnerTests, (Guid, User)> thisToStoreAndManager,
            List<Tuple<string, bool>> newPermissions, bool success, StoreOwnerTests thisObjNotUsed = null)
        {
            //newPermissions.ViewPurchaseHistory = null;
            //givePermissions();
            base.UC_4_6_editStoreManagerPermissions_successOrFailScenarios(thisToStoreAndManager, newPermissions, success, this);
            Setup();
            removePermissions();
            base.UC_4_6_editStoreManagerPermissions_successOrFailScenarios(thisToStoreAndManager, newPermissions, false, this);
        }




        [Test]
        public override void UC_4_6_GetStoresWithPermissions_SuccessScenario()
        {
            List<Tuple<Store, List<string>>> result = systemOperations.GetStoresWithPermissionsSuccess();

            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(9, result[0].Item2.Count);
        }

        [Test]
        public void UC_4_6_GetStoresWithPermissions_ManagerHasNoPermissions_FailureScenario()
        {
            removePermissions();

            List<Tuple<Store, List<string>>> result = systemOperations.GetStoresWithPermissionsSuccess();

            Assert.AreEqual(0, result[0].Item2.Count);
        }

        public static IEnumerable<TestCaseData> managerRemoveStoreManagerRightsCases()
        {
            //success
            yield return new TestCaseData(new Func<StoreOwnerTests, (Store, User, User)>
                (testsObject => (testsObject.store1, testsObject.store1MangerGrantedByMe, testsObject.loggedInStore1Manager)), true);
            //not manager
            yield return new TestCaseData(new Func<StoreOwnerTests, (Store, User, User)>
                (testsObject => (testsObject.store1, testsObject.registeredUser, testsObject.loggedInStore1Manager)), false);
            //notOwnerOfStore
            yield return new TestCaseData(new Func<StoreOwnerTests, (Store, User, User)>
                (testsObject => (testsObject.store2, testsObject.manager2, testsObject.loggedInStore1Manager)), false);

        }

        [Test, TestCaseSource("managerRemoveStoreManagerRightsCases")]
        public override void UC_4_7_removeStoreManagerRights_successAndFailScenario(Func<StoreOwnerTests, (Store, User, User)> thisToStoreManagerRemover,  bool success)
        {
            givePermissions();
            base.UC_4_7_removeStoreManagerRights_successAndFailScenario(thisToStoreManagerRemover, success);
            Setup();
            removePermissions();
            base.UC_4_7_removeStoreManagerRights_successAndFailScenario(thisToStoreManagerRemover, false);

        }

        public static IEnumerable<TestCaseData> removeOwnerFromManager()
        {
            //success
            yield return new TestCaseData(new Func<StoreOwnerTests, User>
                (testsObject => testsObject.loggedInStore1Manager), true, true
                );
          
        }

        [Test,TestCase(true)]
        public override void UC_4_4_RemoveStoreOwner_success(bool calledFromManager = true)
        {
            base.UC_4_4_RemoveStoreOwner_success(true);
        }



        [Test,TestCaseSource("removeOwnerFromManager")]
        public override void UC_4_4_UC_4_3_RemoveStoreOwner_and_assign_complex_withSubOwnersAndManagers_success(Func<StoreOwnerTests, User> thisToInitialUser, bool success, bool calledFromManager)
        {
            givePermissions();
            base.UC_4_4_UC_4_3_RemoveStoreOwner_and_assign_complex_withSubOwnersAndManagers_success(thisToInitialUser,true, true);
        }


        [TestCase(1)]
        [TestCase(2)]
        [TestCase(3)]
        public override void UC_4_10_viewStorePurchaseHistory_successScenario(int numberOfPurchases)
        {
            List<PurchaseRecord> purchasesMade = makeRandomPurchases(numberOfPurchases, store1, loggedInStore1Owner, false);
            CollectionAssert.AreEquivalent(purchasesMade, systemOperations.getStoreHistory(store1));

        }


        [TestCase(1)]
        [TestCase(2)]
        [TestCase(3)]
        public override void UC_4_10_viewStorePurchaseHistory_withPriceChanges_successScenario(int numberOfPurchases)
        {
            (List<PurchaseRecord> purchasesMade, Store store1) = viewStorePurchaseHistory_withPriceChanges_helper(numberOfPurchases, false);
            CollectionAssert.AreEquivalent(purchasesMade, systemOperations.getStoreHistory(store1));
        }

        [TestCase(1)]
        [TestCase(2)]
        [TestCase(3)]
        public override void UC_4_10_viewStorePurchaseHistory_withItemDeletions_successScenario(int numberOfPurchases)
        {

            (List<PurchaseRecord> purchasesMade, Store store1) = viewStorePurchaseHistory_withItemDeletions_helper(numberOfPurchases, false);
            CollectionAssert.AreEquivalent(purchasesMade, systemOperations.getStoreHistory(store1));

        }

        [TestCase(1)]
        [TestCase(2)]
        [TestCase(3)]
        public override void UC_4_10_veiwStorePurchaseHistory_notByOwner_failureScenario(int numberOfPurchase)
        {
            (List<PurchaseRecord> purchasesMade, Store store1) = viewStorePurchaseHistory_withPriceChanges_helper(numberOfPurchase, false);
            systemOperations.logout();
            systemOperations.login(registeredUser.Username, registeredUser.Pw);
            Assert.Null(systemOperations.getStoreHistory(store1));
        }

        private void removePermissions()
        {
            systemOperations.logout();
            systemOperations.login(loggedInStore1Owner.Username, loggedInStore1Owner.Pw);
            systemOperations.editManagerPermissions(store1.Id, loggedInStore1Manager, removeAllPermissionsList());
            systemOperations.logout();
            systemOperations.login(loggedInStore1Manager.Username, loggedInStore1Manager.Pw);
        }


        private void givePermissions()
        {
            Assert.True(systemOperations.logout());
            Assert.True(systemOperations.login(loggedInStore1Owner.Username, loggedInStore1Owner.Pw));
            systemOperations.editManagerPermissions(store1.Id, loggedInStore1Manager, extendToFullPermissionList());
            Assert.True(systemOperations.logout());
            Assert.True(systemOperations.login(loggedInStore1Manager.Username, loggedInStore1Manager.Pw));
        }
    }
}

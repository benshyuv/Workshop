using System;
using System.Collections.Generic;
using System.Linq;
using AcceptanceTests.DataObjects;
using AcceptanceTests.OperationsAPI;
using NUnit.Framework;

namespace AcceptanceTests.tests
{
    public class StoreOwnerTests
    {

        public ISystemOperationsBridge systemOperations { get; private set; }
        public User loggedInStore1Owner { get; private set; }
        public Store store1 { get; private set; }
        public User manager1;
        public User store2Owner;
        public Store store2;
        public User registeredUser;
        public User registeredUser2;
        public Item itemInStore1;
        public Item newItemExample;
        public Item itemInStore2;
        public User manager2;
        public User store1AdditionalOwner;
        public User loggedInStore1Manager;
        public User store1MangerGrantedByMe;
        public ItemOpenedDiscount openedDiscountExample;

        [SetUp]
        public virtual void Setup()
        {
            systemOperations = new SystemOperationsProxy();

            store2Owner = systemOperations.getNewUserForTests();
            systemOperations.login(store2Owner.Username, store2Owner.Pw);
            store2 = systemOperations.getNewStoreForTestsWithOwner();
            itemInStore2 = new Item("iphone", new List<string> { "phone" }, new List<string> { "apple" });
            itemInStore2 = systemOperations.addItemToStore(store2.Id, itemInStore2, 3);
            manager2 = systemOperations.getNewUserForTests();
            systemOperations.assignStoreManagerByOwner(store2.Id, manager2);
            systemOperations.logout();

            loggedInStore1Owner = systemOperations.getNewUserForTests();
            systemOperations.login(loggedInStore1Owner.Username, loggedInStore1Owner.Pw);
            store1 = systemOperations.getNewStoreForTestsWithOwner();
            itemInStore1 = new Item("bannana", new List<string> { "fruit" }, new List<string> { "fruit" });
            itemInStore1 = systemOperations.addItemToStore(store1.Id, itemInStore1, 3);

            manager1 = systemOperations.getNewUserForTests();
            systemOperations.assignStoreManagerByOwner(store1.Id, manager1);

            store1AdditionalOwner = systemOperations.getNewUserForTests();
            Assert.True(systemOperations.assignStoreOwnerByOwnerAndValidate_NoNeedForApprovers_FirstAssignment(store1.Id, store1AdditionalOwner));

            registeredUser = systemOperations.getNewUserForTests();
            registeredUser2 = systemOperations.getNewUserForTests();
            newItemExample = new Item("successItem", new List<string> { "keyword" }, new List<string> { "category" });
        }

        public static IEnumerable<TestCaseData> addItemToStoreCases()
        {
            //success
            yield return new TestCaseData(new Func<StoreOwnerTests, (Guid, Item)>
                (testsObject => (testsObject.store1.Id, testsObject.newItemExample)), true);
            //item already in store
            yield return new TestCaseData(new Func<StoreOwnerTests, (Guid, Item)>
                (testsObject => (testsObject.store1.Id, testsObject.itemInStore1)), false);
            //not valid id
            yield return new TestCaseData(new Func<StoreOwnerTests, (Guid, Item)>
                (testsObject => (Guid.Empty, testsObject.newItemExample)), false);
            //notOwnerOfStore
            yield return new TestCaseData(new Func<StoreOwnerTests, (Guid, Item)>
                (testsObject => (testsObject.store2.Id, testsObject.newItemExample)), false);
        }

        [Test, TestCaseSource("addItemToStoreCases")]
        public virtual void UC_4_1_addItemToStore_successsOrFailScenario(Func<StoreOwnerTests, (Guid, Item)> thisToStoreToAddIDAndProductToAdd, bool success)
        {
            (Guid storeToAddID, Item productToAdd) = thisToStoreToAddIDAndProductToAdd(this);
            Assert.AreEqual(success, systemOperations.addItemToStoreWithValidation(storeToAddID, productToAdd));
        }

        public static IEnumerable<TestCaseData> deleteAndUpdateItemInStoreCases()
        {
            //success
            yield return new TestCaseData(new Func<StoreOwnerTests, (Guid, Item)>
                (testsObject => (testsObject.store1.Id, testsObject.itemInStore1)), true);
            //item not in store
            yield return new TestCaseData(new Func<StoreOwnerTests, (Guid, Item)>
                (testsObject => (testsObject.store1.Id, testsObject.newItemExample)), false);
            //not valid  store id
            yield return new TestCaseData(new Func<StoreOwnerTests, (Guid, Item)>
                (testsObject => (Guid.Empty, testsObject.itemInStore1)), false);
            //notOwnerOfStore
            yield return new TestCaseData(new Func<StoreOwnerTests, (Guid, Item)>
                (testsObject => (testsObject.store2.Id, testsObject.itemInStore2)), false);
        }

        [Test, TestCaseSource("deleteAndUpdateItemInStoreCases")]
        public virtual void UC_4_1_deletItemInStore_successsOrFailScenario(Func<StoreOwnerTests, (Guid, Item)> thisToStoreToDeleteIDAndProductToDelete, bool success)
        {

            (Guid storeToDeleteFromID, Item productToDelete) = thisToStoreToDeleteIDAndProductToDelete(this);
            Assert.AreEqual(success, systemOperations.deleteItemFromStoreWithValidation(storeToDeleteFromID, productToDelete));
        }


        [Test, TestCaseSource("deleteAndUpdateItemInStoreCases")]
        public virtual void UC_4_1_updateItemInStore_successsOrFailScenario(Func<StoreOwnerTests, (Guid, Item)> thisToStoreToUpdateIDAndProductToUpdate, bool success)
        {
            (Guid storeToUpdateFromID, Item productToUpdate) = thisToStoreToUpdateIDAndProductToUpdate(this);
            productToUpdate.Categorys.Add(Utilitys.Utils.RandomAlphaNumericString(8));
            productToUpdate.Keywords.Add(Utilitys.Utils.RandomAlphaNumericString(8));
            productToUpdate.Name = Utilitys.Utils.RandomAlphaNumericString(8);
            Assert.AreEqual(success, systemOperations.updateItemFromStoreWithValidation(storeToUpdateFromID, productToUpdate));
        }

        [Test]
        public virtual void UC_4_1_updateItemInStore_CategoriesEmpty_ShouldFail()
        {
            Guid storeToUpdateFromID = store1.Id;
            Item productToUpdate = itemInStore1;
            productToUpdate.Categorys = new List<string>() { };
            productToUpdate.Keywords.Add(Utilitys.Utils.RandomAlphaNumericString(8));
            productToUpdate.Name = Utilitys.Utils.RandomAlphaNumericString(8);
            Assert.AreEqual(false, systemOperations.updateItemFromStoreWithValidation(storeToUpdateFromID, productToUpdate));
        }

        [Test]
        public virtual void UC_4_2_AddOpenedDiscount_Success()
        {
            ItemOpenedDiscount answer = systemOperations.AddOpenedDiscountSuccess(store1.Id, itemInStore1.Id, 0.20, 30);

            Assert.NotNull(answer);
            Assert.AreEqual(itemInStore1.Id, answer.ItemID);
            Assert.AreEqual(0.20, answer.Discount);
        }


        [Test]
        public virtual void UC_4_2_AddOpenedDiscount_StoreIdNotFound_ShouldFail()
        {
            string answer = systemOperations.AddOpenedDiscountError(Guid.NewGuid(), itemInStore1.Id, 0.20, 30);
            Assert.AreEqual("Store doesnt exist", answer);
        }

        [Test]
        public virtual void UC_4_2_AddOpenedDiscount_ItemIdNotInStore_ShouldFail()
        {
            string answer = systemOperations.AddOpenedDiscountError(store1.Id, itemInStore2.Id, 0.20, 30);
            Assert.AreEqual("item doesnt exist", answer);
        }

        [Test]
        public virtual void UC_4_2_AddOpenedDiscount_DiscountNegative_ShouldFail()
        {
            string answer = systemOperations.AddOpenedDiscountError(store1.Id, itemInStore1.Id, -4, 30);
            Assert.AreEqual("Invalid input", answer);
        }

        [Test]
        public virtual void UC_4_2_AddOpenedDiscount_OwnerOfOtherStore_ShouldFail()
        {
            systemOperations.logout();
            systemOperations.login(store2Owner.Username, store2Owner.Pw);
            string answer = systemOperations.AddOpenedDiscountError(store1.Id, itemInStore1.Id, 0.02, 30);
            Assert.AreEqual("no permission", answer);
        }

        [Test] // TODO: correct
        public virtual void UC_4_2_AddOpenedDiscount_OpenedDiscountOverlappingAnotherOpenedDiscount_ShouldFail()
        {
            ItemOpenedDiscount answer1 = systemOperations.AddOpenedDiscountSuccess(store1.Id, itemInStore1.Id, 0.20, 30);
            string answer2 = systemOperations.AddOpenedDiscountError(store1.Id, itemInStore1.Id, 0.50, 40);

            Assert.AreEqual("Discount not valid - overlapping with exsisting", answer2);
        }


        [Test]
        public virtual void UC_4_2_AddItemConditionalDiscountOnAll_SuccessScenario()
        {
            ItemConditionalDiscountOnAll answer = systemOperations.AddItemConditionalDiscount_MinItems_ToDiscountOnAllSuccess(store1.Id, itemInStore1.Id, 30, 5, 0.6);

            Assert.NotNull(answer);
            Assert.AreEqual(itemInStore1.Id, answer.ItemID);
            Assert.AreEqual(5, answer.NumOfItem);
            Assert.AreEqual(0.6, answer.Discount);
        }


        [Test, TestCaseSource("AddItemConditionalDiscountOnAllDataForFailScenarios")]
        public void UC_4_2_AddItemConditionalDiscountOnAll_FailScenarios(Func<StoreOwnerTests, (Guid, Guid)> thisToGetStoreIdAndItemId, int durationInDays, int minItems, double discount, string ExpectedAnswer)
        {
            (Guid storeID, Guid itemID) = thisToGetStoreIdAndItemId(this);
            string answer = systemOperations.AddItemConditionalDiscount_MinItems_ToDiscountOnAllError(storeID, itemID, durationInDays, minItems, discount);

            Assert.AreEqual(ExpectedAnswer, answer);
        }

        [Test]
        public virtual void UC_4_2_AddItemConditionalDiscountOnAll_OverlappingAnotherConditionalDiscount_ShouldFail()
        {
            ItemConditionalDiscountOnAll answer = systemOperations.AddItemConditionalDiscount_MinItems_ToDiscountOnAllSuccess(store1.Id, itemInStore1.Id, 30, 5, 0.6);
            string answer2 = systemOperations.AddItemConditionalDiscount_MinItems_ToDiscountOnAllError(store1.Id, itemInStore1.Id, 2, 5, 0.6);

            Assert.AreEqual("Discount not valid - overlapping with exsisting", answer2);
        }

        [Test]
        public virtual void UC_4_2_AddItemConditional_DiscountOnExtraItems_SuccessScenario()
        {
            ItemConditionalDiscount_DiscountOnExtraItems answer = systemOperations.AddItemConditionalDiscount_MinItems_ToDiscountOnExtraItemsSuccess(store1.Id, itemInStore1.Id, 30, 3, 1, 0.5);

            Assert.NotNull(answer);
            Assert.AreEqual(itemInStore1.Id, answer.ItemID);
            Assert.AreEqual(3, answer.NumOfItems);
            Assert.AreEqual(1, answer.ExtraItems);
            Assert.AreEqual(0.5, answer.DiscountForExtra);
        }

        [Test, TestCaseSource("AddItemConditionalDiscount_DiscountOnExtraItems_DataForFailScenarios")]
        public virtual void UC_4_2_AddItemConditional_DiscountOnExtraItems_FailScenarios(Func<StoreOwnerTests, (Guid, Guid)> thisToGetStoreIdAndItemId, int durationInDays, int minItems, int extraItems, double discountForExtraItems, string ExpectedAnswer)
        {
            (Guid storeID, Guid itemID) = thisToGetStoreIdAndItemId(this);
            string answer = systemOperations.AddItemConditionalDiscount_MinItems_ToDiscountOnExtraItemsError(storeID, itemID, durationInDays, minItems, extraItems, discountForExtraItems);

            Assert.AreEqual(ExpectedAnswer, answer);
        }

        [Test]
        public virtual void UC_4_2_AddItemConditional_DiscountOnExtraItems_OverlappingAnotherConditionalDiscount_ShouldFail()
        {
            ItemConditionalDiscount_DiscountOnExtraItems answer = systemOperations.AddItemConditionalDiscount_MinItems_ToDiscountOnExtraItemsSuccess(store1.Id, itemInStore1.Id, 30, 3, 1, 0.5);
            string answer2 = systemOperations.AddItemConditionalDiscount_MinItems_ToDiscountOnExtraItemsError(store1.Id, itemInStore1.Id, 10, 4, 2, 0.5);

            Assert.AreEqual("Discount not valid - overlapping with exsisting", answer2);
        }

        [Test]
        public virtual void UC_4_2_AddStoreConditionalDiscount_SuccessScenario()
        {
            StoreConditionalDiscount answer = systemOperations.AddStoreConditionalDiscountSuccess(store1.Id, 60, 270.50, 0.10);

            Assert.NotNull(answer);
            Assert.AreEqual(0.10, answer.Discount);
            Assert.AreEqual(270.50, answer.MinPurchaseSum);
            Assert.AreEqual(DateTime.Now.AddDays(60).Date, answer.DateUntil.Date);
        }

        [Test, TestCaseSource("AddStoreConditionalDiscount_DataForFailScenarios")]
        public virtual void UC_4_2_AddStoreConditionalDiscount_FailScenarios(Func<StoreOwnerTests, Guid> thisToGetStoreId, int durationInDays,double minPurchase, double discount, string ExpectedAnswer)
        {
            Guid storeID = thisToGetStoreId(this);
            string answer = systemOperations.AddStoreConditionalDiscountError(storeID, durationInDays, minPurchase, discount);

            Assert.AreEqual(ExpectedAnswer, answer);
        }

        [Test]
        public virtual void UC_4_2_StoreConditionalDiscount_OverlappingAnotherConditionalDiscount_FailScenario()
        {
            StoreConditionalDiscount answer = systemOperations.AddStoreConditionalDiscountSuccess(store1.Id, 60, 270.50, 0.10);
            string answer2 = systemOperations.AddStoreConditionalDiscountError(store1.Id, 60, 270.50, 0.10);

            Assert.AreEqual("Discount not valid - overlapping with exsisting", answer2);
        }

        [Test]
        public virtual void UC_4_2_AddCompositeTwoDiscounts_And_SuccessScenario()
        {
            ItemOpenedDiscount o_d = systemOperations.AddOpenedDiscountSuccess(store1.Id, itemInStore1.Id, 0.2, 20);
            StoreConditionalDiscount s_c_d = systemOperations.AddStoreConditionalDiscountSuccess(store1.Id, 4, 150, 0.3);

            CompositeTwoDiscounts c_2_d = checkCompositeDiscountBasicScenarious(o_d, s_c_d, "&");
        }

        [Test]
        public virtual void UC_4_2_AddCompositeTwoDiscounts__Xor_SuccessScenario()
        {
            ItemOpenedDiscount o_d = systemOperations.AddOpenedDiscountSuccess(store1.Id, itemInStore1.Id, 0.2, 20);
            ItemConditionalDiscountOnAll c_d_all = systemOperations.AddItemConditionalDiscount_MinItems_ToDiscountOnAllSuccess(store1.Id, itemInStore1.Id, 20, 6, 0.2);

            CompositeTwoDiscounts c_2_d = checkCompositeDiscountBasicScenarious(o_d, c_d_all, "xor");
        }

        [Test]
        public virtual void UC_4_2_AddCompositeTwoDiscounts_Or_SuccessScenario()
        {
            ItemOpenedDiscount o_d = systemOperations.AddOpenedDiscountSuccess(store1.Id, itemInStore1.Id, 0.2, 20);
            ItemConditionalDiscountOnAll c_d_all = systemOperations.AddItemConditionalDiscount_MinItems_ToDiscountOnAllSuccess(store1.Id, itemInStore1.Id, 20, 6, 0.2);

            CompositeTwoDiscounts c_2_d = checkCompositeDiscountBasicScenarious(o_d, c_d_all, "|");
        }

        [Test]
        public virtual void UC_4_2_AddCompositeTwoDiscounts_ComplicatedComposite1_SuccessScenario()
        {
            ItemOpenedDiscount o_d = systemOperations.AddOpenedDiscountSuccess(store1.Id, itemInStore1.Id, 0.2, 20);
            ItemConditionalDiscountOnAll c_d_all = systemOperations.AddItemConditionalDiscount_MinItems_ToDiscountOnAllSuccess(store1.Id, itemInStore1.Id, 20, 6, 0.2);
            StoreConditionalDiscount s_c_d = systemOperations.AddStoreConditionalDiscountSuccess(store1.Id, 4, 150, 0.3);

            CompositeTwoDiscounts composite1 = checkCompositeDiscountBasicScenarious(o_d, c_d_all, "|");
            CompositeTwoDiscounts composite2 = checkCompositeDiscountBasicScenarious(composite1, s_c_d, "&");

        }

        [Test]
        public virtual void UC_4_2_AddCompositeTwoDiscounts_CompositeDiscountsOfDifferentItemsFromSameStore1_SuccessScenario()
        {

            Item item2 = systemOperations.addItemToStore(store1.Id, new Item("chocolate", new List<string> { "sweet" }, new List<string> { "sweet", "milk items" }), 30);
            ItemOpenedDiscount o_d = systemOperations.AddOpenedDiscountSuccess(store1.Id, itemInStore1.Id, 0.2, 20);
            ItemConditionalDiscountOnAll c_d_all = systemOperations.AddItemConditionalDiscount_MinItems_ToDiscountOnAllSuccess(store1.Id, item2.Id, 20, 6, 0.2);

            CompositeTwoDiscounts c_2_d = checkCompositeDiscountBasicScenarious(o_d, c_d_all, "|");
        }

        [Test]
        public virtual void UC_4_2_AddCompositeTwoDiscounts_CompositeDiscountsOfDifferentItemsFromSameStore2_SuccessScenario()
        {

            Item item2 = systemOperations.addItemToStore(store1.Id, new Item("chocolate", new List<string> { "sweet" }, new List<string> { "sweet", "milk items" }), 30);
            ItemOpenedDiscount o_d_1 = systemOperations.AddOpenedDiscountSuccess(store1.Id, itemInStore1.Id, 0.2, 20);
            ItemOpenedDiscount o_d_2 = systemOperations.AddOpenedDiscountSuccess(store1.Id,item2.Id, 0.2, 20);

            CompositeTwoDiscounts c_2_d = checkCompositeDiscountBasicScenarious(o_d_1, o_d_2, "&");

            StoreConditionalDiscount s_c_d = systemOperations.AddStoreConditionalDiscountSuccess(store1.Id, 4, 150, 0.3);

            CompositeTwoDiscounts c_2_d_2 = checkCompositeDiscountBasicScenarious(c_2_d, s_c_d, "|");
        }

        [Test]
        public virtual void UC_4_2_AddCompositeTwoDiscounts_CompositeDiscountsOfDifferentItemsFromNOTSameStore_FailScenario()
        {

            ItemOpenedDiscount o_d = systemOperations.AddOpenedDiscountSuccess(store1.Id, itemInStore1.Id, 0.2, 20);
            systemOperations.logout();

            systemOperations.login(store2Owner.Username, store2Owner.Pw);
            ItemConditionalDiscountOnAll itemFormStore2_c_d_all = systemOperations.AddItemConditionalDiscount_MinItems_ToDiscountOnAllSuccess(store2.Id, itemInStore2.Id, 20, 6, 0.2);
            systemOperations.logout();

            systemOperations.login(loggedInStore1Owner.Username, loggedInStore1Owner.Pw);
            string error = systemOperations.ComposeTwoDiscountsError(store1.Id, o_d.DiscountID, itemFormStore2_c_d_all.DiscountID, "|");
            Assert.AreEqual("DiscountID doesnt exist", error);
        }

        [Test]
        public virtual void UC_4_2_AddCompositeTwoDiscounts_CompositeDiscountsOfDifferentItemsFromSameStore3_SuccessScenario()
        {

            Item item2 = systemOperations.addItemToStore(store1.Id, new Item("chocolate", new List<string> { "sweet" }, new List<string> { "sweet", "milk items" }), 30);
            ItemOpenedDiscount o_d_1 = systemOperations.AddOpenedDiscountSuccess(store1.Id, itemInStore1.Id, 0.2, 20);
            ItemOpenedDiscount o_d_2 = systemOperations.AddOpenedDiscountSuccess(store1.Id, item2.Id, 0.2, 20);

            CompositeTwoDiscounts c_2_d = checkCompositeDiscountBasicScenarious(o_d_1, o_d_2, "&");
        }

        [Test]
        public virtual void UC_4_2_RemoveOpenedDiscount_SucessScenario()
        {
            ItemOpenedDiscount o_d = systemOperations.AddOpenedDiscountSuccess(store1.Id, itemInStore1.Id, 0.2, 20);
            Assert.AreEqual(true, systemOperations.RemoveDiscountSuccess(store1.Id, o_d.DiscountID));
        }

        [Test]
        public virtual void UC_4_2_RemoveItemConditionalDiscountOnAll_SucessScenario()
        {
            ItemConditionalDiscountOnAll c_d_all = systemOperations.AddItemConditionalDiscount_MinItems_ToDiscountOnAllSuccess(store1.Id, itemInStore1.Id, 20, 6, 0.2);
            Assert.AreEqual(true, systemOperations.RemoveDiscountSuccess(store1.Id, c_d_all.DiscountID));
        }

        [Test]
        public virtual void UC_4_2_RemoveStoreConditionalDiscount_SucessScenario()
        {
            StoreConditionalDiscount s_c_d = systemOperations.AddStoreConditionalDiscountSuccess(store1.Id, 4, 150, 0.3);
            Assert.AreEqual(true, systemOperations.RemoveDiscountSuccess(store1.Id, s_c_d.DiscountID));
        }

        [Test]
        public virtual void UC_4_2_RemoveCompositeTwoDiscounts_SucessScenario()
        {
            List<Discount> discounts = addSomeDiscountsOnItem1ToStore();

            Guid id_left = Guid.Empty;
            Guid id_right = Guid.Empty;

            foreach(Discount d in discounts)
            {
                if( d is CompositeTwoDiscounts c_d)
                {
                    id_left = c_d.DiscountLeftID;
                    id_right = c_d.DiscountRightID;
                    Assert.IsTrue(systemOperations.RemoveDiscountSuccess(store1.Id, c_d.DiscountID));
                    break;
                }
            }

            //verify parts of composite discount deleted too
            Assert.AreEqual("DiscountID doesnt exist" ,systemOperations.RemoveDiscountError(store1.Id, id_left));
            Assert.AreEqual("DiscountID doesnt exist", systemOperations.RemoveDiscountError(store1.Id, id_right));
        }

        [Test]
        public virtual void UC_4_2_RemoveItemConditionalDiscountOnAll_NoSuchStoreId_FailScenario()
        {
            ItemConditionalDiscountOnAll c_d_all = systemOperations.AddItemConditionalDiscount_MinItems_ToDiscountOnAllSuccess(store1.Id, itemInStore1.Id, 20, 6, 0.2);
            Assert.AreEqual("Store doesnt exist", systemOperations.RemoveDiscountError(Guid.NewGuid(), c_d_all.DiscountID));
        }

        [Test]
        public virtual void UC_4_2_RemoveItemConditionalDiscountOnAll_NoSuchDiscountId_FailScenario()
        {
            Assert.AreEqual("DiscountID doesnt exist", systemOperations.RemoveDiscountError(store1.Id, Guid.NewGuid()));
        }

        public static IEnumerable<TestCaseData> apointstoreOwnerOrManagerCases()
        {
            //success
            yield return new TestCaseData(new Func<StoreOwnerTests, (Guid, User)>
                (testsObject => (testsObject.store1.Id, testsObject.registeredUser)), true);
            //not registered user
            yield return new TestCaseData(new Func<StoreOwnerTests, (Guid, User)>
                (testsObject => (testsObject.store1.Id, new User("notRegistered", "password"))), false);
            //not valid  store id
            yield return new TestCaseData(new Func<StoreOwnerTests, (Guid, User)>
                (testsObject => (Guid.Empty, testsObject.registeredUser)), false);
            //notOwnerOfStore
            yield return new TestCaseData(new Func<StoreOwnerTests, (Guid, User)>
                (testsObject => (testsObject.store2.Id, testsObject.registeredUser)), false);
        }

        public static string[] DiscountValidTypes_data =
        {
             "opened", "item_conditional", "store_conditional", "composite"
        };

        [Test, TestCaseSource("DiscountValidTypes_data")]
        public virtual void UC_4_2_MakeDiscountNotAllowed_SuccessScenario(string discountType)
        {
            Assert.AreEqual(true, systemOperations.MakeDiscountNotAllowedSuccess(store1.Id, discountType));
        }

        [Test, TestCaseSource("DiscountValidTypes_data")]
        public virtual void UC_4_2_MakeDiscountAllowed_SuccessScenario(string discountType)
        {
            RemoveAllDiscountTypesFromStorePolicy();
            Assert.AreEqual(0, systemOperations.GetAllowedDiscountsSuccess(store1.Id).Count);

            Assert.AreEqual(true, systemOperations.MakeDiscountAllowedSuccess(store1.Id, discountType));
        }

        [Test]
        public virtual void UC_4_2_GetAllowedDiscounts_InitialStorePolicy_SuccessScenario()
        {
            Assert.AreEqual(DiscountValidTypes_data.Length, systemOperations.GetAllowedDiscountsSuccess(store1.Id).Count);
        }

        [Test]
        public virtual void UC_4_2_GetAllowedDiscounts_NoDiscountsTypesAtAll_SuccessScenario()
        {
            RemoveAllDiscountTypesFromStorePolicy();
            Assert.AreEqual(0 , systemOperations.GetAllowedDiscountsSuccess(store1.Id).Count);
        }

        [Test]
        public virtual void UC_4_2_MakeDiscountAllowed_UnknownDiscountType_FailureScenario()
        {
            RemoveAllDiscountTypesFromStorePolicy();
            Assert.AreEqual("Invalid input", systemOperations.MakeDiscountAllowedError(store1.Id, "ggggg"));
        }

        [Test]
        public virtual void UC_4_2_MakeDiscountAllowed_NoSuchStoreId_FailureScenario()
        {
            RemoveAllDiscountTypesFromStorePolicy();
            Assert.AreEqual("Store doesnt exist", systemOperations.MakeDiscountAllowedError(Guid.NewGuid(), "opened"));
        }

        [Test]
        public virtual void UC_4_2_MakeDiscountAllowed_DiscountTypeAlreadyAllowed_FailureScenario()
        {
            RemoveAllDiscountTypesFromStorePolicy();

            Assert.AreEqual(true, systemOperations.MakeDiscountAllowedSuccess(store1.Id, "opened"));
            Assert.AreEqual("DiscountType Already allowed", systemOperations.MakeDiscountAllowedError(store1.Id, "opened"));
        }

        [Test]
        public virtual void UC_4_2_MakeDiscountNotAllowed_UnknownDiscountType_FailureScenario()
        {
            Assert.AreEqual("Invalid input", systemOperations.MakeDiscountNotAllowedError(store1.Id, "gggg"));
        }

        [Test]
        public virtual void UC_4_2_MakeDiscountNotAllowed_NoSuchStoreId_FailureScenario()
        {
            Assert.AreEqual("Store doesnt exist", systemOperations.MakeDiscountNotAllowedError(Guid.NewGuid(), "opened"));
        }

        [Test]
        public virtual void UC_4_2_MakeDiscountAllowed_DiscountTypeAlreadyNotAllowed_FailureScenario()
        {

            Assert.AreEqual(true, systemOperations.MakeDiscountNotAllowedSuccess(store1.Id, "opened"));
            Assert.AreEqual(DiscountValidTypes_data.Length - 1, systemOperations.GetAllowedDiscountsSuccess(store1.Id).Count);

            Assert.AreEqual("DiscountType Already not allowed", systemOperations.MakeDiscountNotAllowedError(store1.Id, "opened"));
        }

        [Test]
        public virtual void UC_4_2_GetAllowedDiscounts_NoSuchStoreId_SuccessScenario()
        {
            Assert.AreEqual("Store doesnt exist", systemOperations.GetAllowedDiscountsError(Guid.NewGuid()));
        }

        internal void RemoveAllDiscountTypesFromStorePolicy()
        {
            foreach( string discountType in DiscountValidTypes_data)
            {
                Assert.AreEqual(true, systemOperations.MakeDiscountNotAllowedSuccess(store1.Id, discountType));
            }
        }

        internal void RemoveAllPolicyTypesFromStorePolicy()
        {
            foreach (string policy in PolicyValidTypes_data)
            {
                Assert.AreEqual(true, systemOperations.MakePolicyNotAllowedSuccess(store1.Id, policy));
            }
        }

        public static string[] PolicyValidTypes_data =
        {
             "item", "store", "days", "composite"
        };

        [Test, TestCaseSource("PolicyValidTypes_data")]
        public virtual void UC_4_2_Make_policy_not_allowed_success(string policy)
        {
            Assert.AreEqual(true, systemOperations.MakePolicyNotAllowedSuccess(store1.Id, policy));
        }

        [Test]
        public virtual void UC_4_2_Make_policy_not_allowed_not_valid_string_failure()
        {
            Assert.AreEqual("Invalid input", systemOperations.MakePolicyNotAllowedError(store1.Id, "gggg"));
        }

        [Test]
        public virtual void UC_4_2_Make_policy_not_allowed_not_bad_store_id_failure()
        {
            Assert.AreEqual("Store doesnt exist", systemOperations.MakePolicyNotAllowedError(Guid.NewGuid(), "item"));
        }

        [Test]
        public virtual void UC_4_2_Make_policy_not_allowed_not_already_not_allowed_failure()
        {
            Assert.AreEqual(true, systemOperations.MakePolicyNotAllowedSuccess(store1.Id, "item"));
            Assert.AreEqual("PurchasePolicyType Already  not allowed", systemOperations.MakePolicyNotAllowedError(store1.Id, "item"));
        }

        [Test, TestCaseSource("PolicyValidTypes_data")]
        public virtual void UC_4_2_Make_policy_allowed_success(string policy)
        {
            RemoveAllPolicyTypesFromStorePolicy();
            Assert.AreEqual(true, systemOperations.MakePolicyAllowedSuccess(store1.Id, policy));
        }

        [Test]
        public virtual void UC_4_2_Make_policy_allowed_not_valid_string_failure()
        {
            RemoveAllPolicyTypesFromStorePolicy();
            Assert.AreEqual("Invalid input", systemOperations.MakePolicyAllowedError(store1.Id, "gggg"));
        }

        [Test]
        public virtual void UC_4_2_Make_policy_allowed_bad_store_id_failure()
        {
            RemoveAllPolicyTypesFromStorePolicy();
            Assert.AreEqual("Store doesnt exist", systemOperations.MakePolicyAllowedError(Guid.NewGuid(), "item"));
        }

        [Test]
        public virtual void UC_4_2_Make_policy_already_allowed_failure()
        {
            RemoveAllPolicyTypesFromStorePolicy();
            Assert.AreEqual(true, systemOperations.MakePolicyAllowedSuccess(store1.Id, "item"));
            Assert.AreEqual("PurchasePolicyType Already  allowed", systemOperations.MakePolicyAllowedError(store1.Id, "item"));
        }
        [Test]
        public virtual void UC_4_2_GetAllPoliciesAllowed_success()
        {
            List<string> allowed = systemOperations.GetAllowedPurchasePoliciesSuccess(store1.Id);
            List<string> allowedLower = new List<string>();
            foreach(string s in allowed)
            {
                allowedLower.Add(s.ToLower().Trim());
            }
            CollectionAssert.AreEquivalent(PolicyValidTypes_data.ToList(),
                allowedLower);
            RemoveAllPolicyTypesFromStorePolicy();
            CollectionAssert.AreEquivalent(new List<string>() { },
                systemOperations.GetAllowedPurchasePoliciesSuccess(store1.Id));
        }


        public static IEnumerable<TestCaseData> apointstoreOwnerCases()
        {
            //success
            yield return new TestCaseData(new Func<StoreOwnerTests, (Guid, User)>
                (testsObject => (testsObject.store1.Id, testsObject.registeredUser)), true, false);
            //not registered user
            yield return new TestCaseData(new Func<StoreOwnerTests, (Guid, User)>
                (testsObject => (testsObject.store1.Id, new User("notRegistered", "password"))), false, false);
            //not valid  store id
            yield return new TestCaseData(new Func<StoreOwnerTests, (Guid, User)>
                (testsObject => (Guid.Empty, testsObject.registeredUser)), false, false);
            //notOwnerOfStore
            yield return new TestCaseData(new Func<StoreOwnerTests, (Guid, User)>
                (testsObject => (testsObject.store2.Id, testsObject.registeredUser)), false, false);
        }

        [Test, TestCaseSource("apointstoreOwnerCases")]
        public virtual void UC_4_3_AppoiningStoreOwner_successOrFailScenarios(Func<StoreOwnerTests, (Guid, User)> thisToStoreToAssingIDAnduser, bool success, bool calledFromManager = false)
        {

            (Guid storeToAssignToID, User userToApoint) = thisToStoreToAssingIDAnduser(this);
            ValidateNoNotificationForApproveOwner(store1AdditionalOwner);
            Assert.AreEqual(success, systemOperations.assignStoreOwnerNoValidation(storeToAssignToID, userToApoint));
            if (calledFromManager)
            {
                systemOperations.approveStoreOwnerAssginment_by_approver(storeToAssignToID, userToApoint, loggedInStore1Owner);
            }
            systemOperations.logout();
            systemOperations.login(store1AdditionalOwner.Username, store1AdditionalOwner.Pw);
            if (success)
            {
                ValidateHasNotificationForApproveOwner();
                Assert.True(systemOperations.approveStoreOwnerAssginment(storeToAssignToID, userToApoint));
                Assert.True(systemOperations.isOwnerOfStore(storeToAssignToID, userToApoint));
            }
         
        }

        private void ValidateHasNotificationForApproveOwner()
        {
            List<string> messages = systemOperations.getAllNotificationMessages();
            ValidateHasNotificationForApproveOwner(messages);
        }

        private void ValidateHasNotificationForApproveOwner(List<string> messages)
        {
            foreach (string m in messages)
            {
                if (m.Contains("is awaiting your approval on ownership contract in store"))
                {
                    return;
                }
            }
            Assert.Fail("No approve owner notifications");
        }

        private void ValidateNoNotificationForApproveOwner()
        {
            List<string> messages = systemOperations.getAllNotificationMessages();
            ValidateNoNotificationForApproveOwner(messages);
        }

        private void ValidateHasNotificationForApproveOwner(User user)
        {
            List<string> messages = systemOperations.getAllNotificationMessages_without_login(user);
            ValidateHasNotificationForApproveOwner(messages);

        }

        private void ValidateNoNotificationForApproveOwner(User user)
        {
            List<string> messages = systemOperations.getAllNotificationMessages_without_login(user);
            ValidateNoNotificationForApproveOwner(messages);
        }

        private void ValidateNoNotificationForApproveOwner(List<string> messages)
        {
            foreach (string m in messages)
            {
                if (m.Contains("is awaiting your approval on ownership contract in store"))
                {
                    Assert.Fail("Has approve owner notifications");
                }
            }
           
        }

        [Test]
        public virtual void UC_4_3_getAllStoresBeforeApproveal_shouldPass()
        {
            (Guid storeToRemoveFromID, User userToRemove) = (store1.Id, registeredUser);
            Assert.AreEqual(true, systemOperations.assignStoreOwnerNoValidation(storeToRemoveFromID, userToRemove));
            Assert.True(systemOperations.getAllStoresAndItems().Count > 0);
            Assert.AreEqual(true, systemOperations.approveStoreOwnerAssginment_by_approver(storeToRemoveFromID, userToRemove, store1AdditionalOwner));

        }

        [Test,TestCase(false)]
        public virtual void UC_4_4_RemoveStoreOwner_success(bool calledFromManager = false)
        {

            (Guid storeToRemoveFromID, User userToRemove) = (store1.Id, registeredUser);
            Assert.AreEqual(true, systemOperations.assignStoreOwnerNoValidation(storeToRemoveFromID, userToRemove));
            Assert.AreEqual(true, systemOperations.approveStoreOwnerAssginment_by_approver(storeToRemoveFromID, userToRemove, store1AdditionalOwner));
            if (calledFromManager)
            {
                Assert.AreEqual(true, systemOperations.approveStoreOwnerAssginment_by_approver(storeToRemoveFromID, userToRemove, loggedInStore1Owner));
            }
            Assert.False(systemOperations.userHasMessages(userToRemove));
            Assert.AreEqual(true, systemOperations.removeStoreOwnerByOwner(storeToRemoveFromID, userToRemove));
            Assert.True(systemOperations.userHasMessages(userToRemove));
            systemOperations.logout();
            validateSingleRemoveOwnerNotification(userToRemove, store1);
        }

        [Test]
        public virtual void UC_4_4_RemoveStoreOwner_notGrantor_failure()
        {

            (Guid storeToRemoveFromID, User userToRemove) = (store1.Id, registeredUser);
            Assert.AreEqual(true, systemOperations.assignStoreOwnerNoValidation(storeToRemoveFromID, userToRemove));
            Assert.AreEqual(true, systemOperations.approveStoreOwnerAssginment_by_approver(storeToRemoveFromID, userToRemove, store1AdditionalOwner));
            systemOperations.logout();
            systemOperations.login(registeredUser.Username, registeredUser.Pw);
            Assert.AreEqual(false, systemOperations.removeStoreOwnerByOwner(storeToRemoveFromID, userToRemove));
            systemOperations.logout();
        }
        [Test]
        public virtual void UC_4_4_RemoveStoreOwner_removerNotOwner_failure()
        {

            (Guid storeToRemoveFromID, User userToRemove) = (store1.Id, registeredUser);
            Assert.AreEqual(true, systemOperations.assignStoreOwnerNoValidation(storeToRemoveFromID, userToRemove));
            Assert.AreEqual(true, systemOperations.approveStoreOwnerAssginment_by_approver(storeToRemoveFromID, userToRemove, store1AdditionalOwner));
            systemOperations.logout();
            systemOperations.login(registeredUser2.Username, registeredUser2.Pw);
            Assert.AreEqual(false, systemOperations.removeStoreOwnerByOwner(storeToRemoveFromID, userToRemove));
            systemOperations.logout();
        }
        public static IEnumerable<TestCaseData> removeOwnerInitialOwner()
        {
            yield return new TestCaseData(new Func<StoreOwnerTests, User>
                (testsObject => testsObject.loggedInStore1Owner),true, false);     
        }

        [Test, TestCaseSource( "removeOwnerInitialOwner")]
        public virtual void UC_4_4_UC_4_3_RemoveStoreOwner_and_assign_complex_withSubOwnersAndManagers_success(Func<StoreOwnerTests, User> thisToInitialUser, bool success, bool calledFromManager = false)
        {
            User initialOwner = thisToInitialUser(this);
            (Guid storeToRemoveFromID, User userToRemove) = (store1.Id, registeredUser);
            Assert.AreEqual(true, systemOperations.assignStoreOwnerNoValidation(storeToRemoveFromID, userToRemove));
            Assert.AreEqual(true, systemOperations.approveStoreOwnerAssginment_by_approver(storeToRemoveFromID, userToRemove, store1AdditionalOwner));
            if (calledFromManager)
                Assert.AreEqual(true, systemOperations.approveStoreOwnerAssginment_by_approver(storeToRemoveFromID, userToRemove, loggedInStore1Owner));
            Assert.True(systemOperations.isOwnerOfStore(storeToRemoveFromID, userToRemove));
            systemOperations.logout();
            //add first layer
            systemOperations.login(userToRemove.Username, userToRemove.Pw);
            User u1 = systemOperations.getNewUserForTests();
            User u2 = systemOperations.getNewUserForTests();
            User u3 = systemOperations.getNewUserForTests();
            Assert.AreEqual(true, systemOperations.assignStoreOwnerNoValidation(storeToRemoveFromID, u1));
            Assert.AreEqual(true, systemOperations.assignStoreOwnerNoValidation(storeToRemoveFromID, u2));
            Assert.AreEqual(true, systemOperations.approveStoreOwnerAssginment_by_approver(storeToRemoveFromID, u1, store1AdditionalOwner));
            Assert.AreEqual(true, systemOperations.approveStoreOwnerAssginment_by_approver(storeToRemoveFromID, u2, store1AdditionalOwner));
            Assert.AreEqual(true, systemOperations.approveStoreOwnerAssginment_by_approver(storeToRemoveFromID, u1, loggedInStore1Owner));
            Assert.AreEqual(true, systemOperations.approveStoreOwnerAssginment_by_approver(storeToRemoveFromID, u2, loggedInStore1Owner));

            //validates both u1 u2 are owners, meaning no circular aproval is required
            Assert.AreEqual(true, systemOperations.isOwnerOfStore(store1, u1));
            Assert.AreEqual(true, systemOperations.isOwnerOfStore(store1, u2));

            Assert.AreEqual(true, systemOperations.assignStoreManagerByOwner(storeToRemoveFromID, u3));
            systemOperations.logout();
            //add second layer
            systemOperations.login(u1.Username, u1.Pw);
            User u4 = systemOperations.getNewUserForTests();
            User u5 = systemOperations.getNewUserForTests();
            //validate u4 needs all owners approval
            Assert.AreEqual(true, systemOperations.assignStoreOwnerNoValidation(storeToRemoveFromID, u4));
            Assert.False(systemOperations.isOwnerOfStore(store1, u4));
            Assert.AreEqual(true, systemOperations.approveStoreOwnerAssginment_by_approver(storeToRemoveFromID, u4, store1AdditionalOwner));
            Assert.AreEqual(true, systemOperations.approveStoreOwnerAssginment_by_approver(storeToRemoveFromID, u4, loggedInStore1Owner));
            Assert.False(systemOperations.isOwnerOfStore(store1, u4));
            Assert.AreEqual(true, systemOperations.approveStoreOwnerAssginment_by_approver(storeToRemoveFromID, u4, userToRemove));
            Assert.False(systemOperations.isOwnerOfStore(store1, u4));
            Assert.AreEqual(true, systemOperations.approveStoreOwnerAssginment_by_approver(storeToRemoveFromID, u4, u2));
            Assert.True(systemOperations.isOwnerOfStore(store1, u4));


            Assert.AreEqual(true, systemOperations.assignStoreManagerByOwner(storeToRemoveFromID, u5));
            //validates remove owner removes all subs.
            systemOperations.logout();
            foreach (User u in new User[] { u1, u2, userToRemove, u4 })
            {
                validateNoRemoveOwnerNotifications(u, store1);
            }
            systemOperations.login(initialOwner.Username, initialOwner.Pw);
            
            Assert.AreEqual(success, systemOperations.removeStoreOwnerByOwner(storeToRemoveFromID, userToRemove));
            if (success)
            {
                Assert.AreEqual(false, systemOperations.isManagerOfStore(store1, u5));
                Assert.AreEqual(false, systemOperations.isManagerOfStore(store1, u3));
                Assert.AreEqual(false, systemOperations.isOwnerOfStore(store1, u1));
                Assert.AreEqual(false, systemOperations.isOwnerOfStore(store1, u2));
                Assert.AreEqual(false, systemOperations.isOwnerOfStore(store1, u4));
            }
            systemOperations.logout();
            foreach(User u in new User[]{ u1, u2, userToRemove, u4 })
            {
                validateSingleRemoveOwnerNotification(u, store1);
            }

        }

        private void validateNoRemoveOwnerNotifications(User u, Store store)
        {
            systemOperations.login(u.Username, u.Pw);
            List<string> notificationMessages = systemOperations.getAllNotificationMessages();
            int numOfRemoveOwnerNotifications = 0;
            notificationMessages.ForEach((s) => {
                if (s.Contains(String.Format("You have been removed from owning store: {0}", store.Name)))
                    numOfRemoveOwnerNotifications++;
                });
            Assert.AreEqual(0, numOfRemoveOwnerNotifications);
            systemOperations.logout();
        }

        private void validateSingleRemoveOwnerNotification(User u, Store store)
        {
            systemOperations.login(u.Username,u.Pw);
            List<string> notificationMessages = systemOperations.getAllNotificationMessages();
            Assert.AreEqual(1, notificationMessages.Count);
            Assert.True(notificationMessages[0].Contains(String.Format("You have been removed from owning store: {0}", store.Name)));
            systemOperations.logout();
        }

        [Test, TestCaseSource("apointstoreOwnerOrManagerCases")]
        public virtual void UC_4_5_AppointingStoreManager_successOrFailScenarios(Func<StoreOwnerTests, (Guid, User)> thisToStoreToAssingIDAnduser, bool success)
        {
            (Guid storeToAssignToID, User userToApoint) = thisToStoreToAssingIDAnduser(this);
            Assert.AreEqual(success, systemOperations.assignStoreManagerByOwner(storeToAssignToID, userToApoint));
        }

        public static List<Tuple<string, bool>> permissions = extendToFullPermissionList();

        public static IEnumerable<TestCaseData> editManagerPermissionsCases()
        {
            //success
            yield return new TestCaseData(new Func<StoreOwnerTests, (Guid, User)>
                (testsObject => (testsObject.store1.Id, testsObject.manager1)), permissions, true, null);
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

        [Test, TestCaseSource("editManagerPermissionsCases")]
        public virtual void UC_4_6_editStoreManagerPermissions_successOrFailScenarios(Func<StoreOwnerTests, (Guid, User)>thisToStoreAndManager,
            List<Tuple<string, bool>> newPermissions, bool success,StoreOwnerTests thisObjForUseBySubclasses = null )
        {
            //newPermissions.ViewPurchaseHistory = null;
            StoreOwnerTests actualThis = thisObjForUseBySubclasses == null ? this : thisObjForUseBySubclasses;
            (Guid storeToEditManagerID, User managerToUpdate) = thisToStoreAndManager(actualThis);
            Assert.AreEqual(success, systemOperations.editManagerPermissions(storeToEditManagerID, managerToUpdate, newPermissions));
        }

        [Test]
        public virtual void UC_4_6_GetStoresWithPermissions_SuccessScenario()
        {
            List<Tuple<Store, List<string>>> result = systemOperations.GetStoresWithPermissionsSuccess();

            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(10, result[0].Item2.Count);
        }

        public static IEnumerable<TestCaseData> removeStoreManagerRightsCases()
        {
            //success
            yield return new TestCaseData(new Func<StoreOwnerTests, (Store, User, User)>
                (testsObject => (testsObject.store1, testsObject.manager1, testsObject.loggedInStore1Owner)), true);
            //not manager
            yield return new TestCaseData(new Func<StoreOwnerTests, (Store, User, User)>
                (testsObject => (testsObject.store1, testsObject.registeredUser, testsObject.loggedInStore1Owner)), false);
            //not by owner
            yield return new TestCaseData(new Func<StoreOwnerTests, (Store, User, User)>
                (testsObject => (testsObject.store1, testsObject.registeredUser, testsObject.loggedInStore1Owner)), false);
            //yes owner but not grantor
            yield return new TestCaseData(new Func<StoreOwnerTests, (Store, User, User)>
                (testsObject => (testsObject.store1, testsObject.manager1, testsObject.store1AdditionalOwner)), false);
            //notOwnerOfStore
            yield return new TestCaseData(new Func<StoreOwnerTests, (Store, User, User)>
                (testsObject => (testsObject.store2, testsObject.manager2, testsObject.loggedInStore1Owner)), false);

        }

        [Test, TestCaseSource("removeStoreManagerRightsCases")]
        public virtual void UC_4_7_removeStoreManagerRights_successAndFailScenario(Func<StoreOwnerTests, (Store, User, User)> thisToStoreManagerRemover, bool success)
        {
            (Store storeToRemoveFrom, User managerToRemovePermissions, User removingUser) = thisToStoreManagerRemover(this);
            //validates only removed from specified store.
            systemOperations.logout();
            systemOperations.login(store2Owner.Username, store2Owner.Pw);
            systemOperations.assignStoreManagerByOwner(store2.Id, managerToRemovePermissions);
            systemOperations.logout();
            systemOperations.login(removingUser.Username, removingUser.Pw);
            Assert.AreEqual(success, systemOperations.removeStoreManagerRights(store1, managerToRemovePermissions));
            systemOperations.logout();
            //log in with known owner to validate if status changed or not
            //systemOperations.login(loggedInStore1Owner.Username, loggedInStore1Owner.Pw);
            Assert.IsTrue(systemOperations.isManagerOfStore(store1, loggedInStore1Owner));
            systemOperations.logout();
            systemOperations.login(store2Owner.Username, store2Owner.Pw);
            Assert.True(systemOperations.isManagerOfStore(store2, managerToRemovePermissions));
        }


        [Test]
        public void UC_4_7_removeStoreManagerRights_unrelatedUsers_failureScenario()
        {
            User unrelated1 = systemOperations.getNewUserForTests();
            Assert.False(systemOperations.removeStoreManagerRights(store1, unrelated1));
            Assert.True(systemOperations.isManagerOfStore(store1, manager1));
        }

        [TestCase(1)]
        [TestCase(2)]
        [TestCase(3)]
        public virtual void UC_4_10_viewStorePurchaseHistory_successScenario(int numberOfPurchases)
        {
            List<PurchaseRecord> purchasesMade = makeRandomPurchases(numberOfPurchases, store1, loggedInStore1Owner, true);
            CollectionAssert.AreEquivalent(purchasesMade, systemOperations.getStoreHistory(store1));

        }

        [TestCase(1)]
        [TestCase(2)]
        [TestCase(3)]
        public virtual void UC_4_10_viewStorePurchaseHistory_withPriceChanges_successScenario(int numberOfPurchases)
        {
            (List<PurchaseRecord> purchasesMade, Store store1) = viewStorePurchaseHistory_withPriceChanges_helper(numberOfPurchases, true);
            CollectionAssert.AreEquivalent(purchasesMade, systemOperations.getStoreHistory(store1));
        }

        //helper function so it can be called by admin tests.
        internal (List<PurchaseRecord>, Store) viewStorePurchaseHistory_withPriceChanges_helper(int numberOfPurchases, bool ownerLoggedIn)
        {
            List<PurchaseRecord> purchasesMade = makeRandomPurchases(numberOfPurchases, store1, loggedInStore1Owner, ownerLoggedIn);

            foreach (PurchaseRecord purchase in purchasesMade)
            {
                systemOperations.changeItemPrice(store1, purchase.ItemObj, purchase.DiscountedPriceOnPurchase + 1);
            }


            return (purchasesMade, store1);
        }

        [TestCase(1)]
        [TestCase(2)]
        [TestCase(3)]
        public virtual void UC_4_10_viewStorePurchaseHistory_withItemDeletions_successScenario(int numberOfPurchases)
        {

            (List<PurchaseRecord> purchasesMade, Store store1) = viewStorePurchaseHistory_withItemDeletions_helper(numberOfPurchases, true);
            CollectionAssert.AreEquivalent(purchasesMade, systemOperations.getStoreHistory(store1));

        }

        //helper function so it can be called by admin tests.
        internal (List<PurchaseRecord>, Store) viewStorePurchaseHistory_withItemDeletions_helper(int numberOfPurchases, bool ownerLogedIn)
        {
            List<PurchaseRecord> purchasesMade = makeRandomPurchases(numberOfPurchases, store1, loggedInStore1Owner, ownerLogedIn);

            foreach (PurchaseRecord purchase in purchasesMade)
            {
                systemOperations.deleteItemFromStoreWithValidation(store1.Id, purchase.ItemId);
            }
            return (purchasesMade, store1);
        }

        internal List<PurchaseRecord> makeRandomPurchases(int numberOfPurchases, Store store, User storeOwner, bool loggedInAsOwner)
        {
            List<PurchaseRecord> purchasesMade = new List<PurchaseRecord>();
            for (int i = 0; i < numberOfPurchases; i++)
            {
                purchasesMade.Add(systemOperations.addRandomPurchase(store, storeOwner, loggedInAsOwner));
            }

            return purchasesMade;
        }

        [TestCase(1)]
        [TestCase(2)]
        [TestCase(3)]
        public virtual void UC_4_10_veiwStorePurchaseHistory_notByOwner_failureScenario(int numberOfPurchase)
        {
            (List<PurchaseRecord> purchasesMade, Store store1) = viewStorePurchaseHistory_withPriceChanges_helper(numberOfPurchase, true);
            systemOperations.logout();
            systemOperations.login(registeredUser.Username, registeredUser.Pw);
            Assert.Null(systemOperations.getStoreHistory(store1));
        }

        [Test]
        public void UC_4_10_viewStorePurchaseHistory_noPurchases_successScenario()
        {
            Assert.AreEqual(0, systemOperations.getStoreHistory(store1).Count);
        }

        internal static List<Tuple<string, bool>> extendToFullPermissionList()
        {
            List<Tuple<string, bool>> lst = new List<Tuple<string, bool>> { new Tuple<string, bool>("inventory", true),
            new Tuple<string, bool>("appoint_owner", true),
            new Tuple<string, bool>("appoint_manager", true),
            new Tuple<string, bool>("edit_permissions", true),
            new Tuple<string, bool>("remove_manager", true),
            new Tuple<string, bool>("policy", true),
            new Tuple<string, bool>("remove_owner", true) };

            return lst;
        }

        internal static List<Tuple<string, bool>> removeAllPermissionsList()
        {
            List<Tuple<string, bool>> lst = new List<Tuple<string, bool>> { new Tuple<string, bool>("inventory", false),
            new Tuple<string, bool>("appoint_owner", false),
            new Tuple<string, bool>("appoint_manager", false),
            new Tuple<string, bool>("edit_permissions", false),
            new Tuple<string, bool>("remove_manager", false),
            new Tuple<string, bool>("policy", false),
            new Tuple<string, bool>("history", false),
            new Tuple<string, bool>("requests", false),
            new Tuple<string, bool>("remove_owner", false)};

            return lst;
        }
        
        internal List<Discount> addSomeDiscountsOnItem1ToStore()
        {
            ItemOpenedDiscount o_d = systemOperations.AddOpenedDiscountSuccess(store1.Id, itemInStore1.Id, 0.2, 20);
            ItemConditionalDiscountOnAll c_d_all = systemOperations.AddItemConditionalDiscount_MinItems_ToDiscountOnAllSuccess(store1.Id, itemInStore1.Id, 20, 6, 0.2);
            StoreConditionalDiscount s_c_d = systemOperations.AddStoreConditionalDiscountSuccess(store1.Id, 4, 150, 0.3);
            CompositeTwoDiscounts c_2_d = systemOperations.ComposeTwoDiscountsSuccess(store1.Id, o_d.DiscountID, s_c_d.DiscountID, "&");

            List<Discount> discounts = new List<Discount>() { o_d, c_d_all, s_c_d, c_2_d };
            return discounts;
        }

        internal CompositeTwoDiscounts checkCompositeDiscountBasicScenarious(Discount left, Discount right, string op)
        {
            CompositeTwoDiscounts c_2_d = systemOperations.ComposeTwoDiscountsSuccess(store1.Id, left.DiscountID, right.DiscountID, op);
            DateTime minDate = new DateTime(Math.Min(left.DateUntil.Ticks, right.DateUntil.Ticks));

            Assert.AreEqual(left.DiscountID, c_2_d.DiscountLeftID);
            Assert.AreEqual(right.DiscountID, c_2_d.DiscountRightID);
            Assert.AreEqual(minDate.Date, c_2_d.DateUntil.Date);
            Assert.AreEqual(Utilitys.Utils.stringToPerator(op), c_2_d.Operator);

            return c_2_d;
        }

        public static IEnumerable<TestCaseData> AddItemConditionalDiscountOnAllDataForFailScenarios()
        {
            //duration In days negative
            yield return new TestCaseData(new Func<StoreOwnerTests, (Guid, Guid)>
                (testsObject => (testsObject.store1.Id, testsObject.itemInStore1.Id)), -5, 5, 0.6, "Invalid input");
            //percentage more then 1
            yield return new TestCaseData(new Func<StoreOwnerTests, (Guid, Guid)>
                (testsObject => (testsObject.store1.Id, testsObject.itemInStore1.Id)), 30, 5, 50, "Invalid input");
            //item amount negative
            yield return new TestCaseData(new Func<StoreOwnerTests, (Guid, Guid)>
                (testsObject => (testsObject.store1.Id, testsObject.itemInStore1.Id)), 30, -9, 0.6, "Invalid input");
            //not valid  store id
            yield return new TestCaseData(new Func<StoreOwnerTests, (Guid, Guid)>
                (testsObject => (Guid.NewGuid(), testsObject.itemInStore1.Id)), 30, 5, 0.6, "Store doesnt exist");
            //not valid item id
            yield return new TestCaseData(new Func<StoreOwnerTests, (Guid, Guid)>
                (testsObject => (testsObject.store1.Id, Guid.NewGuid())), 30, 5, 0.6, "item doesnt exist");
        }

        public static IEnumerable<TestCaseData> AddItemConditionalDiscount_DiscountOnExtraItems_DataForFailScenarios()
        {
            //duration In days negative
            yield return new TestCaseData(new Func<StoreOwnerTests, (Guid, Guid)>
                (testsObject => (testsObject.store1.Id, testsObject.itemInStore1.Id)), -22, 3, 1, 0.5, "Invalid input");
            //percentage more then 1
            yield return new TestCaseData(new Func<StoreOwnerTests, (Guid, Guid)>
                (testsObject => (testsObject.store1.Id, testsObject.itemInStore1.Id)), 30, 3, 1, 1.1, "Invalid input");
            //minimum item amount negative
            yield return new TestCaseData(new Func<StoreOwnerTests, (Guid, Guid)>
                (testsObject => (testsObject.store1.Id, testsObject.itemInStore1.Id)), 30, -33, 1, 0.5, "Invalid input");
            //extra item amount negative
            yield return new TestCaseData(new Func<StoreOwnerTests, (Guid, Guid)>
                (testsObject => (testsObject.store1.Id, testsObject.itemInStore1.Id)), 30, 3, -11, 0.5, "Invalid input");
            //not valid  store id
            yield return new TestCaseData(new Func<StoreOwnerTests, (Guid, Guid)>
                (testsObject => (Guid.NewGuid(), testsObject.itemInStore1.Id)), 30, 3, 1, 0.5, "Store doesnt exist");
            //not valid item id
            yield return new TestCaseData(new Func<StoreOwnerTests, (Guid, Guid)>
                (testsObject => (testsObject.store1.Id, Guid.NewGuid())), 30, 3, 1, 0.5, "item doesnt exist");
        }

        public static IEnumerable<TestCaseData> AddStoreConditionalDiscount_DataForFailScenarios()
        {
            //duration In days negative
            yield return new TestCaseData(new Func<StoreOwnerTests, Guid>
                (testsObject => (testsObject.store1.Id)), -22, 300.66, 0.5, "Invalid input");
            //percentage more then 1
            yield return new TestCaseData(new Func<StoreOwnerTests, Guid>
                (testsObject => (testsObject.store1.Id)), 30, 300.88, 1.1, "Invalid input");
            //percentage less then 0
            yield return new TestCaseData(new Func<StoreOwnerTests, Guid>
                (testsObject => (testsObject.store1.Id)), 30, 300, -1.1, "Invalid input");
            //minimum purchase sum negative
            yield return new TestCaseData(new Func<StoreOwnerTests, Guid>
                (testsObject => (testsObject.store1.Id)), 30, -33, 0.5, "Invalid input");
            //not valid  store id
            yield return new TestCaseData(new Func<StoreOwnerTests, Guid>
                (testsObject => Guid.NewGuid()), 30, 443, 0.5, "Store doesnt exist");
        }

        [Test]
        public void UC_10_notifyOwnersOnPurchase_success()
        {
            systemOperations.logout();
            Assert.False(HasPurchaseMessage(loggedInStore1Owner, store1));
            Assert.False(HasPurchaseMessage(store1AdditionalOwner, store1));
            Assert.True(systemOperations.buyItemsFromDifferentUserAndThenChangeBackToCurrentUser(new Item[] { itemInStore1 },
                new int[] { 1 }, new string[] { "4444-3333-2222-1111", "03/30", "333", "yossi", "212212212" },
                new string[] { "Borer 20", "Tel Aviv", "Israel", "20693" }));
            Assert.True(HasPurchaseMessage(loggedInStore1Owner, store1));
            Assert.True(HasPurchaseMessage(store1AdditionalOwner, store1));
            Assert.False(HasPurchaseMessage(store2Owner, store1));
        }

        private bool HasPurchaseMessage(User owner, Store store)
        {
            systemOperations.login(owner.Username, owner.Pw);
            List<string> messages = systemOperations.getAllNotificationMessages();
            foreach(string m in messages)
            {
                if (m.Contains(String.Format("A purchase has been made in store {0} owned by you", store.Name))){
                    systemOperations.logout();
                    return true;
                }
            }
            return false;
        }
    }
}

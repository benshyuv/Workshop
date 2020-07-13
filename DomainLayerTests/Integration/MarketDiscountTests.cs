using System;
using System.Collections.Generic;
using DomainLayer.Orders;
using DomainLayer.Stores.Discounts;
using DomainLayer.Stores.Inventory;
using Newtonsoft.Json;
using NUnit.Framework;
namespace DomainLayerTests.Integration
{
    internal class MarketDiscountTests : IMarketFacadeTests
    {
        private const int LONG_DURATION = 15;
        private const int SHORT_DURATION = 10;
        private const double HALF_OFF = 0.5;
        private const int MIN_AMOUNT = 10;
        private const int EXTRA_AMOUNT = 5;
        private const int NO_DISCOUNT = 1;
        private const int MIN_PURCHASE = 100;
        private const string XOR = "xor";
        private readonly List<string> allowed_DiscountType_strings =
           new List<string> { "opened", "item_conditional", "store_conditional", "composite" };
        [SetUp]
        public void SetUp()
        {
            SetupUsers();
            SetupStores();
            CreateItemsAndAddToCart(REGISTERED_SESSION_ID, true);
        }
       

        [Test]
        public void IsDiscountTypeAllowed_Success()
        {
            foreach(string discountTypeString in allowed_DiscountType_strings)
            {
                Assert.True(marketFacade.IsDiscountTypeAllowed(FIRST_STORE_ID, discountTypeString));
            }
        }

        [Test]
        public void MakeDiscountNotAllowed_Success()
        {
            LoginSessionSuccess(REGISTERED_SESSION_ID, FIRST_OPENER_USERNAME, PASSWORD);
            foreach (string discountTypeString in allowed_DiscountType_strings)
            {
                Assert.True(DeserializeMakeDiscountNotAllowedSuccess(REGISTERED_SESSION_ID, FIRST_STORE_ID, discountTypeString)); 
            }   
        }

        [Test]
        public void MakeDiscountAllowed_Success()
        {
            LoginSessionSuccess(REGISTERED_SESSION_ID, FIRST_OPENER_USERNAME, PASSWORD);
            foreach (string discountTypeString in allowed_DiscountType_strings)
            {
                Assert.True(DeserializeMakeDiscountNotAllowedSuccess(REGISTERED_SESSION_ID, FIRST_STORE_ID, discountTypeString));
            }
            foreach (string discountTypeString in allowed_DiscountType_strings)
            {
                Assert.True(DeserializeMakeDiscountAllowedSuccess(REGISTERED_SESSION_ID, FIRST_STORE_ID, discountTypeString));
            }
         
        }

        [Test]
        public void ChangeDiscountTypesAndValidate_Success()
        {
            LoginSessionSuccess(REGISTERED_SESSION_ID, FIRST_OPENER_USERNAME, PASSWORD);
            foreach (string discountTypeString in allowed_DiscountType_strings)
            {
                Assert.True(DeserializeMakeDiscountNotAllowedSuccess(REGISTERED_SESSION_ID, FIRST_STORE_ID, discountTypeString));
                Assert.False(marketFacade.IsDiscountTypeAllowed(FIRST_STORE_ID, discountTypeString));
            }
            foreach (string discountTypeString in allowed_DiscountType_strings)
            {
                Assert.True(DeserializeMakeDiscountAllowedSuccess(REGISTERED_SESSION_ID, FIRST_STORE_ID, discountTypeString));
                Assert.True(marketFacade.IsDiscountTypeAllowed(FIRST_STORE_ID, discountTypeString));
            } 
          
        }

        [Test]
        public void GetDiscountTypes_Success()
        {
            LoginSessionSuccess(REGISTERED_SESSION_ID, FIRST_OPENER_USERNAME, PASSWORD);
            List<string> allowedDiscountStrings = DeserializeGetAllowedDiscountsSuccess(REGISTERED_SESSION_ID, FIRST_STORE_ID);

            CollectionAssert.AreEquivalent(allowed_DiscountType_strings, allowedDiscountStrings.ConvertAll((s)=> s.ToLower().Trim()));

            DeserializeMakeDiscountNotAllowedSuccess(REGISTERED_SESSION_ID, FIRST_STORE_ID, "opened");
            allowedDiscountStrings = DeserializeGetAllowedDiscountsSuccess(REGISTERED_SESSION_ID, FIRST_STORE_ID);
            allowedDiscountStrings.Add("opened");
            CollectionAssert.AreEquivalent(allowed_DiscountType_strings, allowedDiscountStrings.ConvertAll((s) => s.ToLower().Trim()));
        }

        [Test]
        public void AddOpenDiscount_typeNotAllowed_failure()
        {
            LoginSessionSuccess(REGISTERED_SESSION_ID, FIRST_OPENER_USERNAME, PASSWORD);
            DeserializeMakeDiscountNotAllowedSuccess(REGISTERED_SESSION_ID, FIRST_STORE_ID, "opened");

            Assert.False(DeserializeOpenDiscount(REGISTERED_SESSION_ID, FIRST_STORE_ID, FIRST_ITEM_FIRST_STORE_ID,
                HALF_OFF, LONG_DURATION).Success);
           
        }

        [Test]
        public void AddItemConditional_toAll_typeNotAllowed_failure()
        {
            LoginSessionSuccess(REGISTERED_SESSION_ID, FIRST_OPENER_USERNAME, PASSWORD);
            DeserializeMakeDiscountNotAllowedSuccess(REGISTERED_SESSION_ID, FIRST_STORE_ID, "item_conditional");

            Assert.False(DeserializeItemConditionalDiscount_discountOnAll(
                REGISTERED_SESSION_ID, FIRST_STORE_ID, FIRST_ITEM_FIRST_STORE_ID, LONG_DURATION, MIN_AMOUNT, HALF_OFF).Success);
        }

        [Test]
        public void AddItemConditional_toextra_typeNotAllowed_failure()
        {
            LoginSessionSuccess(REGISTERED_SESSION_ID, FIRST_OPENER_USERNAME, PASSWORD);
            DeserializeMakeDiscountNotAllowedSuccess(REGISTERED_SESSION_ID, FIRST_STORE_ID, "item_conditional");

            Assert.False(DeserializeItemConditionalDiscount_discountOnExtra(
                REGISTERED_SESSION_ID, FIRST_STORE_ID, FIRST_ITEM_FIRST_STORE_ID, LONG_DURATION, MIN_AMOUNT, EXTRA_AMOUNT, HALF_OFF).Success);

        }

        [Test]
        public void AddStoreConditional_typeNotAllowed_failure()
        {
            LoginSessionSuccess(REGISTERED_SESSION_ID, FIRST_OPENER_USERNAME, PASSWORD);
            DeserializeMakeDiscountNotAllowedSuccess(REGISTERED_SESSION_ID, FIRST_STORE_ID, "store_conditional");

            Assert.False(DeserializeStoreConditionalDiscount(REGISTERED_SESSION_ID,FIRST_STORE_ID, LONG_DURATION, LONG_DURATION, HALF_OFF).Success);

        }

        [Test]
        public void AddcompositeDiscount_typeNotAllowed_failure()
        {
            LoginSessionSuccess(REGISTERED_SESSION_ID, FIRST_OPENER_USERNAME, PASSWORD);
            DeserializeMakeDiscountNotAllowedSuccess(REGISTERED_SESSION_ID, FIRST_STORE_ID, "composite");

            OpenDiscount openDiscount1 = DeserializeOpenDiscountSuccess(REGISTERED_SESSION_ID, FIRST_STORE_ID, FIRST_ITEM_FIRST_STORE_ID,
                HALF_OFF, LONG_DURATION);
            OpenDiscount openDiscount2 = DeserializeOpenDiscountSuccess(REGISTERED_SESSION_ID, FIRST_STORE_ID, SECOND_ITEM_FIRST_STORE_ID,
                HALF_OFF, 10);

            Assert.False(DeserializeCompositeDiscount(REGISTERED_SESSION_ID, FIRST_STORE_ID, openDiscount1.discountID,
                openDiscount2.discountID, "&").Success);

        }

        [Test]
        public void AddOpenDiscountSuccess_FirstStore_FirstItem_HalfOff()
        {
            AddOpenDiscount(REGISTERED_SESSION_ID, FIRST_OPENER_USERNAME, FIRST_STORE_ID, FIRST_ITEM_FIRST_STORE_ID);
        }

        private void AddOpenDiscount(Guid sessionID, string ownerName, Guid storeID, Guid itemID, int duration = LONG_DURATION,
                                                        double discount = HALF_OFF)
        {
            LoginSessionSuccess(sessionID, ownerName);
            OpenDiscount openDiscount = DeserializeOpenDiscountSuccess(sessionID, storeID, itemID, discount, duration);
            Assert.AreEqual(DateTime.Now.Date.AddDays(duration), openDiscount.DateUntil.Date);
            Assert.AreEqual(discount, openDiscount.Precent);
        }

        [Test]
        public void IsValidToCreateOpenDiscount_Success()
        {
            LoginSessionSuccess(REGISTERED_SESSION_ID, FIRST_OPENER_USERNAME, PASSWORD);
            Assert.True(marketFacade.IsValidToCreateItemOpenedDiscount(FIRST_STORE_ID, FIRST_ITEM_FIRST_STORE_ID));
            DeserializeOpenDiscountSuccess(REGISTERED_SESSION_ID, FIRST_STORE_ID, FIRST_ITEM_FIRST_STORE_ID,
                HALF_OFF, LONG_DURATION);

            Assert.True(marketFacade.IsValidToCreateItemOpenedDiscount(FIRST_STORE_ID, SECOND_ITEM_FIRST_STORE_ID));
        }

        [Test]
        public void IsValidToCreateOpenDiscount_Failure()
        {
            LoginSessionSuccess(REGISTERED_SESSION_ID, FIRST_OPENER_USERNAME, PASSWORD);
            DeserializeOpenDiscountSuccess(REGISTERED_SESSION_ID, FIRST_STORE_ID, FIRST_ITEM_FIRST_STORE_ID,
                HALF_OFF, LONG_DURATION);

            Assert.False(marketFacade.IsValidToCreateItemOpenedDiscount(FIRST_STORE_ID,FIRST_ITEM_FIRST_STORE_ID));
        }

        [Test]
        public void IsValidToCreateItemConditionalDiscount_Success()
        {
            LoginSessionSuccess(REGISTERED_SESSION_ID, FIRST_OPENER_USERNAME, PASSWORD);
            Assert.True(marketFacade.IsValidToCreateItemConditionalDiscount(FIRST_STORE_ID, FIRST_ITEM_FIRST_STORE_ID));
            DeserializeItemConditionalDiscount_discountOnExtraSuccess(
                REGISTERED_SESSION_ID, FIRST_STORE_ID, FIRST_ITEM_FIRST_STORE_ID, LONG_DURATION, 10, 5, HALF_OFF);
            DeserializeItemConditionalDiscount_discountOnAllSuccess(
                REGISTERED_SESSION_ID, FIRST_STORE_ID, THIRD_ITEM_FIRST_STORE_ID, HALF_OFF, LONG_DURATION, 10);

            Assert.True(marketFacade.IsValidToCreateItemConditionalDiscount(FIRST_STORE_ID, SECOND_ITEM_FIRST_STORE_ID));
        }

        [Test]
        public void IsValidToCreateItemConditionalDiscount_Failure()
        {
            LoginSessionSuccess(REGISTERED_SESSION_ID, FIRST_OPENER_USERNAME, PASSWORD);
            DeserializeItemConditionalDiscount_discountOnExtraSuccess(
                REGISTERED_SESSION_ID, FIRST_STORE_ID, FIRST_ITEM_FIRST_STORE_ID, LONG_DURATION, 10, 5, HALF_OFF);
            DeserializeItemConditionalDiscount_discountOnAllSuccess(
                REGISTERED_SESSION_ID, FIRST_STORE_ID, SECOND_ITEM_FIRST_STORE_ID, HALF_OFF, LONG_DURATION, 10);

            Assert.False(marketFacade.IsValidToCreateItemConditionalDiscount(FIRST_STORE_ID, FIRST_ITEM_FIRST_STORE_ID));

            Assert.False(marketFacade.IsValidToCreateItemConditionalDiscount(FIRST_STORE_ID, SECOND_ITEM_FIRST_STORE_ID));
        }

        [Test]
        public void IsValidToCreateStoreConditionalDiscount_Success()
        {
            LoginSessionSuccess(REGISTERED_SESSION_ID, FIRST_OPENER_USERNAME, PASSWORD);
            Assert.True(marketFacade.IsValidToCreateStoreConditionalDiscount(FIRST_STORE_ID));
        }

        [Test]
        public void IsValidToCreateStoreConditionalDiscount_Failure()
        {
            LoginSessionSuccess(REGISTERED_SESSION_ID, FIRST_OPENER_USERNAME, PASSWORD);
            DeserializeStoreConditionalDiscountSuccess(REGISTERED_SESSION_ID, FIRST_STORE_ID, LONG_DURATION, MIN_PURCHASE, HALF_OFF);

            Assert.False(marketFacade.IsValidToCreateStoreConditionalDiscount(FIRST_STORE_ID));
        }

        [Test]
        public void StoreConditionalDiscount_Success()
        {
            AddStoreConditionalDiscount(REGISTERED_SESSION_ID, FIRST_OPENER_USERNAME, FIRST_STORE_ID);
        }

        private void AddStoreConditionalDiscount(Guid sessionID, string ownerName, Guid storeID, int duration = LONG_DURATION,
                                                    double minPurchase = MIN_PURCHASE, double discount = HALF_OFF)
        {
            LoginSessionSuccess(sessionID, ownerName);
            StoreConditionalDiscount storeConditional = DeserializeStoreConditionalDiscountSuccess(sessionID, storeID, duration,
                minPurchase, discount);
            Assert.AreEqual(DateTime.Now.Date.AddDays(duration), storeConditional.DateUntil.Date);
            Assert.AreEqual(discount, storeConditional.Precent);
            Assert.AreEqual(minPurchase, storeConditional.MinPurchase);
        }

        [Test]
        public void ItemConditionalDiscount_discountOnExtra_Success()
        {
            AddItemConditionalDiscountOnExtra(REGISTERED_SESSION_ID, FIRST_OPENER_USERNAME, FIRST_STORE_ID, FIRST_ITEM_FIRST_STORE_ID);
        }

        private void AddItemConditionalDiscountOnExtra(Guid sessionID, string ownerName, Guid storeID, Guid itemID, int duration = LONG_DURATION,
                                                        int minAmount = MIN_AMOUNT, int extraAmount = EXTRA_AMOUNT, double discount = HALF_OFF)
        {
            LoginSessionSuccess(sessionID, ownerName);
            ItemConditionalDiscount_MinItems_ToDiscountOnExtraItems itemConditional = DeserializeItemConditionalDiscount_discountOnExtraSuccess(
                sessionID, storeID, itemID, duration, minAmount, extraAmount, discount);
            Assert.AreEqual(DateTime.Now.Date.AddDays(duration), itemConditional.DateUntil.Date);
            Assert.AreEqual(itemID, itemConditional.ItemID);
            Assert.AreEqual(minAmount, itemConditional.MinItems);
            Assert.AreEqual(extraAmount, itemConditional.ExtraItems);
            Assert.AreEqual(discount, itemConditional.DiscountForExtra);
        }

        [Test]
        public void ItemConditionalDiscount_discountOnAll_Success()
        {
            AddItemConditionalDiscountOnAll(REGISTERED_SESSION_ID, FIRST_OPENER_USERNAME, FIRST_STORE_ID, FIRST_ITEM_FIRST_STORE_ID);
        }

        private void AddItemConditionalDiscountOnAll(Guid sessionID, string ownerName, Guid storeID, Guid itemID, int duration = LONG_DURATION, 
                                                        int minAmount = MIN_AMOUNT, double discount = HALF_OFF)
        {
            LoginSessionSuccess(sessionID, ownerName);
            ItemConditionalDiscount_MinItems_ToDiscountOnAll itemConditional = DeserializeItemConditionalDiscount_discountOnAllSuccess(
                sessionID, storeID, itemID, discount, duration, minAmount);
            Assert.AreEqual(DateTime.Now.Date.AddDays(duration), itemConditional.DateUntil.Date);
            Assert.AreEqual(itemID, itemConditional.ItemID);
            Assert.AreEqual(minAmount, itemConditional.MinItems);
            Assert.AreEqual(discount, itemConditional.Precent);
        }

        private void AddComposite_Open_Open(Guid sessionID, string ownerName, Guid storeID, Guid itemID1, Guid itemID2, Operator op,
                                            double discount1 = HALF_OFF, int duration1 = LONG_DURATION, 
                                            double discount2 = HALF_OFF, int duration2 = LONG_DURATION)
        {
            LoginSessionSuccess(sessionID, ownerName);
            OpenDiscount openDiscount1 = DeserializeOpenDiscountSuccess(sessionID, storeID, itemID1,
                discount1, duration1);
            OpenDiscount openDiscount2 = DeserializeOpenDiscountSuccess(sessionID, storeID, itemID2,
                discount2, duration2);
            CompositeDiscount compositeDiscount = DeserializeCompositeDiscountSuccess(sessionID, storeID, openDiscount1.discountID,
                openDiscount2.discountID, op.ToString());

            int shortest = Math.Min(duration1, duration2);
            Assert.AreEqual(DateTime.Now.Date.AddDays(shortest), compositeDiscount.DateUntil.Date);
            Assert.AreEqual(openDiscount1.discountID, compositeDiscount.DiscountLeftID);
            Assert.AreEqual(openDiscount2.discountID, compositeDiscount.DiscountRightID);
            Assert.AreEqual(op, compositeDiscount.Op);
        }

        private void AddComposite_CondAll_CondAll (Guid sessionID, string ownerName, Guid storeID, Guid itemID1, Guid itemID2, Operator op,
                                                    double discount1 = HALF_OFF, int duration1 = LONG_DURATION, int minAmount1 = MIN_AMOUNT,
                                                    double discount2 = HALF_OFF, int duration2 = LONG_DURATION, int minAmount2 = MIN_AMOUNT)
        {
            LoginSessionSuccess(sessionID, ownerName);
            ItemConditionalDiscount_MinItems_ToDiscountOnAll itemCondAll1 = 
                DeserializeItemConditionalDiscount_discountOnAllSuccess(sessionID, storeID, itemID1,
                    discount1, duration1, minAmount1);
            ItemConditionalDiscount_MinItems_ToDiscountOnAll itemCondAll2 = 
                DeserializeItemConditionalDiscount_discountOnAllSuccess(sessionID, storeID, itemID2,
                    discount2, duration2, minAmount2);
            CompositeDiscount compositeDiscount = DeserializeCompositeDiscountSuccess(sessionID, storeID, itemCondAll1.discountID,
                itemCondAll2.discountID, op.ToString());

            int shortest = Math.Min(duration1, duration2);
            Assert.AreEqual(DateTime.Now.Date.AddDays(shortest), compositeDiscount.DateUntil.Date);
            Assert.AreEqual(itemCondAll1.discountID, compositeDiscount.DiscountLeftID);
            Assert.AreEqual(itemCondAll2.discountID, compositeDiscount.DiscountRightID);
            Assert.AreEqual(op, compositeDiscount.Op);
        }

        [Test]
        public void CompositeDiscount_xor_Success()
        {
            AddComposite_Open_Open(REGISTERED_SESSION_ID, FIRST_OPENER_USERNAME, FIRST_STORE_ID, FIRST_ITEM_FIRST_STORE_ID,
                                            SECOND_ITEM_FIRST_STORE_ID, Operator.XOR, HALF_OFF, LONG_DURATION, HALF_OFF, SHORT_DURATION);
        }

        [Test]
        public void CompositeDiscount_and_Success()
        {
            AddComposite_Open_Open(REGISTERED_SESSION_ID, FIRST_OPENER_USERNAME, FIRST_STORE_ID, FIRST_ITEM_FIRST_STORE_ID,
                                            SECOND_ITEM_FIRST_STORE_ID, Operator.AND, HALF_OFF, LONG_DURATION, HALF_OFF, SHORT_DURATION);
        }

        [Test]
        public void CompositeDiscount_or_Success()
        {
            AddComposite_Open_Open(REGISTERED_SESSION_ID, FIRST_OPENER_USERNAME, FIRST_STORE_ID, FIRST_ITEM_FIRST_STORE_ID,
                                            SECOND_ITEM_FIRST_STORE_ID, Operator.OR, HALF_OFF, LONG_DURATION, HALF_OFF, SHORT_DURATION);
        }

        [Test]
        public void DiscountExistInStore_success()
        {
            LoginSessionSuccess(REGISTERED_SESSION_ID, FIRST_OPENER_USERNAME, PASSWORD);
            StoreConditionalDiscount storeConditional = DeserializeStoreConditionalDiscountSuccess(REGISTERED_SESSION_ID, FIRST_STORE_ID, LONG_DURATION,
                MIN_PURCHASE, HALF_OFF);
            Assert.True(marketFacade.DiscountExistsInStore(FIRST_STORE_ID,storeConditional.discountID));
        }

        [Test]
        public void DiscountExistInStore_nodiscount_failure()
        {
            LoginSessionSuccess(REGISTERED_SESSION_ID, FIRST_OPENER_USERNAME, PASSWORD);
            StoreConditionalDiscount storeConditional = DeserializeStoreConditionalDiscountSuccess(REGISTERED_SESSION_ID, FIRST_STORE_ID, LONG_DURATION,
                MIN_PURCHASE, HALF_OFF);
            Assert.False(marketFacade.DiscountExistsInStore(FIRST_STORE_ID, Guid.NewGuid()));
        }

        [Test]
        public void RemoveBasicDiscount_Success()
        {
            LoginSessionSuccess(REGISTERED_SESSION_ID, FIRST_OPENER_USERNAME, PASSWORD);
            StoreConditionalDiscount storeConditional = DeserializeStoreConditionalDiscountSuccess(REGISTERED_SESSION_ID, FIRST_STORE_ID, LONG_DURATION,
                MIN_PURCHASE, HALF_OFF);
            Assert.True(DeserializeRemoveDiscountSuccess(REGISTERED_SESSION_ID, FIRST_STORE_ID, storeConditional.discountID));
            Assert.False(marketFacade.DiscountExistsInStore(FIRST_STORE_ID, storeConditional.discountID));
        }

        [Test]
        public void RemoveBasicDiscount_noSuchDiscount_Failure()
        {
            LoginSessionSuccess(REGISTERED_SESSION_ID, FIRST_OPENER_USERNAME, PASSWORD);
            StoreConditionalDiscount storeConditional = DeserializeStoreConditionalDiscountSuccess(REGISTERED_SESSION_ID, FIRST_STORE_ID, LONG_DURATION,
                MIN_PURCHASE, HALF_OFF);
            Assert.True(DeserializeRemoveDiscountSuccess(REGISTERED_SESSION_ID, FIRST_STORE_ID, storeConditional.discountID));
            Assert.False(marketFacade.DiscountExistsInStore(FIRST_STORE_ID, storeConditional.discountID));
            Assert.False(DeserializeRemoveDiscount(REGISTERED_SESSION_ID, FIRST_STORE_ID, storeConditional.discountID).Success);
        }

        [Test]
        public void RemoveCompositeDiscount_Success()
        {
            LoginSessionSuccess(REGISTERED_SESSION_ID, FIRST_OPENER_USERNAME, PASSWORD);
            OpenDiscount openDiscount1 = DeserializeOpenDiscountSuccess(REGISTERED_SESSION_ID, FIRST_STORE_ID, FIRST_ITEM_FIRST_STORE_ID,
                HALF_OFF, LONG_DURATION);
            OpenDiscount openDiscount2 = DeserializeOpenDiscountSuccess(REGISTERED_SESSION_ID, FIRST_STORE_ID, SECOND_ITEM_FIRST_STORE_ID,
                HALF_OFF, SHORT_DURATION);
            CompositeDiscount compositeDiscount = DeserializeCompositeDiscountSuccess(REGISTERED_SESSION_ID, FIRST_STORE_ID, openDiscount1.discountID,
                openDiscount2.discountID, "|");

            Assert.True(DeserializeRemoveDiscountSuccess(REGISTERED_SESSION_ID, FIRST_STORE_ID, compositeDiscount.discountID));
            Assert.False(marketFacade.DiscountExistsInStore(FIRST_STORE_ID, openDiscount1.discountID));
            Assert.False(marketFacade.DiscountExistsInStore(FIRST_STORE_ID, openDiscount2.discountID));
            Assert.False(marketFacade.DiscountExistsInStore(FIRST_STORE_ID, compositeDiscount.discountID));

        }

        [Test]
        public void RemoveCompositeDiscount_composedOfComposites_Success()
        {
            LoginSessionSuccess(REGISTERED_SESSION_ID, FIRST_OPENER_USERNAME, PASSWORD);
            OpenDiscount openDiscount1 = DeserializeOpenDiscountSuccess(REGISTERED_SESSION_ID, FIRST_STORE_ID, FIRST_ITEM_FIRST_STORE_ID,
                HALF_OFF, LONG_DURATION);
            OpenDiscount openDiscount2 = DeserializeOpenDiscountSuccess(REGISTERED_SESSION_ID, FIRST_STORE_ID, SECOND_ITEM_FIRST_STORE_ID,
                HALF_OFF, SHORT_DURATION);
            CompositeDiscount compositeDiscount = DeserializeCompositeDiscountSuccess(REGISTERED_SESSION_ID, FIRST_STORE_ID, openDiscount1.discountID,
                openDiscount2.discountID, "|");

            OpenDiscount openDiscount3 = DeserializeOpenDiscountSuccess(REGISTERED_SESSION_ID, FIRST_STORE_ID, FIRST_ITEM_FIRST_STORE_ID,
               HALF_OFF, LONG_DURATION);
            OpenDiscount openDiscount4 = DeserializeOpenDiscountSuccess(REGISTERED_SESSION_ID, FIRST_STORE_ID, SECOND_ITEM_FIRST_STORE_ID,
                HALF_OFF, SHORT_DURATION);
            CompositeDiscount compositeDiscount2 = DeserializeCompositeDiscountSuccess(REGISTERED_SESSION_ID, FIRST_STORE_ID, openDiscount3.discountID,
                openDiscount4.discountID, "|");

            CompositeDiscount compositeDiscount3 = DeserializeCompositeDiscountSuccess(REGISTERED_SESSION_ID, FIRST_STORE_ID, compositeDiscount.discountID,
                compositeDiscount2.discountID, "|");

            Assert.True(DeserializeRemoveDiscountSuccess(REGISTERED_SESSION_ID, FIRST_STORE_ID, compositeDiscount3.discountID));
            Assert.False(marketFacade.DiscountExistsInStore(FIRST_STORE_ID, openDiscount1.discountID));
            Assert.False(marketFacade.DiscountExistsInStore(FIRST_STORE_ID, openDiscount2.discountID));
            Assert.False(marketFacade.DiscountExistsInStore(FIRST_STORE_ID, compositeDiscount.discountID));
            Assert.False(marketFacade.DiscountExistsInStore(FIRST_STORE_ID, openDiscount3.discountID));
            Assert.False(marketFacade.DiscountExistsInStore(FIRST_STORE_ID, openDiscount4.discountID));
            Assert.False(marketFacade.DiscountExistsInStore(FIRST_STORE_ID, compositeDiscount2.discountID));
            Assert.False(marketFacade.DiscountExistsInStore(FIRST_STORE_ID, compositeDiscount3.discountID));
        }

        [Test]
        public void GetAllDiscounts_BigCompostite_Success()
        {
            LoginSessionSuccess(REGISTERED_SESSION_ID, FIRST_OPENER_USERNAME, PASSWORD);
            OpenDiscount openDiscount1 = DeserializeOpenDiscountSuccess(REGISTERED_SESSION_ID, FIRST_STORE_ID, FIRST_ITEM_FIRST_STORE_ID,
                HALF_OFF, LONG_DURATION);
            OpenDiscount openDiscount2 = DeserializeOpenDiscountSuccess(REGISTERED_SESSION_ID, FIRST_STORE_ID, SECOND_ITEM_FIRST_STORE_ID,
                HALF_OFF, SHORT_DURATION);
            CompositeDiscount compositeDiscount = DeserializeCompositeDiscountSuccess(REGISTERED_SESSION_ID, FIRST_STORE_ID, openDiscount1.discountID,
                openDiscount2.discountID, "|");

            OpenDiscount openDiscount3 = DeserializeOpenDiscountSuccess(REGISTERED_SESSION_ID, FIRST_STORE_ID, FIRST_ITEM_FIRST_STORE_ID,
               HALF_OFF, LONG_DURATION);
            OpenDiscount openDiscount4 = DeserializeOpenDiscountSuccess(REGISTERED_SESSION_ID, FIRST_STORE_ID, SECOND_ITEM_FIRST_STORE_ID,
                HALF_OFF, SHORT_DURATION);
            CompositeDiscount compositeDiscount2 = DeserializeCompositeDiscountSuccess(REGISTERED_SESSION_ID, FIRST_STORE_ID, openDiscount3.discountID,
                openDiscount4.discountID, "|");

            CompositeDiscount compositeDiscount3 = DeserializeCompositeDiscountSuccess(REGISTERED_SESSION_ID, FIRST_STORE_ID, compositeDiscount.discountID,
                compositeDiscount2.discountID, "|");
            string res = marketFacade.GetAllDicsounts(REGISTERED_SESSION_ID, FIRST_STORE_ID, null);
            Assert.True(MarketOperationSuccess(res));

        }

        [Test]
        public void GetAllDiscounts_BigCompostite_forItem_Success()
        {
            LoginSessionSuccess(REGISTERED_SESSION_ID, FIRST_OPENER_USERNAME, PASSWORD);
            OpenDiscount openDiscount1 = DeserializeOpenDiscountSuccess(REGISTERED_SESSION_ID, FIRST_STORE_ID, FIRST_ITEM_FIRST_STORE_ID,
                HALF_OFF, LONG_DURATION);
            OpenDiscount openDiscount2 = DeserializeOpenDiscountSuccess(REGISTERED_SESSION_ID, FIRST_STORE_ID, SECOND_ITEM_FIRST_STORE_ID,
                HALF_OFF, SHORT_DURATION);
            CompositeDiscount compositeDiscount = DeserializeCompositeDiscountSuccess(REGISTERED_SESSION_ID, FIRST_STORE_ID, openDiscount1.discountID,
                openDiscount2.discountID, "|");

            OpenDiscount openDiscount3 = DeserializeOpenDiscountSuccess(REGISTERED_SESSION_ID, FIRST_STORE_ID, FIRST_ITEM_FIRST_STORE_ID,
               HALF_OFF, LONG_DURATION);
            OpenDiscount openDiscount4 = DeserializeOpenDiscountSuccess(REGISTERED_SESSION_ID, FIRST_STORE_ID, SECOND_ITEM_FIRST_STORE_ID,
                HALF_OFF, SHORT_DURATION);
            CompositeDiscount compositeDiscount2 = DeserializeCompositeDiscountSuccess(REGISTERED_SESSION_ID, FIRST_STORE_ID, openDiscount3.discountID,
                openDiscount4.discountID, "|");

            CompositeDiscount compositeDiscount3 = DeserializeCompositeDiscountSuccess(REGISTERED_SESSION_ID, FIRST_STORE_ID, compositeDiscount.discountID,
                compositeDiscount2.discountID, "|");
            string res = marketFacade.GetAllDicsounts(REGISTERED_SESSION_ID, FIRST_STORE_ID, FIRST_ITEM_FIRST_STORE_ID);
            Assert.True(MarketOperationSuccess(res));

        }

        [Test]
        public void GetAllDiscounts_2_discounts_forItem_Success()
        {
            LoginSessionSuccess(REGISTERED_SESSION_ID, FIRST_OPENER_USERNAME, PASSWORD);
            OpenDiscount openDiscount1 = DeserializeOpenDiscountSuccess(REGISTERED_SESSION_ID, FIRST_STORE_ID, FIRST_ITEM_FIRST_STORE_ID,
                HALF_OFF, LONG_DURATION);
            OpenDiscount openDiscount2 = DeserializeOpenDiscountSuccess(REGISTERED_SESSION_ID, FIRST_STORE_ID, SECOND_ITEM_FIRST_STORE_ID,
                HALF_OFF, SHORT_DURATION);
            string res = marketFacade.GetAllDicsounts(REGISTERED_SESSION_ID, FIRST_STORE_ID, FIRST_ITEM_FIRST_STORE_ID);
            Assert.True(MarketOperationSuccess(res));

        }

        [Test]
        public void GetAllDiscounts_2_discounts_Success()
        {
            LoginSessionSuccess(REGISTERED_SESSION_ID, FIRST_OPENER_USERNAME, PASSWORD);
            OpenDiscount openDiscount1 = DeserializeOpenDiscountSuccess(REGISTERED_SESSION_ID, FIRST_STORE_ID, FIRST_ITEM_FIRST_STORE_ID,
                HALF_OFF, LONG_DURATION);
            OpenDiscount openDiscount2 = DeserializeOpenDiscountSuccess(REGISTERED_SESSION_ID, FIRST_STORE_ID, SECOND_ITEM_FIRST_STORE_ID,
                HALF_OFF, SHORT_DURATION);
            string res = marketFacade.GetAllDicsounts(REGISTERED_SESSION_ID, FIRST_STORE_ID, null);
            Assert.True(MarketOperationSuccess(res));

        }



        [Test]
        public void RemoveBasicDiscount_thatIsPartOfComposite_Failure()
        {
            LoginSessionSuccess(REGISTERED_SESSION_ID, FIRST_OPENER_USERNAME, PASSWORD);
            OpenDiscount openDiscount1 = DeserializeOpenDiscountSuccess(REGISTERED_SESSION_ID, FIRST_STORE_ID, FIRST_ITEM_FIRST_STORE_ID,
                HALF_OFF, LONG_DURATION);
            OpenDiscount openDiscount2 = DeserializeOpenDiscountSuccess(REGISTERED_SESSION_ID, FIRST_STORE_ID, SECOND_ITEM_FIRST_STORE_ID,
                HALF_OFF, SHORT_DURATION);
            CompositeDiscount compositeDiscount = DeserializeCompositeDiscountSuccess(REGISTERED_SESSION_ID, FIRST_STORE_ID, openDiscount1.discountID,
                openDiscount2.discountID, "|");

            Assert.False(DeserializeRemoveDiscount(REGISTERED_SESSION_ID, FIRST_STORE_ID, openDiscount1.discountID).Success);
            Assert.True(marketFacade.DiscountExistsInStore(FIRST_STORE_ID, openDiscount1.discountID));
            Assert.True(marketFacade.DiscountExistsInStore(FIRST_STORE_ID, openDiscount2.discountID));
            Assert.True(marketFacade.DiscountExistsInStore(FIRST_STORE_ID, compositeDiscount.discountID));

        }

        private Order VerifiedCheckoutOrder()
        {
            Order order = null;
            Assert.DoesNotThrow(() => order = CheckoutSuccess(REGISTERED_SESSION_ID));
            Assert.IsNotNull(order);
            return order;
        }

        private StoreOrder VerifiedStoreOrder(Order order, Guid storeID)
        {
            StoreOrder storeOrder = order.StoreOrdersDict[storeID];
            Assert.IsNotNull(storeOrder);
            return storeOrder;
        }

        private OrderItem VerifiedOrderItem(Order order, Guid storeID, Guid itemID)
        {
            StoreOrder storeOrder = VerifiedStoreOrder(order, storeID);
            OrderItem orderItem = storeOrder.OrderItemsDict[itemID];
            Assert.IsNotNull(orderItem);
            return orderItem;
        }

        private void VerifyItemDiscount(Order order, Guid storeID, Guid itemID, double discount)
        {
            OrderItem orderItem = VerifiedOrderItem(order, storeID, itemID);
            Assert.AreEqual(orderItem.DiscountedPricePerItem, orderItem.BasePricePerItem * discount);
        }

        [Test]
        public void PurchaseWithDiscount_Open_Valid_ShouldPass()
        {
            AddOpenDiscountSuccess_FirstStore_FirstItem_HalfOff();
            LogoutUserLoginBuyer(REGISTERED_SESSION_ID);

            Order order = VerifiedCheckoutOrder();
            VerifyItemDiscount(order, FIRST_STORE_ID, FIRST_ITEM_FIRST_STORE_ID, HALF_OFF);
        }

        [Test]
        public void PurchaseWithDiscount_Open_DifferentItem_NoDiscount()
        {
            AddOpenDiscount(REGISTERED_SESSION_ID, FIRST_OPENER_USERNAME, FIRST_STORE_ID, FIRST_ITEM_FIRST_STORE_ID);
            LogoutUserLoginBuyer(REGISTERED_SESSION_ID);

            Order order = VerifiedCheckoutOrder();
            VerifyItemDiscount(order, FIRST_STORE_ID, SECOND_ITEM_FIRST_STORE_ID, NO_DISCOUNT);
        }

        private void VerifyExtraItemsDiscount(Order order, Guid storeID, Guid itemID, int minAmount, int extraAmount, int purchasedAmount, 
                                                double discount = HALF_OFF)
        {
            OrderItem orderItem = VerifiedOrderItem(order, storeID, itemID);
            double computedPrice = 0;
            if (purchasedAmount <= minAmount)
            {
                Assert.IsTrue(orderItem.DiscountedPricePerItem == orderItem.BasePricePerItem);
            }
            else if (purchasedAmount <= minAmount + extraAmount)
            {
                computedPrice += (minAmount * orderItem.BasePricePerItem);
                computedPrice += (purchasedAmount - minAmount) * orderItem.BasePricePerItem * discount;
                computedPrice /= purchasedAmount;
                Assert.AreEqual(computedPrice, orderItem.DiscountedPricePerItem);
            }
            else
            {
                computedPrice += (purchasedAmount - extraAmount) * orderItem.BasePricePerItem;
                computedPrice += extraAmount * orderItem.BasePricePerItem * discount;
                computedPrice /= purchasedAmount;
                Assert.AreEqual(computedPrice, orderItem.DiscountedPricePerItem);
            }
        }

        [Test]
        public void PurchaseWithDiscount_CondItemAll_Valid_ShouldPass()
        {
            AddItemConditionalDiscountOnAll(REGISTERED_SESSION_ID, FIRST_OPENER_USERNAME, FIRST_STORE_ID, FIRST_ITEM_FIRST_STORE_ID,
                                            minAmount: FIRST_ITEM_FIRST_STORE_PURCHASE_AMOUNT);
            LogoutUserLoginBuyer(REGISTERED_SESSION_ID);
            Order order = VerifiedCheckoutOrder();
            VerifyItemDiscount(order, FIRST_STORE_ID, FIRST_ITEM_FIRST_STORE_ID, HALF_OFF);
        }

        [Test]
        public void PurchaseWithDiscount_CondItemAll_NotValid_NoDiscount()
        {
            AddItemConditionalDiscountOnAll(REGISTERED_SESSION_ID, FIRST_OPENER_USERNAME, FIRST_STORE_ID, FIRST_ITEM_FIRST_STORE_ID,
                                            minAmount: FIRST_ITEM_FIRST_STORE_PURCHASE_AMOUNT+1);
            LogoutUserLoginBuyer(REGISTERED_SESSION_ID);
            Order order = VerifiedCheckoutOrder();
            VerifyItemDiscount(order, FIRST_STORE_ID, FIRST_ITEM_FIRST_STORE_ID, NO_DISCOUNT);
        }

        [Test]
        public void PurchaseWithDiscount_CondItemExtra_Valid_SomeExtra_ShouldPass()
        {
            AddItemConditionalDiscountOnExtra(REGISTERED_SESSION_ID, FIRST_OPENER_USERNAME, FIRST_STORE_ID, FIRST_ITEM_FIRST_STORE_ID,
                                                minAmount: FIRST_ITEM_FIRST_STORE_PURCHASE_AMOUNT/2,
                                                extraAmount: FIRST_ITEM_FIRST_STORE_PURCHASE_AMOUNT);
            LogoutUserLoginBuyer(REGISTERED_SESSION_ID);
            Order order = VerifiedCheckoutOrder();
            VerifyExtraItemsDiscount(order, FIRST_STORE_ID, FIRST_ITEM_FIRST_STORE_ID, 
                                        minAmount: FIRST_ITEM_FIRST_STORE_PURCHASE_AMOUNT / 2,
                                        extraAmount: FIRST_ITEM_FIRST_STORE_PURCHASE_AMOUNT, 
                                        purchasedAmount: FIRST_ITEM_FIRST_STORE_PURCHASE_AMOUNT);
        }

        [Test]
        public void PurchaseWithDiscount_CondItemExtra_Valid_OverExtra_ShouldPass()
        {
            AddItemConditionalDiscountOnExtra(REGISTERED_SESSION_ID, FIRST_OPENER_USERNAME, FIRST_STORE_ID, FIRST_ITEM_FIRST_STORE_ID,
                                                minAmount: FIRST_ITEM_FIRST_STORE_PURCHASE_AMOUNT / 2,
                                                extraAmount: (FIRST_ITEM_FIRST_STORE_PURCHASE_AMOUNT / 2) - 1);
            LogoutUserLoginBuyer(REGISTERED_SESSION_ID);
            Order order = VerifiedCheckoutOrder();
            VerifyExtraItemsDiscount(order, FIRST_STORE_ID, FIRST_ITEM_FIRST_STORE_ID,
                                        minAmount: FIRST_ITEM_FIRST_STORE_PURCHASE_AMOUNT / 2,
                                        extraAmount: (FIRST_ITEM_FIRST_STORE_PURCHASE_AMOUNT / 2) - 1, 
                                        purchasedAmount: FIRST_ITEM_FIRST_STORE_PURCHASE_AMOUNT);
        }

        [Test]
        public void PurchaseWithDiscount_CondItemExtra_NotValid_NoDiscount()
        {
            AddItemConditionalDiscountOnExtra(REGISTERED_SESSION_ID, FIRST_OPENER_USERNAME, FIRST_STORE_ID, FIRST_ITEM_FIRST_STORE_ID,
                                                minAmount: FIRST_ITEM_FIRST_STORE_PURCHASE_AMOUNT + 1);
            LogoutUserLoginBuyer(REGISTERED_SESSION_ID);
            Order order = VerifiedCheckoutOrder();
            VerifyItemDiscount(order, FIRST_STORE_ID, FIRST_ITEM_FIRST_STORE_ID, NO_DISCOUNT);
        }

        private void VerifyStoreDiscount(Order order, Guid storeID, double discount)
        {
            StoreOrder storeOrder = VerifiedStoreOrder(order, storeID);
            foreach(OrderItem orderItem in storeOrder.StoreOrderItems)
            {
                Assert.AreEqual(orderItem.DiscountedPricePerItem, orderItem.BasePricePerItem * discount);
            }
        }

        [Test]
        public void PurchaseWithDiscount_CondStore_Valid_ShouldPass()
        {
            AddStoreConditionalDiscount(REGISTERED_SESSION_ID, FIRST_OPENER_USERNAME, FIRST_STORE_ID,minPurchase: 0);
            LogoutUserLoginBuyer(REGISTERED_SESSION_ID);
            Order order = VerifiedCheckoutOrder();
            VerifyStoreDiscount(order, FIRST_STORE_ID, HALF_OFF);
        }

        [Test]
        public void PurchaseWithDiscount_CondStore_NotValid_NoDiscount()
        {
            AddStoreConditionalDiscount(REGISTERED_SESSION_ID, FIRST_OPENER_USERNAME, FIRST_STORE_ID, minPurchase: int.MaxValue);
            LogoutUserLoginBuyer(REGISTERED_SESSION_ID);
            Order order = VerifiedCheckoutOrder();
            VerifyStoreDiscount(order, FIRST_STORE_ID, NO_DISCOUNT);
        }

        [Test]
        public void PurchaseWithDiscount_Comp_2OpenItems_And_2Valid_ShouldPass()
        {
            AddComposite_Open_Open(REGISTERED_SESSION_ID, FIRST_OPENER_USERNAME, FIRST_STORE_ID, FIRST_ITEM_FIRST_STORE_ID,
                                            SECOND_ITEM_FIRST_STORE_ID, Operator.AND);
            LogoutUserLoginBuyer(REGISTERED_SESSION_ID);
            Order order = VerifiedCheckoutOrder();
            VerifyItemDiscount(order, FIRST_STORE_ID, FIRST_ITEM_FIRST_STORE_ID, HALF_OFF);
            VerifyItemDiscount(order, FIRST_STORE_ID, SECOND_ITEM_FIRST_STORE_ID, HALF_OFF);
        }

        [Test]
        public void PurchaseWithDiscount_Comp_2OpenItems_Or_2Valid_ShouldPass()
        {
            AddComposite_Open_Open(REGISTERED_SESSION_ID, FIRST_OPENER_USERNAME, FIRST_STORE_ID, FIRST_ITEM_FIRST_STORE_ID,
                                            SECOND_ITEM_FIRST_STORE_ID, Operator.AND);
            LogoutUserLoginBuyer(REGISTERED_SESSION_ID);
            Order order = VerifiedCheckoutOrder();
            VerifyItemDiscount(order, FIRST_STORE_ID, FIRST_ITEM_FIRST_STORE_ID, HALF_OFF);
            VerifyItemDiscount(order, FIRST_STORE_ID, SECOND_ITEM_FIRST_STORE_ID, HALF_OFF);
        }

        [Test]
        public void PurchaseWithDiscount_Comp_2OpenItems_XOR_2Valid_NoDiscount()
        {
            AddComposite_Open_Open(REGISTERED_SESSION_ID, FIRST_OPENER_USERNAME, FIRST_STORE_ID, FIRST_ITEM_FIRST_STORE_ID,
                                            SECOND_ITEM_FIRST_STORE_ID, Operator.XOR);
            LogoutUserLoginBuyer(REGISTERED_SESSION_ID);
            Order order = VerifiedCheckoutOrder();
            VerifyItemDiscount(order, FIRST_STORE_ID, FIRST_ITEM_FIRST_STORE_ID, NO_DISCOUNT);
            VerifyItemDiscount(order, FIRST_STORE_ID, SECOND_ITEM_FIRST_STORE_ID, NO_DISCOUNT);
        }

        [Test]
        public void PurchaseWithDiscount_Comp_2CondItems_And_2Valid_HalfOff()
        {
            AddComposite_CondAll_CondAll(REGISTERED_SESSION_ID, FIRST_OPENER_USERNAME, FIRST_STORE_ID, FIRST_ITEM_FIRST_STORE_ID,
                                            SECOND_ITEM_FIRST_STORE_ID, Operator.AND, 
                                            minAmount1: FIRST_ITEM_FIRST_STORE_PURCHASE_AMOUNT, 
                                            minAmount2: SECOND_ITEM_FIRST_STORE_PURCHASE_AMOUNT);
            LogoutUserLoginBuyer(REGISTERED_SESSION_ID);
            Order order = VerifiedCheckoutOrder();
            VerifyItemDiscount(order, FIRST_STORE_ID, FIRST_ITEM_FIRST_STORE_ID, HALF_OFF);
            VerifyItemDiscount(order, FIRST_STORE_ID, SECOND_ITEM_FIRST_STORE_ID, HALF_OFF);
        }

        [Test]
        public void PurchaseWithDiscount_Comp_2CondItems_And_1NotValid_NoDiscount()
        {
            AddComposite_CondAll_CondAll(REGISTERED_SESSION_ID, FIRST_OPENER_USERNAME, FIRST_STORE_ID, FIRST_ITEM_FIRST_STORE_ID,
                                            SECOND_ITEM_FIRST_STORE_ID, Operator.AND,
                                            minAmount1: FIRST_ITEM_FIRST_STORE_PURCHASE_AMOUNT + 1,
                                            minAmount2: SECOND_ITEM_FIRST_STORE_PURCHASE_AMOUNT);
            LogoutUserLoginBuyer(REGISTERED_SESSION_ID);
            Order order = VerifiedCheckoutOrder();
            VerifyItemDiscount(order, FIRST_STORE_ID, FIRST_ITEM_FIRST_STORE_ID, NO_DISCOUNT);
            VerifyItemDiscount(order, FIRST_STORE_ID, SECOND_ITEM_FIRST_STORE_ID, NO_DISCOUNT);
        }

        [Test]
        public void PurchaseWithDiscount_Comp_2CondItems_And_2NotValid_NoDiscount()
        {
            AddComposite_CondAll_CondAll(REGISTERED_SESSION_ID, FIRST_OPENER_USERNAME, FIRST_STORE_ID, FIRST_ITEM_FIRST_STORE_ID,
                                            SECOND_ITEM_FIRST_STORE_ID, Operator.AND,
                                            minAmount1: FIRST_ITEM_FIRST_STORE_PURCHASE_AMOUNT + 1,
                                            minAmount2: SECOND_ITEM_FIRST_STORE_PURCHASE_AMOUNT + 1);
            LogoutUserLoginBuyer(REGISTERED_SESSION_ID);
            Order order = VerifiedCheckoutOrder();
            VerifyItemDiscount(order, FIRST_STORE_ID, FIRST_ITEM_FIRST_STORE_ID, NO_DISCOUNT);
            VerifyItemDiscount(order, FIRST_STORE_ID, SECOND_ITEM_FIRST_STORE_ID, NO_DISCOUNT);
        }

        [Test]
        public void PurchaseWithDiscount_Comp_2CondItems_Xor_2Valid_NoDiscount()
        {
            AddComposite_CondAll_CondAll(REGISTERED_SESSION_ID, FIRST_OPENER_USERNAME, FIRST_STORE_ID, FIRST_ITEM_FIRST_STORE_ID,
                                            SECOND_ITEM_FIRST_STORE_ID, Operator.XOR,
                                            minAmount1: FIRST_ITEM_FIRST_STORE_PURCHASE_AMOUNT,
                                            minAmount2: SECOND_ITEM_FIRST_STORE_PURCHASE_AMOUNT);
            LogoutUserLoginBuyer(REGISTERED_SESSION_ID);
            Order order = VerifiedCheckoutOrder();
            VerifyItemDiscount(order, FIRST_STORE_ID, FIRST_ITEM_FIRST_STORE_ID, NO_DISCOUNT);
            VerifyItemDiscount(order, FIRST_STORE_ID, SECOND_ITEM_FIRST_STORE_ID, NO_DISCOUNT);
        }

        [Test]
        public void PurchaseWithDiscount_Comp_2CondItems_Xor_1NotValid_HalfOff()
        {
            AddComposite_CondAll_CondAll(REGISTERED_SESSION_ID, FIRST_OPENER_USERNAME, FIRST_STORE_ID, FIRST_ITEM_FIRST_STORE_ID,
                                            SECOND_ITEM_FIRST_STORE_ID, Operator.XOR,
                                            minAmount1: FIRST_ITEM_FIRST_STORE_PURCHASE_AMOUNT + 1,
                                            minAmount2: SECOND_ITEM_FIRST_STORE_PURCHASE_AMOUNT);
            LogoutUserLoginBuyer(REGISTERED_SESSION_ID);
            Order order = VerifiedCheckoutOrder();
            VerifyItemDiscount(order, FIRST_STORE_ID, FIRST_ITEM_FIRST_STORE_ID, NO_DISCOUNT);
            VerifyItemDiscount(order, FIRST_STORE_ID, SECOND_ITEM_FIRST_STORE_ID, HALF_OFF);
        }

        [Test]
        public void PurchaseWithDiscount_Comp_2CondItems_Xor_2NotValid_NoDiscount()
        {
            AddComposite_CondAll_CondAll(REGISTERED_SESSION_ID, FIRST_OPENER_USERNAME, FIRST_STORE_ID, FIRST_ITEM_FIRST_STORE_ID,
                                            SECOND_ITEM_FIRST_STORE_ID, Operator.XOR,
                                            minAmount1: FIRST_ITEM_FIRST_STORE_PURCHASE_AMOUNT + 1,
                                            minAmount2: SECOND_ITEM_FIRST_STORE_PURCHASE_AMOUNT + 1);
            LogoutUserLoginBuyer(REGISTERED_SESSION_ID);
            Order order = VerifiedCheckoutOrder();
            VerifyItemDiscount(order, FIRST_STORE_ID, FIRST_ITEM_FIRST_STORE_ID, NO_DISCOUNT);
            VerifyItemDiscount(order, FIRST_STORE_ID, SECOND_ITEM_FIRST_STORE_ID, NO_DISCOUNT);
        }

        [Test]
        public void PurchaseWithDiscount_Comp_2CondItems_Or_2Valid_HalfOff()
        {
            AddComposite_CondAll_CondAll(REGISTERED_SESSION_ID, FIRST_OPENER_USERNAME, FIRST_STORE_ID, FIRST_ITEM_FIRST_STORE_ID,
                                            SECOND_ITEM_FIRST_STORE_ID, Operator.OR,
                                            minAmount1: FIRST_ITEM_FIRST_STORE_PURCHASE_AMOUNT,
                                            minAmount2: SECOND_ITEM_FIRST_STORE_PURCHASE_AMOUNT);
            LogoutUserLoginBuyer(REGISTERED_SESSION_ID);
            Order order = VerifiedCheckoutOrder();
            VerifyItemDiscount(order, FIRST_STORE_ID, FIRST_ITEM_FIRST_STORE_ID, HALF_OFF);
            VerifyItemDiscount(order, FIRST_STORE_ID, SECOND_ITEM_FIRST_STORE_ID, HALF_OFF);
        }

        [Test]
        public void PurchaseWithDiscount_Comp_2CondItems_Or_1NotValid_HalfOff()
        {
            AddComposite_CondAll_CondAll(REGISTERED_SESSION_ID, FIRST_OPENER_USERNAME, FIRST_STORE_ID, FIRST_ITEM_FIRST_STORE_ID,
                                            SECOND_ITEM_FIRST_STORE_ID, Operator.OR,
                                            minAmount1: FIRST_ITEM_FIRST_STORE_PURCHASE_AMOUNT + 1,
                                            minAmount2: SECOND_ITEM_FIRST_STORE_PURCHASE_AMOUNT);
            LogoutUserLoginBuyer(REGISTERED_SESSION_ID);
            Order order = VerifiedCheckoutOrder();
            VerifyItemDiscount(order, FIRST_STORE_ID, FIRST_ITEM_FIRST_STORE_ID, NO_DISCOUNT);
            VerifyItemDiscount(order, FIRST_STORE_ID, SECOND_ITEM_FIRST_STORE_ID, HALF_OFF);
        }

        [Test]
        public void PurchaseWithDiscount_Comp_2CondItems_Or_2NotValid_NoDiscount()
        {
            AddComposite_CondAll_CondAll(REGISTERED_SESSION_ID, FIRST_OPENER_USERNAME, FIRST_STORE_ID, FIRST_ITEM_FIRST_STORE_ID,
                                            SECOND_ITEM_FIRST_STORE_ID, Operator.OR,
                                            minAmount1: FIRST_ITEM_FIRST_STORE_PURCHASE_AMOUNT + 1,
                                            minAmount2: SECOND_ITEM_FIRST_STORE_PURCHASE_AMOUNT + 1);
            LogoutUserLoginBuyer(REGISTERED_SESSION_ID);
            Order order = VerifiedCheckoutOrder();
            VerifyItemDiscount(order, FIRST_STORE_ID, FIRST_ITEM_FIRST_STORE_ID, NO_DISCOUNT);
            VerifyItemDiscount(order, FIRST_STORE_ID, SECOND_ITEM_FIRST_STORE_ID, NO_DISCOUNT);
        }
    }
}

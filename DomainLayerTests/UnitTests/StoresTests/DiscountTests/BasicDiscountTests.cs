using System;
using System.Collections.Generic;
using DomainLayer.Stores.Discounts;
using NUnit.Framework;
using System.Reflection;

namespace DomainLayerTests.UnitTests.StoresTests.DiscountTests
{
    [TestFixture]
    public class BasicDiscountTests : IDiscountTests
    {

        private readonly string MOCK_NAME_FOR_DESCRIPTION = "mock";
        [SetUp]
        public override void Setup()
        {
            base.Setup();
            
        }

        

        public static IEnumerable<object[]> openDiscount_Success_cases
        {
            get {
                yield return new object[] { amount1price15, 0.9 };
                yield return new object[] { amount1price15, 1.0 };
                yield return new object[] { amount1price15, 0.5 };
                yield return new object[] { amount1price15, 0.0 };
                yield return new object[] { amount3price10, 0.9 };
                yield return new object[] { amount3price10, 1.0 };
                yield return new object[] { amount3price10, 0.5 };
                yield return new object[] { amount3price10, 0.0 };
                yield return new object[] { amount2price5, 0.9 };
                yield return new object[] { amount2price5, 1.0 };
                yield return new object[] { amount2price5, 0.5 };
                yield return new object[] { amount2price5, 0.0 };
                yield return new object[] { amount10price200, 0.9 };
                yield return new object[] { amount10price200, 1.0 };
                yield return new object[] { amount10price200, 0.5 };
                yield return new object[] { amount10price200, 0.0 };
                }
        }

        [Test]
        public void openDiscount_Success()
        {
            foreach(object[] obj in openDiscount_Success_cases)
            {
                try { openDiscountSuccessRunner((Guid)obj[0], (double)obj[1]); }
                catch (AssertionException)
                {
                    Assert.Fail(String.Format("amount: {0}, price: {1} , discount: {2}",
                        testCart[(Guid)obj[0]].Item1, testCart[(Guid)obj[0]].Item2, (double)obj[1]
                        ));
                }
            }
        }

        public void openDiscountSuccessRunner(Guid itemID, double discount)
        {
            OpenDiscount openDiscount = new OpenDiscount(itemID, discount, DateTime.MaxValue, MOCK_NAME_FOR_DESCRIPTION);
            Dictionary<Guid, Tuple<int, double>> outCart = openDiscount.GetUpdatedPricesFromCart(testCart);
            validateNothingButTargetItemChanged(outCart, testCart, itemID);
            Assert.AreEqual(testCart[itemID].Item1, outCart[itemID].Item1);
            Assert.AreEqual(testCart[itemID].Item2 * (1-discount), outCart[itemID].Item2);
        }


        [Test]
        public void ADiscount_ExpiredDate_Failure()
        {
            OpenDiscount openDiscount = new OpenDiscount(amount1price15, 0.9, DateTime.Now.AddDays(-1), MOCK_NAME_FOR_DESCRIPTION);
            Dictionary<Guid, Tuple<int, double>> outCart = openDiscount.GetUpdatedPricesFromCart(testCart);
            validateNothingButTargetItemChanged(outCart, testCart, amount1price15);
            Assert.AreEqual(testCart[amount1price15].Item1, outCart[amount1price15].Item1);
            Assert.AreEqual(testCart[amount1price15].Item2, outCart[amount1price15].Item2);

        }
        [Test]
        public void ADiscount_toDayDate_Success()
        {
            OpenDiscount openDiscount = new OpenDiscount(amount1price15, 0.9, DateTime.Now, MOCK_NAME_FOR_DESCRIPTION);
            Dictionary<Guid, Tuple<int, double>> outCart = openDiscount.GetUpdatedPricesFromCart(testCart);
            validateNothingButTargetItemChanged(outCart, testCart, amount1price15);
            Assert.AreEqual(testCart[amount1price15].Item1, outCart[amount1price15].Item1);
            Assert.AreEqual(testCart[amount1price15].Item2 * (1-0.9), outCart[amount1price15].Item2);

        }
        [Test]
        public void ADiscount_TommorowDate_Success()
        {
            OpenDiscount openDiscount = new OpenDiscount(amount1price15, 0.9, DateTime.Now.AddDays(1), MOCK_NAME_FOR_DESCRIPTION);
            Dictionary<Guid, Tuple<int, double>> outCart = openDiscount.GetUpdatedPricesFromCart(testCart);
            validateNothingButTargetItemChanged(outCart, testCart, amount1price15);
            Assert.AreEqual(testCart[amount1price15].Item1, outCart[amount1price15].Item1);
            Assert.AreEqual(testCart[amount1price15].Item2 * (1-0.9), outCart[amount1price15].Item2);

        }

        public static IEnumerable<object[]> ItemConditionalDiscount_MinItems_ToDiscountOnAll_SuccessAndFailure_cases
        {
            get{
                yield return new object[] { amount1price15, 0.9, 1, true };
                yield return new object[] { amount1price15, 0.9, 2, false };
                yield return new object[] { amount1price15, 0.9, 0, true };
                yield return new object[] { amount3price10, 0.9, 2, true };
                yield return new object[] { amount3price10, 1.0, 3, true };
                yield return new object[] { amount3price10, 0.5, 4, false };
            }
        }

        [Test]
        public void ItemConditionalDiscount_MinItems_ToDiscountOnAll_SuccessAndFailure()
        {
            foreach (object[] obj in ItemConditionalDiscount_MinItems_ToDiscountOnAll_SuccessAndFailure_cases)
            {
                try { ItemConditionalDiscount_MinItems_ToDiscountOnAll_SuccessAndFailureRunner(
                    (Guid)obj[0], (double)obj[1],(int)obj[2],(bool)obj[3]); }
                catch (AssertionException)
                {
                    Assert.Fail(String.Format("amount: {0}, price: {1} , discount: {2}, minAmount:{3}",
                        testCart[(Guid)obj[0]].Item1, testCart[(Guid)obj[0]].Item2, (double)obj[1], (int)obj[2]
                        ));
                }
            }
        }

        public void ItemConditionalDiscount_MinItems_ToDiscountOnAll_SuccessAndFailureRunner(Guid itemID, double discount, int minItems, bool success)
        {
            ItemConditionalDiscount_MinItems_ToDiscountOnAll DicountObj = new ItemConditionalDiscount_MinItems_ToDiscountOnAll(itemID, DateTime.MaxValue, minItems, discount, MOCK_NAME_FOR_DESCRIPTION);
            Dictionary<Guid, Tuple<int, double>> outCart = DicountObj.GetUpdatedPricesFromCart(testCart);
            validateNothingButTargetItemChanged(outCart, testCart, itemID);
            Assert.AreEqual(testCart[itemID].Item1, outCart[itemID].Item1);
            if (success)
            {
                Assert.AreEqual(testCart[itemID].Item2 * (1 - discount), outCart[itemID].Item2);
            }
            else
            {
                Assert.AreEqual(testCart[itemID].Item2, outCart[itemID].Item2);
            }
        }

        public static IEnumerable<object[]> ItemConditionalDiscount_MinItems_ToDiscountOnExtraItems_SuccessAndFailure_cases
        {
            get
            {
                yield return new object[] { amount1price15, 0.9, 1, 1, 15.0 };
                yield return new object[] { amount1price15, 0.9, 2, 4, 15.0 };
                yield return new object[] { amount1price15, 0.9, 0, 1, 15*(1-0.9) };
                yield return new object[] { amount3price10, 0.9, 2,1, 20.0/3.0 + 10.0*0.1/3.0 };
                yield return new object[] { amount10price200, 0.5, 3,2, 180.0 };
            }
        }

        [Test]
        public void ItemConditionalDiscount_MinItems_ToDiscountOnExtraItems_SuccessAndFailure()
        {
            foreach (object[] obj in ItemConditionalDiscount_MinItems_ToDiscountOnExtraItems_SuccessAndFailure_cases)
            {
                try
                {
                    ItemConditionalDiscount_MinItems_ToDiscountOnExtraItems_SuccessAndFailureRunner(
                  (Guid)obj[0], (double)obj[1], (int)obj[2],(int)obj[3],(double)obj[4]);
                }
                catch (AssertionException)
                {
                    Assert.Fail(String.Format("amount: {0}, price: {1} , discount: {2}, minAmount:{3}, extraItems: {4}",
                        testCart[(Guid)obj[0]].Item1, testCart[(Guid)obj[0]].Item2, (double)obj[1], (int)obj[2], (int)obj[3]
                        ));
                }
            }
        }

        public void ItemConditionalDiscount_MinItems_ToDiscountOnExtraItems_SuccessAndFailureRunner(Guid itemID, double discount, int minItems,int extraItems, double PriceAfter)
        {
            ItemConditionalDiscount_MinItems_ToDiscountOnExtraItems DicountObj = new ItemConditionalDiscount_MinItems_ToDiscountOnExtraItems(itemID, DateTime.MaxValue, minItems, extraItems, discount, MOCK_NAME_FOR_DESCRIPTION);
            Dictionary<Guid, Tuple<int, double>> outCart = DicountObj.GetUpdatedPricesFromCart(testCart);
            validateNothingButTargetItemChanged(outCart, testCart, itemID);
            Assert.AreEqual(testCart[itemID].Item1, outCart[itemID].Item1);
            Assert.AreEqual(PriceAfter, outCart[itemID].Item2);
        }

        [TestCase(0.5,0)]
        [TestCase(0.0, 0)]
        [TestCase(1.0, 0)]
        [TestCase(0.3, 0)]
        [TestCase(0.5, 0.1)]
        [TestCase(0.0, 0.1)]
        [TestCase(1.0, 0.1)]
        [TestCase(0.3, 0.1)]
        [TestCase(0.5, 0-0.1)]
        [TestCase(0.0, 0-0.1)]
        [TestCase(1.0, 0-0.1)]
        [TestCase(0.3, 0-0.1)]
        public void StoreConditionalDiscount_success(double discount, double amountToAddToTotalForMinPurchase)
        {
            double total = calcCartTotal(testCart);
            StoreConditionalDiscount storeConditionalDiscount = new StoreConditionalDiscount(DateTime.MaxValue,
                total + amountToAddToTotalForMinPurchase, discount);
            Dictionary<Guid, Tuple<int, double>> outCart = storeConditionalDiscount.GetUpdatedPricesFromCart(testCart);
            if (amountToAddToTotalForMinPurchase <= 0)
            {
                Assert.AreEqual(total * (1-discount), calcCartTotal(outCart));
            }
            else
            {
                Assert.AreEqual(total, calcCartTotal(outCart));
            }
            validateAmountsAreSame(outCart, testCart);
        }

        
    }
}

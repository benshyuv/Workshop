using System;
using System.Collections.Generic;
using DomainLayer.Stores.Discounts;
using Newtonsoft.Json;
using NUnit.Framework;

namespace DomainLayerTests.UnitTests.StoresTests.DiscountTests
{
    [TestFixture]
    public class CompositeDiscountTest: IDiscountTests
    {
        private readonly string MOCK_NAME_FOR_DESCRIPTION = "mock";

        [SetUp]
        public override void Setup()
        {
            base.Setup();
        }

        [Test]
        public void CompositeDiscount_serealizeIdsWithoutObjects()
        {
            OpenDiscount openDiscountItem1 = new OpenDiscount(amount1price15, 0.7, DateTime.Now, MOCK_NAME_FOR_DESCRIPTION);
            OpenDiscount openDiscountItem3 = new OpenDiscount(amount3price10, 0.3, DateTime.Now, MOCK_NAME_FOR_DESCRIPTION);

            CompositeDiscount compositeDiscount = new CompositeDiscount(openDiscountItem1, openDiscountItem3, Operator.AND);

            string json = JsonConvert.SerializeObject(compositeDiscount);

            CompositeDiscount deserialized = JsonConvert.DeserializeObject<CompositeDiscount>(json);

            Assert.AreEqual(openDiscountItem1.discountID, deserialized.DiscountLeftID);
            Assert.AreEqual(openDiscountItem3.discountID, deserialized.DiscountRightID);
            Assert.Null(deserialized.DiscountLeft);
            Assert.Null(deserialized.DiscountRight);

        }

        [TestCase(Operator.AND)]
        [TestCase(Operator.OR)]
        [TestCase(Operator.IMPLIES)]
        public void CompositeDiscount_2_open_both_disounted(Operator @operator)
        {
            OpenDiscount openDiscountItem1 = new OpenDiscount(amount1price15, 0.7, DateTime.MaxValue, MOCK_NAME_FOR_DESCRIPTION);
            OpenDiscount openDiscountItem3 = new OpenDiscount(amount3price10, 0.3, DateTime.MaxValue, MOCK_NAME_FOR_DESCRIPTION);

            CompositeDiscount compositeDiscount = new CompositeDiscount(openDiscountItem1, openDiscountItem3, @operator);
            Dictionary<Guid, Tuple<int, double>> outCart = compositeDiscount.GetUpdatedPricesFromCart(testCart);
            Assert.AreEqual(15.0 * (1-0.7), outCart[amount1price15].Item2);
            Assert.AreEqual(10.0 * (1-0.3), outCart[amount3price10].Item2);
            Assert.AreEqual(200, outCart[amount10price200].Item2);
            Assert.AreEqual(5, outCart[amount2price5].Item2);
            validateAmountsAreSame(outCart, testCart);

        }
        [TestCase(Operator.XOR)]
        public void CompositeDiscount_2_open_xor_none_discounted(Operator @operator)
        {
            OpenDiscount openDiscountItem1 = new OpenDiscount(amount1price15, 0.7, DateTime.MaxValue, MOCK_NAME_FOR_DESCRIPTION);
            OpenDiscount openDiscountItem3 = new OpenDiscount(amount3price10, 0.3, DateTime.MaxValue, MOCK_NAME_FOR_DESCRIPTION);

            CompositeDiscount compositeDiscount = new CompositeDiscount(openDiscountItem1, openDiscountItem3, @operator);
            Dictionary<Guid, Tuple<int, double>> outCart = compositeDiscount.GetUpdatedPricesFromCart(testCart);
            Assert.AreEqual(15, outCart[amount1price15].Item2);
            Assert.AreEqual(10.0, outCart[amount3price10].Item2);
            Assert.AreEqual(200, outCart[amount10price200].Item2);
            Assert.AreEqual(5, outCart[amount2price5].Item2);
            validateAmountsAreSame(outCart, testCart);
        }

        [TestCase(Operator.XOR)]
        [TestCase(Operator.OR)]
        public void CompositeDiscount_2_leftSucceeds_rightDoesnt_or_implies_xor_cases(Operator @operator)
        {
            ItemConditionalDiscount_MinItems_ToDiscountOnAll discountObjleft = new ItemConditionalDiscount_MinItems_ToDiscountOnAll(amount1price15, DateTime.MaxValue, 1, 0.7, MOCK_NAME_FOR_DESCRIPTION);
            ItemConditionalDiscount_MinItems_ToDiscountOnAll discountObjright = new ItemConditionalDiscount_MinItems_ToDiscountOnAll(amount3price10, DateTime.MaxValue, 5, 0.5, MOCK_NAME_FOR_DESCRIPTION);

            CompositeDiscount compositeDiscount = new CompositeDiscount(discountObjleft, discountObjright, @operator);
            Dictionary<Guid, Tuple<int, double>> outCart = compositeDiscount.GetUpdatedPricesFromCart(testCart);
            Assert.AreEqual(15.0 * (1-0.7), outCart[amount1price15].Item2);
            Assert.AreEqual(10, outCart[amount3price10].Item2);
            Assert.AreEqual(200, outCart[amount10price200].Item2);
            Assert.AreEqual(5, outCart[amount2price5].Item2);
            validateAmountsAreSame(outCart, testCart);

        }

        [TestCase(Operator.AND)]
        [TestCase(Operator.IMPLIES)]
        public void CompositeDiscount_2_leftSucceeds_rightDoesnt_and_implies_case(Operator @operator)
        {
            ItemConditionalDiscount_MinItems_ToDiscountOnAll discountObjleft = new ItemConditionalDiscount_MinItems_ToDiscountOnAll(amount1price15, DateTime.MaxValue, 1, 0.7, MOCK_NAME_FOR_DESCRIPTION);
            ItemConditionalDiscount_MinItems_ToDiscountOnAll discountObjright = new ItemConditionalDiscount_MinItems_ToDiscountOnAll(amount3price10, DateTime.MaxValue,5,0.5, MOCK_NAME_FOR_DESCRIPTION);

            CompositeDiscount compositeDiscount = new CompositeDiscount(discountObjleft, discountObjright, @operator);
            Dictionary<Guid, Tuple<int, double>> outCart = compositeDiscount.GetUpdatedPricesFromCart(testCart);
            Assert.AreEqual(15, outCart[amount1price15].Item2);
            Assert.AreEqual(10, outCart[amount3price10].Item2);
            Assert.AreEqual(200, outCart[amount10price200].Item2);
            Assert.AreEqual(5, outCart[amount2price5].Item2);
            validateAmountsAreSame(outCart, testCart);

        }
        [TestCase(Operator.AND)]
        public void CompositeDiscount_2_rightSucceeds_leftDoesnt_and_implies_case(Operator @operator)
        {
            ItemConditionalDiscount_MinItems_ToDiscountOnAll discountObjright = new ItemConditionalDiscount_MinItems_ToDiscountOnAll(amount1price15, DateTime.MaxValue, 1, 0.7, MOCK_NAME_FOR_DESCRIPTION);
            ItemConditionalDiscount_MinItems_ToDiscountOnAll discountObjleft = new ItemConditionalDiscount_MinItems_ToDiscountOnAll(amount3price10, DateTime.MaxValue, 5, 0.5, MOCK_NAME_FOR_DESCRIPTION);

            CompositeDiscount compositeDiscount = new CompositeDiscount(discountObjleft, discountObjright, @operator);
            Dictionary<Guid, Tuple<int, double>> outCart = compositeDiscount.GetUpdatedPricesFromCart(testCart);
            Assert.AreEqual(15, outCart[amount1price15].Item2);
            Assert.AreEqual(10, outCart[amount3price10].Item2);
            Assert.AreEqual(200, outCart[amount10price200].Item2);
            Assert.AreEqual(5, outCart[amount2price5].Item2);
            validateAmountsAreSame(outCart, testCart);

        }

        [TestCase(Operator.OR)]
        [TestCase(Operator.XOR)]
        [TestCase(Operator.IMPLIES)]
        public void CompositeDiscount_2_rightSucceeds_leftDoesnt_xor_or_case(Operator @operator)
        {
            ItemConditionalDiscount_MinItems_ToDiscountOnAll discountObjright = new ItemConditionalDiscount_MinItems_ToDiscountOnAll(amount1price15, DateTime.MaxValue, 1, 0.7, MOCK_NAME_FOR_DESCRIPTION);
            ItemConditionalDiscount_MinItems_ToDiscountOnAll discountObjleft = new ItemConditionalDiscount_MinItems_ToDiscountOnAll(amount3price10, DateTime.MaxValue, 5, 0.5, MOCK_NAME_FOR_DESCRIPTION);

            CompositeDiscount compositeDiscount = new CompositeDiscount(discountObjleft, discountObjright, @operator);
            Dictionary<Guid, Tuple<int, double>> outCart = compositeDiscount.GetUpdatedPricesFromCart(testCart);
            Assert.AreEqual(15.0 * (1-0.7), outCart[amount1price15].Item2);
            Assert.AreEqual(10, outCart[amount3price10].Item2);
            Assert.AreEqual(200, outCart[amount10price200].Item2);
            Assert.AreEqual(5, outCart[amount2price5].Item2);
            validateAmountsAreSame(outCart, testCart);

        }

        [TestCase(Operator.AND)]
        [TestCase(Operator.OR)]
        [TestCase(Operator.IMPLIES)]
        [TestCase(Operator.XOR)]
        public void CompositeDiscount_2_rightSucceeds_both_fail_all_operators_case(Operator @operator)
        {
            ItemConditionalDiscount_MinItems_ToDiscountOnAll discountObjright = new ItemConditionalDiscount_MinItems_ToDiscountOnAll(amount1price15, DateTime.MaxValue, 5, 0.7, MOCK_NAME_FOR_DESCRIPTION);
            ItemConditionalDiscount_MinItems_ToDiscountOnAll discountObjleft = new ItemConditionalDiscount_MinItems_ToDiscountOnAll(amount3price10, DateTime.MaxValue, 5, 0.5, MOCK_NAME_FOR_DESCRIPTION);

            CompositeDiscount compositeDiscount = new CompositeDiscount(discountObjleft, discountObjright, @operator);
            Dictionary<Guid, Tuple<int, double>> outCart = compositeDiscount.GetUpdatedPricesFromCart(testCart);
            Assert.AreEqual(15, outCart[amount1price15].Item2);
            Assert.AreEqual(10, outCart[amount3price10].Item2);
            Assert.AreEqual(200, outCart[amount10price200].Item2);
            Assert.AreEqual(5, outCart[amount2price5].Item2);
            validateAmountsAreSame(outCart, testCart);

        }

    }
}

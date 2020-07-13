using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace DomainLayerTests.UnitTests.StoresTests.DiscountTests
{
    public class IDiscountTests


    {

        public static Guid amount3price10 = Guid.NewGuid();
        public static Guid amount1price15 = Guid.NewGuid();
        public static Guid amount2price5 = Guid.NewGuid();
        public static Guid amount10price200 = Guid.NewGuid();
        public Dictionary<Guid, Tuple<int, double>> testCart;

        public virtual void Setup()
        {
            testCart = new Dictionary<Guid, Tuple<int, double>>();
            PopulateTestCart();
        }

        public void PopulateTestCart()
        {
            testCart[amount3price10] = new Tuple<int, double>(3, 10);
            testCart[amount1price15] = new Tuple<int, double>(1, 15);
            testCart[amount2price5] = new Tuple<int, double>(2, 5);
            testCart[amount10price200] = new Tuple<int, double>(10, 200);

        }

        public void validateAmountsAreSame(Dictionary<Guid, Tuple<int, double>> toCheck, Dictionary<Guid, Tuple<int, double>> original)
        {
            foreach (Guid itemID in original.Keys)
            {
                Assert.AreEqual(original[itemID].Item1, toCheck[itemID].Item1);
            }
        }

        public double calcCartTotal(Dictionary<Guid, Tuple<int, double>> cart)
        {
            double cartTotal = 0;
            foreach (Guid key in cart.Keys)
            {
                cartTotal += cart[key].Item2;
            }
            return cartTotal;
        }

        public void validateNothingButTargetItemChanged(Dictionary<Guid, Tuple<int, double>> toCheck, Dictionary<Guid, Tuple<int, double>> original, Guid targetItem)
        {
            foreach (Guid itemID in original.Keys)
            {
                if (!itemID.Equals(targetItem))
                {
                    Assert.AreEqual(original[itemID], toCheck[itemID]);
                }
            }
        }

    }
}

using System;
using System.Collections.Generic;
using System.Text;
using DomainLayer.Exceptions;
using DomainLayer.Users;
using NUnit.Framework;

namespace DomainLayerTests.UnitTests.UsersTests
{
	class ShoppingCartTests
	{
		private ShoppingCart cart;
		Guid itemID1;
		Guid itemID2;
		Guid itemID3;
		Guid storeID1;
		Guid storeID2;

		[SetUp]
		public void Setup()
		{
			cart = new ShoppingCart(Guid.NewGuid());
			itemID1 = Guid.NewGuid();
			itemID2 = Guid.NewGuid();
			itemID3 = Guid.NewGuid();
			storeID1 = Guid.NewGuid();
			storeID2 = Guid.NewGuid();
		}

		public void Add1Item1Store1()
		{
			cart.AddToCart(storeID1, itemID1, 1);
		}

		[Test]
		public void ItemAmount_ExsitingItem_ShouldPass()
		{
			Add1Item1Store1();
			Assert.AreEqual(1, cart.ItemAmount(storeID1, itemID1));
		}

		[Test]
		public void ItemAmount_NonExsitingStore_ShouldFail()
		{
			Assert.Throws<CartException>(()=>cart.ItemAmount(storeID1, itemID1));
		}

		[Test]
		public void ItemAmount_NonExsitingItem_ShouldFail()
		{
			Add1Item1Store1();
			Assert.Throws<CartException>(() => cart.ItemAmount(storeID1, itemID2));
		}

		[Test]
		public void RemoveFromCart_ExsitingItem_ShouldPass()
		{
			Add1Item1Store1();
			Assert.DoesNotThrow(() => cart.RemoveFromCart(storeID1, itemID1));
			Assert.Throws<CartException>(() => cart.ItemAmount(storeID1, itemID1));
		}

		[Test]
		public void RemoveFromCart_NonExsitingStore_ShouldFail()
		{
			Assert.Throws<CartException>(() => cart.RemoveFromCart(storeID1, itemID1));
		}

		[Test]
		public void RemoveFromCart_NonExsitingItem_ShouldFail()
		{
			Add1Item1Store1();
			Assert.Throws<CartException>(() => cart.RemoveFromCart(storeID1, itemID2));
		}

		[Test]
		public void SetItemAmount_ExsitingItem_ShouldPass()
		{
			Add1Item1Store1();
			Assert.DoesNotThrow(() => cart.SetItemAmount(storeID1, itemID1, 2));
			Assert.AreEqual(2, cart.ItemAmount(storeID1, itemID1));
		}

		[Test]
		public void SetItemAmount_NonExsitingStore_ShouldFail()
		{
			Assert.Throws<CartException>(() => cart.SetItemAmount(storeID1, itemID1, 2));
		}

		[Test]
		public void SetItemAmount_NonExsitingItem_ShouldFail()
		{
			Add1Item1Store1();
			Assert.Throws<CartException>(() => cart.SetItemAmount(storeID1, itemID2, 2));
		}
	}
}

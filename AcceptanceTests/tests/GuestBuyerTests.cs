using System;
using System.Collections.Generic;
using AcceptanceTests.DataObjects;
using AcceptanceTests.OperationsAPI;
using NUnit.Framework;

namespace AcceptanceTests.tests
{
    public class GuestBuyerTests
    {


        public static string[][] UserNameAndPasswords =
        {
                new string[] { "user1", "pw1" },
                new string[] { "user2", "p@ssw0rd" },
                new string[] { "user3", "Pa55W0Rd" }
        };

        public ISystemOperationsBridge storeOperations;

        [SetUp]
        public virtual void Setup() {
            storeOperations = new SystemOperationsProxy();
        }

        [Test, TestCaseSource("UserNameAndPasswords")]
        public virtual void UC_2_2_register_successScenario(String userName, String pw)
        {
            Assert.True(storeOperations.registerNewUser(userName, pw));
        }

        [Test, TestCaseSource("UserNameAndPasswords")]
        public virtual void UC_2_2_register_existingUserName_failureScenario(String userName, String pw)
        {
            storeOperations.registerNewUser(userName, pw);
            Assert.False(storeOperations.registerNewUser(userName, pw));
        }

        [Test, TestCaseSource("UserNameAndPasswords")]
        public virtual void UC_2_3_login_existingUserName_successScenario(String userName, String pw)
        {
            storeOperations.registerNewUser(userName, pw);
            Assert.True(storeOperations.login(userName, pw));
        }

        [Test, TestCaseSource("UserNameAndPasswords")]
        public void UC_2_3_login_invalidDetails_failureScenario(String userName, String pw)
        {
            string invalidPwExtension = "i";
            storeOperations.registerNewUser(userName, pw);
            Assert.False(storeOperations.login(userName, pw.Substring(0, pw.Length - 1)));
            Assert.False(storeOperations.login(userName, pw + invalidPwExtension));
        }

        [Test]
        public void UC_2_4_watchInformationOfStoresAndProducts_successScenario()
        {
            List<Store> storesWithItems = storeOperations.generateRandomListOfStoresWithItems();
            CollectionAssert.AreEquivalent(storesWithItems, storeOperations.getAllStoresAndItems());
        }

        public static Item bannana = new Item("bannana", new List<string> { "fruit","yellow","yum" }, new List<string> { "fruit" }, 5.5);
        public static Item iphone = new Item("iphone", new List<string> { "apple" }, new List<string> { "phone" }, 10.1);
        public static Item galaxy = new Item("galaxy", new List<string> { "samsung" }, new List<string> { "phone" }, 3.2);
        public static Item spellcheck = new Item("item", new List<string> { "spell check" }, new List<string> { "spell" }, 100);

        public static TestCaseData[] searchForProduct_legalValuesAndMissplled_cases =
            {
                //easy
                new TestCaseData ( new List<Item> { bannana, iphone, galaxy },
                    new SearchDetail(null, null, null,null,"iphone","phone",new string []{"apple" }),
                    new List<Item> { iphone } ).SetName("search1"),
                // and of category and keyword
                new TestCaseData ( new List<Item> { bannana, iphone, galaxy },
                    new SearchDetail(null, null, null,null,null,"phone",new string []{"apple" }),
                    new List<Item> { iphone } ).SetName("search2"),
                //multiple items by category
                new TestCaseData( new List<Item> { bannana, iphone, galaxy },
                    new SearchDetail(null, null,null, null,null,"phone",null), new List<Item> { iphone, galaxy } ).SetName("search3"),
                //one item by category
                new TestCaseData( new List<Item> { bannana, iphone, galaxy },
                    new SearchDetail(null, null,null, null,null,"fruit",null), new List<Item> { bannana } ).SetName("search5"),
                //one name missing letter spellcheck
                new TestCaseData( new List<Item> { bannana, iphone, galaxy ,},
                    new SearchDetail(null, null,null, null,"iphonee",null,null),
                    new List<Item> { iphone } ).SetName("search7"),
                //spellcheck diff letter 
                new TestCaseData( new List<Item> { bannana, iphone, galaxy ,spellcheck},
                    new SearchDetail(null, null,null, null,"itam",null,null),
                    new List<Item> { spellcheck } ).SetName("spellingpass"),
                //spellcheck more letter easy
                new TestCaseData( new List<Item> { bannana, iphone, galaxy ,spellcheck},
                    new SearchDetail(null, null,null, null,"iteem",null,null),
                    new List<Item> { spellcheck } ).SetName("spellingpass1"),
                //name check  phonetic
                new TestCaseData( new List<Item> { bannana, iphone, galaxy },
                    new SearchDetail(null, null,null, null,"iphona",null,null), new List<Item> { iphone } ).SetName("search8"),
                //one item with spell check, more complex spelling error
                new TestCaseData( new List<Item> { bannana, iphone, galaxy },
                    new SearchDetail(null, null,null, null,"bannane",null,null), new List<Item> { bannana } ).SetName("search9"),
                //no items
                new TestCaseData( new List<Item> { bannana, iphone, galaxy },
                    new SearchDetail(null, null,null, null,"thisShouldNotMatch",null,null),new List<Item> { } ).SetName("search10"),
                //no items
                new TestCaseData( new List<Item> { bannana, iphone, galaxy },
                    new SearchDetail(null, null,null, null,null, "thisShouldNotMatch", null), new List<Item> { } ).SetName("search11"),
                //no items
                new TestCaseData( new List<Item> { bannana, iphone, galaxy },
                    new SearchDetail(null, null,null, null,null, null, new string []{"thisShouldNotMatch" }), new List<Item> { } ).SetName("search12"),
                //no items
                new TestCaseData( new List<Item> { bannana, iphone, galaxy },
                    new SearchDetail(null, null,null, null,"thisShouldNotMatch", "thisShouldNotMatch",
                        new string []{"thisShouldNotMatch" }), new List<Item> { } ).SetName("search13"),
                //filter max price just under
                new TestCaseData( new List<Item> { bannana, iphone, galaxy },
                    new SearchDetail(null, null,10.09, null,null,"phone",null), new List<Item> { galaxy } ).SetName("search14"),
                //filter max price same
                new TestCaseData( new List<Item> { bannana, iphone, galaxy },
                    new SearchDetail(null, null,10.1, null,null,"phone",null), new List<Item> { iphone, galaxy } ).SetName("search15"),
                //filter max price over
                new TestCaseData( new List<Item> { bannana, iphone, galaxy },
                    new SearchDetail(null, null,15, null,null,"phone",null), new List<Item> { iphone, galaxy } ).SetName("search16"),
                 //filter min price just over
                new TestCaseData( new List<Item> { bannana, iphone, galaxy },
                    new SearchDetail(null, 3.21,null, null,null,"phone",null), new List<Item> { iphone } ).SetName("search17"),
                //filter min price same
                new TestCaseData( new List<Item> { bannana, iphone, galaxy },
                    new SearchDetail(null, 3.2,null, null,null,"phone",null), new List<Item> { iphone, galaxy } ).SetName("search18"),
                //filter min price over
                new TestCaseData( new List<Item> { bannana, iphone, galaxy },
                    new SearchDetail(null, 1,null, null,null,"phone",null), new List<Item> { iphone, galaxy } ).SetName("search19"),
                //filter max min no items
                new TestCaseData( new List<Item> { bannana, iphone, galaxy },
                    new SearchDetail(null, 5, 10,null,null,"phone",null), new List<Item> { } ).SetName("search20"),
                //filter max min both  items
                new TestCaseData( new List<Item> { bannana, iphone, galaxy },
                    new SearchDetail(null, 1, 15,null,null,"phone",null), new List<Item> { iphone, galaxy } ).SetName("search21"),
                //only filter, all items
                new TestCaseData( new List<Item> { bannana, iphone, galaxy },
                    new SearchDetail(null, 1, 15,null,null,null,null), new List<Item> { iphone, galaxy, bannana } ).SetName("search22"),
                //only filter, 1 item
                new TestCaseData( new List<Item> { bannana, iphone, galaxy },
                    new SearchDetail(null, 5, 6,null,null,null,null), new List<Item> { bannana } ).SetName("search23"),
            };



        [Test,TestCaseSource("searchForProduct_legalValuesAndMissplled_cases")]
        public void UC_2_5_searchForProduct_legalValuesAndMissplled_successScenario(List<Item> itemsToInsert,
            SearchDetail searchDetail, List<Item> expectedItemsToBeReturnedWithoutGUID)
        {
            List<Store> storesWithItems = storeOperations.makeAndInsertItemsToRandomStores(itemsToInsert);
            List<Item> allItemsFromStoresWithGUID = getAllItemsFromStores(storesWithItems);
            List<Item> expectedSearchResultWithGUID = getMatchingItemsWithoutConsideringGUID(expectedItemsToBeReturnedWithoutGUID, allItemsFromStoresWithGUID);
            List<Item> itemsMatchingSearch = storeOperations.searchAllStores(searchDetail);

            CollectionAssert.AreEquivalent(expectedSearchResultWithGUID, itemsMatchingSearch);

        }

        public List<Item> getMatchingItemsWithoutConsideringGUID(List<Item> listToGet, List<Item> listToGetFrom)
        {
            List<Item> returnList = new List<Item>();
            foreach(Item itemToGet in listToGet)
            {
                Item itemFromList = listToGetFrom.Find(item => item.isEqualWithoutGUID(itemToGet));
                if (itemFromList != null)
                {
                    returnList.Add(itemFromList);
                }
            }

            return returnList;
        }

        public List<Item> getAllItemsFromStores(List<Store> stores)
        {
            List<Item> items = new List<Item>();
            foreach (Store store in stores)
            {
                foreach(KeyValuePair<Item,int> itemWithAmount in store.ItemsWithAmount)
                {
                    items.Add(itemWithAmount.Key);
                }
            }
            return items;
        }


        public static object[] addItemToShoppingCart_firstBuyFromStore_cases =
            {
                new object[] { bannana, 1 , new Store(new Item[] { bannana, iphone, galaxy }, new int[] { 1, 1, 1 }), true},
                new object[] { bannana, 2 , new Store(new Item[] { bannana, iphone, galaxy }, new int[] { 3, 3, 3 }), true},
                new object[] { bannana, 3 , new Store(new Item[] { bannana, iphone, galaxy }, new int[] { 3, 3, 3 }), true},
                
        };

        [Test, TestCaseSource("addItemToShoppingCart_firstBuyFromStore_cases")]
        public virtual void UC_2_6_addItemToShoppingCart_firstBuyFromStore_successAndFailureScenario(Item itemToBuywithoutGUID,int amountToBuy, Store storeToGenerateAndBuyFrom,
            bool success)
        {
            Store generatedStoreWithItems = storeOperations.generateStoreWithItemsAndAmountsWithoutLogin(storeToGenerateAndBuyFrom);
            List<Item> allItemsFromStoresWithGUID = getAllItemsFromStores(new List<Store> { generatedStoreWithItems});
            Item itemToBuyWithGUID = getMatchingItemsWithoutConsideringGUID(new List<Item> { itemToBuywithoutGUID }, allItemsFromStoresWithGUID)[0];

            
            Assert.AreEqual(success, storeOperations.addItemToCartFromStoreWithAmount(itemToBuyWithGUID, generatedStoreWithItems, amountToBuy));

            
        }

        public static IEnumerable<TestCaseData> addItemToShoppingCart_secondBuyFromStore_Cases()
        {
            yield return new TestCaseData(new Item[2] { iphone, bannana }, 1,
                new Store(new Item[] { bannana, iphone, galaxy }, new int[] { 1, 1, 1 }), true);
            yield return new TestCaseData(new Item[2] { iphone, bannana }, 2,
                new Store(new Item[] { bannana, iphone, galaxy }, new int[] { 3, 3, 3 }), true);
           
            yield return new TestCaseData(new Item[2] { iphone, bannana }, 3,
                new Store(new Item[] { bannana, iphone, galaxy }, new int[] { 3, 3, 3 }), true);
            //item doesnt exist so cant find it in order to buy
            //yield return new TestCaseData(new Item[2] { iphone, bannana }, 1,
            //    new Store(new Item[] { iphone, galaxy }, new int[] { 1, 1 }), false);
            yield return new TestCaseData(new Item[2] { iphone, bannana }, -1,
                new Store(new Item[] { bannana, iphone, galaxy }, new int[] { 1, 1, 1 }), false);
            yield return new TestCaseData(new Item[2] { iphone, bannana }, 0,
                new Store(new Item[] { bannana, iphone, galaxy }, new int[] { 3, 4, 3 }), false);
            yield return new TestCaseData(new Item[2] { iphone, bannana }, 4 ,
                new Store(new Item[] { bannana, iphone, galaxy }, new int[] { 3, 3, 3 }), false);

        }

        //items is an array of size 2, first item to be bought in order to make cart, second item to be tested on, first item will be bought with amount 1
        [Test, TestCaseSource("addItemToShoppingCart_secondBuyFromStore_Cases")]
        public virtual void UC_2_6_addItemToShoppingCart_secondBuyFromStore_successAndFailureScenario(Item[] itemsToBuywithoutGUID, int amountToBuy, Store storeToGenerateAndBuyFrom,
            bool success)
        {
            Store generatedStoreWithItems = storeOperations.generateStoreWithItemsAndAmountsWithoutLogin(storeToGenerateAndBuyFrom);
            List<Item> allItemsFromStoresWithGUID = getAllItemsFromStores(new List<Store> { generatedStoreWithItems });
            Item dummyItemToBuyWithGUIDForCartCreation = getMatchingItemsWithoutConsideringGUID(new List<Item> { itemsToBuywithoutGUID[0] }, allItemsFromStoresWithGUID)[0];
            Assert.True(storeOperations.addItemToCartFromStoreWithAmount(dummyItemToBuyWithGUIDForCartCreation, generatedStoreWithItems, 1));

            Item itemToBuyWithGUID = getMatchingItemsWithoutConsideringGUID(new List<Item> { itemsToBuywithoutGUID[1] }, allItemsFromStoresWithGUID)[0];


            Assert.AreEqual(success, storeOperations.addItemToCartFromStoreWithAmount(itemToBuyWithGUID, generatedStoreWithItems, amountToBuy));
           
        }

        public static object[] updateAmountInShoppingCartSuccessOrFailCases =
            {
                new object[] { bannana,1,bannana,2, new Store(new Item[] { bannana, iphone, galaxy }, new int[] { 2, 1, 1 }), true},
                new object[] { bannana,1,bannana,2, new Store(new Item[] { bannana, iphone, galaxy }, new int[] { 1, 1, 1 }), false},
                new object[] { bannana,1,iphone,1, new Store(new Item[] { bannana, iphone, galaxy }, new int[] { 2, 1, 1 }), false },
                //cant update to 0
                new object[] { bannana,1,bannana,0, new Store(new Item[] { bannana, iphone, galaxy }, new int[] { 2, 1, 1 }), false},
                new object[] { bannana,1,bannana,1, new Store(new Item[] { bannana, iphone, galaxy }, new int[] { 2, 1, 1 }), true},
                new object[] { bannana,1,bannana,4, new Store(new Item[] { bannana, iphone, galaxy }, new int[] { 2, 1, 1 }), false},
                new object[] { bannana,1,bannana,2, new Store(new Item[] { bannana, iphone, galaxy }, new int[] { 3, 1, 1 }), true},

        };

        [Test, TestCaseSource("updateAmountInShoppingCartSuccessOrFailCases")]
        public virtual void UC_2_7_updateAmountInShoppingCart_successAndFailureScenario(Item itemToSetupWith, int amountSetUp,
            Item itemToUpdate, int amountUpdate, Store storeToGenerateAndBuyFrom, bool success)
        {
            Store generatedStoreWithItems = storeOperations.generateStoreWithItemsAndAmountsWithoutLogin(storeToGenerateAndBuyFrom);
            List<Item> allItemsFromStoresWithGUID = getAllItemsFromStores(new List<Store> { generatedStoreWithItems });
            Item itemToSetupWithGUID = getMatchingItemsWithoutConsideringGUID(new List<Item> { itemToSetupWith }, allItemsFromStoresWithGUID)[0];
            Item itemToUpdateWithGUID = getMatchingItemsWithoutConsideringGUID(new List<Item> { itemToUpdate }, allItemsFromStoresWithGUID)[0];

            storeOperations.addItemToCartFromStoreWithAmount(itemToSetupWithGUID, generatedStoreWithItems, amountSetUp);

            Assert.AreEqual(success, storeOperations.updateItemAmountInCartAndValidateUpdated(
                generatedStoreWithItems.Id, itemToUpdateWithGUID, amountUpdate));

        }
        public static object[] deleteItemInShoppingCartSuccessOrFailCases =
            {
                new object[] { bannana,1,bannana, new Store(new Item[] { bannana, iphone, galaxy }, new int[] { 2, 1, 1 }), true},
                new object[] { bannana,2,bannana, new Store(new Item[] { bannana, iphone, galaxy }, new int[] { 2, 1, 1 }), true},
                new object[] { bannana,1,iphone, new Store(new Item[] { bannana, iphone, galaxy }, new int[] { 2, 1, 1 }), false },
        };

        [Test, TestCaseSource("deleteItemInShoppingCartSuccessOrFailCases")]
        public virtual void UC_2_7_DeleteItemInShoppingCart_successAndFailureScenario(Item itemToSetupWith, int amountSetUp,
            Item itemToDelete, Store storeToGenerateAndBuyFrom, bool success)
        {
            Store generatedStoreWithItems = storeOperations.generateStoreWithItemsAndAmountsWithoutLogin(storeToGenerateAndBuyFrom);
            List<Item> allItemsFromStoresWithGUID = getAllItemsFromStores(new List<Store> { generatedStoreWithItems });
            Item itemToSetupWithGUID = getMatchingItemsWithoutConsideringGUID(new List<Item> { itemToSetupWith }, allItemsFromStoresWithGUID)[0];
            Item itemToDeleteWithGUID = getMatchingItemsWithoutConsideringGUID(new List<Item> { itemToDelete }, allItemsFromStoresWithGUID)[0];

            storeOperations.addItemToCartFromStoreWithAmount(itemToSetupWithGUID, generatedStoreWithItems, amountSetUp);

            Assert.AreEqual(success, storeOperations.deleteItemInCartAndValidateDeleted(
                generatedStoreWithItems.Id, itemToDeleteWithGUID));

        }


        public static object[] buyProductsCases =
            {
                //success 1 cart
                new object[] { new Item[]{bannana },new int[]{1 },  new Store[] {new Store(new Item[] { bannana, iphone, galaxy }, new int[] { 1, 1, 1 }) },
                      new Item[]{}, new int[]{ }, new string[]{"4444-3333-2222-1111","03/30","333","yossi","212212212" }, new string [] { "Borer 20", "Tel Aviv", "Israel", "20693"}, true },

               //success 2 carts
               new object[] { new Item[]{bannana,iphone },new int[]{1,1 },  new Store[]{new Store(new Item[] { bannana, galaxy }, new int[] { 1, 1 }),
                    new Store(new Item[]{iphone },new int[]{1}) },
                      new Item[]{}, new int[]{ }, new string[]{"4444-3333-2222-1111","03/30","333","yossi","212212212" }, new string [] { "Borer 20", "Tel Aviv", "Israel", "20693"}, true },
               //success 2 carts mulypul amounts
               new object[] { new Item[]{bannana,iphone },new int[]{3,2 },  new Store[]{new Store(new Item[] { bannana, galaxy }, new int[] { 4, 1 }),
                    new Store(new Item[]{iphone },new int[]{2}) },
                      new Item[]{}, new int[]{ }, new string[]{"4444-3333-2222-1111","03/30","333","yossi","212212212" }, new string [] { "Borer 20", "Tel Aviv", "Israel", "20693"}, true },

               //inbetween amount not available is 0
               new object[] { new Item[]{bannana },new int[]{1 },  new Store[] {new Store(new Item[] { bannana, iphone, galaxy }, new int[] { 1, 1, 1 }) },
                      new Item[]{bannana}, new int[]{ 1}, new string[]{"4444-3333-2222-1111","03/30","333","yossi","212212212" }, new string [] { "Borer 20", "Tel Aviv", "Israel", "20693"}, false },
               //inbetween amount not available is less than demanded
               new object[] { new Item[]{bannana },new int[]{3 },  new Store[] {new Store(new Item[] { bannana, iphone, galaxy }, new int[] { 3, 1, 1 }) },
                      new Item[]{bannana}, new int[]{ 1}, new string[]{"4444-3333-2222-1111","03/30","333","yossi","212212212" }, new string [] { "Borer 20", "Tel Aviv", "Israel", "20693"}, false },
               //inbetween but still more than enough
               new object[] { new Item[]{bannana },new int[]{3 },  new Store[] {new Store(new Item[] { bannana, iphone, galaxy }, new int[] { 5, 1, 1 }) },
                      new Item[]{bannana}, new int[]{ 1}, new string[]{"4444-3333-2222-1111","03/30","333","yossi","212212212" }, new string [] { "Borer 20", "Tel Aviv", "Israel", "20693"}, true },
               //inbetween but still just enough
               new object[] { new Item[]{bannana },new int[]{3 },  new Store[] {new Store(new Item[] { bannana, iphone, galaxy }, new int[] { 4, 1, 1 }) },
                      new Item[]{bannana}, new int[]{ 1}, new string[]{"4444-3333-2222-1111","03/30","333","yossi","212212212" }, new string [] { "Borer 20", "Tel Aviv", "Israel", "20693"}, true },
               //no products in cart
               new object[] { new Item[]{ },new int[]{ },  new Store[] {new Store(new Item[] { bannana, iphone, galaxy }, new int[] { 1, 1, 1 }) },
                      new Item[]{}, new int[]{ }, new string[]{"4444-3333-2222-1111","03/30","333" ,"yossi","212212212"}, new string [] { "Borer 20", "Tel Aviv", "Israel", "20693"}, false }

        };

        //PRE: itemsToAddToCartWithoutGUID.lenght == amountToAddToCartWithoutGUID.lenght == storeToGenerateAndBuyFrom.lenght
        [Test, TestCaseSource("buyProductsCases")]
        public virtual void UC_2_8_buyProductImmediate_successAndFailure(Item[] itemsToAddToCartWithoutGUID, int[] amountToAddToCart,
            Store[] storesToGenerateAndBuyFrom,  Item[] itemsToBeBoughtBetweenCartAddAndPurchaseWithoutGUID,
            int[] amountToBeBoughtBetweenCartAddAndPurchase, string[] paymentDetails, string[] deliveryDetails, bool success)
        {
            FillStoresAndCartForPurchase(itemsToAddToCartWithoutGUID, amountToAddToCart, storesToGenerateAndBuyFrom);

            storeOperations.buyItemsFromDifferentUserAndThenChangeBackToCurrentUser(itemsToBeBoughtBetweenCartAddAndPurchaseWithoutGUID,
                amountToBeBoughtBetweenCartAddAndPurchase, paymentDetails, deliveryDetails);

            Assert.AreEqual(success, storeOperations.buyCartWithValidation(paymentDetails, deliveryDetails));
        }

        public static object[] StoresWithItems_OpenedDiscount =
        {
            new object[] {new Store[] {new Store(new Item[] { bannana, iphone, galaxy }, new int[] { 1, 1, 1 }) },/*amount*/ new int[][]{ new int [] {1,1,1}},/*percent*/ new double [][] { new double [] {0.2, 0.2, 0.3} }, /*duration*/ new int [][] { new int [] {30,30, 30} } },

            new object[] {new Store[] {new Store(new Item[] { bannana, galaxy }, new int[] { 1, 1 }),
                                       new Store(new Item [] {iphone }, new int [] {2 })},
                         /*amount*/ new int[][]{ new int [] {1,1,1}, new int[] { 2 } }, /*percent*/ new double [][] { new double [] {0.2, 0.2}, new double[] {0.5} },/*duration*/ new int [][] { new int [] {30,30}, new int[] { 20}} },

            new object[] {new Store[] {new Store(new Item[] { bannana, galaxy }, new int[] { 1, 1 }),
                                       new Store(new Item [] {bannana }, new int [] {2 })},
                         /*amount*/ new int[][]{ new int [] {1,1,1}, new int[] { 2 } }, /*percent*/ new double [][] { new double [] {0.2, 0.2}, new double[] {0.5} },/*duration*/ new int [][] { new int [] {30,30}, new int[] { 20}} },
        };


        [Test, TestCaseSource("StoresWithItems_OpenedDiscount")]
        public void UC_2_8_buyProductImmediate_WithOpenedDiscount_successScenarios(Store[] stores, int[][] amountForEachItemToPurchase, double[][] percents, int[][] durations)
        {
            List<Store> generatedStoresWithItems = new List<Store>();
            List<List<Item>> allItemsFromStoresWithGUID = new List<List<Item>>();

            GenerateStoresWithItemsForPurchaseWithDiscountTests(stores, generatedStoresWithItems, allItemsFromStoresWithGUID);

            Dictionary<Guid, double> store_expectedSum = CreateOpenedDiscount_AddToCart_AndReturnExpectedStoreOrderSums(amountForEachItemToPurchase, percents, durations, generatedStoresWithItems, allItemsFromStoresWithGUID);

            BuyCartAndConfrimPricesOfEachStoreOrder(store_expectedSum);

        }



        public static object[] StoresWithItems_ItemConditionalDiscountOnAll =
        {
            new object[] {new Store[] {new Store(new Item[] { bannana, iphone, galaxy }, new int[] { 5, 5, 5 }) },/*amount*/ new int[][]{ new int [] { 3, 3, 3}},/*percent*/ new double [][] { new double [] {0.2, 0.2, 0.3} }, /*duration*/ new int [][] { new int [] {30,30, 30} }, /*minItems*/ new int[][] { new int []{2,2,4} } },

            new object[] {new Store[] {new Store(new Item[] { bannana, galaxy }, new int[] { 1, 1 }),
                                       new Store(new Item [] {iphone }, new int [] {2 })},
                         /*amount*/ new int[][]{ new int [] {1,1,1}, new int[] { 2 } }, /*percent*/ new double [][] { new double [] {0.2, 0.2}, new double[] {0.5} },/*duration*/ new int [][] { new int [] {30,30}, new int[] { 20}} , /*minItems*/ new int[][] { new int []{2,2}, new int [] { 2 } } },

            //will not get discount
            new object[] {new Store[] {new Store(new Item[] { bannana, galaxy }, new int[] { 1, 1 }),
                                       new Store(new Item [] {bannana }, new int [] {2 })},
                         /*amount*/ new int[][]{ new int [] {1,1}, new int[] { 2 } }, /*percent*/ new double [][] { new double [] {0.2, 0.2}, new double[] {0.5} },/*duration*/ new int [][] { new int [] {30,30}, new int[] { 20}} , /*miItems*/ new int [][]{ new int[] { 3, 3}, new int[] { 3} } },
        };

        [Test, TestCaseSource("StoresWithItems_ItemConditionalDiscountOnAll")]
        public void UC_2_8_buyProductImmediate_WithItemConditionalDiscountOnAll_successScenarios(Store[] stores, int[][] amountForEachItemToPurchase, double[][] percents, int[][] durations, int[][] minItems)
        {
            List<Store> generatedStoresWithItems = new List<Store>();
            List<List<Item>> allItemsFromStoresWithGUID = new List<List<Item>>();

            GenerateStoresWithItemsForPurchaseWithDiscountTests(stores, generatedStoresWithItems, allItemsFromStoresWithGUID);

            Dictionary<Guid, double> store_expectedSum = CreateConditionalDiscountOnAll_AddToCart_AndReturnExpectedStoreOrderSums(amountForEachItemToPurchase, percents, durations, minItems, generatedStoresWithItems, allItemsFromStoresWithGUID);

            BuyCartAndConfrimPricesOfEachStoreOrder(store_expectedSum);
        }

        public static object[] StoresAndItems_CompositeDiscount_ItemConditionalDiscountOnAllOrOpenedDiscount =
        {
            new object[] {new Store[] {new Store(new Item[] { bannana, iphone }, new int[] { 5, 5}) },/*amount*/ new int[][]{ new int [] { 3, 2}},/*percent*/ new double [][] { new double [] {0.2, 0.05} }, /*duration*/ new int [][] { new int [] {30,15} }, /*minItems*/ new int[]{2} },
        };

        [Test, TestCaseSource("StoresAndItems_CompositeDiscount_ItemConditionalDiscountOnAllOrOpenedDiscount")]
        public void UC_2_8_buyProductImmediate_WithCompositeDiscount_ItemConditionalDiscountOnAllOrOpenedDiscount_successScenarios(Store[] stores, int[][] amountForEachItemToPurchase, double[][] percents, int[][] durations, int[] minItems)
        {
            List<Store> generatedStoresWithItems = new List<Store>();
            List<List<Item>> allItemsFromStoresWithGUID = new List<List<Item>>();

            GenerateStoresWithItemsForPurchaseWithDiscountTests(stores, generatedStoresWithItems, allItemsFromStoresWithGUID);

            ItemConditionalDiscountOnAll[] discountLeft = storeOperations.GeneratedConditionAllDiscountOnItemsInStoreWithoutLogin(generatedStoresWithItems[0].Id, new Item[] { allItemsFromStoresWithGUID[0][0] }, new double[] { percents[0][0] }, new int[] { durations[0][0] }, minItems);
            ItemOpenedDiscount[] discountRight = storeOperations.GeneratedOpenedDiscountOnItemsInStoreWithoutLogin(generatedStoresWithItems[0].Id, new Item[] { allItemsFromStoresWithGUID[0][1] }, new double[] { percents[0][1] }, new int[] { durations[0][1] });

            CompositeTwoDiscounts composite = storeOperations.CompositeDiscountWithouLogin(generatedStoresWithItems[0].Id, discountLeft[0].DiscountID, discountRight[0].DiscountID, "|");

            storeOperations.addItemToCartFromStoreWithAmount(allItemsFromStoresWithGUID[0][0], generatedStoresWithItems[0], amountForEachItemToPurchase[0][0]);
            storeOperations.addItemToCartFromStoreWithAmount(allItemsFromStoresWithGUID[0][1], generatedStoresWithItems[0], amountForEachItemToPurchase[0][1]);

            Dictionary<Guid, double> store_expectedSum = new Dictionary<Guid, double>();
            if (amountForEachItemToPurchase[0][0] < minItems[0])
            {
                store_expectedSum.Add(generatedStoresWithItems[0].Id, allItemsFromStoresWithGUID[0][1].Price * amountForEachItemToPurchase[0][1] * (1 - percents[0][1]));
            }
            else
            {
                store_expectedSum.Add(generatedStoresWithItems[0].Id, allItemsFromStoresWithGUID[0][1].Price * amountForEachItemToPurchase[0][1] * (1 - percents[0][1]) +
                                                                       allItemsFromStoresWithGUID[0][0].Price * amountForEachItemToPurchase[0][0] * (1 - percents[0][0]));
            }


            BuyCartAndConfrimPricesOfEachStoreOrder(store_expectedSum);
        }


        public static object[] StoresAndItems_CompositeDiscount_OpenedDiscountAndStoreConditionalDiscount =
        {
            new object[] {new Store(new Item[] { bannana, iphone }, new int[] { 5, 5}) ,/*amount*/ new int[]{3, 2},/*percent*/new double[] {/*item opened discount */ 0.4, /*store conditional discount*/ 0.1 }, /*duration*/ new int []{30,15 }, /*minPurchase*/20.5 },
        };


        [Test, TestCaseSource("StoresAndItems_CompositeDiscount_OpenedDiscountAndStoreConditionalDiscount")]
        public void UC_2_8_buyProductImmediate_WithCompositeDiscount_OpenedDiscountAndStoreConditionalDiscount_SuccessAndFailureScenarios(Store store, int[] amountForEachItemToPurchase, double[] percent, int[] duration, double minPurchase)
        {
            Store generatedStoreWithItems = storeOperations.generateStoreWithItemsAndAmountsWithoutLogin(store);
            List<Item> itemsFromStoresWithGUID = getAllItemsFromStores(new List<Store> { generatedStoreWithItems });

            ItemOpenedDiscount itemOpenedDiscount = storeOperations.GeneratedOpenedDiscountOnItemsInStoreWithoutLogin(generatedStoreWithItems.Id, new Item[] { itemsFromStoresWithGUID[0] }, new double[] { percent[0] }, new int[] { duration[0] })[0];
            StoreConditionalDiscount storeDiscount = storeOperations.GenerateStoreConditionalDiscountWithoutLogin(generatedStoreWithItems.Id, minPurchase, percent[1], duration[1]);

            CompositeTwoDiscounts composite = storeOperations.CompositeDiscountWithouLogin(generatedStoreWithItems.Id, itemOpenedDiscount.DiscountID, storeDiscount.DiscountID, "|");

            storeOperations.addItemToCartFromStoreWithAmount(itemsFromStoresWithGUID[0], generatedStoreWithItems, amountForEachItemToPurchase[0]);
            storeOperations.addItemToCartFromStoreWithAmount(itemsFromStoresWithGUID[1], generatedStoreWithItems, amountForEachItemToPurchase[1]);

            Dictionary<Guid, double> store_expectedSum = new Dictionary<Guid, double>();

            double sumWithoutStoreConditionalDiscount = itemsFromStoresWithGUID[0].Price * (1 - itemOpenedDiscount.Discount) * amountForEachItemToPurchase[0] + itemsFromStoresWithGUID[1].Price * amountForEachItemToPurchase[1];

            if (sumWithoutStoreConditionalDiscount >= minPurchase)
            {
                store_expectedSum.Add(generatedStoreWithItems.Id, sumWithoutStoreConditionalDiscount * (1 - storeDiscount.Discount));
            }
            else
            {
                store_expectedSum.Add(generatedStoreWithItems.Id, sumWithoutStoreConditionalDiscount);
            }

            BuyCartAndConfrimPricesOfEachStoreOrder(store_expectedSum);
        }

        public static object[] StoresWithItems_ItemMinMaxPolicy =
       {
            //success min max
            new object[] {new Store[] {new Store(new Item[] { bannana }, new int[] { 20 }) },/*amount*/ new int[][]{ new int [] {5}},/*min*/ new int? [][] { new int? [] {4} }, /*max*/ new int? [][] { new int? [] {6} }, true },

            //success min max
            new object[] {new Store[] {new Store(new Item[] { bannana}, new int[] { 20 }) },/*amount*/ new int[][]{ new int [] {5}},/*min*/ new int? [][] { new int? [] { 5} }, /*max*/ new int? [][] { new int? [] {5} }, true },

            //success min max
            new object[] {new Store[] {new Store(new Item[] { bannana }, new int[] { 20 }) },/*amount*/ new int[][]{ new int [] {5}},/*min*/ new int? [][] { new int? [] { 3} }, /*max*/ new int? [][] { new int? [] { 30} }, true },

            //success min only
            new object[] {new Store[] {new Store(new Item[] {  galaxy }, new int[] { 20 }) },/*amount*/ new int[][]{ new int [] {5}},/*min*/ new int? [][] { new int? [] { 3} }, /*max*/ new int? [][] { new int? [] { null} }, true },

             //success min only
            new object[] {new Store[] {new Store(new Item[] {  galaxy }, new int[] { 20 }) },/*amount*/ new int[][]{ new int [] {5}},/*min*/ new int? [][] { new int? [] { 5} }, /*max*/ new int? [][] { new int? [] { null} }, true },

            //success max only
            new object[] {new Store[] {new Store(new Item[] {  galaxy }, new int[] {  20 }) },/*amount*/ new int[][]{ new int [] {5}},/*min*/ new int? [][] { new int? [] { null} }, /*max*/ new int? [][] { new int?[] { 6 } }, true },

             //success max only
            new object[] {new Store[] {new Store(new Item[] {  galaxy }, new int[] {  20 }) },/*amount*/ new int[][]{ new int [] {5}},/*min*/ new int? [][] { new int? [] { null} }, /*max*/ new int? [][] { new int?[] { 5 } }, true },


            //fail min max
            new object[] {new Store[] {new Store(new Item[] { bannana, iphone, galaxy }, new int[] { 20, 20, 20 }) },/*amount*/ new int[][]{ new int [] {5,5,5}},/*min*/ new int? [][] { new int? [] {6, 3, 1} }, /*max*/ new int? [][] { new int? [] {7,4, 4} }, false },

            //fail min only
            new object[] {new Store[] {new Store(new Item[] { bannana, iphone, galaxy }, new int[] { 20, 20, 20 }) },/*amount*/ new int[][]{ new int [] {5,5,5}},/*min*/ new int? [][] { new int? [] {6, 7, 8} }, /*max*/ new int? [][] { new int? [] {null, null, null } }, false },

            //fail only max
            new object[] {new Store[] {new Store(new Item[] { bannana, iphone, galaxy }, new int[] { 20, 20, 20 }) },/*amount*/ new int[][]{ new int [] {5,5,5}},/*min*/ new int? [][] { new int? [] {null, null, null } }, /*max*/ new int? [][] { new int? [] {4,3, 2} }, false },

           
        };


        [Test, TestCaseSource("StoresWithItems_ItemMinMaxPolicy")]
        public void UC_2_8_buyProductImmediate_WithItemMinMaxPolicy_Scenarios(Store[] stores, int[][] amountForEachItemToPurchase, int?[][] min, int?[][] max, bool success)
        {
            List<Store> generatedStoresWithItems = new List<Store>();
            List<List<Item>> allItemsFromStoresWithGUID = new List<List<Item>>();

            GenerateStoresWithItemsForPurchaseWithDiscountTests(stores, generatedStoresWithItems, allItemsFromStoresWithGUID);

            CreateItemMinMaxPolicies_AddToCart(amountForEachItemToPurchase, min, max, generatedStoresWithItems, allItemsFromStoresWithGUID);

            Assert.AreEqual(success, storeOperations.checkoutAndReuturnIfSucceed());

        }

        private void CreateItemMinMaxPolicies_AddToCart(int[][] amountForEachItemToPurchase, int?[][] min, int?[][] max, List<Store> generatedStoresWithItems, List<List<Item>> allItemsFromStoresWithGUID)
        {
            for (int i = 0; i < generatedStoresWithItems.Count; i++)
            {
                storeOperations.GenerateItemMinMaxPolicyInStoreWithoutLogin(generatedStoresWithItems[i].Id, allItemsFromStoresWithGUID[i].ToArray(), min[i], max[i]);

                for (int j = 0; j < allItemsFromStoresWithGUID[i].Count; j++)
                {
                    storeOperations.addItemToCartFromStoreWithAmount(allItemsFromStoresWithGUID[i][j], generatedStoresWithItems[i], amountForEachItemToPurchase[i][j]);
                }
            }
        }

        public static object[] StoresWithItems_StoreMinMaxPolicy =
      {
            //success min max
            new object[] {new Store[] {new Store(new Item[] { bannana, galaxy }, new int[] { 20, 20 })},
                /*amount*/ new int[][]{ new int [] {5, 10}},
                /*min*/ new int? [] { 15 },
                /*max*/ new int? [] { 15 },
                /*success*/ true },
            //success min max
            new object[] {new Store[] {new Store(new Item[] { bannana, galaxy }, new int[] { 20, 20 })},
                /*amount*/ new int[][]{ new int [] {5, 10}},
                /*min*/ new int? [] { 14 },
                /*max*/ new int? [] { 16 },
                /*success*/ true },

            //success min only
            new object[] {new Store[] {new Store(new Item[] { bannana, galaxy }, new int[] { 20, 20 })},
                /*amount*/ new int[][]{ new int [] {5, 10}},
                /*min*/ new int? [] { 15 },
                /*max*/ new int? [] { null },
                /*success*/ true },

            //success min only
            new object[] {new Store[] {new Store(new Item[] { bannana, galaxy }, new int[] { 20, 20 })},
                /*amount*/ new int[][]{ new int [] {5, 10}},
                /*min*/ new int? [] { 14 },
                /*max*/ new int? [] { null },
                /*success*/ true },

            //success  max only
            new object[] {new Store[] {new Store(new Item[] { bannana, galaxy }, new int[] { 20, 20 })},
                /*amount*/ new int[][]{ new int [] {5, 10}},
                /*min*/ new int? [] { null },
                /*max*/ new int? [] { 15 },
                /*success*/ true },
            //success  max only
            new object[] {new Store[] {new Store(new Item[] { bannana, galaxy }, new int[] { 20, 20 })},
                /*amount*/ new int[][]{ new int [] {5, 10}},
                /*min*/ new int? [] { null },
                /*max*/ new int? [] { 16 },
                /*success*/ true },

             //fail min max
            new object[] {new Store[] {new Store(new Item[] { bannana, galaxy }, new int[] { 20, 20 }),
                          new Store(new Item[] { bannana, iphone }, new int[] { 20, 20 })},
                /*amount*/ new int[][]{ new int [] {5, 10}, new int [] {5, 10}},
                /*min*/ new int? [] { 16 , 13  },
                /*max*/ new int? [] { 17, 14 },
                /*success*/ false },

             //fail min only
             new object[] {new Store[] {new Store(new Item[] { bannana, galaxy }, new int[] { 20, 20 })},
                /*amount*/ new int[][]{ new int [] {5, 10}},
                /*min*/ new int? [] { 16 },
                /*max*/ new int? [] { null },
                /*success*/ false },

             //fail max only
             new object[] {new Store[] {new Store(new Item[] { bannana, galaxy }, new int[] { 20, 20 })},
                /*amount*/ new int[][]{ new int [] {5, 10}},
                /*min*/ new int? [] { null },
                /*max*/ new int? [] { 14 },
                /*success*/ false },



        };


        [Test, TestCaseSource("StoresWithItems_StoreMinMaxPolicy")]
        public void UC_2_8_buyProductImmediate_WithStoreMinMaxPolicy_Scenarios(Store[] stores, int[][] amountForEachItemToPurchase, int?[] min, int?[] max, bool success)
        {
            List<Store> generatedStoresWithItems = new List<Store>();
            List<List<Item>> allItemsFromStoresWithGUID = new List<List<Item>>();

            GenerateStoresWithItemsForPurchaseWithDiscountTests(stores, generatedStoresWithItems, allItemsFromStoresWithGUID);

            CreateStoreMinMaxPolicies_AddToCart(amountForEachItemToPurchase, min, max, generatedStoresWithItems, allItemsFromStoresWithGUID);

            Assert.AreEqual(success, storeOperations.checkoutAndReuturnIfSucceed());

        }

        private void CreateStoreMinMaxPolicies_AddToCart(int[][] amountForEachItemToPurchase, int?[] min, int?[] max, List<Store> generatedStoresWithItems, List<List<Item>> allItemsFromStoresWithGUID)
        {
            for (int i = 0; i < generatedStoresWithItems.Count; i++)
            {
                storeOperations.GenerateStoreMinMaxPolicyInStoreWithoutLogin(generatedStoresWithItems[i].Id, min[i], max[i]);

                for (int j = 0; j < allItemsFromStoresWithGUID[i].Count; j++)
                {
                    storeOperations.addItemToCartFromStoreWithAmount(allItemsFromStoresWithGUID[i][j], generatedStoresWithItems[i], amountForEachItemToPurchase[i][j]);
                }
            }
        }

        private void generateStoreForPolicyTests(List<Store> generatedStoresWithItems, List<List<Item>> allItemsFromStoresWithGUID)
        {
            Store[] stores = new Store[] { new Store(new Item[] { bannana, galaxy, iphone }, new int[] { 100, 100, 100 }) };

            GenerateStoresWithItemsForPurchaseWithDiscountTests(stores, generatedStoresWithItems, allItemsFromStoresWithGUID);

        }

        [Test]
        public void UC_2_8_buyProductImmedate_withDaysNotAllowedPolicy_successAndFail()
        {

            List<Store> generatedStoresWithItems = new List<Store>();
            List<List<Item>> allItemsFromStoresWithGUID = new List<List<Item>>();

            generateStoreForPolicyTests(generatedStoresWithItems, allItemsFromStoresWithGUID);
            DayOfWeek today = DateTime.Now.DayOfWeek;
            int todayDay = (int)today + 1 ;
            int tommorow = todayDay + 1 == 8 ? 1 : todayDay + 1;
            int yesterday = todayDay -1 == 0 ? 7 : todayDay -1 ;
            Guid policyID = storeOperations.CreateDaysNotAllowedPolicyWithoutLogin(generatedStoresWithItems[0].Id, new int[] { todayDay });
            Assert.True(storeOperations.addItemToCartFromStoreWithAmount(allItemsFromStoresWithGUID[0][0], generatedStoresWithItems[0], 5));
            Assert.False(storeOperations.checkoutAndReuturnIfSucceed());
            Assert.True(storeOperations.removePolicySuccessWithoutLogin(generatedStoresWithItems[0].Id, policyID));

            policyID = storeOperations.CreateDaysNotAllowedPolicyWithoutLogin(generatedStoresWithItems[0].Id, new int[] { yesterday, tommorow });
        
            Assert.True(storeOperations.checkoutAndReuturnIfSucceed());

        }

        private Guid createComplexTwoItemMinMax(List<Store> generatedStoresWithItems, List<List<Item>> allItemsFromStoresWithGUID, string op)
        {
            Guid policyLeft = storeOperations.CreateItemMinMaxPolicyWithoutLogin(generatedStoresWithItems[0].Id, allItemsFromStoresWithGUID[0][0], 4, 10);
            Guid policyRight = storeOperations.CreateItemMinMaxPolicyWithoutLogin(generatedStoresWithItems[0].Id, allItemsFromStoresWithGUID[0][1], 6, 7);

            return  storeOperations.CreateCompositePolicyWithoutLogin(generatedStoresWithItems[0].Id, policyLeft, policyRight, op);
        }

        [Test]
        public void UC_2_8_buyProductImmedate_withComplexPolicy_2_itemMinMax_AND_successAndFail()
        {

            List<Store> generatedStoresWithItems = new List<Store>();
            List<List<Item>> allItemsFromStoresWithGUID = new List<List<Item>>();

            generateStoreForPolicyTests(generatedStoresWithItems, allItemsFromStoresWithGUID);

            createComplexTwoItemMinMax(generatedStoresWithItems,  allItemsFromStoresWithGUID, "&");


            Assert.True(storeOperations.addItemToCartFromStoreWithAmount(allItemsFromStoresWithGUID[0][0], generatedStoresWithItems[0], 5));
            Assert.True(storeOperations.addItemToCartFromStoreWithAmount(allItemsFromStoresWithGUID[0][1], generatedStoresWithItems[0], 6));

            Assert.True(storeOperations.checkoutAndReuturnIfSucceed());
            Assert.True(storeOperations.updateItemAmountInCartAndValidateUpdated(generatedStoresWithItems[0].Id, allItemsFromStoresWithGUID[0][0], 3));

            Assert.False(storeOperations.checkoutAndReuturnIfSucceed());

        }

        [Test]
        public void UC_2_8_buyProductImmedate_withComplexPolicy_2_itemMinMax_OR_successAndFail()
        {

            List<Store> generatedStoresWithItems = new List<Store>();
            List<List<Item>> allItemsFromStoresWithGUID = new List<List<Item>>();

            generateStoreForPolicyTests(generatedStoresWithItems, allItemsFromStoresWithGUID);

            createComplexTwoItemMinMax(generatedStoresWithItems, allItemsFromStoresWithGUID, "|");


            Assert.True(storeOperations.addItemToCartFromStoreWithAmount(allItemsFromStoresWithGUID[0][0], generatedStoresWithItems[0], 5));
            Assert.True(storeOperations.addItemToCartFromStoreWithAmount(allItemsFromStoresWithGUID[0][1], generatedStoresWithItems[0], 6));

            Assert.True(storeOperations.checkoutAndReuturnIfSucceed());
            Assert.True(storeOperations.updateItemAmountInCartAndValidateUpdated(generatedStoresWithItems[0].Id, allItemsFromStoresWithGUID[0][0], 3));

            Assert.True(storeOperations.checkoutAndReuturnIfSucceed());

            Assert.True(storeOperations.updateItemAmountInCartAndValidateUpdated(generatedStoresWithItems[0].Id, allItemsFromStoresWithGUID[0][1], 15));

            Assert.False(storeOperations.checkoutAndReuturnIfSucceed());

        }

        [Test]
        public void UC_2_8_buyProductImmedate_withComplexPolicy_2_itemMinMax_XOR_successAndFail()
        {

            List<Store> generatedStoresWithItems = new List<Store>();
            List<List<Item>> allItemsFromStoresWithGUID = new List<List<Item>>();

            generateStoreForPolicyTests(generatedStoresWithItems, allItemsFromStoresWithGUID);

            createComplexTwoItemMinMax(generatedStoresWithItems, allItemsFromStoresWithGUID, "xor");


            Assert.True(storeOperations.addItemToCartFromStoreWithAmount(allItemsFromStoresWithGUID[0][0], generatedStoresWithItems[0], 5));
            Assert.True(storeOperations.addItemToCartFromStoreWithAmount(allItemsFromStoresWithGUID[0][1], generatedStoresWithItems[0], 6));

            Assert.False(storeOperations.checkoutAndReuturnIfSucceed());
            Assert.True(storeOperations.updateItemAmountInCartAndValidateUpdated(generatedStoresWithItems[0].Id, allItemsFromStoresWithGUID[0][0], 3));

            Assert.True(storeOperations.checkoutAndReuturnIfSucceed());

            Assert.True(storeOperations.updateItemAmountInCartAndValidateUpdated(generatedStoresWithItems[0].Id, allItemsFromStoresWithGUID[0][1], 15));

            Assert.False(storeOperations.checkoutAndReuturnIfSucceed());

        }

        [Test]
        public void UC_2_8_buyProductImmedate_withVeryComplexPolic_success()
        {

            List<Store> generatedStoresWithItems = new List<Store>();
            List<List<Item>> allItemsFromStoresWithGUID = new List<List<Item>>();

            generateStoreForPolicyTests(generatedStoresWithItems, allItemsFromStoresWithGUID);

            Guid complex1  = createComplexTwoItemMinMax(generatedStoresWithItems, allItemsFromStoresWithGUID, "&");

            Guid storePolicy = storeOperations.AddStoreMinMaxPolicyWithoutLogin(generatedStoresWithItems[0].Id, 15, 30);
            DayOfWeek today = DateTime.Now.DayOfWeek;
            int todayDay = (int)today + 1;
            Guid DaysPolicyID = storeOperations.CreateDaysNotAllowedPolicyWithoutLogin(generatedStoresWithItems[0].Id, new int[] { todayDay });
            Guid complex2 = storeOperations.CreateCompositePolicyWithoutLogin(generatedStoresWithItems[0].Id, storePolicy, DaysPolicyID, "xor");
            Guid complexFinal = storeOperations.CreateCompositePolicyWithoutLogin(generatedStoresWithItems[0].Id, complex1, complex2, "&");


            Assert.True(storeOperations.addItemToCartFromStoreWithAmount(allItemsFromStoresWithGUID[0][0], generatedStoresWithItems[0], 5));
            Assert.True(storeOperations.addItemToCartFromStoreWithAmount(allItemsFromStoresWithGUID[0][1], generatedStoresWithItems[0], 6));
            Assert.True(storeOperations.addItemToCartFromStoreWithAmount(allItemsFromStoresWithGUID[0][2], generatedStoresWithItems[0], 9));

            Assert.True(storeOperations.checkoutAndReuturnIfSucceed());

            Assert.True(storeOperations.updateItemAmountInCartAndValidateUpdated(generatedStoresWithItems[0].Id, allItemsFromStoresWithGUID[0][2], 1));
            Assert.False(storeOperations.checkoutAndReuturnIfSucceed());

            Assert.True(storeOperations.updateItemAmountInCartAndValidateUpdated(generatedStoresWithItems[0].Id, allItemsFromStoresWithGUID[0][2], 14));
            Assert.True(storeOperations.updateItemAmountInCartAndValidateUpdated(generatedStoresWithItems[0].Id, allItemsFromStoresWithGUID[0][1], 1));

            Assert.False(storeOperations.checkoutAndReuturnIfSucceed());

        }



        public static object[] buyProductsForPaymentSystemFailCases =
         { //cart number wrong
            new object[]
                        { new Item[]{bannana },new int[]{3}, new Store[] {new Store(new Item[] { bannana, iphone, galaxy }, new int[] { 4, 1, 1 }) },
                          new string[]{"4444-2222-1111","03/30","333","yossi","212212212" }, new string [] { "Borer 20", "Tel Aviv", "Israel", "20693"} },
            //expiration month wrong
            new object[]
                        { new Item[]{bannana },new int[]{3}, new Store[] {new Store(new Item[] { bannana, iphone, galaxy }, new int[] { 4, 1, 1 }) },
                          new string[]{"4444-3333-2222-1111","14/30","333","yossi","212212212" }, new string [] { "Borer 20", "Tel Aviv", "Israel", "20693"} },

            //wrong CVV
            new object[]
                        { new Item[]{bannana },new int[]{3}, new Store[] {new Store(new Item[] { bannana, iphone, galaxy }, new int[] { 4, 1, 1 }) },
                          new string[]{"4444-3333-2222-1111","14/30","-77","yossi","212212212" }, new string [] { "Borer 20", "Tel Aviv", "Israel", "20693"} },
        };

        public static object[] buyProductsForDeliverySystemFailCases =
        {    //address problem - no such city
            new object[]
                        { new Item[]{bannana },new int[]{3},  new Store[] {new Store(new Item[] { bannana, iphone, galaxy }, new int[] { 4, 1, 1 }) },
                          new string[]{"4444-3333-2222-1111","03/30","333","yossi","212212212" }, new string [] { "Borer 20", "New York", "Israel", "20693"} },
            //adress problem - zip code wrong
            new object[]
                        { new Item[]{bannana },new int[]{3},  new Store[] {new Store(new Item[] { bannana, iphone, galaxy }, new int[] { 4, 1, 1 }) },
                         new string[]{"4444-3333-2222-1111","03/30","333","yossi","212212212" }, new string [] { "Borer 20", "Tel Aviv", "Israel", "20693"} }

        };

        public static object[] buyProductsForPaymentSystemConnectionFailedCases =
        { //cart number wrong
            new object[]
                        { new Item[]{bannana },new int[]{3}, new Store[] {new Store(new Item[] { bannana, iphone, galaxy }, new int[] { 4, 1, 1 }) },
                          new string[]{"4444-2222-1111","03/30","333","yossi","212212212" }, new string [] { "Borer 20", "Tel Aviv", "Israel", "20693"} },
        };


        [Test, TestCaseSource("buyProductsForPaymentSystemFailCases")]
        public virtual void UC_2_8_buyProductImmediate_PaymentFailedOnCreditCard(Item[] itemsToAddToCartWithoutGUID, int[] amountToAddToCart,
            Store[] storesToGenerateAndBuyFrom, string[] paymentDetails, string[] deliveryDetails)
        {
            FillStoresAndCartForPurchase(itemsToAddToCartWithoutGUID, amountToAddToCart, storesToGenerateAndBuyFrom);

            string error = storeOperations.buyCartShouldFailOnExternalSystem(paymentDetails, deliveryDetails);

            Assert.AreEqual("Failed: credit card", error);

        }

        [Test, TestCaseSource("buyProductsForPaymentSystemConnectionFailedCases")]
        public virtual void UC_2_8_buyProductImmediate_ConnectionToSystemFailed(Item[] itemsToAddToCartWithoutGUID, int[] amountToAddToCart,
            Store[] storesToGenerateAndBuyFrom, string[] paymentDetails, string[] deliveryDetails)
        {
            FillStoresAndCartForPurchase(itemsToAddToCartWithoutGUID, amountToAddToCart, storesToGenerateAndBuyFrom);

            string error = storeOperations.buyCartShouldFailOnExternalSystem(paymentDetails, deliveryDetails);

            Assert.IsTrue(error.Contains("No Connection To external"));

        }

        [Test, TestCaseSource("buyProductsForDeliverySystemFailCases")]
        public virtual void UC_2_8_buyProductImmediate_DeliveryFailed(Item[] itemsToAddToCartWithoutGUID, int[] amountToAddToCart,
            Store[] storesToGenerateAndBuyFrom, string[] paymentDetails, string[] deliveryDetails)
        {
            FillStoresAndCartForPurchase(itemsToAddToCartWithoutGUID, amountToAddToCart, storesToGenerateAndBuyFrom);

            string error = storeOperations.buyCartShouldFailOnExternalSystem(paymentDetails, deliveryDetails);

            Assert.IsTrue(error.Contains("Failed: "));

        }

        [Test]
        public virtual void UC_3_1_logout_shouldFailONGuestPassONRegistered()
        {
            Assert.False(storeOperations.logout());
        }

        public static StoreContactDetails validStoreContact = new StoreContactDetails("valid", "valid", "valid", "valid", "valid", "valid", "valid");
        public static object[] validStoreOpenDetails =
            {
                new object[] {validStoreContact, false}
            };

        [Test,TestCaseSource("validStoreOpenDetails")]
        public virtual void UC_3_2_openingAStore_SuccessAndFailureScenarios(StoreContactDetails contactDetails, bool success)
        {
            Assert.False(storeOperations.openStore(contactDetails));

        }

        [Test]
        public virtual void UC_4_2_AddOpenedDiscount_NoPermmision_ShouldFail()
        {
            List<Store> storesWithItems = storeOperations.generateRandomListOfStoresWithItems();
            Store s = storesWithItems[0];
            Dictionary<Item, int> items = s.ItemsWithAmount;
            Item item = null;
            foreach( KeyValuePair<Item, int> keyValue in items)
            {
                item = keyValue.Key;
                break;
            }

            string answer = storeOperations.AddOpenedDiscountError(s.Id, item.Id, 0.20, 30);

            Assert.AreEqual("not logged in", answer);
        }

        [Test]
        public virtual void UC_4_2_AddItemConditionalDiscountOnAll_NoPermmision_ShouldFail()
        {
            List<Store> storesWithItems = storeOperations.generateRandomListOfStoresWithItems();
            Store s = storesWithItems[0];
            Dictionary<Item, int> items = s.ItemsWithAmount;
            Item item = null;
            foreach (KeyValuePair<Item, int> keyValue in items)
            {
                item = keyValue.Key;
                break;
            }

            string answer = storeOperations.AddItemConditionalDiscount_MinItems_ToDiscountOnAllError(s.Id, item.Id, 30, 5, 0.20);

            Assert.AreEqual("not logged in", answer);
        }

        [Test]
        public virtual void UC_4_2_AddItemConditional_DiscountOnExtraItems_NoPermmision_ShouldFail()
        {
            List<Store> storesWithItems = storeOperations.generateRandomListOfStoresWithItems();
            Store s = storesWithItems[0];
            Dictionary<Item, int> items = s.ItemsWithAmount;
            Item item = null;
            foreach (KeyValuePair<Item, int> keyValue in items)
            {
                item = keyValue.Key;
                break;
            }

            string answer = storeOperations.AddItemConditionalDiscount_MinItems_ToDiscountOnExtraItemsError(s.Id, item.Id, 30, 3, 1, 0.5);

            Assert.AreEqual("not logged in", answer);
        }

        [Test]
        public virtual void UC_4_2_AddStoreConditionalDiscount_NoPermmision_ShouldFail()
        {
            List<Store> storesWithItems = storeOperations.generateRandomListOfStoresWithItems();
            Store s = storesWithItems[0];
            Dictionary<Item, int> items = s.ItemsWithAmount;
            Item item = null;
            foreach (KeyValuePair<Item, int> keyValue in items)
            {
                item = keyValue.Key;
                break;
            }

            string answer = storeOperations.AddStoreConditionalDiscountError(s.Id, 60, 270.50, 0.10);

            Assert.AreEqual("not logged in", answer);
        }

        [Test]
        public virtual void UC_4_2_RemoveDiscount_NoPermmision_ShouldFail()
        {
            Tuple<Store, ItemOpenedDiscount> tuple = storeOperations.GenerateStoreWithItemAndOpenedDiscountLogin(new Store(new Item[] { bannana, iphone, galaxy }, new int[] { 2, 1, 1 }));
            Assert.AreEqual("not logged in", storeOperations.RemoveDiscountError(tuple.Item1.Id, tuple.Item2.DiscountID));
        }

        [Test]
        public virtual void UC_4_2_MakeDiscountAllowed_NoPermission_FailureScenario()
        {

            List<Store> storesWithItems = storeOperations.generateRandomListOfStoresWithItems();
            Store s = storesWithItems[0];

            Assert.AreEqual("not logged in", storeOperations.MakeDiscountAllowedError(s.Id, "opened"));
        }

        [Test]
        public virtual void UC_4_2_MakeDiscountNotAllowed_NoPermission_FailureScenario()
        {
            List<Store> storesWithItems = storeOperations.generateRandomListOfStoresWithItems();
            Store s = storesWithItems[0];

            Assert.AreEqual("not logged in", storeOperations.MakeDiscountNotAllowedError(s.Id, "opened"));
        }

        [Test]
        public virtual void UC_4_2_GetAllowedDiscounts_NoPermission_FailureScenario()
        {
            List<Store> storesWithItems = storeOperations.generateRandomListOfStoresWithItems();
            Store s = storesWithItems[0];

            Assert.AreEqual("not logged in", storeOperations.GetAllowedDiscountsError(s.Id));
        }

        [Test]
        public virtual void UC_4_6_GetStoresWithPermissions_NoPermissions_FailureScenario()
        {

            string result = storeOperations.GetStoresWithPermissionsError();

            Assert.AreEqual("not logged in", result);
        }

        private void FillStoresAndCartForPurchase(Item[] itemsToAddToCartWithoutGUID, int[] amountToAddToCart, Store[] storesToGenerateAndBuyFrom)
        {
            //generateStores
            foreach (Store store in storesToGenerateAndBuyFrom)
            {
                storeOperations.generateStoreWithItemsAndAmountsWithoutLogin(store);
            }

            List<Store> storesWithItems = storeOperations.getAllStoresAndItems();
            List<Item> allItemsWithGUID = getAllItemsFromStores(storesWithItems);

            //populate cart
            for (int i = 0; i < itemsToAddToCartWithoutGUID.Length; i++)
            {
                Item itemWithGUID = getMatchingItemsWithoutConsideringGUID(new List<Item> { itemsToAddToCartWithoutGUID[i] }, allItemsWithGUID)[0];
                storeOperations.addItemToCartFromStoreWithAmount(itemWithGUID,
                    storesWithItems.Find(s => s.Id.Equals(itemWithGUID.storeID)),
                    amountToAddToCart[i]);
            }
        }

        internal void GenerateStoresWithItemsForPurchaseWithDiscountTests(Store[] stores, List<Store> generatedStoresWithItems, List<List<Item>> allItemsFromStoresWithGUID)
        {
            foreach (Store s in stores)
            {
                Store generatedStoreWithItems = storeOperations.generateStoreWithItemsAndAmountsWithoutLogin(s);
                generatedStoresWithItems.Add(generatedStoreWithItems);

                List<Item> itemsFromStoresWithGUID = getAllItemsFromStores(new List<Store> { generatedStoreWithItems });
                allItemsFromStoresWithGUID.Add(itemsFromStoresWithGUID);
            }
        }

        internal Dictionary<Guid, double> CreateConditionalDiscountOnAll_AddToCart_AndReturnExpectedStoreOrderSums(int[][] amountForEachItemToPurchase, double[][] percents, int[][] durations, int[][] minItems, List<Store> generatedStoresWithItems, List<List<Item>> allItemsFromStoresWithGUID)
        {
            Dictionary<Guid, double> store_expectedSum = new Dictionary<Guid, double>();

            for (int i = 0; i < generatedStoresWithItems.Count; i++)
            {
                ItemConditionalDiscountOnAll[] discounts = storeOperations.GeneratedConditionAllDiscountOnItemsInStoreWithoutLogin(generatedStoresWithItems[i].Id, allItemsFromStoresWithGUID[i].ToArray(), percents[i], durations[i], minItems[i]);

                double expectedStoreSum = 0;
                for (int j = 0; j < allItemsFromStoresWithGUID[i].Count; j++)
                {
                    storeOperations.addItemToCartFromStoreWithAmount(allItemsFromStoresWithGUID[i][j], generatedStoresWithItems[i], amountForEachItemToPurchase[i][j]);
                    if (amountForEachItemToPurchase[i][j] >= minItems[i][j])
                    {
                        expectedStoreSum += (allItemsFromStoresWithGUID[i][j].Price * amountForEachItemToPurchase[i][j] * (1 - discounts[j].Discount));
                    }
                    else
                    {
                        expectedStoreSum += allItemsFromStoresWithGUID[i][j].Price * amountForEachItemToPurchase[i][j];
                    }
                }
                store_expectedSum.Add(generatedStoresWithItems[i].Id, expectedStoreSum);
            }
            return store_expectedSum;
        }

        internal Dictionary<Guid, double> CreateOpenedDiscount_AddToCart_AndReturnExpectedStoreOrderSums(int[][] amountForEachItemToPurchase, double[][] percents, int[][] durations, List<Store> generatedStoresWithItems, List<List<Item>> allItemsFromStoresWithGUID)
        {
            Dictionary<Guid, double> store_expectedSum = new Dictionary<Guid, double>();

            for (int i = 0; i < generatedStoresWithItems.Count; i++)
            {
                ItemOpenedDiscount[] discounts = storeOperations.GeneratedOpenedDiscountOnItemsInStoreWithoutLogin(generatedStoresWithItems[i].Id, allItemsFromStoresWithGUID[i].ToArray(), percents[i], durations[i]);

                double expectedStoreSum = 0;
                for (int j = 0; j < allItemsFromStoresWithGUID[i].Count; j++)
                {
                    storeOperations.addItemToCartFromStoreWithAmount(allItemsFromStoresWithGUID[i][j], generatedStoresWithItems[i], amountForEachItemToPurchase[i][j]);
                    expectedStoreSum += allItemsFromStoresWithGUID[i][j].Price * (1 - discounts[j].Discount) * amountForEachItemToPurchase[i][j];
                }
                store_expectedSum.Add(generatedStoresWithItems[i].Id, expectedStoreSum);
            }
            return store_expectedSum;
        }

        internal void BuyCartAndConfrimPricesOfEachStoreOrder(Dictionary<Guid, double> store_expectedSum)
        {
            string[][] paymentAndDeliveryDetails = Utilitys.Utils.PaymentAndDeliveryDetailsValidExample();
            List<StoreOrder> storeOrders = storeOperations.BuyCartAsListOfStoreOrders(paymentAndDeliveryDetails[0], paymentAndDeliveryDetails[1]);

            for (int i = 0; i < storeOrders.Count; i++)
            {
                Assert.AreEqual(String.Format("{0:0.00}", store_expectedSum[storeOrders[i].StoreId]), String.Format("{0:0.00}", storeOrders[i].Sum));
                //Assert.AreEqual(store_expectedSum[storeOrders[i].StoreId], storeOrders[i].Sum);

            }
        }


    }
}

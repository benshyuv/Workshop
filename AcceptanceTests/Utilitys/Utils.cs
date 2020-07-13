using System;
using System.Collections.Generic;
using System.Linq;
using AcceptanceTests.DataObjects;

namespace AcceptanceTests.Utilitys
{
    public class Utils
    {
        private static Random random = new Random();
        public static string RandomAlphaNumericString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return RandomString(chars, length);
        }

        public static string RandomAlphabeticOnlyString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            return RandomString(chars, length);
        }

        public static string RandomMailString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            string s = RandomString(chars, length);
            return s + "@mail.com";
        }

        public static string RandomPhoneString()
        {

            string s = RandomNumberString(7);
            return "052" + s;
        }

        public static string RandomNumberString(int length)
        {
            const string chars = "0123456789";
            return RandomString(chars, length);
        }

        private static string RandomString(string chars, int length)
        {
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        public static Item RandomItem()
        {
            return new Item(RandomAlphabeticOnlyString(5), new List<string> { RandomAlphabeticOnlyString(5) },
                new List<string> { RandomAlphabeticOnlyString(5) });
        }

        public static StoreContactDetails RandomContactDetails()
        {
            return RandomContactDetailsWithName(Utilitys.Utils.RandomAlphaNumericString(8));
        }

        internal static StoreContactDetails RandomContactDetailsWithName(string name)
        {
            return new StoreContactDetails(name,
                RandomMailString(8), RandomAlphaNumericString(8),
                RandomPhoneString(), RandomAlphabeticOnlyString(8),
                RandomNumberString(7), RandomAlphabeticOnlyString(8));
        }

        public static Store RandomStoreWithItemsAndAmount()
        {
            int numOfItems = random.Next(3, 8);
            Item[] items = new Item[numOfItems];
            int[] amounts = new int[numOfItems];
            for (int i = 0; i < numOfItems; i++)
            {
                items[i] = RandomItem();
                amounts[i] = random.Next(10, 20);
            }
            return new Store(items, amounts);
            
        }

        public static string[][] PaymentAndDeliveryDetailsValidExample()
        {
            return new string[][] {
                new string[] { "2221-3333-2222-5647", "03/26", "455","Yosii","21212130"}, new string[] { "Gilon 200", "Kfar Saba", "Israel", "20693" }
            };
        }

        internal static Store RandomStoreNoItems()
        {
            return new Store(new Item[] { }, new int[] { });
        }

        public enum Operator
        {
            XOR, OR, AND, IMPLIES
        }

        public static Operator stringToPerator(string op)
        {
            switch (op) {
                case "xor":
                    return Operator.XOR;
                case "|":
                    return Operator.OR;
                case "&":
                    return Operator.AND;
                case ">":
                    return Operator.IMPLIES;
                default:
                    throw new ArgumentException();
            }

        }
    }
}

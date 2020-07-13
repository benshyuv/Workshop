using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using DomainLayer.Users;
using Newtonsoft.Json;

namespace DomainLayer.Stores.PurchasePolicies
{
    public class DaysNotAllowedPurchasePolicy : ABasicPurchasePolicy
    {

        public string DaysOfWeekJsonArrayString { get; set; }
        //default for active policies is an OR between all of them as discussed with yair
        internal List<DayOfWeek> DaysNotAllowed
        {
            get => DaysOfWeekFromStringList(JsonConvert.DeserializeObject<List<string>>(DaysOfWeekJsonArrayString));
        }

        private List<DayOfWeek> DaysOfWeekFromStringList(List<string> stringList)
        {
            List<DayOfWeek> ret = new List<DayOfWeek>();
            foreach (string day in stringList)
            {
                switch (Int32.Parse(day))
                {
                    case 0:
                        ret.Add(DayOfWeek.Sunday);
                        break;
                    case 1:
                        ret.Add(DayOfWeek.Monday);
                        break;
                    case 2:
                        ret.Add(DayOfWeek.Tuesday);
                        break;
                    case 3:
                        ret.Add(DayOfWeek.Wednesday);
                        break;
                    case 4:
                        ret.Add(DayOfWeek.Thursday);
                        break;
                    case 5:
                        ret.Add(DayOfWeek.Friday);
                        break;
                    case 6:
                        ret.Add(DayOfWeek.Saturday);
                        break;
                }
            }
            return ret;
        }

        public String StoreName { get; set; }
        public DaysNotAllowedPurchasePolicy()
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="daysNotAllowed"> 1 = sunday 7 = saturday</param>
        /// <param name="storeName"></param>
        public DaysNotAllowedPurchasePolicy(int[] daysNotAllowed, string storeName):base()
        {
            if (daysNotAllowed is null || daysNotAllowed.Length == 0 || daysNotAllowed.Length == 7)
                throw new ArgumentException("days not allowed must not be empty or include all days");
            StoreName = storeName;
            DaysOfWeekJsonArrayString = JsonConvert.SerializeObject(daysFromInts(daysNotAllowed));
        }

        private List<DayOfWeek> daysFromInts(int[] daysNotAllowed)
        {
            List < DayOfWeek > ret= new List<DayOfWeek>();
            foreach(int i in daysNotAllowed)
            {
                switch (i)
                {
                    case 1:
                        ret.Add(DayOfWeek.Sunday);
                        break;
                    case 2:
                        ret.Add(DayOfWeek.Monday);
                        break;
                    case 3:
                        ret.Add(DayOfWeek.Tuesday);
                        break;
                    case 4:
                        ret.Add(DayOfWeek.Wednesday);
                        break;
                    case 5:
                        ret.Add(DayOfWeek.Thursday);
                        break;
                    case 6:
                        ret.Add(DayOfWeek.Friday);
                        break;
                    case 7:
                        ret.Add(DayOfWeek.Saturday);
                        break;
                }
            }
            return ret;
        }

        internal override bool IsValidPurchase(StoreCart cart)
        {
            DateTime date = DateTime.Now;
            if (DaysNotAllowed.Contains(date.DayOfWeek))
                return false;

            return true;
        }

        internal override APurchasePolicyDataClassForSerialization ToDataClass()
        {
            return new APurchasePolicyDataClassForSerialization(this);
        }

        public override string ToString()
        {
           
            return String.Format("Buying from {0} is not allowed on {1}", StoreName, stringListFromDays(DaysNotAllowed));
        }

        private string stringListFromDays(ICollection<DayOfWeek> daysNotAllowed)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append('[');
            foreach(DayOfWeek day in daysNotAllowed)
            {
                builder.Append(day.ToString());
                builder.Append(", ");
            }
            builder.Append(']');
            return builder.ToString();
        }
    }
}

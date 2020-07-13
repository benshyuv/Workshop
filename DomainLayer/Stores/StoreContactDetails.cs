using System;
using System.Collections.Generic;
using System.Text;

namespace DomainLayer.Stores
{
    public class StoreContactDetails
    {
        public StoreContactDetails()
        {
        }

        public StoreContactDetails(string name, string email, string address, string phone, string bankAccountNumber, string bank, string description)
        {
            Name = name;
            Email = email;
            Address = address;
            Phone = phone;
            BankAccountNumber = bankAccountNumber;
            Bank = bank;
            Description = description;
        }

        public string Name { get; set; }
        public string Email { get; set; }
        public string Address { get; set; }
        public string Phone { get; set; }
        public string BankAccountNumber { get; set; }
        public string Bank { get; set; }
        public string Description { get; set; }

        public override bool Equals(object obj)
        {
            return obj == null ? false : obj is StoreContactDetails objAsStoreContactDetails ? Equals(objAsStoreContactDetails) : false;
        }

        public bool Equals(StoreContactDetails scd)
        {
            if (!Name.Equals(scd.Name) ||
                !Email.Equals(scd.Email) ||
                !Address.Equals(scd.Address) ||
                !Phone.Equals(scd.Phone) ||
                !BankAccountNumber.Equals(scd.BankAccountNumber) ||
                !Bank.Equals(scd.Bank) ||
                !Description.Equals(scd.Description))
            {
                return false;
            }

            return true;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Name, Email, Address, Phone, BankAccountNumber, Bank, Description);
        }
    }
}

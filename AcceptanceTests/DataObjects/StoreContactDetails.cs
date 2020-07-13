using System;

namespace AcceptanceTests.DataObjects
{
    public class StoreContactDetails : IEquatable<StoreContactDetails>
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public string Address { get; set; }
        public string Phone { get; set; }
        public string Bank { get; set; }
        public string BankAccountNumber { get; set; }
        public string Description { get; set; }



        public StoreContactDetails(string name, string email, string address, string phone, string bank, string bankAcountNumber, string description)
        {
            Name = name;
            Email = email;
            Address = address;
            Phone = phone;
            Bank = bank;
            BankAccountNumber = bankAcountNumber;
            Description = description;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as StoreContactDetails);
        }

        public bool Equals(StoreContactDetails other)
        {
            return other != null &&
                   Name.ToLower().Trim() == other.Name.ToLower().Trim() &&
                   Email.ToLower().Trim() == other.Email.ToLower().Trim() &&
                   Address.ToLower().Trim() == other.Address.ToLower().Trim() &&
                   Phone.ToLower().Trim() == other.Phone.ToLower().Trim() &&
                   Bank.ToLower().Trim() == other.Bank.ToLower().Trim() &&
                   BankAccountNumber.ToLower().Trim() == other.BankAccountNumber.ToLower().Trim() &&
                   Description.ToLower().Trim() == other.Description.ToLower().Trim();
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Name, Email, Address, Phone, Bank, BankAccountNumber, Description);
        }


    }
}

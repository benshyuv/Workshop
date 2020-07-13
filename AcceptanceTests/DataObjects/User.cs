using System;

namespace AcceptanceTests.DataObjects
{
    public class User
    {
        public string Username { get; set; }
        public string Pw { get; set; }

        public User(string username, string pw)
        {
            this.Username = username;
            this.Pw = pw ?? throw new ArgumentNullException(nameof(pw));
        }


    }
}

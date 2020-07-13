using DomainLayer.DbAccess;
using System;
using System.Collections.Generic;
using System.Text;

namespace DomainLayer.Users
{
    public class AdminUser : RegisteredUser
    {
        public AdminUser()
        {
        }

        public AdminUser(string username, byte[] password) : base(username, password)
		{ 
        }

        public override bool IsAdmin() => true;

        internal override void LogEntry(Guid sessionID, DailyStatistics stats)
        {
            stats.LogAdmin(sessionID);
        }
    }
}

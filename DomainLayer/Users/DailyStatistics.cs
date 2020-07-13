using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DomainLayer.Users
{
    public enum Roles
    {
        GUEST, REGISTERED, MANAGER, OWNER, ADMIN
    }
    public class DailyStatistics
    {
        [Key]
        public DateTime Date { get; set; }

        public DailyStatistics()
        {
            GuestSessions = new HashSet<Guid>();
        }

        public DailyStatistics(DateTime date)
        {
            Date = date;
            GuestSessions = new HashSet<Guid>();
            Guests = 0;
            Registered = 0;
            Managers = 0;
            Owners = 0;
            Admins = 0;
        }

        private ISet<Guid> GuestSessions { get; }
        
        public int Guests { get; set; }
        public int Registered { get; set; }
        public int Managers { get; set; }
        public int Owners { get; set; }
        public int Admins { get; set; }

        public void LogSession(Guid sessionID)
        {
            if (GuestSessions.Add(sessionID))
            {
                Guests++;
            }
        }

        public void LogRegistered(Guid sessionID)
        {
            if (GuestSessions.Remove(sessionID))
            {
                Guests--;
            }
            Registered++;
        }

        public void LogManager(Guid sessionID)
        {
            if (GuestSessions.Remove(sessionID))
            {
                Guests--;
            }
            Managers++;
        }

        public void LogOwner(Guid sessionID)
        {
            if (GuestSessions.Remove(sessionID))
            {
                Guests--;
            }
            Owners++;
        }

        public void LogAdmin(Guid sessionID)
        {
            if (GuestSessions.Remove(sessionID))
            {
                Guests--;
            }
            Admins++;
        }

        public int[] Statistics()
        {
            return new int[]
            {
                Guests, Registered, Managers, Owners, Admins
            };
        }
    }
}
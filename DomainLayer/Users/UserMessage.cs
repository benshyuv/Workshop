using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace DomainLayer.Users
{
    public class UserMessage
    {
        public UserMessage(string message, Guid userID)
        {
            Message = message;
            UserID = userID;
        }

        public UserMessage() { }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid ID { get; set; }
        public string Message { get; set; }

        [ForeignKey("User")]
        public Guid UserID { get; set; }
        public virtual RegisteredUser User { get; set; }

        public override bool Equals(object obj) => obj is UserMessage userMessage && ID == userMessage.ID;
        public override int GetHashCode() => HashCode.Combine(Message);

        public virtual bool ShouldSerializeUser()
        {
            return false;
        }
    }
}

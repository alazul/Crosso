using System;
using System.Collections.Generic;
using System.Linq;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Crosso.Models
{
    public class Contact
    {
        public ObjectId ContactID { get; set; } = ObjectId.GenerateNewId();
        public ObjectId ConnectedUserID { get; set; }

        public string Name { get; set; }
        public string Surname { get; set; }
        public string FullName
        {
            get
            {
                return $"{ Name } { Surname }";
            }
        }

        public string Status { get; set; }

        public ObjectId ChatID { get; set; } = ObjectId.GenerateNewId();

        public ChatBubble RecentChatBubble { get; set; }

        public Contact(string name, string status = "Online")
        {
            Name = name;
            Status = status;
        }
    }
}
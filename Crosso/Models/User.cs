using System;
using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Crosso.Models
{
    public class User
    {
        [BsonId]
        public ObjectId UserID { get; set; }

        public string UserName { get; set; }
        public string Password { get; set; }

        public string Name { get; set; }
        public string Surname { get; set; }
        public string FullName
        {
            get => $"{ Name } { Surname }";
        }

        public List<Contact> Contacts { get; set; } = new List<Contact>();

        public User(string userName, string password, List<Contact> contacts)
        {
            UserName = userName;
            Password = password;
            Contacts = contacts;
        }
        public User(string userName, string password, string name)
        {
            UserName = userName;
            Password = password;
            Name = name;
        }
    }
}
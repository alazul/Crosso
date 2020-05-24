using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Crosso.Models
{
    public class Group
    {
        [BsonId]
        public ObjectId GroupID { get; set; }
        
        public List<ObjectId> ConnectedUserIDs { get; set; } = new List<ObjectId>();
        public Chat Chat { get; set; }
    }
}
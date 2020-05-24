using System;
using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Crosso.Models
{
    public class ChatBubble
    {
        public ObjectId ChatBubbleID { get; set; } = ObjectId.GenerateNewId();
        public ObjectId OwnerUserID { get; set; }

        public object MessageBody { get; set; }
        public DateTime MessageDate { get; set; } = DateTime.Now;
        public List<ObjectId> UserIDsOfSeenRecipients { get; set; } = new List<ObjectId>();

        public ChatBubble(object messageBody)
        {
            MessageBody = messageBody;
        }

         public ChatBubble(object messageBody, ObjectId ownerUserID)
        {
            MessageBody = messageBody;
            OwnerUserID = ownerUserID;
        }
        
        public ChatBubble(object messageBody, DateTime messageDate)
        {
            MessageBody = messageBody;
            MessageDate = messageDate;
        }
    }
}
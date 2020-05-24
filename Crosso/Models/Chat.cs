using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Crosso.Models
{
    public class Chat
    {
        [BsonId]
        public ObjectId ChatID { get; set; }

        public bool IsGroupChat { get; set; }
        public List<ObjectId> ConnectedUserIDs { get; set; } = new List<ObjectId>();

        public List<ChatBubble> ChatBubbles {get; set;} = new List<ChatBubble>();

        public Chat(ObjectId chatID, List<ObjectId> connectedUserIDs, bool isGroupChat = false)
        {
            ChatID = chatID;
            ConnectedUserIDs = connectedUserIDs;
            IsGroupChat = isGroupChat;
        }

        public Chat(List<ChatBubble> chatBubbles)
        {
            ChatBubbles = chatBubbles;
        }
        public Chat(ObjectId chatID, List<ChatBubble> chatBubbles)
        {
            ChatID = chatID;
            ChatBubbles = chatBubbles;
        }
    }
}
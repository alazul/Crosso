using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Crosso.Models;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Crosso.Managers.CollectionManagers
{
    public class UserCollectionManager : ICollectionManager<User>
    {
        private DBManager _dbManager;

        public UserCollectionManager(DBManager dbManager)
        {
            _dbManager = dbManager;
        }

        //Drop Users
        public async Task<bool> DropCollectionAsync()
        {
            return await _dbManager.database.DropCollectionAsync("users").ContinueWith(t =>
            {
                if (t.IsCompletedSuccessfully) return true;
                else return false; //!ERROR Couldn't drop Users collection
            });
        }
        public void DropCollection()
        {
            _dbManager.database.DropCollection("users");
        }

        //Add User
        public async Task<bool> AddDocumentAsync(User user)
        {
            return await _dbManager.UserCollection.InsertOneAsync(user).ContinueWith(t =>
            {
                if (t.IsCompletedSuccessfully) return true;
                else return false; //!ERROR Couldn't insert to Users collection
            });
        }
        public void AddDocument(User user)
        {
            //Add Chats of Contacts
            List<Chat> chats = new List<Chat>()
            {
                new Chat(new List<ChatBubble> { new ChatBubble("Merhaba!") }),
                new Chat(new List<ChatBubble> { new ChatBubble("Nasılsın?") }),
                new Chat(new List<ChatBubble> { new ChatBubble("ee...") }),
                new Chat(new List<ChatBubble> { new ChatBubble("Akmak?") })
            };

            for (int i = 0; i < user.Contacts.Count(); i++)
            {
                chats[i].ChatID = user.Contacts[i].ChatID;
                user.Contacts[i].RecentChatBubble = chats[i].ChatBubbles.LastOrDefault();
            }

            _dbManager.ChatCollection.InsertMany(chats);
            _dbManager.UserCollection.InsertOne(user);
        }

        
    }
}
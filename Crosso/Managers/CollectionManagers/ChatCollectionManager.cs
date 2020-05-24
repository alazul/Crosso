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
    public class ChatCollectionManager : ICollectionManager<Chat>
    {
        private DBManager _dbManager;

        public ChatCollectionManager(DBManager dbManager)
        {
            _dbManager = dbManager;
        }

        //Drop Chats
        public async Task<bool> DropCollectionAsync()
        {
            return await _dbManager.database.DropCollectionAsync("chats").ContinueWith(t =>
            {
                if (t.IsCompletedSuccessfully) return true;
                else return false; //!ERROR Couldn't drop Chats collection
            });
        }
        public void DropCollection()
        {
            _dbManager.database.DropCollection("chats");
        }

        //Add Chat
        public async Task<bool> AddDocumentAsync(Chat chat)
        {
            return await _dbManager.ChatCollection.InsertOneAsync(chat).ContinueWith(t =>
            {
                if (t.IsCompletedSuccessfully) return true;
                else return false; //!ERROR Couldn't insert to Chats collection
            });
        }
        public void AddDocument(Chat chat)
        {
            _dbManager.ChatCollection.InsertOne(chat);
        }

        
    }
}
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Crosso.Models;
using MongoDB.Driver;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using Crosso.Managers.CollectionManagers;
using MongoDB.Bson;

namespace Crosso.Managers
{
    public enum Collections
    {
        users,
        chats,
    }

    public class DBManager
    {
        public MongoClient client = new MongoClient("mongodb+srv://bigdaddy:12Mo56Om78@crosso-7ubfa.mongodb.net/test?retryWrites=true&w=majority");
        public IMongoDatabase database;

        public IMongoCollection<User> UserCollection
        {
            get => GetCollection<User>("users");
        }

        public IMongoCollection<Chat> ChatCollection
        {
            get => GetCollection<Chat>("chats");
        }

        public event Action NewMessageArrived;
        public event Action NewContactAdded;
        //public event EventHandler CollectionChanged; //alternative to action


        public DBManager()
        {
            database = client.GetDatabase("crossoDB");
        }

        public async void StartChatWatcher(ObjectId currentUserID)
        {
            var pipeline = new EmptyPipelineDefinition<ChangeStreamDocument<Chat>>()
                        .Match(x => (x.OperationType == ChangeStreamOperationType.Replace || x.OperationType == ChangeStreamOperationType.Update)
                                    && x.FullDocument.ConnectedUserIDs.Contains(currentUserID));

            var changeStreamOptions = new ChangeStreamOptions
            {
                FullDocument = ChangeStreamFullDocumentOption.UpdateLookup
            };

            using (var cursor = await GetCollection<Chat>(nameof(Collections.chats)).WatchAsync(pipeline, changeStreamOptions))
            {
                await cursor.ForEachAsync(changedChat =>
                {
                    if (changedChat.FullDocument.ChatBubbles.Last().OwnerUserID != currentUserID)
                    {
                        NewMessageArrived?.Invoke();
                    }
                });
            }
        }
        public async void StartContactListWatcher(ObjectId currentUserID) //TODO: Filter just for Contacts List use this x.UpdateDescription.UpdatedFields.Contains("Contacts") but its not working
        {
            var pipeline = new EmptyPipelineDefinition<ChangeStreamDocument<User>>()
                        .Match(x => (x.OperationType == ChangeStreamOperationType.Replace || x.OperationType == ChangeStreamOperationType.Update)
                                    && x.FullDocument.UserID == currentUserID);

            var changeStreamOptions = new ChangeStreamOptions
            {
                FullDocument = ChangeStreamFullDocumentOption.UpdateLookup
            };

            using (var cursor = await GetCollection<User>(nameof(Collections.users)).WatchAsync(pipeline, changeStreamOptions))
            {
                await cursor.ForEachAsync(changedUser =>
                {
                    NewContactAdded?.Invoke();
                });
            }
        }

        public void InsertUser(User user)
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

            ChatCollection.InsertMany(chats);
            UserCollection.InsertOne(user);
        }

        public IMongoCollection<T> GetCollection<T>(string name)
        {
            return database.GetCollection<T>(name);
        }

        public void DropCollection(string name)
        {
            database.DropCollection(name);
        }

        //Generic Find
        public T FindDocument<T>(Expression<Func<T, bool>> expression, IMongoCollection<T> targetCollection)
        {
            var result = targetCollection.Find<T>(expression);

            if (!EqualityComparer<IFindFluent<T, T>>.Default.Equals(result, default(IFindFluent<T, T>)) &&
                !EqualityComparer<T>.Default.Equals(result.ToList().FirstOrDefault(), default(T)))
            {
                return result.ToList().FirstOrDefault();
            }
            else return default(T); //!ERROR
        }


        public async Task<T> FindDocumentByIDAsync<T>(ObjectId targetID, IMongoCollection<T> targetCollection)
        {
            var result = await targetCollection.FindAsync(Builders<T>.Filter.Eq("_id", targetID));

            var resultCount = result?.Current?.Count();

            if (resultCount == 1) return result.Current.FirstOrDefault();
            else if (resultCount == 0) return default(T);  //!ERROR
            else if (resultCount > 1) return default(T); //!ERROR
            else return default(T); //!ERROR
        }

        public T FindDocumentByID<T>(ObjectId targetID, IMongoCollection<T> targetCollection)
        {
            var result = targetCollection.Find(Builders<T>.Filter.Eq("_id", targetID));

            var resultCount = result?.CountDocuments();

            if (resultCount == 1) return result.FirstOrDefault();
            else if (resultCount == 0) return default(T);  //!ERROR
            else if (resultCount > 1) return default(T); //!ERROR
            else return default(T); //!ERROR
        }


    }
}
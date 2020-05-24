using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Crosso.Models;
using MongoDB.Bson;
using MongoDB.Driver.Linq;
using MongoDB.Driver;
using Crosso.Managers.CollectionManagers;

namespace Crosso.Managers
{
    public class CrossoManager
    {
        private DBManager _dbManager;

        public User CurrentUser { get; set; }

        public event Action UserLoggedIn;

        public event Action ChatChanged;

        public event Action ContactListChanged;

        public CrossoManager(DBManager dbMan)
        {
            _dbManager = dbMan;

            _dbManager.NewMessageArrived += OnNewMessageArrived;
            _dbManager.NewContactAdded += OnNewContactAdded;

            CreateDummyDB();
        }

        public void OnNewMessageArrived()
        {
            ChatChanged?.Invoke();
        }
        public void OnNewContactAdded()
        {
            UpdateCurrentUser();
            ContactListChanged?.Invoke();
        }

        public void UpdateCurrentUser()
        {
            var result = _dbManager.UserCollection.Find<User>(u => u.UserID == CurrentUser.UserID);
            var foundUser = result.SingleOrDefault();
            if (foundUser != null)
            {
                CurrentUser = foundUser;
            }
        }

        public bool Login(string userName, string password)
        {
            var result = _dbManager.UserCollection.Find<User>(u => u.UserName == userName && u.Password == password);
            var foundUser = result.SingleOrDefault();
            if (foundUser != null)
            {
                CurrentUser = foundUser;
                UserLoggedIn?.Invoke();

                _dbManager.StartChatWatcher(CurrentUser.UserID);
                _dbManager.StartContactListWatcher(CurrentUser.UserID);
                return true;
            }
            else
            {
                return false; //! Error  login error
            }
        }
        public bool Register(string userName, string password)
        {
            if (userName != null && password != null)
            {
                User newUser = new User(userName, password, userName);
                _dbManager.UserCollection.InsertOne(newUser);
                return true;
            }
            else return false; //! error param is
        }
        public bool AddContactToUser(string targetUserName)
        {
            if (!String.IsNullOrWhiteSpace(targetUserName))
            {
                var searchResult = _dbManager.UserCollection.Find(u => u.UserName == targetUserName);
                var foundUser = searchResult?.SingleOrDefault();

                if (foundUser != null)
                {
                    Contact newContact = new Contact(foundUser.Name);
                    newContact.ConnectedUserID = foundUser.UserID;

                    var currentUserUpdateResult = _dbManager.UserCollection.UpdateOne<User>(c => c.UserID == CurrentUser.UserID,
                    Builders<User>.Update.Push<Contact>(u => u.Contacts, newContact));

                    var foundUserUpdateResult = _dbManager.UserCollection.UpdateOne<User>(c => c.UserID == foundUser.UserID,
                    Builders<User>.Update.Push<Contact>(u => u.Contacts, newContact));

                    if (currentUserUpdateResult.IsModifiedCountAvailable && foundUserUpdateResult.IsModifiedCountAvailable)
                    {
                        _dbManager.ChatCollection.InsertOne(new Chat(newContact.ChatID, new List<ObjectId>() { CurrentUser.UserID, foundUser.UserID }));
                        ContactListChanged?.Invoke();
                        return true;
                    }

                    else return false; //! error couldnt update user contacts
                }
                else return false; //! error couldnt find the user


            }
            else return false; //! error param is
        }



        public async Task<Chat> GetChatOfContactAsync(ObjectId targetChatID)
        {
            var result = await _dbManager.ChatCollection.FindAsync<Chat>(c => c.ChatID == targetChatID);
            return await result.SingleOrDefaultAsync(); //!Error if null
        }

        public Chat GetChatOfContact(ObjectId targetChatID)
        {
            var result = _dbManager.ChatCollection.Find<Chat>(c => c.ChatID == targetChatID);
            return result.SingleOrDefault(); //!Error if null
        }

        public Chat SendMessage(ObjectId targetChatID, object message)
        {
            var chatBubble = new ChatBubble(message, CurrentUser.UserID);
            var newChatBubbles = GetChatOfContact(targetChatID).ChatBubbles;
            newChatBubbles.Add(chatBubble);
            //newChatBubbles.Insert(0, chatBubble);
            var result = _dbManager.ChatCollection.UpdateOne<Chat>(c => c.ChatID == targetChatID,
            Builders<Chat>.Update.Set<List<ChatBubble>>(u => u.ChatBubbles, newChatBubbles));
            if (result is null)
            {
                //! update error
            }
            if (result.ModifiedCount != 1)
            {

            }
            return GetChatOfContact(targetChatID);
        }

        public void CreateDummyDB()
        {
            /*_dbManager.database.DropCollection(nameof(Collections.users));
            _dbManager.database.DropCollection(nameof(Collections.chats));

            List<Contact> contacts = new List<Contact>(){
                new Contact("Fatih", "Online"),
                new Contact("Osman", "Online"),
                new Contact("Fatma", "Online"),
                new Contact("Doğan", "Online")
            };
            User user = new User("Fatih", "12345678", contacts);
            _dbManager.InsertUser(user);*/

            //CurrentUser = _dbManager.UserCollection.Find<User>(u => u.UserID == new ObjectId("5e9778324c7f973298f321c4")).SingleOrDefault();
        }

        public void CheckForNewContacts()
        {
            var result = _dbManager.ChatCollection.Find(c => c.ConnectedUserIDs.Contains(CurrentUser.UserID)).ToList();
            foreach (var chat in result)
            {
                if (!CurrentUser.Contacts.Any(c => chat.ConnectedUserIDs.Contains(c.ConnectedUserID)))
                {
                    var IDOfMissingUser = chat.ConnectedUserIDs.Where(id => id != CurrentUser.UserID).SingleOrDefault();
                    var missingUser = _dbManager.UserCollection.Find(u => u.UserID == IDOfMissingUser).SingleOrDefault();

                    Contact newContact = new Contact(missingUser.Name);
                    newContact.ConnectedUserID = missingUser.UserID;

                    var updateResult = _dbManager.UserCollection.UpdateOne<User>(c => c.UserID == CurrentUser.UserID,
                    Builders<User>.Update.Push<Contact>(u => u.Contacts, newContact));

                    // if (updateResult.IsModifiedCountAvailable)
                    // {
                    //     _dbManager.ChatCollection.InsertOne(new Chat(newContact.ChatID, new List<ObjectId>() { CurrentUser.UserID, missingUser.UserID }));
                    // }
                }
            }
        }


    }
}
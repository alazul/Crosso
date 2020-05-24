using System;
using System.Linq.Expressions;
using System.Threading.Tasks;
using MongoDB.Bson;

namespace Crosso.Managers.CollectionManagers
{
    public interface ICollectionManager<T> where T : class
    {
         Task<bool> DropCollectionAsync();
         void DropCollection();

         Task<bool> AddDocumentAsync(T document);
         void AddDocument(T user);
    }
}
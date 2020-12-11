using MongoDB.Driver;
using SayWhat.MongoDAL.Dictionary;
using SayWhat.MongoDAL.Examples;
using SayWhat.MongoDAL.Users;
using SayWhat.MongoDAL.Words;

namespace Chotiskazal.Bot
{
    public class StatsMigrator
    {
        public void Do(IMongoDatabase db)
        {
            var userWordRepo   = new UserWordsRepo(db);
            var dictionaryRepo = new DictionaryRepo(db);
            var userRepo       = new UsersRepo(db);
            var examplesRepo   = new ExamplesRepo(db);
            var allUsers = userRepo.GetAll();
            /*foreach (var user in allUsers)
            {
                user.
            }*/
        }
    }
}
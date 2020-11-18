using MongoDB.Driver;

namespace SayWa.MongoDAL.Tests
{
    public static class MongoTestHelper
    {
        static MongoTestHelper()
        {
            string connectionString = "mongodb://localhost";
            MongoClient client = new MongoClient(connectionString);
            Database = client.GetDatabase("Tests");
        }
        public static readonly IMongoDatabase Database;

        public static void DropAllCollections()
        {
            var collections = Database.ListCollections().ToList();
            foreach (var collection in collections)
            {
                Database.DropCollection(collection["name"].AsString);
            }
        }
    }
}
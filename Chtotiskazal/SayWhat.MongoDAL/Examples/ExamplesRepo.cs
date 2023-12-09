using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;

namespace SayWhat.MongoDAL.Examples;

public class ExamplesRepo {
    private readonly IMongoDatabase _db;
    public ExamplesRepo(IMongoDatabase db) => _db = db;

    public Task Add(Example example) => Collection.InsertOneAsync(example);

    public Task Add(IEnumerable<Example> examples) {
        if (examples.Any())
            return Collection.InsertManyAsync(examples);
        return Task.CompletedTask;
    }

    public Task<Example> GetOrDefault(ObjectId exampleId) =>
        Collection
            .Find(Builders<Example>.Filter.Eq("Id", exampleId))
            .FirstOrDefaultAsync();


    public Task<List<Example>> GetAll(IEnumerable<ObjectId> ids) =>
        Collection
            .Find(Builders<Example>.Filter.In("Id", ids))
            .ToListAsync();

    public Task<List<Example>> GetAll() =>
        Collection
            .Find(Builders<Example>.Filter.Empty)
            .ToListAsync();

    private IMongoCollection<Example> Collection
        => _db.GetCollection<Example>(ExampleCollectionName);

    public const string ExampleCollectionName = "examples";

    public Task UpdateDb() => Task.CompletedTask;

    /*public async Task UpdateDb()
    {
        await Collection.Indexes.DropAllAsync();
        var keys = Builders<User>.IndexKeys.Ascending(UserTelegramIdFieldName);
        var indexOptions = new CreateIndexOptions<User>
        {
            Unique = true ,
            PartialFilterExpression = Builders<User>.Filter.Type(u=>u.TelegramId, BsonType.Int64)
        };
        var model = new CreateIndexModel<User>(keys, indexOptions);
        await Collection.Indexes.CreateOneAsync(model);
    }*/
    public Task<long> GetCount() => Collection.CountDocumentsAsync(new BsonDocument());
}
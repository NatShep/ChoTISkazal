using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;

namespace SayWhat.MongoDAL.LongDataForTranslationButton;

public class LongCallbackDataRepo : IMongoRepo {
    private readonly IMongoDatabase _db;

    public const string LongDataForButtonCollectionName = "longTranslations";
    public const string WordFieldName = "w";
    public const string TranslateFieldName = "tr";

    private IMongoCollection<LongCallbackData> Collection
        => _db.GetCollection<LongCallbackData>(LongDataForButtonCollectionName);

    public LongCallbackDataRepo(IMongoDatabase db) => _db = db;
    public Task Add(LongCallbackData callbackData) => Collection.InsertOneAsync(callbackData);
    public List<LongCallbackData> GetAll() => Collection.AsQueryable().ToList();
    public Task<long> GetCount() => Collection.CountDocumentsAsync(new BsonDocument());

    public Task<LongCallbackData> GetCallbackDataOrDefault(string translation) =>
        Collection
            .Find(Builders<LongCallbackData>.Filter.Eq(TranslateFieldName, translation))
            .FirstOrDefaultAsync();

    public Task<LongCallbackData?> GetCallbackDataOrDefault(ObjectId id) =>
        Collection
            .Find(Builders<LongCallbackData>.Filter.Eq("Id", id))
            .FirstOrDefaultAsync();

    public async Task UpdateDb() {
        await Collection.Indexes.DropAllAsync();
        var keys = Builders<LongCallbackData>.IndexKeys.Ascending(TranslateFieldName);
        var indexOptions = new CreateIndexOptions<LongCallbackData>
        {
            Unique = true
        };

        var model = new CreateIndexModel<LongCallbackData>(keys, indexOptions);
        await Collection.Indexes.CreateOneAsync(model);
    }
}
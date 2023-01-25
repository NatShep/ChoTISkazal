using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;

namespace SayWhat.MongoDAL.LongDataForTranslationButton {
public class LongDataForButtonRepo : IMongoRepo {
    public const string LongDataForButtonCollectionName = "longTranslations";
    public const string WordFieldName = "w";
    public const string TranslateFieldName = "tr";
    // А нужен ли тут вообще hash? Может искать по строке?

    private IMongoCollection<LongDataForButton> Collection
        => _db.GetCollection<LongDataForButton>(LongDataForButtonCollectionName);

    private readonly IMongoDatabase _db;
    public LongDataForButtonRepo(IMongoDatabase db) => _db = db;
    public Task Add(LongDataForButton data) => Collection.InsertOneAsync(data);
    public List<LongDataForButton> GetAll() => Collection.AsQueryable().ToList();
    public Task<long> GetCount() => Collection.CountDocumentsAsync(new BsonDocument());
    public Task<LongDataForButton> GetButtonDataOrDefault(string translation) =>
        Collection
            .Find(Builders<LongDataForButton>.Filter.Eq(TranslateFieldName, translation))
            .FirstOrDefaultAsync();

    public Task<LongDataForButton?> GetButtonDataOrDefault(ObjectId id) =>
        Collection
            .Find(Builders<LongDataForButton>.Filter.Eq("Id", id))
            .FirstOrDefaultAsync();

    public async Task UpdateDb() {
        await Collection.Indexes.DropAllAsync();
        var keys = Builders<LongDataForButton>.IndexKeys.Ascending(TranslateFieldName);
        var indexOptions = new CreateIndexOptions<LongDataForButton>
        {
            Unique = true
        };

        var model = new CreateIndexModel<LongDataForButton>(keys, indexOptions);
        await Collection.Indexes.CreateOneAsync(model);
    }
}
}
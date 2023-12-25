using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;

namespace SayWhat.MongoDAL.FrequentWords;

public class FrequentWordsRepo : IMongoRepo {
    private readonly IMongoDatabase _db;

    private IMongoCollection<FrequentWord> Collection
        => _db.GetCollection<FrequentWord>("frequentWords");

    public FrequentWordsRepo(IMongoDatabase db) => _db = db;

    public Task Add(FrequentWord word) => Collection.InsertOneAsync(word);

    public Task Update(FrequentWord word) => Collection.ReplaceOneAsync(o => o.Id == word.Id, word);

    public Task<FrequentWord> GetOrDefault(ObjectId id) =>
        Collection
            .Find(Builders<FrequentWord>.Filter.Eq("Id", id))
            .FirstOrDefaultAsync();
    
    public Task<FrequentWord> GetOrDefault(int number) =>
        Collection
            .Find(Builders<FrequentWord>.Filter.Eq("n", number))
            .FirstOrDefaultAsync();

    public Task UpdateDb() => Task.CompletedTask;

    public Task<List<FrequentWord>> GetAll() => Collection
        .Find(Builders<FrequentWord>.Filter.Empty)
        .ToListAsync();

    public Task<long> GetCount() => Collection.CountDocumentsAsync(new BsonDocument());
}
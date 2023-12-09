using System.Threading.Tasks;

namespace SayWhat.MongoDAL;

public interface IMongoRepo {
    Task UpdateDb();
}
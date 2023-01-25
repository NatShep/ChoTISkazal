using System.Threading.Tasks;
using MongoDB.Bson;
using SayWhat.MongoDAL.LongDataForTranslationButton;

namespace SayWhat.Bll.Services {
public class LongDataForButtonService {
    private readonly LongDataForButtonRepo _longDataRepository;
    
    public LongDataForButtonService(LongDataForButtonRepo repository) {
        _longDataRepository = repository;
    }
    
    public async Task<LongDataForButton> GetLongButtonData(string translation) {
        return await _longDataRepository.GetButtonDataOrDefault(translation);
    }
    
    public async Task<LongDataForButton?> GetLongButtonData(ObjectId translation) {
        return await _longDataRepository.GetButtonDataOrDefault(translation);
    }
    
    public async Task AddLongButtonData(LongDataForButton data) {
        await _longDataRepository.Add(data);
    }
}
}
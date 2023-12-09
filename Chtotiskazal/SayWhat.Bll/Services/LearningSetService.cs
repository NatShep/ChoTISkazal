using System.Collections.Generic;
using System.Threading.Tasks;
using SayWhat.MongoDAL.LearningSets;

namespace SayWhat.Bll.Services;

public class LearningSetService {
    private readonly LearningSetsRepo _learningSetsRepo;

    public LearningSetService(LearningSetsRepo learningSetsRepo) {
        _learningSetsRepo = learningSetsRepo;
    }

    public Task<List<LearningSet>> GetAllSets() => _learningSetsRepo.GetAll();
    public Task Add(LearningSet learningSet) => _learningSetsRepo.Add(learningSet);
}
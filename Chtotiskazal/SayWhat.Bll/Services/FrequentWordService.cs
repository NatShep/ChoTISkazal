using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SayWhat.MongoDAL.FrequentWords;

namespace SayWhat.Bll.Services;

public class FrequentWordService
{
    private readonly FrequentWordsRepo _repo;

    public FrequentWordService(FrequentWordsRepo repo)
    {
        _repo = repo;
    }

    public async Task<int> Count() => (int) await _repo.GetCount();
    
    public async Task<FrequentWord> GetWord(int number)
    {
        var loaded = await GetAll();
        return loaded.FirstOrDefault(l => l.OrderNumber == number);
    }

    private List<FrequentWord> _loaded = null;
    public async Task<List<FrequentWord>> GetAll() => _loaded ??= await _repo.GetAll();
}

public record CentralKnowledgeSection(int Left, int Right);
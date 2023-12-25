using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Bson;
using SayWhat.MongoDAL.FrequentWords;

namespace SayWhat.Bll.Services;

public class FrequentWordService
{
    private readonly FrequentWordsRepo _repo;

    public FrequentWordService(FrequentWordsRepo repo)
    {
        _repo = repo;
    }
    
    public Task<long> Count() => _repo.GetCount();

    public async Task<FrequentWord> GetWord(int number)
    {
        return new FrequentWord
        {
            Word = "Mother",
            AllowedTranslations = new[]{"Мама"},
            AllowedExamples = Array.Empty<ObjectId>()
        };
        //return _repo.GetOrDefault(number);
    }

    public Task<List<FrequentWord>> GetAll() => _repo.GetAll();
}

public class FreqWordsState
{
    public FreqWordsState(List<FreqWordUsage> history)
    {
        _history = history;
    }

    private readonly List<FreqWordUsage> _history; 
    public bool IsEmpty => _history.Any();
    public int NextNumber
    {
        get
        {
            if (IsEmpty)
                return 1700;
            var lastResult = _history[^1].Result;
            return lastResult switch
            {
                FreqWordResult.Known => CalcNext(20),
                FreqWordResult.Learning => CalcNext(10),
                FreqWordResult.AlreadyLearning => CalcNext(10),
                _ => throw new NotSupportedException()
            };
        }
    }

    public void AddHistory(int number, FreqWordResult result)
    {
        _history.Add(new FreqWordUsage(number, result));
    }

    private int CalcNext(int d)
    {
        var delta = d;
        var next = _history[^1].Number;
        while (true)
        {
            next += delta;
            var already = _history.FirstOrDefault(h => h.Number == next);
            if (already == null)
                return next;
            switch (already.Result)
            {
                case FreqWordResult.Known:
                    delta += d/2;
                    break;
                case FreqWordResult.Learning:
                case FreqWordResult.AlreadyLearning:
                    delta += d/4;
                    break;
                default:
                    throw new NotSupportedException();
            }
        }
    }
}

public record FreqWordUsage(int Number, FreqWordResult Result);
public enum FreqWordResult
{
    Learning = 0,
    Known = 1,
    AlreadyLearning = 2
}
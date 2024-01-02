using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Bson;
using SayWhat.MongoDAL.FrequentWords;
using SayWhat.MongoDAL.Users;

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
            AllowedTranslations = new[] { "Мама" },
            AllowedExamples = Array.Empty<ObjectId>()
        };
        //return _repo.GetOrDefault(number);
    }

    public Task<List<FrequentWord>> GetAll() => _repo.GetAll();
}

public class FreqWordsState
{
    private readonly FrequentWordService _frequentWordService;

    private FreqWordsState(List<UserFreqItem> wordUsages, int size, FrequentWordService frequentWordService)
    {
        _frequentWordService = frequentWordService;
        _selector = new FreqWordsSelector(wordUsages, size);
    }

    public static async Task<FreqWordsState> Initialize(
        UserModel user,
        //List<FreqWordUsage> wordUsages,
        //int size,
        FrequentWordService frequentWordService)
    {
        var size = (await frequentWordService.GetAll()).Last().OrderNumber;
        
        
        var state = new FreqWordsState(user.OrderedFrequentItems.ToList(), size, frequentWordService);
        await state.SetWord();
        return state;
    }

    private readonly FreqWordsSelector _selector;

    public FrequentWord Next { get; private set; }

    public async Task<bool> MoveNext(FreqWordResult? result = null)
    {
        if (result != null)
            AddHistory(Next.OrderNumber, result.Value);
        await SetWord();
        return true;
    }

    private async Task SetWord()
    {
        var section = _selector.CalcCentralSection();
        var number = section.Left + (section.Right - section.Left) / 2;
        while (true)
        {
            var word = _selector.GetFreeLeft(number);
            var frequentWord = await _frequentWordService.GetWord(word.Number);
            if (frequentWord == null)
                continue;
            Next = frequentWord;
        }
    }

    void AddHistory(int currentOrderNumber, FreqWordResult result)
    {
        _selector.Add(currentOrderNumber, result);
    }
}

public class FreqWordsSelector
{
    private readonly int _size;
    private readonly List<UserFreqItem> _orderedHistory;

    public FreqWordsSelector(List<UserFreqItem> history, int size)
    {
        _size = size;
        _orderedHistory = history.OrderBy(h => h.Number).ToList();
    }

    public bool IsEmpty => !_orderedHistory.Any();

    public CentralKnowledgeSection CalcCentralSection()
    {
        /*
         * Отрезок разделения классов - минимально возможный отрезок, внутри которого нету провернного слова, при этом красных точек справа будет столько же сколько и синих слева
         *
         *  Считаем что в этом отрезке вероятность знания слова равна 50 процентов, справа меньше 50, а слева больше 50.
         *
         *  Пример:
         *  ```
         *
         *  ggg----g---g--rr-rg---g[******]r-rrr-----g-g-r-g----r-r-r-
         *  ```
         *  В этом примере слева от отрезка находится 3 красных, а справа - 3 зеленых. Графически видно что справа область незнания, а слева - зона знания
         *
         *  Алгоритм:
         *
         *  в сортированном массиве точек:
         *  while(true):
         *   идем слева на право, пока не встретим первого красного
         *   идем справа на лево, пока не встретим первого зеленого
         *   если первый красный правее первого зеленого - то это и есть отрезок разделения классов
         *
         *  Посмотрим на вырожденные случаи
         *
         *  ```
         *  -gggg-gg-g--g-g-gg-g----g[*]r-----------------------------
         *  -------gggg-gg-g--g-g-gg-g----g------r[****]g-------------
         *  gg-gg-gg-g----g------r-g---gggg-g-g-g-g-g-g[****]g--------
         *  ```
         *  На вырожденных случаях это тоже выглядит разумно
         */
        if (IsEmpty)
            return new CentralKnowledgeSection(0, _size);

        int left = -1;
        int right = _orderedHistory.Count;

        while (true)
        {
            left = GetNextRed(left);
            right = GetPrevGreen(right);
            if (left == -1)
                break;
            if (Math.Abs(left - right) == 1)
                break;
            if (left > right)
                break;
        }

        if (left > right || left == -1)
            (left, right) = (right, left);

        int leftNumber;
        if (left != -1)
            leftNumber = _orderedHistory[left].Number;
        else if (right > 0) //previous left point or 0
            leftNumber = _orderedHistory[right - 1].Number;
        else
            leftNumber = 0;

        int rightNumber;
        if (right != -1)
            rightNumber = _orderedHistory[right].Number;
        else if (left != -1 && _orderedHistory.Count - 1 > left) //next right point or size
            rightNumber = _orderedHistory[left + 1].Number;
        else
            rightNumber = _size;

        return new CentralKnowledgeSection(leftNumber, rightNumber);
    }

    private int GetPrevGreen(int right)
    {
        for (int r = right - 1; r >= 0; r--)
            if (!IsRed(r))
                return r;
        return -1;
    }

    private int GetNextRed(int start)
    {
        for (int l = start + 1; l < _orderedHistory.Count; l++)
            if (IsRed(l))
                return l;
        return -1;
    }

    private bool IsRed(int index) => _orderedHistory[index].Result != FreqWordResult.Known;

    public void Add(int currentOrderNumber, FreqWordResult result)
    {
        int number = -1;
        foreach (var wordUsage in _orderedHistory)
        {
            if (wordUsage.Number > currentOrderNumber)
                break;
            if (wordUsage.Number == currentOrderNumber)
                throw new InvalidOperationException("Number " + currentOrderNumber + " already used");
            number++;
        }

        _orderedHistory.Insert(number, new UserFreqItem(currentOrderNumber, result));
    }

    public UserFreqItem GetFreeLeft(int middle)
    {
        throw new NotImplementedException();
    }

    public bool Contains(int number) => _orderedHistory.Any(o => o.Number == number);
}

public record CentralKnowledgeSection(int Left, int Right);
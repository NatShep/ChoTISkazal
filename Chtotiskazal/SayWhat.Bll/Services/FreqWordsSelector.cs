using System;
using System.Collections.Generic;
using System.Linq;
using SayWhat.MongoDAL.Users;

namespace SayWhat.Bll.Services;

public class FreqWordsSelector
{
    private readonly int _size;
    private readonly List<UserFreqWord> _orderedHistory;

    public FreqWordsSelector(List<UserFreqWord> history, int size)
    {
        _size = size;
        _orderedHistory = history.OrderBy(h => h.Number).ToList();
    }

    public bool IsEmpty => !_orderedHistory.Any();
    public int Count => _orderedHistory.Count;
    public int MaxSize => _size;

    public CentralKnowledgeSection CalcCentralSection()
    {
        /*
         * Отрезок разделения классов - минимально возможный отрезок, внутри которого нету провернного слова,
         * при этом красных точек справа будет столько же сколько и синих слева
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

    private bool IsRed(int index) => IsRed(_orderedHistory[index].Result);

    private bool IsRed(FreqWordResult result)
    {
        if (result is FreqWordResult.AlreadyLearned or FreqWordResult.UserSelectThatItIsKnown)
            return false;
        else
            return true;
    }

    public void Add(int currentOrderNumber, FreqWordResult result)
    {
        int number = 0;
        foreach (var wordUsage in _orderedHistory)
        {
            if (wordUsage.Number > currentOrderNumber)
                break;
            if (wordUsage.Number == currentOrderNumber)
                throw new InvalidOperationException("Number " + currentOrderNumber + " already used");
            number++;
        }

        var item = new UserFreqWord(currentOrderNumber, result);
        if (number >= _orderedHistory.Count)
            _orderedHistory.Add(item);
        else
            _orderedHistory.Insert(number, item);
    }

    public int? GetFreeLeft(int middle)
    {
        var order = middle;
        while (order >= 0)
        {
            var item = FindOrNull(order);
            if (item == null)
            {
                return order;
            }

            order--;
        }

        return null;
    }

    public int? GetFreeRight(int middle)
    {
        var order = middle;
        while (order < MaxSize)
        {
            var item = FindOrNull(order);
            if (item == null)
            {
                return order;
            }

            order++;
        }

        return null;
    }

    private UserFreqWord FindOrNull(int number) => _orderedHistory.FirstOrDefault(o => o.Number == number);

    public FreqWordDistribution GetDistribution(int midPoint)
    {
        var ltRed = 0;
        var ltGreen = 0;
        var gtRed = 0;
        var gtGreen = 0;
        foreach (var word in _orderedHistory)
        {
            var isRed = IsRed(word.Result);
            if (word.Number < midPoint)
            {
                if (isRed) ltRed++;
                else ltGreen++;
            }
            else if (word.Number > midPoint)
            {
                if (isRed) gtRed++;
                else gtGreen++;
            }
        }

        return new FreqWordDistribution(ltRed, ltGreen, gtRed, gtGreen);
    }
}

public record FreqWordDistribution(int LtRed, int LtGreen, int GtRed, int GtGreen);
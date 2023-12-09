using System;

namespace Chotiskazal.Bot.ChatFlows;

public class CalendarItem
{

    public CalendarItem(DateTime date, int examsCount, double score)
    {
            ExamsCount = examsCount;
            Date = date;
            Score = score;
        }

    public DateTime Date { get; }
    public int ExamsCount { get; }
    public double Score { get; }
}
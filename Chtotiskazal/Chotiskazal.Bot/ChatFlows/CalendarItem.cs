using System;

namespace Chotiskazal.Bot.ChatFlows
{
    public class CalendarItem
    {
        public CalendarItem(DateTime date, double score)
        {
            Date = date;
            Score = score;
        }

        public DateTime Date { get; }
        public double Score { get; }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using SayWhat.MongoDAL.Users;

namespace Chotiskazal.Bot.ChatFlows;

public static class StatsHelper
{
    /// <summary>
    /// It exludes today if it not reached goal yet
    /// </summary>
    public static GoalStreakAnalyze GetCurrentGoalsStreak(CalendarItem[] items, int examsCountGoalForDay)
    {
        var todayStats = items.FirstOrDefault(i => i.Date == DateTime.Today);
        if (todayStats != null && todayStats.ExamsCount < examsCountGoalForDay)
        {
            return GetGoalsStreak(items.Where(i => i != todayStats).ToArray(), examsCountGoalForDay);
        }

        return GetGoalsStreak(items, examsCountGoalForDay);
    }

    public static GoalStreakAnalyze GetGoalsStreak(CalendarItem[] items, int examsCountGoalForDay)
    {
        var dayGoals = items.GetOffsets(examsCountGoalForDay)
            .OrderBy(v => v.Key)
            .Select(v => v.Value)
            .Select(v => v == DayGoalResult.Goal || v == DayGoalResult.Overreaching)
            .ToArray();

        const int maxGap = 2;
        int streak = 0;
        int fallDayInWeek = 0;
        int fallDayInRow = 0;
        int totalFallDays = 0;
        foreach (var day in dayGoals)
        {
            if (day)
            {
                streak += fallDayInRow;
                streak++;
                fallDayInRow = 0;
                if (streak % 7 == 0)
                    fallDayInWeek = 0;
            }
            else if (fallDayInWeek < maxGap)
            {
                totalFallDays++;
                fallDayInWeek++;
                fallDayInRow++;
            }
            else
            {
                break;
            }
        }

        return new GoalStreakAnalyze(streak, fallDayInRow != totalFallDays);
    }

    public static CalendarItem[] GetCalendar(this UserModel user) =>
        user.LastDaysStats
            .Select(d => new CalendarItem(d.Date, d.LearningDone, d.GameScoreChanging)).ToArray();

    public static Dictionary<int, DayGoalResult> GetOffsets(this CalendarItem[] items, int examsCountGoalForDay)
    {
        var today = DateTime.Today;

        return items.ToDictionary(
            i => (int)(today - i.Date.Date).TotalDays,
            k => (k.ExamsCount / (double)examsCountGoalForDay) switch
            {
                < 0.1 => DayGoalResult.S1,
                < 0.2 => DayGoalResult.S2,
                < 0.5 => DayGoalResult.S3,
                < 1 => DayGoalResult.S4,
                < 2 => DayGoalResult.Goal,
                _ => DayGoalResult.Overreaching
            }
        );
    }
}

public record GoalStreakAnalyze(int GoalStreakCount, bool hasGap);

public enum DayGoalResult
{
    S1,
    S2,
    S3,
    S4,
    Goal,
    Overreaching
}
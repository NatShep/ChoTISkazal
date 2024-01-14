using System;
using Chotiskazal.Bot.ChatFlows;
using NUnit.Framework;

namespace SayWhat.UnitTests;

public class StatsTest
{
    [Test]
    public void GoalStreak1() => AssertGoalStreak(6, false,
        Day(0, true),
        Day(1, true),
        Day(2, true),
        Day(3, true),
        Day(4, true),
        Day(5, true));

    [Test]
    public void GoalStreak2() =>
        AssertGoalStreak(9, true,
            Day(0, false),
            Day(1, true),
            Day(2, true),
            Day(3, true),
            Day(4, true),
            Day(5, true),
            Day(6, true),
            Day(7, true),
            Day(8, true));

    [Test]
    public void GoalStreak3() =>
        AssertGoalStreak(9, true,
            Day(0, false),
            Day(1, true),
            Day(2, true),
            Day(3, true),
            Day(4, true),
            Day(5, true),
            Day(6, false),
            Day(7, true),
            Day(8, true));


    [Test]
    public void GoalStreak4() =>
        AssertGoalStreak(6, true,
            Day(0, true),
            Day(1, true),
            Day(2, false),
            Day(3, true),
            Day(4, false),
            Day(5, true),
            Day(6, false),
            Day(7, true),
            Day(8, true));

    [Test]
    public void GoalStreak5() =>
        AssertGoalStreak(9, true,
            Day(0, true),
            Day(1, true),
            Day(2, true),
            Day(3, true),
            Day(4, false),
            Day(5, false),
            Day(6, true),
            Day(7, true),
            Day(8, true));

    [Test]
    public void GoalStreak6() =>
        AssertGoalStreak(4, true,
            Day(0, false),
            Day(1, false),
            Day(2, true),
            Day(3, true),
            Day(4, false),
            Day(5, false),
            Day(6, true),
            Day(7, true),
            Day(8, true)
        );

    [Test]
    public void GoalStreak7() =>
        AssertGoalStreak(5, true,
            Day(0, true),
            Day(1, false),
            Day(2, true),
            Day(3, true),
            Day(4, true),
            Day(5, false),
            Day(6, false),
            Day(7, false),
            Day(8, true)
        );

    [Test]
    public void GoalStreak8() => AssertGoalStreak(0, false);

    [Test]
    public void GoalStreak9() =>
        AssertGoalStreak(6, false,
            Day(0, true),
            Day(1, true),
            Day(2, true),
            Day(3, true),
            Day(4, true),
            Day(5, true),
            Day(6, false),
            Day(7, false),
            Day(8, false)
        );

    [Test]
    public void GoalStreak10() =>
        AssertGoalStreak(7, false,
            Day(0, true),
            Day(1, true),
            Day(2, true),
            Day(3, true),
            Day(4, true),
            Day(5, true),
            Day(6, true),
            Day(7, false),
            Day(8, false),
            Day(9, false)
        );

    [Test]
    public void GoalStreak11() =>
        AssertGoalStreak(4, false,
            Day(0, true),
            Day(1, true),
            Day(2, true),
            Day(3, true),
            Day(4, false),
            Day(5, false),
            Day(6, false),
            Day(7, false),
            Day(8, true));

    [Test]
    public void GoalStreak12() =>
        AssertGoalStreak(1, false,
            Day(0, true),
            Day(1, false),
            Day(2, false),
            Day(3, false),
            Day(4, true),
            Day(5, true)
        );

    [Test]
    public void GoalStreak13() =>
        AssertGoalStreak(2, true,
            Day(0, false),
            Day(1, true),
            Day(2, false),
            Day(3, false),
            Day(4, true),
            Day(5, true)
        );

    [Test]
    public void GoalStreak14() =>
        AssertGoalStreak(0, false,
            Day(0, false),
            Day(1, false),
            Day(2, false),
            Day(3, false),
            Day(4, true),
            Day(5, true)
        );

    [Test]
    public void GoalStreak15() =>
        AssertGoalStreak(15, true,
            Day(0, true),
            Day(1, true),
            Day(2, true),
            Day(3, true),
            Day(4, false),
            Day(5, false),
            Day(6, true),
            Day(7, true),
            Day(8, true),
            Day(9, false),
            Day(10, true),
            Day(11, false),
            Day(12, true),
            Day(13, true),
            Day(14, true),
            Day(15, false),
            Day(16, false)
        );
    
    private readonly DateTime _today = DateTime.Today;

    private CalendarItem Day(int dayOffset, bool examsCount) =>
        new(_today.AddDays(-dayOffset), examsCount ? 20 : 0, 0);

    private void AssertGoalStreak(int streak, bool gaps, params CalendarItem[] items)
    {
        var (actualStreak, actualHasGaps) = StatsHelper.GetGoalsStreak(items, 10);
        Assert.AreEqual(streak, actualStreak, "invalid goal streak count");
        Assert.AreEqual(gaps, actualHasGaps, "invalid has gaps");
    }

}
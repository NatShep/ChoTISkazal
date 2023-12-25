using System;
using Chotiskazal.Bot.ChatFlows;
using NUnit.Framework;

namespace SayWhat.UnitTests;

public class StatsTest
{
    [Test]
    public void GoalStreak1()
    {
        var today = DateTime.Today;
        var calendar = new[]
        {
            new CalendarItem(today, 20, 0),
            new CalendarItem(today.AddDays(-1), 20, 0),
            new CalendarItem(today.AddDays(-2), 20, 0),
            new CalendarItem(today.AddDays(-3), 20, 0),
            new CalendarItem(today.AddDays(-4), 20, 0),
            new CalendarItem(today.AddDays(-5), 20, 0),
        };
        var (streak, hasgaps)= StatsHelper.GetGoalsStreak(calendar, 10);
        Assert.AreEqual(6, streak);
        Assert.IsFalse(hasgaps);
    }
    
    [Test]
    public void GoalStreak2()
    {
        var today = DateTime.Today;
        var calendar = new[]
        {
            new CalendarItem(today, 0, 0),
            new CalendarItem(today.AddDays(-1), 20, 0),
            new CalendarItem(today.AddDays(-2), 20, 0),
            new CalendarItem(today.AddDays(-3), 20, 0),
            new CalendarItem(today.AddDays(-4), 20, 0),
            new CalendarItem(today.AddDays(-5), 20, 0),
            new CalendarItem(today.AddDays(-6), 20, 0),
            new CalendarItem(today.AddDays(-7), 20, 0),
            new CalendarItem(today.AddDays(-8), 20, 0),
        };
        var (streak, hasgaps)= StatsHelper.GetGoalsStreak(calendar, 10);
        Assert.AreEqual(9, streak);
        Assert.IsTrue(hasgaps);
    }
    
    [Test]
    public void GoalStreak3()
    {
        var today = DateTime.Today;
        var calendar = new[]
        {
            new CalendarItem(today, 0, 0),
            new CalendarItem(today.AddDays(-1), 20, 0),
            new CalendarItem(today.AddDays(-2), 20, 0),
            new CalendarItem(today.AddDays(-3), 20, 0),
            new CalendarItem(today.AddDays(-4), 20, 0),
            new CalendarItem(today.AddDays(-5), 20, 0),
            new CalendarItem(today.AddDays(-6), 0, 0),
            new CalendarItem(today.AddDays(-7), 20, 0),
            new CalendarItem(today.AddDays(-8), 20, 0),
        };
        var (streak, hasgaps)= StatsHelper.GetGoalsStreak(calendar, 10);
        Assert.AreEqual(9, streak);
        Assert.IsTrue(hasgaps);
    }
    
    
    [Test]
    public void GoalStreak4()
    {
        var today = DateTime.Today;
        var calendar = new[]
        {
            new CalendarItem(today, 20, 0),
            new CalendarItem(today.AddDays(-1), 20, 0),
            new CalendarItem(today.AddDays(-2), 0, 0),
            new CalendarItem(today.AddDays(-3), 20, 0),
            new CalendarItem(today.AddDays(-4), 0, 0),
            new CalendarItem(today.AddDays(-5), 20, 0),
            new CalendarItem(today.AddDays(-6), 0, 0),
            new CalendarItem(today.AddDays(-7), 20, 0),
            new CalendarItem(today.AddDays(-8), 20, 0),
        };
        var (streak, hasgaps)= StatsHelper.GetGoalsStreak(calendar, 10);
        Assert.AreEqual(6, streak);
        Assert.IsTrue(hasgaps);
    }
    
    [Test]
    public void GoalStreak5()
    {
        var today = DateTime.Today;
        var calendar = new[]
        {
            new CalendarItem(today, 20, 0),
            new CalendarItem(today.AddDays(-1), 20, 0),
            new CalendarItem(today.AddDays(-2), 20, 0),
            new CalendarItem(today.AddDays(-3), 20, 0),
            new CalendarItem(today.AddDays(-4), 0, 0),
            new CalendarItem(today.AddDays(-5), 0, 0),
            new CalendarItem(today.AddDays(-6), 20, 0),
            new CalendarItem(today.AddDays(-7), 20, 0),
            new CalendarItem(today.AddDays(-8), 20, 0),
        };
        var (streak, hasgaps)= StatsHelper.GetGoalsStreak(calendar, 10);
        Assert.AreEqual(9, streak);
        Assert.IsTrue(hasgaps);
    }
    
    [Test]
    public void GoalStreak6()
    {
        var today = DateTime.Today;
        var calendar = new[]
        {
            new CalendarItem(today, 0, 0),
            new CalendarItem(today.AddDays(-1), 0, 0),
            new CalendarItem(today.AddDays(-2), 20, 0),
            new CalendarItem(today.AddDays(-3), 20, 0),
            new CalendarItem(today.AddDays(-4), 0, 0),
            new CalendarItem(today.AddDays(-5), 0, 0),
            new CalendarItem(today.AddDays(-6), 20, 0),
            new CalendarItem(today.AddDays(-7), 20, 0),
            new CalendarItem(today.AddDays(-8), 20, 0),
        };
        var (streak, hasgaps)= StatsHelper.GetGoalsStreak(calendar, 10);
        Assert.AreEqual(4, streak);
        Assert.IsTrue(hasgaps);
    }
    
    [Test]
    public void GoalStreak7()
    {
        var today = DateTime.Today;
        var calendar = new[]
        {
            new CalendarItem(today, 20, 0),
            new CalendarItem(today.AddDays(-1), 0, 0),
            new CalendarItem(today.AddDays(-2), 20, 0),
            new CalendarItem(today.AddDays(-3), 20, 0),
            new CalendarItem(today.AddDays(-4), 20, 0),
            new CalendarItem(today.AddDays(-5), 0, 0),
            new CalendarItem(today.AddDays(-6), 0, 0),
            new CalendarItem(today.AddDays(-7), 0, 0),
            new CalendarItem(today.AddDays(-8), 20, 0),
        };
        var (streak, hasgaps)= StatsHelper.GetGoalsStreak(calendar, 10);
        Assert.AreEqual(5, streak);
        Assert.IsTrue(hasgaps);
    }
    
    [Test]
    public void GoalStreak8()
    {
        var (streak, hasgaps)= StatsHelper.GetGoalsStreak(new CalendarItem[0], 10);
        Assert.AreEqual(0, streak);
        Assert.IsFalse(hasgaps);
    }
    
    [Test]
    public void GoalStreak9()
    {
        var today = DateTime.Today;
        var calendar = new[]
        {
            new CalendarItem(today, 20, 0),
            new CalendarItem(today.AddDays(-1), 20, 0),
            new CalendarItem(today.AddDays(-2), 20, 0),
            new CalendarItem(today.AddDays(-3), 20, 0),
            new CalendarItem(today.AddDays(-4), 20, 0),
            new CalendarItem(today.AddDays(-5), 20, 0),
            new CalendarItem(today.AddDays(-6), 0, 0),
            new CalendarItem(today.AddDays(-7), 0, 0),
            new CalendarItem(today.AddDays(-8), 0, 0),
        };
        var (streak, hasgaps)= StatsHelper.GetGoalsStreak(calendar, 10);
        Assert.AreEqual(6, streak);
        Assert.IsFalse(hasgaps);
    }
    
    [Test]
    public void GoalStreak10()
    {
        var today = DateTime.Today;
        var calendar = new[]
        {
            new CalendarItem(today, 20, 0),
            new CalendarItem(today.AddDays(-1), 20, 0),
            new CalendarItem(today.AddDays(-2), 20, 0),
            new CalendarItem(today.AddDays(-3), 20, 0),
            new CalendarItem(today.AddDays(-4), 20, 0),
            new CalendarItem(today.AddDays(-5), 20, 0),
            new CalendarItem(today.AddDays(-6), 20, 0),
            new CalendarItem(today.AddDays(-7), 0, 0),
            new CalendarItem(today.AddDays(-8), 0, 0),
            new CalendarItem(today.AddDays(-9), 0, 0),
        };
        var (streak, hasgaps)= StatsHelper.GetGoalsStreak(calendar, 10);
        Assert.AreEqual(7, streak);
        Assert.IsFalse(hasgaps);
    }
    
    [Test]
    public void GoalStreak11()
    {
        var today = DateTime.Today;
        var calendar = new[]
        {
            new CalendarItem(today, 20, 0),
            new CalendarItem(today.AddDays(-1), 20, 0),
            new CalendarItem(today.AddDays(-2), 20, 0),
            new CalendarItem(today.AddDays(-3), 20, 0),
            new CalendarItem(today.AddDays(-4), 0, 0),
            new CalendarItem(today.AddDays(-5), 0, 0),
            new CalendarItem(today.AddDays(-6), 0, 0),
            new CalendarItem(today.AddDays(-7), 0, 0),
            new CalendarItem(today.AddDays(-8), 20, 0),
        };
        var (streak, hasGaps)= StatsHelper.GetGoalsStreak(calendar, 10);
        Assert.AreEqual(4, streak);
        Assert.IsFalse(hasGaps);
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Chotiskazal.Bot.Texts;
using SayWhat.Bll.Services;
using SayWhat.Bll.Strings;
using SayWhat.MongoDAL;
using SayWhat.MongoDAL.Words;

namespace Chotiskazal.Bot.ChatFlows.FlowLearning;

public static class ExamResultHelper
{
    public static Markdown CreateLearningResultsMessage(
        ChatRoom chat,
        ExamSettings examSettings,
        UserWordModel[] wordsInExam,
        Dictionary<string, double> originWordsScore,
        int questionsPassed,
        int questionsCount
    )
    {
        var message = Markdown.Escaped($"{chat.Texts.LearningDone}:").ToSemiBold()
                          .AddEscaped($" {questionsPassed}/{questionsCount}")
                          .NewLine() +
                      Markdown.Escaped($"{chat.Texts.WordsInTestCount}:").ToSemiBold()
                          .AddEscaped($" {wordsInExam.Length}")
                          .NewLine();

        var forgottenWords = new List<UserWordModel>();
        var newWellLearnedWords = new List<UserWordModel>();

        foreach (var word in wordsInExam)
        {
            if (word.AbsoluteScore >= WordLeaningGlobalSettings.WellDoneWordMinScore)
            {
                if (originWordsScore[word.Word] < WordLeaningGlobalSettings.WellDoneWordMinScore)
                    newWellLearnedWords.Add(word);
            }
            else
            {
                if (originWordsScore[word.Word] > WordLeaningGlobalSettings.WellDoneWordMinScore)
                    forgottenWords.Add(word);
            }
        }

        message += GetLearnWordMessage(chat.Texts, newWellLearnedWords);
        message += GetForgottenWordMessage(chat.Texts, forgottenWords);
        message += GetGoalStreakMessage(chat, examSettings);
        return message;
    }

    private static Markdown GetGoalStreakMessage(ChatRoom chat, ExamSettings examSettings)
    {
        var todayStats = chat.User.GetToday();

        var message = Markdown.Empty.NewLine() +
                      Markdown
                          .Escaped(
                              $"{chat.Texts.TodaysGoal}: {todayStats.LearningDone}/{examSettings.ExamsCountGoalForDay} {chat.Texts.Exams}")
                          .ToSemiBold()
                          .NewLine();

        if (todayStats.LearningDone >= examSettings.ExamsCountGoalForDay)
        {
            message = message
                .AddEscaped($"{Emojis.GreenCircle} {chat.Texts.TodayGoalReached}")
                .NewLine();
            var (goalStreak, hasGap) = StatsHelper.GetGoalsStreak(
                chat.User.GetCalendar(),
                examSettings.ExamsCountGoalForDay);
            if (goalStreak > 1)
                message += chat.Texts.YouHaveGoalStreak(goalStreak, hasGap);
        }

        return message;
    }

    private static Markdown GetLearnWordMessage(IInterfaceTexts texts, List<UserWordModel> newWellLearnedWords)
    {
        var message = Markdown.Empty;
        if (!newWellLearnedWords.Any()) return message;
        if (newWellLearnedWords.Count > 1)
            message = message.NewLine() +
                      texts.LearnMoreWords(newWellLearnedWords.Count).ToSemiBold().AddEscaped(":")
                          .NewLine();
        else
            message = message.NewLine() +
                      Markdown
                          .Escaped($"{texts.YouHaveLearnedOneWord}:").ToSemiBold()
                          .NewLine();

        foreach (var word in newWellLearnedWords)
        {
            message = message
                .AddEscaped($"{Emojis.HeavyPlus} ")
                .AddEscaped(word.Word)
                .NewLine();
        }

        return message;
    }

    private static Markdown GetForgottenWordMessage(IInterfaceTexts texts, List<UserWordModel> forgottenWords)
    {
        var message = Markdown.Empty;
        if (!forgottenWords.Any()) return message;
        if (forgottenWords.Count > 1)
            message = message
                .NewLine()
                .AddEscaped($"{texts.YouForgotCountWords(forgottenWords.Count)}:").ToSemiBold()
                .NewLine();
        else
            message = message
                .NewLine()
                .AddEscaped($"{texts.YouForgotOneWord}:").ToSemiBold()
                .NewLine();

        foreach (var word in forgottenWords)
        {
            message = message
                .AddEscaped($"{Emojis.HeavyMinus} ")
                .AddEscaped(word.Word)
                .NewLine();
        }

        return message;
    }
}
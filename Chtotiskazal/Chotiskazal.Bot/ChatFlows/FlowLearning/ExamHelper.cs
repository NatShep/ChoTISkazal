using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Chotiskazal.Bot.ConcreteQuestions;
using Chotiskazal.Bot.Questions;
using Chotiskazal.Bot.Texts;
using SayWhat.Bll.Services;
using SayWhat.Bll.Strings;
using SayWhat.MongoDAL;
using SayWhat.MongoDAL.Words;
using Telegram.Bot.Types.ReplyMarkups;

namespace Chotiskazal.Bot.ChatFlows.FlowLearning;

public static class ExamHelper
{
    public static Markdown GetCarefullyLearnTheseWordsMessage(
        IInterfaceTexts texts, ExamType examType, UserWordModel[] newLearningWords)
    {
        var markdown = Markdown.Escaped($"{Emojis.Learning}").ToSemiBold().Space();

        markdown += examType == ExamType.NoInput
            ? Markdown.Escaped(texts.FastExamLearningHeader).ToSemiBold().NewLine()
            : Markdown.Escaped(texts.WriteExamLearningHeader).ToSemiBold();

        markdown = markdown.NewLine() + texts.CarefullyStudyTheList
            .NewLine()
            .NewLine();

        var messageWithListOfWords = newLearningWords.Shuffle()
            .Aggregate(Markdown.Empty, (current, pairModel) =>
                current + Markdown.Escaped($"{pairModel.Word}\t\t:{pairModel.AllTranslationsAsSingleString}\r\n"));
        markdown += messageWithListOfWords.ToQuotationMono();
        return markdown;
    }

    public static Markdown GetExamStartsMessage(
        IInterfaceTexts texts, ExamType examType, UserWordModel[] newLearningWords)
    {
        var markdown = GetCarefullyLearnTheseWordsMessage(texts, examType, newLearningWords);
        markdown = markdown.NewLine().AddEscaped($"... {texts.thenClickStart}");

        if (examType != ExamType.NoInput)
        {
            markdown = markdown.NewLine().NewLine() +
                       texts.TipYouCanEnterCommandIfYouDontKnowTheAnswerForWriteExam(
                           QuestionScenarioHelper.IDontKnownSubcommand);
        }

        return markdown;
    }

    public static InlineKeyboardButton[][] GetStartExaminationButtons(IInterfaceTexts texts) =>
        new[]
        {
            new[]
            {
                InlineButtons.StartExaminationButton(texts),
                InlineButtons.Button(texts.CancelButton, BotCommands.Start)
            }
        };

    public static async Task SendMotivationMessages(ChatRoom chat, ExamSettings settings, ExamResults examResults)
    {
        if (examResults.QuestionsCount == examResults.QuestionsPassed)
            await SendMotivation(chat, "\U0001F973", chat.Texts.CongratulateAllQuestionPassed);

        if (chat.User.GetToday().LearningDone == settings.ExamsCountGoalForDay - 2)
            await SendMotivation(chat, "\U0001F90F", chat.Texts.TwoExamsToGoal);

        if (chat.User.GetToday().LearningDone == settings.ExamsCountGoalForDay)
            await SendMotivation(chat, "\U00002705", Markdown.Escaped(chat.Texts.TodayGoalReached));
    }

    private static async Task SendMotivation(ChatRoom chat, string emoji, Markdown message)
    {
        await chat.SendMessageAsync(emoji);
        await chat.SendMarkdownMessageAsync(message, InlineButtons.Button(chat.Texts.ContinueButton, "/$continue"));
        await chat.WaitInlineKeyboardInput();
    }

    public static InlineKeyboardButton[][] GetButtonsForExamResultMessage(IInterfaceTexts texts) =>
        new[]
        {
            new[] { InlineButtons.Learn($"🔁 {texts.OneMoreLearnButton}") },
            new[]
            {
                InlineButtons.Stats(texts),
                InlineButtons.Translation(texts)
            }
        };

    public static Markdown CreateLearningResultsMessage(ChatRoom chat, ExamSettings examSettings, ExamResults results)
    {
        var message = Markdown.Escaped($"{chat.Texts.LearningDone}:").ToSemiBold()
                          .AddEscaped($" {results.QuestionsPassed}/{results.QuestionsCount}")
                          .NewLine() +
                      Markdown.Escaped($"{chat.Texts.WordsInTestCount}:").ToSemiBold()
                          .AddEscaped($" {results.Words.Length}")
                          .NewLine();

        var forgottenWords = new List<UserWordModel>();
        var newWellLearnedWords = new List<UserWordModel>();

        foreach (var word in results.Words)
        {
            if (word.AbsoluteScore >= WordLeaningGlobalSettings.WellDoneWordMinScore)
            {
                if (results.OriginWordsScore[word.Word] < WordLeaningGlobalSettings.WellDoneWordMinScore)
                    newWellLearnedWords.Add(word);
            }
            else
            {
                if (results.OriginWordsScore[word.Word] > WordLeaningGlobalSettings.WellDoneWordMinScore)
                    forgottenWords.Add(word);
            }
        }

        message += GetLearnWordMessage(chat.Texts, newWellLearnedWords);
        message += GetForgottenWordMessage(chat.Texts, forgottenWords);
        message += GetGoalStreakMessage(chat, examSettings);
        return message;
    }

    public static Markdown GetGoalStreakMessage(ChatRoom chat, ExamSettings examSettings)
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
            var (goalStreak, hasGap) = StatsHelper.GetCurrentGoalsStreak(
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
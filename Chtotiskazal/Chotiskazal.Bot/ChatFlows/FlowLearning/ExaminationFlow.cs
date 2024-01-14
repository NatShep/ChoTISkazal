using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Chotiskazal.Bot.Questions;
using SayWhat.Bll;
using SayWhat.Bll.Services;
using SayWhat.Bll.Strings;
using SayWhat.MongoDAL;
using SayWhat.MongoDAL.QuestionMetrics;
using SayWhat.MongoDAL.Words;

namespace Chotiskazal.Bot.ChatFlows.FlowLearning;

public class ExaminationFlow
{
    private readonly ExamSettings _regularExamSettings;
    private readonly QuestionSelector _questionSelector;
    private ChatRoom Chat { get; }
    private readonly UserService _userService;
    private readonly UsersWordsService _usersWordsService;
    private readonly ExamSettings _noInputExamSettings;

    public ExaminationFlow(
        ChatRoom chat,
        UserService userService,
        UsersWordsService usersWordsService,
        ExamSettings regularExamSettings,
        QuestionSelector questionSelector)
    {
        Chat = chat;
        _userService = userService;
        _usersWordsService = usersWordsService;
        _regularExamSettings = regularExamSettings;
        _noInputExamSettings = new ExamSettings
        {
            MinWordsQuestionsInOneExam = _regularExamSettings.MinWordsQuestionsInOneExam + 1,
            MaxWordsQuestionsInOneExam = (int)(_regularExamSettings.MaxWordsQuestionsInOneExam * 1.5),
            CountOfVariantsForChoose = _regularExamSettings.CountOfVariantsForChoose,
            ExamsCountGoalForDay = _regularExamSettings.ExamsCountGoalForDay,
            LearnedWordsInOneExam = _regularExamSettings.LearnedWordsInOneExam,
            MaxExamSize = (int)(_regularExamSettings.MaxExamSize * 1.1),
            LearningWordsInOneExam = (int)(_regularExamSettings.LearnedWordsInOneExam * 1.5),
            MaxTranslationsInOneExam = _regularExamSettings.MaxTranslationsInOneExam,
            WellDoneWordsInOneExam = (int)(_regularExamSettings.WellDoneWordsInOneExam * 1.5),
            NewWordInOneExam = (int)(_regularExamSettings.NewWordInOneExam * 1.5)
        };
        _questionSelector = questionSelector;
    }

    public async Task EnterAsync()
    {
        if (!await _usersWordsService.HasWordsFor(Chat.User))
        {
            await Chat.SendMessageAsync(Chat.Texts.NeedToAddMoreWordsBeforeLearning);
            return;
        }

        // The idea is:
        // Either you input only with buttons,
        // Either you input only ru input or buttons (so you need no to switch language on desktop)
        // Either you input only en input or buttons (so you need no to switch language on desktop)
        var examType = Rand.GetRandomItem(
            ExamType.NoInput,     // Input exams are twice more often then no input exam
            ExamType.EnInputOnly,   
            ExamType.EnInputOnly, 
            ExamType.RuInputOnly,
            ExamType.RuInputOnly
            );

        var examSettings = GetExamSetings(examType);

        var wellDoneWords =
            await _usersWordsService.GetWellDoneWords(Chat.User, examSettings.WellDoneWordsInOneExam,
                examSettings.MaxTranslationsInOneExam);

        var freeSpaceForWords = 0;
        if (wellDoneWords.Length < examSettings.WellDoneWordsInOneExam)
            freeSpaceForWords = examSettings.WellDoneWordsInOneExam - wellDoneWords.Length;

        var learningWords = await _usersWordsService.GetLearningWords(Chat.User,
            examSettings.LearningWordsInOneExam + freeSpaceForWords, examSettings.MaxTranslationsInOneExam);

        if (learningWords.Length < examSettings.WellDoneWordsInOneExam + freeSpaceForWords)
            freeSpaceForWords = examSettings.WellDoneWordsInOneExam + examSettings.NewWordInOneExam -
                                learningWords.Length;
        else
            freeSpaceForWords = 0;

        var newLearningWords = await _usersWordsService.GetBeginnerWords(Chat.User,
            examSettings.NewWordInOneExam + freeSpaceForWords,
            examSettings.MaxTranslationsInOneExam);

        var markdown = ExamHelper.GetExamStartsMessage(Chat.Texts, examType, newLearningWords);
        await Chat.SendMarkdownMessageAsync(markdown, ExamHelper.GetStartExaminationButtons(Chat.Texts));

        var needToContinue = await Chat.WaitInlineKeyboardInput();
        if (needToContinue != InlineButtons.StartExaminationButtonData)
            return;

        var learnedWords = await _usersWordsService.GetWordsWithPhrasesAsync(
            Chat.User,
            examSettings.LearnedWordsInOneExam,
            WordSortingType.Ascending,
            WordLeaningGlobalSettings.LearnedWordMinScore,
            maxTranslations: examSettings.MaxTranslationsInOneExam);


        //TODO before this values used for count questions for advanced words. Maybe they are important. Maybe not
        /*

         var timesThatWordHasToBeAsked =
            Rand.RandomIn(_examSettings.MinAdvancedExamMinQuestionAskedForOneWordCount,
                _examSettings.MaxAdvancedExamMinQuestionAskedForOneWordCount);
        Console.WriteLine($":Непонятный подсчте: Желательное количество повтора экзаменов слов {timesThatWordHasToBeAsked}");

         var minimumTimesThatWordHasToBeAsked =
            Rand.RandomIn(_examSettings.MinAdvancedExamMinQuestionAskedForOneWordCount,
                _examSettings.MaxAdvancedExamMinQuestionAskedForOneWordCount);

        var advancedlistMaxCountQuestions = Math.Min(Rand.RandomIn(_examSettings.MinAdvancedQuestionsCount, _examSettings.MaxAdvancedQuestionsCount),
            _examSettings.MaxExamSize - examsWords.Count);

        if (advancedlistMaxCountQuestions <= _examSettings.MinAdvancedQuestionsCount)
            advancedlistMaxCountQuestions = _examSettings.MinAdvancedQuestionsCount;

        */

        var distinctLearningWords = newLearningWords.ToList()
            .Union(learningWords)
            .Union(wellDoneWords)
            .Union(learnedWords)
            .ToArray();

        var results = await DoExam(examType, distinctLearningWords, examSettings.MaxExamSize);
        await ExamHelper.SendMotivationMessages(Chat, _regularExamSettings, results);
        var doneMessage = ExamHelper.CreateLearningResultsMessage(Chat, _regularExamSettings, results);
        await Chat.SendMarkdownMessageAsync(doneMessage, ExamHelper.GetButtonsForExamResultMessage(Chat.Texts));
    }
    
    public async Task<ExamResults> DoExam(ExamType examType, UserWordModel[] words, int maxQuestionCount)
    {
        var examSettings = GetExamSetings(examType);
        var examsLearnedWords = CreateExamListForNewWords(
            words,
            examSettings.MinWordsQuestionsInOneExam,
            examSettings.MaxWordsQuestionsInOneExam,
            maxQuestionCount);

        var originWordsScore = new Dictionary<string, double>();
        foreach (var word in words)
        {
            if (!originWordsScore.ContainsKey(word.Word))
                originWordsScore.Add(word.Word, word.AbsoluteScore);
        }

        //begin testing
        var started = DateTime.Now;

        var questionsCount = 0;
        var questionsPassed = 0;
        var wordQuestionNumber = 0;
        QuestionResult lastExamResult = null;

        var previousWord = new UserWordModel();
        foreach (var word in examsLearnedWords.Shuffle())
        {
            //exclude the next same exam
            if (previousWord == word)
                continue;
            previousWord = word;

            var question = _questionSelector.GetNextQuestionFor(word, examType);
            wordQuestionNumber++;

            var learnList = words;

            if (!words.Contains(word))
                learnList = words.Append(word).ToArray();

            if (wordQuestionNumber > 1 && question.NeedClearScreen)
                await WriteDontPeakMessage(lastExamResult?.ResultsBeforeHideousTextMarkdown.GetOrdinalString());

            var originRate = word.Score;

            var questionMetric = new QuestionMetric(word, question.Name);
            var result = await PassWithRetries(question, word, learnList, examType);
            switch (result.Results)
            {
                case QResult.Passed:
                    var succTask = _usersWordsService.RegisterSuccess(word, question.PassScore);
                    questionsCount++;
                    questionsPassed++;
                    questionMetric.OnExamFinished(word.Score, true);
                    Reporter.ReportQuestionDone(questionMetric, Chat.ChatId.ToString(), question.Name);
                    await succTask;
                    Chat.User.OnQuestionPassed(word.Score - originRate);
                    break;
                case QResult.Failed:
                    var failureTask = _usersWordsService.RegisterFailure(word, question.FailScore);
                    questionMetric.OnExamFinished(word.Score, false);
                    Reporter.ReportQuestionDone(questionMetric, Chat.ChatId.ToString(), question.Name);
                    questionsCount++;
                    await failureTask;
                    Chat.User.OnQuestionFailed(word.Score - originRate);
                    break;
                case QResult.Retry:
                case QResult.Impossible:
                    throw new NotSupportedException(result.Results.ToString());
            }

            if (!result.OpenResultsTextMarkdown.IsNullOrEmpty())
                await Chat.SendMarkdownMessageAsync(result.OpenResultsTextMarkdown);

            lastExamResult = result;

            Reporter.ReportExamPassed(Chat.User.TelegramId, started, questionsCount, questionsPassed);
        }

        Chat.User.OnLearningDone();
        var (goalStreak, _) = StatsHelper.GetCurrentGoalsStreak(
            Chat.User.GetCalendar(),
            _regularExamSettings.ExamsCountGoalForDay);

        if (goalStreak > Chat.User.MaxGoalStreak)
            Chat.User.MaxGoalStreak = goalStreak;

        await _userService.Update(Chat.User);
        return new ExamResults(words, originWordsScore, questionsPassed, questionsCount);
    }
    
    private ExamSettings GetExamSetings(ExamType examType)
    {
        var examSettings = examType == ExamType.NoInput ? _noInputExamSettings : _regularExamSettings;
        return examSettings;
    }


    private async Task<QuestionResult> PassWithRetries(Question question,
        UserWordModel word,
        UserWordModel[] learnList,
        ExamType examType)
    {
        int retrieNumber = 0;
        for (int i = 0; i < 40; i++)
        {
            var result = await question.Scenario.Pass(Chat, word, learnList);
            if (result.Results == QResult.Impossible)
            {
                var qName = question.Name;
                for (int iteration = 0; question.Name == qName; iteration++)
                {
                    question = _questionSelector.GetNextQuestionFor(word, examType);
                    if (iteration > 100)
                        return QuestionResult.Failed(Markdown.Empty, Markdown.Empty);
                }
            }
            else if (result.Results == QResult.Retry)
            {
                retrieNumber++;
                if (retrieNumber >= 4)
                    return QuestionResult.Failed(Markdown.Empty, Markdown.Empty);
            }
            else return result;
        }

        return QuestionResult.Failed(Markdown.Empty, Markdown.Empty);
    }


    private async Task WriteDontPeakMessage(string resultsBeforeHideousText)
    {
        //it is not an empty string;
        // it contains invisible character, that allows to show blank message
        string emptySymbol = "‎";

        await Chat.SendMessageAsync(
            $"\r\n\r\n{emptySymbol}‎\r\n{emptySymbol}‎\r\n{emptySymbol}\r\n{emptySymbol}\r\n{emptySymbol}\r\n{emptySymbol}\r\n{emptySymbol}\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n{resultsBeforeHideousText}\r\n\r\n" +
            $"{Chat.Texts.DontPeekUpward}\r\n");
        await Chat.SendMessageAsync("\U0001F648");
    }

    private List<UserWordModel> CreateExamListForNewWords(UserWordModel[] learningWords,
        int minWordsQuestionsInOneExam, int maxWordsQuestionsInOneExam, int maxExamSize)
    {
        var examsList = new List<UserWordModel>(maxExamSize);

        //Every learning word appears in exam from MIN to MAX times
        for (int i = 0; i < minWordsQuestionsInOneExam; i++)
            examsList.AddRange(learningWords);
        for (int i = 0; i < maxWordsQuestionsInOneExam - minWordsQuestionsInOneExam; i++)
            examsList.AddRange(learningWords.Where(w => Rand.Next() % 2 == 0));
        while (examsList.Count > maxExamSize)
            examsList.RemoveAt(examsList.Count - 1);
        return examsList;
    }
}

public record ExamResults(
    UserWordModel[] Words,
    Dictionary<string, double> OriginWordsScore,
    int QuestionsPassed,
    int QuestionsCount);
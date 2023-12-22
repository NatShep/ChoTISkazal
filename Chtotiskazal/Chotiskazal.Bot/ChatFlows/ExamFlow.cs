using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Chotiskazal.Bot.ConcreteQuestions;
using Chotiskazal.Bot.Questions;
using SayWhat.Bll;
using SayWhat.Bll.Services;
using SayWhat.Bll.Strings;
using SayWhat.MongoDAL;
using SayWhat.MongoDAL.QuestionMetrics;
using SayWhat.MongoDAL.Words;

namespace Chotiskazal.Bot.ChatFlows;

public class ExamFlow {
    private readonly ExamSettings _regularExamSettings;
    private readonly QuestionSelector _questionSelector;
    private ChatRoom Chat { get; }
    private readonly UserService _userService;
    private readonly UsersWordsService _usersWordsService;
    private readonly ExamSettings _noInputExamSettings;

    public ExamFlow(
        ChatRoom chat,
        UserService userService,
        UsersWordsService usersWordsService,
        ExamSettings regularExamSettings,
        QuestionSelector questionSelector) {
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

    public async Task EnterAsync() {
        if (!await _usersWordsService.HasWordsFor(Chat.User)) {
            await Chat.SendMessageAsync(Chat.Texts.NeedToAddMoreWordsBeforeLearning);
            return;
        }

        // The idea is:
        // Either you input only with buttons,
        // Either you input only ru input or buttons (so you need no to switch language on desktop)
        // Either you input only en input or buttons (so you need no to switch language on desktop)
        var examType = Rand.GetRandomItem(
            ExamType.NoInput,
            ExamType.EnInputOnly,
            ExamType.RuInputOnly);

        var examSettings = examType == ExamType.NoInput ? _noInputExamSettings : _regularExamSettings;

        var wellDoneWords = await _usersWordsService.GetWordsWithPhrasesAsync(
            Chat.User,
            examSettings.WellDoneWordsInOneExam,
            examSettings.MaxTranslationsInOneExam,
            CurrentScoreSortingType.LongAsked,
            WordLeaningGlobalSettings.WellDoneWordMinScore,
            WordLeaningGlobalSettings.LearnedWordMinScore);

        var freeSpaceForWords = 0;
        if (wellDoneWords.Length < examSettings.WellDoneWordsInOneExam) {
            freeSpaceForWords = examSettings.WellDoneWordsInOneExam - wellDoneWords.Length;
        }

        var learningWords = await _usersWordsService.GetWordsWithPhrasesAsync(
            Chat.User,
            examSettings.LearningWordsInOneExam + freeSpaceForWords,
            examSettings.MaxTranslationsInOneExam,
            CurrentScoreSortingType.LongAsked,
            WordLeaningGlobalSettings.LearningWordMinScore,
            WordLeaningGlobalSettings.WellDoneWordMinScore);

        if (learningWords.Length < examSettings.WellDoneWordsInOneExam + freeSpaceForWords)
            freeSpaceForWords = examSettings.WellDoneWordsInOneExam + examSettings.NewWordInOneExam -
                                learningWords.Length;
        else
            freeSpaceForWords = 0;

        var newLearningWords = await _usersWordsService.GetRandomWordsWithPhrasesAsync(
            Chat.User,
            examSettings.NewWordInOneExam + freeSpaceForWords,
            examSettings.CountOfVariantsForChoose,
            examSettings.MaxTranslationsInOneExam,
            CurrentScoreSortingType.JustAsked,
            WordLeaningGlobalSettings.StartScoreForWord,
            WordLeaningGlobalSettings.LearningWordMinScore);

        await PrintExamStartsMessage(examType, newLearningWords);

        var needToContinue = await Chat.WaitInlineKeyboardInput();
        if (needToContinue != "/startExamination")
            return;

        var learnedWords = await _usersWordsService.GetWordsWithPhrasesAsync(
            Chat.User,
            examSettings.LearnedWordsInOneExam,
            examSettings.MaxTranslationsInOneExam,
            CurrentScoreSortingType.LongAsked,
            WordLeaningGlobalSettings.LearnedWordMinScore);


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

        var examsLearnedWords = CreateExamListForNewWords(examType, distinctLearningWords, examSettings);
        Console.WriteLine($"Количество экзаменов для всех слов {examsLearnedWords.Count}");
        Console.WriteLine(string.Join(" \r\n", examsLearnedWords.ToList()));

        Console.WriteLine($"Количество всех слов: {distinctLearningWords.Count()}");
        Console.WriteLine(string.Join(" \r\n", distinctLearningWords.ToList()));

        var learningWordsCount = distinctLearningWords.Length;

        var originWordsScore = new Dictionary<string, double>();
        foreach (var word in distinctLearningWords) {
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
        foreach (var word in examsLearnedWords.Shuffle()) {
            //exclude the next same exam
            if (previousWord == word)
                continue;
            previousWord = word;

            var allLearningWordsWereShowedAtLeastOneTime = wordQuestionNumber < learningWordsCount;
            var question =
                _questionSelector.GetNextQuestionFor(allLearningWordsWereShowedAtLeastOneTime, word, examType);
            wordQuestionNumber++;

            var learnList = distinctLearningWords;

            if (!distinctLearningWords.Contains(word))
                learnList = distinctLearningWords.Append(word).ToArray();

            if (wordQuestionNumber > 1 && question.NeedClearScreen)
                await WriteDontPeakMessage(lastExamResult?.ResultsBeforeHideousTextMarkdown.GetOrdinalString());

            var originRate = word.Score;

            var questionMetric = new QuestionMetric(word, question.Name);
            var result = await PassWithRetries(question, word, learnList, wordQuestionNumber, examType);
            switch (result.Results) {
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
        var updateUserTask = _userService.Update(Chat.User);

        //info after examination
        await SendExamResultToUser(
            distinctLearningWords: distinctLearningWords,
            originWordsScore: originWordsScore,
            questionsPassed: questionsPassed,
            questionsCount: questionsCount);

        await updateUserTask;
    }

    private async Task PrintExamStartsMessage(ExamType examType, UserWordModel[] newLearningWords) {
        var markdown = Markdown.Escaped($"{Emojis.Learning}").ToSemiBold().Space();
        
        markdown += examType == ExamType.NoInput
            ? Markdown.Escaped(Chat.Texts.FastExamLearningHeader).ToSemiBold().NewLine()
            : Markdown.Escaped(Chat.Texts.WriteExamLearningHeader).ToSemiBold();
        
        markdown = markdown.NewLine() + Chat.Texts.CarefullyStudyTheList
            .NewLine()
            .NewLine();

        var messageWithListOfWords = newLearningWords.Shuffle()
            .Aggregate(Markdown.Empty, (current, pairModel) =>
                current + Markdown.Escaped($"{pairModel.Word}\t\t:{pairModel.AllTranslationsAsSingleString}\r\n"));

        markdown += messageWithListOfWords.ToQuotationMono()
            .NewLine()
            .AddEscaped($"... {Chat.Texts.thenClickStart}");

        if (examType != ExamType.NoInput) {
            markdown = markdown.NewLine().NewLine() +
                       Chat.Texts.TipYouCanEnterCommandIfYouDontKnowTheAnswerForWriteExam(
                           QuestionLogicHelper.IDontKnownSubcommand);
        }

        await Chat.SendMarkdownMessageAsync(markdown, new[]
        {
            new[]
            {
                InlineButtons.Button(Chat.Texts.StartButton, "/startExamination"),
                InlineButtons.Button(Chat.Texts.CancelButton, BotCommands.Start)
            }
        });
    }

    private async Task SendExamResultToUser(
        UserWordModel[] distinctLearningWords, Dictionary<string, double> originWordsScore, int questionsPassed,
        int questionsCount) {
        var doneMessage = CreateLearningResultsMessage(
            distinctLearningWords,
            originWordsScore,
            questionsPassed,
            questionsCount);

        await Chat.SendMarkdownMessageAsync(
            doneMessage,
            new[]
            {
                new[] { InlineButtons.Exam($"🔁 {Chat.Texts.OneMoreLearnButton}") },
                new[]
                {
                    InlineButtons.Stats(Chat.Texts),
                    InlineButtons.Translation(Chat.Texts)
                }
            });
    }

    private async Task<QuestionResult> PassWithRetries(Question question,
        UserWordModel word,
        UserWordModel[] learnList,
        int wordQuestionNumber,
        ExamType examType) {
        int retrieNumber = 0;
        for (int i = 0; i < 40; i++) {
            var result = await question.Scenario.Pass(Chat, word, learnList);
            if (result.Results == QResult.Impossible) {
                var qName = question.Name;
                for (int iteration = 0; question.Name == qName; iteration++) {
                    question = _questionSelector.GetNextQuestionFor(wordQuestionNumber == 0, word, examType);
                    if (iteration > 100)
                        return QuestionResult.Failed(Markdown.Empty, Markdown.Empty);
                }
            }
            else if (result.Results == QResult.Retry) {
                wordQuestionNumber++;
                retrieNumber++;
                if (retrieNumber >= 4)
                    return QuestionResult.Failed(Markdown.Empty, Markdown.Empty);
            }
            else return result;
        }

        return QuestionResult.Failed(Markdown.Empty, Markdown.Empty);
    }

    private Markdown CreateLearningResultsMessage(
        UserWordModel[] wordsInExam,
        Dictionary<string, double> originWordsScore,
        int questionsPassed,
        int questionsCount
    ) {
        var newWellLearnedWords = new List<UserWordModel>();
        var forgottenWords = new List<UserWordModel>();

        foreach (var word in wordsInExam) {
            if (word.AbsoluteScore >= WordLeaningGlobalSettings.WellDoneWordMinScore) {
                if (originWordsScore[word.Word] < WordLeaningGlobalSettings.WellDoneWordMinScore)
                    newWellLearnedWords.Add(word);
            }
            else {
                if (originWordsScore[word.Word] > WordLeaningGlobalSettings.WellDoneWordMinScore)
                    forgottenWords.Add(word);
            }
        }

        var doneMessageMarkdown = Markdown.Escaped($"{Chat.Texts.LearningDone}:").ToSemiBold()
                                      .AddEscaped($" {questionsPassed}/{questionsCount}")
                                      .NewLine() +
                                  Markdown.Escaped($"{Chat.Texts.WordsInTestCount}:").ToSemiBold()
                                      .AddEscaped($" {wordsInExam.Length}")
                                      .NewLine();

        if (newWellLearnedWords.Any()) {
            if (newWellLearnedWords.Count > 1)
                doneMessageMarkdown = doneMessageMarkdown.NewLine() +
                                      Chat.Texts.LearnMoreWords(newWellLearnedWords.Count).ToSemiBold().AddEscaped(":")
                                          .NewLine();
            else
                doneMessageMarkdown = doneMessageMarkdown.NewLine() +
                                      Markdown
                                          .Escaped($"{Chat.Texts.YouHaveLearnedOneWord}:").ToSemiBold()
                                          .NewLine();

            foreach (var word in newWellLearnedWords) {
                doneMessageMarkdown = doneMessageMarkdown
                    .AddEscaped($"{Emojis.HeavyPlus} ")
                    .AddEscaped(word.Word)
                    .NewLine();
            }
        }

        if (forgottenWords.Any()) {
            if (forgottenWords.Count > 1)
                doneMessageMarkdown = doneMessageMarkdown
                    .NewLine()
                    .AddEscaped($"{Chat.Texts.YouForgotCountWords(forgottenWords.Count)}:").ToSemiBold()
                    .NewLine();
            else
                doneMessageMarkdown = doneMessageMarkdown
                    .NewLine()
                    .AddEscaped($"{Chat.Texts.YouForgotOneWord}:").ToSemiBold()
                    .NewLine();

            foreach (var word in forgottenWords) {
                doneMessageMarkdown = doneMessageMarkdown
                    .AddEscaped($"{Emojis.HeavyMinus} ")
                    .AddEscaped(word.Word)
                    .NewLine();
            }
        }

        // doneMessage.Append(($"\r\n*{Chat.Texts.EarnedScore}:* " + $"{(int)(Chat.User.GamingScore - gamingScoreBefore)}"));
        // doneMessage.Append(($"\r\n*{Chat.Texts.TotalScore}:* {(int) Chat.User.GamingScore}\r\n"));

        var todayStats = Chat.User.GetToday();

        doneMessageMarkdown = doneMessageMarkdown.NewLine() +
                              Markdown
                                  .Escaped(
                                      $"{Chat.Texts.TodaysGoal}: {todayStats.LearningDone}/{_regularExamSettings.ExamsCountGoalForDay} {Chat.Texts.Exams}")
                                  .ToSemiBold()
                                  .NewLine();
        if (todayStats.LearningDone >= _regularExamSettings.ExamsCountGoalForDay)
            doneMessageMarkdown = doneMessageMarkdown
                .AddEscaped($"{Emojis.GreenCircle} {Chat.Texts.TodayGoalReached}")
                .NewLine();

        /*   if (Chat.User.Zen.NeedToAddNewWords)
               doneMessageMarkdown = doneMessageMarkdown.NewLine()
                   .AddEscaped(Chat.Texts.ZenRecomendationAfterExamWeNeedMoreNewWords);
       */
        return doneMessageMarkdown;
    }

    private async Task WriteDontPeakMessage(string resultsBeforeHideousText) {
        //it is not an empty string;
        // it contains invisible character, that allows to show blank message
        string emptySymbol = "‎";

        await Chat.SendMessageAsync(
            $"\r\n\r\n{emptySymbol}‎\r\n{emptySymbol}‎\r\n{emptySymbol}\r\n{emptySymbol}\r\n{emptySymbol}\r\n{emptySymbol}\r\n{emptySymbol}\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n{resultsBeforeHideousText}\r\n\r\n" +
            $"{Chat.Texts.DontPeekUpward}\r\n");
        await Chat.SendMessageAsync("\U0001F648");
    }

    private List<UserWordModel> CreateExamListForNewWords(ExamType examType, UserWordModel[] learningWords,
        ExamSettings examSettings) {
        var examsList = new List<UserWordModel>(examSettings.MaxExamSize);

        //Every learning word appears in exam from MIN to MAX times
        for (int i = 0; i < examSettings.MinWordsQuestionsInOneExam; i++)
            examsList.AddRange(learningWords);
        for (int i = 0; i < examSettings.MaxWordsQuestionsInOneExam - examSettings.MinWordsQuestionsInOneExam; i++)
            examsList.AddRange(learningWords.Where(w => Rand.Next() % 2 == 0));
        while (examsList.Count > examSettings.MaxExamSize)
            examsList.RemoveAt(examsList.Count - 1);
        return examsList;
    }
}
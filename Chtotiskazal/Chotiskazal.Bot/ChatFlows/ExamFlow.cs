using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Chotiskazal.Bot.Interface;
using Chotiskazal.Bot.Questions;
using SayWhat.Bll;
using SayWhat.Bll.Services;
using SayWhat.MongoDAL;
using SayWhat.MongoDAL.QuestionMetrics;
using SayWhat.MongoDAL.Words;
using Telegram.Bot.Types.ReplyMarkups;

namespace Chotiskazal.Bot.ChatFlows
{
    public class ExamFlow
    {
        private readonly ExamSettings _examSettings;
        private ChatRoom Chat { get; }
        private readonly UserService _userService;
        private readonly UsersWordsService _usersWordsService;

        public ExamFlow(
            ChatRoom chat, 
            UserService userService,
            UsersWordsService usersWordsService, 
            ExamSettings examSettings)
        {
            Chat = chat;
            _userService = userService;
            _usersWordsService = usersWordsService;
            _examSettings = examSettings;
        }

        public async Task EnterAsync()
        {
            if (!await _usersWordsService.HasWordsFor(Chat.User)) {
                await Chat.SendMessageAsync(Chat.Texts.NeedToAddMoreWordsBeforeLearning);
                return;
            }
            
            var typing =  Chat.SendTyping();

            if ((DateTime.Now - Chat.User.LastExam).TotalDays >= 1) {
                await Chat.SendMarkdownMessageAsync(Markdown.Escaped("Refresh words scores..."));
                await _usersWordsService.RefreshWordsCurrentScoreAsync(Chat.User);
            }

            await typing;
            
            var lastAskedWords =
                await _usersWordsService.GetLastAskedWordsWithPhrasesAsync(Chat.User, _examSettings.LastAskedWordsInOneExam, _examSettings);
           
            var wellDoneWords 
                = await _usersWordsService.GetWordsWithPhrasesAsync(
                    Chat.User, 
                    _examSettings.WellDoneWordsInOneExam, 
                    _examSettings.MaxTranslationsInOneExam,
                    WordLeaningGlobalSettings.WellDoneWordMinScore,
                    WordLeaningGlobalSettings.LearnedWordMinScore);

            var freeSpaceForWords = 0;
            if (wellDoneWords.Length < _examSettings.WellDoneWordsInOneExam) {
                freeSpaceForWords = _examSettings .WellDoneWordsInOneExam - wellDoneWords.Length;
            }
            
            var learningWords 
                = await _usersWordsService.GetWordsWithPhrasesAsync(
                    Chat.User, 
                    _examSettings.LearningWordsInOneExam + freeSpaceForWords, 
                    _examSettings.MaxTranslationsInOneExam,
                    WordLeaningGlobalSettings.LearningWordMinScore,
                    WordLeaningGlobalSettings.WellDoneWordMinScore);

            if (learningWords.Length < _examSettings.WellDoneWordsInOneExam + freeSpaceForWords) 
                freeSpaceForWords = _examSettings.WellDoneWordsInOneExam + _examSettings.NewWordInOneExam -
                                    learningWords.Length;
            else
                freeSpaceForWords = 0;
            
            var newLearningWords 
                = await _usersWordsService.GetRandomWordsWithPhrasesAsync(
                    Chat.User, 
                    _examSettings.NewWordInOneExam+freeSpaceForWords, 
                    _examSettings.CountOfVariantsForChoose,
                    _examSettings.MaxTranslationsInOneExam,
                    WordLeaningGlobalSettings.StartScoreForWord,
                    WordLeaningGlobalSettings.LearningWordMinScore);
            
            await PrintNewWordsValues(newLearningWords);
            var needToContinue = await Chat.WaitInlineKeyboardInput();
            if (needToContinue != "/startExamination")
                return;
            
            var learnedWords 
                = await _usersWordsService.GetWordsWithPhrasesAsync(
                    Chat.User, 
                    _examSettings.LearnedWordsInOneExam, 
                    _examSettings.MaxTranslationsInOneExam,
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
                .Union(lastAskedWords)
                .Union(learningWords)
                .Union(wellDoneWords)
                .Union(learnedWords)
                .ToArray();
            
            var examsLearnedWords = CreateExamListForNewWords(distinctLearningWords, _examSettings);
            Console.WriteLine($"Количество экзаменов для всех слов {examsLearnedWords.Count}");
            Console.WriteLine(string.Join(" \r\n", examsLearnedWords.ToList()));
            
            Console.WriteLine($"Количество всех слов: {distinctLearningWords.Count()}");
            Console.WriteLine(string.Join(" \r\n", distinctLearningWords.ToList()));

            var learningWordsCount = distinctLearningWords.Length;

            var originWordsScore = new Dictionary<string,double>();
            foreach (var word in distinctLearningWords)
            {
                if (!originWordsScore.ContainsKey(word.Word))
                    originWordsScore.Add(word.Word,word.AbsoluteScore);
            }

            //begin testing
            var started = DateTime.Now;
            var gamingScoreBefore = Chat.User.GamingScore;

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
                    QuestionSelector.Singletone.GetNextQuestionFor(allLearningWordsWereShowedAtLeastOneTime, word);
                wordQuestionNumber++;

                var learnList = distinctLearningWords;

                if (!distinctLearningWords.Contains(word))
                    learnList = distinctLearningWords.Append(word).ToArray();

                if (wordQuestionNumber > 1 && question.NeedClearScreen)
                    await WriteDontPeakMessage(lastExamResult?.ResultsBeforeHideousTextMarkdown.GetOrdinalString());

                var originRate = word.Score;

                var questionMetric = new QuestionMetric(word, question.Name);
                var result = await PassWithRetries(question, word, learnList, wordQuestionNumber);
                switch (result.Results)
                {
                    case ExamResult.Passed:
                        var succTask = _usersWordsService.RegisterSuccess(word, question.PassScore);
                        questionsCount++;
                        questionsPassed++;
                        questionMetric.OnExamFinished(word.Score, true);
                        Reporter.ReportQuestionDone(questionMetric, Chat.ChatId, question.Name);
                        await succTask;
                        Chat.User.OnQuestionPassed(word.Score - originRate);
                        break;
                    case ExamResult.Failed:
                        var failureTask = _usersWordsService.RegisterFailure(word,question.FailScore);
                        questionMetric.OnExamFinished(word.Score, false);
                        Reporter.ReportQuestionDone(questionMetric, Chat.ChatId, question.Name);
                        questionsCount++;
                        await failureTask;
                        Chat.User.OnQuestionFailed(word.Score - originRate);
                        break;
                    case ExamResult.Retry:
                    case ExamResult.Impossible:
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
                questionsCount: questionsCount, 
                gamingScoreBefore: gamingScoreBefore);

            await updateUserTask;
        }

        private async Task PrintNewWordsValues(UserWordModel[] newLearningWords) {
            var markdown = Markdown.Escaped($"{Emojis.Learning}").ToSemiBold() +
                           Chat.Texts.LearningCarefullyStudyTheList
                               .NewLine()
                               .NewLine();

            var messageWithListOfWords = newLearningWords.Shuffle()
                .Aggregate(Markdown.Empty, (current, pairModel) =>
                    current + Markdown.Escaped($"{pairModel.Word}\t\t:{pairModel.AllTranslationsAsSingleString}\r\n"));

            markdown += messageWithListOfWords.ToPreFormattedMono()
                .NewLine()
                .AddEscaped($"... {Chat.Texts.thenClickStart}");

            await Chat.SendMarkdownMessageAsync(markdown, new[]
            {
                new[]
                {
                    new InlineKeyboardButton
                    {
                        CallbackData = "/startExamination",
                        Text = Chat.Texts.StartButton
                    },
                    new InlineKeyboardButton
                    {
                        CallbackData = BotCommands.Start,
                        Text = Chat.Texts.CancelButton,
                    }
                }
            });
        }

        private async Task SendExamResultToUser(
            UserWordModel[] distinctLearningWords, Dictionary<string, double> originWordsScore, int questionsPassed, int questionsCount,
            double gamingScoreBefore) {
            var doneMessage = CreateLearningResultsMessage(
                distinctLearningWords,
                originWordsScore,
                questionsPassed,
                questionsCount,
                gamingScoreBefore);

            await Chat.SendMarkdownMessageAsync(
                doneMessage,
                new[] {
                    new[] { InlineButtons.Exam($"🔁 {Chat.Texts.OneMoreLearnButton}") },
                    new[] {
                        InlineButtons.Stats(Chat.Texts),
                        InlineButtons.Translation(Chat.Texts.TranslateButton + " " + Emojis.Translate)
                    }
                });
        }

        private async Task<QuestionResult> PassWithRetries(
            IQuestion question, 
            UserWordModel word, 
            UserWordModel[] learnList, 
            int wordQuestionNumber)
        {
            int retrieNumber = 0;
            for(int i = 0; i<40; i++)
            {
                var result = await question.Pass(Chat, word, learnList);
                if (result.Results == ExamResult.Impossible)
                {
                    var qName = question.Name;
                    for (int iteration = 0;question.Name==qName; iteration++)
                    {
                        question = QuestionSelector.Singletone.GetNextQuestionFor(wordQuestionNumber == 0, word);
                        if(iteration>100)
                            return QuestionResult.Failed(Markdown.Empty,Markdown.Empty);
                    }
                }
                else if (result.Results == ExamResult.Retry)
                {
                    wordQuestionNumber++;
                    retrieNumber++;
                    if(retrieNumber>=4)
                        return QuestionResult.Failed(Markdown.Empty,Markdown.Empty);
                }
                else return result;
            }
            return QuestionResult.Failed(Markdown.Empty,Markdown.Empty);
        }

        private Markdown CreateLearningResultsMessage(
            UserWordModel[] wordsInExam,
            Dictionary<string, double> originWordsScore, 
            int questionsPassed, 
            int questionsCount,
            double gamingScoreBefore
        )
        {
            var newWellLearnedWords = new List<UserWordModel>();
            var forgottenWords = new List<UserWordModel>();

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

            var doneMessageMarkdown = Markdown.Escaped($"{Chat.Texts.LearningDone}:").ToSemiBold()
                                          .AddEscaped($" {questionsPassed}/{questionsCount}")
                                          .NewLine() +
                                      Markdown.Escaped($"{Chat.Texts.WordsInTestCount}:").ToSemiBold()
                                          .AddEscaped($" {wordsInExam.Length}")
                                          .NewLine();

            if (newWellLearnedWords.Any()) {
                if (newWellLearnedWords.Count > 1)
                    doneMessageMarkdown = doneMessageMarkdown.NewLine() +
                                          Markdown
                                              .Escaped($"{Chat.Texts.LearnMoreWords(newWellLearnedWords.Count)}:").ToSemiBold()
                                              .NewLine();
                else
                    doneMessageMarkdown = doneMessageMarkdown.NewLine() +
                                         Markdown
                                             .Escaped($"{Chat.Texts.YouHaveLearnedOneWord}:").ToSemiBold()
                                             .NewLine();
                        
                foreach (var word in newWellLearnedWords)
                {
                    doneMessageMarkdown = doneMessageMarkdown
                        .AddEscaped($"{Emojis.HeavyPlus} ")
                        .AddEscaped(word.Word)
                        .NewLine();
                }
            }

            if (forgottenWords.Any())
            {
                if(forgottenWords.Count>1)
                    doneMessageMarkdown = doneMessageMarkdown
                        .NewLine()
                        .AddEscaped($"{Chat.Texts.YouForgotCountWords(forgottenWords.Count)}:").ToSemiBold()
                        .NewLine();
                else
                    doneMessageMarkdown = doneMessageMarkdown
                        .NewLine()
                        .AddEscaped($"{Chat.Texts.YouForgotOneWord}:").ToSemiBold()
                        .NewLine();
                
                foreach (var word in forgottenWords)
                {
                    doneMessageMarkdown = doneMessageMarkdown
                        .AddEscaped($"{Emojis.HeavyMinus} ")
                        .AddEscaped(word.Word)
                        .NewLine();
                }
            }
            
            // doneMessage.Append(($"\r\n*{Chat.Texts.EarnedScore}:* " + $"{(int)(Chat.User.GamingScore - gamingScoreBefore)}"));
            // doneMessage.Append(($"\r\n*{Chat.Texts.TotalScore}:* {(int) Chat.User.GamingScore}\r\n"));
            
            var todayStats =Chat.User.GetToday();

            doneMessageMarkdown = doneMessageMarkdown.NewLine() +
                                  Markdown
                                      .Escaped(
                                          $"{Chat.Texts.TodaysGoal}: {todayStats.LearningDone}/{_examSettings.ExamsCountGoalForDay} {Chat.Texts.Exams}").ToSemiBold()
                                      .NewLine();
            if (todayStats.LearningDone >= _examSettings.ExamsCountGoalForDay)
                doneMessageMarkdown = doneMessageMarkdown
                    .AddEscaped($"{Emojis.GreenCircle} {Chat.Texts.TodayGoalReached}")
                    .NewLine();

         /*   if (Chat.User.Zen.NeedToAddNewWords)
                doneMessageMarkdown = doneMessageMarkdown.NewLine()
                    .AddEscaped(Chat.Texts.ZenRecomendationAfterExamWeNeedMoreNewWords);
        */
            return doneMessageMarkdown;
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
        
        private List<UserWordModel> CreateExamListForNewWords(UserWordModel[] learningWords, ExamSettings examSettings) {
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
}

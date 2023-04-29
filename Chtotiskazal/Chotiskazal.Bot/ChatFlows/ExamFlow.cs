﻿using System;
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
           
      //      var startupScoreUpdate =  _usersWordsService.UpdateCurrentScoreForRandomWords(Chat.User, _examSettings.MaxLearningWordsCountInOneExam*2);
            var typing =  Chat.SendTyping();
            
            var count = Rand.RandomIn(_examSettings.MinNewLearningWordsCountInOneExam,
                _examSettings.MaxNewLearningWordsCountInOneExam);
      //      await startupScoreUpdate;
            await typing;
           
            //TODO возможо надо делать рандомные худшие слова, пока что они выбираеются по принципу минимальных очков
            var newLearningWords 
                = await _usersWordsService.GetWordsForLearningWithPhrasesAsync(
                    Chat.User, 
                    count, 
                    _examSettings.MaxTranslationsInOneExam,
                    WordLeaningGlobalSettings.LearnedWordMinScore); 
            Console.WriteLine($"Количество худших слов: {newLearningWords.Length}");
            Console.WriteLine(string.Join(" \r\n", newLearningWords.ToList()));
            
            var examsWords = CreateExamListForNewWords(newLearningWords, _examSettings);
            Console.WriteLine($"Количество экзаменов для новых слов {examsWords.Count}");

            await PrintNewWordsValues(newLearningWords);
            var needToContinue = await Chat.WaitInlineKeyboardInput();
            if (needToContinue != "/startExamination")
                return;

            var advancedWords = await _usersWordsService.AppendAdvancedWordsToExamList(
                    Chat.User, _examSettings, _examSettings.MaxLearningWordsCountInOneExam-newLearningWords.Length);
            
            Console.WriteLine($"{_examSettings.MaxLearningWordsCountInOneExam}-{newLearningWords.Length} = {_examSettings.MaxLearningWordsCountInOneExam-newLearningWords.Length}");
            //TODO before this values used for count questions for advanced words. Maybe they are important. Maybe not
            /*var minimumTimesThatWordHasToBeAsked =
                Rand.RandomIn(_examSettings.MinAdvancedExamMinQuestionAskedForOneWordCount,
                    _examSettings.MaxAdvancedExamMinQuestionAskedForOneWordCount);

            var advancedlistMaxCountQuestions = Math.Min(Rand.RandomIn(_examSettings.MinAdvancedQuestionsCount, _examSettings.MaxAdvancedQuestionsCount),
                _examSettings.MaxExamSize - examsWords.Count);
            
            if (advancedlistMaxCountQuestions <= _examSettings.MinAdvancedQuestionsCount) 
                advancedlistMaxCountQuestions = _examSettings.MinAdvancedQuestionsCount;

            Console.WriteLine($" Максимальное Количество экзаменов для продвинутых  слов {advancedlistMaxCountQuestions}");

            Console.WriteLine($"Минимальное кол во повтора продвинутых слов {minimumTimesThatWordHasToBeAsked}");*/

            var examAdvancedWords = CreateExamListForNewWords(advancedWords, _examSettings);
            Console.WriteLine($"Количество экзаменов для advanced слов: {examAdvancedWords.Count}");

            examsWords.AddRange(examAdvancedWords);
            
            var distinctLearningWords = newLearningWords.ToList().Union(advancedWords).ToArray();
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
            foreach (var word in examsWords.Shuffle()) {
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

                Chat.User.OnAnyActivity();
                var originRate = word.Score;

                var questionMetric = new QuestionMetric(word, question.Name);
                var result = await PassWithRetries(question, word, learnList, wordQuestionNumber);
                switch (result.Results)
                {
                    case ExamResult.Passed:
                        var succTask = _usersWordsService.RegisterSuccess(word);
                        questionsCount++;
                        questionsPassed++;
                        questionMetric.OnExamFinished(word.Score, true);
                        Reporter.ReportQuestionDone(questionMetric, Chat.ChatId, question.Name);
                        await succTask;
                        Chat.User.OnQuestionPassed(word.Score - originRate);
                        break;
                    case ExamResult.Failed:
                        var failureTask = _usersWordsService.RegisterFailure(word);
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
     //       var finializeScoreUpdateTask =_usersWordsService.UpdateCurrentScoreForRandomWords(Chat.User,10);

            //info after examination
            await SendExamResultToUser(
                distinctLearningWords: distinctLearningWords, 
                originWordsScore: originWordsScore, 
                questionsPassed: questionsPassed, 
                questionsCount: questionsCount, 
                gamingScoreBefore: gamingScoreBefore);

            await updateUserTask;
     //       await finializeScoreUpdateTask;
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
                if (word.AbsoluteScore >= 4)
                {
                    if (originWordsScore[word.Word] < 4)
                        newWellLearnedWords.Add(word);
                }
                else
                {
                    if (originWordsScore[word.Word] > 4)
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
            for (int i = 0; i < examSettings.MinNewLearningWordsCountInOneExam; i++) 
                examsList.AddRange(learningWords);
            for (int i = 0; i < examSettings.MaxNewLearningWordsCountInOneExam - examSettings.MinNewLearningWordsCountInOneExam; i++) 
                examsList.AddRange(learningWords.Where(w => Rand.Next() % 2 == 0));
            while (examsList.Count > examSettings.MaxExamSize) 
                examsList.RemoveAt(examsList.Count - 1);
            return examsList;
        }
    }
}

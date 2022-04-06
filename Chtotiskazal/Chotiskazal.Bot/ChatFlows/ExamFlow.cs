using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
            
            var startupScoreUpdate =  _usersWordsService.UpdateCurrentScoreForRandomWords(Chat.User, _examSettings.MaxLearningWordsCountInOneExam*2);
            var typing =  Chat.SendTyping();

            var c = Rand.RandomIn(_examSettings.MinLearningWordsCountInOneExam,
                _examSettings.MaxLearningWordsCountInOneExam);
            await startupScoreUpdate;
            await typing;
            var learningWords 
                = await _usersWordsService.GetWordsForLearningWithPhrasesAsync(Chat.User, c, 3);
      
            var learningWordsCount = learningWords.Length;
            if (learningWords.Average(w => w.AbsoluteScore) <= WordLeaningGlobalSettings.FamiliarWordMinScore)
            {
                var sb = new StringBuilder($"{Emojis.Learning} "+Chat.Texts.LearningCarefullyStudyTheListMarkdown +"\r\n\r\n```\r\n");

                foreach (var pairModel in learningWords.Shuffle())
                {
                    sb.AppendLine($"{pairModel.Word.EscapeForMarkdown()}\t\t:{pairModel.AllTranslationsAsSingleString.EscapeForMarkdown()}");
                }
                sb.AppendLine($"\r\n```\r\n\\.\\.\\. {Chat.Texts.thenClickStartMarkdown}");
                await Chat.SendMarkdownMessageAsync(sb.ToString(),new[]{ new[]{ new InlineKeyboardButton
                {
                    CallbackData = "/startExamination",
                    Text = Chat.Texts.StartButton
                }, new InlineKeyboardButton
                {
                    CallbackData = BotCommands.Start,
                    Text = Chat.Texts.CancelButton,
                }}});
                var userInput = await Chat.WaitInlineKeyboardInput();
                if (userInput != "/startExamination")
                    return;
            }
            var started = DateTime.Now;

            var learningAndAdvancedWords
                = (await _usersWordsService.AppendAdvancedWordsToExamList(
                    Chat.User, learningWords, _examSettings));
            
            var distinctLearningWords = learningAndAdvancedWords.Distinct().ToArray();
            
            var originWordsScore = new Dictionary<string,double>();
            foreach (var word in distinctLearningWords)
            {
                if (!originWordsScore.ContainsKey(word.Word))
                    originWordsScore.Add(word.Word,word.AbsoluteScore);
            }

            var gamingScoreBefore = Chat.User.GamingScore;

            var questionsCount = 0;
            var questionsPassed = 0;
            var wordQuestionNumber = 0;
            QuestionResult lastExamResult = null;

            foreach (var word in learningAndAdvancedWords)
            {
                var allLearningWordsWereShowedAtLeastOneTime = wordQuestionNumber < learningWordsCount;
                var question =
                    QuestionSelector.Singletone.GetNextQuestionFor(allLearningWordsWereShowedAtLeastOneTime, word);
                wordQuestionNumber++;

                var learnList = learningWords;

                if (!learningWords.Contains(word))
                    learnList = learningWords.Append(word).ToArray();

                if (wordQuestionNumber > 1 && question.NeedClearScreen)
                    await WriteDontPeakMessage(lastExamResult?.ResultsBeforeHideousText);

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
                        Reporter.ReportQuestionDone(questionMetric, Chat.ChatId);
                        await succTask;
                        Chat.User.OnQuestionPassed(word.Score - originRate);
                        break;
                    case ExamResult.Failed:
                        var failureTask = _usersWordsService.RegisterFailure(word);
                        questionMetric.OnExamFinished(word.Score, false);
                        Reporter.ReportQuestionDone(questionMetric, Chat.ChatId);
                        questionsCount++;
                        await failureTask;
                        Chat.User.OnQuestionFailed(word.Score - originRate);
                        break;
                    case ExamResult.Retry:
                    case ExamResult.Impossible:
                        throw new NotSupportedException(result.Results.ToString());
                }

                if (!string.IsNullOrWhiteSpace(result.OpenResultsText))
                    await Chat.SendMarkdownMessageAsync(result.OpenResultsText);

                lastExamResult = result;

                Reporter.ReportExamPassed(Chat.User.TelegramId, started, questionsCount, questionsPassed);
            }

            Chat.User.OnLearningDone();
            var updateUserTask = _userService.Update(Chat.User);
            var finializeScoreUpdateTask =_usersWordsService.UpdateCurrentScoreForRandomWords(Chat.User,10);

            //info after examination
            await SendExamResultToUser(
                distinctLearningWords: distinctLearningWords, 
                originWordsScore: originWordsScore, 
                questionsPassed: questionsPassed, 
                questionsCount: questionsCount, 
                learningWords: learningWords, 
                gamingScoreBefore: gamingScoreBefore);

            await updateUserTask;
            await finializeScoreUpdateTask;
        }

        private async Task SendExamResultToUser(
            UserWordModel[] distinctLearningWords, Dictionary<string, double> originWordsScore, int questionsPassed, int questionsCount,
            UserWordModel[] learningWords, double gamingScoreBefore) {
            var doneMessage = CreateLearningResultsMessage(
                distinctLearningWords,
                originWordsScore,
                questionsPassed,
                questionsCount,
                learningWords,
                gamingScoreBefore);

            await Chat.SendMarkdownMessageAsync(
                doneMessage.EscapeForMarkdown(),
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
                            return QuestionResult.Failed("","");
                    }
                }
                else if (result.Results == ExamResult.Retry)
                {
                    wordQuestionNumber++;
                    retrieNumber++;
                    if(retrieNumber>=4)
                        return QuestionResult.Failed("","");
                }
                else return result;
            }
            return QuestionResult.Failed("","");
        }

        private string CreateLearningResultsMessage(
            UserWordModel[] wordsInExam,
            Dictionary<string, double> originWordsScore, 
            int questionsPassed, 
            int questionsCount, 
            UserWordModel[] learningWords,
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

            
            var doneMessage = new StringBuilder();
            
            doneMessage.Append($"*{Chat.Texts.LearningDone}:* {questionsPassed}/{questionsCount}\r\n" +
                                                $"*{Chat.Texts.WordsInTestCount}:* {learningWords.Length}\r\n");
            
            if (newWellLearnedWords.Any())
            {
                if (newWellLearnedWords.Count > 1)
                    doneMessage.Append($"*\r\n{Chat.Texts.YouHaveLearnedWords(newWellLearnedWords.Count)}:*\r\n");
                else
                    doneMessage.Append($"*\r\n{Chat.Texts.YouHaveLearnedOneWord}:*\r\n");
                foreach (var word in newWellLearnedWords)
                {
                    doneMessage.Append($"{Emojis.HeavyPlus} {word.Word}\r\n");
                }
            }

            if (forgottenWords.Any())
            {
                if(forgottenWords.Count>1)
                    doneMessage.Append($"\r\n*{Chat.Texts.YouForgotCountWords(forgottenWords.Count)}:*\r\n");
                else
                    doneMessage.Append($"\r\n*{Chat.Texts.YouForgotOneWord}:*\r\n");
                foreach (var word in forgottenWords)
                {
                    doneMessage.Append($"{Emojis.HeavyMinus} {word.Word}\r\n");
                }
            }
            
            // doneMessage.Append(($"\r\n*{Chat.Texts.EarnedScore}:* " + $"{(int)(Chat.User.GamingScore - gamingScoreBefore)}"));
            // doneMessage.Append(($"\r\n*{Chat.Texts.TotalScore}:* {(int) Chat.User.GamingScore}\r\n"));
            
            var todayStats =Chat.User.GetToday();
            doneMessage.Append($"\r\n*{Chat.Texts.TodaysGoal}: {todayStats.LearningDone}/{_examSettings.ExamsCountGoalForDay} {Chat.Texts.Exams}*\r\n");
            if (todayStats.LearningDone >= _examSettings.ExamsCountGoalForDay) 
                doneMessage.Append($"{Emojis.GreenCircle} {Chat.Texts.TodayGoalReached}\r\n");
            
            if(Chat.User.Zen.NeedToAddNewWords)
                doneMessage.Append($"\r\n{Chat.Texts.ZenRecomendationAfterExamWeNeedMoreNewWords}");

            return doneMessage.ToString();
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
    }
}

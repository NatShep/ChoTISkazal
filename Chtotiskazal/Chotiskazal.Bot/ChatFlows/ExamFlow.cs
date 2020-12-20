using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Chotiskazal.Bot.InterfaceLang;
using Chotiskazal.Bot.Questions;
using SayWhat.Bll;
using SayWhat.Bll.Services;
using SayWhat.MongoDAL;
using SayWhat.MongoDAL.QuestionMetrics;
using SayWhat.MongoDAL.Users;
using SayWhat.MongoDAL.Words;
using Telegram.Bot.Types.ReplyMarkups;

namespace Chotiskazal.Bot.ChatFlows
{
    public class ExamFlow
    {
        private readonly ExamSettings _examSettings;
        private readonly ChatIO _chatIo;
        private readonly UserModel _user;
        private readonly UserService _userService;
        private readonly UsersWordsService _usersWordsService;

        public ExamFlow(
            ChatIO chatIo, 
            UserModel user,
            UserService userService,
            UsersWordsService usersWordsService, 
            ExamSettings examSettings)
        {
            _chatIo = chatIo;
            _user = user;
            _userService = userService;
            _usersWordsService = usersWordsService;
            _examSettings = examSettings;
        }

        public async Task EnterAsync()
        {
            if (!await _usersWordsService.HasWords(_user)) {
                await _chatIo.SendMessageAsync(Texts.Current.NeedToAddMoreWordsBeforeLearning);
                return;
            }
            
            var startupScoreUpdate =  _usersWordsService.UpdateCurrentScoreForRandomWords(_user, _examSettings.MaxLearningWordsCountInOneExam*2);
            var typing =  _chatIo.SendTyping();

            var c = Rand.RandomIn(_examSettings.MinLearningWordsCountInOneExam,
                _examSettings.MaxLearningWordsCountInOneExam);
            await startupScoreUpdate;
            await typing;
            var learningWords 
                = await _usersWordsService.GetWordsForLearningWithPhrasesAsync(_user, c, 3);
      
            var learningWordsCount = learningWords.Length;
            if (learningWords.Average(w => w.AbsoluteScore) <= WordLeaningGlobalSettings.FamiliarWordMinScore)
            {
                var sb = new StringBuilder(Texts.Current.LearningCarefullyStudyTheListMarkdown +"\r\n\r\n```\r\n");

                foreach (var pairModel in learningWords.Randomize())
                {
                    sb.AppendLine($"{pairModel.Word.EscapeForMarkdown()}\t\t:{pairModel.AllTranslationsAsSingleString.EscapeForMarkdown()}");
                }
                sb.AppendLine($"\r\n```\r\n\\.\\.\\. {Texts.Current.thenClickStartMarkdown}");
                await _chatIo.SendMarkdownMessageAsync(sb.ToString(),new[]{ new[]{ new InlineKeyboardButton
                {
                    CallbackData = "/startExamination",
                    Text = Texts.Current.StartButton
                }, new InlineKeyboardButton
                {
                    CallbackData = "/start",
                    Text = Texts.Current.CancelButton,
                }}});
                var userInput = await _chatIo.WaitInlineKeyboardInput();
                if (userInput != "/startExamination")
                    return;
            }
            var started = DateTime.Now;

            var learningAndAdvancedWords
                = (await _usersWordsService.AppendAdvancedWordsToExamList(_user, learningWords, _examSettings));
            
            var distinctLearningWords = learningAndAdvancedWords.Distinct().ToArray();
            
            var originWordsScore = new Dictionary<string,double>();
            foreach (var word in distinctLearningWords)
            {
                if (!originWordsScore.ContainsKey(word.Word))
                    originWordsScore.Add(word.Word,word.AbsoluteScore);
            }

            var gamingScoreBefore = _user.GamingScore;

            var questionsCount = 0;
            var questionsPassed = 0;
            var wordQuestionNumber = 0;
            QuestionResult? lastExamResult = null;

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

                _user.OnAnyActivity();
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
                        Botlog.SaveQuestionMetricInfo(questionMetric, _chatIo.ChatId);
                        await succTask;
                        _user.OnQuestionPassed(word.Score - originRate);
                        break;
                    case ExamResult.Failed:
                        var failureTask = _usersWordsService.RegisterFailure(word);
                        questionMetric.OnExamFinished(word.Score, false);
                        Botlog.SaveQuestionMetricInfo(questionMetric, _chatIo.ChatId);
                        questionsCount++;
                        await failureTask;
                        _user.OnQuestionFailed(word.Score - originRate);
                        break;
                    case ExamResult.Retry:
                    case ExamResult.Impossible:
                        throw new NotSupportedException(result.Results.ToString());
                }

                if (!string.IsNullOrWhiteSpace(result.OpenResultsText))
                    await _chatIo.SendMessageAsync(result.OpenResultsText);

                lastExamResult = result;

                Botlog.RegisterExamInfo(_user.TelegramId, started, questionsCount, questionsPassed);
            }

            _user.OnLearningDone();
            var updateUserTask = _userService.Update(_user);
            var finializeScoreUpdateTask =_usersWordsService.UpdateCurrentScoreForRandomWords(_user,10);

            //info after examination
            var doneMessage = CreateLearningResultsMessage(
                distinctLearningWords, 
                originWordsScore,
                questionsPassed, 
                questionsCount, 
                learningWords, 
                gamingScoreBefore);
            
            await _chatIo.SendMarkdownMessageAsync(doneMessage.EscapeForMarkdown(),
            new[]{new[] { InlineButtons.ExamText($"🔁 {Texts.Current.OneMoreLearnButton}")}, 
                  new[] { InlineButtons.Stats,InlineButtons.Translation}});
            
            await updateUserTask;
            await finializeScoreUpdateTask;
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
                var result = await question.Pass(_chatIo, word, learnList);
                if (result.Results == ExamResult.Impossible)
                {
                    var qName = question.Name;
                    for (int iteration = 0;question.Name==qName; iteration++)
                    {
                        question = QuestionSelector.Singletone.GetNextQuestionFor(wordQuestionNumber == 0, word);
                        if(iteration>100)
                            return QuestionResult.FailedText("","");
                    }
                }
                else if (result.Results == ExamResult.Retry)
                {
                    wordQuestionNumber++;
                    retrieNumber++;
                    if(retrieNumber>=4)
                        return QuestionResult.FailedText("","");
                }
                else return result;
            }
            return QuestionResult.FailedText("","");
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

            var doneMessage = new StringBuilder($"*{Texts.Current.LearningDone}:* {questionsPassed}/{questionsCount}\r\n" +
                                                $"*{Texts.Current.WordsInTestCount}:* {learningWords.Length}\r\n");

            if (newWellLearnedWords.Any())
            {
                if (newWellLearnedWords.Count > 1)
                    doneMessage.Append($"*\r\n{Texts.Current.YouHaveLearnedWords(newWellLearnedWords.Count)}:*\r\n");
                else
                    doneMessage.Append($"*\r\n{Texts.Current.YouHaveLearnedOneWord}:*\r\n");
                foreach (var word in newWellLearnedWords)
                {
                    doneMessage.Append("✅ " + word.Word + "\r\n");
                }
            }

            if (forgottenWords.Any())
            {
                if(forgottenWords.Count>1)
                    doneMessage.Append($"\r\n*{Texts.Current.YouForgotCountWords(forgottenWords.Count)}:*\r\n");
                else
                    doneMessage.Append($"\r\n*{Texts.Current.YouForgotOneWord}:*\r\n");
                foreach (var word in forgottenWords)
                {
                    doneMessage.Append("❗ " + word.Word + "\r\n\r\n");
                }
            }
            
            doneMessage.Append(($"\r\n*{Texts.Current.EarnedScore}:* " + $"{(int)(_user.GamingScore - gamingScoreBefore)}"));
            doneMessage.Append(($"\r\n*{Texts.Current.TotalScore}:* {(int) _user.GamingScore}\r\n"));

            return doneMessage.ToString();
        }

        private async Task WriteDontPeakMessage(string resultsBeforeHideousText)
        {
            //it is not an empty string;
            // it contains invisible character, that allows to show blank message
            string emptySymbol = "‎";
            
            await _chatIo.SendMessageAsync(
                $"\r\n\r\n{emptySymbol}‎\r\n{emptySymbol}‎\r\n{emptySymbol}\r\n{emptySymbol}\r\n{emptySymbol}\r\n{emptySymbol}\r\n{emptySymbol}\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n{resultsBeforeHideousText}\r\n\r\n" +
                $"{Texts.Current.DontPeekUpward}\r\n");
            await _chatIo.SendMessageAsync("\U0001F648");
        }
    }
}

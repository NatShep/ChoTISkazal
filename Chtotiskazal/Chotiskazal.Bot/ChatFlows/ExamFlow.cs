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
            if (!await _usersWordsService.HasWords(_user))
            {
                await _chatIo.SendMessageAsync("You need to add some more words before examination");
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
                var sb = new StringBuilder("*Learning*\r\n\r\n" +
                                           "Carefully study the words in the list below:\r\n\r\n" +
                                           "```\r\n");

                foreach (var pairModel in learningWords.Randomize())
                {
                    sb.AppendLine($"{pairModel.Word}\t\t:{pairModel.TranslationAsList}");
                }
                sb.AppendLine("\r\n```\r\n\\.\\.\\. then click start");
                await _chatIo.SendMarkdownMessageAsync(sb.ToString(),new[]{ new[]{ new InlineKeyboardButton
                {
                    CallbackData = "/startExamination",
                    Text = "Start"
                }, new InlineKeyboardButton
                {
                    CallbackData = "/start",
                    Text = "Cancel",
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
            var i = 0;
            QuestionResult? lastExamResult = null;

            string lastQuestionName = null;
            UserWordModel lastWord = null;
            foreach (var word in learningAndAdvancedWords)
            {
                var allLearningWordsWereShowedAtLeastOneTime = i < learningWordsCount;
                var exam = QuestionSelector.Singletone.GetNextQuestionFor(allLearningWordsWereShowedAtLeastOneTime, word);
                i++;
                var retryFlag = false;
                int questionIterations = 0;
                do
                {
                    // Protection from inifity cycle
                    questionIterations++;
                    if (questionIterations > 100)
                    {
                        Botlog.WriteError(this._chatIo.ChatId.Identifier,
                            $"Infinite loop in question selection. " +
                            $"Last Question:{lastQuestionName}," +
                            $"Last word: {word?.Word}", true);
                        break;
                    }
                    
                    // cancel if question is absolutely the same with previous
                    if (lastExamResult?.Results!= ExamResult.Retry 
                        && lastWord == word 
                        && lastQuestionName == exam.Name)
                        continue;
                    lastWord = word;
                    lastQuestionName = exam.Name;
                    
                    retryFlag = false;
                    var questionMetric = new QuestionMetric(word, exam.Name);

                    var learnList = learningWords;

                    if (!learningWords.Contains(word))
                        learnList = learningWords.Append(word).ToArray();
                        
                    if (i>1 && exam.NeedClearScreen && lastExamResult.Results != ExamResult.Impossible)
                    {
                        await WriteDontPeakMessage(lastExamResult.ResultsBeforeHideousText);
                    }
                    _user.OnAnyActivity();
                    var originRate =word.Score;

                    var result = await exam.Pass(_chatIo, word, learnList);

                    switch (result.Results)
                    {
                        case ExamResult.Impossible:
                            exam = QuestionSelector.Singletone.GetNextQuestionFor(i == 0, word);
                            retryFlag = true;
                            break;
                        case ExamResult.Passed:
                            var succTask = _usersWordsService.RegisterSuccess(word);
                            questionsCount++;
                            questionsPassed++;
                            questionMetric.OnExamFinished(word.Score, true ); 
                            Botlog.SaveQuestionMetricInfo(questionMetric, _chatIo.ChatId);
                            await succTask;
                            _user.OnQuestionPassed(word.Score - originRate);
                            break;
                        case ExamResult.Failed:
                            var failureTask = _usersWordsService.RegisterFailure(word);
                            questionMetric.OnExamFinished(word.Score, false );
                            Botlog.SaveQuestionMetricInfo(questionMetric, _chatIo.ChatId);
                            questionsCount++;
                            await failureTask;
                            _user.OnQuestionFailed(word.Score - originRate);
                            break;
                        case ExamResult.Ignored:
                            break;
                        case ExamResult.Retry:
                            retryFlag = true;
                            break;
                    }
                    
                    if(!string.IsNullOrWhiteSpace(result.OpenResultsText))
                        await _chatIo.SendMessageAsync(result.OpenResultsText);

                    lastExamResult = result;

                } while (retryFlag);
                
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
            await updateUserTask;
            await finializeScoreUpdateTask;
            await _chatIo.SendMarkdownMessageAsync(doneMessage,
            new[]{new[] { InlineButtons.ExamText("🔁 One more learn")}, 
                  new[] { InlineButtons.Stats,InlineButtons.Translation}});
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

            var doneMessage = new StringBuilder($"*Learning done:* {questionsPassed}/{questionsCount}\r\n" +
                                                $"*Words in test:* {learningWords.Length}\r\n");

            if (newWellLearnedWords.Any())
            {
                if (newWellLearnedWords.Count > 1)
                    doneMessage.Append($"*\r\nYou have learned {newWellLearnedWords.Count} words:*\r\n");
                else
                    doneMessage.Append($"*\r\nYou have learned one word:*\r\n");
                foreach (var word in newWellLearnedWords)
                {
                    doneMessage.Append("✅ " + word.Word + "\r\n");
                }
            }

            if (forgottenWords.Any())
            {
                if(forgottenWords.Count>1)
                    doneMessage.Append($"\r\n*You forgot {forgottenWords} words:*\r\n");
                else
                    doneMessage.Append("\r\n*You forgot one word:*\r\n");
                foreach (var word in forgottenWords)
                {
                    doneMessage.Append("❗ " + word.Word + "\r\n\r\n");
                }
            }
            
            /*
            doneMessage.Append("\r\n*All words in test:*\r\n");
            var emoji = "";
            foreach (var word in distinctLearningWords)
            {
                if (word.AbsoluteScore > 4) emoji = "✅";
                else if (word.AbsoluteScore <= 4 || word.AbsoluteScore > 1) emoji = "👌";
                else emoji = "❌";
                doneMessage.Append(emoji+" "+word.Word + "   " + word.TranslationAsList + "\r\n");
            }*/

            doneMessage.Append(($"\r\n*Earned score:* " + $"{(int)(_user.GamingScore - gamingScoreBefore)}").Replace("-","\\-"));
            doneMessage.Append(($"\r\n*Total score:* {(int) _user.GamingScore}\r\n").Replace("-","\\-"));

            return doneMessage.ToString();
        }

        private async Task WriteDontPeakMessage(string resultsBeforeHideousText)
        {
            //it is not an empty string;
            // it contains invisible character, that allows to show blank message
            string emptySymbol = "‎";
            
            await _chatIo.SendMessageAsync(
                $"\r\n\r\n{emptySymbol}‎\r\n{emptySymbol}‎\r\n{emptySymbol}\r\n{emptySymbol}\r\n{emptySymbol}\r\n{emptySymbol}\r\n{emptySymbol}\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n{resultsBeforeHideousText}\r\n\r\n" +
                $"Now try to answer without hints. Don't peek upward!\r\n");
            await _chatIo.SendMessageAsync("\U0001F648");
        }
    }
}

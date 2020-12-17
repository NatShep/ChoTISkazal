using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        private readonly UserService _userService;
        private readonly UsersWordsService _usersWordsService;

        public ExamFlow(
            ChatIO chatIo, 
            UserService userService,
            UsersWordsService usersWordsService, 
            ExamSettings examSettings)
        {
            _chatIo = chatIo;
            _userService = userService;
            _usersWordsService = usersWordsService;
            _examSettings = examSettings;
        }

        public async Task EnterAsync(UserModel user)
        {
            if (!await _usersWordsService.HasWords(user))
            {
                await _chatIo.SendMessageAsync("You need to add some more words before examination");
                return;
            }
            
            //Randomization and jobs
            //if (RandomTools.Rnd.Next() % 30 == 0)
            //    await _usersWordsService.AddMutualPhrasesToVocabAsync(user, 10);
            // else
            
            var startupScoreUpdate =  _usersWordsService.UpdateCurrentScoreForRandomWords(user, _examSettings.MaxLearningWordsCountInOneExam*2);
            var typing =  _chatIo.SendTyping();

            var c = Rand.RandomIn(_examSettings.MinLearningWordsCountInOneExam,
                _examSettings.MaxLearningWordsCountInOneExam);
            await startupScoreUpdate;
            await typing;
            var learningWords 
                = await _usersWordsService.GetWordsForLearningWithPhrasesAsync(user, c, 3);
      
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
                = (await _usersWordsService.AppendAdvancedWordsToExamList(user, learningWords, _examSettings));
            
            var distinctLearningWords = learningAndAdvancedWords.Distinct().ToArray();
            
            var wordAndScore = new Dictionary<string,double>();
            foreach (var word in distinctLearningWords)
            {
                if (!wordAndScore.ContainsKey(word.Word))
                    wordAndScore.Add(word.Word,word.AbsoluteScore);
            }

            var gamingScoreBefore = user.GamingScore;
            //var scoresBefore = learningAndAdvancedWords.Distinct().Select(l => l.Score).ToArray();
            
            var questionsCount = 0;
            var questionsPassed = 0;
            var i = 0;
            ExamResult? lastExamResult = null;
         
            foreach (var word in learningAndAdvancedWords)
            {
                var allLearningWordsWereShowedAtLeastOneTime = i < learningWordsCount;
                var exam = QuestionSelector.Singletone.GetNextQuestionFor(allLearningWordsWereShowedAtLeastOneTime, word);
                i++;
                var retryFlag = false;
                do
                {
                    retryFlag = false;
                    var questionMetric = new QuestionMetric(word, exam.Name);

                    var learnList = learningWords;

                    if (!learningWords.Contains(word))
                        learnList = learningWords.Append(word).ToArray();

                    if (i>1 && exam.NeedClearScreen && lastExamResult != ExamResult.Impossible)
                    {
                        await WriteDontPeakMessage();
                        if (lastExamResult == ExamResult.Passed)
                            await WritePassed();
                    }
                    user.OnAnyActivity();
                    var originRate =word.Score;

                    var result = await exam.Pass(_chatIo, word, learnList);

                    switch (result)
                    {
                        case ExamResult.Impossible:
                            exam = QuestionSelector.Singletone.GetNextQuestionFor(i == 0, word);
                            retryFlag = true;
                            break;
                        case ExamResult.Passed:
                            var succTask = _usersWordsService.RegisterSuccess(word);
                            await WritePassed();
                            questionsCount++;
                            questionsPassed++;
                            questionMetric.OnExamFinished(word.Score, true ); 
                            Botlog.SaveQuestionMetricInfo(questionMetric, _chatIo.ChatId);
                            await succTask;
                            user.OnQuestionPassed(word.Score - originRate);
                            break;
                        case ExamResult.Failed:
                            var failureTask = _usersWordsService.RegisterFailure(word);
                            await WriteFailed();
                            questionMetric.OnExamFinished(word.Score, false );
                            Botlog.SaveQuestionMetricInfo(questionMetric, _chatIo.ChatId);
                            questionsCount++;
                            await failureTask;
                            user.OnQuestionFailed(word.Score - originRate);
                            break;
                        case ExamResult.Retry:
                            retryFlag = true;
                            break;
                        case ExamResult.Exit: return;
                    }

                    lastExamResult = result;

                } while (retryFlag);
                
                Botlog.RegisterExamInfo(user.TelegramId, started, questionsCount, questionsPassed);
            }              
            user.OnLearningDone();
            var updateUserTask = _userService.Update(user);
            var finializeScoreUpdateTask =_usersWordsService.UpdateCurrentScoreForRandomWords(user,10);

            //info after examination
           var wellLearnedWords 
                = learningAndAdvancedWords.Distinct().Where(y => y.AbsoluteScore >= 4).ToArray();

            var newWellLearnedWords=new List<UserWordModel>();
            var forgottenWords = new List<UserWordModel>();
            
            foreach (var word in distinctLearningWords)
            {
                if (word.AbsoluteScore > 4)
                {
                    if (wordAndScore[word.Word]<=4)
                        newWellLearnedWords.Add(word);
                }
                else
                {
                    if (wordAndScore[word.Word]>4)
                        forgottenWords.Add(word);
                }
            }

            var doneMessage = new StringBuilder("*Learning done:*\r\n" +
                                                $"Count of questions: {questionsCount}\r\n" +
                                                $"Passed questions: {questionsPassed}\r\n" +
                                                $"Words in test: {learningWords.Length}\r\n\r\n"); 
            
          if (newWellLearnedWords.Any())
            {
                doneMessage.Append($"*You have learned {newWellLearnedWords.Count} words:*\r\n");
                foreach (var word in newWellLearnedWords)
                {
                    doneMessage.Append("✅ "+word.Word + "\r\n");
                }
            }
            if (forgottenWords.Any())
            {
                doneMessage.Append($"\r\n" +
                                   $"*Forgotten words:*\r\n");
                foreach (var word in forgottenWords)
                {
                    doneMessage.Append("❗ "+word.Word + "\r\n\r\n");
                }
            }

            doneMessage.Append("*All words in test:*\r\n");
            var emoji = "";
            foreach (var word in distinctLearningWords)
            {
                if (word.AbsoluteScore > 4) emoji = "✅";
                else if (word.AbsoluteScore <= 4 || word.AbsoluteScore > 1) emoji = "👌";
                else emoji = "❌";
                doneMessage.Append(emoji+" "+word.Word + "   " + word.TranslationAsList + "\r\n");
            }

            doneMessage.Append($"\r\n*Earned score:* "+$"{user.GamingScore-gamingScoreBefore}");
            doneMessage.Append($"\r\n*Total score:* "+ user.GamingScore+"\r\n");
            
            await updateUserTask;
            await finializeScoreUpdateTask; 
            /*var scoresAfter = learningAndAdvancedWords.Distinct().Select(l => l.Score).ToArray();
            var changing = WordStatsChanging.Zero;
            for (int j = 0; j < scoresBefore.Length; j++)
            {
                changing += scoresAfter[j] - scoresBefore[j];
            }*/

            //doneMessage.Append(
            //    $"Changings. as:{(int) changing.AbsoluteScoreChanging} od:{changing.OutdatedChanging}\r\ncat: {string.Join(",", changing.WordScoreChangings)}\r\n"
            //        .Replace("-", "\\-"));
          //  doneMessage.Append("\r\nEnter new word to translate or /start to return to main menu");
            await _chatIo.SendMarkdownMessageAsync(doneMessage.ToString(),
            new[]{new[]{
                        InlineButtons.Exam, InlineButtons.Stats}, 
                    new[]{ InlineButtons.EnterWords}});
        }

        private async Task WriteDontPeakMessage()
        {
            await _chatIo.SendMessageAsync(
                "\r\n.\r\n.\r\n.\r\n.\r\n.\r\n.\r\n.\r\n.\r\n.\r\n.\r\n.\r\n.\r\n.\r\n.\r\n.\r\n.\r\n.\r\n.\r\n.\r\n.");
            await _chatIo.SendMessageAsync(
                "Don't peek");
            await _chatIo.SendMessageAsync("\U0001F648");
        }

        private Task WriteFailed() => _chatIo.SendMessageAsync("Noo... \U0001F61E");

        private Task WritePassed() => _chatIo.SendMessageAsync($"It's right! \U0001F609");
    }
}

﻿using System;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Chotiskazal.Bot.Questions;
using SayWhat.Bll;
using SayWhat.Bll.Services;
using SayWhat.MongoDAL;
using SayWhat.MongoDAL.Users;
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
            if (learningWords.Average(w => w.AbsoluteScore) <= 4)
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
                = await _usersWordsService.AppendAdvancedWordsToExamList(user, learningWords,_examSettings);

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
                    var sw = Stopwatch.StartNew();
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
                    var result = await exam.Pass(_chatIo, word, learnList);
                    
                    sw.Stop();
                    questionMetric.ElaspedMs = (int) sw.ElapsedMilliseconds;
                    var originRate =word.Score;
                    
                    switch (result)
                    {
                        case ExamResult.Impossible:
                            exam = QuestionSelector.Singletone.GetNextQuestionFor(i == 0, word);
                            retryFlag = true;
                            break;
                        case ExamResult.Passed:
                                                        
                           var succTask = _usersWordsService.RegisterSuccess(word);

                            await WritePassed();
                            Botlog.SaveQuestionMetricInfo(questionMetric,_chatIo.ChatId );
                            questionsCount++;
                            questionsPassed++;
                           
                            await succTask;
                            user.OnQuestionPassed(word.Score - originRate);
                           break;
                        case ExamResult.Failed:
                            var failureTask = _usersWordsService.RegisterFailure(word);
                            await WriteFailed();
                            questionMetric.Result = 0;
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

            var doneMessage = new StringBuilder($"*Learning done:  {questionsPassed}/{questionsCount}*\r\n\r\n```\r\n");
            foreach (var pairModel in learningAndAdvancedWords.Distinct())
            {
                doneMessage.Append(pairModel.Word + "  -  " + pairModel.TranslationAsList + "  (" +
                                   pairModel.AbsoluteScore + ")\r\n");
            }

            doneMessage.Append("```\r\n\r\nEnter new word to translate or /start to return to main menu");
            await _chatIo.SendMarkdownMessageAsync(doneMessage.ToString());
            await updateUserTask;
            await finializeScoreUpdateTask;
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

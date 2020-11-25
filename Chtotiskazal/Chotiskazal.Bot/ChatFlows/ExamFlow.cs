using System;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Chotiskazal.Bot.Questions;
using SayWhat.Bll;
using SayWhat.Bll.Services;
using SayWhat.MongoDAL.Users;
using Telegram.Bot.Types.ReplyMarkups;

namespace Chotiskazal.Bot.ChatFlows
{
    public class ExamFlow
    {
        private readonly ChatIO _chatIo;
        private readonly UsersWordsService _usersWordsService;

        public ExamFlow(
            ChatIO chatIo, 
            UsersWordsService usersWordsService)
        {
            _chatIo = chatIo;
            _usersWordsService = usersWordsService;
        }

        public async Task EnterAsync(User user)
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
            
            var startupScoreUpdate =  _usersWordsService.UpdateCurrentScore(user,10);
            var _ =  _chatIo.SendTyping();
                            
            var sb = new StringBuilder("Examination\r\n");
            var learningWords = await _usersWordsService.GetWordsForLearningWithPhrasesAsync(user, 9, 3);
            if (learningWords.Average(w => w.AbsoluteScore) <= 4)
            {
                foreach (var pairModel in learningWords.Randomize())
                {
                    sb.AppendLine($"{pairModel.Word}\t\t:{pairModel.TranslationAsList}");
                }
            }

            var startMessageSending = _chatIo.SendMessageAsync(sb.ToString(), new InlineKeyboardButton
            {
                CallbackData = "/startExamination",
                Text = "Start"
            }, new InlineKeyboardButton
            {
                CallbackData = "/start",
                Text = "Cancel",
            });

            //Get exam list and test words
            var examsList = ExamHelper.PrepareExamList(learningWords);
            var testWords = await _usersWordsService.GetWordsForAdvancedQuestions(user, examsList);
            examsList.AddRange(testWords);

            var questionsCount = 0;
            var questionsPassed = 0;
            var i = 0;
            ExamResult? lastExamResult = null;

            await startMessageSending;
            var started = DateTime.Now;
            var userInput = await _chatIo.WaitInlineKeyboardInput();
            if (userInput != "/startExamination")
                return;
            foreach (var word in examsList)
            {
                var exam = ExamSelector.Singletone.GetNextExamFor(i < 9, word);
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

                    if (exam.NeedClearScreen && lastExamResult != ExamResult.Impossible)
                    {
                        await WriteDontPeakMessage();
                        if (lastExamResult == ExamResult.Passed)
                            await WritePassed();
                    }

                    var result = await exam.Pass(_chatIo, _usersWordsService, word, learnList);

                    sw.Stop();
                    questionMetric.ElaspedMs = (int) sw.ElapsedMilliseconds;
                    switch (result)
                    {
                        case ExamResult.Impossible:
                            exam = ExamSelector.Singletone.GetNextExamFor(i == 0, word);
                            retryFlag = true;
                            break;
                        case ExamResult.Passed:
                            await WritePassed();
                            Botlog.SaveQuestionMetric(questionMetric);
                            questionsCount++;
                            questionsPassed++;
                            break;
                        case ExamResult.Failed:
                            await WriteFailed();
                            questionMetric.Result = 0;
                            Botlog.SaveQuestionMetric(questionMetric);
                            questionsCount++;
                            break;
                        case ExamResult.Retry:
                            retryFlag = true;
                            break;
                        case ExamResult.Exit: return;
                    }

                    lastExamResult = result;

                } while (retryFlag);
                //run asyncroniously
                var finializeScoreUpdateTask =_usersWordsService.UpdateCurrentScore(user,10);
                
                Botlog.RegisterExamAsync(user.TelegramId, started, questionsCount, questionsPassed);
            }

            var doneMessage = new StringBuilder($"Test done:  {questionsPassed}/{questionsCount}\r\n");
            foreach (var pairModel in learningWords.Concat(testWords))
            {
                doneMessage.Append(pairModel.Word + " - " + pairModel.TranslationAsList + "  (" +
                                   pairModel.AbsoluteScore + ")\r\n");
            }

            await _chatIo.SendMessageAsync(doneMessage.ToString());
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

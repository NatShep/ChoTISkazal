using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Chotiskazal.Bot.Questions;
using Chotiskazal.ConsoleTesting.Services;
using Chotiskazal.Dal;
using Chotiskazal.DAL;
using Chotiskazal.DAL.Services;
using Telegram.Bot.Types.ReplyMarkups;

namespace Chotiskazal.Bot.ChatFlows
{
    
    public class ExamFlow
    {
        private readonly Chat _chat;
        private readonly ExamService _examService;

        public ExamFlow(Chat chat , ExamService service)
        {
            _chat = chat;
            _examService = service;
        }

        public async Task EnterAsync(int userId)
        {
           
            /*if (!await _examService.HasAnyAsync())
            {
                await _chat.SendMessage("You need to add some words before examination");
                return;
            }
            */
            
            //Randomization and jobs
            await _examService.RandomizationAndJobsAsync(userId);

            
            var sb = new StringBuilder("Examination\r\n");
            var learningWords =await _examService.GetWordsForLearningWithPhrasesAsync(userId, 9, 3);
            if (learningWords.Average(w => w.PassedScore) <= 4)
            {
                foreach (var pairModel in learningWords.Randomize())
                {
                    sb.AppendLine($"{pairModel.EnWord}\t\t:{pairModel.UserTranslations}");
                }
            }

      
            var startMessageSending = _chat.SendMessage(sb.ToString(), new InlineKeyboardButton {
                CallbackData = "/startExamination", 
                Text = "Start"
            }, new InlineKeyboardButton
            {
                CallbackData = "/start",
                Text= "Cancel",
            });
            
            //Get exam list and test words
            var examsList =  _examService.PreparingExamsList(learningWords);
            var testWords = await _examService.GetTestWordsAsync(userId, examsList);
            examsList.AddRange(testWords);

        
            int examsCount = 0;
            int examsPassed = 0;
            int i = 0;
            ExamResult? lastExamResult = null;
            
            await startMessageSending;
            DateTime started = DateTime.Now;
            var userInput = await _chat.WaitInlineKeyboardInput();
            if (userInput != "/startExamination")
                return;
            foreach (var pairModel in examsList)
            {
                var exam = ExamSelector.GetNextExamFor(i < 9, pairModel);
                i++;
                bool retryFlag = false;
                do
                {
                    retryFlag = false;
                    Stopwatch sw = Stopwatch.StartNew();
                    var questionMetric = _examService.CreateQuestionMetric(pairModel, exam);

                    var learnList = learningWords;

                    if (!learningWords.Contains(pairModel))
                        learnList = learningWords.Append(pairModel).ToArray();

                    if (exam.NeedClearScreen && lastExamResult != ExamResult.Impossible)
                    {
                        await WriteDontPeakMessage();
                        if (lastExamResult == ExamResult.Passed)
                            await WritePassed();
                    }

                    var result = await exam.Pass(_chat, _examService, pairModel, learnList);

                    sw.Stop();
                    questionMetric.ElaspedMs = (int) sw.ElapsedMilliseconds;
                    switch (result)
                    {
                        case ExamResult.Impossible:
                            exam = ExamSelector.GetNextExamFor(i == 0, pairModel);
                            retryFlag = true;
                            break;
                        case ExamResult.Passed:
                            await WritePassed();
                            await _examService.SaveQuestionMetrics(questionMetric);
                            examsCount++;
                            examsPassed++;
                            break;
                        case ExamResult.Failed:
                            await WriteFailed();
                            questionMetric.Result = 0;
                            await _examService.SaveQuestionMetrics(questionMetric);
                            examsCount++;
                            break;
                        case ExamResult.Retry:
                            retryFlag = true;
                            Console.WriteLine();
                            Console.WriteLine();
                            break;
                        case ExamResult.Exit: return;
                    }
                    lastExamResult = result;

                } while (retryFlag);


                await _examService.RegistrateExamAsync(userId, started, examsCount, examsPassed);

            }
            var doneMessage = new StringBuilder($"Test done:  {examsPassed}/{examsCount}\r\n");
            foreach (var pairModel in learningWords.Concat(testWords))
            {
                doneMessage.Append(pairModel.EnWord + " - " + pairModel.UserTranslations + "  (" + pairModel.PassedScore +
                                  ")\r\n");
            }
            await _chat.SendMessage(doneMessage.ToString());
        }

        private async Task WriteDontPeakMessage() => await _chat.SendMessage("Don't peek\r\n.\r\n.\r\n.\r\n.\r\n.\r\n.\r\n.\r\n.\r\n.\r\n.\r\n.\r\n..\r\n.\r\n.\r\n.Don't peek");

        private Task WriteFailed() => _chat.SendMessage("Noo... \U0001F61E");
        private Task WritePassed() => _chat.SendMessage($"It's right! \U0001F609");
        private static QuestionMetric CreateQuestionMetric(UserWordForLearning pairModel, IExam exam) =>
            new QuestionMetric
            {
                AggregateScoreBefore = pairModel.AggregateScore,
                WordId = pairModel.Id,
                Created = DateTime.Now,
                ExamsPassed = pairModel.Examed,
                PassedScoreBefore = pairModel.PassedScore,
                PhrasesCount = pairModel.Phrases?.Count ?? 0,
                PreviousExam = pairModel.LastExam,
                Type = exam.Name,
            };
    }
}

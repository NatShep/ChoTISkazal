using System;
using Chotiskazal.Api.Models;
using Chotiskazal.ConsoleTesting.Services;

namespace Chotiskazal.ApI.Exams
{
    public class EnTrustExam : IExam
    {
        public bool NeedClearScreen => false;
        public string Name => "Eng trust";

        public ExamResult Pass(ExamService service, WordForLearning word, WordForLearning[] examList)
        {
            Console.WriteLine("=====>   " + word.OriginWord + "    <=====");
            Console.WriteLine("Press any key to see the translation...");
            Console.ReadKey();

            Console.WriteLine("Translation is \r\n" + word.Translations + "\r\n Did you guess?");
            Console.WriteLine("[Y]es [N]o [E]xit");
            var answer = Console.ReadKey();
            switch (answer.Key)
            {
                case ConsoleKey.Y:
                    service.RegistrateSuccess(word.MetricId);
                    return ExamResult.Passed;
                case ConsoleKey.N:
                    service.RegistrateFailure(word.MetricId);
                    return ExamResult.Failed;
                case ConsoleKey.E: return ExamResult.Exit;
                case ConsoleKey.Escape: return ExamResult.Exit;
                default: return ExamResult.Retry;
            }
        }
    }
}
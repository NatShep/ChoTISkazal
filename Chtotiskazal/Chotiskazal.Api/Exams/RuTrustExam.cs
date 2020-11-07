using System;
using Chotiskazal.ConsoleTesting.Services;
using Chotiskazal.DAL;

namespace Chotiskazal.ApI.Exams
{
    public class RuTrustExam : IExam
    {
        public bool NeedClearScreen => false;

        public string Name => "Ru trust";

        public ExamResult Pass(ExamService service, UserWordForLearning word, UserWordForLearning[] examList)
        {
            Console.WriteLine("=====>   " + word.UserTranslations + "    <=====");
            Console.WriteLine("Press any key to see the translation...");
            Console.ReadKey();
            Console.WriteLine("Translation is \r\n" + word.EnWord + "\r\n Did you guess?");
            Console.WriteLine("[Y]es [N]o [E]xit");
            var answer = Console.ReadKey();
            switch (answer.Key)
            {
                case ConsoleKey.Y:
                    service.RegistrateSuccess(word);
                    return ExamResult.Passed;
                case ConsoleKey.N:
                    service.RegistrateFailure(word);
                    return ExamResult.Failed;
                case ConsoleKey.E: return ExamResult.Exit;
                case ConsoleKey.Escape: return ExamResult.Exit;
                default: return ExamResult.Retry;
            }

        }
    }
}
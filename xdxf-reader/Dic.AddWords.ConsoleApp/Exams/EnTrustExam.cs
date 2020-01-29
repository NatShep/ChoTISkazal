using System;
using Dic.Logic.DAL;
using Dic.Logic.Services;

namespace Dic.AddWords.ConsoleApp.Exams
{
    public class EnTrustExam : IExam
    {
        public string Name => "Eng trust";

        public ExamResult Pass(NewWordsService service, PairModel word, PairModel[] examList)
        {
            Console.WriteLine("=====>   " + word.OriginWord + "    <=====");
            Console.WriteLine("Press any key to see the translation...");
            Console.ReadKey();

            Console.WriteLine("Translation is \r\n" + word.Translation + "\r\n Did you guess?");
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
using System;
using Dic.Logic.DAL;
using Dic.Logic.Services;

namespace Dic.AddWords.ConsoleApp
{
    public class RuTrustExam : IExam
    {
        public string Name => "Ru trust";

        public ExamResult Pass(NewWordsService service, PairModel word, PairModel[] examList)
        {
            Console.WriteLine("=====>   " + word.Translation + "    <=====");
            Console.WriteLine("Press any key to see the translation...");
            Console.ReadKey();
            Console.WriteLine("Translation is \r\n" + word.OriginWord + "\r\n Did you guess?");
            Console.WriteLine("[Y]es [N]o [E]xit");
            var answer = Console.ReadKey();
            switch (answer.Key)
            {
                case ConsoleKey.Y: return ExamResult.Passed;
                case ConsoleKey.N: return ExamResult.Failed;
                case ConsoleKey.Escape: return ExamResult.Exit;
                default: return ExamResult.Retry;
            }

        }
    }
}
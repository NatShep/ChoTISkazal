using System;
using System.Linq;
using Dic.Logic.DAL;
using Dic.Logic.Services;

namespace Dic.AddWords.ConsoleApp
{
    public class RuWriteExam : IExam
    {
        public string Name => "Eng Write";

        public ExamResult Pass(NewWordsService service, PairModel word, PairModel[] examList)
        {
            var words = word.OriginWord.Split(',').Select(s => s.Trim());
            if (words.All(t => t.Contains(' ')))
                return new RuTrustExam().Pass(service, word, examList);

            Console.WriteLine("=====>   " + word.Translation+ "    <=====");

            Console.Write("Write the translation: ");
            var userEntry = Console.ReadLine();
            if (string.IsNullOrEmpty(userEntry))
                return ExamResult.Retry;

            if (words.Any(t => string.Compare(userEntry, t, StringComparison.OrdinalIgnoreCase) == 0))
            {
                service.RegistrateSuccess(word);
                return ExamResult.Passed;
            }
            else
            {
                Console.WriteLine("The translation was: " + word.OriginWord);
                service.RegistrateFailure(word);
                return ExamResult.Failed;
            }
        }
    }
}
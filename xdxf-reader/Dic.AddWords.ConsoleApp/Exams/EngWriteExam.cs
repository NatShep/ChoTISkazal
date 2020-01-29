using System;
using System.Linq;
using Dic.Logic.DAL;
using Dic.Logic.Services;

namespace Dic.AddWords.ConsoleApp.Exams
{
    public class EngWriteExam : IExam
    {
        public string Name => "Eng Write";

        public ExamResult Pass(NewWordsService service, PairModel word, PairModel[] examList)
        {
            var translations = word.Translation.Split(',').Select(s => s.Trim());
            if (translations.All(t => t.Contains(' ')))
                return ExamResult.Impossible;


            Console.WriteLine("=====>   " + word.OriginWord + "    <=====");

            Console.Write("Write the translation: ");
            var translation = Console.ReadLine();
            if (string.IsNullOrEmpty(translation))
                return ExamResult.Retry;

            if (translations.Any(t => string.Compare(translation, t, StringComparison.OrdinalIgnoreCase) == 0))
            {
                service.RegistrateSuccess(word);
                return ExamResult.Passed;
            }
            else
            {
                Console.WriteLine("The translation was: "+ word.Translation);
                service.RegistrateFailure(word);
                return ExamResult.Failed;
            }
        }
    }
}
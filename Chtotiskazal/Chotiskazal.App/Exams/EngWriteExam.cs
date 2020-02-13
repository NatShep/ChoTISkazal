using System;
using System.Linq;
using Chotiskazal.Logic.Services;
using Dic.Logic.DAL;

namespace Chotiskazal.App.Exams
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
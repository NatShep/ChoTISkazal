using System;
using System.Linq;
using Dic.Logic;
using Dic.Logic.DAL;
using Dic.Logic.Services;

namespace Chotiskazal.App.Exams
{
    public class EngPhraseSubstitudeExam: IExam
    {
        public string Name => "Eng phrase substitude";
        public ExamResult Pass(NewWordsService service, PairModel word, PairModel[] examList)
        {
            if (!word.Phrases.Any())
                return ExamResult.Impossible;

            var phrase =  word.Phrases.GetRandomItem();
            var replaced =  phrase.Origin.Replace(phrase.OriginWord, "...");
            if (replaced == phrase.Origin)
                return ExamResult.Impossible;
            Console.WriteLine($"\"{phrase.Translation}\"");
            Console.WriteLine($" translated as ");
            Console.WriteLine($"\"{replaced}\"");
            Console.WriteLine();
            Console.Write($"Enter missing word: ");
            while (true)
            {
                var enter = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(enter))
                    continue;
                if (string.CompareOrdinal(word.OriginWord.ToLower().Trim(), enter.ToLower().Trim()) == 0)
                {
                    service.RegistrateSuccess(word);
                    return ExamResult.Passed;
                }

                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Origin phrase was \"{phrase.Origin}\"");
                Console.ResetColor();
                service.RegistrateFailure(word);

                return ExamResult.Failed;
            }
        }
    }
}

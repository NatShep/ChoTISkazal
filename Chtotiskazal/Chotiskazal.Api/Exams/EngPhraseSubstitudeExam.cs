using System;
using System.Linq;
using Chotiskazal.Api.Models;
using Chotiskazal.ConsoleTesting.Services;
using Chotiskazal.LogicR;

namespace Chotiskazal.ApI.Exams
{
    public class EngPhraseSubstitudeExam: IExam
    {
        public bool NeedClearScreen => false;
        public string Name => "Eng phrase substitude";
        public ExamResult Pass(ExamService service, WordForLearning word, WordForLearning[] examList)
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
                    service.RegistrateSuccess(word.MetricId);
                    return ExamResult.Passed;
                }

                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Origin phrase was \"{phrase.Origin}\"");
                Console.ResetColor();
                service.RegistrateFailure(word.MetricId);

                return ExamResult.Failed;
            }
        }
    }
}

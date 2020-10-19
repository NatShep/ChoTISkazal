using System;
using System.Linq;
using Chotiskazal.Api.Models;
using Chotiskazal.ConsoleTesting.Services;
using Chotiskazal.LogicR;

namespace Chotiskazal.ApI.Exams
{
    public class RuPhraseSubstitudeExam : IExam
    {
        public bool NeedClearScreen => false;

        public string Name => "Ru phrase substitude";
        public ExamResult Pass(ExamService service, WordForLearning word, WordForLearning[] examList)
        {
            if (!word.Phrases.Any())
                return ExamResult.Impossible;

            var phrase = word.Phrases.GetRandomItem();
            
            var replaced = phrase.Translation.Replace(phrase.TranslationWord, "...");
            if (replaced == phrase.Translation)
                return ExamResult.Impossible;

            Console.WriteLine($"\"{phrase.Origin}\"");
            Console.WriteLine($" translated as ");
            Console.WriteLine($"\"{replaced}\"");
            Console.WriteLine();
            Console.Write($"Enter missing word: ");
            while (true)
            {
                var enter = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(enter))
                    continue;
                if (string.CompareOrdinal(phrase.TranslationWord.ToLower().Trim(), enter.ToLower().Trim()) == 0)
                {
                    service.RegistrateSuccess(word.MetricId);
                    return ExamResult.Passed;
                }

                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Origin phrase was \"{phrase.Translation}\"");
                Console.ResetColor();
                service.RegistrateFailure(word.MetricId);
                return ExamResult.Failed;
            }
        }
    }
}
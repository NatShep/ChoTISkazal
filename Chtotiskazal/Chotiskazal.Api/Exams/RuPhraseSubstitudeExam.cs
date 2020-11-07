using System;
using System.Linq;
using Chotiskazal.Api.Services;
using Chotiskazal.ConsoleTesting.Services;
using Chotiskazal.DAL;
using Chotiskazal.DAL.Services;
using Chotiskazal.LogicR;

namespace Chotiskazal.ApI.Exams
{
    public class RuPhraseSubstitudeExam : IExam
    {
        public bool NeedClearScreen => false;

        public string Name => "Ru phrase substitude";
        public ExamResult Pass(ExamService service, UserWordForLearning word, UserWordForLearning[] examList)
        {
            if (!word.Phrases.Any())
                return ExamResult.Impossible;

            var phrase = word.Phrases.GetRandomItem();
            
            var replaced = phrase.PhraseRuTranslate.Replace(phrase.WordTranslate, "...");
            if (replaced == phrase.PhraseRuTranslate)
                return ExamResult.Impossible;

            Console.WriteLine($"\"{phrase.EnPhrase}\"");
            Console.WriteLine($" translated as ");
            Console.WriteLine($"\"{replaced}\"");
            Console.WriteLine();
            Console.Write($"Enter missing word: ");
            while (true)
            {
                var enter = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(enter))
                    continue;
                if (string.CompareOrdinal(phrase.WordTranslate.ToLower().Trim(), enter.ToLower().Trim()) == 0)
                {
                    service.RegistrateSuccess(word);
                    return ExamResult.Passed;
                }

                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Origin phrase was \"{phrase.PhraseRuTranslate}\"");
                Console.ResetColor();
                service.RegistrateFailure(word);
                return ExamResult.Failed;
            }
        }
    }
}
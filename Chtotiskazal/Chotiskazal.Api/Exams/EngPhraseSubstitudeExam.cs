using System;
using System.Linq;
using Chotiskazal.Api.Services;
using Chotiskazal.ConsoleTesting.Services;
using Chotiskazal.DAL;
using Chotiskazal.DAL.Services;
using Chotiskazal.LogicR;

namespace Chotiskazal.ApI.Exams
{
    public class EngPhraseSubstitudeExam: IExam
    {
        public bool NeedClearScreen => false;
        public string Name => "Eng phrase substitude";
        public ExamResult Pass(ExamService service, UserWordForLearning word, UserWordForLearning[] examList)
        {
            if (!word.Phrases.Any())
                return ExamResult.Impossible;

            var phrase =  word.Phrases.GetRandomItem();
            var replaced =  phrase.EnPhrase.Replace(phrase.EnWord, "...");
            if (replaced == phrase.EnPhrase)
                return ExamResult.Impossible;
            Console.WriteLine($"\"{phrase.PhraseRuTranslate}\"");
            Console.WriteLine($" translated as ");
            Console.WriteLine($"\"{replaced}\"");
            Console.WriteLine();
            Console.Write($"Enter missing word: ");
            while (true)
            {
                var enter = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(enter))
                    continue;
                if (string.CompareOrdinal(word.EnWord.ToLower().Trim(), enter.ToLower().Trim()) == 0)
                {
                    service.RegistrateSuccessAsync(word);
                    return ExamResult.Passed;
                }

                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Origin phrase was \"{phrase.EnPhrase}\"");
                Console.ResetColor();
                service.RegistrateFailureAsync(word);

                return ExamResult.Failed;
            }
        }
    }
}

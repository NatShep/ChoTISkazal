using System;
using System.Linq;
using Chotiskazal.Api.ConsoleModes;
using Chotiskazal.Api.Services;
using Chotiskazal.ConsoleTesting.Services;
using Chotiskazal.DAL;
using Chotiskazal.DAL.Services;
using Chotiskazal.LogicR;

namespace Chotiskazal.ApI.Exams
{
    public class EngChooseWordInPhraseExam : IExam
    {
        public bool NeedClearScreen => false;

        public string Name => "Eng Choose word in phrase";

        public ExamResult Pass(ExamService service, UserWordForLearning word, UserWordForLearning[] examList)
        {
            if (!word.Phrases.Any())
                return ExamResult.Impossible;
            
            var phrase = word.Phrases.GetRandomItem();

            var replaced = phrase.EnPhrase.Replace(phrase.EnWord, "...");
            if (replaced == phrase.EnWord)
                return ExamResult.Impossible;
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"\"{phrase.PhraseRuTranslate}\"");
            Console.ResetColor();
            Console.WriteLine();
            Console.WriteLine($" translated as ");
            Console.WriteLine();
            Console.WriteLine($"\"{replaced}\"");
            Console.ResetColor();

            Console.WriteLine();
            Console.WriteLine($"Choose missing word: ");

            var variants = examList.Randomize().Select(e => e.EnWord).ToArray();

            for (int i = 1; i <= variants.Length; i++)
            {
                Console.WriteLine($"{i}: " + variants[i - 1]);
            }

            Console.Write("Choose the translation: ");

            var selected = Console.ReadLine();
            if (selected.ToLower().StartsWith("e"))
                return ExamResult.Exit;

            if (!int.TryParse(selected, out var selectedIndex) || selectedIndex > variants.Length ||
                selectedIndex < 1)
                return ExamResult.Retry;

            if (variants[selectedIndex - 1] == word.EnWord)
            {
                service.RegistrateSuccess(word);
                return ExamResult.Passed;
            }

            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Origin was: \"{phrase.EnPhrase}\"");
            Console.ResetColor();
            
            service.RegistrateFailure(word);
            return ExamResult.Failed;

        }
    }
}
using System;
using System.Linq;
using Chotiskazal.Api.ConsoleModes;
using Chotiskazal.Api.Models;
using Chotiskazal.ConsoleTesting.Services;
using Chotiskazal.LogicR;

namespace Chotiskazal.ApI.Exams
{
    public class EngChooseWordInPhraseExam : IExam
    {
        public bool NeedClearScreen => false;

        public string Name => "Eng Choose word in phrase";

        public ExamResult Pass(ExamService service, WordForLearning word, WordForLearning[] examList)
        {
            if (!word.Phrases.Any())
                return ExamResult.Impossible;
            
            var phrase = word.Phrases.GetRandomItem();

            var replaced = phrase.Origin.Replace(phrase.OriginWord, "...");
            if (replaced == phrase.Origin)
                return ExamResult.Impossible;
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"\"{phrase.Translation}\"");
            Console.ResetColor();
            Console.WriteLine();
            Console.WriteLine($" translated as ");
            Console.WriteLine();
            Console.WriteLine($"\"{replaced}\"");
            Console.ResetColor();

            Console.WriteLine();
            Console.WriteLine($"Choose missing word: ");

            var variants = examList.Randomize().Select(e => e.OriginWord).ToArray();

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

            if (variants[selectedIndex - 1] == word.OriginWord)
            {
                service.RegistrateSuccess(word.MetricId);
                return ExamResult.Passed;
            }

            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Origin was: \"{phrase.Origin}\"");
            Console.ResetColor();
            
            service.RegistrateFailure(word.MetricId);
            return ExamResult.Failed;

        }
    }
}
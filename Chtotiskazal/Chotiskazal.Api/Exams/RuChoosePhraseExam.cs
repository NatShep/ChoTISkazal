using System;
using System.Linq;
using Chotiskazal.Api.Models;
using Chotiskazal.ConsoleTesting.Services;
using Chotiskazal.LogicR;

namespace Chotiskazal.ApI.Exams
{
    public class RuChoosePhraseExam : IExam
    {
        public bool NeedClearScreen => false;

        public string Name => "Ru Choose Phrase";

        public ExamResult Pass(ExamService service, WordForLearning word, WordForLearning[] examList)
        {
            if (!word.Phrases.Any())
                return ExamResult.Impossible;
            
            var targetPhrase = word.Phrases.GetRandomItem();

            var other = examList.SelectMany(e => e.Phrases)
                .Where(p => !string.IsNullOrWhiteSpace(p?.Origin) && p!= targetPhrase)
                .Take(8);

            if(!other.Any())
                return ExamResult.Impossible;

            var variants = other
                .Append(targetPhrase)
                .Randomize()
                .Select(e => e.Origin)
                .ToArray();
            
            Console.WriteLine("=====>   " + targetPhrase.Translation + "    <=====");

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

            if (variants[selectedIndex - 1] == targetPhrase.Origin)
            {
                service.RegistrateSuccess(word.MetricId);
                return ExamResult.Passed;
            }
            service.RegistrateFailure(word.MetricId);
            return ExamResult.Failed;

        }
    }
}
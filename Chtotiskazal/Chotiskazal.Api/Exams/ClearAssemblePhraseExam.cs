using System;
using System.Linq;
using Chotiskazal.Api.Models;
using Chotiskazal.ConsoleTesting.Services;
using Chotiskazal.LogicR;

namespace Chotiskazal.ApI.Exams
{
    public class ClearAssemblePhraseExam : IExam
    {
        public bool NeedClearScreen => true;

        public string Name => "Assemble phrase";

        public ExamResult Pass(ExamService service, WordForLearning word, WordForLearning[] examList)
        {
            if (!word.Phrases.Any())
                return ExamResult.Impossible;

            var targetPhrase = word.Phrases.GetRandomItem();

            string shuffled;
            while (true)
            {
                var split = 
                    targetPhrase.Origin.Split(new[] {' '}, StringSplitOptions.RemoveEmptyEntries);
                if (split.Length < 2)
                    return ExamResult.Impossible;

                shuffled = string.Join(" ", split.Randomize());
                if(shuffled!= targetPhrase.Origin)
                    break;
            }

            Console.WriteLine("Words in phrase are shuffled. Write them in correct order:\r\n'" +  shuffled+ "'");
            string entry= null;
            while (string.IsNullOrWhiteSpace(entry))
            {
                Console.WriteLine(":");
                entry = Console.ReadLine().Trim();
            }

            if (string.CompareOrdinal(targetPhrase.Origin, entry) == 0)
            {
                service.RegistrateSuccess(word.MetricId);
                return ExamResult.Passed;
            }

            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Original phrase was: '{targetPhrase.Origin}'");
            Console.ResetColor();
            service.RegistrateFailure(word.MetricId);
            return ExamResult.Failed;
        }
    }
}
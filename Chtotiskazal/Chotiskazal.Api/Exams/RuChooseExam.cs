using System;
using System.Linq;
using Chotiskazal.Api.Models;
using Chotiskazal.ConsoleTesting.Services;
using Chotiskazal.LogicR;

namespace Chotiskazal.ApI.Exams
{
    public class RuChooseExam: IExam
    {
        public bool NeedClearScreen => false;

        public string Name => "RuChoose";

        public ExamResult Pass(ExamService service, WordForLearning word, WordForLearning[] examList)
        {
            var variants = examList.Randomize().Select(e => e.OriginWord).ToArray();

            Console.WriteLine("=====>   " + word.Translations + "    <=====");

            for (int i = 1; i <= variants.Length; i++)
            {
                Console.WriteLine($"{i}: " + variants[i - 1]);
            }
            
            Console.Write("Choose the translation: ");
                
            var selected = Console.ReadLine();
            if (selected == null || selected.ToLower().StartsWith("e"))
                return ExamResult.Exit;

            if (!int.TryParse(selected, out var selectedIndex) || selectedIndex > variants.Length ||
                selectedIndex < 1)
                return ExamResult.Retry;

            if (variants[selectedIndex - 1] == word.OriginWord)
            {
                service.RegistrateSuccess(word.MetricId);
                return ExamResult.Passed;
            }
            service.RegistrateFailure(word.MetricId);

            return ExamResult.Failed;
        }
    }
}